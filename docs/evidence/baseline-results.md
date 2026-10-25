# Baseline: tools versus review versus planning/review

Source: [original report](baseline/evaluations-20261024T043445508.json), [all results](baseline/comparison-summary.md), [inspection notes](baseline/inspection-notes.md) and [42 original traces](baseline/runs/). All business inputs are synthetic. The same pinned model, seven objectives and two repetitions per mode were used. Run-level limits were the same. These are small-sample observations, not production reliability estimates.

| Mode | Original automatic passes | Median duration (seconds) | Model calls | Tool calls | Reported tokens | Estimated USD |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Tools | 11/14 | 3.850 | 47 | 47 | 60,636 | 0.051805 |
| Review | 8/14 | 3.942 | 50 | 46 | 70,416 | 0.058165 |
| PlanReview | 6/14 | 6.185 | 83 | 79 | 135,972 | 0.124319 |

Median duration includes clarification scenarios that finish without a recommendation/reviewer pass. Currency estimates use complete reported usage and documented standard rates; they are not invoice totals. The per-run IDs and original unrounded cost values are in the report.

## What the numbers establish

In this sample the additional review/planning roles did not demonstrate improved end-to-end automatic success and added calls/usage. Tools remains the default demonstration mode. This does not prove that review is universally ineffective; integration failures and grader defects affect these results.

Eight of seventeen automatic failures were safe clarifications on missing material/fees that the strict grader rejected because no invalid calculation was attempted. Four expired-rule failures stopped safely but included undesirable offers to bypass expiry. Five failures had an invalid structured review. Do not conflate these categories or silently reclassify the original report as all passing.

All 48 successful calculator rows in the traces were independently checked using Decimal against the documented synthetic formulas and matched. No trace applied a price. These facts establish financial consistency and the observed recommendation-only boundary; they do not establish the semantic correctness of every narrative/reviewer statement.

## Changes and follow-up scope

The original prompts are archived in [baseline instructions](baseline-instructions/). Follow-up adds explicit reviewer pointer-root examples and /draft-prefix support, persists review output/rejection diagnostics, and instructs the analyst not to offer expiry bypasses or a nonexistent highest feasible margin. Local assertions passed 57 cases after these changes. New instruction hashes identify the changed runs.

A targeted two-scenario repeated A/B/C suite and linked human-feedback run are requested separately. The original 42-run results remain unchanged. The follow-up is smaller and changes multiple implementation/prompt details, so it cannot alone establish a causal quality improvement. Expired-rule wording corrections also need a dedicated future observation; they must not be called live-validated by a recommendation-only follow-up.
