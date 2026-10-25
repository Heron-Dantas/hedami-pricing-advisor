namespace Hedami.PricingAdvisor.Domain;

// All amounts are BRL; rates are fractions (0.10 = 10%). Null means unknown.
public sealed record ProductionCosts(
    decimal? MaterialGrams, decimal? FilamentBrlPerKg,
    decimal? PrintHours, decimal? AveragePowerKw, decimal? EnergyBrlPerKwh,
    decimal? PrinterDepreciationBrlPerHour, decimal? PrinterMaintenanceBrlPerHour,
    decimal? PreparationHours, decimal? FinishingHours, decimal? PackingHours,
    decimal? LaborBrlPerHour, decimal? PackagingBrl, decimal? FailureRate);

public sealed record CostBreakdown(decimal MaterialBrl, decimal EnergyBrl,
    decimal PrinterBrl, decimal PrintAttemptBrl, decimal FailureProvisionBrl,
    decimal LaborBrl, decimal PackagingBrl, decimal TotalBrl);

public sealed record PriceScenario(decimal TargetRevenueMargin, decimal RecommendedPriceBrl,
    decimal DiscountRate, decimal ChargedPriceBrl, decimal ContributionBrl,
    decimal RevenueMargin, decimal MarkupOnProductionCost);

public static class PricingCalculator
{
    private static decimal Required(decimal? value, string name)
    {
        if (value is null) throw new ArgumentException($"Missing required value: {name}.", name);
        if (value < 0) throw new ArgumentOutOfRangeException(name, "Must be non-negative.");
        return value.Value;
    }

    private static void Rate(decimal value, string name)
    {
        if (value < 0 || value >= 1) throw new ArgumentOutOfRangeException(name, "Must be in [0, 1).");
    }

    public static CostBreakdown CalculateCosts(ProductionCosts costs)
    {
        ArgumentNullException.ThrowIfNull(costs);
        var grams = Required(costs.MaterialGrams, nameof(costs.MaterialGrams));
        var filament = Required(costs.FilamentBrlPerKg, nameof(costs.FilamentBrlPerKg));
        var hours = Required(costs.PrintHours, nameof(costs.PrintHours));
        var power = Required(costs.AveragePowerKw, nameof(costs.AveragePowerKw));
        var tariff = Required(costs.EnergyBrlPerKwh, nameof(costs.EnergyBrlPerKwh));
        var depreciation = Required(costs.PrinterDepreciationBrlPerHour, nameof(costs.PrinterDepreciationBrlPerHour));
        var maintenance = Required(costs.PrinterMaintenanceBrlPerHour, nameof(costs.PrinterMaintenanceBrlPerHour));
        var preparation = Required(costs.PreparationHours, nameof(costs.PreparationHours));
        var finishing = Required(costs.FinishingHours, nameof(costs.FinishingHours));
        var packing = Required(costs.PackingHours, nameof(costs.PackingHours));
        var hourlyLabor = Required(costs.LaborBrlPerHour, nameof(costs.LaborBrlPerHour));
        var packaging = Required(costs.PackagingBrl, nameof(costs.PackagingBrl));
        var failure = Required(costs.FailureRate, nameof(costs.FailureRate));
        Rate(failure, nameof(costs.FailureRate));
        var material = grams * filament / 1000m;
        var energy = power * hours * tariff;
        var printer = hours * (depreciation + maintenance);
        var attempt = material + energy + printer;
        var provision = attempt * failure / (1m - failure);
        var labor = (preparation + finishing + packing) * hourlyLabor;
        return new(material, energy, printer, attempt, provision, labor, packaging,
            attempt + provision + labor + packaging);
    }

    // Raw scenario rates are caller-supplied assumptions, not validated marketplace rules.
    public static PriceScenario CalculateScenario(decimal productionCostBrl,
        decimal revenueChargeRate, decimal fixedChannelFeeBrl,
        decimal targetRevenueMargin, decimal discountRate = 0m)
    {
        if (productionCostBrl <= 0) throw new ArgumentOutOfRangeException(nameof(productionCostBrl));
        Required(fixedChannelFeeBrl, nameof(fixedChannelFeeBrl));
        Rate(revenueChargeRate, nameof(revenueChargeRate));
        Rate(targetRevenueMargin, nameof(targetRevenueMargin));
        Rate(discountRate, nameof(discountRate));
        var denominator = 1m - revenueChargeRate - targetRevenueMargin;
        if (denominator <= 0) throw new ArgumentException("Target margin is infeasible for the revenue charge rate.");
        var price = decimal.Ceiling((productionCostBrl + fixedChannelFeeBrl) / denominator * 100m) / 100m;
        var charged = decimal.Round(price * (1m - discountRate), 2, MidpointRounding.AwayFromZero);
        if (charged <= 0) throw new ArgumentException("Discount produces a zero charged price.");
        var contribution = charged * (1m - revenueChargeRate) - fixedChannelFeeBrl - productionCostBrl;
        return new(targetRevenueMargin, price, discountRate, charged, contribution,
            contribution / charged, (charged - productionCostBrl) / productionCostBrl);
    }
}
