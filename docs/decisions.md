# Decision log

Entries preserve the state when each increment was made. D005 supersedes the provider/framework deferral in D002; D006–D008 record implemented tools, review/planning and runtime loading that were pending in D004.

## D001 — Use installed .NET 10 and a console entry point (2026-10-24)

Observed environment: macOS 26.5 ARM64, Git 2.50.1, .NET SDK 10.0.302, runtime 10.0.10. Directory inspection found no files and no Git repository. No Git name/email was returned by configuration queries.

Pin SDK 10.0.302 with latest-patch roll-forward. Use a domain library, console and xUnit tests. This supports executable increments and separates financial rules without a web application or speculative service layers. [Official .NET versioning](https://learn.microsoft.com/en-us/dotnet/core/versions/) was consulted.

## D002 — Defer agent framework and provider selection (2026-10-24)

No agent package is installed. A small explicit loop may make permission enforcement and budgets easier to demonstrate, at the cost of implementing lifecycle controls ourselves. Microsoft Agent Framework supplies sessions, tools, middleware and workflows; those capabilities could reduce custom plumbing but introduce framework lifecycle/API learning. Semantic Kernel remains a candidate only if a concrete requirement justifies it. Do not choose by familiarity alone.

The [official Agent Framework overview](https://learn.microsoft.com/en-us/agent-framework/overview/) was consulted. Package/API selection and a hands-on comparison remain pending provider confirmation. No model integration or real LLM evidence exists yet.

## D003 — First calculation scope (2026-10-24)

Use synthetic BRL inputs and raw scenario charges, not marketplace rules. Failure provision assumes independent full-print retries, with no repeated manual labor or packaging. Document this limitation instead of silently generalizing it. Printer use contains depreciation and maintenance only; energy and labor are separate.

## D004 — Incremental evidence (2026-10-24)

Arithmetic tests do not evaluate agent behavior. Test infrastructure uses the installed xUnit template; exact package versions live in its project file. Runtime skills, tool metadata, persistent runs and real provider evaluations are future increments. A SKILL.md alone is not runtime functionality.

## Contribution record

- Heron supplied the business problem, product scope, architectural constraints, evaluation criteria and delivery priorities.
- Codex inspected the environment and authored the initial calculator, console scenarios, tests and project documents. Heron's code review and business-data confirmation remain pending.
- Local Git identity has not been configured: do not infer email or commit identity from the OS account or stated name alone.

## D005 — OpenAI API selected; explicit Responses REST loop (2026-10-24)

Heron confirmed OpenAI API, an existing account and ability to create a local key. He requested immediate continuation, without waiting for milestone dates. The key and evaluation-spend decision are still pending. Codex authentication is not reused.

Use BCL HttpClient and System.Text.Json rather than an agent framework or provider package in this increment. The [official function-calling guide](https://developers.openai.com/api/docs/guides/function-calling) was consulted for function schemas, call/result replay, strict mode and disabling parallel calls. The adapter owns parsing, cancellation and sanitized failures; the harness owns retries. This avoids hidden retry/state behavior but requires us to maintain protocol compatibility and resilience tests. No claim is made that this is generally superior to an SDK/framework.

Initial configurable snapshot: `gpt-5.4-mini-2026-03-17`, supported by the [official model page](https://developers.openai.com/api/docs/models/gpt-5.4-mini). It supports Responses, function calling, structured output and reasoning effort none. This is an initial narrow-task candidate, not a benchmark winner or confirmed account entitlement. Current listed rates were inspected, but currency cost calculation is intentionally not implemented; traces report observed usage and leave USD cost null.

## D006 — Typed tools use authoritative catalog inputs (2026-10-24)

Model calculation arguments contain product/rule IDs, target margins and discount only. The tool retrieves stored costs, verifies provenance/dates and records separate revenue charges in scenario evidence. Strict local schema validation rejects missing numerics and extra fields, independently of the model's remote strict schema. No marketplace write primitive exists.

## D007 — Bounded context-separated review and enforced planning (2026-10-24)

Implement three selectable modes with one provider/model: tools; review; plan-review. A reviewer gets a fresh objective/draft context, no analyst history and only a structured review tool. Require existing evidence pointers and verdict consistency; correctness still requires evaluation. Permit one analyst revision by default, separate from one total technical retry.

Planning selects the next action and an observable objective; the gate consumes that selection and checks dependencies. Errors require replanning; missing data can end with clarification. This is adaptive next-action planning, not a general planning framework. No quality improvement is claimed before real comparisons.

## D008 — Runtime procedures are explicitly loaded (2026-10-24)

The application reads exactly two allowlisted Markdown files in `runtime-skills`, bounds their size and records a SHA-256 content hash. These are application procedures, not Codex development skills and not implicit SKILL.md discovery. General discovery, MCP and plugin execution are out of scope. The OpenAI Docs development skill was used to consult official APIs; it is not loaded by the application.

## D009 — Local reproducible verification under sandbox restrictions (2026-10-24)

Preserve standard xUnit tests and add a small in-process verification executable referencing compiled test DLLs. It supports the current Fact/Theory/InlineData suite, including async tasks; it is not a replacement xUnit framework and cannot claim VSTest success. Standard VSTest remains a local-terminal check because session sockets are blocked. `.git` writes remain prohibited; no identity is inferred and no commit is claimed.

## Contribution update

Heron chose OpenAI API and requested immediate continuation. Codex implemented the application increment and authored the architecture, demo script, portfolio draft, live-evaluation runner and learning log. Heron's code review, business-data confirmation and real-model evaluation remain explicit next steps.

## D010 — Synthetic data and USD 2 total authorization (2026-10-24)

Heron configured the local .env key and authorized a maximum USD 2 for application calls, retaining fictional business data. The first real request attempt (`0b1658d005a642bf8ce48ad663d826fa`) stopped with network_error. Independent DNS resolution failed for api.openai.com. No response was received; this does not establish account balance, billing status or model availability. The user-terminal smoke test is prepared in `scripts/validate-live.sh`; live evidence is still pending.

## D011 — Reserve estimated spend before HTTP transmission (2026-10-24)

Use a persistent, exclusive-lock ledger shared across CLI commands. Price whole serialized request bytes plus a protocol allowance conservatively as uncached input, reserve maximum output, and settle from complete reported usage. Failed/unknown calls retain reservations. Reject unsupported pricing models and concurrent ledger writers; do not silently reset spend. Verified GPT-5.4 Mini standard text rates are documented in code and README with official source/date. These estimates do not control other account traffic or guarantee provider invoice totals.

Allowlist billing error codes and avoid retrying credit/spend/quota failures. Keep arbitrary provider messages and credential values out of logs. This follows the [official error guidance](https://developers.openai.com/api/docs/guides/error-codes). DNS failures get a distinct non-retryable local status.

## D012 — Bound linked human-feedback context (2026-10-24)

Code review found full prior recommendations could exceed the objective limit when embedded in a feedback request. Preserve the original objective, concise prior scenario evidence and new feedback separately; do not recursively embed whole run histories. Retrieve authoritative inputs again. A regression test uses a six-scenario draft larger than the objective limit. The real human-feedback walkthrough remains pending model connectivity.

## D013 — First real provider evidence obtained through host terminal (2026-10-24)

Heron ran the prepared validation script in his terminal. Run `fc0add6ccb984f7186c774372e69a938` reached draft_ready with four model/tool calls, complete usage and a USD 0.0058245 estimate. Codex inspected tool order, calculator evidence, summary/assumptions/limitations and no-price-application flags. A credential-pattern-checked copy of the synthetic-data trace is preserved without reserializing decimal numbers. This confirms one tool-use example, not repeated reliability or benefit from review/planning.

## D014 — Preserve failed baseline and keep tools-only as default (2026-10-24)

The 42-run baseline had original automatic results of 11/14 tools, 8/14 review and 6/14 plan-review. Investigation separated strict-grader false negatives, problematic expiry wording and invalid review responses. The original results/traces and prompts are preserved; no failures are silently overwritten. All 48 calculator rows matched independent Decimal checks.

Tools-only remains the default because this sample did not demonstrate value from the additional roles. Reviewer diagnostics and pointer compatibility are improved and regression-tested; a smaller follow-up is distinct from the baseline and cannot establish general superiority. Review quality, not arithmetic alone, remains an experimental concern.

## D015 — Freeze paid evaluation and move to owner understanding/rehearsal (2026-10-24)

The 12-run targeted follow-up completed with 4/4 tools, 2/4 review and 1/4 plan-review automatic passes. No structurally invalid review occurred, but iteration/token budgets still stopped several workflows. One useful critique and several overreaching critiques are visible in the preserved traces. A real linked feedback run successfully replaced targets with 25%. Consolidated estimated spend is USD 0.44420565 from 280 settled request records, below the authorized USD 2.

No further paid calls are needed for the current portfolio evidence pack. Keep tools-only as default and label review/planning experimental. Preserve all original failures, distinguish the strict grader from semantic usefulness, and move to Heron's guided code review and demo rehearsal. This completes the executed console slice, not production hardening or Heron's independent understanding.
