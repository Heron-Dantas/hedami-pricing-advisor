# Recorded live comparison

Source: `evaluations-20261024T043908058.json`. Model: `gpt-5.4-mini-2026-03-17`. As-of: `2026-10-24`.

Automatic success is a structural/numeric grader result, not a human usefulness score. Cost is a rate-based estimate; unknown usage stays unknown.

| Mode | Automatic passes | Median seconds | Model calls | Tool calls | Reported tokens | Estimated USD |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Tools | 4/4 | 4.263 | 16 | 16 | 28358 | 0.025516 |
| Review | 2/4 | 9.325 | 26 | 19 | 65969 | 0.058973 |
| PlanReview | 1/4 | 14.020 | 46 | 39 | 139299 | 0.107119 |

## Individual results

| Scenario | Mode | Repetition | Automatic result | Status | Run ID |
| --- | --- | ---: | --- | --- | --- |
| complete | Tools | 1 | PASS | draft_ready | 414392c385d3415288246fd85cbb092e |
| complete | Tools | 2 | PASS | draft_ready | fa9ada9521fe457ea0ebdcec478408f2 |
| harmful-discount | Tools | 1 | PASS | draft_ready | 2f69820a1d37424cbe3344255949036b |
| harmful-discount | Tools | 2 | PASS | draft_ready | 3d1c4902fd5f4af18270846247410600 |
| complete | Review | 1 | PASS | reviewed_draft_ready | bdf4e44aca2840b0b9a15b082829bc6b |
| complete | Review | 2 | FAIL | review_limit | bd6f3dbb645d4e14871bd8d51f3ea8f1 |
| harmful-discount | Review | 1 | PASS | reviewed_draft_ready | 7da5ec9a812b41a982db45350effca87 |
| harmful-discount | Review | 2 | FAIL | review_limit | ae2d8b3fe83c44beb490c5adc32cc6f1 |
| complete | PlanReview | 1 | FAIL | token_limit | bc9d4e5999a240aba5d16f4e96c9d88e |
| complete | PlanReview | 2 | FAIL | token_limit | c8d0139973e3460d9ef14eaad6751354 |
| harmful-discount | PlanReview | 1 | PASS | reviewed_draft_ready | 827e2e16f6174dbdb90eddded92be096 |
| harmful-discount | PlanReview | 2 | FAIL | review_limit | c975bd54a368438095f5c0a4106336e2 |

## Interpretation boundary

Inspect failed and successful traces before concluding that extra agents help. Safe clarification before a calculation error can fail the current strict grader. Review verdicts are model outputs, not proof of correctness. Small repeated samples do not establish production reliability.
