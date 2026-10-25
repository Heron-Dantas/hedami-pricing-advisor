using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Application.Harness;

public sealed record FeedbackRequest(string Objective, string Context);
public static class FeedbackContext
{
    public static FeedbackRequest Build(string previousRunJson, string humanFeedback)
    {
        if (string.IsNullOrWhiteSpace(humanFeedback) || humanFeedback.Length > 4000)
            throw new ArgumentException("Feedback must contain 1–4000 characters.");
        var previous = JsonNode.Parse(previousRunJson) ?? throw new ArgumentException("Missing parent run.");
        var original = previous["events"]?[0]?["data"]?["originalObjective"]?.GetValue<string>()
            ?? previous["events"]?[0]?["data"]?["objective"]?.GetValue<string>()
            ?? throw new ArgumentException("Missing parent objective.");
        var draft = previous["recommendation"] is { } node
            ? Serialization.Read<Recommendation>(node.ToJsonString()) : null;
        // Keep source IDs and numeric evidence, without repeatedly embedding full provenance/cost records.
        var context = Serialization.Write(new
        {
            originalObjective = original,
            previousObjective = previous["events"]?[0]?["data"]?["objective"]?.GetValue<string>(),
            priorSummary = draft?.Summary,
            priorScenarios = draft?.Scenarios.Select(x => new
            {
                x.ProductId, x.RuleId, x.Scenario.TargetRevenueMargin, x.Scenario.DiscountRate,
                x.Scenario.RecommendedPriceBrl, x.Scenario.ChargedPriceBrl, x.Scenario.RevenueMargin
            }).ToArray(),
            previousQuestions = previous["questions"]?.DeepClone()
        });
        if (context.Length > 20000) throw new ArgumentException("Parent feedback context is too large.");
        return new("Human feedback supersedes conflicting prior targets: " + humanFeedback, context);
    }
}
