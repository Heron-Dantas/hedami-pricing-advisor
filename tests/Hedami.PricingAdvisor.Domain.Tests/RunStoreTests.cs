using Hedami.PricingAdvisor.Application.Harness;

namespace Hedami.PricingAdvisor.Domain.Tests;

public class RunStoreTests
{
    [Fact]
    public async Task AtomicTracePersistenceRedactsConfiguredCredential()
    {
        var directory = Path.Combine(Path.GetTempPath(), "hedami-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            var run = new RunRecord { RequestedModel = "test", Mode = "Tools", Limits = new(), InstructionsVersion = "test" };
            run.Events.Add(new(1, DateTimeOffset.UtcNow, "test", new { value = "fake-sensitive-value" }));
            await new JsonRunStore(directory, "fake-sensitive-value").SaveAsync(run);
            var text = await File.ReadAllTextAsync(Path.Combine(directory, run.Id + ".json"));
            Assert.DoesNotContain("fake-sensitive-value", text);
            Assert.Contains("[REDACTED]", text);
            Assert.Single(Directory.GetFiles(directory));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
}
