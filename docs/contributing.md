# Contributing to Bigsmile

## Purpose

This document defines the contribution rules for **Bigsmile**.

Its purpose is to ensure that every change made to the project protects the product in terms of:

- architecture
- maintainability
- security
- multi-tenant safety
- code clarity
- product consistency
- long-term evolution

Bigsmile is not being built as a throwaway internal app.  
It is being built as a **commercial SaaS product** for dental clinics, so contributions must be evaluated with product-quality standards.

---

## 1. Core Contribution Principle

Every contribution must improve the product **without weakening the foundation**.

This means:
- no shortcuts that damage maintainability
- no features that break tenant isolation
- no rushed code that creates architectural confusion
- no UI additions that degrade operational UX
- no security compromises for convenience

If a change solves one problem but introduces structural risk, it is not a good contribution.

---

## 2. What Contributors Must Protect

Every contributor must protect the following foundations:

- multi-tenant architecture
- modular boundaries
- security rules
- clean separation of responsibilities
- consistent user experience
- testability
- readability
- product coherence

These are not optional concerns.

---

## 3. Bigsmile Architectural Rules

### 3.1 Respect module boundaries
Bigsmile is organized by business domains, not by random technical convenience.

Contributors must respect bounded contexts such as:
- Platform
- Identity
- Patients
- Scheduling
- Clinical
- Odontogram
- Treatments
- Billing
- Documents
- Notifications
- Reporting

Do not place code in unrelated modules just because it is faster.

### 3.2 Do not bypass the Application layer
Business use cases must be implemented through the intended application flow.

Do not:
- place business rules directly in controllers
- place domain logic directly in repositories
- move cross-cutting decisions into UI-only logic

### 3.3 Protect the Domain
Critical business rules should remain in the domain or in clearly defined use cases.

Do not reduce the model into passive data containers unless that is intentionally justified.

### 3.4 Avoid “god services”
Do not create oversized services that centralize too many responsibilities.

Prefer:
- focused handlers
- use-case oriented organization
- clear service boundaries
- explicit dependencies

### 3.5 Respect frontend feature architecture
Frontend changes must preserve the separation between:
- pages
- components
- facades
- data-access
- shared UI
- core infrastructure

Do not create giant page components with mixed responsibilities.

---

## 4. Multi-Tenant Safety Rules

Multi-tenancy is one of the most critical architectural concerns in Bigsmile.

### Mandatory rules
- every tenant-owned record must remain tenant-scoped
- tenant isolation must not depend on manual discipline alone
- `BranchId` never replaces `TenantId` as the primary boundary
- platform bypass must always be explicit and auditable
- cross-tenant reads and writes are forbidden by default

### Never do the following
- add code that bypasses tenant boundaries silently
- trust client input blindly for tenant scope
- implement tenant filtering manually in scattered ways if a centralized mechanism exists
- mix platform and tenant operations without explicit modeling

### Required mindset
Every change involving data access should be evaluated with this question:

**Could this accidentally expose or modify another tenant’s data?**

If the answer is not clearly “no,” the change is not ready.

---

## 5. Security Rules

Bigsmile must be developed with a security-first mindset.

### Required rules
- never commit secrets to the repository
- never add insecure defaults that could reach production
- never rely only on frontend validation for critical rules
- always validate sensitive operations in backend
- keep authorization explicit and testable
- audit critical actions

### Not allowed
- secrets in versioned config files
- insecure session/token handling
- hidden privilege escalation paths
- business-critical authorization enforced only in UI
- silent platform-level bypasses

### Sensitive areas
Extra care is required when changing:
- authentication
- authorization
- user roles
- tenant context
- branch access
- patient records
- clinical notes
- payments
- documents

---

## 6. Code Quality Expectations

### 6.1 Write code for long-term maintenance
Bigsmile should remain understandable years from now.

Prefer:
- explicit naming
- small focused classes
- small focused components
- simple flows
- predictable structure
- low surprise

Avoid:
- oversized methods
- deeply nested logic
- hidden side effects
- inconsistent naming
- duplicated business rules

### 6.2 Clarity over cleverness
Readable code is preferred over clever code.

If something is difficult to understand, it is a maintenance risk.

### 6.3 Keep files focused
Contributors should avoid very large files when the responsibility can be separated clearly.

### 6.4 Prefer explicitness
Make business intent visible.

A future contributor should be able to understand:
- what this code does
- why it exists
- which module owns it
- which rules it protects

---

## 7. Backend Contribution Guidelines

### Expected structure
Backend changes should align with:
- Api
- Application
- Domain
- Infrastructure
- SharedKernel

### Recommended approach
- implement use cases as focused application slices
- place domain rules in the correct layer
- keep infrastructure concerns out of business logic
- use explicit validation
- preserve transaction boundaries
- protect tenant context

### Backend anti-patterns to avoid
- controllers with business logic
- repository methods that become mini-application services
- direct database assumptions leaking into domain
- arbitrary shared utility classes for business rules
- fragile cross-module coupling

---

## 8. Frontend Contribution Guidelines

### Expected structure
Frontend changes should align with:
- core
- shell
- shared
- features

Each feature should preserve separation between:
- pages
- components
- facades
- data-access
- models

### Frontend goals
- fast operational UX
- low-friction navigation
- consistent interaction patterns
- maintainable feature growth

### Frontend anti-patterns to avoid
- giant components
- direct HTTP access scattered across components
- business logic mixed into presentation
- repeated form logic without abstraction
- inconsistent state handling patterns

### UX expectation
Operational speed is a product feature.

Contributors should actively avoid adding friction to:
- patient registration
- appointment creation
- clinical note entry
- odontogram interaction
- payment registration

---

## 9. Testing Expectations

Testing is not optional.

### Every meaningful change should include appropriate validation
Depending on the type of change, contributors should add or update:
- unit tests
- integration tests
- architecture tests
- frontend unit tests
- end-to-end tests

### Minimum expectation
A contribution should provide confidence that:
- the change works
- tenant isolation remains safe
- authorization still behaves correctly
- existing behavior was not silently broken

### High-risk changes
Changes involving these areas should almost always include tests:
- tenant handling
- authorization
- patient data
- clinical flows
- payments
- documents
- branch-specific behavior
- platform overrides

---

## 10. Pull Request Rules

Every pull request should be intentional, understandable, and reviewable.

### A pull request should include
- clear purpose
- summary of the change
- affected modules
- risks introduced
- test evidence
- architectural notes if relevant

### Pull requests should avoid
- mixing unrelated concerns
- combining large refactors with new business features
- hiding risky infrastructure changes inside feature work
- shipping unreviewable large diffs without structure

### Preferred pull request style
Small to medium, coherent, and reviewable changes are preferred over large mixed changes.

---

## 11. Documentation Expectations

Documentation is part of the product foundation.

A contributor should update documentation when a change affects:
- architecture
- tenant model
- roadmap
- conventions
- setup
- developer workflows
- permissions or access model

### Important rule
If a change alters how the system should be understood, relevant docs must be updated.

---

## 12. How to Evaluate a Proposed Change

Before implementing a change, contributors should ask:

### Product questions
- Does this solve a real user or platform need?
- Does this fit the current roadmap stage?
- Is this introducing unnecessary scope too early?

### Architecture questions
- Which module owns this?
- Does this preserve clear boundaries?
- Does this create long-term debt?

### Multi-tenant questions
- Is tenant ownership explicit?
- Could this leak data across tenants?
- Does this need branch awareness?
- Does it require platform override logic?

### Security questions
- Is authorization correct?
- Is validation happening in the right layer?
- Could this introduce privilege or data exposure risk?

### UX questions
- Does this make the workflow faster or slower?
- Does it simplify or complicate the interface?
- Would front desk or clinicians find this clear?

If these questions cannot be answered well, the change is not ready.

---

## 13. Rules for AI-Assisted Contributions

Bigsmile may be developed with the assistance of AI tools such as CODEX/OpenClaw.

AI-generated changes must follow the same quality bar as human-generated changes.

### Required rules for AI-assisted work
- do not accept generated code blindly
- verify module ownership
- verify tenant safety
- verify security implications
- verify architectural consistency
- verify naming and clarity
- verify tests and docs

### AI-specific risks to watch
- oversized files
- duplicated patterns
- hidden coupling
- generic abstractions with weak ownership
- missing tenant enforcement
- missing authorization checks
- UI code that mixes too many concerns

### Important principle
AI can accelerate implementation, but it must not weaken the architecture.

---

## 14. Commit and Change Discipline

Contributors should aim for:
- focused commits
- coherent change sets
- meaningful commit messages
- clear change intent

### Avoid
- dumping many unrelated edits together
- leaving dead code behind
- partially implemented abandoned flows
- untracked architecture drift

---

## 15. When to Create an Architectural Decision Record (ADR)

A contributor should create or update an ADR when a change affects:
- tenant model
- authentication/session model
- authorization strategy
- module boundaries
- storage strategy
- integration pattern
- major frontend state strategy
- major cross-cutting conventions

### Examples
Create an ADR if the team decides:
- to change tenant resolution rules
- to move from cookies to another session strategy
- to introduce an outbox pattern
- to modify branch access semantics
- to redefine module ownership

---

## 16. Non-Negotiable Rules

The following rules are non-negotiable:

- do not break tenant isolation
- do not commit secrets
- do not bypass authorization casually
- do not introduce giant mixed-responsibility files
- do not weaken architecture for speed
- do not add product scope without roadmap awareness
- do not merge risky changes without appropriate validation

---

## 17. Definition of a Good Contribution

A good contribution to Bigsmile is one that is:

- correct
- secure
- clear
- scoped
- testable
- maintainable
- aligned with the roadmap
- respectful of tenant boundaries
- respectful of product UX

A change is not good only because it works.

It is good when it helps Bigsmile remain a strong product over time.

---

## 18. Final Principle

Every contribution should be evaluated through this question:

**Does this make Bigsmile stronger as a secure, maintainable, multi-tenant product?**