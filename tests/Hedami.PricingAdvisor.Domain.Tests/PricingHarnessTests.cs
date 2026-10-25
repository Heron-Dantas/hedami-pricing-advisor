using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application;
using Hedami.PricingAdvisor.Application.Agents;
using Hedami.PricingAdvisor.Application.Harness;
using Hedami.PricingAdvisor.Application.Models;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Domain.Tests;

public class PricingHarnessTests
{
    private static RuntimeInstructions Instructions => new("Test analyst instructions", "Test separate reviewer instructions", "test-v1");
    [Fact]
    public async Task CompleteLoopUsesCalculatorAndPersistsObservableActions()
    {
        var tools = ApplicationTestData.Tools();
        var model = new TestModel((n, _) => n switch
        {
            1 => TestModel.Call("get_product", new ProductArguments("stand")),
            2 => TestModel.Call("get_channel_rule", new RuleArguments("direct")),
            3 => TestModel.Call("calculate_scenarios", new CalculateArguments("stand", "direct", [0.3m], 0.1m)),
            _ => Draft(tools)
        });
        var store = new MemoryStore();
        var run = await new PricingHarness(model, tools, store, Instructions, new()).RunAsync("Compare 30% margin with 10% discount");
        Assert.Equal("draft_ready", run.Status);
        Assert.True(run.Simulated);
        Assert.Equal(4, run.ToolCalls);
        Assert.Equal(2, run.Recommendation!.Scenarios.Length);
        Assert.False(run.Recommendation.PriceApplied);
        Assert.True(store.Snapshots.Count > 10);
        Assert.Contains(run.Events, e => e.Kind == "deterministic_verification");
    }
    [Fact]
    public async Task MissingDataProducesClarificationWithoutInventedDraft()
    {
        var p = ApplicationTestData.Product;
        var tools = ApplicationTestData.Tools(product: p with { Costs = p.Costs with { MaterialGrams = null } });
        var model = new TestModel((n, _) => n == 1
            ? TestModel.Call("calculate_scenarios", new CalculateArguments("stand", "direct", [0.3m], 0m))
            : TestModel.Call("request_clarification", new ClarificationArguments(["What is the total material consumption in grams?"])));
        var run = await new PricingHarness(model, tools, new MemoryStore(), Instructions, new()).RunAsync("Price stand");
        Assert.Equal("awaiting_human", run.Status);
        Assert.Null(run.Recommendation);
        Assert.Single(run.Questions!);
    }
    [Fact]
    public async Task RepeatedCallsWithDifferentPropertyOrderStopWithoutProgress()
    {
        var model = new TestModel((n, _) => TestModel.Call("calculate_scenarios", n % 2 == 0
            ? """{"ruleId":"absent","productId":"stand","discountRate":0.10,"margins":[0.30]}"""
            : """{"productId":"stand","ruleId":"absent","margins":[0.3],"discountRate":0.1}"""));
        var run = await new PricingHarness(model, ApplicationTestData.Tools(), new MemoryStore(), Instructions, new()).RunAsync("Price stand");
        Assert.Equal("no_progress", run.Status);
        Assert.Equal(2, run.ToolCalls);
    }
    [Theory]
    [InlineData(false, "tool_failure")]
    [InlineData(true, "tool_timeout")]
    public async Task ToolFailureAndTimeoutReturnStructuredErrors(bool timeout, string expected)
    {
        var tools = ApplicationTestData.Tools();
        var model = new TestModel((_, _) => TestModel.Call("get_product", new ProductArguments("stand")));
        var run = await new PricingHarness(model, tools, new MemoryStore(), Instructions,
            new(MaxModelCalls: 1, ToolTimeoutSeconds: 1), toolExecutor: new FaultingTools(tools, timeout)).RunAsync("Price stand");
        Assert.Equal("model_call_limit", run.Status);
        Assert.Contains(run.Events, e => e.Kind == "tool_completed" && Serialization.Write(e.Data).Contains(expected));
        Assert.Null(run.Recommendation);
    }
    [Fact]
    public async Task TechnicalRetryDoesNotConsumeReviewBudget()
    {
        var model = new TestModel((n, _) => n == 1 ? throw new ModelException("rate_limit", true)
            : TestModel.Call("request_clarification", new ClarificationArguments(["Which product?"])));
        var run = await new PricingHarness(model, ApplicationTestData.Tools(), new MemoryStore(), Instructions, new()).RunAsync("Price product");
        Assert.Equal("awaiting_human", run.Status);
        Assert.Equal(1, run.TechnicalRetries);
        Assert.Equal(0, run.Revisions);
        Assert.False(run.UsageComplete);
    }
    [Fact]
    public async Task ReviewerUsesFreshContextAndHasNoPricingTools()
    {
        var tools = ApplicationTestData.Tools();
        var model = new TestModel((n, _) => n switch
        {
            1 => TestModel.Call("calculate_scenarios", new CalculateArguments("stand", "direct", [0.3m], 0m)),
            2 => Draft(tools),
            _ => TestModel.Call("submit_review", new ReviewVerdict("approve", []))
        });
        var run = await new PricingHarness(model, tools, new MemoryStore(), Instructions, new(), AgentMode.Review).RunAsync("Compare 30% margin");
        Assert.Equal("reviewed_draft_ready", run.Status);
        Assert.Single(model.Contexts[2]);
        Assert.Single(model.Definitions[2]);
        Assert.Equal("submit_review", model.Definitions[2][0].Name);
        Assert.DoesNotContain("function_call_output", model.Contexts[2].ToJsonString());
    }
    [Fact]
    public async Task ReviewLimitCannotBeOverriddenByModel()
    {
        var tools = ApplicationTestData.Tools();
        var model = new TestModel((n, _) => n switch
        {
            1 => TestModel.Call("calculate_scenarios", new CalculateArguments("stand", "direct", [0.3m], 0m)),
            2 or 4 => Draft(tools),
            _ => TestModel.Call("submit_review", new ReviewVerdict("revise", [new("/summary", "Unsupported summary", "Use evidence only")]))
        });
        var run = await new PricingHarness(model, tools, new MemoryStore(), Instructions, new(), AgentMode.Review).RunAsync("Price stand");
        Assert.Equal("review_limit", run.Status);
        Assert.Equal(1, run.Revisions);
        Assert.Equal(2, run.ReviewCalls);
    }
    [Fact]
    public async Task InventedReviewEvidenceIsRejected()
    {
        var tools = ApplicationTestData.Tools();
        var model = new TestModel((n, _) => n switch
        {
            1 => TestModel.Call("calculate_scenarios", new CalculateArguments("stand", "direct", [0.3m], 0m)),
            2 => Draft(tools),
            _ => TestModel.Call("submit_review", new ReviewVerdict("revise", [new("/invented/path", "Issue", "Change")]))
        });
        var run = await new PricingHarness(model, tools, new MemoryStore(), Instructions, new(), AgentMode.Review).RunAsync("Price stand");
        Assert.Equal("invalid_review", run.Status);
        Assert.Contains(run.Events, e => e.Kind == "review_response");
        Assert.Contains(run.Events, e => e.Kind == "review_rejected");
    }
    [Fact]
    public void ReviewerMayCiteTheExplicitDraftEnvelope()
    {
        var draft = new Recommendation("Unsupported summary", ["Synthetic"], ["No overhead"], []);
        var turn = TestModel.Call("submit_review", new ReviewVerdict("revise", [new("/draft/summary", "Unsupported summary", "Use evidence only")]));
        Assert.Equal("revise", PricingReview.Validate(turn, draft).Verdict);
    }
    [Fact]
    public async Task CancelledRunStillPersistsFinalStatus()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var store = new MemoryStore();
        var model = new TestModel((_, _) => throw new InvalidOperationException("Should not call model"));
        var run = await new PricingHarness(model, ApplicationTestData.Tools(), store, Instructions, new()).RunAsync("Price stand", cancellationToken: cancellation.Token);
        Assert.Equal("cancelled", run.Status);
        Assert.Equal(0, run.ModelCalls);
        Assert.Contains("cancelled", store.Snapshots.Last());
    }
    [Fact]
    public async Task TokenBudgetStopsBeforeAnotherToolExecutes()
    {
        var model = new TestModel((_, _) => TestModel.Call("get_product", new ProductArguments("stand")) with { Usage = new(100, 100, 0) });
        var run = await new PricingHarness(model, ApplicationTestData.Tools(), new MemoryStore(), Instructions,
            new(MaxTotalTokens: 200, MaxOutputTokensPerCall: 128)).RunAsync("Price stand");
        Assert.Equal("token_limit", run.Status);
        Assert.Equal(0, run.ToolCalls);
    }
    [Fact]
    public async Task AdaptivePlanningCanExecuteTheFullLoop()
    {
        var tools = ApplicationTestData.Tools(adaptive: true);
        var model = new TestModel((n, _) => n switch
        {
            1 => TestModel.Call("update_plan", new PlanArguments("get_product", "Read product costs")),
            2 => TestModel.Call("get_product", new ProductArguments("stand")),
            3 => TestModel.Call("update_plan", new PlanArguments("get_channel_rule", "Read dated fee rules")),
            4 => TestModel.Call("get_channel_rule", new RuleArguments("direct")),
            5 => TestModel.Call("update_plan", new PlanArguments("calculate_scenarios", "Compare target and discount")),
            6 => TestModel.Call("calculate_scenarios", new CalculateArguments("stand", "direct", [0.3m], 0.1m)),
            7 => TestModel.Call("update_plan", new PlanArguments("submit_recommendation", "Draft using verified calculator evidence")),
            8 => Draft(tools),
            _ => TestModel.Call("submit_review", new ReviewVerdict("approve", []))
        });
        var run = await new PricingHarness(model, tools, new MemoryStore(), Instructions, new(), AgentMode.PlanReview).RunAsync("Price stand");
        Assert.Equal("reviewed_draft_ready", run.Status);
        Assert.Equal(9, run.ModelCalls);
        Assert.Equal(8, run.ToolCalls);
        Assert.Equal(1, run.ReviewCalls);
    }
    [Fact]
    public async Task RepeatedRateLimitsStopAtTheTechnicalRetryBudget()
    {
        var model = new TestModel((_, _) => throw new ModelException("rate_limit", true));
        var run = await new PricingHarness(model, ApplicationTestData.Tools(), new MemoryStore(), Instructions, new()).RunAsync("Price stand");
        Assert.Equal("rate_limit", run.Status);
        Assert.Equal(2, run.ModelCalls);
        Assert.Equal(1, run.TechnicalRetries);
    }
    [Fact]
    public async Task WrongTypedToolResultIsRejectedBeforeModelReceivesIt()
    {
        var tools = ApplicationTestData.Tools();
        var model = new TestModel((_, _) => TestModel.Call("get_product", new ProductArguments("stand")));
        var run = await new PricingHarness(model, tools, new MemoryStore(), Instructions, new(),
            toolExecutor: new InvalidTools(tools)).RunAsync("Price stand");
        Assert.Equal("invalid_tool_result", run.Status);
        Assert.Equal(1, run.ModelCalls);
    }
    [Fact]
    public async Task ModelTimeoutIsBoundedAndPersisted()
    {
        var run = await new PricingHarness(new SlowModel(), ApplicationTestData.Tools(), new MemoryStore(), Instructions,
            new(ModelTimeoutSeconds: 1)).RunAsync("Price stand");
        Assert.Equal("model_timeout", run.Status);
        Assert.Equal(0, run.ToolCalls);
        Assert.False(run.UsageComplete);
    }
    private sealed class InvalidTools(IToolExecutor inner) : IToolExecutor
    {
        public IReadOnlyList<ToolDefinition> Definitions => inner.Definitions;
        public Task<ToolResult> ExecuteAsync(string name, string arguments, CancellationToken cancellationToken) =>
            Task.FromResult(ToolResult.Ok(new { unexpected = "Not a product" }));
    }
    private sealed class SlowModel : IModelClient
    {
        public string Model => "slow-simulated";
        public bool IsSimulated => true;
        public async Task<ModelTurn> CompleteAsync(string instructions, JsonArray input, IReadOnlyList<ToolDefinition> tools, int maxOutputTokens, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException();
        }
    }
    private static ModelTurn Draft(PricingTools tools) => TestModel.Call("submit_recommendation",
        new DraftArguments(tools.Scenarios.Keys.ToArray(), "Synthetic draft", ["Synthetic rates"], ["No overhead; contribution is not net profit"]));
}
