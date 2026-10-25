using System.Security.Cryptography;
using System.Text;

namespace Hedami.PricingAdvisor.Application.Agents;

public sealed record RuntimeInstructions(string Analyst, string Reviewer, string Version)
{
    // Explicit allowlist: arbitrary model/user paths cannot be loaded as runtime instructions.
    public static RuntimeInstructions Load(string directory)
    {
        var analyst = File.ReadAllText(Path.Combine(directory, "analyze-profitability.md"));
        var reviewer = File.ReadAllText(Path.Combine(directory, "review-pricing.md"));
        if (analyst.Length is < 20 or > 16000 || reviewer.Length is < 20 or > 16000)
            throw new ArgumentException("Runtime instruction size is invalid.");
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(analyst + "\n" + reviewer)));
        return new(analyst, reviewer, "runtime-v1:" + digest);
    }
}
