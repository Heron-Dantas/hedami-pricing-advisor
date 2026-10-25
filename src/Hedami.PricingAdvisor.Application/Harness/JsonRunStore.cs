using System.Text.RegularExpressions;

namespace Hedami.PricingAdvisor.Application.Harness;

public sealed class JsonRunStore(string directory, string? secret = null) : IRunStore
{
    public async Task SaveAsync(RunRecord run)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, run.Id + ".json");
        var text = Serialization.Write(run);
        if (!string.IsNullOrEmpty(secret)) text = text.Replace(secret, "[REDACTED]", StringComparison.Ordinal);
        text = Regex.Replace(text, @"sk-[A-Za-z0-9_-]{12,}", "[REDACTED]");
        var temp = path + ".tmp";
        // Deliberately independent of run cancellation so the final cancellation status is durable.
        await File.WriteAllTextAsync(temp, text);
        File.Move(temp, path, overwrite: true);
    }
}
