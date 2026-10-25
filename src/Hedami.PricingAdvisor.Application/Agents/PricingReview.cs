using System.Text.Json;
using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application.Models;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Application.Agents;

public sealed record ReviewFinding(string EvidencePointer, string Issue, string RequestedChange);
public sealed record ReviewVerdict(string Verdict, ReviewFinding[] Findings);

public static class PricingReview
{
    public static readonly ToolDefinition Definition = new("submit_review",
        "Approve or request revision with concrete evidence pointers and testable changes.",
        Schemas.Object(("verdict", new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("approve", "revise") }),
            ("findings", Schemas.Array(Schemas.Object(("evidencePointer", Schemas.String()),
                ("issue", Schemas.String()), ("requestedChange", Schemas.String()))))));

    public static ReviewVerdict Validate(ModelTurn turn, Recommendation draft)
    {
        if (turn.Calls.Length != 1 || turn.Calls[0].Name != Definition.Name)
            throw new ArgumentException("Review must return exactly one submit_review call.");
        if (turn.Calls[0].Arguments.Length > 12000) throw new ArgumentException("Review arguments exceed the size limit.");
        var json = JsonNode.Parse(turn.Calls[0].Arguments)?.AsObject() ?? throw new JsonException();
        if (json.Count != 2 || !json.ContainsKey("verdict") || !json.ContainsKey("findings")) throw new ArgumentException("Invalid review fields.");
        var verdict = Serialization.Read<ReviewVerdict>(turn.Calls[0].Arguments);
        if (verdict.Verdict is not ("approve" or "revise") || verdict.Findings is null || verdict.Findings.Length > 8)
            throw new ArgumentException("Invalid review verdict.");
        if ((verdict.Verdict == "approve") != (verdict.Findings.Length == 0)) throw new ArgumentException("Verdict and findings disagree.");
        var evidence = JsonNode.Parse(Serialization.Write(draft));
        foreach (var finding in verdict.Findings)
        {
            if (finding is null || string.IsNullOrWhiteSpace(finding.Issue) || string.IsNullOrWhiteSpace(finding.RequestedChange) ||
                finding.Issue.Length > 2000 || finding.RequestedChange.Length > 2000 ||
                string.IsNullOrWhiteSpace(finding.EvidencePointer) || finding.EvidencePointer.Length > 1000 ||
                !finding.EvidencePointer.StartsWith('/')) throw new ArgumentException("Each finding requires bounded text and an evidence pointer.");
            JsonNode? current = evidence;
            // The supplied review payload wraps the draft. Accept its explicit /draft prefix too.
            var pointer = finding.EvidencePointer.StartsWith("/draft/", StringComparison.Ordinal)
                ? finding.EvidencePointer[6..] : finding.EvidencePointer;
            foreach (var raw in pointer[1..].Split('/'))
            {
                var part = raw.Replace("~1", "/").Replace("~0", "~");
                current = current switch
                {
                    JsonObject obj when obj.ContainsKey(part) => obj[part],
                    JsonArray arr when int.TryParse(part, out var index) && index >= 0 && index < arr.Count => arr[index],
                    _ => throw new ArgumentException("Review finding points to nonexistent evidence.")
                };
            }
            if (current is null) throw new ArgumentException("Review finding has no evidence value.");
        }
        return verdict;
    }
}
