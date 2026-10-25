using Hedami.PricingAdvisor.Application.Models;

namespace Hedami.PricingAdvisor.Domain.Tests;

public class ApiSpendBudgetTests
{
    private const string Model = "gpt-5.4-mini-2026-03-17";
    [Fact]
    public void ReportedUsageAccountsForCachedInputAtItsOwnRate() =>
        Assert.Equal(0.002325m, OpenAiPricing.Estimate(Model, new(1000, 500, 1000)));
    [Fact]
    public void ReservationsPreventOverspendBeforeSendingRequests()
    {
        var path = Path.Combine(Path.GetTempPath(), "hedami-spend-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            using var budget = new ApiSpendBudget(path, 0.01m);
            budget.Reserve(Model, 1000, 1800);
            var error = Assert.Throws<ModelException>(() => budget.Reserve(Model, 1000, 1800));
            Assert.Equal("spend_limit", error.Code);
        }
        finally { File.Delete(path); }
    }
    [Fact]
    public void UnknownConsumptionSurvivesProcessRestartAndBlocksConcurrentWriters()
    {
        var path = Path.Combine(Path.GetTempPath(), "hedami-spend-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            decimal reserved;
            using (var budget = new ApiSpendBudget(path))
            {
                budget.Reserve(Model, 1000, 1800);
                reserved = budget.UsedOrReservedUsd;
                Assert.Throws<IOException>(() => new ApiSpendBudget(path));
            }
            using var restarted = new ApiSpendBudget(path);
            Assert.Equal(reserved, restarted.UsedOrReservedUsd);
            Assert.True(reserved > 0);
        }
        finally { File.Delete(path); }
    }
    [Fact]
    public void SettledUsageReleasesExcessReservationAndUnknownModelsAreDenied()
    {
        var path = Path.Combine(Path.GetTempPath(), "hedami-spend-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            using var budget = new ApiSpendBudget(path);
            var id = budget.Reserve(Model, 1000, 1800);
            budget.Settle(id, Model, new(100, 100, 0));
            Assert.Equal(0.000525m, budget.UsedOrReservedUsd);
            Assert.Throws<ModelException>(() => budget.Reserve("unpriced", 100, 100));
            Assert.Throws<ModelException>(() => budget.Settle(id, Model, new(100, 100, 0)));
        }
        finally { File.Delete(path); }
    }
}
