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
- Billing document issuance
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

Releases 4 — Odontogram, 5 — Treatments and Quotes, 6 — Billing, and 7 — Documents and Dashboard are formally accepted after module-specific audits of domain, application/API, persistence, permissions, frontend and automated tests.

Release 7 closure also formally accepts the initial operational MVP. This is a bounded product milestone: issued Billing documents do not imply payments/cash/CFDI, Documents do not imply OCR/sharing, and Dashboard does not imply advanced analytics.

Code in later capabilities such as reminders/manual reminders, providers, jobs, online booking, Phase 2 patient intake or advanced analytics does not imply acceptance. Visual slices may improve presentation and UX debt without changing backend behavior, APIs, permissions, auth, tenant context, branch context, migrations or functional scope.

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

Completed foundation and functional milestones:

* **Foundation / Release 0 base**
* **Pre-auth hardening**
* **Identity + Persistence Foundation**
* **Tenant-Aware Authorization Foundation**
* **Release 1 — Patients**
* **Release 2 — Scheduling**
* **Release 3 — Clinical Records**
* **Release 4 — Odontogram**
* **Release 5 — Treatments and Quotes**
* **Release 6 — Billing**
* **Release 7 — Documents and Dashboard**

Current roadmap position:

* **Latest completed delivery phase:** **Release 7 — Documents and Dashboard**
* **Initial operational MVP:** **formally accepted**
* **Next planned phase:** **Phase 2.1 — Patient Intake and Portal Foundation**
* **Phase 2.1 runtime status:** architecture accepted in ADR 006; PI-1 to PI-4 not implemented or automatically opened

Release 2 is formally complete with branch-aware daily and weekly calendar views, appointment create/edit/reschedule/cancel flows, appointment notes, blocked slots, and explicit attended/no-show states.

Doctor-based views are explicitly deferred to a future bounded slice because they require provider/doctor assignment rather than a small UI-only filter.

Release 3 is formally complete as the foundational clinical release through accepted slices 3.1 to 3.6. It includes explicit clinical-record creation, medical background/current medications/current allergies, append-only notes, basic diagnoses, a bounded clinical timeline, separate snapshot history, the fixed medical questionnaire, and clinical encounters/vitals.

The fixed questionnaire uses the accepted `Unknown` / `Yes` / `No` catalog with optional bounded details. Patient demographics remain owned by Patients, age is derived from date of birth, and the questionnaire does not automatically modify allergies, alerts, timeline or later modules.

Clinical access remains restricted to `clinical.read` / `clinical.write` for `PlatformAdmin` and `TenantAdmin`; `TenantUser` does not receive clinical permissions.

Release 4 is formally complete as the foundational Odontogram release through:

* **Release 4.1 — Odontogram Foundation**
* **Release 4.2 — Odontogram Surface Foundation**
* **Release 4.3 — Basic Dental Findings Foundation**
* **Release 4.4 — Dental Findings Change History**

The accepted Odontogram boundary includes explicit creation and `404` when missing, one tenant-owned/patient-owned chart per patient/tenant, 32 permanent adult FDI teeth, bounded tooth and surface states, five `O/M/D/B/L` surfaces, a small basic finding catalog, explicit finding add/remove and append-only finding history returned newest-first.

Odontogram reads/writes use `odontogram.read` / `odontogram.write`. `TenantUser` does not receive those permissions. The supported path remains through the tenant-owned aggregate root; future direct child-table queries must be tenant-aware.

Advanced Odontogram capabilities remain deferred: child/mixed dentition, bulk editing, full tooth/surface history, restore/versioning, treatment/diagnosis/document linkage, advanced orthodontic/periodontal charting, imaging overlays and AI detection.

Release 4 closure evidence:

* `docs/release-4-odontogram-audit-and-closure.md`
* ADR 007 — `docs/decisions/007-release-4-odontogram-closure.md`

Release 5 is formally complete as the foundational Treatments and Quotes release through:

* **Release 5.1 — Treatment Plan Foundation**
* **Release 5.2 — Quote Foundation**

The accepted Treatment Plan boundary includes explicit creation and `404` when missing, one tenant-owned/patient-owned plan per patient/tenant in the current slice, basic items with optional permanent-adult FDI tooth/surface references, explicit item add/remove, a bounded `Draft / Proposed / Accepted` lifecycle and accepted-plan immutability.

The accepted Quote boundary includes explicit snapshot creation from an existing non-empty plan, one quote per plan, no autocreation, fixed `MXN` in the public application/API path, line-level pricing, calculated line/quote totals, positive-price gates for `Proposed`/`Accepted` and accepted-quote immutability.

Treatment and quote reads/writes use `treatmentplan.*` and `treatmentquote.*`. `TenantUser` does not receive those permissions. Treatment/quote items remain child records accessed through tenant-owned aggregate roots.

Advanced commercial and execution capabilities remain deferred: treatment catalog administration, multiple or archived plans, regenerate/versioning, multiple quotes or negotiation, taxes, discounts, Scheduling linkage, treatment execution/progress, insurance, financing, advanced approvals and Patient Portal access. Billing is accepted separately through Release 6.1; payments remain deferred.

Release 5 closure evidence:

* `docs/release-5-treatments-and-quotes-audit-and-closure.md`
* ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`

Release 6 is formally complete through:

* **Release 6.1 — Billing Document Foundation**

The accepted Billing boundary includes explicit snapshot creation from an existing accepted quote, one Billing document per quote, no autocreation, preserved currency and `decimal(18,2)` line/total amounts, a bounded `Draft -> Issued` lifecycle, issue actor/time metadata and issued-document immutability.

Billing reads/writes use `billing.read` / `billing.write`. `TenantUser` does not receive those permissions. Billing lines remain child records accessed through the tenant-owned aggregate root.

Payments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency, accounting and Patient Portal access remain deferred.

Release 6 closure evidence:

* `docs/release-6-billing-audit-and-closure.md`
* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

Release 7 is formally complete through:

* **Release 7.1 — Patient Documents Foundation**
* **Release 7.2 — Dashboard Read Model Foundation**

The accepted Documents boundary includes tenant/patient-owned metadata, explicit authorized upload/list/download/logical retire, private root-contained storage, PDF/JPEG/PNG type-plus-signature validation, bounded upload size and tenant/cross-tenant protection.

The accepted Dashboard boundary is read-only and tenant-scoped, with active patients, tenant-local today/pending appointments, active documents, treatment plans, accepted quotes and issued Billing documents. ADR 010 makes `Tenant.TimeZoneId` the server-authoritative source of the clinic operational day.

Release 7 closure evidence:

* `docs/release-7-documents-and-dashboard-audit-and-closure.md`
* ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`
* ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`

The current authorization foundation includes scope-aware JWT claims, explicit permission policies, policy-gated platform override, centralized tenant read/write enforcement in EF Core, `/api/auth/me`, and frontend session state in memory.

The initial operational MVP is accepted, but Bigsmile is not feature-complete. Payments/cash/CFDI, provider views, automated messaging, online booking, Phase 2.1 implementation, advanced analytics and the full Patient Portal remain future bounded work.

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

* Completed foundational clinical release through slices 3.1 to 3.6
* Explicit record creation with no autocreation
* Snapshot, current allergies, append-only notes and basic diagnoses
* Bounded timeline and separate snapshot history
* Fixed medical questionnaire
* Clinical encounter / vital-sign capture
* Advanced clinical history, form builder, auto-sync and cross-module timeline remain deferred

### Release 4 — Odontogram

* Completed foundational Odontogram release through slices 4.1 to 4.4
* Explicit odontogram creation with `GET` returning `404` when missing and no autocreation
* Exactly one odontogram per patient/tenant
* Permanent adult FDI numbering (`11-18`, `21-28`, `31-38`, `41-48`)
* Bounded tooth status and explicit updates
* Five bounded surfaces `O/M/D/B/L` and explicit surface updates
* Basic surface findings with explicit add/remove
* Append-only finding change history
* Advanced charting, full history/versioning and cross-module linkage deferred

### Release 5 — Treatments and Quotes

* Completed foundational Treatments and Quotes release through slices 5.1 and 5.2
* Explicit treatment-plan creation with `GET` returning `404` when missing and no autocreation
* One plan per patient/tenant in the current slice
* Basic items with optional adult FDI tooth/surface references
* `Draft / Proposed / Accepted` lifecycle and accepted-plan immutability
* Explicit quote snapshot creation from a non-empty plan
* One quote per plan, fixed `MXN`, line pricing and calculated totals
* Positive pricing gates and accepted-quote immutability
* Treatment execution, taxes/discounts, Scheduling linkage, versioning and negotiation remain deferred; Billing is accepted separately through Release 6.1

### Release 6 — Billing

* Completed through Release 6.1 — Billing Document Foundation
* Explicit creation from an accepted quote with `GET` returning `404` when missing and no autocreation
* One Billing document per quote and snapshot-only lines
* Preserved currency, unit price, line total and total with SQL precision `18,2`
* `Draft -> Issued` lifecycle with issue metadata and issued read-only behavior
* Payments, balances, receipts, cash management, taxes/discounts and CFDI remain deferred

### Release 7 — Documents and Dashboard

* Completed through Release 7.1 — Patient Documents Foundation and Release 7.2 — Dashboard Read Model Foundation
* Private tenant/patient-owned documents with explicit upload/list/download/logical retire
* PDF/JPEG/PNG declared-type plus binary-signature validation, 10 MB file limit and root-contained storage
* Read-only tenant-scoped Dashboard with tenant-local operational day from `Tenant.TimeZoneId`
* OCR, public sharing, generated PDFs, payments/revenue metrics, charts, exports and branch/doctor dashboards remain deferred

### Phase 2 Expansion — Modern Operations

* Next planned phase after formal MVP acceptance; not automatically opened
* **Phase 2.1 — Patient Intake and Portal Foundation** is architecturally accepted in ADR 006, but PI-1 to PI-4 are not implemented
* The bounded capability includes patient activation, intake/update, clinic review/application and append-only audit
* The full patient portal, automated messaging, online booking, providers, jobs, queues, campaigns and advanced dashboards remain deferred

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

The repository validation baseline includes:

* Backend unit tests
* Backend integration tests
* Architecture validation/tests
* Frontend unit tests
* End-to-end tests where relevant
* GitHub Actions CI

High-risk changes involving tenant handling, authorization, patient/clinical data, billing, documents or platform override require explicit automated evidence.

---

## Contribution Guidelines

### Before submitting changes

* Validate backend and frontend build
* Run relevant tests
* Do not commit secrets
* Respect architectural structure
* Preserve tenant isolation
* Avoid unnecessary duplication
* Keep components and services focused
* Update canonical documentation when project state changes

### Pull requests

* Explain purpose and affected modules
* Describe architectural and tenant/security implications
* Describe risks and reversibility
* Include test evidence
* Separate unrelated refactors from business changes

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
