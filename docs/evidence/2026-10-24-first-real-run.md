# First successful real model run — October 24, 2026

## Evidence

Run `fc0add6ccb984f7186c774372e69a938` completed in Heron's own terminal using the application's real OpenAI Responses adapter and synthetic product/rule data. The initial attempt from the restricted Codex session failed at DNS resolution; this successful terminal run is separate evidence.

The [full inspected synthetic-data trace](runs/fc0add6ccb984f7186c774372e69a938.json) is preserved without rewriting decimal values. It contains observable actions and structured evidence, not private chain of thought. It has been checked for credential patterns and contains no API key.

| Measure | Observed |
| --- | --- |
| Mode | Tools only |
| Status | draft_ready |
| Requested model | gpt-5.4-mini-2026-03-17 |
| Duration | 7,745 ms |
| Model calls | 4 |
| Tool calls | 4 |
| Input tokens | 5,258 |
| Output tokens | 418 |
| Estimated cost | USD 0.0058245 |
| Usage complete | Yes |
| Price applied | No |

Cost is an estimate from the documented standard text rates, not an invoice/balance statement. It counts only this run, not Codex development usage or other account activity.

## Observed behavior

The model selected get_product, get_channel_rule, calculate_scenarios and submit_recommendation in order. It used the stored IDs and requested a 30% revenue margin with/without a 10% discount. Both scenarios passed deterministic evidence verification.

The undiscounted list/charged price was BRL 44.45 with a 30.01% displayed contribution margin. After discount, the charged price was BRL 40.01 and margin 23.35%. The summary correctly stated that the discount does not retain the requested 30% margin. It identified synthetic source/validity, zero tax/advertising assumptions, common revenue basis, excluded overhead, full-print retry limitations and contribution versus net profit.

Codex inspected the numeric/source/limitation statements against the returned evidence. This is one successful real example, not a measured reliability rate or a substitute for Heron's review. Repeated comparisons, failure cases, review quality and human-feedback walkthrough remain pending.
