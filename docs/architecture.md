Perfecto. Para mantener coherencia con el `README.md` que te propuse, te dejo un **`docs/architecture.md` inicial en inglés**, listo para copiar al repo. Está alineado con la visión multi-tenant de Bigsmile, con `Tenant = clinic` y `Branch = clinic location`, y con el MVP dental que definimos.   

````md
# Bigsmile Architecture

## Purpose

This document describes the initial architectural foundation of **Bigsmile**, a SaaS platform for dental clinics and private practices.

Its purpose is to define the structural decisions that guide the implementation of the product from the beginning, ensuring that Bigsmile is:

- scalable
- maintainable
- secure
- multi-tenant by design
- operationally efficient
- ready to evolve into a commercial product

This document focuses on the **initial architectural direction**, not on low-level implementation details.

---

## 1. Architectural Goals

Bigsmile is being built to support:

- a strong operational core for dental clinics
- multiple independent tenants
- multiple branches per tenant
- clear module boundaries
- fast and safe evolution over time
- a pleasant and efficient UX for front desk and clinicians
- product growth without rewriting the foundation

The architecture must support a single clinic at the beginning without becoming a single-tenant system.

---

## 2. Core Business Model

### Main business definitions

- **Tenant**: a clinic or private practice using Bigsmile
- **Branch**: a location or branch that belongs to a tenant
- **Platform Admin**: internal Bigsmile administrator
- **Tenant Admin**: administrator of a tenant
- **Branch User**: operational user assigned to one or more branches

### High-level business flow

The core operational flow of Bigsmile is:

**appointment → patient record → clinical record → odontogram → treatment plan → quote → payment → follow-up**

This flow represents the product backbone and should be optimized both in architecture and UX.

---

## 3. Architectural Style

Bigsmile starts as a **Modular Monolith**.

### Why Modular Monolith

This decision is intentional because it provides:

- faster delivery in the early stages
- simpler operations and deployment
- lower infrastructure complexity
- transactional consistency across modules
- clearer control over cross-cutting concerns
- a better foundation before considering distributed services

### Evolution path

The modular monolith must be designed so that specific modules may be extracted later if needed, but **service decomposition is not an initial goal**.

---

## 4. Technology Stack

## Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

## Frontend
- Angular 21
- TypeScript
- Feature-based architecture
- Lazy loading
- Facades and data-access separation

## Quality and Delivery
- GitHub Actions
- Unit tests
- Integration tests
- Architecture tests
- End-to-end tests
- Structured logging
- Health checks
- Auditing

---

## 5. High-Level Solution Structure

## Backend

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
````

### Responsibilities

#### BigSmile.Api

Responsible for:

* HTTP endpoints
* request/response contracts
* authentication/authorization entry points
* middleware registration
* API composition
* versioning and transport concerns

#### BigSmile.Application

Responsible for:

* use cases
* commands and queries
* handlers
* validations
* authorization orchestration
* transaction orchestration
* application-level policies

#### BigSmile.Domain

Responsible for:

* aggregates
* entities
* value objects
* domain rules
* invariants
* domain events

#### BigSmile.Infrastructure

Responsible for:

* EF Core
* repositories
* persistence
* external integrations
* file storage
* notifications
* auditing persistence
* tenant resolution infrastructure

#### BigSmile.SharedKernel

Responsible for:

* common abstractions
* base entity contracts
* shared value objects
* base exceptions
* common result structures if applicable

#### Test Projects

* **UnitTests**: fast isolated tests
* **IntegrationTests**: DB/infrastructure/API integration validation
* **ArchitectureTests**: enforcement of structural rules

---

## 6. Frontend Structure

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

### Responsibilities

#### core/

Cross-cutting frontend concerns:

* authentication
* session handling
* HTTP infrastructure
* interceptors
* guards
* tenant context
* global error handling

#### shell/

Application shell:

* main layout
* navigation
* branch selector
* global header
* side menu
* authenticated layout composition

#### shared/

Reusable UI and utilities:

* shared UI components
* pipes
* directives
* utility helpers
* form utilities

#### features/

Business features organized by module, each with:

* pages
* components
* facades
* data-access
* models

### Frontend design rules

* avoid oversized pages
* keep components focused
* separate orchestration from presentation
* use facades for feature coordination
* keep API access inside data-access layers

---

## 7. Multi-Tenancy Architecture

Multi-tenancy is a **foundational architectural concern** in Bigsmile.

It must not be implemented as an afterthought or through manual filtering scattered across the codebase.

### 7.1 Tenant Strategy

Initial strategy:

* **shared database**
* **shared schema**
* **TenantId as transversal discriminator**

This is the most pragmatic approach for the initial product stage.

### 7.2 Isolation Rules

* Every tenant-scoped business record must include **TenantId**
* Many operational records also include **BranchId**
* Security isolation is primarily enforced by **TenantId**
* `BranchId` is used for operational segmentation, visibility, scheduling, reporting, and branch-specific permissions

### 7.3 Tenant Resolution

Tenant resolution must be centralized and explicit.

Possible sources may include:

* authenticated user membership
* platform override for internal administration
* explicit platform-level tenant selection where allowed

Tenant resolution must never rely on ad hoc logic spread across handlers or controllers.

Current foundation note:

* authenticated requests resolve user id, tenant id, branch id, and scope from JWT claims
* development headers are only a local fallback for anonymous requests and never override an authenticated identity

### 7.4 Enforcement

Tenant isolation must be enforced through:

* request-scoped `TenantContext`
* entity modeling
* EF Core global query filters
* repository/specification rules
* authorization policies
* integration tests for tenant isolation

Current foundation note:

* authenticated reads are filtered centrally in EF Core for the current tenant unless an explicit platform override is active
* authenticated writes are blocked in `SaveChanges` when the target tenant does not match the current tenant context

### 7.5 Platform Operations

Some platform-level operations require bypassing tenant filters.

These cases must be:

* explicit
* audited
* tightly controlled
* limited to platform contexts only

Current foundation note:

* platform scope alone does not bypass tenant filters
* a request-scoped platform override is activated only by explicit authorization policies/handlers

---

## 8. Branch Model

A **Branch** belongs to a **Tenant**.

Branches are intended to support:

* operational segmentation
* scheduling by location
* user assignment by location
* reporting by location
* future multi-branch management

### Important rule

`BranchId` does not replace `TenantId`.

Every branch-scoped record is still part of a tenant and must remain protected by tenant isolation first.

---

## 9. Bounded Contexts

Bigsmile is structured around business domains, not screens.

## Initial bounded contexts

* Platform
* Identity
* Patients
* Scheduling
* Clinical
* Odontogram
* Treatments
* Billing
* Documents
* Notifications
* Reporting

### 9.1 Platform

Responsible for:

* tenants
* branches
* plans
* feature flags
* tenant settings
* branding
* product-level administration

### 9.2 Identity

Responsible for:

* users
* roles
* permissions
* memberships
* sessions
* access policies

### 9.3 Patients

Responsible for:

* patient profile
* contact data
* responsible party
* search
* basic medical alerts

### 9.4 Scheduling

Responsible for:

* appointments
* calendar
* time blocks
* appointment states
* rescheduling
* no-show tracking

### 9.5 Clinical

Responsible for:

* clinical records
* notes
* background information
* allergies
* diagnoses

### 9.6 Odontogram

Responsible for:

* dental chart
* tooth and surface states
* findings
* visual dental status

### 9.7 Treatments

Responsible for:

* treatment catalog
* treatment plans
* quotes
* treatment lifecycle
* acceptance status

### 9.8 Billing

Responsible for:

* charges
* payments
* balances
* receipts
* cash management
* future invoicing integration

### 9.9 Documents

Responsible for:

* file attachments
* radiographies
* consent forms
* clinical files

### 9.10 Notifications

Responsible for:

* reminders
* confirmations
* future WhatsApp/email flows
* notification templates

### 9.11 Reporting

Responsible for:

* dashboards
* operational KPIs
* appointment metrics
* billing metrics
* treatment conversion metrics

---

## 10. Domain Design Direction

Bigsmile should avoid an anemic domain model for critical business areas.

### Key aggregates expected in the initial foundation

* `Patient`
* `Appointment`
* `ClinicalRecord`
* `Odontogram`
* `TreatmentPlan`
* `Payment`

These aggregates should own important invariants and lifecycle rules.

### Examples of domain responsibilities

#### Patient

* valid registration state
* active/inactive lifecycle
* responsible party consistency
* clinical alert registration

#### Appointment

* valid duration
* state transitions
* rescheduling rules
* cancellation rules
* schedule conflict validation

#### ClinicalRecord

* creation linked to patient
* clinical notes lifecycle
* clinician ownership rules where applicable

#### Odontogram

* tooth/surface state consistency
* finding registration
* status update rules

#### TreatmentPlan

* item composition
* totals consistency
* acceptance workflow
* status transitions

#### Payment

* valid amount
* balance impact
* allocation consistency
* reversal constraints if added

---

## 11. Application Layer Strategy

The Application layer should follow a **use-case oriented** structure.

### Recommended style

* vertical slices
* lightweight CQRS
* handler per use case
* validator per use case

### Expected organization example

```text
Application/
  Patients/
    Commands/
      CreatePatient/
      UpdatePatient/
    Queries/
      GetPatientById/
      SearchPatients/
  Scheduling/
    Commands/
      CreateAppointment/
      RescheduleAppointment/
    Queries/
      GetCalendarView/
```

### Cross-cutting concerns handled in Application

* validation
* authorization orchestration
* transaction boundaries
* auditing triggers
* idempotency where needed
* tenant enforcement coordination

The Application layer should not become a dumping ground for arbitrary service classes.

---

## 12. Infrastructure Direction

Infrastructure must support the domain and application layers without leaking transport or persistence concerns into business rules.

### Responsibilities

* EF Core DbContext
* migrations
* repositories
* specifications or query abstractions
* auditing persistence
* tenant resolution services
* file storage providers
* notification providers
* integration adapters
* outbox/integration event infrastructure if needed

### Database conventions

* `TenantId` for tenant-scoped entities
* `BranchId` when operationally needed
* audit columns where appropriate
* concurrency tokens in critical records
* explicit indexing strategy
* migrations under version control

---

## 13. Security Architecture

Security is not optional in Bigsmile.

### Initial goals

* secure authentication
* role and permission-based authorization
* audit trail for critical actions
* safe secrets management
* tenant-aware access enforcement
* consistent backend validation
* predictable error contracts

### Required rules

* no secrets in the repository
* no insecure tenant bypasses
* no business-critical authorization logic only in frontend
* no manual tenant isolation scattered across the codebase

### Access model

Initial access model should support:

* Platform Admin
* Tenant Admin
* Dentist
* Front Desk
* Assistant

Permissions should be explicit and extensible over time.

---

## 14. UX and Frontend Experience Principles

Bigsmile must prioritize speed and clarity for operational users.

### UX goals

* common tasks in very few steps
* fast patient search
* fast appointment creation
* clear clinical workflow
* visual odontogram experience
* understandable treatment and quote flow
* quick payment registration

### UX principle

Operational speed is a product feature.

The system must not feel heavy for front desk or clinicians.

---

## 15. Initial MVP Modules

The initial MVP includes:

* Patients
* Scheduling
* Clinical Records
* Odontogram
* Treatments and Quotes
* Billing / Cash
* Documents
* Users / Roles
* Basic Dashboard

This MVP is intended to validate the core business value before expanding into automation-heavy features.

---

## 16. Future Expansion Areas

The architecture must be ready to support future features such as:

* WhatsApp reminders
* email reminders
* online booking
* electronic invoicing
* advanced multi-branch administration
* patient portal
* advanced analytics
* automated follow-up workflows
* inventory basics

These future features must not force a rewrite of the current structure.

---

## 17. Quality Strategy

Bigsmile should include automated quality controls from the beginning.

### Required quality layers

* backend unit tests
* backend integration tests
* frontend unit tests
* end-to-end tests
* architecture tests
* CI pipeline validation

### Important goal

Architecture rules should be enforced automatically, not only by convention.

---

## 18. Initial Non-Goals

To keep the foundation focused, the following are not immediate goals for the initial stage:

* microservices
* overly complex distributed architecture
* advanced insurance/claims flows
* full ERP functionality
* advanced AI workflows
* premature scaling infrastructure

The objective is to build the right base, not to over-engineer the first version.

---

## 19. Architectural Risks to Avoid

The project must actively avoid:

* manual tenant filtering everywhere
* giant service classes
* oversized frontend pages
* secrets committed to the repository
* weak authorization boundaries
* business rules duplicated across layers
* product growth without modular boundaries

---

## 20. Guiding Principle

Bigsmile should evolve as a product, not as a collection of disconnected features.

Every architectural decision should be evaluated through this lens:

**Will this help Bigsmile remain secure, maintainable, scalable, and commercially viable over time?**

````
