# HEDAMI Pricing Advisor — portfolio narrative

## Product problem and value hypothesis

HEDAMI 3D Labs spends manual effort preparing product listings, including pricing, images and publication. Heron Dantas selected pricing as a narrow, measurable first slice: retrieve costs, compare requested revenue-margin targets and determine whether a discount preserves them. The value hypothesis is reduced manual analysis time with inspectable assumptions. No time-saving metric has been measured yet.

This project supports a Principal Digital Product Owner interview by making product scope, engineering controls and agent evaluation visible. It is a local prototype, not a production service or Volvo Group system.

## Product boundary

The application recommends and drafts. It cannot update a marketplace or publish a listing. Photos, catalog administration, web UI and MCP are future options. This boundary lets the prototype demonstrate agent decision-making without granting commercial side effects.

## Engineering approach

A decimal C# calculator owns arithmetic, explicit units and rounding. An OpenAI Responses adapter handles model/tool protocol. Typed local tools validate products, rule provenance, effective dates and financial inputs. A small explicit harness controls the loop, budgets, context, cancellation, retries, review iterations and persisted observable events.

The console can operate as a tools-only analyst, a reviewed analyst, or an analyst with enforced next-action planning and independent review. The reviewer gets a fresh context, returns structured critiques with evidence pointers, and cannot alter prices or calculator evidence. Runtime procedure loading is explicitly implemented; development skills do not automatically become application functionality.

See [architecture](architecture.md), [calculation contract](calculation-contract.md) and [decisions](decisions.md) for concrete design choices and trade-offs.

## Status: implementation is distinct from evaluation

| Capability | Implemented | Evidence/limitation |
| --- | --- | --- |
| Decimal costs and scenarios | Yes | Deterministic assertions and executed console demo |
| Product/rule tools and provenance checks | Yes | Local tests; synthetic data only |
| OpenAI Responses integration | Yes | Mocked HTTP/protocol tests plus one real tools-only run from Heron's terminal |
| Bounded loop and persisted traces | Yes | Scripted model tests and local persistence tests |
| Separate-context reviewer | Yes | Real approvals and rejected reviews observed; baseline integration/quality limits documented |
| Enforced next-action planning | Yes | Real plan/tool sequences observed; higher baseline calls/usage and lower automatic success |
| Human feedback and linked reruns | Yes | Real linked feedback run changed target to 25% and recalculated; evidence archived |
| Runtime procedure loading | Yes | Explicit allowlisted Markdown loader with content hash; no general plugin system |
| Comparative live evaluations | Runner implemented | 42-run baseline and 12-run follow-up archived, including failures and inspection |
| Marketplace writes, UI, MCP | No | Outside current delivery scope |

## Evaluation design

Calculator tests specify expected values independently. Scripted model tests exercise deterministic harness behavior under missing data, invalid arguments, repetition, faults, cancellation and review limits. Mocked HTTP tests cover the wire contract and sanitized errors. They establish engineering behavior, not LLM reliability.

Seven live scenarios are available for repeated A/B/C comparisons. Reports record scenario success, duration, calls and reported tokens, with estimated currency cost when complete usage and supported verified pricing are available; unknown consumption remains flagged. Structural/numeric graders cannot judge all prose claims or whether a review critique is substantively correct. Add human annotations before drawing quality conclusions. One real smoke run completed in 7.745 seconds with four model calls, 5,258 input tokens, 418 output tokens and USD 0.0058245 estimated cost. The model correctly explained the discount's margin reduction in the synthetic example. This single run is not a reliability rate. The subsequent 42-run baseline returned original automatic passes of 11/14 (tools), 8/14 (review), and 6/14 (plan-review), with documented metric false negatives and invalid reviews. Extra agents did not demonstrate a benefit in this sample. See [baseline](evidence/baseline-results.md). See [trace and inspection](evidence/2026-10-24-first-real-run.md).

## Current learning

Visible controls matter more than decorative agent labels. Decimal arithmetic does not by itself resolve incomplete business assumptions. Traceable review pointers do not guarantee sound criticism. More roles add calls and latency and require measured benefit. Read the [learning log](learning-log.md) for observed engineering facts versus hypotheses awaiting real runs.

## Contribution and disclosure

Heron defined the business problem, scope, architecture constraints, financial expectations, acceptance scenarios and deadline, then selected OpenAI API and requested immediate continuation. Codex authored the initial implementation, tests and documents and executed local verification. Heron's code review, confirmed business inputs and further real-model evaluation remain pending. Heron configured the key locally and executed the first successful provider run. No professional/client data, credential, unpublished Volvo work or invented deployment result is presented as evidence.

## Next human review and presentation steps

The real baseline, targeted follow-up and linked feedback workflow are recorded in the [consolidated results](evidence/final-results.md). Total estimated application spend is USD 0.44420565 under the authorized USD 2. No further paid runs are planned for this delivery. Heron should inspect the evidence, review code/decisions and rehearse the demonstration. Keep fictional costs/rules explicitly labeled. A local Git checkpoint remains pending the host environment and confirmed identity. PDF/slides are optional representations of the existing English narrative, not unimplemented product capabilities.
