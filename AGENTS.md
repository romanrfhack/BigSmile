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
- Protect the continuous clinic workflow: appointment → patient → clinical record → odontogram → treatment plan → quote → billing → follow-up
- Long-term goal: scalable, maintainable, secure, multi-tenant product for multiple clinics

# Mandatory reading order
Before proposing or implementing any change, always read and follow these sources in this order if present:

1. `STATE — BigSmile.md`
2. `AGENTS.md`
3. `README.md`
4. `PROJECT_MAP.md`
5. `REVIEW_RULES.md`
6. `docs/architecture.md`
7. `docs/tenant-model.md`
8. `docs/product-roadmap.md`
9. `docs/contributing.md`
10. `docs/decisions/*.md`

Then inspect the relevant code, tests, migrations, current repository structure and module-specific audit documents.

Treat `STATE — BigSmile.md` as the canonical starting point for current project status.
If these sources diverge from the repository, summarize the drift explicitly and use the canonical state plus actual code to choose the next safe step.
Do not invent implemented modules, completed releases or functional coverage that are not confirmed by code and aligned documentation.

# Working mode
Operate with high autonomy, but not with hidden assumptions.

Default behavior:
- Make decisions autonomously when documentation and code make the correct direction reasonably clear.
- Ask only when a decision is ambiguous, irreversible, business-critical, security-sensitive or materially changes architecture/product direction.
- Prefer progress over unnecessary blocking.
- Prefer small, auditable and reversible changes.
- Do not reimplement a capability that already exists; audit and reconcile it first.

# Decision policy
You may decide without asking when:
- the change is clearly aligned with canonical state and repository docs
- the change is local, safe and reversible
- naming, placement and implementation style are already implied by the architecture
- a missing detail can be resolved without changing product direction

You must ask before proceeding when:
- the decision changes a core product rule
- the decision changes tenant isolation or tenant resolution
- the decision changes authentication/session strategy
- the decision changes authorization scope or role semantics
- the decision introduces external services, paid dependencies or vendor lock-in
- the decision requires secrets, credentials, certificates or infrastructure access not already configured
- the decision deletes or rewrites significant existing work
- the decision conflicts with roadmap priorities
- multiple strong architectural paths remain and the docs do not settle them

# Non-negotiable architecture rules
- Bigsmile is a product, not a throwaway internal app.
- Multi-tenancy is foundational.
- Tenant isolation is non-negotiable.
- Branch is operational scope, not the primary security boundary.
- Do not use manual tenant filtering as the main safety strategy when centralized enforcement is possible.
- Do not weaken tenant safety for convenience.
- Use a modular monolith unless project documentation explicitly changes that decision.
- Respect backend boundaries: Api / Application / Domain / Infrastructure / SharedKernel.
- Respect frontend boundaries: core / shell / shared / features.
- Prefer use-case-oriented application design and lightweight CQRS.
- Avoid giant services and giant frontend pages.
- Keep important invariants close to the domain model.
- Security-first: do not add insecure defaults.
- Code presence does not equal formal release acceptance; require module-specific evidence.

# Stack expectations
- Backend: .NET 10 / ASP.NET Core Web API
- Frontend: Angular 21
- Database: SQL Server
- Architecture: modular monolith
- Product model: multi-tenant SaaS

# Current product direction
This repository is beyond the bootstrap / early foundation stage.

Canonical project status:
- `Foundation / Release 0 base`: completed
- `Pre-auth hardening`: completed
- `Identity + Persistence Foundation`: completed
- `Tenant-Aware Authorization Foundation`: completed
- `Release 1 — Patients`: completed
- `Release 2 — Scheduling`: completed
- `Release 3 — Clinical Records`: completed through accepted slices 3.1 to 3.6
- `Release 4 — Odontogram`: completed through accepted slices 4.1 to 4.4
- `Release 5 — Treatments and Quotes`: completed through accepted slices 5.1 and 5.2
- `Release 6 — Billing`: completed through accepted Release 6.1 — Billing Document Foundation
- Next planned functional phase: `Release 7 — Documents and Dashboard`

Release 4 closure evidence:
- `docs/release-4-odontogram-audit-and-closure.md`
- ADR 007 — `docs/decisions/007-release-4-odontogram-closure.md`

Release 5 closure evidence:
- `docs/release-5-treatments-and-quotes-audit-and-closure.md`
- ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`

Release 6 closure evidence:
- `docs/release-6-billing-audit-and-closure.md`
- ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

Treat Release 4 as the accepted foundational Odontogram boundary:
- explicit creation and `404` when missing
- one tenant-owned/patient-owned odontogram per patient/tenant
- 32 permanent adult FDI teeth
- bounded tooth and surface states
- basic surface findings
- append-only finding add/remove history
- tenant-aware access and explicit `odontogram.read` / `odontogram.write`

Do not reopen advanced Odontogram scope incidentally. Child/mixed dentition, bulk editing, full dental timeline/history, restore/versioning, treatment/diagnosis/document linkage, advanced charting, imaging overlays and AI detection remain future bounded work.

Treat Release 5 as the accepted foundational Treatments/Quotes boundary:
- explicit treatment-plan creation and `404` when missing
- one tenant-owned/patient-owned plan per patient/tenant in the current slice
- basic items with optional adult FDI tooth/surface references
- `Draft` / `Proposed` / `Accepted` lifecycle and accepted-plan immutability
- explicit quote snapshot creation from a non-empty plan
- one quote per plan, fixed `MXN` public path and calculated line/quote totals
- positive pricing gates and accepted-quote immutability
- tenant-aware access with explicit treatment-plan/quote permissions

Do not reopen advanced Treatments/Quotes scope incidentally. Treatment catalog administration, multiple/archived plans, quote regeneration/versioning, multiple quotes, negotiation, taxes, discounts, scheduling linkage, treatment execution/progress, insurance, financing, advanced approvals and Patient Portal access remain future bounded work. Billing is accepted separately through Release 6.1; payments remain deferred.

Treat Release 6 as the accepted foundational Billing boundary:
- explicit Billing creation from an existing accepted quote
- one tenant-owned/patient-owned Billing document per quote
- snapshot-only lines with preserved currency and totals
- bounded `Draft -> Issued` lifecycle and issue metadata
- issued Billing document read-only
- tenant-aware access with explicit `billing.read` / `billing.write`

Do not reopen advanced Billing scope incidentally. Payments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency, accounting and Patient Portal access remain future bounded work. Payment must be designed as a separate aggregate rather than mutable fields on `BillingDocument`.

Repository code also exists in later modules, including Documents, Dashboard and reminders. Until each module receives a specific audit and acceptance pass, classify it as `implemented but not formally accepted/reconciled`.

Phase 2.1 — Patient Intake and Portal Foundation is planned after the initial MVP:
- architecture accepted in ADR 006
- implementation issues #4 to #7 remain open
- PI-1 to PI-4 are not implemented or active
- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability

# Immediate objective
Preserve Releases 1 to 6 and audit the existing `Release 7 — Documents and Dashboard` implementation against the bounded roadmap before accepting or changing it.

Immediate priorities:
- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`
- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes and Billing behavior
- audit Documents and Dashboard domain/application/API/persistence/storage/read models, permissions, frontend, migrations and tests
- distinguish code presence from accepted Release 7 scope
- avoid reopening Billing, TreatmentPlan, TreatmentQuote, Odontogram or Clinical Records through incidental linkage
- keep doctor-based views deferred until provider/doctor assignment is intentionally opened
- keep Phase 2.1 planned and inactive until the MVP gate or explicit reprioritization
- keep privileged/platform paths explicit and auditable
- maintain automated coverage for cross-tenant, branch-aware and allowed platform scenarios
- update canonical documentation whenever a release or architectural decision changes

If a task touches later modules, keep the change bounded and do not assume the module is accepted merely because implementation exists.

# Implementation style
- Prefer clear and conventional naming.
- Prefer focused classes, handlers, services and components.
- Avoid clever abstractions with weak ownership.
- Prefer explicit code over magical code.
- Keep the codebase understandable for human and AI review.
- Preserve consistency over novelty.
- Do not mix functional changes with unrelated visual or documentation refactors.

# Change strategy
For any non-trivial task:
1. inspect canonical docs
2. inspect relevant code, migrations and tests
3. state current situation and any drift
4. identify the smallest correct next step
5. implement in coherent increments
6. run relevant validation
7. document what changed and why
8. leave explicit completed and pending scope

# Documentation obligations
When making important changes, update the relevant documentation.

At minimum, keep these aligned when state changes:
- `STATE — BigSmile.md`
- `AGENTS.md`
- `README.md`
- `PROJECT_MAP.md`
- `REVIEW_RULES.md`
- `docs/architecture.md`
- `docs/tenant-model.md`
- `docs/product-roadmap.md`
- `docs/contributing.md`
- `docs/decisions/*.md`

Create or update an ADR when a change affects:
- architecture style
- tenant model or resolution
- authentication/session strategy
- authorization model
- database tenancy strategy
- major module boundaries
- major frontend state strategy
- important integration patterns
- formal release-scope reconciliation when ambiguity would otherwise remain

If project status advances, update STATE and reconcile base documentation in the same change.

# Reviewability requirements
Every meaningful task should leave enough evidence for later review.

Review packet:
- objective
- files changed
- architectural reasoning
- tenant/security implications
- tests run
- completed scope
- pending scope
- risks and follow-ups

# Validation expectations
Validate as much as available tooling allows:
- backend restore/build
- architecture validation/tests
- backend unit tests
- backend integration tests
- frontend install/build
- frontend unit tests
- e2e tests when relevant
- linting/formatting when configured

If validation cannot run, state exactly what was not validated and why.

# Security rules
- Never commit secrets, tokens, passwords, certificates or private keys.
- Never hardcode sensitive production configuration.
- Never trust tenant scope from unsafe client input.
- Never enforce critical authorization only in frontend.
- Treat patient, clinical, billing and document data as sensitive.
- Audit platform overrides and sensitive operations.

# Tenant safety checklist
Before finalizing any backend or data-access change, verify:
- Is the record tenant-owned?
- Is `TenantId` modeled correctly?
- Does Branch remain subordinate to Tenant?
- Can this read another tenant’s data?
- Can this write another tenant’s data?
- Does authorization align with scope?
- Is any platform bypass explicit and auditable?
- If querying a non-tenant-owned child table directly, is there an explicit tenant-aware join?

# Frontend rules
- Keep pages focused.
- Use facades for feature orchestration.
- Keep HTTP access in data-access layers.
- Avoid business logic hidden inside presentation components.
- Keep visible user copy free of internal release/slice terminology.
- Use shared `--bsm-*` tokens instead of isolated hardcoded colors when touching visual code.
- Optimize for operational UX: patient search/registration, scheduling, clinical capture, odontogram, treatment planning and billing.

# What not to do
- Do not invent modules that contradict the documented architecture.
- Do not introduce infrastructure or vendors without justification.
- Do not refactor unrelated areas while implementing a feature unless necessary.
- Do not mix platform and tenant concerns casually.
- Do not create large mixed-purpose diffs when smaller ones are possible.
- Do not optimize prematurely for microservices or distributed complexity.
- Do not assume the repository is empty or nearly empty.
- Do not mark a release completed without code, tests and aligned documentation.
- Do not reopen closed release scope to make a later module easier.

# Escalation rule
If blocked:
- explain the exact blocker
- explain what was confirmed from docs/code
- explain the valid options
- recommend the best option
- ask only the minimum necessary question

# Default mindset
When in doubt, favor:
- tenant safety
- maintainability
- clear ownership
- small reversible steps
- explicit documentation
- product coherence
- evidence before acceptance

# Final guiding question
For every meaningful decision, ask:

“Does this make Bigsmile stronger as a secure, maintainable, multi-tenant SaaS product for dental clinics?”
