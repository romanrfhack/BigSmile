# Bigsmile Product Roadmap

## Purpose

This document defines the initial product roadmap for **Bigsmile**.

Its purpose is to establish a clear and controlled sequence for building the platform, so that Bigsmile evolves as a coherent product instead of a collection of disconnected features.

This roadmap is intentionally designed to:
- validate business value early
- keep scope under control
- protect architectural quality
- support future SaaS growth
- avoid premature complexity

---

## 1. Product Direction

Bigsmile is being built as a SaaS platform for dental clinics and private practices.

The product starts with a strong operational core for a single clinic, but it is designed from the beginning to support:
- multiple tenants
- multiple branches
- independent administration
- future feature expansion
- commercial licensing

### Product vision

Bigsmile should become a platform that helps clinics operate their daily workflow through one continuous flow:

**appointment → patient record → clinical record → odontogram → treatment plan → quote → payment → follow-up**

The roadmap is organized around strengthening this operational backbone first.

---

## 2. Roadmap Principles

The roadmap follows these principles:

### 2.1 Build the operational core first
The first releases must solve the everyday workflow of a clinic before adding automation-heavy or premium features.

### 2.2 Protect the foundation
No feature should be added in a way that compromises:
- tenant isolation
- maintainability
- modularity
- security
- UX clarity

### 2.3 Grow by business capability
Features should be grouped and delivered by domain value, not by isolated technical tasks.

### 2.4 Keep the MVP small but real
The first public version should already be useful in real clinic operations.

### 2.5 Delay non-essential complexity
Advanced features should only be added after the core workflow is stable.

---

## 3. Product Phases Overview

The initial roadmap is divided into the following phases:

- **Release 0 — Foundation**
- **Release 1 — Patients**
- **Release 2 — Scheduling**
- **Release 3 — Clinical Records**
- **Release 4 — Odontogram**
- **Release 5 — Treatments and Quotes**
- **Release 6 — Billing**
- **Release 7 — Documents and Dashboard**
- **Phase 2 Expansion — Modern Operations**
- **Phase 3 Expansion — SaaS Growth**
- **Phase 4 Expansion — Advanced Product Capabilities**

---

## 4. Release 0 — Foundation

## Goal
Build the technical and architectural base of the product before developing business modules.

## Scope
- solution structure
- backend projects
- frontend projects
- modular architecture baseline
- multi-tenancy foundation
- authentication
- authorization
- roles and permissions baseline
- tenant context
- branch context
- audit foundation
- logging foundation
- error handling foundation
- architecture tests
- CI/CD foundation
- initial database bootstrap
- coding conventions and repository structure

## Expected outcome
At the end of Release 0, Bigsmile should have:
- a compilable backend
- a compilable frontend
- base authentication and authorization
- a safe tenant model
- architectural guardrails
- a clear structure for future modules

## Out of scope
- full patient management
- complete appointment workflows
- business dashboards
- billing flows
- clinical modules

## Why this release matters
Without a strong foundation, every future module would introduce architectural risk and technical debt.

---

## 5. Release 1 — Patients

## Goal
Enable clinics to register, maintain, and search patients efficiently.

## Scope
- patient registration
- patient update
- patient search
- patient profile
- responsible party data
- basic clinical alerts
- patient status (active/inactive)
- basic validations
- tenant-scoped ownership
- branch-neutral patient model

## Expected outcome
Clinics can create and maintain patient records safely within tenant scope.

## Core users
- Front Desk
- Dentist
- Tenant Admin

## Key UX goals
- fast patient search
- fast registration flow
- low-friction editing
- clear patient identification

## Out of scope
- full clinical record
- odontogram
- full treatment workflow
- advanced segmentation
- patient portal

---

## 6. Release 2 — Scheduling

## Goal
Provide the clinic with a practical operational calendar for managing appointments.

## Scope
- daily calendar view
- weekly calendar view
- appointment creation
- appointment editing
- rescheduling
- cancellation
- no-show status
- attended status
- blocked time slots
- branch-aware scheduling
- appointment notes

## Expected outcome
Front desk users can manage the daily operation of the clinic through the system.

## Core users
- Front Desk
- Dentist
- Tenant Admin

## Key UX goals
- create appointment in very few steps
- reschedule quickly
- visually understand the day
- avoid calendar friction

## Out of scope
- doctor-based views, deferred to a future bounded Scheduling slice that introduces provider/doctor assignment
- online booking
- reminder automation
- advanced waiting room workflows
- AI-assisted scheduling

---

## 7. Release 3 — Clinical Records

## Goal
Allow clinicians to maintain a structured patient clinical record.

## Scope
- clinical record creation
- medical background
- allergies
- clinical notes
- note history
- basic diagnoses structure
- patient clinical timeline
- clinician attribution
- audit-sensitive updates

## Expected outcome
A dentist can consult and update the patient’s clinical record in a structured way.

## Core users
- Dentist
- Assistant
- Tenant Admin

## Key UX goals
- clear navigation from patient profile to clinical record
- readable history
- fast note entry
- secure access to sensitive information

## Out of scope
- advanced specialty templates
- AI note generation
- electronic consent flows
- advanced forms intake

---

## 8. Release 4 — Odontogram

## Goal
Introduce the dental visual layer of the product.

## Scope
- interactive odontogram
- tooth selection
- surface selection
- dental findings registration
- tooth state tracking
- surface state tracking
- visual update of dental status
- linkage with treatment planning foundation

## Expected outcome
Clinicians can capture and review dental findings visually in the patient context.

## Core users
- Dentist
- Assistant

## Key UX goals
- intuitive visual interaction
- low-click registration
- immediate visual feedback
- clinically understandable state representation

## Out of scope
- advanced orthodontic charting
- advanced perio charting
- imaging overlays
- AI-assisted findings detection

---

## 9. Release 5 — Treatments and Quotes

## Goal
Connect diagnosis with operational and commercial treatment planning.

## Scope
- treatment catalog
- treatment plan creation
- treatment plan items
- quote generation
- estimated totals
- plan status
- item status
- treatment acceptance tracking
- basic treatment progress states

## Expected outcome
Clinicians and front desk staff can move from findings to a structured treatment proposal and quote.

## Core users
- Dentist
- Front Desk
- Tenant Admin

## Key UX goals
- easy transition from diagnosis to treatment
- clear quote presentation
- understandable plan status
- simple treatment lifecycle

## Out of scope
- complex insurance processing
- installment financing logic
- advanced approval workflows
- automated treatment follow-up campaigns

---

## 10. Release 6 — Billing

## Goal
Enable clinics to record charges, payments, and balances as part of the operational workflow.

## Scope
- charges linked to treatment plans or items
- payment registration
- partial payments
- total payments
- balance tracking
- payment methods
- receipts
- basic cash management
- daily payment visibility
- branch-aware payment operations when applicable

## Expected outcome
The clinic can register and track money related to patient care in one operational flow.

## Core users
- Front Desk
- Tenant Admin

## Key UX goals
- register a payment quickly
- see outstanding balance clearly
- reduce operational confusion
- support daily front desk operations

## Out of scope
- full accounting
- full tax invoicing
- insurance claims
- ERP-level financial modules

---

## 11. Release 7 — Documents and Dashboard

## Goal
Complete the initial operational MVP with file support and high-level visibility.

## Scope
### Documents
- patient attachments
- radiographies
- file upload
- file association to patient
- basic document categorization
- simple document access controls

### Dashboard
- appointments of the day
- attended appointments
- no-shows
- daily income summary
- pending balances
- treatments pending acceptance

## Expected outcome
The MVP becomes operationally complete for day-to-day usage.

## Core users
- Dentist
- Front Desk
- Tenant Admin

## Key UX goals
- quick operational visibility
- easy access to relevant patient files
- low-friction dashboard scanning

## Out of scope
- advanced analytics
- BI-level dashboards
- full document workflows
- advanced storage lifecycle policies

---

## 12. MVP Definition

The initial MVP of Bigsmile is complete after:
- Patients
- Scheduling
- Clinical Records
- Odontogram
- Treatments and Quotes
- Billing
- Documents
- Roles and Permissions
- Basic Dashboard

This MVP should be strong enough to validate:
- real operational usability
- clinic adoption
- workflow fit
- commercial value

---

## 13. Phase 2 Expansion — Modern Operations

Once the MVP is stable, the next phase focuses on modernizing clinic workflows.

## Candidate features
- WhatsApp reminders
- email reminders
- appointment confirmation flows
- online booking
- intake forms
- digital patient updates before appointments
- branch-level refinements
- improved dashboard metrics
- better treatment follow-up visibility

## Goal
Reduce operational friction, missed appointments, and manual coordination.

## Why this phase comes after MVP
These features are highly valuable, but they depend on the operational core being stable first.

---

## 14. Phase 3 Expansion — SaaS Growth

This phase focuses on turning Bigsmile from a strong clinic system into a stronger SaaS platform.

## Candidate features
- advanced multi-branch administration
- tenant settings expansion
- branding by tenant
- feature flags by plan
- onboarding automation
- plan and subscription management
- tenant support tools
- platform administration improvements
- tenant-level analytics

## Goal
Strengthen the commercial and operational SaaS layer.

## Why this phase matters
A product can work for one clinic and still fail as a SaaS if platform-level capabilities are weak.

---

## 15. Phase 4 Expansion — Advanced Product Capabilities

This phase focuses on premium and advanced capabilities.

## Candidate features
- patient portal
- advanced analytics
- conversion metrics
- treatment recall workflows
- automation campaigns
- inventory basics
- electronic invoicing
- support workflows for platform operations
- future AI-assisted features

## Goal
Increase product value, differentiation, and retention.

## Important note
These features should be added only after the product foundation and core workflows are operationally strong.

---

## 16. Release Dependencies

The roadmap is intentionally sequential because some releases depend on earlier capabilities.

### Core dependency chain

- Foundation is required by all releases
- Patients is required before meaningful Clinical workflows
- Scheduling depends on tenant, user, and branch foundations
- Clinical depends on Patients
- Odontogram depends on Clinical and Patients
- Treatments depend on Clinical and Odontogram context
- Billing depends on Treatments or at least patient financial records
- Documents depend on Patients and security rules
- Dashboard depends on data produced by all prior releases

This dependency chain should guide implementation order.

---

## 17. Non-Goals for the Initial Product Stages

The following are intentionally not part of the early roadmap:
- microservices
- overly complex enterprise integrations
- advanced insurance claims
- full accounting
- full ERP processes
- advanced AI workflows
- excessive reporting depth
- multi-region deployment complexity
- separate database per tenant from day one

These may be considered later if the business requires them.

---

## 18. Validation Priorities

The roadmap should validate these questions as early as possible:

### Product validation
- Do clinics actually use the scheduling flow daily?
- Is patient registration fast enough?
- Does the clinical flow feel natural to dentists?
- Is the odontogram easy enough to use?
- Does treatment planning support real conversations with patients?
- Is billing simple enough for front desk staff?

### SaaS validation
- Is tenant isolation working correctly?
- Is branch segmentation sufficient for real operations?
- Is the permission model practical?
- Is the product structure sustainable for future modules?

---

## 19. Risks to Control

The roadmap must actively control these risks:
- growing scope too early
- introducing premium features before the core is stable
- adding modules without preserving architecture quality
- mixing platform features with tenant operational features
- weakening tenant isolation for convenience
- building UX-heavy features without validating operational workflows first

---

## 20. Success Criteria by Stage

### Foundation success
- the architecture is stable
- authentication and tenant model are working
- CI and tests are in place

### MVP success
- a clinic can operate core workflows inside Bigsmile
- the core product feels usable and coherent
- the system is ready for real pilot validation

### SaaS success
- multiple tenants can operate safely
- branch support is practical
- onboarding and platform administration are sustainable

### Product growth success
- premium features add value without destabilizing the core
- the product remains maintainable as it expands

---

## 21. Guiding Principle

The roadmap exists to ensure that Bigsmile grows in the right order.

Every new feature should be evaluated against this question:

**Does this strengthen the operational core or the SaaS foundation at the right time?**
