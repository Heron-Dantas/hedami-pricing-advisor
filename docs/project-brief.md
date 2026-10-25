# Project brief

## Owner and purpose

Heron Dantas, owner of HEDAMI Labs and Chapter Lead / Tech Lead at Volvo Group, is developing this personal project to explore practical applications of agentic AI in his 3D printing business.

The project builds on his longstanding interest in using AI to support product pricing, content creation and business operations. It combines his C#/.NET, Angular, Azure and software architecture experience with hands-on experimentation in tool use, agent orchestration, evaluation and human oversight.

Heron defines the business objectives, scope and architectural direction. Codex supports implementation, testing and documentation. AI-generated contributions remain subject to human review, and implementation claims are grounded in recorded evidence.

## Product

Reduce product registration effort, starting with pricing. Given a request such as “Analyze the headset stand, calculate margin scenarios and explain whether I can offer 10% off,” an analyst should retrieve product data and applicable rules, ask for missing essentials, call a deterministic calculator, adapt its plan and produce an inspectable recommendation. Human feedback should trigger another analysis.

Photos, listings, catalog management, marketplace publication and price mutation are outside this MVP. All price application requires human approval; no application tool is planned for the initial demo.

## Milestones

1. Saturday, October 24: executable domain calculator, unit tests, documented assumptions and local project setup.
2. Saturday, after provider confirmation: typed product/rule/calculation tools, provider adapter, single-analyst tool loop, persisted real LLM run and explicit missing-data response.
3. Sunday, October 25: bounded harness with validation, permissions, cancellation, timeouts, no-progress detection, separate retry/review budgets and evaluation evidence. Complete tools-only baseline before adding an independent reviewer.
4. Sunday, remaining time: compare bounded reflection and adaptive planning/collaboration if implemented; write demo script, architecture explanation, limitations, learning log and contribution record. Freeze a reproducible demo before adding UI or MCP.

These were the initial delivery targets; see the immediate-continuation status below for implemented capabilities. If time is constrained, prioritize verified calculations and real tools-only evidence over reviewer or web UI.

## Boundaries

Domain arithmetic must use decimal and documented units/rounding. Tool and rule validation must distinguish facts, estimates and pending information. Rates require identity, source, effective dates and validation status. Unknown inputs cannot silently become zero. Do not claim net profit from incomplete overhead allocation.

Runtime layers will separate business logic, tools, model integration, harness/state, instructions and evaluations. Introduce each boundary when used. Observable plans, tool actions, results and concise rationales are evidence; private chain of thought is not required.

## Real model prerequisites

Heron selected OpenAI API and confirmed an existing account on October 24, 2026. The REST adapter targets Responses API with configurable gpt-5.4-mini-2026-03-17 initially. Heron configured the application key locally and authorized USD 2 total using synthetic data. Pending: confirmed account/model/billing access through a real run. A first attempt failed at DNS resolution in the restricted session, before any provider response. Configure credentials locally through environment variables or a suitable secret store after selecting the provider. Codex login is not an application credential. Do not paste secrets into chat or commit them. Verify official provider documentation before selecting SDK/API versions.

## Delivery artifacts

Executable repository and reproduction instructions; actual run traces; demonstration script; English portfolio narrative explaining product, architecture, patterns, evaluations, lessons and limits; decision and contribution records. The prototype is not production-ready. Publication and deployment require Heron's instruction.

## Immediate continuation (October 24)

Heron requested continuation across all items now; milestone dates are deadlines, not waiting periods. The application layer, tools, OpenAI adapter, bounded harness, reviewer, enforced next-action planning, live evaluation runner and portfolio draft are implemented. Local scripted verification is recorded separately from live model evidence: the first smoke and 42-run baseline are now recorded. Targeted follow-up and Heron's review remain separate. No UI/MCP work should displace that real validation.
