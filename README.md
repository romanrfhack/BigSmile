# Bigsmile

**Bigsmile** is a SaaS platform for managing dental clinics and private practices.  
It is designed from the ground up to support **multi-tenant architecture**, allowing multiple clinics to operate independently with isolated data, their own users, branches, and administrative controls.

Bigsmile starts with a strong operational core for dental workflows and is intended to evolve into a scalable, commercial product for multiple clinics and organizations.

---

## Overview

Bigsmile is being built to streamline the daily operation of dental clinics through a unified platform that connects:

- Appointment scheduling
- Patient records
- Clinical history
- Odontogram
- Treatment plans
- Quotes and budgets
- Billing and collections
- Documents and attachments
- User and role management
- Operational dashboards

The long-term vision is to provide a modern, secure, maintainable, and scalable platform that can be licensed to many clinics while preserving a fast and intuitive user experience.

---

## Product Vision

Bigsmile is not intended to be a single-clinic internal system.  
It is being designed as a **reusable SaaS product** with a strong architectural foundation to support:

- Multi-tenant isolation
- Multi-branch operations
- Independent clinic administration
- Feature growth over time
- Maintainability and long-term evolution
- High usability for front desk and clinicians

### Core business definitions

- **Tenant**: a clinic or dental practice that uses the platform
- **Branch**: a branch or location that belongs to a tenant
- **Platform Admin**: internal administrator of the Bigsmile platform
- **Tenant Admin**: administrator for a clinic
- **Branch User**: operational user assigned to one or more branches

---

## Initial Scope

The first functional stage of Bigsmile focuses on the operational core of a dental clinic:

- Patients
- Scheduling
- Clinical records
- Odontogram
- Treatment plans
- Quotes
- Billing / cash management
- Documents
- Roles and permissions
- Basic dashboard

Future phases will expand into:

- WhatsApp and email reminders
- Online booking
- Electronic invoicing
- Advanced multi-branch operations
- Patient portal
- Advanced analytics
- Automations and follow-up workflows

### UX / Existing Code Reconciliation

After the formal closure of Release 3, active work should be treated as a `client-driven UX redesign / visual organization pass`.

The repository contains functional code in modules that are later than the formal roadmap state, including Odontogram, Treatments/Quotes, Billing, Documents, Dashboard, and reminders/manual reminders. The presence of code, routes, permissions, migrations, or tests in those areas does not mean Release 4, Release 5, Release 6, Release 7, or Phase 2 are open, accepted, or closed.

Until each module receives a specific audit and acceptance pass, those later modules must be classified as `implemented but not formally accepted/reconciled`. Visual slices may improve presentation, organization, copy, color, microinteractions, modals, drawers, tabs, sticky action bars, and UX debt without changing backend behavior, APIs, permissions, auth, tenant context, branch context, migrations, or functional scope.

---

## Technology Stack

### Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

### Frontend
- Angular 21
- TypeScript
- Feature-based architecture
- Lazy loading
- Facades and data-access layers
- Runtime UI localization defaults to Spanish for Mexico (`es-MX`) with English (`en-US`) as a selectable fallback.
- The only browser-persisted frontend preference in this foundation is the non-sensitive UI language key `bigsmile.ui.language`; auth/session, tenant, branch, token, and permission state stay out of `localStorage`.

### Quality and Operations
- GitHub Actions
- Unit, integration, architecture, and end-to-end tests
- Structured logging
- Auditing
- Health checks
- CI/CD foundation

---

## Architectural Principles

Bigsmile is being developed under the following principles:

- **Modular architecture**
- **Multi-tenancy by design**
- **SOLID principles**
- **Clear separation of concerns**
- **Security-first mindset**
- **Maintainability over shortcuts**
- **Operational UX focused on speed and clarity**
- **Incremental evolution without rewriting the core**

---

## Multi-Tenancy Model

Bigsmile is designed as a **multi-tenant SaaS application**.

### Foundational rules
- Every business record belongs to a **TenantId**
- Many operational records also belong to a **BranchId**
- Security isolation is enforced primarily by **TenantId**
- `BranchId` is used for operational segmentation such as scheduling, reporting, and branch-specific permissions

### Initial strategy
- Shared database
- Shared schema
- `TenantId` as transversal discriminator
- Global tenant query filters in the data access layer
- Explicit bypass only for platform-level operations

This allows Bigsmile to support a single clinic initially without sacrificing the future SaaS model.

---

## Solution Architecture

### Backend structure

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

### Frontend structure

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

---

## Functional Domains

Bigsmile is organized around bounded contexts rather than screens:

* **Platform**
* **Identity**
* **Patients**
* **Scheduling**
* **Clinical**
* **Odontogram**
* **Treatments**
* **Billing**
* **Documents**
* **Notifications**
* **Reporting**

This structure is intended to support growth while keeping the solution understandable and maintainable.

---

## Development Conventions

### Backend

* Use-case oriented organization
* Clear application boundaries
* Domain rules placed in the domain when appropriate
* Infrastructure isolated from business logic
* No secrets in the repository
* No manual tenant filtering scattered throughout the codebase

### Frontend

* Feature-based structure
* Small, composable components
* Separation between pages, components, facades, and data access
* Avoid oversized UI modules
* Clear UX flows for operational tasks
* Keep user-facing labels behind the frontend localization foundation when touching visible UI

---

## Security Guidelines

Bigsmile treats security as a foundational concern.

### Initial guidelines

* Secure authentication
* Role and permission-based authorization
* Audit trail for critical actions
* Safe secrets management
* Cross-tenant data isolation
* Backend validation
* Consistent error contracts

### Not allowed

* Secrets committed into versioned configuration files
* Insecure session strategies
* Sensitive business rules hardcoded only in the frontend
* Missing tenant isolation rules in data access

---

## Current Status

Bigsmile is no longer in the initial bootstrap stage.

Completed foundation milestones:

* **Foundation / Release 0 base**
* **Pre-auth hardening**
* **Identity + Persistence Foundation**
* **Tenant-Aware Authorization Foundation**
* **Release 1 — Patients**
* **Release 2 — Scheduling**
* **Release 3 — Clinical Records**

Current roadmap position:

* **Latest completed delivery phase:** **Release 3 — Clinical Records**
* **Next planned functional phase:** **Release 4 — Odontogram**
* **Phase 2 Expansion — Modern Operations:** later roadmap phase after the initial MVP, not the next phase after Release 3

Release 2 is formally complete with branch-aware daily and weekly calendar views, appointment create/edit/reschedule/cancel flows, appointment notes, blocked slots, and explicit attended/no-show states.

Doctor-based views are explicitly deferred to a future bounded slice because they require provider/doctor assignment rather than a small UI-only filter.

Release 3 is formally complete as the foundational clinical release. Release 3.1 is accepted with tenant-owned, patient-owned clinical records; explicit record creation; `GET` returning `404` when the record does not exist; no autocreation; medical background and current medications summaries; current allergies; append-only clinical notes returned newest-first; and minimal clinician attribution.

Release 3.2 is also accepted and adds only the bounded diagnoses foundation on top of an existing clinical record: explicit diagnosis creation, explicit diagnosis resolution, diagnoses included in the clinical record read model, basic non-coded diagnosis text/notes, and `Active` / `Resolved` status with active-first ordering and newest-first ordering within each status group in reads and UI.

Release 3.3 is also accepted and adds only a bounded clinical timeline read model inside the existing clinical record read contract: no new endpoint, no new timeline table, no cross-module timeline, and only `ClinicalNoteCreated`, `ClinicalDiagnosisCreated`, and `ClinicalDiagnosisResolved` events returned newest-first from existing Clinical data.

Release 3.4 is also accepted and adds only a bounded snapshot change history inside the existing clinical record read contract: an initial history entry when the record is created explicitly, additional entries only for effective changes to medical background, current medications, and current allergies, and a separate `snapshotHistory` section that does not merge with the accepted Release 3.3 timeline. This bounded history does not introduce restore, full record versioning, or rich diffs.

Release 3.5 — Medical Questionnaire Backend is also accepted as a backend slice. It adds tenant-owned `ClinicalMedicalAnswer` records under an existing clinical record, a fixed `QuestionKey` catalog based on `docs/release-3-clinical-records-form-mapping.md`, `Unknown` / `Yes` / `No` answers, optional bounded details, and `GET` / `PUT` endpoints at `/api/patients/{patientId}/clinical-record/questionnaire`. It reuses `clinical.read` / `clinical.write`, does not accept `TenantId` from the request, and does not create a form builder or sync automatically to allergies, timeline, snapshot history, Billing, Odontogram, Treatments, Documents, Scheduling, or doctor/provider assignment.

The current frontend includes a bounded UI integration for the fixed medical questionnaire inside the existing clinical record screen. It consumes only the accepted Release 3.5 endpoints, keeps Patient demographics read-only from Patients, derives age from `Patient.DateOfBirth` without persisting it, groups the fixed questions with i18n labels, and does not change backend contracts, permissions, allergy synchronization, timeline, snapshot history, or later modules.

Release 3.6 — Clinical Encounter / Vitals Backend is also accepted as a backend slice. It adds tenant-owned and patient-owned `ClinicalEncounter` records under an existing clinical record, `GET` / `POST` endpoints at `/api/patients/{patientId}/clinical-record/encounters`, bounded consultation type values (`Treatment`, `Urgency`, `Other`), optional bounded vitals, server-derived `TenantId` and `CreatedByUserId`, and optional append-only linked `ClinicalNote` creation from encounter `noteText`. It reuses `clinical.read` / `clinical.write`, does not add PUT/DELETE, does not introduce new timeline events, and does not touch Patient demographics, Billing, Scheduling, Odontogram, Treatments, Documents, or doctor/provider assignment.

The current frontend now also includes a bounded UI integration for clinical encounters and vital signs inside the existing clinical record screen. It consumes only the accepted Release 3.6 encounter endpoints through the Clinical Records data-access/facade, keeps Patient demographics read-only from Patients, lists recent encounters, captures a compact new encounter/vitals form, and does not change backend contracts, permissions, timeline behavior, Patient data, or later modules.

Release 3 remains preserved through the accepted slices above. The full or advanced clinical timeline, any cross-module timeline, restore, full clinical record versioning, form builder behavior, automatic allergy synchronization, configurable intake forms, questionnaire-driven cross-module behavior, encounter editing/deletion, and encounter-specific timeline events remain outside the accepted Release 3.6 scope.

Clinical access in this phase is intentionally restricted: `clinical.read` and `clinical.write` are granted to `PlatformAdmin` and `TenantAdmin`, while `TenantUser` does not receive clinical permissions.

The next planned functional phase is Release 4 — Odontogram. Release 4 should start only through an explicit bounded slice. Treatments, Billing, Documents/Dashboard and Phase 2 capabilities remain roadmap work after their prerequisite releases.

The current authorization foundation now includes scope-aware JWT claims, explicit permission-based policies, platform override activation only through allowed policies, centralized tenant read/write enforcement in EF Core, `/api/auth/me`, and frontend route/session wiring that stays in memory.

Release 1 is now formally complete. The Patients module covers tenant-scoped patient registration, update, search, and profile retrieval; responsible-party data; active/inactive status; basic clinical alerts; and the small validation guardrails required for the release, including backend enforcement and client-side prevention of future dates of birth.

The repository should be treated as having an established technical and architectural foundation, but not as functionally complete. No functional roadmap release should be assumed closed unless the codebase and aligned documentation explicitly prove it.

---

## Technical Roadmap

### Release 0 — Foundation

* Solution structure
* Multi-tenancy foundation
* Authentication and authorization
* Roles and permissions
* Auditing
* Error handling
* Logging
* Architecture tests
* Initial CI/CD

### Release 1 — Patients

* Patient registration and update
* Search
* General information
* Basic clinical alerts

### Release 2 — Scheduling

* Daily and weekly branch-aware calendar
* Appointment creation, editing, rescheduling, and cancellation
* Appointment statuses
* Appointment notes
* Schedule blocking
* Doctor-based views deferred to a future bounded slice

### Release 3 — Clinical Records

* Release 3 is complete as the foundational clinical release
* Closure evidence: Release 3.1 — Clinical Record Foundation, Release 3.2 — Basic Diagnoses Foundation, Release 3.3 — Clinical Timeline Read Model, Release 3.4 — Clinical Snapshot Change History, Release 3.5 — Medical Questionnaire Backend, and Release 3.6 — Clinical Encounter / Vitals Backend
* Explicit clinical record creation with `GET` returning `404` when missing and no autocreation
* Medical background summary, current medications summary, current allergies, and append-only clinical notes
* Notes returned newest-first in API/UI
* Basic non-coded diagnoses on existing clinical records, with explicit add/resolve flows and `Active` / `Resolved` states
* Diagnosis reads ordered active-first and newest-first within each status group
* Clinical timeline read model inside the existing clinical record using only note-created / diagnosis-created / diagnosis-resolved events, newest-first, with no new endpoint and no new timeline table
* Bounded snapshot history for the base clinical snapshot, returned newest-first and kept separate from the Release 3.3 timeline
* Backend structured medical questionnaire with fixed question keys, `Unknown` / `Yes` / `No` answers, optional bounded details, and `GET` / `PUT /api/patients/{patientId}/clinical-record/questionnaire`
* Bounded frontend integration for the fixed medical questionnaire inside the existing clinical record screen, with no backend/API/permission changes, no automatic allergy synchronization, and Patient age derived read-only from `Patient.DateOfBirth`
* Backend clinical encounters/vitals on an existing clinical record through `GET` / `POST /api/patients/{patientId}/clinical-record/encounters`, with bounded consultation type, optional bounded vitals, and optional linked append-only clinical note
* Bounded frontend integration for clinical encounters/vitals inside the existing clinical record screen, with no backend/API/permission changes, no Patient data duplication, no timeline enrichment, and no later-module linkage
* Full or advanced clinical timeline, any cross-module timeline, restore, full versioning, rich snapshot diff, form builder behavior, automatic allergy synchronization, configurable intake forms, encounter editing/deletion, encounter-specific timeline events, odontogram, treatments, and documents deferred beyond the accepted slices
* `clinical.read` / `clinical.write` restricted to `PlatformAdmin` and `TenantAdmin` in this phase

### Release 4 — Odontogram

* Release 4 is the next planned functional phase after Release 3 closure
* Release 4 is not opened by the Release 3 closure documentation
* Planned bounded slices start with Release 4.1 — Odontogram Foundation
* Explicit odontogram creation with `GET` returning `404` when missing and no autocreation
* Permanent adult FDI tooth numbering only (`11-18`, `21-28`, `31-38`, `41-48`)
* Initial scope should remain bounded to the minimal odontogram foundation before later surfaces/findings/history slices
* Complex findings, treatment linkage, diagnosis linkage, documents, wider dental history/timeline, surface history, restore/full versioning, and advanced charting remain deferred

### Release 5 — Treatments and Quotes

* Planned after Release 4
* Explicit treatment plan creation with `GET` returning `404` when missing and no autocreation
* Exactly one active treatment plan per patient per tenant
* Basic treatment plan items with required title, optional category, simple quantity, short note, and optional adult FDI tooth/surface reference
* Minimal plan lifecycle with `Draft`, `Proposed`, and `Accepted`
* Quotes, pricing, taxes, billing linkage, scheduling linkage, treatment execution tracking, regenerate/versioning workflows, and multi-quote negotiation remain deferred until explicitly opened

### Release 6 — Billing

* Planned after Release 5
* Billing should start only after treatment/quote foundations are intentionally opened
* Payments, balances, receipts, taxes, discounts, CFDI, cancellations, and advanced billing workflows remain deferred

### Release 7 — Documents and Dashboard

* Planned after Release 6
* Tenant-owned and patient-owned `PatientDocument` records with explicit patient-scoped document upload through multipart/form-data
* Active document list with authorized API download and logical retire
* Private local filesystem storage with explicit allowlist `application/pdf`, `image/jpeg`, `image/png` and a 10 MB maximum size
* Basic dashboard should remain read-model oriented
* OCR, rich preview, versioning, external sharing, templates, generated PDFs, advanced analytics, charts, complex filters, branch dashboards, doctor dashboards, exports, and advanced reporting remain deferred

### Phase 2 Expansion — Modern Operations

* Later roadmap phase after the MVP is stable
* Phase 2 is not the next phase after Release 3 closure
* Candidate scope includes reminders, confirmations, online booking, patient communication, patient portal and operational automation only after the MVP is stable
* WhatsApp, email, SMS sending, automatic reminders, providers, jobs, queues, webhooks, external delivery templates, real scheduler workflows, retry automation, campaigns, online booking, patient portal, and advanced dashboards remain deferred

---

## Local Development

> Update this section to match the current runnable setup of the repository; the project is beyond the initial bootstrap stage.

### Expected prerequisites

* .NET SDK 10
* Node.js LTS
* Angular CLI 21
* SQL Server
* Git

### Expected startup flow

#### Backend

```bash
cd backend
dotnet restore
dotnet build
```

#### Frontend

```bash
cd frontend
npm install
npm run start
```

---

## Environment Configuration

> Update this section to match the current environment contract of the repository; the base foundation is already established.

Expected configuration includes:

* SQL Server connection string
* Authentication secrets
* Tenant and platform configuration
* File storage configuration
* Notifications configuration

---

## Testing Strategy

> Update this section to reflect the current automated validation baseline already present in the repository.

The project is expected to include:

* Unit tests
* Integration tests
* Architecture tests
* Frontend unit tests
* End-to-end tests

---

## Contribution Guidelines

### Before submitting changes

* Validate backend and frontend build
* Run relevant tests
* Do not commit secrets
* Respect the architectural structure
* Preserve tenant isolation
* Avoid unnecessary duplication
* Keep components and services focused and clear

### Pull requests

* Explain the purpose of the change
* Indicate affected modules
* Describe risks
* Include test evidence
* Avoid mixing large refactors with unrelated business changes

---

## Quality Goal

Bigsmile should grow as a product that is:

* usable
* elegant
* scalable
* secure
* maintainable
* ready to commercialize

The goal is not only to make it work, but to build a foundation that can support the product for years.

---

## License

Pending definition.
