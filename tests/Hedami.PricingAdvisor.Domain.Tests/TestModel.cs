using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application;
using Hedami.PricingAdvisor.Application.Models;
using Hedami.PricingAdvisor.Application.Tools;
using Hedami.PricingAdvisor.Application.Harness;

namespace Hedami.PricingAdvisor.Domain.Tests;

internal sealed class TestModel(Func<int, JsonArray, ModelTurn> respond) : IModelClient
{
    public string Model => "simulated-test-model";
    public bool IsSimulated => true;
    public int Calls { get; private set; }
    public List<JsonArray> Contexts { get; } = [];
    public List<IReadOnlyList<ToolDefinition>> Definitions { get; } = [];
    public Task<ModelTurn> CompleteAsync(string instructions, JsonArray input, IReadOnlyList<ToolDefinition> tools, int maxOutputTokens, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Contexts.Add(input.DeepClone().AsArray());
        Definitions.Add(tools);
        return Task.FromResult(respond(++Calls, input));
    }
    public static ModelTurn Call(string name, object arguments)
    {
        var id = Guid.NewGuid().ToString("N");
        var json = arguments is string raw ? raw : Serialization.Write(arguments);
        return new("response-" + id, "simulated-test-model", new JsonArray(new JsonObject
        {
            ["type"] = "function_call", ["call_id"] = id, ["name"] = name, ["arguments"] = json
        }), [new(id, name, json)], "", new(10, 10, 0));
    }
}
internal sealed class MemoryStore : IRunStore
{
    public List<string> Snapshots { get; } = [];
    public Task SaveAsync(RunRecord run) { Snapshots.Add(Serialization.Write(run)); return Task.CompletedTask; }
}
internal sealed class FaultingTools(IToolExecutor inner, bool timeout) : IToolExecutor
{
    public IReadOnlyList<ToolDefinition> Definitions => inner.Definitions;
    public async Task<ToolResult> ExecuteAsync(string name, string arguments, CancellationToken cancellationToken)
    {
        if (timeout) await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new IOException("Test tool failure");
    }
}
