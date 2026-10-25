using System.Globalization;
using Hedami.PricingAdvisor.Domain;

public static class CalculatorDemo
{
public static void Run()
{
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.WriteLine("HEDAMI Pricing Advisor — deterministic calculator demo (no LLM)");
Console.WriteLine("Synthetic BRL assumptions; not current marketplace fees or confirmed business costs.");
var costs = PricingCalculator.CalculateCosts(new ProductionCosts(
    MaterialGrams: 100m, FilamentBrlPerKg: 100m, PrintHours: 2m,
    AveragePowerKw: 0.1m, EnergyBrlPerKwh: 1m,
    PrinterDepreciationBrlPerHour: 1m, PrinterMaintenanceBrlPerHour: 0.5m,
    PreparationHours: 0.1m, FinishingHours: 0.2m, PackingHours: 0.1m,
    LaborBrlPerHour: 20m, PackagingBrl: 2m, FailureRate: 0.1m));
Console.WriteLine($"Production cost: BRL {costs.TotalBrl:F4} (unrounded calculation)");
foreach (var margin in new[] { 0.2m, 0.3m, 0.4m })
{
    foreach (var discount in new[] { 0m, 0.1m })
    {
        var result = PricingCalculator.CalculateScenario(costs.TotalBrl, 0.1m, 2m, margin, discount);
        Console.WriteLine($"Target {margin:P0}; list BRL {result.RecommendedPriceBrl:F2}; discount {discount:P0}; charged BRL {result.ChargedPriceBrl:F2}; contribution BRL {result.ContributionBrl:F4}; margin {result.RevenueMargin:P2}");
    }
}
Console.WriteLine("Contribution excludes unmodeled overhead; it is not net profit. Recommendation only.");

}
}
