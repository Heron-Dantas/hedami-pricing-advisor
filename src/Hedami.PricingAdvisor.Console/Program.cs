using System.Globalization;
using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application;
using Hedami.PricingAdvisor.Application.Agents;
using Hedami.PricingAdvisor.Application.Harness;
using Hedami.PricingAdvisor.Application.Models;
using Hedami.PricingAdvisor.Application.Tools;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var root = Directory.GetCurrentDirectory();
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
try
{
    LocalConfiguration.Load(root);
    var command = args.FirstOrDefault() ?? "calculator";
    var options = ParseOptions(args.Skip(1).ToArray());
    var modelName = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-5.4-mini-2026-03-17";
    var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (command == "calculator") { CalculatorDemo.Run(); return 0; }
    if (command == "doctor")
    {
        Console.WriteLine($"Model: {modelName}; API credential configured: {!string.IsNullOrWhiteSpace(key)}");
        Console.WriteLine("No API request was made. Run commands from the repository root.");
        return 0;
    }
    if (command is not ("analyze" or "evaluate")) throw new ArgumentException("Commands: calculator, doctor, analyze, evaluate. See README.md.");
    if (string.IsNullOrWhiteSpace(key))
    {
        Console.Error.WriteLine("Missing OPENAI_API_KEY. Fill the ignored local .env file in your editor or set your shell environment. No API call was made.");
        return 2;
    }
    var dataset = Serialization.Read<PricingDataset>(File.ReadAllText(Get(options, "data", "data/demo.json")));
    var asOf = DateOnly.ParseExact(Get(options, "as-of", DateTime.Today.ToString("yyyy-MM-dd")), "yyyy-MM-dd", CultureInfo.InvariantCulture);
    var allowDemo = options.ContainsKey("demo");
    var instructions = RuntimeInstructions.Load(Path.Combine(root, "runtime-skills"));
    using var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
    using var spendBudget = new ApiSpendBudget(Path.Combine(root, "artifacts", "api-spend.json"));
    var client = new OpenAiResponsesClient(http, key, modelName, spendBudget);
    var limits = new RunLimits(MaxModelCalls: Int(options, "max-calls", 20), MaxToolCalls: Int(options, "max-tools", 20),
        MaxTotalTokens: Int(options, "max-tokens", 40000), MaxRevisions: Int(options, "max-revisions", 1),
        RunTimeoutSeconds: Int(options, "timeout", 180));
    limits.Validate();
    var store = new JsonRunStore(Path.Combine(root, "artifacts", "runs"), key);
    async Task<RunRecord> Execute(string request, AgentMode mode, string? parent = null, string? feedbackContext = null)
    {
        var tools = new PricingTools(new PricingCatalog(dataset, allowDemo, asOf), mode == AgentMode.PlanReview);
        var harness = new PricingHarness(client, tools, store, instructions, limits, mode);
        var context = $"As-of date: {asOf:yyyy-MM-dd}. Demonstration assumptions allowed: {allowDemo}. " + request;
        return await harness.RunAsync(context, parent, cancellation.Token, feedbackContext);
    }
    if (command == "analyze")
    {
        var mode = Mode(Get(options, "mode", "tools"));
        var parent = options.GetValueOrDefault("feedback-from");
        string? feedbackContext = null;
        var request = Get(options, "request", "Analyze productId=headset-stand using ruleId=demo-direct. Compare revenue margins 20%, 30%, 40% and a 10% discount. Explain whether the discount preserves each target.");
        if (parent is not null)
        {
            if (!Guid.TryParseExact(parent, "N", out _)) throw new ArgumentException("Invalid parent run ID.");
            if (!options.TryGetValue("feedback", out var feedback) || string.IsNullOrWhiteSpace(feedback)) throw new ArgumentException("Supply --feedback with human feedback.");
            var previous = await File.ReadAllTextAsync(Path.Combine(root, "artifacts", "runs", parent + ".json"));
            var continuation = FeedbackContext.Build(previous, feedback);
            request = continuation.Objective;
            feedbackContext = continuation.Context;
        }
        var run = await Execute(request, mode, parent, feedbackContext);
        Print(run);
        return run.Status is "draft_ready" or "reviewed_draft_ready" or "awaiting_human" ? 0 : 1;
    }
    if (!options.ContainsKey("live")) throw new ArgumentException("evaluate requires --live because it makes paid API calls. Offline evaluations use the test suite.");
    if (!allowDemo) throw new ArgumentException("The current live evaluation suite requires --demo synthetic fixtures.");
    var repeats = Int(options, "repeat", 1);
    if (repeats is < 1 or > 5) throw new ArgumentException("Repeat must be 1–5.");
    var modes = Get(options, "mode", "tools") == "all" ? new[] { AgentMode.Tools, AgentMode.Review, AgentMode.PlanReview } : new[] { Mode(Get(options, "mode", "tools")) };
    var cases = LiveEvaluations.Cases;
    if (options.TryGetValue("scenarios", out var scenarioList))
    {
        var selected = scenarioList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (selected.Length == 0 || selected.Distinct().Count() != selected.Length || selected.Any(id => !cases.Any(c => c.Id == id)))
            throw new ArgumentException("Choose distinct known evaluation scenario IDs.");
        cases = cases.Where(c => selected.Contains(c.Id)).ToArray();
    }
    var maxRuns = Int(options, "max-runs", 7);
    if (cases.Length * repeats * modes.Length > maxRuns) throw new ArgumentException("Requested evaluation count exceeds --max-runs. Set an explicit run budget.");
    var results = new List<object>();
    foreach (var mode in modes)
    foreach (var scenario in cases)
    for (var repetition = 1; repetition <= repeats; repetition++)
    {
        cancellation.Token.ThrowIfCancellationRequested();
        Console.WriteLine($"Evaluating {scenario.Id}, {mode}, repetition {repetition}...");
        var run = await Execute(scenario.Objective, mode);
        var passed = LiveEvaluations.Check(scenario.Id, run);
        results.Add(new { scenarioId = scenario.Id, mode = mode.ToString(), repetition, passed, runId = run.Id, run.Status,
            run.DurationMs, run.ModelCalls, run.ToolCalls, run.InputTokens, run.OutputTokens, run.UsageComplete, run.EstimatedCostUsd });
        Console.WriteLine($"{(passed ? "PASS" : "FAIL")} {scenario.Id}: {run.Status}; run {run.Id}");
    }
    var report = Path.Combine(root, "artifacts", "evaluations-" + DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfff") + ".json");
    await File.WriteAllTextAsync(report, Serialization.Write(new { simulated = false, modelName, asOf, results }));
    Console.WriteLine($"Report: {report}");
    return results.All(x => JsonNode.Parse(Serialization.Write(x))!["passed"]!.GetValue<bool>()) ? 0 : 1;
}
catch (OperationCanceledException) { Console.Error.WriteLine("Cancelled."); return 130; }
catch (Exception ex) when (ex is ArgumentException or IOException or System.Text.Json.JsonException or InvalidOperationException or FormatException)
{
    // Configuration errors never echo file contents, credentials, raw API messages or user inputs.
    Console.Error.WriteLine($"Configuration or local I/O error ({ex.GetType().Name}). Check command options, data files and README.md. No credentials are printed.");
    if (ex is ArgumentException { ParamName: "options" }) Console.Error.WriteLine("Unknown or missing command option.");
    return 2;
}

static Dictionary<string, string> ParseOptions(string[] values)
{
    var allowed = new HashSet<string> { "data", "as-of", "demo", "request", "mode", "feedback-from", "feedback", "max-calls", "max-tools", "max-tokens", "max-revisions", "timeout", "live", "repeat", "max-runs", "scenarios" };
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var i = 0; i < values.Length; i++)
    {
        if (!values[i].StartsWith("--")) throw new ArgumentException("Invalid option.", "options");
        var name = values[i][2..];
        if (!allowed.Contains(name) || result.ContainsKey(name)) throw new ArgumentException("Unknown or duplicate option.", "options");
        if (name is "demo" or "live") result[name] = "true";
        else if (++i < values.Length && !values[i].StartsWith("--")) result[name] = values[i];
        else throw new ArgumentException("Missing option value.", "options");
    }
    return result;
}
static string Get(Dictionary<string, string> options, string name, string fallback) => options.GetValueOrDefault(name) ?? fallback;
static int Int(Dictionary<string, string> options, string name, int fallback) => int.Parse(Get(options, name, fallback.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
static AgentMode Mode(string name) => name switch { "tools" => AgentMode.Tools, "review" => AgentMode.Review, "plan-review" => AgentMode.PlanReview, _ => throw new ArgumentException("Mode must be tools, review or plan-review.") };
static void Print(RunRecord run)
{
    Console.WriteLine($"Run {run.Id}: {run.Status}; mode={run.Mode}; simulated={run.Simulated}");
    Console.WriteLine($"Calls: model={run.ModelCalls}, tool={run.ToolCalls}; tokens={run.InputTokens + run.OutputTokens}, usageComplete={run.UsageComplete}; duration={run.DurationMs}ms");
    if (run.Questions is not null) foreach (var question in run.Questions) Console.WriteLine("Question: " + question);
    if (run.Recommendation is { } draft)
    {
        Console.WriteLine("DRAFT ONLY — human approval required; no price applied. Status must be assessed before use.");
        Console.WriteLine("Model narrative (verify against deterministic evidence): " + draft.Summary);
        foreach (var costs in draft.Scenarios.DistinctBy(x => x.ProductId))
            Console.WriteLine($"Costs BRL ({costs.ProductId}): material={costs.Costs.MaterialBrl:F4}; energy={costs.Costs.EnergyBrl:F4}; printer={costs.Costs.PrinterBrl:F4}; failure provision={costs.Costs.FailureProvisionBrl:F4}; labor={costs.Costs.LaborBrl:F4}; packaging={costs.Costs.PackagingBrl:F4}; total={costs.Costs.TotalBrl:F4}");
        foreach (var fee in draft.Scenarios.DistinctBy(x => x.RuleId))
            Console.WriteLine($"Charges ({fee.RuleId}): channel={fee.AppliedRule.ChannelRate:P2}; tax={fee.AppliedRule.TaxRate:P2}; advertising={fee.AppliedRule.AdvertisingRate:P2}; fixed BRL {fee.FixedFeeBrl:F2}; source={fee.RuleProvenance.Source}; effective={fee.RuleProvenance.ValidFrom:yyyy-MM-dd}..{fee.RuleProvenance.ValidThrough:yyyy-MM-dd}; as-of={fee.AsOf:yyyy-MM-dd}");
        foreach (var row in draft.Scenarios)
            Console.WriteLine($"{row.Id}: target={row.Scenario.TargetRevenueMargin:P0}; list BRL {row.Scenario.RecommendedPriceBrl:F2}; discount={row.Scenario.DiscountRate:P0}; charged BRL {row.Scenario.ChargedPriceBrl:F2}; contribution BRL {row.Scenario.ContributionBrl:F4}; margin={row.Scenario.RevenueMargin:P2}; cost BRL {row.Costs.TotalBrl:F4}; source status={row.RuleProvenance.Status}");
        foreach (var assumption in draft.Assumptions) Console.WriteLine("Assumption: " + assumption);
        foreach (var limitation in draft.Limitations) Console.WriteLine("Limitation: " + limitation);
    }
    Console.WriteLine($"Estimated API cost USD: {run.EstimatedCostUsd?.ToString("F6", CultureInfo.InvariantCulture) ?? "unknown"}; {run.CostStatus}");
    Console.WriteLine($"Trace: artifacts/runs/{run.Id}.json");
}
