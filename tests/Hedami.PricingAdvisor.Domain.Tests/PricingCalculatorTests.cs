using Hedami.PricingAdvisor.Domain;

namespace Hedami.PricingAdvisor.Domain.Tests;

public class PricingCalculatorTests
{
    private static ProductionCosts Complete => new(100m, 100m, 2m, 0.1m, 1m,
        1m, 0.5m, 0.1m, 0.2m, 0.1m, 20m, 2m, 0.1m);

    [Fact]
    public void CostsUseExplicitUnitsAndExpectedRepeatedPrintAttempts()
    {
        var result = PricingCalculator.CalculateCosts(Complete);
        Assert.Equal(10m, result.MaterialBrl);
        Assert.Equal(0.2m, result.EnergyBrl);
        Assert.Equal(3m, result.PrinterBrl);
        Assert.Equal(13.2m, result.PrintAttemptBrl);
        Assert.Equal(8m, result.LaborBrl);
        Assert.Equal(2m, result.PackagingBrl);
        Assert.InRange(result.TotalBrl, 24.66666666666666666666666666m, 24.66666666666666666666666668m);
    }

    [Fact]
    public void UnknownIsRejectedButExplicitZeroIsAccepted()
    {
        Assert.Throws<ArgumentException>(() => PricingCalculator.CalculateCosts(Complete with { EnergyBrlPerKwh = null }));
        Assert.Equal(0m, PricingCalculator.CalculateCosts(Complete with { EnergyBrlPerKwh = 0m }).EnergyBrl);
    }

    [Fact]
    public void NegativeInputsAreRejected() => Assert.Throws<ArgumentOutOfRangeException>(() =>
        PricingCalculator.CalculateCosts(Complete with { MaterialGrams = -1m }));

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1)]
    [InlineData(1.1)]
    public void InvalidFailureProbabilitiesAreRejected(double rate) => Assert.Throws<ArgumentOutOfRangeException>(() =>
        PricingCalculator.CalculateCosts(Complete with { FailureRate = (decimal)rate }));

    [Fact]
    public void ZeroFailureRateAddsNoProvision() => Assert.Equal(0m,
        PricingCalculator.CalculateCosts(Complete with { FailureRate = 0m }).FailureProvisionBrl);

    [Fact]
    public void PriceCoversFixedFeesAndRevenueMargin()
    {
        var result = PricingCalculator.CalculateScenario(20m, 0.1m, 2m, 0.3m);
        Assert.Equal(36.67m, result.RecommendedPriceBrl);
        Assert.Equal(11.003m, result.ContributionBrl);
        Assert.True(result.RevenueMargin >= 0.3m);
        Assert.NotEqual(result.MarkupOnProductionCost, result.RevenueMargin);
    }

    [Fact]
    public void DiscountCanTurnContributionNegative()
    {
        var result = PricingCalculator.CalculateScenario(20m, 0m, 0m, 0.05m, 0.1m);
        Assert.Equal(21.06m, result.RecommendedPriceBrl);
        Assert.Equal(18.95m, result.ChargedPriceBrl);
        Assert.Equal(-1.05m, result.ContributionBrl);
        Assert.True(result.RevenueMargin < 0m);
    }

    [Fact]
    public void InfeasibleMarginIsRejected() => Assert.Throws<ArgumentException>(() =>
        PricingCalculator.CalculateScenario(20m, 0.2m, 0m, 0.8m));

    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(20, -0.1, 0, 0.3, 0)]
    [InlineData(20, 0.1, -1, 0.3, 0)]
    [InlineData(20, 0.1, 0, 1, 0)]
    [InlineData(20, 0.1, 0, 0.3, 1)]
    public void InvalidScenariosAreRejected(decimal cost, decimal rate, decimal fee, decimal margin, decimal discount) =>
        Assert.ThrowsAny<ArgumentException>(() => PricingCalculator.CalculateScenario(cost, rate, fee, margin, discount));

    [Fact]
    public void ChargedPriceRoundsMidpointsAwayFromZero()
    {
        var result = PricingCalculator.CalculateScenario(1.01m, 0m, 0m, 0m, 0.5m);
        Assert.Equal(0.51m, result.ChargedPriceBrl);
    }
}
