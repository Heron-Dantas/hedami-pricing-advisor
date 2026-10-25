# Consolidated prototype results — October 24, 2026

## Scope and evidence

The console MVP has completed real OpenAI tool-use workflows using fictional business data only. No marketplace price was applied, no repository was published and no deployment was performed.

Evidence comprises a 42-case original A/B/C comparison, a 12-case targeted follow-up, two standalone smoke workflows and one linked human-feedback workflow: **57 real workflows**, plus one separate failed restricted-session DNS attempt. These are workflow counts, not individual HTTP calls. The persistent [request-spend ledger](api-spend-final.json) contains **280 settled response-usage records** and an estimated total **USD 0.44420565**, below Heron's USD 2 authorization. Estimates use documented standard rates, not an account invoice or remaining-balance lookup.

The independent local assertion suite passed **57 cases** (a separate count from real workflows). The solution build passed. VSTest sockets remain blocked in this Codex session; the actual xUnit assertions were executed through the committed in-process fallback. NuGet vulnerability metadata was unavailable in this session. The latest pointer/diagnostic changes passed those assertions before follow-up execution.

## Original comparison

See [baseline results](baseline-results.md), the [original 42-run report](baseline/evaluations-20261024T043445508.json), all [baseline traces](baseline/runs/) and [inspection notes](baseline/inspection-notes.md).

Original automatic passes were 11/14 tools, 8/14 review and 6/14 plan-review. Safe clarification false negatives, problematic expiry wording and invalid review contracts are separate failure categories. The original failures are preserved. No production reliability percentage is inferred from these small samples.

## Targeted follow-up

Two recommendation scenarios (complete inputs and a harmful discount), three modes, two repetitions per mode. Prompt/diagnostic/pointer changes make this a separate experiment; its smaller scenario mix is not directly comparable with the baseline's overall success fraction.

| Mode | Automatic passes | Median duration (seconds) | Model calls | Reported tokens | Estimated USD |
| --- | ---: | ---: | ---: | ---: | ---: |
| Tools | 4/4 | 4.263 | 16 | 28,358 | 0.025516 |
| Review | 2/4 | 9.325 | 26 | 65,969 | 0.058973 |
| PlanReview | 1/4 | 14.020 | 46 | 139,299 | 0.107119 |

See the [original follow-up report](followup/evaluations-20261024T043908058.json), [summary](followup/comparison-summary.md) and [traces](followup/runs/). No invalid_review status occurred in this follow-up; three runs exhausted the review budget and two exhausted the token budget. That observation supports contract compatibility in this sample, not general review quality or proof of the exact cause of the original invalid reviews.

One review usefully requested an explicit negative-contribution statement, which the analyst added before approval (run `7da5ec9a812b41a982db45350effca87`). Other reviews requested repetitive wording, information already nested in provenance or changes to immutable scenario fields that the analyst could not make through its draft tool. More review did not consistently yield a completed useful recommendation. Bounds stopped unresolved iterations.

Tools-only remains the default. Reviewer/planner modes remain optional experiments with observable limitations, not production quality controls proven superior to deterministic checks.

## Human feedback

[Linked real run](followup/runs/85d6a4cc56a74a7b9051f0a378393c3a.json) changed the prior target to exactly 25%, retained the same authoritative synthetic product/rule and recalculated both discount cases. It returned draft_ready:

- List/undiscounted price BRL 41.03; displayed margin 25.01%.
- With 10% discount: charged price BRL 36.93; displayed margin 17.79%.
- Contribution remained positive; the requested margin was not retained.

The trace links parent run `3d1c4902fd5f4af18270846247410600`, re-reads product/rule data and contains only the newly requested 25% scenarios. It does not mutate the catalog or apply a price.

## Arithmetic cross-check

Codex independently checked 48 baseline calculation rows and 50 follow-up/feedback rows with Python Decimal against the documented synthetic formulas, ceiling and discount rounding, contribution and margin. All 98 matched (1e-24 tolerance for unrounded ratios). Raw archived JSON is copied without reserializing decimal values. This supplements the C# unit tests and runtime verification; it does not validate fictional inputs as business facts.

## Remaining work and limits

Heron should review and understand the code/decisions, annotate recommendation/reviewer usefulness and rehearse the demo. A local Git checkpoint still requires a host-terminal initialization and confirmed identity because this session cannot write .git. Publication, deployment, PDF/slides, web UI and MCP require separate decisions; none is needed to claim this console prototype exists.

Known limitations remain explicit: strict grader path assumptions create false negatives; reviewer findings are traceable but not guaranteed correct/actionable; planning grows context and can hit budgets; expiry-wording prompt changes were not retested in the recommendation-only follow-up; pricing rates/ledger are local estimates; runtime persistence is local JSON; credentials and traces are not a multi-user production security model. Heron's contribution includes problem/scope/constraints, provider/budget/data choices and host-terminal execution; his code review is not falsely claimed complete.
