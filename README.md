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

The latest completed delivery phase is **Release 2 — Scheduling**.

Release 2 is formally complete with branch-aware daily and weekly calendar views, appointment create/edit/reschedule/cancel flows, appointment notes, blocked slots, and explicit attended/no-show states.

Doctor-based views are explicitly deferred to a future bounded slice because they require provider/doctor assignment rather than a small UI-only filter.

The next planned phase is **Release 3 — Clinical Records**.

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

* Clinical record
* Clinical notes
* Patient history

### Release 4 — Odontogram

* Interactive odontogram
* Findings registration
* Visual dental state tracking

### Release 5 — Treatments

* Treatment catalog
* Treatment plans
* Quotes
* Acceptance workflow

### Release 6 — Billing

* Charges
* Payments
* Balances
* Receipts

### Release 7 — Documents and Dashboard

* Attachments and files
* Operational dashboard

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
