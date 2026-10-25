using Hedami.PricingAdvisor.Domain;

namespace Hedami.PricingAdvisor.Application.Tools;

public enum ValidationStatus { Demonstration, Confirmed, Unverified }
public sealed record Provenance(string Id, string Source, DateOnly ValidFrom,
    DateOnly ValidThrough, ValidationStatus Status);
public sealed record Product(string Id, string Name, ProductionCosts Costs, Provenance Provenance);
public sealed record ChannelRule(string Id, string Channel, decimal? ChannelRate,
    decimal? TaxRate, decimal? AdvertisingRate, decimal? FixedFeeBrl, Provenance Provenance);
public sealed record CatalogProduct(string Id, string Name, ValidationStatus Status);
public sealed record CatalogRule(string Id, string Channel, Provenance Provenance);
public sealed record CatalogOverview(CatalogProduct[] Products, CatalogRule[] Rules);
public sealed record PricingDataset(Product[] Products, ChannelRule[] Rules);

public sealed class PricingCatalog(PricingDataset dataset, bool allowDemonstration, DateOnly asOf)
{
    public CatalogOverview Overview() => new(
        dataset.Products.Select(x => new CatalogProduct(x.Id, x.Name, x.Provenance.Status)).ToArray(),
        dataset.Rules.Select(x => new CatalogRule(x.Id, x.Channel, x.Provenance)).ToArray());
    public bool AllowDemonstration => allowDemonstration;
    public DateOnly AsOf => asOf;
    public Product Product(string id) => dataset.Products.SingleOrDefault(x => x.Id == id)
        ?? throw new ArgumentException("Product not found.");
    public ChannelRule Rule(string id) => dataset.Rules.SingleOrDefault(x => x.Id == id)
        ?? throw new ArgumentException("Channel rule not found. Ask for a sourced rule; do not invent fees.");
    public void Validate(Provenance provenance)
    {
        if (string.IsNullOrWhiteSpace(provenance.Id) || string.IsNullOrWhiteSpace(provenance.Source))
            throw new ArgumentException("Rule/data identity and source are required.");
        if (provenance.ValidFrom > asOf || provenance.ValidThrough < asOf)
            throw new ArgumentException("Data or rule is outside its effective dates.");
        if (provenance.Status != ValidationStatus.Confirmed &&
            !(allowDemonstration && provenance.Status == ValidationStatus.Demonstration))
            throw new ArgumentException("Data or rule is unverified; request confirmation.");
    }
    public static decimal RevenueRate(ChannelRule rule)
    {
        var rates = new[] { rule.ChannelRate, rule.TaxRate, rule.AdvertisingRate };
        if (rates.Any(x => x is null)) throw new ArgumentException("A revenue charge is unknown. Request the missing rate.");
        if (rates.Any(x => x < 0 || x >= 1)) throw new ArgumentException("Each revenue charge must be in [0, 1).");
        if (rule.FixedFeeBrl is null or < 0) throw new ArgumentException("A non-negative fixed fee must be explicit.");
        return rates.Sum(x => x!.Value);
    }
}
