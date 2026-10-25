using Hedami.PricingAdvisor.Application;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Domain.Tests;

public class PricingToolsTests
{
    private const string Calculation = """{"productId":"stand","ruleId":"direct","margins":[0.3],"discountRate":0.1}""";
    [Theory]
    [InlineData("{\"productId\":\"stand\",\"ruleId\":\"direct\",\"margins\":[0.3]}")]
    [InlineData("{\"productId\":\"stand\",\"ruleId\":\"direct\",\"margins\":[0.3],\"discountRate\":0.1,\"costOverride\":1}")]
    [InlineData("{\"productId\":null,\"ruleId\":\"direct\",\"margins\":[0.3],\"discountRate\":0.1}")]
    [InlineData("not json")]
    public async Task InvalidArgumentsCannotUseDefaultsOrOverrideCosts(string args)
    {
        var tools = ApplicationTestData.Tools();
        var result = await tools.ExecuteAsync("calculate_scenarios", args, default);
        Assert.False(result.Success);
        Assert.Empty(tools.Scenarios);
    }
    [Fact]
    public async Task CompleteCostsGenerateVerifiedDiscountComparisons()
    {
        var tools = ApplicationTestData.Tools();
        Assert.True((await tools.ExecuteAsync("calculate_scenarios", Calculation, default)).Success);
        Assert.Equal(2, tools.Scenarios.Count);
        Assert.Contains(tools.Scenarios.Values, x => x.Scenario.RecommendedPriceBrl == 44.45m && x.Scenario.DiscountRate == 0.1m && x.Scenario.ChargedPriceBrl == 40.01m);
        foreach (var evidence in tools.Scenarios.Values) tools.VerifyEvidence(evidence);
    }
    [Fact]
    public async Task MissingDataDoesNotProduceEvidence()
    {
        var product = ApplicationTestData.Product;
        var tools = ApplicationTestData.Tools(product: product with { Costs = product.Costs with { MaterialGrams = null } });
        Assert.False((await tools.ExecuteAsync("calculate_scenarios", Calculation, default)).Success);
        Assert.Empty(tools.Scenarios);
    }
    [Fact]
    public async Task ExpiredRulesCannotBeUsed()
    {
        var rule = ApplicationTestData.Rule;
        var tools = ApplicationTestData.Tools(rule: rule with { Provenance = rule.Provenance with { ValidThrough = new(2026, 10, 23) } });
        Assert.False((await tools.ExecuteAsync("calculate_scenarios", Calculation, default)).Success);
    }
    [Fact]
    public async Task UnverifiedAndUnknownFeesCannotBeUsed()
    {
        foreach (var rule in new[] { ApplicationTestData.Rule with { ChannelRate = null }, ApplicationTestData.Rule with { Provenance = ApplicationTestData.Provenance with { Status = ValidationStatus.Unverified } } })
            Assert.False((await ApplicationTestData.Tools(rule: rule).ExecuteAsync("calculate_scenarios", Calculation, default)).Success);
    }
    [Fact]
    public async Task DemonstrationRequiresExplicitPermission() => Assert.False(
        (await ApplicationTestData.Tools(allowDemo: false).ExecuteAsync("calculate_scenarios", Calculation, default)).Success);
    [Fact]
    public async Task InfeasibleBatchProducesNoPartialEvidence()
    {
        var tools = ApplicationTestData.Tools();
        Assert.False((await tools.ExecuteAsync("calculate_scenarios", Calculation.Replace("[0.3]", "[0.3,0.95]"), default)).Success);
        Assert.Empty(tools.Scenarios);
    }
    [Fact]
    public async Task PriceApplicationIsDeniedOutsidePrompt()
    {
        var result = await ApplicationTestData.Tools().ExecuteAsync("apply_price", "{}", default);
        Assert.Equal("permission_denied", result.Code);
    }
    [Fact]
    public async Task AdaptivePlanGatesActionsAndChecksDependencies()
    {
        var tools = ApplicationTestData.Tools(adaptive: true);
        Assert.Equal("plan_precondition", (await tools.ExecuteAsync("get_product", """{"productId":"stand"}""", default)).Code);
        Assert.False((await tools.ExecuteAsync("update_plan", """{"nextTool":"calculate_scenarios","objective":"Calculate before reading"}""", default)).Success);
        Assert.True((await tools.ExecuteAsync("update_plan", """{"nextTool":"get_product","objective":"Read costs"}""", default)).Success);
        Assert.Equal("get_product", tools.NextTool);
        Assert.True((await tools.ExecuteAsync("get_product", """{"productId":"stand"}""", default)).Success);
        Assert.Null(tools.NextTool);
    }
    [Fact]
    public async Task RecommendationsCannotCiteInventedEvidence()
    {
        var result = await ApplicationTestData.Tools().ExecuteAsync("submit_recommendation", Serialization.Write(new DraftArguments(["invented"], "Draft", ["Synthetic"], ["No overhead"])), default);
        Assert.False(result.Success);
    }
}
