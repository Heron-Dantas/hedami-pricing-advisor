# HEDAMI Pricing Advisor — initial documentation outline (v0.1)

Purpose: give Heron Dantas a standalone study document and an evidence-based portfolio narrative. Language: English, matching the portfolio agreement. This is an initial outline, not the final claim of completeness.

## 1. Executive explanation
Define the pricing problem, user, value hypothesis, console-only scope, synthetic-data boundary and recommendation-only behavior. Explain why an LLM is useful for interpreting an objective but does not own money calculations.

## 2. Architecture and stack
Map console, domain and application projects. Show components and model/data boundaries. List .NET SDK/runtime, C#, System.Text.Json, HttpClient, OpenAI Responses, JSON files and xUnit packages. Explain why no agent framework, web UI, database or MCP was introduced.

## 3. Financial rules
Describe typed inputs, null versus zero, decimal arithmetic, material/energy/printer/labor/packaging/failure provision, margin versus markup, selling charges, list-price equation, discounts and rounding. Work through the real synthetic example.

## 4. Agent roles
Describe analyst and independent reviewer with their context, allowed tools, inputs, outputs, limits and failure modes. Explain that planner is an analyst mode rather than a third independently instantiated agent.

## 5. Tool contracts
Document discovery, product/rule reads, calculate_scenarios, submit_recommendation, request_clarification, update_plan and reviewer submit_review. Show strict schemas and local validation.

## 6. Agentic loops and harness
Trace model -> tool request -> validation -> execution -> result -> model. Detail reflection/revision, enforced planning and human linked feedback loops. Locate while(true), CallModelAsync, permission checks, context controls, cancellation, budgets, repetitions, persistence and status handling.

## 7. Runtime skills
Explain both Markdown procedures, their explicit loader and hash. Distinguish development skills and AGENTS.md from runtime-loaded files. State that there is no generic skill discovery/framework behavior.

## 8. Code walkthrough
Use short real excerpts with repository paths and line references. Explain responsibilities, design choices and limitations at each boundary. Avoid copying entire files without interpretation.

## 9. Creation process
Record environment inspection, small calculator slice, provider choice, actual tool loop, sandbox/DNS constraints, terminal handoff, baseline evaluation, diagnosis, targeted changes and follow-up. Attribute Heron and Codex work accurately.

## 10. Evaluations and evidence
Separate local assertion cases, mocked/scripted behavior and real workflows. Present the 42-case baseline, 12-case follow-up, human-feedback example and estimated API cost. Preserve failures and grader defects; do not claim production reliability or productivity gain.

## 11. Running and troubleshooting
Give prerequisites, commands, .env handling, budgets, trace locations, historical fixture date, status meanings and differences among DNS, HTTP authentication, quota and agent/tool errors.

## 12. Limitations and second phase
Prioritize outcome-based graders, actionable review, context efficiency, confirmed business rules, approved human workflows, persistent state, observability, UI/Azure/MCP only with evidence. Define acceptance criteria and product questions rather than promise unimplemented features.

## 13. Interview preparation and appendices
Provide glossary, questions to defend decisions, code/evidence index, instruction text, references and a documented self-review/improvement log.

## Planned figures
A. Component architecture. B. Main tool-use loop. C. Analyst/reviewer contexts and bounded reflection. D. Adaptive plan gate. E. Human-linked feedback. F. Phase-two gated roadmap.

## Initial review questions
- Are the two different counts of 57 unambiguous?
- Can a reader locate each loop in code without a framework being assumed?
- Are reviewer/planner limitations visible beside benefits?
- Does the document explain the actual meaning of human approval in a no-write MVP?
- Are skills explicitly loaded, rather than inferred from Markdown filenames?
- Are examples and metrics traceable to actual files without access to this chat?
- Are calculations and demonstration rates clearly separated from current marketplace facts?
- Are second-phase ideas labeled and measurable?
