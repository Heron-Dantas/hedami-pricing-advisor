using System.Text.Json;
using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Domain;

namespace Hedami.PricingAdvisor.Application.Tools;

// Only these read/calculation/draft operations can execute. No mutation capability exists.
public sealed class PricingTools(PricingCatalog catalog, bool adaptivePlanning = false) : IToolExecutor
{
    public bool AdaptivePlanning => adaptivePlanning;
    private readonly HashSet<string> readProducts = [];
    private readonly HashSet<string> readRules = [];
    private readonly Dictionary<string, ScenarioEvidence> scenarios = [];
    public IReadOnlyDictionary<string, ScenarioEvidence> Scenarios => scenarios;
    public Recommendation? Draft { get; private set; }
    public string[]? Questions { get; private set; }
    public string? NextTool { get; private set; }
    public string? PlanObjective { get; private set; }
    public string PlanStage => Draft is not null ? "drafted" : Questions is not null ? "awaiting_human" :
        scenarios.Count > 0 ? "recommend_or_recalculate" : readProducts.Count > 0 && readRules.Count > 0 ? "calculate_or_clarify" : "retrieve_data";

    public IReadOnlyList<ToolDefinition> Definitions => adaptivePlanning
        ? [.. BaseDefinitions, new("update_plan", "Choose the next allowed tool and record a short observable objective. This choice gates execution; replan after failures.",
            Schemas.Object(("nextTool", Schemas.String()), ("objective", Schemas.String())))]
        : BaseDefinitions;

    private static IReadOnlyList<ToolDefinition> BaseDefinitions { get; } =
    [
        new("list_pricing_data", "Discover local product IDs/names and available channel rule IDs with provenance. Choose only a matching record; clarify ambiguity.", Schemas.Object()),
        new("get_product", "Read product costs (BRL, grams, hours, kW) and provenance. Null is unknown.",
            Schemas.Object(("productId", Schemas.String()))),
        new("get_channel_rule", "Read separate revenue charge rates, fixed fee, source and validity. No Shopee rules exist.",
            Schemas.Object(("ruleId", Schemas.String()))),
        new("calculate_scenarios", "Deterministic C# costs and prices. Margins and discount are fractions. Uses stored product/rule, never model-invented costs. Includes undiscounted and discounted scenarios.",
            Schemas.Object(("productId", Schemas.String()), ("ruleId", Schemas.String()),
                ("margins", Schemas.Array(Schemas.Number())), ("discountRate", Schemas.Number()))),
        new("submit_recommendation", "Submit a draft referring only to successful scenario IDs. No prices are applied. Human approval is required.",
            Schemas.Object(("scenarioIds", Schemas.Array(Schemas.String())), ("summary", Schemas.String()),
                ("assumptions", Schemas.Array(Schemas.String())), ("limitations", Schemas.Array(Schemas.String())))),
        new("request_clarification", "Stop and ask specific questions when essential data/rules/objectives are missing.",
            Schemas.Object(("questions", Schemas.Array(Schemas.String()))))
    ];

    public Task<ToolResult> ExecuteAsync(string name, string arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            if (!Definitions.Any(x => x.Name == name))
                return Task.FromResult(ToolResult.Error("permission_denied", "Operation is not permitted. Price application is unavailable even when requested by a user."));
            if (arguments.Length > 12000) throw new ArgumentException("Tool arguments exceed the size limit.");
            // Validate the same strict schema locally; required zero-valued numerics cannot silently default.
            ValidateShape(JsonNode.Parse(arguments), Definitions.Single(x => x.Name == name).Parameters);
            if (adaptivePlanning && name is not "update_plan" and not "request_clarification")
            {
                if (NextTool != name) return Task.FromResult(ToolResult.Error("plan_precondition", "Call update_plan to select this action before executing it."));
                NextTool = null;
            }
            var result = name switch
            {
                "list_pricing_data" => ToolResult.Ok(catalog.Overview()),
                "update_plan" => Plan(Serialization.Read<PlanArguments>(arguments)),
                "get_product" => GetProduct(Serialization.Read<ProductArguments>(arguments)),
                "get_channel_rule" => GetRule(Serialization.Read<RuleArguments>(arguments)),
                "calculate_scenarios" => Calculate(Serialization.Read<CalculateArguments>(arguments)),
                "submit_recommendation" => Submit(Serialization.Read<DraftArguments>(arguments)),
                "request_clarification" => Clarify(Serialization.Read<ClarificationArguments>(arguments)),
                _ => throw new InvalidOperationException()
            };
            return Task.FromResult(result);
        }
        catch (Exception ex) when (ex is ArgumentException or JsonException or OverflowException)
        {
            return Task.FromResult(ToolResult.Error("validation_error", ex.Message));
        }
    }

    private ToolResult Plan(PlanArguments args)
    {
        CheckText(args.Objective);
        if (!BaseDefinitions.Any(x => x.Name == args.NextTool)) throw new ArgumentException("Choose an available pricing tool.");
        if (args.NextTool == "calculate_scenarios" && (readProducts.Count == 0 || readRules.Count == 0))
            throw new ArgumentException("The plan must retrieve product and rule evidence before calculation.");
        if (args.NextTool == "submit_recommendation" && scenarios.Count == 0)
            throw new ArgumentException("The plan needs calculator evidence before recommendation.");
        NextTool = args.NextTool;
        PlanObjective = args.Objective;
        return ToolResult.Ok(new PlanState(NextTool, PlanObjective, PlanStage));
    }
    private ToolResult GetProduct(ProductArguments args)
    {
        var product = catalog.Product(args.ProductId);
        readProducts.Add(product.Id);
        return ToolResult.Ok(product);
    }
    private ToolResult GetRule(RuleArguments args)
    {
        var rule = catalog.Rule(args.RuleId);
        readRules.Add(rule.Id);
        return ToolResult.Ok(rule);
    }
    private ToolResult Calculate(CalculateArguments args)
    {
        if (adaptivePlanning && (!readProducts.Contains(args.ProductId) || !readRules.Contains(args.RuleId)))
            return ToolResult.Error("plan_precondition", "Retrieve this product and rule before calculating; revise the plan if information is missing.");
        if (args.Margins.Length is < 1 or > 5) throw new ArgumentException("Supply between one and five margin targets.");
        var product = catalog.Product(args.ProductId);
        var rule = catalog.Rule(args.RuleId);
        catalog.Validate(product.Provenance);
        catalog.Validate(rule.Provenance);
        var costs = PricingCalculator.CalculateCosts(product.Costs);
        var rate = PricingCatalog.RevenueRate(rule);
        // Compute the entire batch before changing state, so invalid batches cannot leak partial evidence.
        var results = args.Margins.Distinct().SelectMany(margin =>
            (args.DiscountRate == 0m ? new[] { 0m } : new[] { 0m, args.DiscountRate }).Select(discount =>
                new ScenarioEvidence("scenario-" + Guid.NewGuid().ToString("N"), product.Id, rule.Id,
                    product.Provenance, rule.Provenance, catalog.AsOf, costs,
                    PricingCalculator.CalculateScenario(costs.TotalBrl, rate, rule.FixedFeeBrl!.Value, margin, discount),
                    rate, rule.FixedFeeBrl!.Value, rule))).ToArray();
        foreach (var result in results) scenarios.Add(result.Id, result);
        return ToolResult.Ok(results);
    }
    private ToolResult Submit(DraftArguments args)
    {
        if (args.ScenarioIds.Length is < 1 or > 20 || args.ScenarioIds.Distinct().Count() != args.ScenarioIds.Length)
            throw new ArgumentException("Use one to twenty distinct scenario IDs.");
        if (args.ScenarioIds.Any(id => !scenarios.ContainsKey(id))) throw new ArgumentException("Unknown scenario evidence ID.");
        CheckText(args.Summary);
        CheckTexts(args.Assumptions);
        CheckTexts(args.Limitations);
        var selected = args.ScenarioIds.Select(id => scenarios[id]).ToArray();
        foreach (var evidence in selected) VerifyEvidence(evidence);
        Draft = new(args.Summary, args.Assumptions, args.Limitations, selected);
        return ToolResult.Ok(Draft);
    }
    public void VerifyEvidence(ScenarioEvidence evidence)
    {
        var product = catalog.Product(evidence.ProductId);
        var rule = catalog.Rule(evidence.RuleId);
        catalog.Validate(product.Provenance);
        catalog.Validate(rule.Provenance);
        var cost = PricingCalculator.CalculateCosts(product.Costs);
        var expected = PricingCalculator.CalculateScenario(cost.TotalBrl, PricingCatalog.RevenueRate(rule),
            rule.FixedFeeBrl!.Value, evidence.Scenario.TargetRevenueMargin, evidence.Scenario.DiscountRate);
        if (cost != evidence.Costs || expected != evidence.Scenario) throw new ArgumentException("Deterministic evidence verification failed.");
    }
    public void ResetDraft() => Draft = null;
    private ToolResult Clarify(ClarificationArguments args)
    {
        CheckTexts(args.Questions);
        Questions = args.Questions;
        return ToolResult.Ok(new ClarificationResult(Questions, "awaiting_human"));
    }
    private static void CheckText(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 3000) throw new ArgumentException("Text must contain 1–3000 characters.");
    }
    private static void CheckTexts(string[] texts)
    {
        if (texts.Length is < 1 or > 10) throw new ArgumentException("Supply one to ten text entries.");
        foreach (var text in texts) CheckText(text);
    }
    private static void ValidateShape(JsonNode? value, JsonObject schema)
    {
        switch (schema["type"]!.GetValue<string>())
        {
            case "object":
                if (value is not JsonObject obj) throw new ArgumentException("Expected an object.");
                var properties = schema["properties"]!.AsObject();
                if (obj.Count != properties.Count || obj.Any(x => !properties.ContainsKey(x.Key)))
                    throw new ArgumentException("All declared arguments are required; additional arguments are forbidden.");
                foreach (var field in properties) ValidateShape(obj[field.Key], field.Value!.AsObject());
                break;
            case "array":
                if (value is not JsonArray array || array.Count > 20) throw new ArgumentException("Expected an array of at most twenty items.");
                foreach (var item in array) ValidateShape(item, schema["items"]!.AsObject());
                break;
            case "string":
                if (value is not JsonValue str || !str.TryGetValue<string>(out var text) || string.IsNullOrWhiteSpace(text) || text.Length > 3000)
                    throw new ArgumentException("Expected a non-empty bounded string.");
                break;
            case "number":
                if (value is not JsonValue number || !number.TryGetValue<decimal>(out _)) throw new ArgumentException("Expected a decimal number.");
                break;
        }
    }
}
