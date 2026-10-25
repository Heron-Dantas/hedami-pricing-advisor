# Evaluations and evidence

## Distinct evaluation layers

1. Deterministic arithmetic tests assert independently specified units, cost components, failure provision, margin, markup, discount impact, infeasibility and rounding.
2. Tool tests check explicit-zero versus missing data, strict argument shapes, forbidden cost overrides, dated/validated rules, demonstration permission, atomic batch behavior, plan gates and unknown scenario IDs.
3. Scripted model tests exercise complete loops, clarification, canonical repeated-call detection, faults/timeouts, cancellation, reported-token limits, technical retry exhaustion, result types, reviewer isolation, evidence pointers, review limits and a full planning path. Scripted models provide controlled inputs; these tests do not demonstrate model intelligence or reliability.
4. Mocked HTTP tests verify the OpenAI Responses wire contract, usage parsing, incomplete outputs, strict/stateless settings and sanitized provider failures. No real credential or network call is used.
5. One real tools-only smoke run completed in Heron's terminal and was inspected. The original seven-scenario, two-repetition A/B/C comparison completed (42 runs); its metrics and failure inspection are recorded in [baseline results](evidence/baseline-results.md). See [first real run](evidence/2026-10-24-first-real-run.md).

The current suite passed **57 cases** in the in-process fallback on October 24. Standard VSTest remains blocked by session sockets. See [agent increment evidence](evidence/2026-10-24-agent-increment.md).

## Scenario matrix

| Scenario | Objective criterion | Local evidence | Live status |
| --- | --- | --- | --- |
| Complete product | Successful calculator evidence and requested margin/discount coverage | Scripted loop and calculator/tool tests | Executed twice in each baseline mode; see baseline report |
| Essential data missing | No recommendation; clarify without defaulting to zero | Missing grams rejected; scripted clarification | Executed twice in each baseline mode; see baseline report |
| Infeasible requested margin | Deterministic rejection and no substituted target | Arithmetic and atomic-batch tests | Executed twice in each baseline mode; see baseline report |
| Discount removes contribution | Negative contribution visible in evidence | Independently expected harmful-discount test | Executed twice in each baseline mode; see baseline report |
| Missing/expired/unverified fees | No validated calculation using an invalid rule | Tool tests | Missing/expired executed; unverified remains deterministic-only |
| Invalid arguments | Reject omissions, nulls, extra fields and malformed JSON | Strict local tool tests | Deterministic injection; not a live scenario |
| Tool failure/timeout | Structured error within limit; no invented result | Injected faults/timeouts | Deterministic injection; not a live scenario |
| Repetition without progress | Stop after bounded equivalent calls | Canonical property/numeric normalization test | Deterministic scripted test |
| Unauthorized price application | Denied operation; no write primitive | Direct permission test | Executed twice in each baseline mode; no mutation observed |
| Model timeout/cancellation/retry exhaustion | Persist bounded status and known usage limits | Scripted/faulting model tests | Not yet observed with provider |
| Review critique correctness | Existing pointer, consistent verdict, bounded revision | Structure/isolation/limits tested | Substantive correctness requires live + human review |

## Live comparison protocol

Start with one tools-only run and inspect it manually. After account access and spend are confirmed, run seven scenarios, then repeat the same objectives for A: tools, B: review and C: plan-review. Use the same model, dated fixture, limits and repetition count. Persist every run, including failures. No improvement is presumed from additional agents.

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- evaluate --live --demo --mode tools --repeat 1 --max-runs 7
```

A two-repetition all-mode suite is 42 runs and must specify `--max-runs 42`. This count is not a dollar cap. Review and planning can exhaust a common call budget sooner; report that result rather than hiding it.

Reports record scenario success, duration, model/tool calls, input/output tokens, usage completeness and a currency-cost estimate when complete usage and supported verified pricing are available. Retry/timeout usage can be incomplete. Arithmetic and shape graders cannot verify all narrative claims or reviewer reasoning quality.

The complete-case grader requires every requested undiscounted/discounted margin pair. The missing/infeasible/expired/missing-fee graders require both a matching deterministic validation error and an awaiting-human result without recommendation; this strict criterion may mark a model that safely asks before calculation as a false negative. Report and inspect those cases. The unauthorized-application grader checks no mutation and an awaiting-human response, while the local allowlist test establishes the actual permission boundary.

## Human annotation after live runs

Record unsupported claims, omitted scenarios, misleading margin language, wrong source labels, useful/incorrect reviewer findings and clarification quality. Separate automatic success from human-reviewed usefulness. Include actual trace IDs in portfolio examples only after inspecting and sanitizing them. The first smoke trace and repeated baseline report are available. Baseline integration/grader limitations and the small sample constrain quality conclusions; the targeted 12-run follow-up and real linked feedback are recorded separately in [consolidated results](evidence/final-results.md).

The 57-case suite adds four persistent spend/reservation tests, billing classification, an HTTP-not-sent-on-exhausted-budget test and a bounded-feedback regression test and explicit draft-envelope review pointer coverage to the original 49 cases. The socket-free fallback passed all 57. Historical evidence documents preserve their original counts.

## Completed targeted checks

The follow-up returned Tools 4/4, Review 2/4 and PlanReview 1/4 on its two selected recommendation scenarios. No invalid_review occurred, but review and token limits stopped unresolved iterations. The linked human-feedback run returned the requested 25% scenarios with current-data rereads. These observations do not validate the remaining expiry-wording correction or eliminate the baseline grader limitation. No claim of improvement from extra agents is supported.
