# Learning log

## Observed during local implementation

- A strict remote tool schema is not enough: local argument validation prevents omitted numerics from becoming default zero and denies additional cost-override fields.
- Percentage margin and markup are different quantities. A 10% discount may retain positive contribution while failing the originally requested margin. The report needs both undiscounted and discounted evidence.
- Full-print failure probability implies expected repeated-attempt cost `attempt / (1 − p)`, not an arbitrary p-percent surcharge. The model's assumptions exclude repeated setup labor and partial failures and need business confirmation.
- The model need not supply cost numbers to the calculator. Product and rule IDs let the application retrieve authoritative local inputs and preserve provenance.
- A reviewer can cite a nonexistent field. The application rejects those findings, but pointer existence alone does not establish correctness.
- Planning becomes an execution feature when the next action is enforced and preconditions prevent premature calculation/drafting.
- Model technical retries and analyst revisions need separate counters; otherwise a transport fault can consume the review budget or create misleading metrics.
- The restricted session permits compilation but blocks VSTest sockets and `.git` creation. A committed fallback driver runs the existing xUnit assertions in process; its results must not be described as a successful VSTest run.
- NuGet vulnerability metadata could not be fetched. Locally available packages allowed builds; the vulnerability check remains unverified.

## Hypotheses awaiting real-model evaluation

- Tool use will reduce arithmetic hallucinations, but narrative claims and scenario selection can still be wrong.
- Review may detect unsupported claims or missing requested scenarios; it may also produce unnecessary critiques, latency and extra calls.
- Enforced next-action plans may help recovery from missing data but may be too costly for simple pricing questions.
- A small pinned model may be sufficient for this narrow workflow. Account access, reliability and actual consumption remain unverified.

## Record after each real run

Link the run ID; state expected versus observed behavior; list deterministic errors and human-reviewed narrative/reviewer issues; record duration, calls and reported tokens; propose one targeted change. Separate engineering fixes from business-rule changes. Do not retrospectively label a successful demonstration as a measured productivity gain.

## Further observed engineering issues

- Configuring a key is different from verifying account access. A local DNS failure prevented a provider response; it says nothing about available credits. Billing/quota errors must be classified separately and not retried as transient traffic limits.
- A total-token stopping condition alone cannot guarantee an estimated spend ceiling. The CLI now reserves conservative request/output costs before sending and persists unknown consumption instead of assuming zero. Pricing/source assumptions and account-wide limits remain distinct.
- Replaying an entire recommendation into human feedback can exceed the objective bound. A concise typed evidence projection avoids repeating cost/provenance records and original histories while retaining an original objective.

## First real-model observation

Heron's terminal completed run `fc0add6ccb984f7186c774372e69a938`. The analyst used the four expected tools, retained the requested 30% target, called the calculator for a 10% discount comparison and correctly stated that 23.35% discounted margin falls below the target. The summary retained synthetic-data and contribution limitations. This confirms one real loop, not general model reliability. The difference between failed session DNS and a successful host-terminal run is an environment boundary, not evidence that adding credits fixed anything.

## Repeated real-run findings

- The original automatic score was sensitive to a test-path assumption: correctly asking for missing data before attempting an invalid calculation was counted as failure. Acceptance should eventually be redesigned around grounded outcomes, while keeping this original report unchanged.
- Five original reviews were structurally invalid and their contents were not logged, exposing an observability gap. The follow-up logged tool output and allowed the draft-envelope pointer prefix. No invalid reviews appeared in its small sample; this is not proof of the original cause.
- Some reviewers usefully requested an explicit negative-contribution conclusion. Others asked for redundant labels or immutable schema changes and continued raising new issues. A separate context and checkable pointers do not guarantee useful or actionable critique.
- Adaptive planning added model calls and context. Two follow-up runs hit the token budget; three reviewed/planned runs hit the review budget. The limits worked as stop conditions, but the added roles did not demonstrate enough benefit to replace tools-only as default.
- Human feedback is demonstrated: a real linked run replaced previous margin targets with 25%, reread the same synthetic data and returned both discount cases.

Full counts, estimates, trace IDs and scope limits are preserved in the [consolidated results](evidence/final-results.md). No productivity gain, production reliability rate or superiority of additional agents has been measured.
