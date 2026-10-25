# 1. What this solution is

HEDAMI Pricing Advisor is a local, recommendation-only .NET console prototype for analyzing the economics of a 3D-printed product. A user states a pricing objective in natural language. A language-model analyst selects tools to retrieve stored product costs and channel rules, requests deterministic calculations, interprets the results, and submits a structured draft or asks the human for missing information. Optional modes add a separate-context reviewer and an enforced next-action planning gate.

The most important architectural sentence is: the model proposes actions and explains trade-offs; the harness controls execution; deterministic C# owns financial arithmetic. Understanding those three responsibilities is more useful than memorizing a framework name.

The real business is HEDAMI 3D Labs, owned by Heron Dantas. Product registration involves photographs, pricing and listing preparation. Pricing was selected as a small first slice that can be demonstrated and evaluated before photos, catalog administration and marketplace publication. The prototype is also an interview portfolio artifact for a Principal Digital Product Owner process. It demonstrates product scope, engineering judgment, agent development and learning from evidence.

The value hypothesis is that grounded conversational analysis can reduce manual pricing effort and expose assumptions clearly. No business time-saving study, sales uplift, real profit improvement or production reliability rate has been measured. All evaluated business inputs are deliberately fictional. They exercise the product without disclosing or fabricating confirmed HEDAMI costs.

## 1.1 A representative request and result

A request can ask the application to analyze the headset stand, calculate a 30% contribution margin on revenue, and explain the effect of a 10% discount. In the first inspected real run, synthetic production cost was approximately BRL 24.6667, a synthetic channel charge was 10% of actual revenue, and the fixed fee was BRL 2. The deterministic list price was BRL 44.45. With the discount, charged price was BRL 40.01 and realized contribution margin fell from approximately 30.01% to 23.35%.

The recommendation correctly concluded that contribution remained positive but the requested 30% margin was not preserved. This distinction is the product's core decision support: a discount can be affordable in the sense of avoiding a negative contribution while still failing the owner's target.

The draft also identified synthetic provenance, dates, explicit zero tax/advertising assumptions, missing overhead and limitations of the failure model. Nothing was applied to a marketplace. The recorded example proves one observed model/tool interaction, not universal correctness of future explanations.

## 1.2 Scope and maturity

| Area | Current state | What a reader must not infer |
| --- | --- | --- |
| Financial calculator | Implemented, locally tested and used by real workflows | All possible business costs and tax treatments are covered |
| Analyst tools and harness | Implemented and exercised locally and with OpenAI | Production-grade availability or security |
| Reviewer and planner | Implemented and experimentally evaluated | Additional agents are proven to improve quality |
| Human feedback | One linked real workflow demonstrated | A full catalog editing or approval service exists |
| Evidence and English portfolio material | Stored in the repository | Heron has already completed independent code review |
| Marketplace writes | No capability exists | A reviewer verdict can authorize a commercial action |
| Web UI, Azure deployment and MCP | Not implemented | Architecture diagrams describe deployed cloud resources |

No current Shopee rate schedule is included. Historical conversations about fees are not treated as current rules. The prototype can produce synthetic scenarios; it cannot certify a real marketplace's commercial policy.

# 2. How to read this document

For a first reading, follow chapters 1–7 to understand the product, vocabulary, architecture, stack and financial model. Then read chapters 8–14 to follow tools, the harness, agents, skills, the API and human feedback. Chapters 15–19 explain creation, actual evidence, limitations, operation and interview discussion. The final chapters provide references, the documentation improvement record and a proposed second phase.

For a code-focused reading, keep the repository beside the Word document. Source excerpts are extracted from actual files and labeled with their file and line range at generation time. They are small enough to explain a decision; they are not replacement source files. Paths are repository-relative, so the document remains useful after moving or cloning the repository. The code can change after this documentation snapshot; the source manifest appendix identifies the snapshot used here.

For an interview reading, focus on the decisions and observed counterexamples: financial authority outside the model, a permission allowlist outside the prompt, a separate reviewer context that still makes mistakes, planning that changes execution, and evaluation criteria that initially penalized correct early clarification.

## 2.1 Evidence categories used throughout

| Label | Meaning in this document |
| --- | --- |
| Implemented | A concrete code path or artifact exists in the inspected repository |
| Locally verified | Assertions ran against deterministic, scripted-model or mocked-HTTP inputs |
| Real observed | The actual OpenAI adapter received model responses in a recorded workflow |
| Codex-assisted inspection | Code or trace was inspected by the development assistant; not independent owner approval |
| Pending human review | Heron still needs to assess business meaning, code or presentation |
| Proposed phase two | An idea or acceptance gate; not a capability delivered by this prototype |

The two counts of 57 refer to different things: 57 local assertion cases and 57 real workflows. There are also 54 comparative workflow cases, two standalone smoke workflows, one linked feedback workflow, and 280 settled API request-usage entries. A workflow normally contains several API requests. Keeping these units separate prevents inflated claims.

# 3. Vocabulary and responsibility boundaries

An agent is a role that uses model decisions, instructions, context and available actions to pursue an objective across steps. In this application, the analyst and reviewer are distinct roles even when they use the same model and the same IModelClient implementation. Their separation comes from instructions, context and tool permissions, not separate servers or different model brands.

A model is the remote generative service used to produce a response. A tool is an executable application capability described to the model by a schema. A function call is a request generated by the model; it becomes a tool execution only after application checks. The model never directly runs arbitrary C# or shell commands.

The harness is the application-controlled execution environment around the model: the loop, state, validation, context, permissions, budgets, cancellation, failure handling and persistence. A prompt can advise the model, but enforcement must live in this code when a rule is important enough to guarantee locally.

A runtime skill in this project is a trusted reusable Markdown procedure explicitly loaded into the analyst or reviewer instruction field. The runtime has exactly two allowlisted files. There is no generic plugin engine or automatic SKILL.md discovery. Development skills used by Codex are a different mechanism and are not implicitly available to the application.

A plan is a selected next tool and a short observable objective. In plan-review mode, the next tool is checked and the selection consumed before execution. A review is a structured approve/revise verdict with evidence pointers and requested changes. Human feedback is new human input that starts a linked run with concise prior context. Neither a plan nor a review contains or requires private model chain of thought.

| Responsibility | Owner | Example |
| --- | --- | --- |
| Business objective | Human | Compare 30% margin and 10% discount |
| Next action selection | Analyst model | Retrieve the selected channel rule |
| Permissions and budgets | Harness/tool boundary | Reject apply_price; stop repeated calls |
| Financial arithmetic | Domain calculator | Price ceiling and realized margin |
| Source data and status | Human-maintained local catalog | Synthetic versus confirmed records |
| Draft criticism | Optional reviewer model | Missing negative-contribution statement |
| Commercial approval | Human; application workflow absent | No price application in this MVP |

# 4. Architecture: the smallest useful separation

{{FIG:architecture.png|Figure 1. Implemented component architecture. The model is external; catalog, calculator, harness and traces are local. There is no marketplace write path.}}

The production solution contains a console project, a domain library and an application library. Tests and the fallback verifier are separate supporting projects. These are code boundaries, not microservices. Everything application-owned runs in the same local process except the HTTP call to OpenAI. JSON files provide local data and traces; they are not a multi-user database.

The domain library does not depend on OpenAI, file I/O or the console. The application library depends on the domain and organizes tools, models, agents and harness responsibilities in folders. The console composes the objects, loads local settings, parses commands, runs evaluations and formats output. This gives Heron concrete boundaries to explain without creating a separate project for every class.

## 4.1 Component inventory

| Component | Concrete location | Responsibility |
| --- | --- | --- |
| Console composition | src/Hedami.PricingAdvisor.Console/Program.cs | Commands, data/configuration loading, dependencies and rendering |
| Financial domain | src/Hedami.PricingAdvisor.Domain/PricingCalculator.cs | Cost and scenario formulas using decimal |
| Catalog and provenance | Application/Tools/PricingData.cs | Product/rule lookup, status and date validation |
| Contracts and schemas | Application/Tools/ToolContracts.cs | Typed arguments/results and tool definitions |
| Tool execution | Application/Tools/PricingTools.cs | Allowlist, argument checks, calculations, evidence and draft state |
| Model boundary | Application/Models/IModelClient.cs | A small asynchronous provider contract |
| OpenAI adapter | Application/Models/OpenAiResponsesClient.cs | HTTP request, response parsing, usage and sanitized errors |
| Execution harness | Application/Harness/PricingHarness.cs | Repeated model/tool cycle, limits, review and final status |
| Trace store | Application/Harness/JsonRunStore.cs | Local JSON snapshot after observable events |
| Spend control | Application/Models/ApiSpendBudget.cs | Estimated-cost reservation and settlement across commands |
| Runtime instructions | Application/Agents/RuntimeInstructions.cs | Two trusted procedure files and version hash |
| Review validation | Application/Agents/PricingReview.cs | Verdict consistency and evidence-pointer checks |
| Feedback projection | Application/Harness/FeedbackContext.cs | Bounded parent context for a new run |

In this table, Application/... is shorthand for src/Hedami.PricingAdvisor.Application/... . No component called planner service, marketplace service or database repository has been created. The planner is behavior inside PricingTools and the analyst loop.

## 4.2 Why the direct harness was chosen

The workflow initially needs a handful of local tools, a model boundary and inspectable controls. An explicit loop makes those controls visible and easy to relate to interview evidence. It also avoids relying on framework defaults for retries, conversation state or approval behavior that Heron would otherwise have to learn and explain under a short deadline.

The trade-off is real maintenance ownership. The team must maintain protocol parsing, schema compatibility, context management, persistence, resilience and evaluation. A framework could reduce some plumbing if its lifecycle matches future requirements. Microsoft Agent Framework and Semantic Kernel were considered possibilities; neither is installed. The decision was not a general claim that frameworks are bad, and no framework benchmark was performed.

# 5. Stack and environment

The inspected development machine was macOS 26.5 on ARM64 with Git 2.50.1, .NET SDK 10.0.302 and runtime 10.0.10. The repository pins SDK 10.0.302 in global.json with latestPatch roll-forward. Projects target net10.0. The code uses C# as supported by the selected SDK; no explicit LangVersion is pinned in the project files.

| Layer | Technology actually used | Reason and trade-off |
| --- | --- | --- |
| Entry point | .NET console | Fast executable slice and repeatable terminal demonstration; less approachable than a business UI |
| Financial core | C# decimal and records | Explicit units/contracts and testable arithmetic; business assumptions still require validation |
| JSON | System.Text.Json / JsonNode | Built-in typed serialization plus protocol DOM handling; some schema checks remain handwritten |
| HTTP | HttpClient | Small visible REST adapter; protocol and retry maintenance are application responsibilities |
| Model API | OpenAI Responses, pinned configurable GPT-5.4 Mini snapshot | Real function-call workflows; access and behavior remain provider-dependent |
| Data and persistence | Local JSON files | Easy to inspect and archive; no cross-user transactions or authorization |
| Testing | xUnit and mocked/scripted boundaries | Independent local checks; not a substitute for real model behavior |
| Documentation generation | Python, python-docx, Pillow | Local Word packaging and technical figures; not part of the C# runtime |

The test project references xunit 2.9.3, xunit.runner.visualstudio 3.1.4, Microsoft.NET.Test.Sdk 17.14.1 and coverlet.collector 6.0.4. These are test dependencies. The presence of coverlet does not mean a coverage percentage was collected. The application itself does not reference an OpenAI SDK, agent framework package, cloud SDK or database package.

## 5.1 Configuration and credentials

LocalConfiguration reads OPENAI_API_KEY and OPENAI_MODEL from an ignored .env file when the corresponding shell variable is not already set. It supports simple KEY=value and quoted values, without shell expansion. It is a small parser, not a full dotenv specification. The prepared local file had permission mode 0600. Values are not printed.

The default model is gpt-5.4-mini-2026-03-17. OPENAI_MODEL makes the model configurable, but not all names are immediately usable: the request assumes supported tool settings and the spend ledger rejects models without a supported price definition. A configuration switch is not proof of provider portability.

Codex authentication is unrelated to the application's API key. No credential is embedded in source, archived traces or this document. The application key was created/configured by Heron locally. A successful doctor command only proves a non-empty local value is available; a real request is needed to verify access.

# 6. Financial model: units, assumptions and boundaries

The financial model operates in BRL, grams, hours, kW and BRL/kWh. Rates are fractions: 0.10 means 10%. The method signatures and property names encode units, reducing the risk of confusing filament per kg with filament per gram or watts with kilowatts. All required cost values are nullable at the input boundary: null means unknown, explicit zero means supplied zero.

{{SNIP:src/Hedami.PricingAdvisor.Domain/PricingCalculator.cs|public sealed record ProductionCosts|7}}

The Required helper rejects null and negative values. Rate rejects values below zero or at least one. A 100% failure probability is invalid because the expected number of attempts until success would not be finite. Rates are still validated in aggregate where necessary: individually valid percentage fees can sum to an unusable total.

Using decimal avoids the binary floating-point representation of common decimal monetary inputs. It does not solve every financial problem: repeated division still has finite precision, incomplete costs remain incomplete, settlement systems may round each fee differently, and an invalid tax basis still produces a wrong business model even with exact arithmetic.

## 6.1 Cost components

| Component | Formula | Important assumption |
| --- | --- | --- |
| Material | total grams × BRL/kg / 1000 | Grams include applicable supports and per-attempt waste once |
| Energy | average kW × print hours × BRL/kWh | Average power, not an unqualified peak wattage |
| Printer use | hours × (depreciation/hour + maintenance/hour) | Energy, labor and material excluded from these two rates |
| Print attempt | material + energy + printer use | Failure exposure is limited to these components |
| Failure provision | attempt × p / (1 − p) | Independent identical complete print attempts until success |
| Labor | preparation + finishing + packing hours, multiplied by hourly labor | These manual tasks occur once per saleable item in this model |
| Packaging | explicit BRL amount | Not wasted on failed prints under the current assumptions |
| Total production cost | attempt + failure provision + labor + packaging | Unmodeled overhead is excluded |

The printer-use composition is explicit to avoid charging energy again inside an opaque machine hourly rate. The code currently accepts depreciation/maintenance hourly rates as inputs; it does not derive them from equipment purchase cost, useful operating hours and maintenance history. That derivation is a possible future business-data improvement.

{{SNIP:src/Hedami.PricingAdvisor.Domain/PricingCalculator.cs|var material = grams|10}}

## 6.2 Why failure provision divides by success probability

Let A be the cost of one complete print attempt and p the failure probability. If attempts are independent and the process continues until success, expected attempts are 1/(1−p). Expected print-attempt cost is therefore A/(1−p). The extra provision above the first attempt is A×p/(1−p).

For the synthetic example, material is BRL 10, energy BRL 0.20 and printer use BRL 3, so A is BRL 13.20. With p=0.10, provision is approximately BRL 1.4667. Adding BRL 8 labor and BRL 2 packaging gives approximately BRL 24.6667 total production cost.

This is not a measured HEDAMI failure distribution. Partial failures, repeated manual setup, batch failures and correlated events are absent. A product decision is needed before expanding the model; a more complex equation is not automatically a more useful model.

## 6.3 Price equation and feasibility

Let C be production cost, F a fixed channel fee per sale, r the sum of percentage charges on actual revenue, m the requested contribution margin on revenue, and P the charged price before a promotional discount. Contribution is P−rP−F−C. Setting contribution equal to mP yields P=(C+F)/(1−r−m).

The denominator must be positive. A 95% requested margin with 10% revenue charges is infeasible. The application rejects it rather than silently choosing a different target. The mathematical bound is strict; speaking of a highest attainable margin can be misleading when there is only a limiting value below which targets must remain.

{{SNIP:src/Hedami.PricingAdvisor.Domain/PricingCalculator.cs|var denominator =|10}}

The list price is rounded upward to a cent so rounding does not reduce the undiscounted target. The discount is applied to that list price, and the charged amount is rounded to cents with MidpointRounding.AwayFromZero. No intermediate rounding is applied to cost components or analytical contribution. Displayed components may therefore not add exactly to a displayed rounded total.

The specific midpoint matters in the example: BRL 44.45 × 0.90 is BRL 40.005, which becomes BRL 40.01 under the chosen rule. This is a documented application convention, not a claim that every marketplace settles discounts or fees identically.

## 6.4 Margin, markup and profit vocabulary

Revenue margin is contribution divided by charged revenue. MarkupOnProductionCost is (charged price−production cost)/production cost; in this code it excludes selling charges and uses a different denominator. They answer different questions and must not be substituted for one another.

The result is called contribution, not net profit. Rent, administration, financing, depreciation categories outside the supplied machine input, owner compensation and other overhead may be absent. Taxes and advertising are included only as configured percentage charges in the same revenue-basis assumption, not through a complete accounting/tax model.

The current model does not cover different tax bases, fee caps, minimum fees, per-component settlement rounding or advertising per order. Adding channel/tax/advertising rates is valid only under the documented shared basis. Their separate values and provenance remain present in scenario evidence so the assumption is visible.

# 7. Data, provenance and the difference between missing and zero

The demo dataset contains a complete headset stand, a product with missing material grams, a valid synthetic channel rule, an expired rule, a rule with a missing charge and an unverified rule. Names and IDs support targeted tests. These are fixtures, not a product administration system.

Provenance includes an ID, source description, effective start/end dates and a validation status. The status enum is Demonstration, Confirmed or Unverified. Null financial fields represent pending information. There is no separate formal Fact/Estimate/Pending ontology; the implementation is a record-level status plus nullable cost/rate values. A future field-level provenance model would be more precise.

{{SNIP:src/Hedami.PricingAdvisor.Application/Tools/PricingData.cs|public void Validate(Provenance|12}}

The as-of date determines whether data is usable. Demo permission allows Demonstration records but does not bypass effective dates. Confirmed is a human-maintained label; the application does not independently fetch, sign or authenticate a source. A reader must not interpret it as proof that a marketplace published a current policy.

Product/rule reads can return records for inspection even when their inputs are incomplete or expired. Calculation validates them before arithmetic. This lets the analyst identify what is missing and ask a grounded question. The model is not forced to attempt a known-invalid calculation merely to prove it noticed a missing value; the original evaluator's contrary path assumption caused documented false negatives.

The historical demo fixtures are valid October 24–26, 2026. Reproducing those examples after that period requires an explicitly historical as-of date, not silently extending validity or presenting them as current policy. Changing the fixture date is a data change that should be documented.

# 8. Tools: the model-facing API of the application

A tool definition exposes a name, description and JSON schema. A typed C# argument record defines what the application expects after deserialization. These two representations meet at a strict local validation boundary. The model can request the capability; it cannot redefine its implementation.

| Tool | Available to | Arguments | Result and effect |
| --- | --- | --- | --- |
| list_pricing_data | Analyst | Empty object | Catalog IDs/names and rule provenance; no business mutation |
| get_product | Analyst | productId | Stored Product; tracks retrieval in run-scoped state |
| get_channel_rule | Analyst | ruleId | Stored ChannelRule; tracks retrieval |
| calculate_scenarios | Analyst | productId, ruleId, margins[], discountRate | Verified-cost scenario evidence for 0% and requested discount |
| submit_recommendation | Analyst | scenarioIds[], summary, assumptions[], limitations[] | Structured draft referencing existing evidence |
| request_clarification | Analyst | questions[] | Terminal awaiting-human outcome |
| update_plan | Analyst in PlanReview | nextTool, objective | Selects and gates the next permitted action |
| submit_review | Reviewer only | verdict, findings[] | Structured verdict parsed by PricingReview; not PricingTools dispatch |

The analyst has six base tools and a seventh only in planning mode. The reviewer has one separate submission tool and no catalog, calculator or write tool. No apply_price, publish_listing, arbitrary HTTP, file-write or shell tool is exposed to the model.

## 8.1 Why the calculation arguments contain IDs, not costs

{{SNIP:src/Hedami.PricingAdvisor.Application/Tools/ToolContracts.cs|public sealed record CalculateArguments|3}}

The model may choose target margins and discount based on the user request, but cannot submit an invented production cost. PricingTools looks up the product/rule itself, validates provenance and passes stored values to the calculator. This moves factual authority to a known application boundary and keeps source metadata attached to outputs.

If the user changes a material price in conversation, the current application does not automatically mutate the catalog. It should ask for an authoritative data update or clarification. A future input-edit workflow needs explicit validation, provenance and change history; it should not be smuggled into the model's arithmetic arguments.

## 8.2 Remote strict schema and local shape validation

Schemas.Object sets additionalProperties=false and lists all declared fields as required. ToolDefinition adds strict=true when generating the provider definition. Local ValidateShape independently checks objects, property counts, arrays, strings and decimal numbers; Serialization disallows unmapped members and uses case-sensitive property matching.

{{SNIP:src/Hedami.PricingAdvisor.Application/Tools/ToolContracts.cs|public JsonObject ToApi()|5}}

Requiring all fields matters for value types. If discountRate were omitted and ordinary deserialization defaulted it to zero, the application could silently analyze the wrong scenario. The local shape check rejects that omission before creating typed arguments. Extra fields such as costOverride are also rejected rather than ignored.

This validator supports the schema subset actually emitted by this prototype. It is not a general JSON Schema implementation or a substitute for semantic checks. Financial ranges, rule status, ID existence and cross-field feasibility are checked separately.

## 8.3 Permission enforcement and structured failures

{{SNIP:src/Hedami.PricingAdvisor.Application/Tools/PricingTools.cs|if (!Definitions.Any|8}}

Unknown operations receive permission_denied. This remains true if a user or model asks to ignore restrictions. The decisive protection is that no mutation branch exists in the dispatcher. Instructions reinforce the boundary but are not the only defense.

Known validation failures become a ToolResult with success=false, code, message and no data. They are returned to the analyst as a tool result so it can adapt or ask a question. An error result is not automatically a failed run: a correct recovery can end as awaiting_human. Error objects must not contain invented successful data.

The harness also checks successful result types by tool name and rejects malformed or oversized results. This is a concrete typed-contract check; it does not prove every narrative claim in the payload is true or authenticate an external data source.

## 8.4 Atomic scenario batches and evidence IDs

calculate_scenarios permits one to five target margins, removes duplicate targets and returns an undiscounted case plus the requested discount case when the discount is nonzero. The whole result array is computed before entries are added to the evidence dictionary. If one margin is infeasible, no partially successful batch is persisted into that dictionary.

Each scenario receives a run-scoped generated ID. submit_recommendation accepts one to twenty distinct IDs and rejects unknown references. It selects the stored evidence rather than trusting model-supplied numeric fields, then recomputes the relevant cost/scenario records before assembling the draft. A model cannot fabricate an accepted scenario ID merely by writing a plausible string.

The model-editable draft fields are summary, assumptions and limitations. Scenario records come from application state and are not an arbitrary editable JSON document. This restriction later explains why reviewer requests to add immutable scenario fields can be unproductive.

# 9. The main agentic loop and where it lives

{{FIG:main-loop.png|Figure 2. The actual repeated analyst loop. The model chooses a call; application code validates, executes, records and returns its result before asking the model again.}}

The loop is in PricingHarness.RunAsync. Its repeated structure is a while(true) with explicit stop conditions. Each iteration checks cancellation, assembles the analyst instruction context, calls the model, requires exactly one function call, enforces call identity/repetition/budgets, executes a tool and returns its output to the next model input. A successful terminal tool either stores questions or a draft.

{{SNIP:src/Hedami.PricingAdvisor.Application/Harness/PricingHarness.cs|while (true)|13}}

This is agentic because the next tool is selected from the model's interpretation of the objective and accumulated results. The application does not hardcode get_product, get_channel_rule and calculate_scenarios as a fixed sequence for every request. The first real run happened to choose that sequence. A request with missing grams may stop earlier with a question, and a tool error may change the next action.

The harness remains an ordinary deterministic C# program around those model choices. Agentic does not mean unrestricted execution or a loop without limits. Its job is precisely to make nondeterministic choices inspectable and constrained.

## 9.1 Context continuity

After a model turn, output items are cloned into the in-memory input context. After execution, a function_call_output item uses the same call_id and contains the serialized ToolResult. The next model response therefore sees what happened rather than guessing whether a requested action succeeded.

{{SNIP:src/Hedami.PricingAdvisor.Application/Harness/PricingHarness.cs|context.Add(new JsonObject { ["type"] = "function_call_output"|11}}

The provider request uses store=false and sends the bounded application-managed conversation. This avoids relying on hidden server conversation state for the loop, but repeated context can increase input tokens. There is no general context compaction algorithm. Character limits, token checks and bounded prior-feedback projection are the implemented controls.

Private reasoning is not requested as evidence. If a provider returns protocol reasoning items, the adapter can carry them in memory for continuity without writing their content to the trace. Observable tool requests/results, selected plan objectives and concise review findings are the evidence this project relies on.

## 9.2 Termination is part of the design

A successful tool-only recommendation ends as draft_ready. A valid reviewer approval can end as reviewed_draft_ready. Clarification ends as awaiting_human. Limits and failures produce distinct statuses rather than silently pretending a complete recommendation exists.

The adapter asks for a tool call, and the harness rejects a model turn without exactly one call. A free-text paragraph is not accepted as a completed recommendation. The draft must pass through the submission tool and existing evidence checks. This simplifies the output contract at the cost of a more constrained conversational interface.

# 10. The harness: execution controls, state and failure behavior

The harness instance owns one run. Interlocked.Exchange prevents reuse, avoiding accidental carryover of drafts, retrieved IDs or selected plans between objectives. CLI continuations create a new catalog/tool/harness instance with a parent-run link. This is intentional isolation, not an implementation of a persistent interactive session server.

RunRecord stores IDs, parent link, start/duration, status, simulation flag, requested model, mode, instruction version, limits, counters, usage, cost estimate, recommendation/questions and observable events. PricingTools separately holds retrieved-ID sets, scenario evidence, selected next action and terminal draft/question state for that run.

## 10.1 Limits: library defaults versus CLI defaults

| Control | RunLimits default | Console default | Enforcement meaning |
| --- | --- | --- | --- |
| Model calls | 12 | 20 | Attempts counted before provider invocation |
| Analyst tool calls | 12 | 20 | Executor attempts; reviewer submissions are separate |
| Output tokens per response | 1,800 | 1,800 | Provider request cap |
| Reported total tokens | 40,000 | 40,000 | Checked using returned usage; one response can cross threshold |
| Context characters | 90,000 | 90,000 | Approximate size bound, not an exact tokenizer |
| Equivalent tool calls | 2 allowed | 2 allowed | Third equivalent request stops as no_progress |
| Total technical retries | 1 | 1 | Shared run budget for eligible provider failures |
| Analyst revisions | 1 | 1 | At most one revised draft after critique by default |
| Run duration | 180 seconds | 180 seconds | Linked cancellation budget |
| Model call duration | 45 seconds | 45 seconds | Per-attempt timeout |
| Tool duration | 5 seconds | 5 seconds | Cooperative asynchronous timeout/cancellation |

RunLimits.Validate constrains configuration ranges. CLI flags expose a subset; not every library setting has a command-line switch. Exceeding the authorized API spend is controlled separately by the persistent spend ledger, not by interpreting a token threshold as an exact currency cap.

## 10.2 Cancellation and timeout mechanics

The run creates a linked CancellationTokenSource and applies CancelAfter. Model calls and tool executions receive linked child tokens with their own deadlines. WaitAsync can stop waiting on a cooperative asynchronous operation. Cancellation can come from the user, run deadline or child timeout, and statuses distinguish cancelled, run_timeout and model_timeout where implemented.

A tool timeout is returned as a structured tool error so the analyst can respond. The prototype's actual tools are local reads and deterministic calculations; injected asynchronous delays test the timeout path. A synchronous executor that blocks before returning a Task is not forcibly interrupted by WaitAsync. Uncooperative background work and hostile side-effecting plugins are outside the guarantee. This is an important phase-two hardening boundary, not an absolute execution sandbox.

Persistence in finalization is deliberately independent of the run cancellation token, allowing a cancellation status to be written after cancellation. Filesystem failure or process termination can still prevent a final snapshot.

## 10.3 Technical retry and review revision are different loops

CallModelAsync retries eligible transient ModelException failures while the total technical-retry count remains below its limit. The implemented backoff is a small 250 ms × retry-count delay. It is not a full jittered exponential policy, does not interpret every provider Retry-After header and should not be described as production resilience.

A review revision instead sends structured feedback back to the analyst and asks for a changed draft. It consumes the revision counter, not the technical-retry counter. Mixing these budgets would make transport failures look like reasoning revisions and obscure evaluation metrics.

Known billing/quota codes are not retried; repeatedly sending the same request does not refill an account. DNS has a separate non-retryable classification. Unclassified 429/5xx or generic network failures may use the technical retry budget.

## 10.4 Repetition and lack of progress

The harness canonicalizes tool arguments: object keys are sorted, arrays retain order, and decimal numbers use a consistent representation. Equivalent requests with reordered fields or 0.10 versus 0.1 therefore count as the same signature. After the configured number of equivalent calls, the run stops as no_progress before another tool execution.

This is bounded repeated-call detection, not a semantic theorem about progress. Alternating different unhelpful calls can evade the signature counter, though model/tool/time/token bounds still stop indefinite execution. Repeating a successful read may also be conservatively treated as no progress. A richer state-aware detector would need its own evaluation.

## 10.5 Type and result validation

The harness verifies ToolResult consistency: successful results need data and code=ok; failed results need a message and no successful data. It checks concrete data types for each known tool and rejects oversized serialized results. PricingTools performs business checks before returning those types.

{{SNIP:src/Hedami.PricingAdvisor.Application/Harness/PricingHarness.cs|var typed = name switch|13}}

These layers answer different questions. The schema checks shape, the typed record checks structure, domain validation checks money rules, provenance checks input usability, and deterministic verification checks evidence consistency. None alone establishes that a model's final prose is complete, clear or free of unsupported claims.

## 10.6 Trace persistence and observability

RecordAsync appends a numbered, timestamped event and asks JsonRunStore to save the current RunRecord. The store serializes to a temporary file and replaces the run JSON file, reducing the chance of readers seeing a partially written run snapshot. Known credential values and key-like patterns are redacted defensively.

{{SNIP:src/Hedami.PricingAdvisor.Application/Harness/JsonRunStore.cs|public async Task SaveAsync|12}}

Events include started, model_requested/model_completed, tool_requested/tool_completed, deterministic_verification, review_response/review/review_rejected, technical_retry and finished where applicable. Early baseline invalid reviews lacked their rejected arguments/reasons; this was an observability defect, fixed for follow-up. Their exact original cause cannot be recovered and is not invented in this guide.

The trace is sufficient for inspection of observable actions and many metrics; it is not a complete restartable provider-session journal. It does not persist private reasoning or every prompt payload. It has no database transactions across runs, no encryption at rest, no retention service and no multi-user access policy. Business text in traces should be reviewed before sharing, even though the evaluated fixtures are synthetic.

# 11. Agent roles, bounded reflection and collaboration

{{FIG:review-loop.png|Figure 3. Analyst and reviewer have different contexts and capabilities. The harness coordinates critique and bounded revision; neither reviewer approval nor human-approval metadata applies a price.}}

## 11.1 Analyst

The analyst receives the user objective, the trusted profitability procedure, allowed tool definitions and accumulated results. In planning mode it also receives a brief current-stage/selected-action description. Its responsibility is to ground facts, choose requested scenarios, recover appropriately from errors and produce either a traceable draft or specific questions.

The analyst can discover/read local records, request deterministic scenarios and submit references to evidence. It cannot overwrite calculator results, edit arbitrary scenario fields, change .env, perform marketplace HTTP calls or grant itself a new capability. Its summary is model-generated narrative and remains subject to semantic review.

## 11.2 Reviewer

The reviewer receives a fresh model input with the objective, structured draft and concise prior feedback context when applicable. It receives the review procedure and only submit_review. It does not receive the analyst's tool-call conversation or general tools. The same provider client/model can serve both roles; fresh context and reduced capabilities are the implemented separation.

A review contains verdict=approve or revise, plus findings. Each finding includes evidencePointer, issue and requestedChange. Approve requires zero findings; revise requires one or more. Pointers resolve to an existing value in the supplied draft. The implementation accepts draft-relative pointers and an explicit /draft prefix. Pointer existence makes the criticism traceable, not substantively correct.

{{SNIP:src/Hedami.PricingAdvisor.Application/Agents/PricingReview.cs|public sealed record ReviewFinding|2}}

The reviewer is instructed not to recompute money because deterministic verification already does that. It should examine scenario coverage, misleading profit terminology, source labels, discount interpretation and unsupported claims. However, instructions do not guarantee useful judgment. Actual follow-up reviews sometimes demanded redundant labels or changes the analyst could not make through its tool contract.

## 11.3 Reflection loop and its stop conditions

After a valid draft, the harness rechecks calculator evidence. In review modes it calls the reviewer in the fresh context. Approval yields reviewed_draft_ready. Revision increments the bounded revision count, clears the active draft, appends the critique to analyst context and resumes the model/tool loop.

{{SNIP:src/Hedami.PricingAdvisor.Application/Harness/PricingHarness.cs|if (review.Verdict == "approve")|13}}

If the next review still requests changes after the revision budget is spent, review_limit is a truthful terminal result. The retained rejected draft is useful for inspection but must not be presented as a successfully reviewed recommendation. Token and time limits can also stop before a second review completes.

This is reflection implemented as a distinct reviewer role with a feedback loop. It is also a limited form of multi-agent collaboration: different model contexts pursue related roles and exchange structured evidence through the harness. There are no parallel agent workers, shared-agent memory service or third autonomous planner agent.

## 11.4 Approval has three meanings that must remain separate

Reviewer approve means that model returned a structurally valid review verdict. HumanApprovalRequired=true means the draft says commercial use would require human authorization. An implemented approval-to-marketplace workflow would be a separate capability with identity, audit and permission enforcement; it does not exist here.

Consequently, neither reviewer approval nor a user saying approve can cause a price change in the current application. The absence of a write primitive is the operative boundary. The metadata is not a substitute for a future human approval service.

# 12. Planning that actually constrains execution

{{FIG:planning.png|Figure 4. Planning is a next-action gate within the analyst role. The chosen action is consumed; missing-data clarification remains an allowed safe exit.}}

PlanReview adds update_plan to the analyst tool definitions. A plan contains nextTool and objective, where objective is a short observable reason for the selected action. The selection is stored in PricingTools.NextTool. Before a non-plan action, PricingTools checks that the requested tool matches the selection and then consumes it. After an error, another plan is needed for the next gated action.

{{SNIP:src/Hedami.PricingAdvisor.Application/Tools/PricingTools.cs|if (adaptivePlanning && name is not|6}}

Planning dependencies are checked in two places. The plan cannot select calculation before some product/rule retrieval exists, and the calculation itself verifies retrieval of the particular product/rule IDs. A draft plan requires existing calculation evidence. This prevents a decorative plan from authorizing an unrelated calculation merely because some other record was read earlier.

request_clarification is an intentional escape from the gate, so uncertainty can end safely without forcing an unnecessary sequence. update_plan itself selects only known base tools. PlanStage provides a small summary such as retrieve_data, calculate_or_clarify or recommend_or_recalculate; it is derived from run-scoped tool state.

This is adaptive next-action planning, not a general multi-step planning language, task scheduler or goal decomposition system. It adds calls and can grow context. The real follow-up showed token-limit failures in two PlanReview complete-scenario runs. This is evidence to optimize or defer the mode, not an invitation to remove the budgets until the demo appears to pass.

# 13. Runtime skills and development instructions

The application has two reusable procedure files. They are named analyze-profitability.md and review-pricing.md inside runtime-skills. Their names and contents are explicitly loaded by RuntimeInstructions.Load. The term skill describes their reusable procedure role in this product; it does not imply an external skill runtime was installed.

| Procedure | Used by | Main responsibilities |
| --- | --- | --- |
| analyze-profitability.md | Analyst instruction field | Ground facts, retrieve IDs, call calculator, label assumptions, ask for missing data, interpret discounts, submit evidence-backed draft |
| review-pricing.md | Reviewer instruction field | Check coverage/claims/labels, cite existing evidence, request testable changes, approve or revise, avoid arithmetic authority |

{{SNIP:src/Hedami.PricingAdvisor.Application/Agents/RuntimeInstructions.cs|public static RuntimeInstructions Load|10}}

The loader reads exactly the two allowlisted names, bounds file sizes and computes SHA-256 over analyst text, a newline and reviewer text. A runtime-v1 prefix plus digest becomes InstructionsVersion in the run. This identifies the content version but does not cryptographically attest who approved the procedure or prevent a person with filesystem write access from editing it.

The analyst procedure is passed as instructions.Analyst to CallModelAsync. The reviewer procedure is used only for the fresh reviewer call. Neither file executes code directly. Its influence occurs through the model's next response, with hard limits/permissions still enforced in application code.

AGENTS.md guides Codex during development. The OpenAI Docs development skill was used to consult official API guidance. Neither is automatically read by RuntimeInstructions.Load. No generic discovery of arbitrary SKILL.md files, remote skill registry, MCP skill loader or framework-provided plugin system is present.

The complete current runtime procedures are included in an appendix for offline reading. The archived baseline procedures preserve the earlier content used for the original 42-run comparison; current wording should not be retroactively attributed to older traces.

# 14. Model integration, cost control and human feedback

## 14.1 Provider boundary and request choices

IModelClient exposes a model name, a simulation indicator and an asynchronous CompleteAsync contract accepting instructions, input context, tool definitions, output-token cap and cancellation. This is a small boundary used by both the real adapter and scripted test models; it is not a full provider abstraction covering every possible multimodal API.

OpenAiResponsesClient sends to https://api.openai.com/v1/responses with an application-owned bearer credential. Its payload includes strict tool definitions, tool_choice=required, parallel_tool_calls=false, store=false, output cap and reasoning effort none. These are choices made by this implementation. The supported pinned model was used in actual recorded runs; an arbitrary configurable model may reject some settings.

{{SNIP:src/Hedami.PricingAdvisor.Application/Models/OpenAiResponsesClient.cs|var payload = new JsonObject|11}}

The parser reads function-call items, call IDs, argument strings and usage, while accepting only completed provider responses. Incomplete outputs cannot be executed as if complete. Success bodies have a bounded read size. Error handling extracts only allowlisted billing codes from a bounded body; arbitrary provider messages and headers are not logged. HTTP status alone is insufficient to distinguish traffic rate limits from exhausted credits.

The implementation maintains its own conversation replay and retries. No SDK provides hidden automatic retries. That makes behavior explicit but creates ongoing compatibility responsibility. The OpenAI function-calling guide and model page are background references; the precise request/parse behavior described here is grounded in this repository's adapter and tests.

## 14.2 Estimated spend ledger

Heron authorized USD 2 total for application calls. Before HTTP transmission, ApiSpendBudget reserves estimated cost using serialized request UTF-8 bytes plus a protocol allowance priced at the uncached-input rate and the maximum allowed output tokens. A completed response with usable usage settles the reservation using uncached, cached and output token rates. An unknown or failed response retains the reservation instead of becoming assumed zero consumption.

The ledger file is held with an exclusive lock to reject concurrent CLI writers. Reservations are persisted before sending and survive process restart. Unsupported pricing models are blocked. A settled reservation cannot be silently settled twice. Tests verify overspend denial, restart persistence, concurrent-writer denial, cached-input pricing and no HTTP transmission when the budget is exhausted.

This is a conservative estimate-based local control conditional on the supported text model, rate configuration and request assumptions. It is not an account billing integration, invoice guarantee or control over other applications. The ledger writes in place and can fail/corrupt on process/filesystem failure; failing closed is preferable to silently resetting it. A production service would need stronger durability and shared-account governance.

Rates recorded in code were verified October 24, 2026: USD 0.75 per million uncached input tokens, USD 0.075 cached input and USD 4.50 output. These are historical standard text rate assumptions, not a perpetual pricing guarantee. Unknown usage produces a null run cost estimate. ModelUsage counts and the ledger should be reviewed together when timeouts or retries occur.

## 14.3 Linked human-feedback loop

{{FIG:feedback.png|Figure 5. A clarification or draft can lead to a new linked run. Human feedback is preserved with concise context, while current catalog data is retrieved again.}}

Human feedback is implemented as a new run linked by parentRunId, not by reusing the old harness instance. FeedbackContext.Build extracts the original objective, prior objective/summary, a compact projection of prior scenario values and any questions. It puts new feedback into an objective that explicitly supersedes conflicting prior targets.

{{SNIP:src/Hedami.PricingAdvisor.Application/Harness/FeedbackContext.cs|priorScenarios =|9}}

This was necessary because embedding a whole prior recommendation, with repeated cost/provenance objects for six scenarios, could exceed the objective limit. The regression test builds such a large prior draft and confirms the bounded projection. The original objective is retained without recursively copying an entire history.

The real linked run changed the target from earlier values to exactly 25%, re-read the same synthetic product/rule and recalculated. It returned list price BRL 41.03 and discounted price BRL 36.93; displayed margins were 25.01% and 17.79%. This demonstrates feedback-driven re-analysis. It does not demonstrate a catalog-editing UI, automatic data corrections or human approval for a price write.

# 15. How the product was created

The sequence below records actual development decisions rather than a fictional ideal process. Work was performed in small executable increments. Heron supplied the business problem, acceptance expectations and architectural constraints; Codex implemented and inspected code/evidence with Heron's authorization and terminal participation.

| Stage | Concrete action | Decision or lesson |
| --- | --- | --- |
| Environment inspection | Inspected the initially empty directory, OS, Git and .NET SDK | Use installed .NET 10; do not assume an existing repository or SDK |
| First executable slice | Domain calculator, console example, financial tests and scope/decision documents | Establish deterministic financial authority before a model loop |
| Provider selection | Heron chose OpenAI API and later configured a local key | Codex login is not an application credential |
| Tool layer | Typed contracts, strict schemas, stored IDs, provenance and date checks | The model cannot inject replacement cost numbers |
| Explicit harness | Added limits, errors, cancellation, traces and terminal statuses | Make controls visible rather than assume framework defaults |
| Reflection/planning | Added fresh reviewer context and an enforced selected-action gate | Implement distinct behavior before comparing benefit |
| Runtime procedures | Added an explicit two-file Markdown loader and instruction hash | Skills need a demonstrated loading mechanism |
| Local verification | Scripted model/mocked HTTP/financial assertions; fallback driver | Simulated tests do not prove a real model loop |
| First real attempt | Session DNS could not resolve api.openai.com | Connectivity failure does not diagnose account credits |
| Host-terminal handoff | Heron ran the prepared script successfully | The real tool-use example reached draft_ready |
| Baseline comparison | 42 workflows: seven scenarios × two repetitions × three modes | Preserve original failures and compare actual results |
| Diagnosis | Found grader path assumptions, invalid reviews and weak expiry wording | Evaluate the evaluator and observability, not just the agent |
| Targeted changes | Added review diagnostics/pointer compatibility and clarified procedures | Do not retroactively claim older runs used newer instructions |
| Follow-up | 12 recommendation workflows plus a linked feedback workflow | Contract compatibility improved in sample; review still overreached |
| Delivery evidence | Archived traces/reports, checked calculations, documented limits | Tools remains default; additional agents remain experiments |

## 15.1 Environment constraints and their effect

The session could compile but the standard VSTest runner could not bind local sockets. The application test assembly was still compiled, and a small in-process driver invoked the existing xUnit Fact/Theory/InlineData methods and awaited asynchronous cases. That allowed assertions to run without mislabeling the result as successful VSTest execution.

Writes to .git were prohibited by the session policy, so no local checkpoint was claimed. Git initialization/commits need a host-terminal environment with write access and confirmed identity. This limitation is operational and separate from whether the application code runs.

The first network attempt ended as network_error; independent DNS and unauthenticated curl diagnostics also failed to resolve api.openai.com. The later terminal workflow succeeded. Adding credits was not established as the fix because the observed earlier failure occurred before a provider response.

NuGet vulnerability metadata could not be fetched in the restricted session. Locally available dependencies enabled builds, but that is not a completed dependency vulnerability check. Documentation records the warning instead of silently calling every check green.

## 15.2 Contribution and ownership

Heron defined the business opportunity, prioritization, financial constraints, evaluation scenarios, provider choice, synthetic-data policy and USD 2 spending authorization. He configured the credential and executed the host-terminal scripts. These are concrete product and engineering contributions.

Codex authored the implementation, local assertions, scripts and documents, executed local verification, and inspected actual traces. Heron's independent code review and fluency are separate remaining activities. The portfolio should describe assisted development candidly and avoid attributing unreviewed generated choices to Heron as if independently authored or approved.

# 16. Evaluation design and actual results

Evaluations answer different questions at different boundaries. Combining them into a single undifferentiated test count would conceal important limits.

| Layer | What it establishes | What it does not establish |
| --- | --- | --- |
| Calculator assertions | Expected unit conversions, formulas, rounding and invalid-input behavior | Real business inputs or model quality |
| Tool assertions | Contract enforcement, provenance checks, permissions and evidence references | Semantic correctness of every model explanation |
| Scripted model tests | Harness behavior under controlled calls, repetition, faults and review | Real LLM reliability or useful autonomous planning |
| Mocked HTTP tests | Request/parse contract, sanitized errors and denied transmission | Real account/model access |
| Real workflows | Observed model choices, traces, latency and reported usage | Production reliability or a measured productivity improvement |
| Codex-assisted trace inspection | Grounded examination of specific numeric/narrative cases | Independent human business approval |

## 16.1 Local assertion inventory

| Area | Passing cases | Representative checks |
| --- | ---: | --- |
| Calculator | 16 | Costs, missing versus zero, invalid rates, margin/markup, discount harm and rounding |
| Pricing tools | 13 | Missing/expired/unverified data, argument shape, evidence IDs, plan gates, deny application |
| Harness and reviewer | 16 | Complete loop, clarification, repetition, faults, cancellation, limits, fresh context and pointer validation |
| OpenAI HTTP/protocol | 6 | Strict/stateless request, response usage, incomplete output, credit classification and budget-before-send |
| Spend ledger | 4 | Cached rates, reservation ceiling, persistence and concurrent-writer rejection |
| Feedback context | 1 | Large prior draft reduced to bounded numeric projection |
| Trace store | 1 | Atomic replacement and credential redaction |
| Total | 57 | Actual xUnit assertions through the in-process fallback |

These are case counts, including individual InlineData rows, not necessarily the number of test methods. No coverage percentage or mutation-testing score was collected. The fallback is intentionally small and does not implement full xUnit fixtures, skip semantics or parallelism; it supports the current suite. Standard test execution in a permissive terminal remains distinct.

## 16.2 Seven baseline scenarios

The baseline scenarios were complete product, missing material, infeasible 95% margin, harmful discount, expired rule, missing fee and an unauthorized immediate marketplace application request. Each ran twice in Tools, Review and PlanReview. Fault/timeouts/invalid-argument injection and canonical repetition were covered locally rather than claimed as separate real-provider scenarios.

The original automatic grader required successful requested scenario coverage for complete/harmful-discount cases, an awaiting-human state plus matching deterministic validation error for several missing/invalid cases, and no application plus an awaiting-human response for the unauthorized case. This path-specific requirement is central to interpreting the results.

## 16.3 Original 42-workflow comparison

| Mode | Original automatic passes | Median seconds | Model calls | Analyst tool calls | Reported tokens | Estimated USD |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Tools | 11/14 | 3.850 | 47 | 47 | 60,636 | 0.051805 |
| Review | 8/14 | 3.942 | 50 | 46 | 70,416 | 0.058165 |
| PlanReview | 6/14 | 6.185 | 83 | 79 | 135,972 | 0.124319 |

All modes used the same pinned model and objective set under the recorded run limits. Medians include quick clarification outcomes that never invoke the reviewer. Those medians are not a claim that reviewed recommendations take only approximately four seconds. ModelCalls includes reviewer/provider attempts; ToolCalls counts analyst executor attempts and does not count submit_review as an executor tool call. ReviewCalls is separately recorded for scheduled review stages.

The original pass counts are preserved, not overwritten after diagnosis. Additional roles increased calls/usage and did not demonstrate better end-to-end automatic success in this sample. This supports keeping Tools as the default for this prototype, not a universal conclusion about all multi-agent systems.

## 16.4 What the baseline failures actually meant

Eight of seventeen automatic failures were safe early clarifications about missing material or fees. The model read the null value and asked a specific question without producing a price. The grader marked that as failure because no invalid calculation call occurred. The intended user behavior was achieved, so these are documented false negatives in the metric's path assumption.

Four expired-rule failures ended without a draft but included undesirable offers to proceed with expired data. The hard tool boundary would still reject calculation, yet the wording was not satisfactory. Safety of the execution boundary and quality of communication are separate criteria.

Five reviewed attempts ended invalid_review. The original harness did not persist rejected review contents/reasons, so the exact causes cannot be reconstructed. A draft-envelope pointer mismatch was a plausible integration hypothesis. Supporting that prefix and adding diagnostics were justified changes, but the document does not claim to know what those five old responses contained.

An outcome-based future grader should accept a grounded early clarification without forcing a known-invalid operation, while separately scoring misleading suggestions. That future improvement must not silently rewrite historical scores or be confused with independently measured model improvement.

## 16.5 Targeted 12-workflow follow-up

After pointer compatibility, diagnostic and procedure changes, two recommendation scenarios were repeated twice across the three modes. This smaller scenario mix differs from the baseline, and multiple changes were introduced together. The follow-up cannot isolate a single causal effect.

| Mode | Automatic passes | Median seconds | Model calls | Reported tokens | Estimated USD |
| --- | ---: | ---: | ---: | ---: | ---: |
| Tools | 4/4 | 4.263 | 16 | 28,358 | 0.025516 |
| Review | 2/4 | 9.325 | 26 | 65,969 | 0.058973 |
| PlanReview | 1/4 | 14.020 | 46 | 139,299 | 0.107119 |

No invalid_review occurred in this follow-up. Three workflows exhausted the review budget; two exhausted the token budget. The absence of a structural review failure in twelve cases is evidence of observed compatibility, not proof of general review quality or of the precise original failure cause.

One reviewer usefully asked for an explicit statement that the discounted contribution became negative. The analyst revised the summary and later received approval. Other reviews demanded repetitive language, information already visible in provenance, or changes to scenario fields that the analyst could not edit. Some continued introducing issues after revision. A fresh context is not enough to make criticism correct or actionable.

## 16.6 Real workflow and cost totals

The complete evidence set contains 42 baseline workflows, 12 follow-up workflows, two smoke workflows and one linked feedback workflow: 57 real workflows with a provider response. There was also one separate DNS-failed session attempt. The persistent ledger has 280 settled response-usage entries. Estimated accumulated application cost was USD 0.44420565, below the authorized USD 2. This is not the account's remaining credit balance or a provider invoice.

The first smoke workflow took 7,745 ms with four model calls, four analyst tool calls, 5,258 input tokens and 418 output tokens; estimated cost was USD 0.0058245. Its four calls are the concrete tool-use example discussed earlier.

Codex independently cross-checked 48 baseline calculation rows and 50 follow-up/feedback rows using Python Decimal and the documented synthetic formulas. All 98 matched within 1e-24 for unrounded ratios. Raw archived JSON retains its original decimal text. This supplements, rather than replaces, the C# assertions and runtime evidence verification.

# 17. Reading a real trace and understanding statuses

Start with docs/evidence/2026-10-24-first-real-run.md and its linked raw JSON. Confirm simulated=false, the requested model, instructionsVersion, objective, event order and final status. Then inspect tool_requested arguments and corresponding tool_completed results. A requested action is not evidence of execution until its result exists.

For a recommendation, inspect costs, appliedRule, product/rule provenance, asOf and scenario IDs. Compare the narrative against these deterministic fields. Check humanApprovalRequired=true and priceApplied=false. A ready status indicates a completed draft under that mode, not commercial authorization or production suitability.

| Status or error | Meaning | Correct interpretation |
| --- | --- | --- |
| draft_ready | Analyst submitted a verified-evidence draft in Tools mode | Read the narrative and limitations before use |
| reviewed_draft_ready | A valid reviewer approval ended the workflow | Model review passed, not human commercial approval |
| awaiting_human | Specific clarification terminal path | A valid intended outcome for missing information |
| invalid_review | Reviewer submission failed validation | No successful review should be claimed |
| review_limit | Critiques remained after the revision budget | Retained draft is inspection material, not approved output |
| token_limit / context_limit | A configured context/usage bound stopped execution | Do not remove bounds merely to obtain a success screenshot |
| no_progress | Equivalent tool-call limit reached | Repetition was bounded; not a semantic progress proof |
| model_timeout / run_timeout / cancelled | Time/user cancellation path | Usage may be incomplete and spend reservation may remain |
| dns_resolution_failed / network_error | Connectivity failure before a usable response | Does not establish credit balance |
| openai_insufficient_quota or other allowlisted billing code | Provider indicated a quota/credit/spend condition | Requires account action, not repeated retries |
| spend_limit | Local estimated-spend reservation would exceed authorization | Do not reset the ledger to bypass the limit |
| permission_denied (tool result) | Requested operation is outside the allowlist | The model can recover or clarify; not necessarily a run status |
| validation_error (tool result) | A known argument/business check failed | Interpret the message and adapt without fabricating a result |

For invalid_review in follow-up, review_response preserves observable tool arguments and review_rejected preserves a bounded reason. Those are not private reasoning transcripts. For a review_limit result, inspect all review events and submitted drafts to distinguish a useful unresolved issue from repetitive or impossible-to-act-on criticism.

# 18. Runbook: reproducing and troubleshooting the prototype

Run commands from the repository root. Use the SDK declared in global.json. The calculator requires no API credential. Paid commands use the application key and the existing persistent USD 2 ledger; do not rerun large suites just to read this document.

```sh
dotnet restore
dotnet build -m:1 -p:UseSharedCompilation=false -nodeReuse:false
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- calculator
```

The standard test command is dotnet test. In a session where VSTest sockets are prohibited, build first and use the committed fallback below. Report which route actually ran rather than silently treating them as identical infrastructure.

```sh
dotnet run --project tests/Hedami.PricingAdvisor.Verification \
  -m:1 -p:UseSharedCompilation=false -nodeReuse:false
```

## 18.1 Configuration and a small real request

Create or edit the ignored local .env in an editor, not in chat. Set OPENAI_API_KEY and optionally OPENAI_MODEL. The example file has no secret value. doctor reports configuration presence without a provider request. Then a deliberately small historical demonstration can test actual access.

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- doctor
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- analyze \
  --demo --as-of 2026-10-24 --mode tools \
  --request "Analyze productId=headset-stand using ruleId=demo-direct. Compare 30% revenue margin with and without a 10% discount."
```

Use review or plan-review only when demonstrating those experimental modes. Their failures and additional usage are part of the evidence. Do not interpret a rejected draft printed for inspection as successful just because numeric rows appear in the terminal.

## 18.2 Feedback and inspection

```sh
dotnet run --no-build --project src/Hedami.PricingAdvisor.Console -- analyze \
  --demo --as-of 2026-10-24 --mode tools --feedback-from RUN_ID \
  --feedback "Replace previous targets with exactly 25% and keep the 10% discount comparison."
```

RUN_ID is the 32-character recorded ID, not a path or credential. The command retrieves the local parent trace and starts a new linked run. Data corrections must still be made through an authoritative local input process; free-text feedback does not silently edit costs.

Raw local runs live in artifacts/runs. Reports are artifacts/evaluations-*.json. Artifacts are ignored because traces can contain business data. Selected inspected synthetic evidence is copied into docs/evidence. The copied evidence is historical; the local mutable spend ledger must not be replaced with an archived copy.

## 18.3 Comparison scripts and their purpose

scripts/validate-live.sh builds, runs local assertions and performs one small real smoke analysis. Its compare argument also runs the original seven-scenario, two-repetition all-mode suite. scripts/validate-followup.sh runs the two recommendation scenarios across modes and a linked human-feedback request. The latter continues to the feedback example after saving comparison failures, then returns the comparison status.

scripts/summarize-evaluations.py reads an existing report and creates a local Markdown summary without network calls. It is an aggregation aid, not a semantic grader. Always retain the original report and underlying run IDs when publishing a conclusion.

## 18.4 Troubleshooting sequence

First determine whether the failure occurred before configuration, before network resolution, at HTTP access, during protocol parsing, at tool validation or at a bounded agent outcome. These are different issues. A DNS failure should not trigger a billing diagnosis. A provider 401 suggests credential/access investigation; a 429 needs its structured code to distinguish rate pressure from billing. A tool validation error may correctly lead to clarification.

Do not print .env contents, request authorization headers, complete arbitrary provider error bodies or private business traces into a public issue. The prototype sanitizes known error paths, but a developer's manual diagnostic command can still reveal a secret. For account-level financial controls, consult the current provider console and official guidance rather than assuming the local estimate ledger controls unrelated account activity.

# 19. What Heron should be able to defend

The interview discussion should connect product objectives to engineering choices and observed learning. It is unnecessary to memorize every API field, but essential to understand where authority and failure boundaries are located.

| Interview question | A defensible answer anchored in this implementation |
| --- | --- |
| Why use an LLM here? | Interpret objectives, select tools, identify missing inputs and explain scenarios across steps |
| Why not let it calculate the price? | Financial arithmetic needs explicit units, deterministic rules, rounding and independent assertions |
| What makes this agentic? | Model-selected actions repeat with actual execution feedback rather than a single generated paragraph |
| Where is the harness? | PricingHarness owns the loop and budgets; tools/provider/store implement concrete enforcement boundaries |
| What does planning change? | The selected next action gates execution and is consumed; retrieval/evidence dependencies are checked |
| Are there multiple agents? | Analyst and reviewer have separate instructions, contexts and tools, even with the same model; planner is a mode |
| What are the skills? | Two trusted reusable Markdown procedures loaded explicitly and versioned with a content hash |
| Is the reviewer a financial authority? | No; C# rechecks arithmetic, and review findings can be useful, wrong or unworkable |
| What did evaluations teach? | More roles added cost/latency without demonstrated benefit; a path-specific grader also penalized safe behavior |
| Can the system change prices? | No executable mutation capability exists; draft metadata and reviewer verdicts are not a write workflow |
| What did you personally decide? | Product scope, constraints, data policy, provider/budget and evaluation expectations; explain assisted coding candidly |
| Is it production ready? | No; input governance, semantic quality, persistence/security and operational controls remain prototype limitations |

A useful demonstration begins with the Tools workflow, shows the actual trace and calculator evidence, then shows linked feedback. One useful review and one bounded failure illustrate reflection honestly. Do not make an interview demo depend on rerunning all fifty-four comparison cases live.

For a short explanation, say: the analyst can decide what to inspect, but it cannot invent accepted cost inputs or execute an unregistered action. The harness records the interaction and enforces budgets. Deterministic evidence is rechecked before a draft is accepted. Experiments showed why extra agent roles require measured value rather than architectural enthusiasm.

# 20. References, source map and evidence inventory

The repository is the primary source for implementation claims. The preserved reports and traces are the primary source for observed metrics. Official provider documentation supports the API/model background; it is not evidence that this application passes its own scenarios.

| Source | What to read it for |
| --- | --- |
| README.md | Run commands, current status and operational caveats |
| AGENTS.md | Development agreement, not runtime agent instructions |
| docs/project-brief.md | Product scope and delivery context |
| docs/decisions.md | Ordered choices, changes and contribution record |
| docs/calculation-contract.md | Financial formulas, units and assumptions |
| docs/architecture.md | Component and pattern explanation |
| docs/evaluations.md | Evaluation layers and strict-grader limitations |
| docs/evidence/final-results.md | Consolidated numbers, feedback example and limits |
| docs/evidence/baseline/ | Original report and 42 unchanged traces |
| docs/evidence/followup/ | Targeted report, 12 traces and linked feedback |
| docs/evidence/baseline-instructions/ | Procedures actually used for original baseline |
| docs/evidence/api-spend-final.json | Historical settled request-estimate snapshot |
| docs/evidence/manifest.sha256 | Integrity hashes for archived JSON evidence |
| docs/learning-log.md | Observations versus untested hypotheses |
| docs/demo-script.md | Rehearsal flow and failure handling |
| docs/solution-walkthrough.md | Concise code-reading route |

Official OpenAI background: https://developers.openai.com/api/docs/guides/function-calling ; https://developers.openai.com/api/docs/models/gpt-5.4-mini ; https://developers.openai.com/api/docs/guides/error-codes . These pages were consulted during development/documentation. Pricing and model availability can change; the recorded rates/snapshot describe the historical implementation assumptions.

Official Microsoft background used during the initial decisions: https://learn.microsoft.com/en-us/dotnet/core/versions/ and https://learn.microsoft.com/en-us/agent-framework/overview/ . No installed framework capability should be inferred from those links.

## 20.1 Selected real run IDs

| Run | Why it matters |
| --- | --- |
| fc0add6ccb984f7186c774372e69a938 | First inspected successful real tools-only analysis |
| d8134f5a701b4ffd8069b04da3c06cae | Original invalid review with insufficient rejection diagnostics; cause not reconstructed |
| 7da5ec9a812b41a982db45350effca87 | Useful negative-contribution critique followed by revision and approval |
| bd6f3dbb645d4e14871bd8d51f3ea8f1 | Review overreach/continued criticism reached review_limit |
| bc9d4e5999a240aba5d16f4e96c9d88e | Planned/reviewed complete analysis hit token_limit |
| 85d6a4cc56a74a7b9051f0a378393c3a | Real human feedback replaced targets with 25% and recalculated |

# 21. Documentation creation, self-review and improvements

This Word guide was produced from an initial outline, followed by a structured author self-review against user requirements, actual source files and recorded evidence. It is not claimed as an independent external review. The outline and review matrix are saved separately so the process is inspectable.

The initial outline identified architecture, stack, finances, agents, harness, skills, code, creation, evaluations and phase two. The review found ambiguity around agent identity, planning scope, skill execution, approval meaning, repeated counts, metric quality and limits of technical controls. The final version adds explicit definitions, actual excerpts, diagrams, financial derivations and caveats at the point where each claim is made.

The document deliberately includes failed evidence and implementation limitations beside successful examples. Code excerpts are extracted from existing files; metrics come from preserved reports. Diagrams depict code boundaries rather than imaginary deployed services. A source snapshot manifest and document validation report accompany the generated Word file.

## 21.1 Material improvements over the outline

| Review concern | Final improvement |
| --- | --- |
| Decorative loop explanation | Exact while loop, context/result append, state and terminal paths are mapped to code |
| Agent/skill terminology | Analyst/reviewer contexts, planner mode and explicit Markdown loading are separated |
| Unqualified safety claims | Cooperative timeouts, approximate spend bounds, repetition limits and local persistence weaknesses are explained |
| Metrics without units | Local cases, real workflows, comparative cases and request entries are distinguished |
| Unexplained code | Short excerpts include rationale, business meaning and failure consequences |
| Overstated approval | Reviewer verdict, human-approval metadata and absent commercial workflow are distinct |
| Hidden negative evidence | Original automatic scores, false negatives, invalid reviews and review/token limits are retained |
| Unbounded roadmap | Phase-two work is ranked with acceptance gates and conditions for deferral |
| Difficult long-form reading | Chapter navigation, a glossary, tables, captions, six figures and page numbering are included |

## 21.2 Glossary

| Term | Meaning here |
| --- | --- |
| Agentic loop | Repeated model choice, checked execution and returned result |
| Harness | Application-controlled loop, state, enforcement and observability |
| Tool | Allowlisted executable capability exposed with a schema |
| Runtime skill | Explicitly loaded reusable procedure text for an agent role |
| Reflection | Separate review and bounded analyst revision |
| Planning gate | Stored next action checked and consumed before execution |
| Provenance | Record identity, source, dates and human-maintained validation status |
| Contribution | Revenue minus modeled production/selling costs, not complete net profit |
| Revenue margin | Contribution divided by charged revenue |
| Markup | Price increase relative to production-cost denominator in this implementation |
| Scenario evidence | Stored numeric result with IDs, source metadata and assumptions |
| Clarification | Terminal request for human information rather than an invented price |
| Technical retry | Reattempt after an eligible transport/provider failure |
| Review revision | Changed draft after a critique; different budget from retries |
| Simulated model | Scripted test boundary, not a real provider demonstration |
| Reported token usage | Provider-returned counts; may be unknown after failures |
| Estimated spend reservation | Conservative local authorization accounting before transmission |
| MCP | A possible future tool-exposure protocol; no server exists in this MVP |

## 21.3 Complete current runtime procedures

{{FILE:runtime-skills/analyze-profitability.md}}

{{FILE:runtime-skills/review-pricing.md}}

## 21.4 Source snapshot used by this guide

{{MANIFEST}}

# 22. What a second phase could do

{{FIG:phase-two.png|Figure 6. Proposed phase-two gates. Start with demonstrated quality gaps and verified business inputs; add channels and deployment only when the earlier gates justify them. Nothing in this figure is claimed implemented.}}

A second phase should improve decision quality and product usefulness before adding visible surface area. The baseline already shows that more orchestration is not automatically valuable. Work should therefore start with defects and assumptions that prevent trustworthy use, followed by a measured human workflow. The items below are proposals and decision gates, not a delivery promise or an existing architecture.

## 22.1 Priority one: evaluate outcomes and make review actionable

Replace path-specific missing-data criteria with versioned outcome criteria that accept a grounded early clarification. Keep the original report unchanged and, if historical traces are regraded, publish a separate regrade version. Score narrative omissions, misleading expiry suggestions and unsupported profit claims independently of numeric consistency. Add owner/human annotations and track inter-reviewer disagreement when relevant.

Constrain reviewer requests to fields the analyst can actually edit, or distinguish a product/schema change request from a draft correction. Require a concrete user-impacting defect rather than repetitive editorial preferences. Test whether a focused checker, a user-triggered review or no reviewer gives a better quality/latency/cost trade-off. Do not remove revision bounds to make automatic scores look better.

Acceptance gate: on a larger, held-out set with a declared grader version and common budgets, the chosen review mode must improve an agreed error-detection/usefulness measure without unacceptable false critiques, latency or spend. If it fails that gate, keep Tools as default and defer automatic multi-agent review.

## 22.2 Priority two: validate the financial model with business data

Add per-field source/estimate/pending metadata and a human confirmation workflow. Derive printer depreciation and maintenance rates from documented asset assumptions. Validate whether failures are complete, partial or batch-correlated and which labor steps repeat. Support charges with distinct bases, caps, fixed advertising or settlement rounding only when actual channels require them.

Acceptance gate: Heron can trace every business input and rule to a source/date/status; domain cases independently cover the chosen channel's settlement semantics; unknowns cannot silently become zero; a confirmed example reconciles against a manually reviewed calculation. Synthetic fixtures remain available for public demonstrations.

## 22.3 Priority three: reduce context and preserve reliable state

Replace repeated full evidence payloads with stable references and concise model-facing projections, retaining complete evidence in storage. Evaluate whether planning adds value for ambiguous multi-product or multi-channel tasks rather than simple single-product pricing. Improve no-progress detection with meaningful state changes, and test context reduction without losing assumptions or source identity.

For a service, consider transactional persistence such as SQLite initially or a suitable managed database later, durable spend reservations, explicit schema versions, interrupted-run handling and replay tooling. A database should solve a real state/concurrency requirement; it is not needed merely to rename the prototype enterprise-ready.

Acceptance gate: recovery/replay does not duplicate actions or lose known usage, concurrent runs honor shared authorization, and model context remains bounded under tested long interactions. Uncooperative tool execution needs an isolated worker/process boundary if such tools are introduced.

## 22.4 Priority four: a human decision interface

Only after the analysis is trustworthy, add a small UI for inspecting assumptions, missing inputs, paired scenarios and trace summaries. Human feedback should update authoritative records through validated actions, not by silently treating conversational claims as confirmed facts. Approval should capture actor, exact draft/version, intended action and expiry.

Acceptance gate: a representative business user can review and correct a recommendation, distinguish synthetic from confirmed inputs, and understand whether a discount meets the target. Measure task completion and decision quality before claiming reduced effort. Authentication, authorization, trace privacy and retention become concrete requirements when multiple users or real business data enter.

## 22.5 Conditional expansion: Azure, MCP and commercial writes

Azure hosting can be considered when there is a deployment/user requirement and an explicit cost/security plan. MCP can expose the existing typed tool capabilities when another authorized client needs them; it should not replace local domain validation or permission checks. Re-evaluate SDK/framework options against actual lifecycle, observability and integration needs before migration.

Marketplace price application is a separate product increment. It would need a dedicated approval-bound capability, authenticated actor, exact approved price/rule version, idempotency, least-privilege channel credentials, auditable execution, failure recovery and tests proving that unapproved or stale approvals cannot write. Keep publication disabled until that workflow is designed and explicitly authorized.

Photos, listing text and catalog automation may follow only if pricing outcomes and customer value justify expansion. The next product conversation should ask which repeated business decision is costly enough to automate and what evidence would demonstrate improvement. A stronger prototype is one that earns its next feature through observed value, not one with the largest number of agents.
