# Documentation self-review and improvement plan

This is an author self-review against the repository and recorded evidence, not an independent external review. The initial outline is preserved as documentation-outline-v0.1.md. The final Word guide incorporates the actions below.

| Initial-outline gap or ambiguity | Why it matters | Improvement required in final guide |
| --- | --- | --- |
| An agent can be confused with a model, class or tool | Heron must describe actual roles accurately | Define analyst/reviewer by instructions, context, allowed tools and output contract; identify planning as an analyst mode |
| Generic loop arrows can conceal implementation | A decorative diagram does not prove an agentic loop | Map each arrow to RunAsync, CallModelAsync, ExecuteAsync and context append; include exact stop paths |
| Runtime Markdown may appear automatically executable | Skills terminology can overstate capability | Explain explicit two-file loader, SHA-256 version, no discovery, no AGENTS.md runtime loading |
| Approval wording can imply a commercial approval service | No such service exists | Separate reviewer approval, draft human-approval flag and absent mutation workflow |
| Two unrelated counts both equal 57 | Easy to misrepresent test coverage | Separate local assertion cases, real workflows, evaluation cases and 280 request records |
| Test scores can be mistaken for accuracy | Grader false negatives and sample size matter | Show original scores, categorize failures, retain raw reports and discuss hindsight/causal limits |
| Code can be presented without reasoning | Copying files does not teach architecture | Use small actual excerpts with line ranges, responsibility, rationale and failure example |
| Decimal can be portrayed as sufficient financial correctness | Incomplete cost models still mislead | Derive price/failure equations, document units, base assumptions and excluded costs |
| Timeouts and spend control can sound absolute | Cooperative timeouts and estimates have limits | Explain async cancellation limits, token overshoot, persistent reservations and conditional pricing bounds |
| Context separation may imply independent truth | Same-model reviewers can repeat errors or overreach | Describe exact fresh context and observed useful/overreaching findings |
| Planner may be overstated as general planning | Implementation is a next-action gate | Explain selected-action consumption, dependencies, clarification escape and no generic scheduling |
| Source tags may imply authentic verification | Records are human-maintained demo data | Describe record-level Demonstration/Confirmed/Unverified and null pending values; no signed sources |
| Creation process may overclaim Heron's review | Portfolio attribution must be honest | Separate Heron's product/provider/budget decisions and terminal runs from Codex implementation/inspection |
| Phase two can become an unbounded feature list | Principal product decisions require priorities | Rank by value/risk, define acceptance gates and stop conditions, defer UI/MCP without benefit evidence |
| A long Word document can be difficult to use | Reader needs navigation and readability | Add cover, chapter navigation, glossary, six labeled figures, tables, code styles, page numbers and reading routes |
| Evidence can be difficult to find outside this chat | The document must be standalone | Include numeric examples, trace IDs, file/evidence index, run commands and source snapshot metadata |

## Completion criteria for the document

Every requested topic has a section; implemented/evaluated/planned distinctions are explicit; all code snippets come from existing files; all numeric evaluation tables come from preserved reports; figures match code; no credentials are included; the generated .docx package parses and passes integrity checks. Any unavailable full Word page rendering must be reported rather than claimed.

## Final verification outcome

The final guide has 22 chapters, approximately 14,363 source words including extracted code/procedures, six original technical diagrams, 22 tables including the cover callout and 17 real source excerpts. All 17 automated document checks passed. Word ZIP/XML relationships, bookmarks, embedded image descriptions, source hashes, snippet ranges, documented local case count, credential-pattern scan and readable macOS text extraction were verified. The accidental python-docx template creation date was corrected to the actual generation timestamp. A final diagram review corrected a label/arrow intersection.

Six diagrams were visually inspected as a montage and the main-loop figure at full size. Full Word pagination could not be rendered: macOS Quick Look failed sandbox initialization. The validation report records this limitation; no full-page Word visual verification is claimed. No application code, runtime instructions, credentials or paid-model budget were changed by documentation generation.
