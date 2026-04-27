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

Current active phase:

* **Phase 2 Expansion — Modern Operations:** open in progress
* **Accepted Phase 2 slices:** **Phase 2.1 — Appointment Confirmation Foundation**, **Phase 2.2 — Manual Reminder Log Foundation**, **Phase 2.3 — Reminder Scheduling Preparation**, and **Phase 2.4 — Reminder Worklist Follow-up Actions**
* **Accepted MVP slices preserved:** **Release 7.1 — Documents Foundation** and **Release 7.2 — Dashboard Foundation**
* **Accepted upstream slice preserved:** **Release 6.1 — Billing Foundation**

The latest completed delivery phase remains **Release 2 — Scheduling**.

Release 2 is formally complete with branch-aware daily and weekly calendar views, appointment create/edit/reschedule/cancel flows, appointment notes, blocked slots, and explicit attended/no-show states.

Doctor-based views are explicitly deferred to a future bounded slice because they require provider/doctor assignment rather than a small UI-only filter.

Release 3 is now open. Release 3.1 is accepted with tenant-owned, patient-owned clinical records; explicit record creation; `GET` returning `404` when the record does not exist; no autocreation; medical background and current medications summaries; current allergies; append-only clinical notes returned newest-first; and minimal clinician attribution.

Release 3.2 is also accepted and adds only the bounded diagnoses foundation on top of an existing clinical record: explicit diagnosis creation, explicit diagnosis resolution, diagnoses included in the clinical record read model, basic non-coded diagnosis text/notes, and `Active` / `Resolved` status with active-first ordering and newest-first ordering within each status group in reads and UI.

Release 3.3 is also accepted and adds only a bounded clinical timeline read model inside the existing clinical record read contract: no new endpoint, no new timeline table, no cross-module timeline, and only `ClinicalNoteCreated`, `ClinicalDiagnosisCreated`, and `ClinicalDiagnosisResolved` events returned newest-first from existing Clinical data.

Release 3.4 is also accepted and adds only a bounded snapshot change history inside the existing clinical record read contract: an initial history entry when the record is created explicitly, additional entries only for effective changes to medical background, current medications, and current allergies, and a separate `snapshotHistory` section that does not merge with the accepted Release 3.3 timeline. This bounded history does not introduce restore, full record versioning, or rich diffs.

Release 3 remains preserved through the accepted slices above. The full or advanced clinical timeline, any cross-module timeline, restore, and full clinical record versioning remain outside the accepted Release 3 scope.

Clinical access in this phase is intentionally restricted: `clinical.read` and `clinical.write` are granted to `PlatformAdmin` and `TenantAdmin`, while `TenantUser` does not receive clinical permissions.

Release 4 is now open with Release 4.1 accepted as the minimal odontogram foundation, Release 4.2 accepted as the minimal surface foundation, Release 4.3 accepted as the basic dental findings foundation, and Release 4.4 accepted as a bounded dental findings change history slice on top of them: explicit `POST /api/patients/{patientId}/odontogram`, `GET` returning `404` when missing, no autocreation, 32 permanent adult teeth using FDI/ISO two-digit numbering (`11-18`, `21-28`, `31-38`, `41-48`), minimal O/M/D/B/L surface detail, explicit tooth and surface updates, explicit `POST` / `DELETE` basic findings operations per surface returning the updated odontogram, and a root-level `findingsHistory` read-model section that records only `FindingAdded` / `FindingRemoved` events newest-first for the basic findings catalog.

Odontogram access in the accepted Release 4.4 slice remains intentionally restricted: `odontogram.read` and `odontogram.write` are granted to `PlatformAdmin` and `TenantAdmin`, while `TenantUser` does not receive odontogram permissions. The accepted basic findings catalog remains intentionally small (`Caries`, `Restoration`, `MissingStructure`, `Sealant`) and stays separate from tooth/surface status. The accepted findings history is bounded on purpose: it stays separate from any future dental timeline and does not introduce restore, full odontogram versioning, treatment linkage, diagnosis linkage, or surface history. Complex findings, treatment linkage, documents, wider dental timeline/history, surface history, bulk editing, and advanced charting remain outside Release 4.4.

Release 5 is now open with Release 5.1 accepted as the minimal treatment plan foundation: explicit `POST /api/patients/{patientId}/treatment-plan`, `GET` returning `404` when missing, no autocreation, exactly one active treatment plan per patient per tenant, basic add/remove item flows through `POST /items` and `DELETE /items/{itemId}`, and explicit `PUT /status` over a bounded `Draft` / `Proposed` / `Accepted` lifecycle.

Treatment plan items in Release 5.1 stay intentionally small: required title, optional category, simple integer quantity, short note, and optional dental reference through adult permanent FDI `toothCode` plus optional O/M/D/B/L `surfaceCode` when a tooth is present. Release 5.1 intentionally does not open pricing, discounts, taxes, billing linkage, scheduling linkage, treatment execution tracking, or plan versioning.

Treatment plan access in the accepted Release 5.1 slice is intentionally restricted: `treatmentplan.read` and `treatmentplan.write` are granted to `PlatformAdmin` and `TenantAdmin`, while `TenantUser` does not receive treatment plan permissions.

Release 5.2 — Quote Basics is now also accepted on top of that foundation: explicit `POST /api/patients/{patientId}/treatment-plan/quote`, `GET` returning `404` when the quote does not exist, no autocreation, exactly one quote per treatment plan, snapshot-only quote items copied from the current treatment plan, fixed `MXN` currency in this slice, line-level `UnitPrice`, basic line totals and quote total, and explicit `PUT` operations for line price and overall quote status across the same bounded `Draft` / `Proposed` / `Accepted` lifecycle. In this accepted slice, a `Proposed` quote must keep all line prices positive and `Proposed -> Accepted` revalidates that invariant.

Treatment quote access in the accepted Release 5.2 slice is intentionally restricted: `treatmentquote.read` and `treatmentquote.write` are granted to `PlatformAdmin` and `TenantAdmin`, while `TenantUser` does not receive treatment quote permissions. Advanced pricing, discounts, taxes, billing linkage, scheduling linkage, regenerate/versioning workflows, and multi-quote negotiation remain outside this slice.

Release 6 remains open and is currently preserved through the accepted Release 6.1 — Billing Foundation slice. This accepted slice adds only explicit billing document creation from an accepted quote, `GET` returning `404` when the billing document does not exist, no autocreation, exactly one billing document per quote, snapshot-only billing lines copied from the accepted quote, inherited simple currency handling from that accepted quote, currently `MXN` in the repo implementation, bounded `Draft` / `Issued` status with explicit issue, and read-only behavior once issued.

Billing access in the accepted Release 6.1 slice is intentionally restricted: `billing.read` and `billing.write` are granted to `PlatformAdmin` and `TenantAdmin`, while `TenantUser` does not receive billing permissions. Payments, balances, receipts, taxes, discounts, CFDI/PAC, cancellations, multi-billing, and advanced billing workflows remain outside this accepted slice and Release 6 itself is not yet closed.

Release 7 remains open in the repository and is currently preserved through the accepted Release 7.1 — Documents Foundation and Release 7.2 — Dashboard Foundation slices. Release 7.1 adds only tenant-owned, patient-owned `PatientDocument` records with explicit patient-scoped document upload through multipart/form-data, active document listing, authorized download through the API, logical retire for mistaken uploads, private local filesystem storage, an allowlist limited to `application/pdf`, `image/jpeg`, and `image/png`, and a simple 10 MB maximum size. Documents are never auto-created in this slice, and OCR, rich preview, thumbnails, versioning, public sharing, templates, and generated PDFs remain outside the accepted scope.

Document access in the accepted Release 7.1 slice is intentionally restricted: `document.read` and `document.write` are granted to `PlatformAdmin` and `TenantAdmin`, while `TenantUser` does not receive document permissions.

Release 7.2 — Dashboard Foundation is accepted without closing Release 7. It adds only a tenant-scoped operational dashboard summary through `GET /api/dashboard/summary`, simple KPI cards, and the `dashboard.read` permission for `TenantAdmin`. `TenantUser` does not receive dashboard permissions in this phase. `PlatformAdmin` also does not receive `dashboard.read` in this phase because the tenant-scoped dashboard does not yet have a safe platform tenant-selection path. The implementation does not add dashboard tables or persisted snapshots, and does not open advanced analytics, charts, complex filters, branch dashboards, doctor dashboards, exports, real-time alerts, or historical reporting.

Phase 2 Expansion — Modern Operations is now open in the repo through the accepted Phase 2.1 — Appointment Confirmation Foundation, Phase 2.2 — Manual Reminder Log Foundation, Phase 2.3 — Reminder Scheduling Preparation, and Phase 2.4 — Reminder Worklist Follow-up Actions slices. Phase 2.1 adds only a separate appointment confirmation status (`Pending` / `Confirmed`) on existing appointments, confirmation metadata (`confirmedAtUtc`, `confirmedByUserId`), a minimal `PUT /api/appointments/{id}/confirmation` API protected by existing `scheduling.write`, enriched scheduling reads through existing `scheduling.read`, and minimal scheduling UI actions to confirm or mark pending. It adds no new permissions and does not implement WhatsApp, email, SMS, automatic reminders, external providers, jobs, queues, webhooks, online booking, patient portal, templates, campaigns, or advanced dashboard behavior.

Phase 2.2 — Manual Reminder Log Foundation is accepted. It adds only a tenant-owned manual contact attempt log for existing appointments, with `GET /api/appointments/{id}/reminder-log` and `POST /api/appointments/{id}/reminder-log`, minimal channel values (`Phone`, `WhatsApp`, `Email`, `Other`), minimal outcomes (`Reached`, `NoAnswer`, `LeftMessage`), optional short notes, and `createdAtUtc` / `createdByUserId` metadata. This is manual log only: BigSmile does not send WhatsApp, email, or SMS messages in this slice, does not add providers, jobs, queues, webhooks, templates, automatic reminders, scheduler workflows, retry behavior, campaigns, patient portal, online booking, or advanced dashboard behavior, and does not change `AppointmentStatus` or `AppointmentConfirmationStatus`. It reuses `scheduling.read` / `scheduling.write` and adds no new permissions.

Phase 2.3 — Reminder Scheduling Preparation is accepted. It adds only manual reminder preparation on existing appointments: `ReminderRequired`, a preferred manual channel, a target reminder date/time, manual completion metadata, explicit set/clear/complete APIs, and a branch-aware pending/due reminder list inside Scheduling. This slice does not send WhatsApp, email, or SMS, does not add delivery providers, jobs, queues, webhooks, templates, a real scheduler, retry automation, campaigns, patient portal, online booking, or advanced dashboard behavior, does not automatically change `AppointmentStatus` or `AppointmentConfirmationStatus`, does not auto-create reminder log entries, reuses `scheduling.read` / `scheduling.write`, and adds no new permissions.

Phase 2.4 — Reminder Worklist Follow-up Actions is accepted. It adds only an explicit manual follow-up action from the pending/due reminder worklist: the operation creates exactly one manual reminder log entry, optionally completes the manual reminder only when `completeReminder = true`, and optionally confirms the appointment only when `confirmAppointment = true`. It does not auto-complete or auto-confirm when `outcome = Reached`, does not change `AppointmentStatus`, does not change `ReminderDueAtUtc` or `ReminderChannel`, adds no permissions, adds no persistence schema, and still does not send WhatsApp, email, or SMS or add providers, jobs, queues, webhooks, templates, a real scheduler, retry automation, campaigns, patient portal, online booking, or advanced dashboard behavior.

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

* Release 3 is in progress
* Accepted slices: Release 3.1 — Clinical Record Foundation, Release 3.2 — Basic Diagnoses Foundation, Release 3.3 — Clinical Timeline Read Model, and Release 3.4 — Clinical Snapshot Change History
* Explicit clinical record creation with `GET` returning `404` when missing and no autocreation
* Medical background summary, current medications summary, current allergies, and append-only clinical notes
* Notes returned newest-first in API/UI
* Basic non-coded diagnoses on existing clinical records, with explicit add/resolve flows and `Active` / `Resolved` states
* Diagnosis reads ordered active-first and newest-first within each status group
* Clinical timeline read model inside the existing clinical record using only note-created / diagnosis-created / diagnosis-resolved events, newest-first, with no new endpoint and no new timeline table
* Bounded snapshot history for the base clinical snapshot, returned newest-first and kept separate from the Release 3.3 timeline
* Full or advanced clinical timeline, any cross-module timeline, restore, full versioning, rich snapshot diff, odontogram, treatments, and documents deferred beyond the accepted slices
* `clinical.read` / `clinical.write` restricted to `PlatformAdmin` and `TenantAdmin` in this phase

### Release 4 — Odontogram

* Release 4 is in progress
* Accepted slices: Release 4.1 — Odontogram Foundation, Release 4.2 — Odontogram Surface Foundation, Release 4.3 — Basic Dental Findings Foundation, and Release 4.4 — Dental Findings Change History
* Explicit odontogram creation with `GET` returning `404` when missing and no autocreation
* Permanent adult FDI tooth numbering only (`11-18`, `21-28`, `31-38`, `41-48`)
* Tooth-level current state with `Unknown`, `Healthy`, `Missing`, `Restored`, and `Caries`
* Minimal O/M/D/B/L surface detail with `Unknown`, `Healthy`, `Restored`, and `Caries`
* Minimal basic surface findings with explicit add/remove per surface and a small catalog `Caries`, `Restoration`, `MissingStructure`, and `Sealant`
* Bounded `findingsHistory` for basic finding add/remove changes only, returned newest-first and kept separate from any future dental timeline
* Minimal patient-context UI for empty state, explicit creation, grid/list visualization, single-tooth state updates, per-surface editing, and per-surface findings editing inside the existing odontogram page
* `odontogram.read` / `odontogram.write` restricted to `PlatformAdmin` and `TenantAdmin` in this phase
* Complex findings, treatment linkage, diagnosis linkage, documents, wider dental history/timeline, surface history, restore/full versioning, and advanced charting deferred beyond Release 4.4

### Release 5 — Treatments and Quotes

* Release 5 is in progress
* Accepted slices: Release 5.1 — Treatment Plan Foundation and Release 5.2 — Quote Basics
* Explicit treatment plan creation with `GET` returning `404` when missing and no autocreation
* Exactly one active treatment plan per patient per tenant
* Basic treatment plan items with required title, optional category, simple quantity, short note, and optional adult FDI tooth/surface reference
* Minimal plan lifecycle with `Draft`, `Proposed`, and `Accepted`
* Minimal patient-context UI for empty state, explicit creation, item add/remove, and status updates
* `treatmentplan.read` / `treatmentplan.write` restricted to `PlatformAdmin` and `TenantAdmin` in this phase
* Minimal quote creation from the existing treatment plan, snapshot-only quote items, fixed `MXN` currency, line-level pricing, basic totals, and bounded `Draft` / `Proposed` / `Accepted` quote status with positive pricing preserved through `Proposed` and revalidated on `Accepted`
* `treatmentquote.read` / `treatmentquote.write` restricted to `PlatformAdmin` and `TenantAdmin` in this phase
* Advanced pricing, taxes, discounts, billing linkage, scheduling linkage, treatment execution tracking, regenerate/versioning workflows, and multi-quote negotiation deferred beyond the accepted Release 5.2 slice

### Release 6 — Billing

* Release 6 is now open
* Accepted current slice: Release 6.1 — Billing Foundation
* Explicit billing document creation from an accepted quote with `GET` returning `404` when missing and no autocreation
* Exactly one billing document per quote in this slice
* Snapshot-only billing lines copied from the accepted quote with inherited simple currency handling, currently `MXN` in the repo implementation
* Explicit `Draft` / `Issued` billing status with read-only behavior once issued
* `billing.read` / `billing.write` restricted to `PlatformAdmin` and `TenantAdmin`
* Payments, balances, receipts, taxes, discounts, CFDI, cancellations, and advanced billing workflows deferred beyond the accepted Release 6.1 slice

### Release 7 — Documents and Dashboard

* Release 7 is now open in the repository
* Accepted current slices: Release 7.1 — Documents Foundation and Release 7.2 — Dashboard Foundation
* Tenant-owned and patient-owned `PatientDocument` records with explicit patient-scoped document upload through multipart/form-data
* Active document list with authorized API download and logical retire
* Private local filesystem storage with explicit allowlist `application/pdf`, `image/jpeg`, `image/png` and a 10 MB maximum size
* `document.read` / `document.write` restricted to `PlatformAdmin` and `TenantAdmin`; `TenantUser` does not receive document permissions in this phase
* Tenant-scoped dashboard summary implemented as a bounded read model with active patients, today appointments, today pending appointments, active documents, active treatment plans, accepted quotes, issued billing documents, and generated timestamp
* `dashboard.read` restricted to `TenantAdmin`; `TenantUser` and `PlatformAdmin` do not receive dashboard permissions in this phase
* OCR, rich preview, versioning, external sharing, templates, generated PDFs, advanced analytics, charts, complex filters, branch dashboards, doctor dashboards, exports, and advanced reporting remain outside the current accepted scope

### Phase 2 Expansion — Modern Operations

* Phase 2 is open in progress
* Accepted slices: Phase 2.1 — Appointment Confirmation Foundation, Phase 2.2 — Manual Reminder Log Foundation, Phase 2.3 — Reminder Scheduling Preparation, and Phase 2.4 — Reminder Worklist Follow-up Actions
* Appointment confirmation status is separate from `AppointmentStatus`
* Minimal confirmation catalog: `Pending` and `Confirmed`
* Existing scheduling permissions are reused: `scheduling.read` and `scheduling.write`
* No new permissions are added
* Confirmation changes are blocked for terminal appointment statuses: `Cancelled`, `Attended`, and `NoShow`
* Manual reminder/contact attempts can be logged per appointment in Phase 2.2 without changing appointment status or confirmation status
* Manual reminder preparation can be configured, cleared, completed, and listed in Phase 2.3 without sending messages and without coupling automatically to the reminder log
* Manual reminder follow-up can be recorded from the worklist in Phase 2.4, creating exactly one log entry and changing confirmation/completion only through explicit request flags
* WhatsApp, email, SMS sending, automatic reminders, providers, jobs, queues, webhooks, templates, real scheduler workflows, retry automation, campaigns, online booking, patient portal, and advanced dashboards remain outside Phase 2.4

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
