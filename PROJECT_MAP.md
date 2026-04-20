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
- README.md
- docs/architecture.md
- docs/tenant-model.md
- docs/product-roadmap.md
- docs/contributing.md
- docs/decisions/*.md
- AGENTS.md

If there is any conflict, follow the architecture and ADR documents first, then use this file as an operational guide.

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
* **Release 3 — Clinical Records:** in progress
* **Accepted slices:** **Release 3.1 — Clinical Record Foundation** and **Release 3.2 — Basic Diagnoses Foundation**

### Current expected priority

Continue the current active phase:

* **Release 3 — Clinical Records**
* keeping Release 3 explicitly open, not closed, while treating Release 3.1 and Release 3.2 as the accepted slices so far
* preserving the accepted Release 3.1 scope: tenant-owned and patient-owned clinical records, explicit creation, `GET` returning `404` when missing, no autocreation, base snapshot, current allergies, and append-only notes returned newest-first
* preserving the accepted Release 3.2 scope: basic non-coded diagnoses on existing clinical records, explicit add/resolve operations, diagnosis inclusion in the clinical record read model, and `Active` / `Resolved` status with active-first and newest-first ordering in reads and UI
* keeping the full clinical timeline, odontogram, treatments, and documents outside the accepted Release 3 slices
* preserving the current clinical access restriction where `clinical.read` / `clinical.write` are granted to `PlatformAdmin` and `TenantAdmin`, and `TenantUser` does not receive clinical permissions in this phase
* preserving the completed Scheduling release covering appointment foundation, blocked slots, appointment notes, explicit attended/no-show completion states, and branch-aware day/week calendar views
* keeping doctor-based views explicitly deferred to a future bounded slice that introduces provider/doctor assignment
* only on top of the completed tenant-aware authorization and tenant enforcement baseline
* while preserving scope-aware authorization, explicit platform override behavior, and centralized tenant safety
* with continued validation through CI, tests, logging, auditing, and architectural guardrails
* building forward from the completed Patients and Scheduling releases without reopening closed release scope casually

Do not treat the repository as feature-complete unless the actual codebase clearly proves it. No functional roadmap release should be treated as closed without explicit code and documentation evidence.

---

## 3. High-Level Repository Layout

Expected layout:

```text
/
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

* `README.md` -> product and repo overview
* `AGENTS.md` -> how the agent should behave
* `PROJECT_MAP.md` -> operational repo map
* `REVIEW_RULES.md` -> review and validation checklist
* `docs/` -> architectural and product documentation
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
* `docs/contributing.md`
* `docs/decisions/*.md`

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
* Payment

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
* no giant page components
* no feature logic hidden in shared unless truly shared
* no business-critical authorization only in UI

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

* users
* roles
* permissions
* memberships
* sessions
* access policies

### 7.3 Patients

Owns:

* patient registration
* patient profile
* responsible party
* patient alerts
* patient search

### 7.4 Scheduling

Owns:

* appointments
* calendar views
* appointment states
* blocked slots
* rescheduling
* no-shows

### 7.5 Clinical

Owns:

* clinical record
* clinical notes
* medical background
* allergies
* diagnoses

### 7.6 Odontogram

Owns:

* odontogram
* tooth state
* surface state
* findings
* dental visual status

### 7.7 Treatments

Owns:

* treatment catalog
* treatment plans
* quotes
* acceptance
* treatment progress

### 7.8 Billing

Owns:

* charges
* payments
* balances
* receipts
* cash sessions
* future invoicing integration

### 7.9 Documents

Owns:

* file attachments
* patient documents
* radiographies
* consent-related file storage

### 7.10 Notifications

Owns:

* reminders
* confirmations
* delivery status
* templates
* future WhatsApp/email flows

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
* treatment plan -> tenant, patient-owned context

### Never assume

* branch alone is enough for security
* users can jump tenant boundaries
* platform operations can bypass tenant safety silently

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
* new appointment rules -> `Domain/Scheduling` or equivalent module structure
* new payment persistence adapter -> `Infrastructure/...`
* new endpoint -> `Api/...`

### Frontend

Place code in the owning feature.

Examples:

* patient search screen -> `features/patients/pages`
* appointment calendar widget -> `features/scheduling/components`
* patient facade -> `features/patients/facades`
* treatment API calls -> `features/treatments/data-access`

### Documentation

If a structural rule changes, also update docs and possibly ADRs.

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
* roadmap order
* security-first posture

### Evolving areas

These may still evolve through implementation and ADRs:

* exact auth/session implementation
* exact permission catalog
* exact frontend state pattern details
* storage provider choice
* notification provider choice
* reporting depth
* invoicing integration details
* onboarding automation details

### Rule

Do not casually change stable areas without explicit documentation and review.

---

## 12. Release-Oriented Map

The project should grow in this order unless docs explicitly change:

### Release 0

Foundation:

* solution structure
* tenant model implementation base
* auth/authz
* auditing
* error handling
* logging
* architecture tests
* CI baseline

### Release 1

Patients

### Release 2

Scheduling

### Release 3

Clinical Records

### Release 4

Odontogram

### Release 5

Treatments and Quotes

### Release 6

Billing

### Release 7

Documents and Dashboard

### Later phases

* reminders
* online booking
* electronic invoicing
* advanced SaaS platform features
* patient portal
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
* payments
* document access
* platform bypass logic

### High-risk frontend areas

* auth/session handling
* tenant/branch context switching
* route guards
* patient data screens
* payment flows
* clinical note entry
* large page growth
* cross-feature dependencies

### High-risk documentation areas

* architecture changes
* roadmap changes
* tenant model changes
* permission model changes

---

## 14. Agent Workflow Guidance

When working in this repo:

1. Read the core docs first
2. Inspect current code and file structure
3. Determine the owning module
4. Choose the smallest correct implementation step
5. Keep changes auditable and reversible
6. Validate as much as possible
7. Update docs if structure or rules changed
8. Leave a clear summary for human/CODEX review

#### Agent Workflow Guidance note

If a specific area is still sparse or scaffold-only, prefer:

* finishing its local foundation
* conventions
* architecture guardrails
* minimal vertical slice examples

Do not expand many business features at once from a still-sparse area.

---

## 15. What Must Never Happen

* platform and tenant concerns mixed carelessly
* tenant safety implemented only by manual discipline
* giant mixed-purpose diffs
* business rules hidden in controllers or UI
* secrets committed to repo
* feature code placed in unrelated modules
* documentation drifting far behind implementation
* advanced features added before the core workflow is stable

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
* frontend scheduling feature
* reporting implications later

### If working on auth or permissions

Look at:

* Identity module
* tenant model
* ADRs
* security rules
* contribution rules
* tenant/platform scope separation

### If working on platform admin functions

Look at:

* Platform module
* platform bypass rules
* audit requirements
* tenant safety rules
* roadmap fit

---

## 17. Final Rule

Use this map to stay oriented, but do not use it as an excuse to ignore the architecture documents.

When in doubt, follow this order:

1. security
2. tenant isolation
3. module ownership
4. maintainability
5. operational UX
6. speed of delivery

### Guiding question

For any new file, feature, or change, ask:

**Does this belong in the right place and preserve Bigsmile as a secure, maintainable, multi-tenant SaaS product?**
