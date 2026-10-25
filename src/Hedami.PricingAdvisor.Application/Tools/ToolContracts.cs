using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Domain;

namespace Hedami.PricingAdvisor.Application.Tools;

public sealed record ToolDefinition(string Name, string Description, JsonObject Parameters)
{
    public JsonObject ToApi() => new()
    {
        ["type"] = "function", ["name"] = Name, ["description"] = Description,
        ["parameters"] = Parameters.DeepClone(), ["strict"] = true
    };
}
public sealed record PlanArguments(string NextTool, string Objective);
public sealed record PlanState(string NextTool, string Objective, string Stage);
public sealed record ClarificationResult(string[] Questions, string Status);
public sealed record ProductArguments(string ProductId);
public sealed record RuleArguments(string RuleId);
public sealed record CalculateArguments(string ProductId, string RuleId, decimal[] Margins, decimal DiscountRate);
public sealed record DraftArguments(string[] ScenarioIds, string Summary, string[] Assumptions, string[] Limitations);
public sealed record ClarificationArguments(string[] Questions);
public sealed record ScenarioEvidence(string Id, string ProductId, string RuleId,
    Provenance ProductProvenance, Provenance RuleProvenance, DateOnly AsOf,
    CostBreakdown Costs, PriceScenario Scenario, decimal RevenueChargeRate, decimal FixedFeeBrl, ChannelRule AppliedRule);
public sealed record Recommendation(string Summary, string[] Assumptions, string[] Limitations,
    ScenarioEvidence[] Scenarios, bool HumanApprovalRequired = true, bool PriceApplied = false);
public sealed record ToolResult(bool Success, string Code, object? Data, string? Message)
{
    public static ToolResult Ok(object data) => new(true, "ok", data, null);
    public static ToolResult Error(string code, string message) => new(false, code, null, message);
}

public interface IToolExecutor
{
    IReadOnlyList<ToolDefinition> Definitions { get; }
    Task<ToolResult> ExecuteAsync(string name, string arguments, CancellationToken cancellationToken);
}

public static class Schemas
{
    public static JsonObject Object(params (string Name, JsonNode Type)[] fields) => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject(fields.Select(x => KeyValuePair.Create<string, JsonNode?>(x.Name, x.Type))),
        ["required"] = new JsonArray(fields.Select(x => (JsonNode?)JsonValue.Create(x.Name)).ToArray())
    };
    public static JsonObject String() => new() { ["type"] = "string" };
    public static JsonObject Number() => new() { ["type"] = "number" };
    public static JsonObject Array(JsonNode item) => new() { ["type"] = "array", ["items"] = item };
}
