using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application.Models;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Domain.Tests;

public class OpenAiResponsesTests
{
    private const string Response = """{"id":"resp_test","model":"fixture-model","status":"completed","output":[{"type":"function_call","call_id":"call_test","name":"get_product","arguments":"{\"productId\":\"stand\"}"}],"usage":{"input_tokens":30,"output_tokens":12,"input_tokens_details":{"cached_tokens":5}}}""";
    [Fact]
    public void ParsesToolCallsAndUsageWithoutRequiringPrivateReasoning()
    {
        var turn = OpenAiResponsesClient.Parse(Response);
        Assert.Equal("get_product", Assert.Single(turn.Calls).Name);
        Assert.Equal(42, turn.Usage!.TotalTokens);
        Assert.Equal(5, turn.Usage.CachedInputTokens);
    }
    [Fact]
    public void IncompleteOutputsCannotBeExecuted() => Assert.Throws<ModelException>(() =>
        OpenAiResponsesClient.Parse(Response.Replace("completed", "incomplete")));
    [Fact]
    public async Task AdapterUsesExplicitEndpointAndStatelessStrictTools()
    {
        var handler = new Handler(async request =>
        {
            Assert.Equal("https://api.openai.com/v1/responses", request.RequestUri!.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            var payload = JsonNode.Parse(await request.Content!.ReadAsStringAsync())!;
            Assert.False(payload["store"]!.GetValue<bool>());
            Assert.False(payload["parallel_tool_calls"]!.GetValue<bool>());
            Assert.True(payload["tools"]![0]!["strict"]!.GetValue<bool>());
            Assert.Equal("required", payload["tool_choice"]!.GetValue<string>());
            return new(HttpStatusCode.OK) { Content = new StringContent(Response, Encoding.UTF8, "application/json") };
        });
        using var http = new HttpClient(handler);
        var client = new OpenAiResponsesClient(http, "fake-test-credential", "fixture-model");
        var turn = await client.CompleteAsync("Instructions", new JsonArray(new JsonObject { ["role"] = "user", ["content"] = "Test" }),
            ApplicationTestData.Tools().Definitions, 128, default);
        Assert.Single(turn.Calls);
    }
    [Fact]
    public async Task RateLimitErrorDoesNotExposeResponseBodyOrCredential()
    {
        using var http = new HttpClient(new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        { Content = new StringContent("sensitive raw body") })));
        var client = new OpenAiResponsesClient(http, "fake-test-credential", "fixture-model");
        var error = await Assert.ThrowsAsync<ModelException>(() => client.CompleteAsync("instructions", [], [], 128, default));
        Assert.Equal("openai_http_429", error.Code);
        Assert.True(error.Transient);
        Assert.DoesNotContain("sensitive", error.ToString());
    }
    [Fact]
    public async Task CreditErrorsExposeOnlyAnAllowlistedCodeAndAreNotRetried()
    {
        using var http = new HttpClient(new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        { Content = new StringContent("""{"error":{"code":"insufficient_quota","message":"private account details"}}""") })));
        var client = new OpenAiResponsesClient(http, "fake-test-credential", "fixture-model");
        var error = await Assert.ThrowsAsync<ModelException>(() => client.CompleteAsync("instructions", [], [], 128, default));
        Assert.Equal("openai_insufficient_quota", error.Code);
        Assert.False(error.Transient);
        Assert.DoesNotContain("private account", error.ToString());
    }
    [Fact]
    public async Task ExhaustedSpendBudgetPreventsHttpTransmission()
    {
        var path = Path.Combine(Path.GetTempPath(), "hedami-spend-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var sent = 0;
            using var budget = new ApiSpendBudget(path, 0.000001m);
            using var http = new HttpClient(new Handler(_ => { sent++; throw new InvalidOperationException("Must not send"); }));
            var client = new OpenAiResponsesClient(http, "fake-test-credential", "gpt-5.4-mini-2026-03-17", budget);
            var error = await Assert.ThrowsAsync<ModelException>(() => client.CompleteAsync("instructions", [], [], 128, default));
            Assert.Equal("spend_limit", error.Code);
            Assert.Equal(0, sent);
        }
        finally { File.Delete(path); }
    }
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }
}
