# PROJECT_MAP.md

# Project
Bigsmile

# Purpose of this file
This file gives agents and contributors a fast operational map of the repository.

It exists to answer:
- what this repository is for
- how it is organized
- which module owns what
- which dependencies are allowed
- where new code should go
- which areas are stable vs evolving
- how to avoid architectural drift

This file complements:
- STATE — BigSmile.md
- README.md
- docs/architecture.md
- docs/tenant-model.md
- docs/product-roadmap.md
- docs/contributing.md
- docs/decisions/*.md
- AGENTS.md

If there is any conflict, start with `STATE — BigSmile.md`, then follow accepted architecture and ADR documents before using this file as an operational guide.

---

## 1. Product Summary

Bigsmile is a SaaS platform for managing dental clinics and private practices.

Core business definitions:
- **Tenant** = clinic or private practice customer
- **Branch** = internal location belonging to a tenant

The system is designed as a **multi-tenant SaaS product from day one**.

Initial product direction:
- start with a strong operational MVP
- protect tenant isolation
- preserve maintainability
- optimize for operational UX
- grow through clearly defined business modules

---

## 2. Current Repository State

This repository is beyond the early foundation / bootstrap stage.

Canonical project status:

* **Foundation / Release 0 base:** completed
* **Pre-auth hardening:** completed
* **Identity + Persistence Foundation:** completed
* **Tenant-Aware Authorization Foundation:** completed
* **Release 1 — Patients:** completed
* **Release 2 — Scheduling:** completed
* **Release 3 — Clinical Records:** completed as the foundational clinical release through accepted slices **Release 3.1 — Clinical Record Foundation**, **Release 3.2 — Basic Diagnoses Foundation**, **Release 3.3 — Clinical Timeline Read Model**, **Release 3.4 — Clinical Snapshot Change History**, **Release 3.5 — Medical Questionnaire Backend**, and **Release 3.6 — Clinical Encounter / Vitals Backend**
* **Release 4 — Odontogram:** completed as the foundational odontogram release through accepted slices **Release 4.1 — Odontogram Foundation**, **Release 4.2 — Odontogram Surface Foundation**, **Release 4.3 — Basic Dental Findings Foundation**, and **Release 4.4 — Dental Findings Change History**
* **Next planned functional phase:** **Release 5 — Treatments and Quotes**

### Release 4 closure evidence

* `docs/release-4-odontogram-audit-and-closure.md`
* ADR 007 — `docs/decisions/007-release-4-odontogram-closure.md`

### UX / existing-code reconciliation

Odontogram is now `Accepted / preserved` after a module-specific audit of its domain, application, API, persistence, permissions, frontend, migrations and tests.

The repository still contains functional code in later roadmap modules, including Treatments/Quotes, Billing, Documents, Dashboard, and reminders/manual reminders. Code, routes, permissions, migrations, or tests in those modules do not by themselves open, accept, or close Release 5, Release 6, Release 7, or Phase 2.

Until a module-specific audit and acceptance pass happens, those later modules are `implemented but not formally accepted/reconciled`.

Visual slices may improve presentation, organization, copy, color, microinteractions, modals/drawers/tabs/sticky action bars, and UX debt without changing backend, APIs, permissions, auth, tenant context, branch context, migrations, or functional scope.

### Current expected priority

Preserve Releases 1 through 4 and audit the existing Treatments/Quotes implementation before accepting or adding functionality:

* preserve the accepted Release 3.1 to 3.6 clinical boundary
* preserve the accepted Release 4.1 to 4.4 odontogram boundary
* keep advanced Odontogram scope deferred, including child/mixed dentition, full state history/versioning, restore, advanced charting, imaging and cross-module linkage
* inspect Treatments/Quotes domain, application, API, persistence, permissions, migrations, frontend and tests against Release 5
* do not treat existing TreatmentPlan/TreatmentQuote code as accepted solely because it exists
* keep payments, balances, receipts, taxes, discounts, CFDI/PAC, multi-billing, OCR, document workflows, automated messaging/providers/jobs/queues/retries, online booking and advanced dashboards deferred until their owning phases are explicitly accepted
* keep doctor-based views deferred until provider/doctor assignment is intentionally opened
* preserve scope-aware authorization, explicit platform override behavior and centralized tenant safety
* continue validation through CI, tests, logging, auditing and architectural guardrails
* keep Phase 2.1 Patient Intake and Portal planned under ADR 006 without opening PI-1 to PI-4 before the MVP gate or an explicit reprioritization

Do not treat the repository as feature-complete unless actual code and aligned documentation clearly prove it. No functional roadmap release should be treated as closed without explicit audit, tests and documentation evidence.

---

## 3. High-Level Repository Layout

Expected layout:

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

### Root-level intent

* `STATE — BigSmile.md` -> canonical current project state
* `README.md` -> product and repo overview
* `AGENTS.md` -> how agents/contributors should behave
* `PROJECT_MAP.md` -> operational repo map
* `REVIEW_RULES.md` -> review and validation checklist
* `docs/` -> architectural, release, product and operational documentation
* `backend/` -> .NET backend
* `frontend/` -> Angular frontend
* `.github/` -> CI/CD and repo automation

---

## 4. Documentation Map

### Core documents

* `README.md`
* `docs/architecture.md`
* `docs/tenant-model.md`
* `docs/product-roadmap.md`
* `docs/frontend-ux-guidelines.md`
* `docs/contributing.md`
* `docs/decisions/*.md`

### Release and future-scope evidence

* `docs/release-3-clinical-records-form-mapping.md`
* `docs/release-4-odontogram-audit-and-closure.md`
* `docs/patient-intake-and-portal-plan.md`

### Documentation ownership rules

Update documentation when changes affect:

* architecture
* module boundaries
* tenant model
* auth/session model
* authorization model
* roadmap priorities
* repository conventions
* important workflows
* formal release state

### ADR expectation

Create or update an ADR when changing:

* architecture style
* tenant resolution model
* database tenancy strategy
* authentication/session strategy
* authorization model
* module ownership
* major integration patterns
* major frontend state strategy
* formal release scope when ambiguity would otherwise remain

---

## 5. Backend Map

Expected backend layout:

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

### 5.1 BigSmile.Api

Owns:

* HTTP endpoints
* API contracts
* middleware registration
* authentication wiring
* authorization entry points
* API versioning
* transport concerns
* app composition

Must not own:

* core business rules
* persistence logic
* domain invariants

### 5.2 BigSmile.Application

Owns:

* use cases
* commands
* queries
* handlers
* validators
* application orchestration
* transaction boundaries
* authorization orchestration
* tenant-aware execution flow

Must not become:

* a dumping ground for generic services
* a place for infrastructure-specific hacks
* a replacement for domain rules

### 5.3 BigSmile.Domain

Owns:

* aggregates
* entities
* value objects
* domain invariants
* domain events
* business lifecycle rules

Expected important aggregates:

* Patient
* Appointment
* ClinicalRecord
* Odontogram
* TreatmentPlan
* TreatmentQuote
* BillingDocument / future Payment records

Must not depend on:

* infrastructure
* transport
* UI concerns

### 5.4 BigSmile.Infrastructure

Owns:

* EF Core
* DbContext
* repositories
* query/specification support
* migrations
* external integrations
* file storage providers
* notification providers
* auditing persistence
* tenant resolution infrastructure

Must not:

* redefine business ownership rules
* hide tenant bypass logic casually
* absorb application logic

### 5.5 BigSmile.SharedKernel

Owns:

* shared abstractions
* base types
* common primitives
* cross-cutting contracts
* common exceptions where appropriate

Must stay small and intentional.

### 5.6 Backend Test Projects

* `BigSmile.UnitTests` -> isolated logic tests
* `BigSmile.IntegrationTests` -> infra/API/DB validation
* `BigSmile.ArchitectureTests` -> structural rule enforcement

---

## 6. Frontend Map

Expected frontend layout:

```text
frontend/
  src/app/
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

### 6.1 core/

Owns:

* auth infrastructure
* session handling
* HTTP configuration
* interceptors
* guards
* tenant context
* global error handling
* app-level services
* frontend localization state and dictionaries

Must not contain:

* module-specific business UI
* feature-specific orchestration

### 6.2 shell/

Owns:

* authenticated app shell
* main layout
* navigation
* header
* side menu
* global branch selector
* layout composition

### 6.3 shared/

Owns:

* reusable UI components
* directives
* pipes
* app-wide translation/date pipes and language selector components
* generic helpers
* shared form utilities

Must not become:

* an unstructured dumping ground
* a place for module-specific code pretending to be shared

### 6.4 features/

Owns business features by bounded context.

Each feature should prefer this layout:

```text
features/<feature>/
  pages/
  components/
  facades/
  data-access/
  models/
```

#### pages/

Route-level containers.

#### components/

Reusable feature UI pieces.

#### facades/

Feature orchestration and UI-facing coordination.

#### data-access/

HTTP/API integration and feature-specific data retrieval.

#### models/

Feature-local types and view contracts.

### Frontend anti-drift rules

* no direct HTTP calls scattered across pages/components
* no giant page components without an explicit decomposition plan
* no feature logic hidden in shared unless truly shared
* no business-critical authorization only in UI
* no internal release/slice language in clinic-facing copy when touching visible UI
* use `--bsm-*` tokens instead of introducing isolated hardcoded visual styles

---

## 7. Business Module Map

Bigsmile grows through bounded contexts.

### 7.1 Platform

Owns:

* tenants
* branches
* plans
* feature flags
* branding
* tenant settings
* platform administration

### 7.2 Identity

Owns:

* staff users
* roles
* permissions
* memberships
* sessions
* access policies

Patient-facing identity is a separate future boundary under ADR 006 and must not reuse staff membership semantics.

### 7.3 Patients

Owns:

* patient registration
* patient profile
* responsible party
* patient alerts
* patient search
* contact/demographic source of truth

### 7.4 Scheduling

Owns:

* appointments
* calendar views
* appointment states
* blocked slots
* rescheduling
* no-shows

Doctor-based views remain outside the accepted Release 2 scope until provider assignment exists.

### 7.5 Clinical

Owns:

* clinical record
* clinical notes
* medical background
* allergies
* diagnoses
* fixed medical questionnaire backend and bounded frontend integration
* clinical encounters and vitals backend and bounded frontend integration
* bounded timeline and snapshot history

### 7.6 Odontogram

Accepted Release 4 ownership:

* odontogram aggregate
* permanent adult FDI tooth state
* bounded `O/M/D/B/L` surface state
* basic surface findings
* append-only finding add/remove history
* dental visual status and patient-context UI

Future treatment/diagnosis/document linkage, complete dental history/versioning, child/mixed dentition and advanced charting remain deferred.

### 7.7 Treatments

Owns:

* treatment catalog
* treatment plans
* quotes
* acceptance
* future treatment progress

Existing code is the next audit target and is not formally accepted until Release 5 reconciliation.

### 7.8 Billing

Owns:

* charges
* payments
* balances
* receipts
* cash sessions
* future invoicing integration

Existing code remains unaccepted until Release 6 audit.

### 7.9 Documents

Owns:

* file attachments
* patient documents
* radiographies
* consent-related file storage

Existing code remains unaccepted until Release 7 audit.

### 7.10 Notifications

Owns:

* reminders
* confirmations
* delivery status
* templates
* future WhatsApp/email flows

Manual reminder helpers/templates do not imply automated provider delivery, jobs, queues or retry workflows.

### 7.11 Reporting

Owns:

* dashboards
* operational metrics
* treatment metrics
* scheduling metrics
* billing summaries

---

## 8. Tenant and Branch Map

### Tenant rules

All tenant-owned records must belong to a tenant.

Security boundary:

* primary boundary = `TenantId`

### Branch rules

Branch is operational scope only.

Operational boundary:

* secondary boundary = `BranchId` where the business requires it

### Practical rule

If a record has `BranchId`, it must still be tenant-bound.

Examples:

* appointment -> tenant + branch
* payment -> tenant + sometimes branch
* patient -> tenant, not necessarily branch-owned
* clinical record -> tenant, patient-owned context
* odontogram -> tenant, patient-owned context
* treatment plan -> tenant, patient-owned context

### Child-table rule

If a child table does not carry `TenantId` and is queried directly rather than through its tenant-owned aggregate, the query must include an explicit tenant-aware join or the model must be changed to explicit tenant ownership.

### Never assume

* branch alone is enough for security
* users can jump tenant boundaries
* platform operations can bypass tenant safety silently
* a route/body `PatientId` or `TenantId` is an authority source

---

## 9. Dependency Direction Rules

### Backend dependency direction

Allowed direction:

```text
Api -> Application
Application -> Domain
Infrastructure -> Domain / Application abstractions
SharedKernel -> shared by others where justified
```

Avoid:

* Domain -> Infrastructure
* Domain -> Api
* Application -> Api
* feature logic depending on unrelated module internals without explicit design

### Frontend dependency direction

Preferred direction:

```text
pages -> facades -> data-access
components -> inputs/outputs only where possible
core/shared -> reusable foundations
features -> isolated by module
```

Avoid:

* pages directly calling HTTP everywhere
* shared depending on specific features
* shell owning feature business logic
* one feature casually reaching deep into another without explicit boundary

---

## 10. Where New Code Should Go

### Backend

Add new behavior according to module ownership.

Examples:

* new patient registration use case -> `Application/Patients/...`
* new appointment rules -> Scheduling domain/application
* new Odontogram rule -> Odontogram domain/application, not Clinical or Treatments for convenience
* new TreatmentPlan rule -> Treatments domain/application after Release 5 audit identifies an accepted gap
* new payment persistence adapter -> Infrastructure
* new endpoint -> Api

### Frontend

Place code in the owning feature.

Examples:

* patient search screen -> `features/patients/pages`
* appointment calendar widget -> `features/scheduling/components`
* patient facade -> `features/patients/facades`
* odontogram API calls -> `features/odontogram/data-access`
* treatment API calls -> `features/treatments/data-access`

### Documentation

If structure, security, release state or ownership changes, update docs and an ADR where appropriate.

---

## 11. What Is Stable vs Evolving

### Stable by intention

These should be treated as stable unless formally changed:

* product vision
* tenant = clinic/practice
* branch = tenant location
* modular monolith direction
* multi-tenancy as foundational
* backend layered structure
* frontend feature structure
* accepted Release 1 through Release 4 boundaries
* roadmap order
* security-first posture

### Evolving areas

These may still evolve through implementation and ADRs:

* exact future permission catalog
* Release 5+ formal scope acceptance
* patient-facing identity implementation
* storage/notification provider choices
* reporting depth
* invoicing integration details
* onboarding automation details
* visual debt and component decomposition

### Rule

Do not casually change stable areas without explicit documentation and review.

---

## 12. Release-Oriented Map

The project grows in this order unless docs explicitly change:

### Release 0

Foundation — completed.

### Release 1

Patients — completed.

### Release 2

Scheduling — completed.

### Release 3

Clinical Records — completed.

### Release 4

Odontogram — completed through slices 4.1 to 4.4.

### Release 5

Treatments and Quotes — next; existing code must be audited before acceptance.

### Release 6

Billing — planned after Release 5.

### Release 7

Documents and Dashboard — planned after Release 6.

### Later phases

* Phase 2.1 Patient Intake and Portal Foundation under ADR 006
* reminders/providers/online booking
* electronic invoicing
* advanced SaaS platform features
* full patient portal
* advanced analytics
* automations

---

## 13. Review Hotspots

Changes in these areas require extra care:

### High-risk backend areas

* tenant resolution
* query filtering
* authorization
* patient records
* clinical records
* odontogram aggregate/child access
* treatments and quotes
* payments/billing
* document access
* platform bypass logic
* public patient-facing endpoints

### High-risk frontend areas

* auth/session handling
* tenant/branch context switching
* route guards
* patient data screens
* clinical and odontogram editing
* treatment/payment flows
* large page growth
* cross-feature dependencies

### High-risk documentation areas

* architecture changes
* roadmap/release-state changes
* tenant model changes
* permission model changes
* ADR numbering and canonical references

---

## 14. Agent Workflow Guidance

When working in this repo:

1. Read canonical docs first.
2. Inspect current code, migrations and tests.
3. Determine the owning module.
4. Distinguish existing code from formally accepted scope.
5. Choose the smallest correct implementation or reconciliation step.
6. Keep changes auditable and reversible.
7. Validate tenant/security and CI as much as possible.
8. Update docs if state or rules changed.
9. Leave a clear completed/pending summary.

#### Agent Workflow Guidance note

For Release 5, audit existing Treatments/Quotes code before adding new behavior. Reuse valid implementation rather than rebuilding it. If a specific area is sparse, prefer local foundations and minimal vertical slices rather than broad expansion.

---

## 15. What Must Never Happen

* platform and tenant concerns mixed carelessly
* tenant safety implemented only by manual discipline
* giant mixed-purpose diffs
* business rules hidden in controllers or UI
* secrets committed to repo
* feature code placed in unrelated modules
* documentation drifting far behind implementation
* code presence treated as automatic release acceptance
* advanced features added before the core workflow is stable
* closed Clinical/Odontogram boundaries reopened incidentally for Treatments

---

## 16. Quick Navigation Cheatsheet

### If working on patient registration

Look at:

* docs/product-roadmap.md
* docs/architecture.md
* docs/tenant-model.md
* backend Application/Patients
* backend Domain/Patients
* frontend features/patients

### If working on appointment scheduling

Look at:

* Scheduling module
* branch-aware rules
* tenant/branch context
* ADR 003
* frontend scheduling feature

### If working on Clinical Records

Look at:

* ClinicalRecords application/domain/API
* `docs/release-3-clinical-records-form-mapping.md`
* clinical permissions and tenant tests
* frontend features/clinical-records

### If working on Odontogram

Look at:

* Odontograms domain/application/API/persistence
* frontend features/odontogram
* `docs/release-4-odontogram-audit-and-closure.md`
* ADR 007
* accepted deferred-scope list before proposing linkage or advanced charting

### If working on Treatments/Quotes

Look at:

* Release 5 roadmap section
* TreatmentPlans/TreatmentQuotes code and tests
* tenant/permission behavior
* existing code as audit evidence, not automatic acceptance

### If working on auth or permissions

Look at:

* Identity module
* tenant model
* ADRs
* security rules
* contribution rules
* tenant/platform scope separation

### If working on Patient Intake/Portal

Look at:

* ADR 006
* `docs/patient-intake-and-portal-plan.md`
* issues #2 and #4 to #7
* no implementation is active yet

### If working on platform admin functions

Look at:

* Platform module
* platform bypass rules
* audit requirements
* tenant safety rules
* roadmap fit

---

## 17. Final Rule

Use this map to stay oriented, but do not use it as an excuse to ignore canonical state or architecture documents.

When in doubt, follow this order:

1. security
2. tenant isolation
3. accepted release boundary
4. module ownership
5. maintainability
6. operational UX
7. speed of delivery

### Guiding question

For any new file, feature, or change, ask:

**Does this belong in the right place and preserve Bigsmile as a secure, maintainable, multi-tenant SaaS product?**

---

## Manual VPS Deployment Note

BigSmile has a first accepted manual VPS deployment foundation.

Operational deployment baseline:

- domain: https://bigsmile.com.mx
- API local binding: 127.0.0.1:5010
- systemd service: bigsmile-api.service
- release root: /var/www/bigsmile
- active symlink: /var/www/bigsmile/current
- private environment file: /etc/bigsmile/bigsmile-api.env
- private appsettings file: /var/www/bigsmile/shared/appsettings.Production.json
- SQL Server target: 127.0.0.1,14330
- database: BigSmile
- pilot protection: Nginx Basic Auth

This is documented in `docs/decisions/004-manual-vps-deployment-foundation.md`.

GitHub Actions deployment should automate the same release/rollback flow and must not introduce a different deployment model without a new decision.
