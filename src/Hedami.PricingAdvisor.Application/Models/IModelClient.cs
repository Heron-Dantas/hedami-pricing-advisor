using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Application.Models;

public sealed record ToolCall(string Id, string Name, string Arguments);
public sealed record ModelUsage(int InputTokens, int OutputTokens, int CachedInputTokens)
{
    public int TotalTokens => checked(InputTokens + OutputTokens);
}
public sealed record ModelTurn(string ResponseId, string Model, JsonArray Output,
    ToolCall[] Calls, string Text, ModelUsage? Usage);
public interface IModelClient
{
    string Model { get; }
    bool IsSimulated { get; }
    Task<ModelTurn> CompleteAsync(string instructions, JsonArray input,
        IReadOnlyList<ToolDefinition> tools, int maxOutputTokens, CancellationToken cancellationToken);
}
public sealed class ModelException(string code, bool transient = false) : Exception(code)
{
    public string Code { get; } = code;
    public bool Transient => transient;
}
