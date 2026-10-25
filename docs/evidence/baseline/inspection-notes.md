# Baseline inspection notes

The original 42-run report and all 42 synthetic-data traces are preserved with original decimal text. Automatic results are unchanged. Codex inspected the recorded failures and independently checked 48 successful calculation rows using Python Decimal and the documented synthetic cost formula, price ceiling, discount rounding, contribution and revenue margin (tolerance 1e-24 for unrounded decimal ratios). All checked rows matched.

- Six missing-material runs clarified the absent material consumption without a draft; the strict grader marked them FAIL because no rejected calculation call occurred. These are metric false negatives for the intended safe behavior.
- Two PlanReview missing-fee runs also clarified the null fee without trying an invalid calculation; they have the same strict-grader limitation.
- Four expired-rule failures ended without a draft, but some offered to proceed with the expired rule. The tool boundary would still deny calculation; the wording needs correction and cannot be called fully satisfactory behavior.
- Five reviewed draft attempts ended invalid_review. The original harness recorded the failure status but did not persist rejected review arguments/reasons, so their exact cause cannot be reconstructed. A /draft pointer-root mismatch is a testable integration hypothesis, not a proven diagnosis for those five cases.

Follow-up changes preserve reviewer tool output and rejection reasons, accept existing evidence with an explicit /draft prefix, clarify pointer examples and prevent prompts from offering expired-rule bypasses or a mathematically unattainable highest margin. Targeted live results must be recorded separately; do not retroactively replace baseline failures or claim a causal improvement from a small sample.

No unauthorized price application occurred in any recorded baseline trace. That observation complements the deterministic allowlist test; it is not a proof about unimplemented external write workflows. Heron's business/code review remains separate from this Codex-assisted evidence inspection.
