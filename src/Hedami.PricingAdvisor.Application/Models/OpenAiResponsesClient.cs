using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Application.Models;

// Small REST adapter: no automatic SDK retries, hidden conversation state or marketplace access.
public sealed class OpenAiResponsesClient(HttpClient http, string apiKey, string model, ApiSpendBudget? spendBudget = null) : IModelClient
{
    public string Model => model;
    public bool IsSimulated => false;
    private static readonly Uri Endpoint = new("https://api.openai.com/v1/responses");

    public async Task<ModelTurn> CompleteAsync(string instructions, JsonArray input,
        IReadOnlyList<ToolDefinition> tools, int maxOutputTokens, CancellationToken cancellationToken)
    {
        var payload = new JsonObject
        {
            ["model"] = Model, ["instructions"] = instructions, ["input"] = input.DeepClone(),
            ["tools"] = new JsonArray(tools.Select(x => (JsonNode)x.ToApi()).ToArray()),
            ["tool_choice"] = "required", ["parallel_tool_calls"] = false,
            ["max_output_tokens"] = maxOutputTokens, ["store"] = false,
            ["reasoning"] = new JsonObject { ["effort"] = "none" }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var payloadJson = payload.ToJsonString();
        var reservation = spendBudget?.Reserve(Model, Encoding.UTF8.GetByteCount(payloadJson), maxOutputTokens);
        request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.HttpRequestError == HttpRequestError.NameResolutionError)
        { throw new ModelException("dns_resolution_failed"); }
        catch (HttpRequestException) { throw new ModelException("network_error", true); }
        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var transient = response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500;
                // Read only a bounded error code from an allowlist; never expose arbitrary bodies/messages.
                var billingCode = await BillingCodeAsync(response, cancellationToken);
                throw new ModelException(billingCode is null ? $"openai_http_{(int)response.StatusCode}" : "openai_" + billingCode,
                    billingCode is null && transient);
            }
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            var chunk = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(chunk, cancellationToken)) != 0)
            {
                if (buffer.Length + count > 2_000_000) throw new ModelException("response_too_large");
                buffer.Write(chunk, 0, count);
            }
            try
            {
                var turn = Parse(Encoding.UTF8.GetString(buffer.ToArray()));
                if (reservation is not null && turn.Usage is not null) spendBudget!.Settle(reservation, turn.Model, turn.Usage);
                return turn;
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or OverflowException)
            { throw new ModelException("invalid_model_response"); }
        }
    }

    private static async Task<string?> BillingCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var bytes = new byte[16384];
        var used = 0;
        while (used < bytes.Length)
        {
            var count = await stream.ReadAsync(bytes.AsMemory(used), cancellationToken);
            if (count == 0) break;
            used += count;
        }
        try
        {
            var root = JsonNode.Parse(Encoding.UTF8.GetString(bytes, 0, used));
            var code = root?["error"]?["code"]?.GetValue<string>() ?? root?["error"]?["type"]?.GetValue<string>();
            return code is "insufficient_quota" or "credit_balance_exhausted" or
                "organization_spend_limit_exceeded" or "project_spend_limit_exceeded" or "organization_usage_limit_exceeded" ? code : null;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException) { return null; }
    }

    public static ModelTurn Parse(string json)
    {
        var root = JsonNode.Parse(json)?.AsObject() ?? throw new JsonException();
        var id = root["id"]?.GetValue<string>() ?? throw new JsonException();
        var model = root["model"]?.GetValue<string>() ?? throw new JsonException();
        if (root["status"]?.GetValue<string>() != "completed") throw new ModelException("incomplete_model_response");
        var output = root["output"]?.AsArray() ?? throw new JsonException();
        var calls = new List<ToolCall>();
        var text = new StringBuilder();
        foreach (var item in output)
        {
            switch (item?["type"]?.GetValue<string>())
            {
                case "function_call":
                    calls.Add(new(item["call_id"]?.GetValue<string>() ?? throw new JsonException(),
                        item["name"]?.GetValue<string>() ?? throw new JsonException(),
                        item["arguments"]?.GetValue<string>() ?? throw new JsonException()));
                    break;
                case "message":
                    foreach (var content in item["content"]?.AsArray() ?? [])
                        if (content?["type"]?.GetValue<string>() == "output_text")
                            text.Append(content["text"]?.GetValue<string>());
                    break;
                // Reasoning items, if any, are carried in memory for protocol continuity, never logged or requested.
            }
        }
        ModelUsage? usage = null;
        if (root["usage"] is JsonObject u)
        {
            usage = new(u["input_tokens"]!.GetValue<int>(), u["output_tokens"]!.GetValue<int>(),
                u["input_tokens_details"]?["cached_tokens"]?.GetValue<int>() ?? 0);
            if (usage.InputTokens < 0 || usage.OutputTokens < 0 || usage.CachedInputTokens < 0 || usage.CachedInputTokens > usage.InputTokens)
                throw new JsonException();
        }
        return new(id, model, output.DeepClone().AsArray(), calls.ToArray(), text.ToString(), usage);
    }
}
