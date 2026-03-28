Perfecto. Te dejo el contenido para:

`docs/decisions/001-architecture-principles.md`

````md
# ADR 001 — Initial Architecture Principles

- **Status:** Accepted
- **Date:** 2026-03-28
- **Decision Type:** Foundational
- **Scope:** Product-wide
- **Applies To:** Backend, Frontend, Data, Security, Multi-tenancy, Delivery

---

## Context

Bigsmile is being built as a SaaS platform for dental clinics and private practices.

The product starts with a focused operational MVP, but it must be designed from the beginning to support:
- multiple tenants
- multiple branches per tenant
- independent administration
- secure tenant isolation
- long-term maintainability
- incremental product growth
- commercial product evolution

The project must avoid becoming:
- a single-tenant app that is difficult to evolve
- a collection of disconnected modules
- a codebase dependent on manual discipline for critical concerns
- a system that solves short-term needs while damaging the product foundation

This ADR establishes the initial principles that must guide all architectural and implementation decisions.

---

## Decision

Bigsmile will be built under the following initial architectural principles.

---

## 1. Bigsmile is a Product, Not a Custom Internal App

Bigsmile must be designed as a **commercial SaaS product**.

This means:
- product consistency matters
- architectural clarity matters
- maintainability matters
- extensibility matters
- operational safety matters
- tenant isolation is non-negotiable

The system must not be optimized only for the first clinic or first release.

---

## 2. Multi-Tenancy Is Foundational

Multi-tenancy is part of the architecture from day one.

### Decision
- **Tenant = clinic or private practice**
- **Branch = internal location of a tenant**

### Implications
- every tenant-owned business record must belong to a tenant
- tenant isolation is enforced primarily by `TenantId`
- `BranchId` is operational, not the primary security boundary
- multi-tenancy must be enforced by architecture, not by developer memory

### Consequences
- no single-tenant assumptions in the domain model
- no manual tenant filtering scattered across the codebase as the main strategy
- no unsafe cross-tenant access
- no hidden platform bypass

---

## 3. Architectural Style: Modular Monolith First

Bigsmile starts as a **Modular Monolith**.

### Decision
The system will be built as a modular monolith instead of microservices.

### Rationale
This allows:
- faster delivery
- simpler deployment
- stronger transactional consistency
- lower operational complexity
- easier early-stage evolution
- clearer domain boundaries before distribution

### Consequences
- modules must be clearly separated
- module boundaries must be treated seriously
- future extraction of services is possible, but not an immediate goal
- distributed complexity is intentionally postponed

---

## 4. Backend Must Follow Clear Layer Responsibilities

The backend will follow a layered structure with explicit responsibilities.

### Decision
Initial backend structure:

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

### Layer responsibilities

* **Api**: transport and composition
* **Application**: use cases
* **Domain**: business rules and invariants
* **Infrastructure**: persistence and external services
* **SharedKernel**: shared abstractions and common primitives

### Consequences

* business logic must not live in controllers
* infrastructure concerns must not leak into domain logic
* application flow must remain explicit
* architectural tests should enforce boundaries

---

## 5. Application Layer Is Use-Case Oriented

The Application layer must be organized around use cases, not generic service dumping.

### Decision

Bigsmile will use:

* vertical slices
* lightweight CQRS
* focused commands and queries
* per-use-case validation
* explicit handlers

### Rationale

This structure improves:

* clarity
* testability
* modularity
* consistency across features
* separation of concerns

### Consequences

* avoid oversized application services
* avoid generic service classes with mixed responsibilities
* cross-cutting concerns should be coordinated systematically
* new features should be added by use case, not by random utility growth

---

## 6. The Domain Must Own Critical Business Rules

The domain should not become purely anemic in critical areas.

### Decision

Important aggregates must own real invariants and lifecycle rules.

Expected key aggregates include:

* `Patient`
* `Appointment`
* `ClinicalRecord`
* `Odontogram`
* `TreatmentPlan`
* `Payment`

### Rationale

Critical business rules should not be scattered across:

* controllers
* repositories
* frontend code
* unrelated helper classes

### Consequences

* important lifecycle rules must be explicit
* business invariants should live close to the domain model
* not every entity needs to be rich, but important aggregates must not become passive containers only

---

## 7. Frontend Must Be Feature-Oriented and Operationally Efficient

The frontend must be structured by features and optimized for operational workflows.

### Decision

Initial frontend structure:

```text
frontend/
  src/app/
    core/
    shell/
    shared/
    features/
```

Each feature should preserve separation between:

* pages
* components
* facades
* data-access
* models

### Rationale

This supports:

* maintainability
* feature growth
* cleaner ownership
* better UX consistency
* lower complexity in large screens

### Consequences

* avoid giant page components
* avoid direct HTTP calls spread across UI components
* avoid mixing orchestration and presentation in the same place
* operational UX must remain fast for front desk and clinicians

---

## 8. Security Is a Core Architectural Concern

Security must be treated as foundational, not as a later hardening phase.

### Decision

Bigsmile will adopt a security-first posture.

### Required rules

* secrets must not be committed to the repository
* critical authorization must be enforced in backend
* audit-sensitive actions must be traceable
* cross-tenant access must be blocked by default
* tenant context must be explicit
* role and permission enforcement must be testable

### Consequences

* insecure defaults are not acceptable
* convenience-based privilege bypass is forbidden
* sensitive operations require careful validation
* architecture must support secure growth

---

## 9. Tenant Isolation Must Be Enforced in Multiple Layers

Tenant isolation must not depend on one mechanism only.

### Decision

Tenant safety must be enforced through:

* domain modeling
* application orchestration
* infrastructure filtering
* API access rules
* automated testing

### Rationale

A single weak enforcement point is not enough for a SaaS product.

### Consequences

* use request-scoped tenant context
* use EF Core global tenant filters or equivalent central enforcement
* require explicit platform bypass
* include tenant isolation tests as part of quality strategy

---

## 10. Branch Is an Operational Scope, Not a Security Shortcut

Branch support is part of the product foundation, but it does not replace tenant boundaries.

### Decision

Branch-aware entities may include `BranchId` where the business meaning requires it.

### Examples

Likely branch-aware records:

* appointments
* cash sessions
* branch assignments
* branch-level reports

Likely tenant-only records:

* patient ownership
* treatment ownership
* clinical record ownership

### Consequences

* `BranchId` cannot be used alone to enforce safety
* branch relationships must always remain tenant-consistent
* branch is subordinate to tenant in all security-sensitive paths

---

## 11. Product Growth Must Follow Business Domains

Bigsmile must grow through coherent bounded contexts.

### Decision

Initial bounded contexts include:

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

### Rationale

This keeps the product coherent and prevents architecture from being driven by screen-by-screen implementation only.

### Consequences

* code ownership should follow domain boundaries
* features should not be implemented in unrelated modules
* integration between modules should be explicit and intentional

---

## 12. Operational UX Is a Product Feature

User experience is not decoration. It is part of product quality.

### Decision

Bigsmile must prioritize speed and clarity for operational users.

### Core users

* front desk
* dentists
* assistants
* tenant administrators

### Consequences

The architecture and frontend implementation must support:

* fast patient search
* fast appointment creation
* low-friction clinical workflows
* understandable treatment planning
* quick payment registration
* clear dashboard visibility

### Important principle

A workflow that is technically complete but operationally slow is not acceptable.

---

## 13. Testing and Quality Controls Are Mandatory

Testing is part of the architecture.

### Decision

Bigsmile must include:

* unit tests
* integration tests
* architecture tests
* frontend unit tests
* end-to-end tests
* CI validation

### Rationale

As a SaaS product, Bigsmile cannot rely on manual confidence only.

### Consequences

* high-risk changes require tests
* tenant isolation and authorization need automated coverage
* architecture drift must be controlled over time
* CI is part of the product foundation, not an optional enhancement

---

## 14. Architecture Rules Must Be Enforced, Not Merely Documented

Documentation alone is not enough to protect the system.

### Decision

Important architectural rules should be codified through:

* architecture tests
* repository structure
* contribution rules
* explicit ADRs
* review discipline

### Consequences

* the team should not depend only on memory
* repeated mistakes should be prevented structurally
* major architectural changes must be documented in ADRs

---

## 15. Roadmap Discipline Matters

Features must be built in the right order.

### Decision

The product roadmap should prioritize:

1. foundation
2. patients
3. scheduling
4. clinical records
5. odontogram
6. treatments and quotes
7. billing
8. documents and dashboard

### Rationale

This order protects the operational core and keeps scope realistic.

### Consequences

* avoid premium features too early
* avoid platform over-engineering before the core is useful
* avoid scope growth that damages delivery quality

---

## 16. Non-Goals for the Initial Stage

The following are intentionally not core initial architectural goals:

* microservices
* separate database per tenant from day one
* advanced insurance claim flows
* ERP-level expansion
* premature AI-heavy functionality
* unnecessary distributed complexity

### Rationale

The initial goal is to build the right foundation, not the most complex architecture.

---

## 17. What Must Be Avoided

The following patterns are explicitly discouraged:

* manual tenant filtering as the primary safety strategy
* giant service classes
* giant frontend page components
* secrets in versioned configuration
* backend business logic hidden inside controllers
* domain rules duplicated across layers
* platform and tenant concerns mixed without explicit scope
* premature complexity without business justification

---

## 18. Consequences of This ADR

By accepting this ADR, Bigsmile commits to:

* treating tenant isolation as a first-class concern
* building a modular monolith before considering service distribution
* structuring backend and frontend with explicit ownership boundaries
* protecting operational UX as part of product quality
* enforcing architecture through tests, docs, and contribution rules
* growing the product incrementally without sacrificing maintainability

This ADR becomes the baseline reference for all future architectural decisions.

---

## 19. Future ADRs Expected

This ADR is foundational and will likely be followed by additional ADRs such as:

* ADR 002 — Tenant Resolution Strategy
* ADR 003 — Authentication and Session Strategy
* ADR 004 — Authorization Model
* ADR 005 — Database Multi-Tenancy Strategy
* ADR 006 — Frontend State and Feature Coordination Strategy
* ADR 007 — Auditing and Observability Strategy

---

## 20. Final Principle

Every future architectural decision in Bigsmile should be evaluated through this question:

**Does this preserve Bigsmile as a secure, maintainable, multi-tenant product with a strong operational core?**

```