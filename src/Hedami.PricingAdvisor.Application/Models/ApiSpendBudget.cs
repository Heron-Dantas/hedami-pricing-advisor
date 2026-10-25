using System.Text.Json;

namespace Hedami.PricingAdvisor.Application.Models;

// Verified public standard text rates; estimates, not an account billing integration.
public static class OpenAiPricing
{
    public const string Source = "https://developers.openai.com/api/docs/models/gpt-5.4-mini";
    public const string VerifiedOn = "2026-10-24";
    public static bool Supports(string model) => model is "gpt-5.4-mini-2026-03-17" or "gpt-5.4-mini";
    public static decimal Estimate(string model, ModelUsage usage)
    {
        if (!Supports(model) || usage.InputTokens < 0 || usage.OutputTokens < 0 || usage.CachedInputTokens < 0 || usage.CachedInputTokens > usage.InputTokens)
            throw new ModelException("unpriced_model_or_usage");
        return ((usage.InputTokens - usage.CachedInputTokens) * 0.75m + usage.CachedInputTokens * 0.075m + usage.OutputTokens * 4.50m) / 1_000_000m;
    }
}
public sealed record SpendEntry(string Id, decimal ReservedUsd, decimal? EstimatedActualUsd,
    string State, DateTimeOffset StartedAt);
public sealed record SpendLedger(decimal LimitUsd, string RateSource, string RatesVerifiedOn, List<SpendEntry> Entries);

// Exclusive file lock prevents concurrent CLI processes from reserving the same remaining budget.
// Reservations survive crashes/errors so unknown consumption never silently becomes zero.
public sealed class ApiSpendBudget : IDisposable
{
    private readonly FileStream file;
    private readonly SpendLedger ledger;
    public decimal UsedOrReservedUsd => ledger.Entries.Sum(x => x.EstimatedActualUsd ?? x.ReservedUsd);
    public ApiSpendBudget(string path, decimal authorizedLimitUsd = 2m)
    {
        if (authorizedLimitUsd <= 0 || authorizedLimitUsd > 2m) throw new ArgumentOutOfRangeException(nameof(authorizedLimitUsd));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try
        {
            ledger = file.Length == 0 ? new(authorizedLimitUsd, OpenAiPricing.Source, OpenAiPricing.VerifiedOn, []) :
                JsonSerializer.Deserialize<SpendLedger>(file, Serialization.Json) ?? throw new JsonException("Invalid spend ledger.");
            if (ledger.LimitUsd != authorizedLimitUsd || ledger.RateSource != OpenAiPricing.Source || ledger.RatesVerifiedOn != OpenAiPricing.VerifiedOn ||
                ledger.Entries.Any(x => x.ReservedUsd < 0 || x.EstimatedActualUsd < 0))
                throw new ArgumentException("Spend ledger configuration mismatch.");
            Save();
        }
        catch { file.Dispose(); throw; }
    }
    public string Reserve(string model, int requestUtf8Bytes, int maxOutputTokens)
    {
        if (!OpenAiPricing.Supports(model)) throw new ModelException("unpriced_model");
        if (requestUtf8Bytes < 0 || maxOutputTokens < 1) throw new ModelException("invalid_spend_reservation");
        // Whole serialized request bytes plus a conservative protocol allowance bound text input.
        // Charge all input at the uncached rate, and reserve the entire allowed output.
        var upper = OpenAiPricing.Estimate(model, new(checked(requestUtf8Bytes + 1024), maxOutputTokens, 0));
        if (UsedOrReservedUsd + upper > ledger.LimitUsd) throw new ModelException("spend_limit");
        var id = Guid.NewGuid().ToString("N");
        ledger.Entries.Add(new(id, upper, null, "reserved_or_unknown", DateTimeOffset.UtcNow));
        Save();
        return id;
    }
    public void Settle(string id, string model, ModelUsage usage)
    {
        var index = ledger.Entries.FindIndex(x => x.Id == id);
        if (index < 0 || ledger.Entries[index].State != "reserved_or_unknown") throw new ModelException("invalid_spend_settlement");
        var actual = OpenAiPricing.Estimate(model, usage);
        if (actual > ledger.Entries[index].ReservedUsd) throw new ModelException("spend_reservation_exceeded");
        ledger.Entries[index] = ledger.Entries[index] with { EstimatedActualUsd = actual, State = "settled_estimate" };
        Save();
    }
    private void Save()
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(ledger, Serialization.Json);
        file.Position = 0;
        file.Write(bytes);
        file.SetLength(bytes.Length);
        file.Flush(flushToDisk: true);
    }
    public void Dispose() => file.Dispose();
}
