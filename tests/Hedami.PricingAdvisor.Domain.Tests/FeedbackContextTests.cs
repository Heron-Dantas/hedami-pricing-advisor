using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application;
using Hedami.PricingAdvisor.Application.Harness;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Domain.Tests;
public class FeedbackContextTests
{
    [Fact]
    public async Task LargePriorDraftIsReducedToBoundedNumericEvidenceAndNewObjective()
    {
        var tools = ApplicationTestData.Tools();
        await tools.ExecuteAsync("calculate_scenarios", Serialization.Write(new CalculateArguments("stand", "direct", [0.2m, 0.3m, 0.4m], 0.1m)), default);
        var prior = Serialization.Write(new { events = new[] { new { data = new { objective = "Compare 20%, 30%, 40% margins", originalObjective = "Price the stand" } } },
            recommendation = new Recommendation(new string('a', 2900), ["Synthetic"], ["No overhead"], tools.Scenarios.Values.ToArray()), questions = (string[]?)null });
        Assert.True(prior.Length > 8000);
        var feedback = FeedbackContext.Build(prior, "Use 25% instead");
        Assert.Contains("25%", feedback.Objective);
        Assert.True(feedback.Context.Length < 8000);
        var node = JsonNode.Parse(feedback.Context)!;
        Assert.Equal("Price the stand", node["originalObjective"]!.GetValue<string>());
        Assert.Equal(6, node["priorScenarios"]!.AsArray().Count);
        Assert.DoesNotContain("printAttemptBrl", feedback.Context);
    }
}
