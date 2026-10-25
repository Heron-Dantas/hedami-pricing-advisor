using Hedami.PricingAdvisor.Application.Tools;
using Hedami.PricingAdvisor.Domain;

namespace Hedami.PricingAdvisor.Domain.Tests;

internal static class ApplicationTestData
{
    public static Provenance Provenance => new("fixture-v1", "Synthetic test data", new(2026, 10, 24), new(2026, 10, 26), ValidationStatus.Demonstration);
    public static Product Product => new("stand", "Test stand", new ProductionCosts(100m, 100m, 2m, 0.1m, 1m, 1m, 0.5m, 0.1m, 0.2m, 0.1m, 20m, 2m, 0.1m), Provenance);
    public static ChannelRule Rule => new("direct", "Synthetic channel", 0.1m, 0m, 0m, 2m, Provenance);
    public static PricingTools Tools(bool adaptive = false, Product? product = null, ChannelRule? rule = null, bool allowDemo = true) =>
        new(new PricingCatalog(new([product ?? Product], [rule ?? Rule]), allowDemo, new(2026, 10, 24)), adaptive);
}
