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
* **Release 5 — Treatments and Quotes:** completed as the foundational treatment-planning and quote release through accepted slices **Release 5.1 — Treatment Plan Foundation** and **Release 5.2 — Quote Foundation**
* **Release 6 — Billing:** completed through **Release 6.1 — Billing Document Foundation**
* **Release 7 — Documents and Dashboard:** completed through **Release 7.1 — Patient Documents Foundation** and **Release 7.2 — Dashboard Read Model Foundation**
* **Initial operational MVP:** formally accepted
* **Next planned phase:** **Phase 2.1 — Patient Intake and Portal Foundation**, not yet opened

### Release 4 closure evidence

* `docs/release-4-odontogram-audit-and-closure.md`
* ADR 007 — `docs/decisions/007-release-4-odontogram-closure.md`

### Release 5 closure evidence

* `docs/release-5-treatments-and-quotes-audit-and-closure.md`
* ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`

### Release 6 closure evidence

* `docs/release-6-billing-audit-and-closure.md`
* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

### Release 7 / MVP closure evidence

* `docs/release-7-documents-and-dashboard-audit-and-closure.md`
* ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`
* ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`

### UX / existing-code reconciliation

Odontogram, Treatments/Quotes, Billing, Documents and Dashboard are `Accepted / preserved` after module-specific audits of domain/application, API, persistence/storage/read models, permissions, frontend, migrations and tests.

The initial operational MVP is formally accepted. The repository still contains later capabilities such as reminders/manual reminders, but code presence does not open or accept providers, jobs, online booking, Phase 2 or advanced analytics.

Every later capability still requires an explicit bounded audit and acceptance pass.

Visual slices may improve presentation, organization, copy, color, microinteractions, modals/drawers/tabs/sticky action bars, and UX debt without changing backend, APIs, permissions, auth, tenant context, branch context, migrations, or functional scope.

### Current expected priority

Preserve Releases 1 through 7 and prepare the explicit Phase 2.1 opening decision:

* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries
* keep payments, balances, receipts, cash management, fiscal/CFDI and automatic quote mutation outside Release 6.1
* keep OCR/sharing/versioning and advanced Dashboard analytics outside Release 7
* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries
* resolve the patient access/bootstrap choices tracked in issue #2 before starting PI-1
* start only PI-1 / issue #4 after an explicit Phase 2.1 opening and update STATE in the same PR
* keep automated messaging/providers/jobs/queues/retries, online booking and full Patient Portal deferred
* keep doctor-based views deferred until provider/doctor assignment is intentionally opened
* preserve scope-aware authorization, explicit platform override behavior and centralized tenant safety
* continue validation through CI, tests, logging, auditing and architectural guardrails

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
* `docs/release-5-treatments-and-quotes-audit-and-closure.md`
* `docs/release-6-billing-audit-and-closure.md`
* `docs/release-7-documents-and-dashboard-audit-and-closure.md`
* `docs/decisions/010-tenant-time-zone-foundation.md`
* `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`
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
* tenant settings, including server-authoritative `TimeZoneId`
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

Accepted Release 5 ownership:

* tenant-owned/patient-owned treatment-plan aggregate
* bounded treatment-plan items
* optional permanent-adult FDI tooth/surface references
* treatment-plan `Draft / Proposed / Accepted` lifecycle
* tenant-owned/patient-owned quote aggregate
* snapshot-only quote lines
* bounded line pricing and calculated totals
* quote `Draft / Proposed / Accepted` lifecycle
* patient-context Angular plan and quote workflows

Treatment catalog administration, multiple/archived plans, quote versioning/negotiation, execution/progress, taxes/discounts and Patient Portal access remain deferred. Billing is accepted separately through Release 6.1; payments remain deferred.

### 7.8 Billing

Accepted Release 6 ownership:

* tenant-owned/patient-owned `BillingDocument` aggregate
* explicit snapshot creation from an accepted TreatmentQuote
* snapshot-only Billing lines with currency, unit price, line total and total
* bounded `Draft -> Issued` lifecycle
* issue timestamp/actor metadata
* issued-document read-only behavior
* patient-context Angular Billing workflow

Payments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency and accounting remain deferred and must not be modeled as incidental mutable fields on `BillingDocument`.

### 7.9 Documents

Accepted Release 7.1 ownership:

* tenant-owned/patient-owned `PatientDocument` metadata
* explicit authorized upload/list/download/logical retire
* private root-contained binary storage
* server-generated storage keys
* PDF/JPEG/PNG declared-type and binary-signature validation
* bounded upload limits and UTC/actor metadata
* patient-context Angular Documents workflow

OCR, rich preview, versioning, public sharing, generated PDFs, advanced consent/e-signature, antivirus providers, retention automation and Patient Portal access remain deferred.

### 7.10 Notifications

Owns:

* reminders
* confirmations
* delivery status
* templates
* future WhatsApp/email flows

Manual reminder helpers/templates do not imply automated provider delivery, jobs, queues or retry workflows.

### 7.11 Reporting

Accepted Release 7.2 Dashboard ownership:

* tenant-scoped read-only operational summary
* active patients
* tenant-local today/pending appointments
* active documents
* treatment-plan and accepted-quote counts
* issued Billing-document count
* generated-at UTC
* Angular summary-card workflow

Future Reporting owns deeper treatment/scheduling/billing metrics, charts, exports and analytics. Revenue/balance metrics, branch/doctor dashboards, BI, real-time and AI recommendations remain deferred.

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
* treatment quote -> tenant, patient-owned and treatment-plan-owned context
* billing record -> tenant, patient-owned and quote-linked where the accepted Billing model requires it
* patient document -> tenant + patient; binary access follows authorized metadata lookup
* dashboard summary -> tenant-scoped read model; operational day derives from tenant-owned `TimeZoneId`

### Child-table rule

If a child table does not carry `TenantId` and is queried directly rather than through its tenant-owned aggregate, the query must include an explicit tenant-aware join or the model must be changed to explicit tenant ownership.

### Never assume

* branch alone is enough for security
* users can jump tenant boundaries
* platform operations can bypass tenant safety silently
* a route/body `PatientId` or `TenantId` is an authority source
* downstream Billing may silently mutate an accepted TreatmentQuote

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
* new TreatmentPlan/TreatmentQuote rule -> Treatments domain/application only when it fits the accepted Release 5 boundary or an explicit future slice
* new Billing-document rule -> Billing domain/application only when it preserves the accepted Release 6.1 boundary
* new Payment/Receipt/Cash behavior -> a separate future aggregate/slice, not incidental BillingDocument fields
* new Documents rule -> Documents domain/application/storage only within an explicit accepted or future slice
* new Dashboard metric -> Reporting read model over accepted tenant-owned roots with explicit metric semantics
* new payment persistence adapter -> Infrastructure after its owning aggregate is accepted
* new endpoint -> Api

### Frontend

Place code in the owning feature.

Examples:

* patient search screen -> `features/patients/pages`
* appointment calendar widget -> `features/scheduling/components`
* patient facade -> `features/patients/facades`
* odontogram API calls -> `features/odontogram/data-access`
* treatment API calls -> `features/treatments/data-access`
* billing API calls -> `features/billing/data-access`

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
* accepted Release 1 through Release 7 boundaries and MVP scope
* roadmap order
* security-first posture

### Evolving areas

These may still evolve through implementation and ADRs:

* exact future permission catalog
* Phase 2.1+ formal scope acceptance
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

Treatments and Quotes — completed through slices 5.1 and 5.2.

### Release 6

Billing — completed through Release 6.1 — Billing Document Foundation.

### Release 7

Documents and Dashboard — completed through Release 7.1 and 7.2; initial operational MVP accepted.

### Later phases

* Phase 2.1 Patient Intake and Portal Foundation under ADR 006 — next planned, not opened
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
* treatment plans and quotes
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

For Release 6, audit existing Billing code before adding new behavior. Reuse valid implementation rather than rebuilding it. Preserve accepted TreatmentPlan/TreatmentQuote snapshots and keep payment, receipt, tax and cancellation behavior outside the accepted scope until the audit proves otherwise.

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
* closed Clinical/Odontogram/Treatments boundaries reopened incidentally for Billing
* accepted quote snapshots silently rewritten by downstream modules

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
* `docs/release-5-treatments-and-quotes-audit-and-closure.md`
* ADR 008
* accepted deferred-scope list before proposing execution, Billing linkage or advanced pricing

### If working on Billing

Look at:

* Release 6 roadmap section
* Billing domain/application/API/persistence and tests
* accepted TreatmentQuote contracts and immutability rules
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
