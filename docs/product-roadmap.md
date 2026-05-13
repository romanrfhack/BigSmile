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

## Current status
- Release 3 is complete as the foundational clinical release
- Release 3.1 — Clinical Record Foundation is accepted as closure evidence
- Release 3.2 — Basic Diagnoses Foundation is accepted as closure evidence
- Release 3.3 — Clinical Timeline Read Model is accepted as closure evidence
- Release 3.4 — Clinical Snapshot Change History is accepted as closure evidence
- Release 3.5 — Medical Questionnaire Backend is accepted as closure evidence
- Release 3.6 — Clinical Encounter / Vitals Backend is accepted as closure evidence
- A bounded frontend UI integration for the fixed Release 3.5 medical questionnaire now exists inside the existing clinical record screen, without changing backend contracts or permissions
- A bounded frontend UI integration for Release 3.6 clinical encounters/vitals now exists inside the existing clinical record screen, without changing backend contracts, permissions, timeline behavior, Patient data, or later modules
- Patient age is derived in the Clinical Records UI from `Patient.DateOfBirth`; age is not persisted and no `Age` field is added to backend/API contracts

## Scope
Release 3 was delivered in bounded slices. The accepted closure slices are Release 3.1, Release 3.2, Release 3.3, Release 3.4, Release 3.5, and Release 3.6, and they cover:

- explicit clinical record creation
- medical background summary
- current medications summary
- current allergies
- append-only clinical notes
- note history returned newest-first in reads and UI
- clinician attribution
- audit-sensitive updates
- basic non-coded diagnoses on existing clinical records
- explicit diagnosis creation
- explicit diagnosis resolution
- diagnoses included in the clinical record read model
- diagnosis ordering with active diagnoses first and newest-first within each status group
- clinical timeline read model inside the existing clinical record read model
- timeline events limited to `ClinicalNoteCreated`, `ClinicalDiagnosisCreated`, and `ClinicalDiagnosisResolved`
- timeline ordering newest-first
- no new timeline endpoint and no new timeline table
- bounded snapshot history inside the existing clinical record read model
- initial snapshot history entry on explicit clinical record creation
- snapshot history entries only for effective changes to medical background summary, current medications summary, and current allergies
- snapshot history ordering newest-first
- snapshot history kept separate from the accepted Release 3.3 timeline
- backend structured medical questionnaire on an existing clinical record
- fixed `QuestionKey` catalog based on the physical form mapping
- `ClinicalMedicalAnswer` records owned by tenant, patient, and clinical record
- `Unknown` / `Yes` / `No` answers with optional bounded details
- `GET` and `PUT /api/patients/{patientId}/clinical-record/questionnaire`
- upsert by `QuestionKey` without accepting `TenantId` from the request
- frontend form inside the existing clinical record screen for the fixed questionnaire catalog, grouped by clinical section with i18n labels, read-only Patient demographics context, and loading/empty/error/saving states
- no questionnaire-driven changes to allergies, timeline, snapshot history, or other modules
- backend clinical encounter/vitals capture on an existing clinical record
- `ClinicalEncounter` records owned by tenant, patient, and clinical record
- `GET` and `POST /api/patients/{patientId}/clinical-record/encounters`
- bounded consultation type catalog `Treatment` / `Urgency` / `Other`
- optional bounded vitals: temperature C, systolic/diastolic blood pressure, weight kg, height cm, respiratory rate, and heart rate
- optional append-only linked `ClinicalNote` when encounter `noteText` is provided
- no request-provided `TenantId` or `CreatedByUserId` for encounters
- frontend section inside the existing clinical record screen for recent encounters and compact encounter/vitals creation, with i18n labels, read-only Patient context, and loading/empty/error/saving states
- no frontend-driven changes to timeline, Patient demographics, Scheduling, Billing, Odontogram, Treatments, Documents, or doctor/provider scope

## Expected outcome
A user operating under the current clinical permissions can consult and update the patient’s foundational clinical record in a structured way, including basic diagnosis tracking, a bounded clinical timeline read model, bounded snapshot history, a fixed medical questionnaire, and encounter/vitals capture without opening later clinical modules.

## Core users
- Dentist
- Assistant
- Tenant Admin

## Current access note
- `clinical.read` and `clinical.write` are granted to `PlatformAdmin` and `TenantAdmin`
- `TenantUser` does not receive clinical permissions in the accepted Release 3 closure

## Key UX goals
- clear navigation from patient profile to clinical record
- readable history
- fast note entry
- clear active/resolved diagnosis visibility
- secure access to sensitive information

## Out of scope
- full or advanced patient clinical timeline
- any cross-module timeline
- restore or revert of clinical snapshot history
- full clinical record versioning
- rich snapshot diff
- coded diagnosis catalogs such as ICD/CIE/SNOMED
- advanced diagnosis workflows beyond basic add/resolve
- tenant-configurable form builder
- automatic allergy synchronization from questionnaire answers
- questionnaire events in the clinical timeline
- configurable or dynamic questionnaire UI
- encounter editing or deletion
- new encounter-specific timeline event model
- odontogram
- treatments
- documents
- advanced specialty templates
- AI note generation
- electronic consent flows
- advanced forms intake

---

## 8. Release 4 — Odontogram

## Goal
Introduce the dental visual layer of the product.

## Current status
- Release 4 is the next planned functional phase after Release 3 closure
- Release 4 is not opened by the Release 3 closure documentation
- Release 4 should start with a dedicated bounded slice, beginning with Release 4.1 — Odontogram Foundation

## Scope
Release 4 should be delivered in bounded slices. The first planned slice should cover:

- explicit odontogram creation
- exactly one odontogram per patient per tenant
- explicit `GET` returning `404` when missing
- no autocreation
- permanent adult FDI/ISO two-digit tooth numbering only (`11-18`, `21-28`, `31-38`, `41-48`)
- tooth-level current state
- minimal status catalog `Unknown` / `Healthy` / `Missing` / `Restored` / `Caries`
- explicit single-tooth status updates
- minimal audit metadata on the odontogram and each tooth state
- minimal patient-context UI for empty state, creation, visualization, and single-tooth state updates

Later Release 4 slices may add surfaces, basic findings and bounded findings history only after the foundation is explicitly accepted.

## Expected outcome
A user operating under future odontogram permissions can initialize, consult, and update a bounded odontogram in the patient context without opening later dental modules.

## Core users
- Dentist
- Assistant
- Tenant Admin

## Current access note
- Odontogram permissions must be introduced only with an explicit Release 4 slice.
- Do not grant odontogram access incidentally from Release 3 closure.

## Key UX goals
- clear patient-context navigation
- low-friction explicit creation
- fast single-tooth updates
- fast single-surface updates
- immediately understandable tooth and surface status representation

## Out of scope
- complex dental findings registration
- treatment linkage
- diagnosis linkage
- documents linkage
- wider dental timeline/history
- surface history
- odontogram restore/versioning
- bulk editing
- child or mixed dentition
- advanced charting/canvas interaction
- advanced orthodontic charting
- advanced perio charting
- imaging overlays
- AI-assisted findings detection

---

## 9. Release 5 — Treatments and Quotes

## Goal
Connect diagnosis with operational and commercial treatment planning.

## Current status
- Release 5 is planned after Release 4
- Release 5 is not opened by the Release 3 closure documentation

## Scope
Release 5 should be delivered in bounded slices. The initial planned scope includes:
- explicit treatment plan creation for an existing patient
- exactly one active treatment plan per patient per tenant
- `GET` returning `404` when the treatment plan does not exist
- no autocreation by read or by item operations
- treatment plan items with required title, optional category, simple quantity, short note, and optional adult FDI `toothCode` plus optional `surfaceCode`
- `surfaceCode` limited to `O` / `M` / `D` / `B` / `L` and requiring `toothCode`
- explicit add/remove item flows
- bounded plan status with `Draft` / `Proposed` / `Accepted`
- minimal patient-context UI for empty state, explicit creation, item add/remove, and status changes
- explicit treatment quote creation from the existing treatment plan
- exactly one quote per treatment plan in this slice
- `GET` returning `404` when the quote does not exist
- no autocreation of the quote
- quote items as a snapshot-only copy of the current treatment plan items
- fixed `CurrencyCode = MXN` in this slice
- line-level `UnitPrice`, `LineTotal`, and `QuoteTotal`
- bounded quote status with `Draft` / `Proposed` / `Accepted`
- positive line pricing required to move `Draft -> Proposed`, preserved while the quote stays `Proposed`, and revalidated on `Proposed -> Accepted`
- minimal patient-context UI for treatment planning and later quote work when explicitly opened
- no discounts or taxes
- no billing linkage
- no scheduling linkage
- no treatment execution tracking
- no quote regenerate/versioning or multi-quote negotiation
- no plan versioning or archive

## Expected outcome
Clinicians and tenant administrators can move from clinical or odontogram context to a structured, minimal treatment plan and then into a bounded commercial quote workflow without yet opening billing or advanced pricing.

## Core users
- Dentist
- Front Desk
- Tenant Admin

## Current access note
- Treatment plan and quote permissions must be introduced only with explicit Release 5 slices.

## Key UX goals
- easy transition from diagnosis to treatment
- clear plan status
- low-friction item capture
- understandable plan status
- simple treatment lifecycle

## Out of scope
- advanced pricing beyond a bounded quote slice
- discounts and taxes
- billing handoff
- scheduling handoff
- treatment execution/progress tracking
- quote regenerate/versioning
- multiple quotes per patient/negotiation
- plan archive/versioning
- complex insurance processing
- installment financing logic
- advanced approval workflows
- automated treatment follow-up campaigns

---

## 10. Release 6 — Billing

## Goal
Enable clinics to record charges, payments, and balances as part of the operational workflow.

## Current status
- Release 6 is planned after Release 5
- Release 6 is not opened by the Release 3 closure documentation
- Payments, balances, receipts, taxes, discounts, cancellations, CFDI/PAC, multi-billing, and advanced billing workflows remain deferred

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

## Current status
- Release 7 is planned after Release 6
- Release 7 is not opened by the Release 3 closure documentation
- OCR, rich preview, versioning, external sharing, templates, generated PDFs, advanced analytics, charts, complex filters, branch dashboard, doctor dashboard, advanced reporting, exports, and advanced document workflows remain deferred

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
- tenant-scoped operational summary foundation
- active patients
- appointments of the day
- pending appointments of the day
- active patient documents
- active treatment plans
- accepted quotes when Release 5 has introduced quotes
- issued billing documents when Release 6 has introduced billing
- advanced analytics, charts, complex filters, daily income summaries, balances, branch dashboards, doctor dashboards, exports, and advanced reporting remain deferred beyond the initial dashboard foundation

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

## Current status
- Phase 2 is a later roadmap phase after the MVP is stable
- Phase 2 is not the next phase after Release 3 closure
- WhatsApp, email, SMS sending, automatic reminders, provider integrations, jobs, queues, webhooks, online booking, patient portal, external delivery templates, campaigns, retry automation, real reminder scheduler, delivery status, and advanced dashboard behavior remain deferred

## Candidate features
- WhatsApp reminders
- email reminders
- expanded appointment confirmation workflows beyond Phase 2.1
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
