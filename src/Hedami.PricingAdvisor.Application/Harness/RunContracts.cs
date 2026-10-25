using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Application.Harness;

public sealed record RunLimits(int MaxModelCalls = 12, int MaxToolCalls = 12,
    int MaxOutputTokensPerCall = 1800, int MaxTotalTokens = 40000,
    int MaxContextCharacters = 90000, int MaxRepeatedCalls = 2,
    int MaxTechnicalRetries = 1, int MaxRevisions = 1,
    int RunTimeoutSeconds = 180, int ModelTimeoutSeconds = 45, int ToolTimeoutSeconds = 5)
{
    public void Validate()
    {
        if (MaxModelCalls is < 1 or > 30 || MaxToolCalls is < 1 or > 40 ||
            MaxOutputTokensPerCall is < 128 or > 8000 || MaxTotalTokens is < 128 or > 200000 ||
            MaxContextCharacters is < 1000 or > 200000 || MaxRepeatedCalls is < 1 or > 4 ||
            MaxTechnicalRetries is < 0 or > 3 || MaxRevisions is < 0 or > 2 ||
            RunTimeoutSeconds is < 1 or > 600 || ModelTimeoutSeconds is < 1 or > 120 || ToolTimeoutSeconds is < 1 or > 30)
            throw new ArgumentException("Run limits are outside the supported bounds.");
    }
}
public sealed record RunEvent(int Sequence, DateTimeOffset At, string Kind, object Data);
public sealed class RunRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string? ParentRunId { get; init; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = "running";
    public bool Simulated { get; init; }
    public required string RequestedModel { get; init; }
    public required string Mode { get; init; }
    public required RunLimits Limits { get; init; }
    public required string InstructionsVersion { get; init; }
    public List<RunEvent> Events { get; } = [];
    public int ModelCalls { get; set; }
    public int ToolCalls { get; set; }
    public int TechnicalRetries { get; set; }
    public int Revisions { get; set; }
    public int ReviewCalls { get; set; }
    public long DurationMs { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int CachedInputTokens { get; set; }
    public bool UsageComplete { get; set; } = true;
    public decimal? EstimatedCostUsd { get; set; }
    public string CostStatus { get; set; } = "not_estimated";
    public Recommendation? Recommendation { get; set; }
    public string[]? Questions { get; set; }
}
public interface IRunStore
{
    Task SaveAsync(RunRecord run);
}
