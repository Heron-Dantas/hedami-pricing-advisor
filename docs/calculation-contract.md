# Calculation contract — first increment

All amounts are BRL. All arithmetic uses C# decimal. Rates are fractions in [0, 1). Required cost fields are nullable at the input boundary: null means unknown and is rejected; explicit zero is accepted. Negative inputs are rejected. Overflow propagates as an error; the tool boundary translates validation/overflow exceptions into structured failures.

## Production costs

- Material = total grams × BRL/kg / 1000. Total grams must include supports and per-attempt waste where applicable; do not add them twice.
- Energy = average power in kW × print hours × BRL/kWh.
- Printer = print hours × (depreciation BRL/hour + maintenance BRL/hour). These components exclude energy, material and labor. Derivation from purchase price, useful hours and maintenance history is pending confirmed business data.
- Print attempt = material + energy + printer.
- Failure provision = print attempt × failure probability / (1 − failure probability). This models independent identical full-print attempts until success. It is not simply a surcharge of the same percentage. It assumes manual preparation/finishing/packing occur once per saleable item and packaging is not wasted on failures. Partial failures, repeated setup labor, correlated failures and batch production need an extended model.
- Labor = (preparation hours + finishing hours + packing hours) × BRL/hour.
- Total production cost = print attempt + failure provision + labor + packaging.

No intermediate rounding is performed. Components displayed to fewer digits may not sum exactly to the displayed total.

## Price scenarios

Let C be production cost, F the fixed channel fee per sale, r the sum of charges assessed on the actual discounted revenue, and m the requested contribution margin on revenue:

`list price = ceil_to_cent((C + F) / (1 − r − m))`

Reject nonpositive C and a nonpositive denominator. Ceiling protects the undiscounted target margin. The discount is applied to that list price, rather than increasing list price to compensate for it:

`charged price = round_to_cent(list price × (1 − discount))`

Round charged price with MidpointRounding.AwayFromZero; reject a zero charged price. Compute unrounded analytical contribution as `charged price × (1 − r) − F − C`, and revenue margin as `contribution / charged price`. Markup on production cost is `(charged price − C) / C`; it excludes selling charges and is not margin.

Percentage channel fees, taxes and advertising may be combined in r only if they share this revenue basis. This increment does not model alternative tax bases, fee caps, minimum fees, per-component settlement rounding or advertising per order. The tool layer preserves separate channel/tax/advertising rates, fixed fee and provenance in each scenario record; it sums percentage rates only under this documented common-basis assumption.

These are contribution estimates, not net profit: overhead and other business expenses may be absent. The raw calculator does not certify rule validity. The tool increment now validates rule metadata, effective dates, confirmation status and missing rates before calculation; failures are returned to the model for recovery/clarification. No Shopee rules are included.
