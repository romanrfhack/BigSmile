# AGENTS.md

# Project
Bigsmile

# Mission
Bigsmile is a SaaS platform for managing dental clinics and private practices.

The product must be built as a commercial multi-tenant SaaS product from day one.

Core business definitions:
- Tenant = clinic or private practice customer
- Branch = internal location that belongs to a tenant

Initial product direction:
- Start with a strong operational MVP
- Early functional focus: patients / clinical records first, then continue through the roadmap
- Long-term goal: scalable, maintainable, secure, multi-tenant product for multiple clinics

# Mandatory reading order
Before proposing or implementing any change, always read and follow these files if present:

1. README.md
2. docs/architecture.md
3. docs/tenant-model.md
4. docs/product-roadmap.md
5. docs/contributing.md
6. docs/decisions/*.md

If any of these documents conflict with local code assumptions, prefer the documented architecture and report the conflict.

# Working mode
Operate with high autonomy, but not with hidden assumptions.

Default behavior:
- Make decisions autonomously when the documentation and codebase make the correct direction reasonably clear.
- Ask the user only when a decision is ambiguous, irreversible, business-critical, security-sensitive, or when multiple valid options would materially change the architecture or product direction.
- Prefer progress over unnecessary blocking.
- Prefer small, auditable, reversible changes.

# Decision policy
You may decide without asking when:
- The change is clearly aligned with README.md and docs/*
- The change is local, safe, and reversible
- Naming, file placement, folder structure, and implementation style are already implied by the architecture
- A missing detail can be resolved using the documented principles without changing product direction

You must ask before proceeding when:
- The decision changes a core product rule
- The decision affects tenant isolation rules
- The decision changes authentication/session strategy
- The decision changes authorization scope or role semantics
- The decision introduces external services, paid dependencies, or vendor lock-in
- The decision requires secrets, credentials, certificates, or infrastructure access not already configured
- The decision would delete or rewrite significant existing work
- The decision conflicts with roadmap priorities
- The decision has more than one strong architectural path and the docs do not settle it

# Non-negotiable architecture rules
- Bigsmile is a product, not a throwaway internal app.
- Multi-tenancy is foundational.
- Tenant isolation is non-negotiable.
- Branch is an operational scope, not the primary security boundary.
- Do not implement manual tenant filtering as the main safety strategy when centralized enforcement is possible.
- Do not weaken tenant safety for convenience.
- Use a modular monolith architecture unless the project documentation explicitly changes that decision.
- Respect backend boundaries: Api / Application / Domain / Infrastructure / SharedKernel.
- Respect frontend boundaries: core / shell / shared / features.
- Prefer use-case oriented application design (vertical slices / lightweight CQRS).
- Avoid giant services and giant frontend pages.
- Keep important business invariants close to the domain model.
- Security-first: do not add insecure defaults.

# Stack expectations
- Backend: .NET 10 / ASP.NET Core Web API
- Frontend: Angular 21
- Database: SQL Server
- Architecture: modular monolith
- Product model: multi-tenant SaaS

# Current product direction
Build in this order unless the documentation changes:

Release 0:
- foundation / bootstrap
- solution structure
- multi-tenancy base
- auth / authz
- tenant and branch context
- error handling
- auditing
- logging
- architecture tests
- CI foundation

Then continue according to docs/product-roadmap.md.

# Immediate objective
Help bootstrap the project foundation in alignment with the repository documentation.

If the repository is empty or nearly empty:
1. audit current files
2. summarize what exists
3. propose a minimal foundation plan
4. implement the foundation in small steps
5. document decisions and results for review

# Implementation style
- Prefer clear and conventional naming
- Prefer focused classes, handlers, services, and components
- Avoid clever abstractions with weak ownership
- Prefer explicit code over magical code
- Keep the codebase understandable for future review by humans and CODEX
- Preserve consistency over novelty

# Change strategy
For any non-trivial task:
1. inspect the relevant code and docs
2. state the current situation
3. identify the smallest correct next step
4. implement in small coherent increments
5. run relevant validation
6. document what changed and why

# Documentation obligations
When making important changes, update the relevant documentation.

At minimum, keep these aligned:
- README.md
- docs/architecture.md
- docs/tenant-model.md
- docs/product-roadmap.md
- docs/contributing.md
- docs/decisions/*.md

Create or update an ADR when a change affects:
- architecture style
- tenant model
- authentication/session strategy
- authorization model
- database tenancy strategy
- major module boundaries
- major frontend state strategy
- important integration patterns

# Reviewability requirements
Every meaningful task should leave enough evidence for later review by humans or CODEX.

When completing work, produce a concise review packet including:
- objective
- files changed
- architectural reasoning
- tenant/security implications
- tests run
- open questions
- risks or follow-up items

# Validation expectations
Always validate the change as much as possible with available tooling.

Expected validation types:
- backend build
- frontend build
- unit tests
- integration tests
- architecture tests
- linting / formatting
- e2e tests when relevant

If validation cannot be run, explicitly state what was not validated and why.

# Security rules
- Never commit secrets, tokens, passwords, certificates, or private keys
- Never hardcode sensitive configuration in versioned files
- Never trust tenant scope from unsafe client input
- Never enforce critical authorization only in frontend
- Always treat patient, clinical, payment, and document data as sensitive
- Audit platform overrides and sensitive operations

# Tenant safety checklist
Before finalizing any backend or data-access change, verify:
- Is the record tenant-owned?
- Is TenantId modeled correctly?
- Does branch remain subordinate to tenant?
- Can this read another tenant’s data?
- Can this write another tenant’s data?
- Does authorization align with scope?
- Does a platform bypass exist, and if so, is it explicit and auditable?

# Frontend rules
- Keep pages focused
- Use facades for feature orchestration
- Keep HTTP access in data-access layers
- Avoid business logic hidden inside presentation components
- Optimize for operational UX:
  - fast patient search
  - fast patient registration
  - fast appointment creation
  - clear clinical workflows
  - low-friction treatment planning
  - quick payment registration

# What not to do
- Do not invent modules that contradict the documented architecture
- Do not introduce infrastructure or vendors without justification
- Do not refactor unrelated areas while implementing a feature unless necessary
- Do not mix platform and tenant concerns casually
- Do not create large mixed-purpose diffs when smaller ones are possible
- Do not optimize prematurely for microservices or advanced distributed complexity

# Escalation rule
If blocked, do not stop at a vague message.

Instead:
- explain the exact blocker
- explain what was confirmed from docs/code
- explain the options
- recommend the best option
- ask only the minimum necessary question

# Default mindset
When in doubt, favor:
- tenant safety
- maintainability
- clear boundaries
- small reversible steps
- explicit documentation
- product coherence

# Final guiding question
For every meaningful decision, ask:

“Does this make Bigsmile stronger as a secure, maintainable, multi-tenant SaaS product for dental clinics?”
