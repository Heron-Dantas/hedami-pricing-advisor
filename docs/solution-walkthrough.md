# Understanding and defending the solution

## Start with the product decision

The user wants to decide whether a target selling margin and a discount make sense for a specific 3D-printed product. The first slice is deliberately local and recommendation-only. This reduces delivery scope while preserving the agent behaviors that matter: retrieve, calculate, detect missing facts, adapt, explain and accept feedback.

The LLM is useful for selecting actions and communicating trade-offs. It is not needed to multiply grams by a filament rate or solve the selling-price equation. That distinction drives every boundary below.

## Follow one real execution

Use [the first real run](evidence/2026-10-24-first-real-run.md) and its full JSON trace. Its four observable steps are:

1. `get_product` returns stored costs and provenance.
2. `get_channel_rule` returns separate channel/tax/advertising rates, fixed fee and dates.
3. `calculate_scenarios` loads those records again, validates them and calls the C# calculator. It returns both undiscounted and discounted evidence.
4. `submit_recommendation` references successful scenario IDs. The application assembles and rechecks a draft. It cannot apply a price.

This is a real model-to-tool loop. The model chose calls and received their results; a model-written paragraph about tools alone would not demonstrate that behavior.

## Read the code in this order

| File | Question it answers |
| --- | --- |
| `src/Hedami.PricingAdvisor.Domain/PricingCalculator.cs` | What exactly does the business calculation mean? |
| `docs/calculation-contract.md` | Which units, assumptions and rounding policies make the formula defensible? |
| `src/Hedami.PricingAdvisor.Application/Tools/PricingData.cs` | Where do inputs come from, and when may a rule be used? |
| `src/Hedami.PricingAdvisor.Application/Tools/ToolContracts.cs` | What can the model ask for and what typed evidence comes back? |
| `src/Hedami.PricingAdvisor.Application/Tools/PricingTools.cs` | How are arguments, permissions, dependencies and references enforced? |
| `src/Hedami.PricingAdvisor.Application/Models/OpenAiResponsesClient.cs` | How do model/tool calls cross the API boundary? |
| `src/Hedami.PricingAdvisor.Application/Harness/PricingHarness.cs` | Who owns the loop, state, budgets, failure outcomes and persistence? |
| `src/Hedami.PricingAdvisor.Application/Agents/PricingReview.cs` | What makes a review structurally checkable? |
| `runtime-skills/*.md` and `RuntimeInstructions.cs` | Which instructions are loaded, how and with what version? |
| `ApiSpendBudget.cs` | How are estimated costs reserved before transmission and recorded across runs? |
| `FeedbackContext.cs` | How does new human input preserve concise context and override prior targets? |
| Console and tests | How is the solution composed, run and evaluated? |

Folder boundaries do not imply separate deployment units. A console, domain library and application library are enough for this prototype. A database, service bus or cloud resource would need a concrete use case before being added.

## Explain the price equation

Let C be production cost, F a fixed selling fee, r a sum of charges on actual selling revenue, and m a target contribution margin on that revenue. Solve `P − rP − F − C = mP`, giving `P = (C + F) / (1 − r − m)`. The calculator rounds the list price upward to a cent so the undiscounted target is not missed by rounding.

The discount then reduces that list price. It does not magically preserve the target margin. In the actual synthetic example, BRL 44.45 becomes BRL 40.01 after 10% off, and margin becomes 23.35%, below the requested 30%. Positive contribution and preservation of a target margin are different conclusions.

The number is contribution after the modeled costs/charges, not net profit. Unmodeled overhead and alternative tax/fee bases can invalidate broader profit claims.

## Defend the control boundaries

**Prompt versus permission:** a prompt says no price application, but the stronger boundary is the absence of any write primitive from the executable tool allowlist. A model cannot gain that capability by inventing an `apply_price` call.

**Remote schema versus local validation:** strict tool schemas reduce malformed model output. C# still rejects missing, extra, negative, unknown or infeasible inputs before calculation. A remote provider setting is not a substitute for local contracts.

**Planner versus decorative checklist:** in `plan-review`, the selected next tool is consumed by the gate; retrieval is required before calculation and evidence before drafting. Error recovery therefore changes the next permitted action. This is a small next-action planner, not a universal autonomous scheduler.

**Reviewer versus arithmetic authority:** the reviewer sees a fresh objective/draft context and only a review tool. Existing JSON pointers make findings traceable. They do not prove that criticism is correct. Deterministic recalculation and human review remain necessary.

**Technical retry versus reflection:** a transport retry tries to obtain the same missing model response. A revision changes a draft after critique. Their counters and limits are deliberately separate.

**Runtime procedure versus development skill:** the application explicitly reads two trusted Markdown files and hashes their contents. It does not inherit Codex's skills or gain runtime behavior merely because a file is named SKILL.md.

## Evaluate claims before presenting them

A unit test passing proves its asserted local behavior for that case. A scripted model test proves the harness handles supplied responses. A real run proves one observed provider interaction. Repetitions show variability for the sampled conditions. None alone proves production reliability, business time savings or that more agents improve quality.

Use the comparison report to discuss trade-offs: success by scenario, useful/incorrect findings, extra calls, duration and estimated cost. Preserve counterexamples instead of selecting only successful demonstrations.

## Discussion topics for Heron

- Should a missing input stop immediately, or should an explicitly labeled sensitivity analysis be offered?
- Does the business really retry full print jobs, and which manual tasks repeat on failure?
- Which source/status workflow should qualify a marketplace rule as confirmed?
- Should the desired margin apply before or after an advertised discount?
- Which review findings change a decision enough to justify their latency/cost?
- What would justify a web UI, a framework migration, a database or MCP?
- What evidence would justify moving from drafts to a human-approved price-application workflow?

Heron's answers are product and architecture contributions to record; they should not be retroactively attributed to Codex or assumed without confirmation.
