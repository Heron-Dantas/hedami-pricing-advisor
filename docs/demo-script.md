# Demonstration script

## Preparation

Build the solution, verify the local calculator and tests, configure an application API key locally and run `doctor`. Confirm billing/model access with one small real run. Inspect every trace before sharing it. Do not present a scripted test as live evidence. Use the archived first real run and linked-feedback trace as inspected fallbacks. The consolidated results and original failure traces are available; do not rerun all comparisons live during the interview.

## Five-minute walkthrough

1. Explain the real HEDAMI product-registration problem and why pricing is the first slice. State that photos and publishing are outside scope.
2. Run `calculator` to show the deterministic financial core. Point out units, contribution versus net profit, margin versus markup, rounding and the full-print retry assumption.
3. Run `analyze --demo --mode tools`. Show product/rule retrieval, calculator tool results and a draft with undiscounted/discounted cases. Open the saved JSON trace and identify sources, dates, model, instruction version, observable tool sequence and usage.
4. Run the missing-material request from the README. Show a specific clarification and no invented price. Explain how a human can correct a local authoritative dataset, then rerun with `--feedback-from` and `--feedback`.
5. Run the same complete request in `review` mode. Show the reviewer's separate context design and structured verdict. If it finds an issue, show its evidence pointer and bounded revision; if it approves immediately, report that honestly.
6. Run `plan-review` if latency/time allows. Show an `update_plan` event, its chosen next tool and the enforced retrieval/calculation preconditions. Explain that extra model calls are a cost to measure, not an automatic benefit.
7. Summarize actual evaluation results only. Open the learning log, limitations and implemented/evaluated/planned status table. Finish with next product decisions, not a production claim.

## Demonstration failure handling

A missing key or provider error is a configuration issue, not successful agent behavior. A timeout or budget stop is a valid harness outcome, not a completed recommendation. If no real API run is available, demonstrate the calculator and scripted tests and explicitly state that live evidence remains pending.

## Interview discussion prompts

- Why is a deterministic tool the authority for money?
- Why distinguish unknown values from explicit zero?
- Which checks are enforced outside the prompt?
- How does the selected plan change execution?
- What can an independent reviewer verify, and what can it still get wrong?
- How will tools-only and reviewed modes be compared fairly?
- Which business inputs would change the product recommendation most?
- What did Heron decide, what did Codex implement, and what did Heron review?

## Recommended presentation choice after evaluation

Lead with tools-only, the [first real trace](evidence/2026-10-24-first-real-run.md), then the [feedback example](evidence/final-results.md). Show one useful review and one bounded failure as engineering lessons rather than positioning review as a proven improvement. Present original automatic scores with their grader caveats, plus the smaller follow-up separately. Do not hide failed runs or call progress percentages model-accuracy metrics.
