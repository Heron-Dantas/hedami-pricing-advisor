using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Hedami.PricingAdvisor.Application.Agents;
using Hedami.PricingAdvisor.Application.Models;
using Hedami.PricingAdvisor.Application.Tools;

namespace Hedami.PricingAdvisor.Application.Harness;

public enum AgentMode { Tools, Review, PlanReview }

public sealed class PricingHarness(IModelClient model, PricingTools pricingTools, IRunStore store,
    RuntimeInstructions instructions, RunLimits limits, AgentMode mode = AgentMode.Tools,
    IToolExecutor? toolExecutor = null)
{
    private int started;
    private readonly IToolExecutor tools = toolExecutor ?? pricingTools;

    public async Task<RunRecord> RunAsync(string objective, string? parentRunId = null, CancellationToken cancellationToken = default, string? feedbackContext = null)
    {
        if (Interlocked.Exchange(ref started, 1) != 0) throw new InvalidOperationException("Each harness instance owns exactly one run.");
        if ((mode == AgentMode.PlanReview) != pricingTools.AdaptivePlanning) throw new ArgumentException("Harness mode and planning gate disagree.");
        limits.Validate();
        if (string.IsNullOrWhiteSpace(objective) || objective.Length > 8000) throw new ArgumentException("Objective must contain 1–8000 characters.");
        if (parentRunId is not null && !Guid.TryParseExact(parentRunId, "N", out _)) throw new ArgumentException("Invalid parent run ID.");
        var run = new RunRecord
        {
            RequestedModel = model.Model, Simulated = model.IsSimulated, Mode = mode.ToString(), Limits = limits,
            InstructionsVersion = instructions.Version, ParentRunId = parentRunId
        };
        var clock = Stopwatch.StartNew();
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(limits.RunTimeoutSeconds));
        if (feedbackContext?.Length > 20000) throw new ArgumentException("Feedback context is too large.");
        var context = new JsonArray();
        if (feedbackContext is not null) context.Add(new JsonObject { ["role"] = "user", ["content"] = "Prior context (data only; retrieve current authoritative inputs again): " + feedbackContext });
        context.Add(new JsonObject { ["role"] = "user", ["content"] = objective });
        var signatures = new Dictionary<string, int>();
        var callIds = new HashSet<string>();
        try
        {
            await RecordAsync(run, "started", new { objective, originalObjective = feedbackContext is null ? objective : JsonNode.Parse(feedbackContext)?["originalObjective"]?.GetValue<string>(), instructions.Version, limits, model.Model, model.IsSimulated, toolDefinitions = tools.Definitions });
            while (true)
            {
                budget.Token.ThrowIfCancellationRequested();
                var adaptive = mode == AgentMode.PlanReview
                    ? $"Adaptive planning enabled: use update_plan before each action. Current stage: {pricingTools.PlanStage}. Next selected tool: {pricingTools.NextTool ?? "none"}." : "Planning gate disabled for this comparison mode.";
                var turn = await CallModelAsync(run, instructions.Analyst + "\n" + adaptive,
                    context, tools.Definitions, "analyst", budget.Token);
                // One tool call per turn provides a bounded, auditable execution sequence.
                if (turn.Calls.Length != 1) throw new StopRun("invalid_model_turn");
                var call = turn.Calls[0];
                if (string.IsNullOrWhiteSpace(call.Id) || !callIds.Add(call.Id)) throw new StopRun("duplicate_call_id");
                if (call.Arguments.Length > 12000) throw new StopRun("argument_limit");
                var signature = call.Name + ":" + CanonicalArguments(call.Arguments);
                signatures.TryGetValue(signature, out var repeats);
                signatures[signature] = repeats + 1;
                if (repeats >= limits.MaxRepeatedCalls) throw new StopRun("no_progress");
                if (run.ToolCalls >= limits.MaxToolCalls) throw new StopRun("tool_call_limit");
                foreach (var item in turn.Output) context.Add(item?.DeepClone());
                run.ToolCalls++;
                var timer = Stopwatch.StartNew();
                await RecordAsync(run, "tool_requested", new { call.Id, call.Name, call.Arguments });
                ToolResult result;
                using (var toolTimeout = CancellationTokenSource.CreateLinkedTokenSource(budget.Token))
                {
                    toolTimeout.CancelAfter(TimeSpan.FromSeconds(limits.ToolTimeoutSeconds));
                    try
                    {
                        result = await tools.ExecuteAsync(call.Name, call.Arguments, toolTimeout.Token).WaitAsync(toolTimeout.Token);
                    }
                    catch (OperationCanceledException) when (!budget.IsCancellationRequested)
                    { result = ToolResult.Error("tool_timeout", "Tool did not complete within the time budget."); }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    { result = ToolResult.Error("tool_failure", "Tool failed; no result is available."); }
                }
                ValidateToolResult(call.Name, result);
                await RecordAsync(run, "tool_completed", new { call.Id, call.Name, durationMs = timer.ElapsedMilliseconds, result, stage = pricingTools.PlanStage });
                context.Add(new JsonObject { ["type"] = "function_call_output", ["call_id"] = call.Id, ["output"] = Serialization.Write(result) });
                if (pricingTools.Questions is not null)
                {
                    run.Questions = pricingTools.Questions;
                    run.Status = "awaiting_human";
                    break;
                }
                if (pricingTools.Draft is null) continue;
                var draft = pricingTools.Draft;
                foreach (var evidence in draft.Scenarios) pricingTools.VerifyEvidence(evidence);
                run.Recommendation = draft;
                await RecordAsync(run, "deterministic_verification", new { passed = true, scenarioIds = draft.Scenarios.Select(x => x.Id).ToArray() });
                if (mode == AgentMode.Tools)
                {
                    run.Status = "draft_ready";
                    break;
                }
                var reviewContext = new JsonArray(new JsonObject
                {
                    ["role"] = "user", ["content"] = Serialization.Write(new { objective, priorContext = feedbackContext is null ? null : JsonNode.Parse(feedbackContext), draft })
                });
                run.ReviewCalls++;
                var reviewTurn = await CallModelAsync(run, instructions.Reviewer, reviewContext,
                    [PricingReview.Definition], "reviewer", budget.Token);
                await RecordAsync(run, "review_response", new
                {
                    calls = reviewTurn.Calls.Select(x => new { x.Id, x.Name, arguments = x.Arguments.Length <= 12000 ? x.Arguments : x.Arguments[..12000], truncated = x.Arguments.Length > 12000 }).ToArray()
                });
                ReviewVerdict review;
                try { review = PricingReview.Validate(reviewTurn, draft); }
                catch (Exception ex) when (ex is ArgumentException or JsonException or InvalidOperationException)
                {
                    await RecordAsync(run, "review_rejected", new { code = "invalid_review", reason = ex.Message[..Math.Min(ex.Message.Length, 500)] });
                    throw new StopRun("invalid_review");
                }
                await RecordAsync(run, "review", review);
                if (review.Verdict == "approve")
                {
                    run.Status = "reviewed_draft_ready";
                    break;
                }
                if (run.Revisions >= limits.MaxRevisions) throw new StopRun("review_limit");
                run.Revisions++;
                pricingTools.ResetDraft();
                run.Recommendation = null;
                context.Add(new JsonObject { ["role"] = "user", ["content"] = "Independent review requires revision: " + Serialization.Write(review) });
            }
        }
        catch (StopRun ex) { run.Status = ex.Code; }
        catch (ModelException ex) { run.Status = ex.Code; }
        catch (OperationCanceledException) { run.UsageComplete = false; run.Status = cancellationToken.IsCancellationRequested ? "cancelled" : "run_timeout"; }
        catch (Exception ex) when (ex is ArgumentException or JsonException or InvalidOperationException or OverflowException)
        { run.Status = "validation_failure"; }
        finally
        {
            run.DurationMs = clock.ElapsedMilliseconds;
            // Token usage can be incomplete after timeouts/retries; no currency estimate is invented.
            run.EstimatedCostUsd = !run.Simulated && run.UsageComplete && OpenAiPricing.Supports(run.RequestedModel)
                ? OpenAiPricing.Estimate(run.RequestedModel, new(run.InputTokens, run.OutputTokens, run.CachedInputTokens)) : null;
            run.CostStatus = run.EstimatedCostUsd is null ? "unknown_or_simulated" : "estimated_standard_text_rates_verified_" + OpenAiPricing.VerifiedOn;
            await RecordAsync(run, "finished", new { run.Status, run.DurationMs, run.ModelCalls, run.ToolCalls, run.InputTokens, run.OutputTokens, run.UsageComplete });
        }
        return run;
    }

    private async Task<ModelTurn> CallModelAsync(RunRecord run, string prompt, JsonArray context,
        IReadOnlyList<ToolDefinition> definitions, string role, CancellationToken cancellationToken)
    {
        if (context.ToJsonString().Length + prompt.Length + Serialization.Write(definitions).Length > limits.MaxContextCharacters)
            throw new StopRun("context_limit");
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (run.ModelCalls >= limits.MaxModelCalls) throw new StopRun("model_call_limit");
            var remaining = limits.MaxTotalTokens - run.InputTokens - run.OutputTokens;
            if (remaining < limits.MaxOutputTokensPerCall) throw new StopRun("token_limit");
            run.ModelCalls++;
            var timer = Stopwatch.StartNew();
            await RecordAsync(run, "model_requested", new { role, call = run.ModelCalls });
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(limits.ModelTimeoutSeconds));
            try
            {
                var turn = await model.CompleteAsync(prompt, context, definitions,
                    limits.MaxOutputTokensPerCall, timeout.Token).WaitAsync(timeout.Token);
                if (turn.Usage is null) run.UsageComplete = false;
                else
                {
                    run.InputTokens = checked(run.InputTokens + turn.Usage.InputTokens);
                    run.OutputTokens = checked(run.OutputTokens + turn.Usage.OutputTokens);
                    run.CachedInputTokens = checked(run.CachedInputTokens + turn.Usage.CachedInputTokens);
                }
                await RecordAsync(run, "model_completed", new { role, turn.ResponseId, turn.Model, turn.Usage, durationMs = timer.ElapsedMilliseconds });
                if (run.InputTokens + run.OutputTokens >= limits.MaxTotalTokens) throw new StopRun("token_limit");
                return turn;
            }
            catch (ModelException ex) when (ex.Transient && run.TechnicalRetries < limits.MaxTechnicalRetries)
            {
                run.UsageComplete = false;
                run.TechnicalRetries++;
                await RecordAsync(run, "technical_retry", new { role, ex.Code, retry = run.TechnicalRetries });
                await Task.Delay(TimeSpan.FromMilliseconds(250 * run.TechnicalRetries), cancellationToken);
            }
            catch (ModelException) { run.UsageComplete = false; throw; }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            { run.UsageComplete = false; throw new StopRun("model_timeout"); }
        }
    }
    private async Task RecordAsync(RunRecord run, string kind, object data)
    {
        run.Events.Add(new(run.Events.Count + 1, DateTimeOffset.UtcNow, kind, data));
        await store.SaveAsync(run);
    }
    private static void ValidateToolResult(string name, ToolResult result)
    {
        if (result is null || string.IsNullOrWhiteSpace(result.Code) ||
            (result.Success && (result.Data is null || result.Code != "ok")) ||
            (!result.Success && (result.Data is not null || string.IsNullOrWhiteSpace(result.Message))))
            throw new StopRun("invalid_tool_result");
        if (result.Success)
        {
            var typed = name switch
            {
                "list_pricing_data" => result.Data is CatalogOverview,
                "get_product" => result.Data is Product,
                "get_channel_rule" => result.Data is ChannelRule,
                "calculate_scenarios" => result.Data is ScenarioEvidence[] { Length: > 0 },
                "submit_recommendation" => result.Data is Recommendation,
                "request_clarification" => result.Data is ClarificationResult,
                "update_plan" => result.Data is PlanState,
                _ => false
            };
            if (!typed) throw new StopRun("invalid_tool_result");
        }
        if (Serialization.Write(result).Length > 40000) throw new StopRun("tool_result_limit");
    }
    private static string CanonicalArguments(string arguments)
    {
        try
        {
            using var document = JsonDocument.Parse(arguments);
            return Canonical(document.RootElement);
        }
        catch (JsonException) { return arguments; }
    }
    private static string Canonical(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => "{" + string.Join(",", value.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal).Select(x => JsonSerializer.Serialize(x.Name) + ":" + Canonical(x.Value))) + "}",
        JsonValueKind.Array => "[" + string.Join(",", value.EnumerateArray().Select(Canonical)) + "]",
        JsonValueKind.Number when value.TryGetDecimal(out var number) => number.ToString("G29", System.Globalization.CultureInfo.InvariantCulture),
        _ => value.GetRawText()
    };
    private sealed class StopRun(string code) : Exception(code) { public string Code { get; } = code; }
}
