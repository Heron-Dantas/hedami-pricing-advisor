# HEDAMI Pricing Advisor

An inspectable .NET portfolio prototype for 3D-print pricing. A deterministic C# calculator owns all money arithmetic. An analyst can discover local products/rules, call tools, request missing information and submit a draft. Optional separate-context review and enforced adaptive planning are available.

**Status:** implemented locally and verified with scripted model/HTTP tests. A real OpenAI tools-only example and a 42-run A/B/C baseline have been recorded and inspected; the targeted follow-up and linked human-feedback run are also recorded. See [consolidated results](docs/evidence/final-results.md). See [baseline results](docs/evidence/baseline-results.md). See [first real run](docs/evidence/2026-10-24-first-real-run.md). This is not production-ready. There is no marketplace integration or price mutation capability.

## Requirements and quick start

Use .NET SDK 10.0.302 (see `global.json`) and run commands from this repository root. NuGet access is needed on the first test dependency restore.

```sh
dotnet restore
dotnet build -m:1 -p:UseSharedCompilation=false -nodeReuse:false
dotnet test --no-build -m:1
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- calculator
```

The calculator needs no credential. Data is synthetic BRL, not confirmed business costs or current Shopee rules. For production-cost formulas, units, rounding and failure assumptions see the [calculation contract](docs/calculation-contract.md).

In the current restricted session VSTest cannot bind local sockets. Build the solution first, then run the reproducible fallback using the **same xUnit assertions**, including async tests and InlineData cases:

```sh
dotnet run --project tests/Hedami.PricingAdvisor.Verification -m:1 -p:UseSharedCompilation=false -nodeReuse:false
```

The fallback is not a general-purpose xUnit runner: it supports the current Fact/Theory/InlineData tests and does not implement fixtures, skips or parallel execution. Rerun standard VSTest in a terminal that permits sockets. Current fallback suite: 57 local cases passed. See [verification history](docs/evidence/2026-10-24-budget-and-feedback.md) and [baseline/follow-up notes](docs/evidence/baseline-results.md).

## Connect OpenAI

The application uses its own OpenAI API credential, independent of Codex authentication. Copy `.env.example` to `.env` if the local file does not exist, then fill `OPENAI_API_KEY` **in your editor**. Never paste it into chat. `.env` is ignored; shell environment values take precedence. The loader handles plain KEY=value and simple quoted values, without shell expansion.

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- doctor
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- analyze --demo --mode tools
```

`doctor` only reports whether a credential is configured; it makes no API request and does not validate account/model access. `analyze` makes paid API calls. Default model: `gpt-5.4-mini-2026-03-17`; override with `OPENAI_MODEL`. The adapter uses Responses API, strict function schemas, `store=false`, no parallel tool calls and reasoning effort `none`. Alternative models must support these request settings.

Compare modes with the same objective:

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- analyze --demo --mode review
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- analyze --demo --mode plan-review
```

Modes: `tools` (analyst only), `review` (analyst plus isolated reviewer), `plan-review` (enforced next-action plans plus reviewer). These are implemented, not proven improvements.

Use `--request "..."` for a different objective. Example missing-data flow:

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- analyze --demo --request "Analyze productId=missing-material using ruleId=demo-direct for 30% margin."
```

The human can edit their authoritative local dataset, then start a linked run with feedback:

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- analyze --demo --feedback-from RUN_ID --feedback "Use a 25% target margin and retain the 10% discount comparison."
```

Replace `RUN_ID` with the 32-character ID printed by the previous run. Feedback is passed as human input; it does not write business data or authorize price application. Each new run retrieves/recalculates from the current dataset. The parent objective/draft and parent ID are carried forward; hidden model reasoning is not.

## Data, budgets and traces

`data/demo.json` has complete and incomplete products, demonstration fee rules, and expired/missing/unverified rules. `--demo` explicitly permits demonstration provenance, while still enforcing dates. Demo fixtures are valid October 24–26, 2026. After that, `--as-of 2026-10-24` can reproduce historical examples; label them historical rather than current. Use `--data path/to/confirmed.json` for your own records; never retag demonstration values as confirmed without verification.

Defaults: 20 model calls, 20 tool calls, 40,000 reported tokens, 1,800 maximum output tokens per response, 180 seconds per run, 45 seconds per model call, 5 seconds per tool call, one total technical retry, one analyst revision, and at most two identical tool calls. CLI overrides: `--max-calls`, `--max-tools`, `--max-tokens`, `--max-revisions`, `--timeout`.

Token checks stop on reported usage; input tokens are only known after a response, so a call may cross the total-token threshold. A timeout/retry may consume unreported tokens. The CLI also reserves estimated spend before each HTTP request in `artifacts/api-spend.json`, shared across commands, with the authorized USD 2 total ceiling. It conservatively prices serialized request bytes plus a protocol allowance at the uncached rate and reserves maximum output; complete reported usage settles the reservation, including cached-input rates. Unknown/error consumption retains the full reservation. Unpriced models are blocked. Do not delete/reset this ledger to bypass the authorized ceiling.

Rates are estimates from the official GPT-5.4 Mini standard text pricing verified October 24, 2026 (USD 0.75 input / 0.075 cached input / 4.50 output per million tokens). This local control is conditional on those rates/model/request assumptions; it does not observe invoice adjustments or unrelated account usage. Use account-level controls for account-wide enforcement. Run currency estimates remain null when usage is incomplete or simulated.

Each run is saved atomically after observable events to `artifacts/runs/RUN_ID.json`. The terminal prints status, deterministic cost/charge/scenario evidence and the model's draft narrative. Only `draft_ready` or `reviewed_draft_ready` indicate a completed draft; review-limit/failed runs can retain a rejected draft for inspection. Every draft still requires human approval. No price is applied.

Traces contain business data, objectives, tool arguments/results, model IDs, instruction hash, schemas, durations, budgets, retries, reviews and reported token usage. They exclude credentials/HTTP bodies and do not persist private reasoning items. Known keys are redacted defensively, but traces are local sensitive artifacts and require inspection before portfolio publication. Artifacts are ignored by Git.

## Evaluations

The test suite separates arithmetic checks from scripted harness and mocked HTTP tests. Scripted model tests are **not real LLM evidence**. Seven live scenarios are implemented; they make paid calls only when explicitly invoked with `--live`:

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- evaluate --live --demo --mode tools --repeat 1 --max-runs 7
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- evaluate --live --demo --mode all --repeat 2 --max-runs 42
```

Heron authorized USD 2 total with synthetic data. Confirm a small real run before starting the larger comparison. Reports go to `artifacts/evaluations-*.json`; each underlying trace remains inspectable. Automatic graders verify structural/numeric criteria, not the full semantic quality of prose or reviewer findings. [Evaluation details](docs/evaluations.md).

## Architecture and portfolio

- [Project brief](docs/project-brief.md)
- [Architecture](docs/architecture.md)
- [Decision log and contribution record](docs/decisions.md)
- [Demo script](docs/demo-script.md)
- [Portfolio narrative](docs/portfolio.md)
- [Learning log](docs/learning-log.md)
- [Solution walkthrough](docs/solution-walkthrough.md)

Git initialization is blocked by this session's read-only `.git` policy. Initialize Git in your own terminal, confirm commit name/email and create a local checkpoint. Nothing has been published or deployed.

## Current network diagnostic and terminal handoff

The configured key is recognized locally. The first live attempt stopped with `network_error`; independent DNS resolution of `api.openai.com` failed in the restricted session. That attempt received no OpenAI response. A subsequent run from Heron's terminal completed successfully, confirming access for that request; it does not establish the remaining account balance. The adapter now reports DNS failures as `dns_resolution_failed` and distinguishes allowlisted billing/quota codes (without printing raw bodies) from transient rate limits. Billing errors are not automatically retried.

Run the small smoke test from your own terminal:

```sh
bash scripts/validate-live.sh
```

This builds, executes the local assertions and attempts one small real analysis with historical synthetic fixtures. It stops before comparisons by default. Inspect that run before using `bash scripts/validate-live.sh compare`, which attempts the two-repetition A/B/C suite under the same persistent USD 2 ceiling. Scenario failures are recorded honestly; this script is not evidence until actually executed successfully.

After comparisons finish, generate a local summary without network calls:

```sh
python3 scripts/summarize-evaluations.py
```

The source report and all underlying traces remain the evidence. Inspect both passes and failures before adding conclusions to the portfolio.

A targeted follow-up after the baseline corrections is available as `bash scripts/validate-followup.sh`: two recommendation scenarios, all three modes, two repetitions, then one linked human-feedback run. Filter custom live suites using `--scenarios complete,harmful-discount`. Budgets and original results are preserved.

## Delivery status

The recommendation-only console MVP is executed with a real model and documented. Tools-only is the default; reviewed/planned modes are evaluated experiments and were not shown to improve outcomes. See [consolidated evidence](docs/evidence/final-results.md). Remaining portfolio work is Heron's code/material review, demonstration rehearsal and local Git checkpoint; no additional paid runs are needed for the current evidence pack.

## Detailed Word study guide

Read the [revised technical guide in Word](docs/deliverables/HEDAMI-Pricing-Advisor-Technical-Guide.docx) for the complete product explanation, architecture diagrams, stack, annotated code, agentic loops, harness, runtime skills, creation process, evaluation evidence and proposed second phase. The [documentation package](docs/deliverables/README.md) preserves the initial outline, self-review and reproducible generation/validation artifacts.

For Heron's personal study, use the [Brazilian Portuguese Word guide](docs/deliverables/pt-BR/HEDAMI-Pricing-Advisor-Guia-Pessoal-pt-BR.docx). This study material is separate from a future final presentation; the English edition remains available.

For persistent local Git/network access in Codex CLI, see [the scoped access profile and activation steps](docs/codex-local-access.md). This is owner-activated configuration, not an automatic repository permission change.
