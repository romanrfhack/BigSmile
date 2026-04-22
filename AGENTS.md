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

Then inspect the relevant code, tests, and current repository structure.

Treat `STATE — BigSmile.md` as the canonical starting point for current project status.
If these sources diverge from the repository, summarize the drift explicitly and use the canonical state plus the actual codebase to choose the next safe step.
Do not invent implemented modules, completed releases, or functional coverage that are not confirmed by code and aligned documentation.

# Working mode
Operate with high autonomy, but not with hidden assumptions.

Default behavior:
- Make decisions autonomously when the documentation and codebase make the correct direction reasonably clear.
- Ask the user only when a decision is ambiguous, irreversible, business-critical, security-sensitive, or when multiple valid options would materially change the architecture or product direction.
- Prefer progress over unnecessary blocking.
- Prefer small, auditable, reversible changes.

# Decision policy
You may decide without asking when:
- The change is clearly aligned with `STATE — BigSmile.md`, `README.md`, and the project docs
- The change is local, safe, and reversible
- Naming, file placement, folder structure, and implementation style are already implied by the architecture
- A missing detail can be resolved using the documented principles without changing product direction

You must ask before proceeding when:
- The decision changes a core product rule
- The decision affects tenant isolation rules
- The decision changes tenant resolution strategy
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
This repository is beyond the initial bootstrap / early foundation stage.

Canonical project status:
- `Foundation / Release 0 base`: completed
- `Pre-auth hardening`: completed
- `Identity + Persistence Foundation`: completed
- `Tenant-Aware Authorization Foundation`: completed
- `Release 1 — Patients`: completed
- `Release 2 — Scheduling`: completed
- `Release 3 — Clinical Records`: preserved through accepted slices `Release 3.1 — Clinical Record Foundation`, `Release 3.2 — Basic Diagnoses Foundation`, `Release 3.3 — Clinical Timeline Read Model`, and `Release 3.4 — Clinical Snapshot Change History`
- `Release 4 — Odontogram`: preserved through accepted slices `Release 4.1 — Odontogram Foundation`, `Release 4.2 — Odontogram Surface Foundation`, `Release 4.3 — Basic Dental Findings Foundation`, and `Release 4.4 — Dental Findings Change History`
- `Release 5 — Treatments and Quotes`: in progress
- accepted slices: `Release 5.1 — Treatment Plan Foundation` and `Release 5.2 — Quote Basics`

Current active phase:
- `Release 5 — Treatments and Quotes`

Treat the repository as having an established technical and architectural foundation, but not as functionally complete.
Do not assume roadmap releases `Patients`, `Scheduling`, `Clinical Records`, `Odontogram`, `Treatments and Quotes`, `Billing`, or `Documents and Dashboard` are implemented or closed unless the actual codebase and aligned documentation explicitly prove it.
Within `Release 3 — Clinical Records`, do not assume slices beyond the accepted `Release 3.1 — Clinical Record Foundation`, `Release 3.2 — Basic Diagnoses Foundation`, `Release 3.3 — Clinical Timeline Read Model`, and `Release 3.4 — Clinical Snapshot Change History` are implemented unless the actual codebase and aligned documentation explicitly prove it.
Within `Release 4 — Odontogram`, do not assume slices beyond the accepted `Release 4.1 — Odontogram Foundation`, `Release 4.2 — Odontogram Surface Foundation`, `Release 4.3 — Basic Dental Findings Foundation`, and `Release 4.4 — Dental Findings Change History` are implemented unless the actual codebase and aligned documentation explicitly prove it.
Within `Release 5 — Treatments and Quotes`, do not assume slices beyond the accepted `Release 5.1 — Treatment Plan Foundation` and `Release 5.2 — Quote Basics` slices are implemented unless the actual codebase and aligned documentation explicitly prove it.
After the current phase, continue following `docs/product-roadmap.md`.

# Immediate objective
Help preserve the completed authorization foundation, the completed `Release 1 — Patients` module, the completed `Release 2 — Scheduling` module, the accepted `Release 3.1 — Clinical Record Foundation`, `Release 3.2 — Basic Diagnoses Foundation`, `Release 3.3 — Clinical Timeline Read Model`, `Release 3.4 — Clinical Snapshot Change History`, `Release 4.1 — Odontogram Foundation`, `Release 4.2 — Odontogram Surface Foundation`, `Release 4.3 — Basic Dental Findings Foundation`, `Release 4.4 — Dental Findings Change History`, and the accepted `Release 5.1 — Treatment Plan Foundation` and `Release 5.2 — Quote Basics` slices while continuing `Release 5 — Treatments and Quotes` in bounded, auditable slices aligned with `STATE — BigSmile.md`, the repository documentation, and the actual codebase.

Immediate priorities:
- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`
- keep authorization decisions based on scope (`platform` / `tenant` / `branch`), membership, role, and permission
- preserve tenant isolation across identity, persistence, and access enforcement while new business modules appear
- preserve the closed Scheduling release without reopening scope casually
- preserve the accepted Release 3.1 scope: explicit clinical record creation, base snapshot, current allergies, and append-only notes
- preserve the accepted Release 3.2 scope: basic diagnoses on existing clinical records, explicit diagnosis add/resolve operations, diagnosis read-model inclusion, basic non-coded diagnosis data, and no full timeline / odontogram / treatments / documents yet
- preserve the accepted Release 3.3 scope: clinical timeline read model inside the existing clinical record, built from notes and diagnoses only, with no new endpoint, no new table, and no cross-module timeline
- preserve the accepted Release 3.4 scope: bounded snapshot history for the base clinical snapshot, with an initial entry on explicit clinical record creation, effective-change entries for background / medications / allergies only, separation from the Release 3.3 timeline, and no restore / full versioning / rich diff
- preserve the accepted Release 4.1 scope: explicit odontogram creation, exactly one odontogram per patient per tenant, permanent adult FDI tooth numbering, tooth-level current status only, minimal audit metadata, and no autocreation
- preserve the accepted Release 4.2 scope: minimal O/M/D/B/L surfaces on top of the accepted odontogram foundation, surface-level current status only, explicit surface updates, no tooth-status auto-aggregation, and no complex findings/treatment linkage/surface history
- preserve the accepted Release 4.3 scope: basic surface findings on top of the accepted odontogram foundation and surface foundation, explicit add/remove operations, a minimal finding catalog `Caries` / `Restoration` / `MissingStructure` / `Sealant`, enriched odontogram reads, and no findings history, treatment linkage, diagnosis linkage, or auto-aggregation from findings
- preserve the accepted Release 4.4 scope: bounded findings change history for the accepted basic findings only, `FindingAdded` / `FindingRemoved` entries on effective add/remove operations, root-level odontogram read-model enrichment, UI history inside the existing odontogram page filtered by selected surface, separation from any future dental timeline, and no restore / full odontogram versioning / treatment linkage / diagnosis linkage / surface history
- preserve the accepted Release 5.1 scope: explicit treatment plan creation, `GET` returning `404` when missing, no autocreation, exactly 1 active treatment plan por Patient por Tenant, basic add/remove of treatment items, bounded `Draft` / `Proposed` / `Accepted` plan status, optional adult FDI tooth/surface reference per item, and no pricing / formal quotes / billing linkage / scheduling linkage / treatment execution tracking / versioning in this slice
- preserve the accepted Release 5.2 scope: explicit quote creation from the existing treatment plan, `GET` returning `404` when missing, no autocreation, exactly 1 quote por TreatmentPlan, snapshot-only copy of current treatment plan items, `CurrencyCode` fijo/simple en este slice, line-level `UnitPrice`, basic `LineTotal` / `QuoteTotal`, bounded `Draft` / `Proposed` / `Accepted` quote status, pricing positivo preservado al estar `Proposed`, revalidación de precios positivos en `Proposed -> Accepted`, read-only quote once `Accepted`, and no discounts / taxes / billing linkage / regenerate workflow / multi-quote / versioning in this slice
- preserve the current clinical access restriction: `clinical.read` / `clinical.write` belong to `PlatformAdmin` and `TenantAdmin`; `TenantUser` does not receive clinical permissions in this phase
- preserve the current odontogram access restriction: `odontogram.read` / `odontogram.write` belong to `PlatformAdmin` and `TenantAdmin`; `TenantUser` does not receive odontogram permissions in this phase
- preserve the current treatment-plan access restriction: `treatmentplan.read` / `treatmentplan.write` belong to `PlatformAdmin` and `TenantAdmin`; `TenantUser` does not receive treatment-plan permissions in this phase
- preserve the current treatment-quote access restriction: `treatmentquote.read` / `treatmentquote.write` belong to `PlatformAdmin` and `TenantAdmin`; `TenantUser` does not receive treatment-quote permissions in this phase
- keep doctor-based views deferred until a dedicated provider/doctor assignment slice is intentionally opened
- keep privileged/platform paths explicit and auditable
- maintain automated coverage for forbidden cross-tenant reads/writes, branch-aware restrictions, and permitted platform override scenarios
- update documentation and ADRs whenever tenant resolution, auth/session, authorization, or patient-module behavior materially changes

If a task touches later functional roadmap modules, keep the change bounded and do not assume unfinished business modules already exist.

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
- tenant model
- tenant resolution strategy
- authentication/session strategy
- authorization model
- database tenancy strategy
- major module boundaries
- major frontend state strategy
- important integration patterns

If project status advances to a new completed phase, reflect it in `STATE — BigSmile.md` and reconcile the rest of the base documentation accordingly.
Do not leave `AGENTS.md` or other base docs describing the repository as bootstrap-stage once the canonical state has moved forward.

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
- architecture validation / architecture tests
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
- Do not assume the repository is empty or nearly empty as the default working condition
- No functional roadmap release should be treated as completed unless the actual codebase and aligned documentation explicitly prove it

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
