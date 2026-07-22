# PROJECT_MAP.md

# Project
Bigsmile

# Purpose of this file
This file gives contributors and agents a fast operational map of the repository.

It answers:
- what this repository is for
- how it is organized
- which module owns what
- which dependencies are allowed
- where new code should go
- which areas are stable, accepted, planned or still unreconciled
- how to avoid architectural drift

This file complements:
- `STATE — BigSmile.md`
- `README.md`
- `AGENTS.md`
- `REVIEW_RULES.md`
- `docs/architecture.md`
- `docs/tenant-model.md`
- `docs/product-roadmap.md`
- `docs/contributing.md`
- `docs/decisions/*.md`

If there is a conflict, start with `STATE — BigSmile.md`, then accepted ADRs and architecture/tenant documents.

---

## 1. Product Summary

Bigsmile is a SaaS platform for managing dental clinics and private practices.

Core definitions:
- **Tenant** = clinic or private practice customer
- **Branch** = internal location belonging to a tenant

The system is a multi-tenant SaaS product from day one.

Product direction:
- build a strong operational MVP
- protect tenant isolation
- preserve maintainability
- optimize operational UX
- grow through explicit bounded contexts and accepted release slices

---

## 2. Current Repository State

This repository is beyond early foundation/bootstrap.

Canonical project status:

* **Foundation / Release 0 base:** completed
* **Pre-auth hardening:** completed
* **Identity + Persistence Foundation:** completed
* **Tenant-Aware Authorization Foundation:** completed
* **Release 1 — Patients:** completed
* **Release 2 — Scheduling:** completed
* **Release 3 — Clinical Records:** completed through accepted slices 3.1 to 3.6
* **Release 4 — Odontogram:** completed through accepted slices 4.1 to 4.4
* **Next planned functional phase:** **Release 5 — Treatments and Quotes**

### Release 4 closure evidence

- Audit: `docs/release-4-odontogram-audit-and-closure.md`
- ADR 007: `docs/decisions/007-release-4-odontogram-closure.md`

Accepted Odontogram foundation:
- explicit creation and `404` when missing
- exactly one odontogram per patient/tenant
- 32 permanent adult FDI teeth
- bounded tooth states
- five `O/M/D/B/L` surfaces and bounded surface states
- basic surface findings
- append-only finding add/remove history
- `odontogram.read` / `odontogram.write`
- tenant-aware aggregate access and automated coverage

### Existing-code reconciliation

Functional code also exists in later roadmap modules:
- Treatments/Quotes
- Billing
- Documents
- Dashboard
- reminders/manual reminders

Those modules remain `implemented but not formally accepted/reconciled` until a specific audit and acceptance pass occurs. Code, routes, permissions, migrations and tests are evidence to inspect, not automatic release closure.

### Phase 2.1

Patient Intake and Portal Foundation is planned after the initial MVP:
- ADR 006 accepted
- parent issue #2
- PI-1 to PI-4 tracked in #4 to #7
- no patient-facing implementation opened
- no displacement of Release 5

### Current expected priority

1. Preserve Releases 1 to 4.
2. Audit existing Treatments/Quotes code against the bounded Release 5 roadmap.
3. Open or accept only slices supported by domain, API, persistence, authorization, frontend and tests.
4. Keep advanced Odontogram and cross-module linkage deferred.
5. Keep Phase 2.1 inactive until its roadmap gate or explicit reprioritization.

---

## 3. High-Level Repository Layout

```text
/
  STATE — BigSmile.md
  README.md
  AGENTS.md
  PROJECT_MAP.md
  REVIEW_RULES.md
  docs/
  backend/
  frontend/
  .github/
```

Root intent:
- `STATE — BigSmile.md` -> canonical current state
- `README.md` -> product/repository overview
- `AGENTS.md` -> operating rules for agents/contributors
- `PROJECT_MAP.md` -> operational repository map
- `REVIEW_RULES.md` -> review and validation standard
- `docs/` -> architecture, product, release and operational decisions
- `backend/` -> .NET backend
- `frontend/` -> Angular frontend
- `.github/` -> CI/CD and repository automation

---

## 4. Documentation Map

Core documents:
- `docs/architecture.md`
- `docs/tenant-model.md`
- `docs/product-roadmap.md`
- `docs/frontend-ux-guidelines.md`
- `docs/contributing.md`
- `docs/decisions/*.md`

Release-specific evidence:
- Release 3 physical-form mapping: `docs/release-3-clinical-records-form-mapping.md`
- Release 4 audit/closure: `docs/release-4-odontogram-audit-and-closure.md`
- Patient Intake plan: `docs/patient-intake-and-portal-plan.md`

Update documentation when changes affect:
- project state
- architecture or module boundaries
- tenant model
- auth/session or authorization
- roadmap priority
- public/clinical workflows
- operational/deployment conventions

Create or update an ADR for:
- architecture style
- tenant resolution/database tenancy
- auth/session/authorization
- major module ownership
- major integration/state patterns
- formal release-scope reconciliation when ambiguity exists

---

## 5. Backend Map

```text
backend/
  src/
    BigSmile.Api/
    BigSmile.Application/
    BigSmile.Domain/
    BigSmile.Infrastructure/
    BigSmile.SharedKernel/
  tests/
    BigSmile.UnitTests/
    BigSmile.IntegrationTests/
    BigSmile.ArchitectureTests/
```

### BigSmile.Api
Owns:
- HTTP endpoints and transport contracts
- middleware/composition
- authentication and authorization entry points
- API versioning/transport concerns

Must not own:
- core business rules
- persistence logic
- domain invariants

### BigSmile.Application
Owns:
- use cases
- commands/queries
- validation/orchestration
- transaction boundaries
- tenant-aware execution

Must not become:
- a generic-service dumping ground
- infrastructure-specific logic
- a substitute for domain invariants

### BigSmile.Domain
Owns:
- aggregates/entities/value objects
- invariants and lifecycle rules
- domain events where appropriate

Important aggregates currently include:
- `Patient`
- `Appointment`
- `ClinicalRecord`
- `Odontogram`
- `TreatmentPlan`
- `TreatmentQuote`
- `BillingDocument`

Must not depend on Infrastructure, Api or UI concerns.

### BigSmile.Infrastructure
Owns:
- EF Core/DbContext
- repositories
- migrations/configuration
- file storage/integrations
- auditing persistence
- tenant-resolution infrastructure

Must not hide tenant bypasses or absorb application logic.

### BigSmile.SharedKernel
Owns intentionally small cross-cutting primitives and contracts.

### Tests
- UnitTests -> isolated domain/application/controller behavior
- IntegrationTests -> persistence, tenant and service behavior
- ArchitectureTests -> structural rules

---

## 6. Frontend Map

```text
frontend/src/app/
  core/
  shell/
  shared/
  features/
    auth/
    platform/
    dashboard/
    patients/
    scheduling/
    clinical-records/
    odontogram/
    treatments/
    billing/
    documents/
    reporting/
    settings/
```

### core/
Owns auth/session, HTTP infrastructure, interceptors, guards, tenant context, global error handling and localization state.

### shell/
Owns authenticated layout, navigation, header, branch selector and layout composition.

### shared/
Owns genuinely reusable UI, directives, pipes and generic form/helper primitives.

### features/
Each business feature should prefer:

```text
features/<feature>/
  pages/
  components/
  facades/
  data-access/
  models/
```

Rules:
- pages coordinate route-level work
- facades orchestrate feature state
- data-access owns HTTP
- components prefer inputs/outputs
- shared must not absorb feature business logic
- critical authorization remains backend-enforced

---

## 7. Business Module Map

### Platform
Tenants, branches, plans, feature flags, branding, tenant settings and platform administration.

### Identity
Staff users, roles, permissions, memberships, sessions and access policies.

Future patient identity is a separate Phase 2.1 boundary under ADR 006; it must not reuse staff membership semantics.

### Patients
Patient registration/profile, contact, responsible party, search and basic clinical alerts.

### Scheduling
Appointments, calendar views, blocked slots, states, rescheduling and no-show tracking. Doctor-based views remain deferred until provider assignment exists.

### Clinical
Clinical records, snapshot, allergies, notes, diagnoses, bounded timeline/history, medical questionnaire and encounters/vitals.

### Odontogram
Accepted foundation:
- chart aggregate
- tooth/surface state
- basic surface findings
- bounded finding history

Future advanced charting/linkage remains outside Release 4.

### Treatments
Treatment plans and quotes. Code exists but Release 5 still requires formal audit/acceptance.

### Billing
Billing documents and future financial workflows. Formal Release 6 acceptance remains pending.

### Documents
Patient attachments and authorized storage/access. Formal Release 7 acceptance remains pending.

### Notifications
Manual reminders/templates exist inside Scheduling, but automated providers/jobs/retry workflows are not accepted Phase 2 behavior.

### Reporting
Dashboard/read models and future metrics. Advanced reporting remains deferred.

---

## 8. Tenant and Branch Map

Primary security boundary:
- `TenantId`

Secondary operational scope:
- `BranchId` only when business meaning requires it

Rules:
- every tenant-owned root is tenant-bound
- Branch never replaces Tenant
- records with both values must remain tenant-consistent
- client input does not choose arbitrary tenant scope
- platform bypass is explicit and auditable
- direct queries against non-tenant-owned child tables require a tenant-aware join

Examples:
- Appointment -> tenant + branch
- Patient -> tenant, branch-neutral ownership
- ClinicalRecord -> tenant + patient
- Odontogram -> tenant + patient
- TreatmentPlan -> tenant + patient

---

## 9. Dependency Direction Rules

Backend:

```text
Api -> Application
Application -> Domain
Infrastructure -> Domain + Application abstractions
SharedKernel -> only intentional shared primitives
```

Avoid:
- Domain -> Infrastructure/Api
- Application -> Api
- feature internals coupled without explicit design

Frontend:

```text
pages -> facades -> data-access
components -> inputs/outputs
core/shared -> foundations
features -> bounded ownership
```

Avoid direct HTTP in pages/components and feature-specific business logic in shared/shell.

---

## 10. Where New Code Should Go

Examples:
- Patient use case -> Application/Patients and owning domain entity
- Appointment rule -> Scheduling domain/application
- Odontogram rule -> Odontogram domain/application; do not place in Clinical or Treatments for convenience
- Treatment-plan audit/opening work -> Treatments feature and Release 5 documents
- Persistence adapter -> Infrastructure
- Endpoint -> Api
- Patient search UI -> features/patients
- Treatment API calls -> features/treatments/data-access

Structural changes also require documentation/ADR updates.

---

## 11. Stable vs Evolving

Stable by accepted decision:
- product vision
- Tenant/Branch definitions
- modular monolith
- shared DB/schema with TenantId discriminator
- layered backend and feature-based frontend
- Releases 1 to 4 accepted boundaries
- no hidden platform bypass
- roadmap discipline

Evolving through bounded slices:
- exact future role/permission catalog
- Release 5+ formal acceptance
- patient-facing identity implementation
- provider/doctor assignment
- advanced integrations and reporting
- visual debt and component decomposition

---

## 12. Release-Oriented Map

- Release 0 — Foundation: completed
- Release 1 — Patients: completed
- Release 2 — Scheduling: completed
- Release 3 — Clinical Records: completed
- Release 4 — Odontogram: completed
- Release 5 — Treatments and Quotes: next; audit required
- Release 6 — Billing: planned after Release 5
- Release 7 — Documents and Dashboard: planned after Release 6
- Phase 2.1 — Patient Intake and Portal Foundation: planned after initial MVP
- Phase 3 — SaaS Growth
- Phase 4 — Advanced Product Capabilities/full portal

---

## 13. Review Hotspots

High-risk backend:
- tenant resolution/filters
- authorization and platform bypass
- patient/clinical/odontogram records
- treatment/billing/document access
- public patient-facing endpoints

High-risk frontend:
- auth/session/guards
- tenant/branch context
- patient/clinical/odontogram screens
- treatment/billing workflows
- cross-feature dependencies
- large page/component growth

High-risk documentation:
- release state
- roadmap
- tenant/auth/permission model
- module ownership

---

## 14. Agent Workflow Guidance

1. Read canonical docs.
2. Inspect actual code, migrations and tests.
3. Identify ownership and drift.
4. Choose the smallest safe step.
5. Reuse existing valid behavior instead of rebuilding it.
6. Validate tenant/security explicitly.
7. Run CI-relevant validation.
8. Reconcile docs when state changes.
9. Report completed and pending scope.

For Release 5, start with an audit of existing code. Do not add functionality until the audit identifies a concrete gap within the bounded roadmap.

---

## 15. What Must Never Happen

- platform and tenant concerns mixed casually
- tenant safety based only on manual discipline
- giant mixed-purpose diffs
- business rules hidden in controllers/UI
- secrets committed
- feature code placed in unrelated modules
- release acceptance inferred only from code presence
- advanced features added before the core boundary is accepted
- closed Release 4 scope reopened incidentally from Treatments

---

## 16. Quick Navigation

Patient registration:
- Patients module
- tenant model
- Release 1 docs

Scheduling:
- Scheduling module
- branch-aware rules
- ADR 003

Clinical:
- Clinical Records feature
- Release 3 mapping/closure evidence

Odontogram:
- `features/odontogram`
- backend Odontograms application/domain
- Release 4 audit
- ADR 007

Treatments/Quotes:
- current Treatments code
- Release 5 roadmap
- audit before acceptance

Patient Intake/Portal:
- ADR 006
- general plan
- issues #2 and #4 to #7
- no implementation yet

---

## 17. Final Rule

When in doubt, follow this order:

1. security
2. tenant isolation
3. accepted release boundary
4. module ownership
5. maintainability
6. operational UX
7. speed of delivery

Guiding question:

**Does this belong in the right place and preserve Bigsmile as a secure, maintainable, multi-tenant SaaS product?**
