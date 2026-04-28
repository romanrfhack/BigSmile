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
- Release 3 is open and in progress
- Release 3.1 — Clinical Record Foundation is accepted
- Release 3.2 — Basic Diagnoses Foundation is accepted
- Release 3.3 — Clinical Timeline Read Model is accepted
- Release 3.4 — Clinical Snapshot Change History is accepted

## Scope
Release 3 is being delivered in bounded slices. The currently accepted slices are Release 3.1, Release 3.2, Release 3.3, and Release 3.4, and they cover:

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

## Expected outcome
A user operating under the current clinical permissions can consult and update the patient’s foundational clinical record in a structured way, including basic diagnosis tracking, a bounded clinical timeline read model, and a bounded snapshot history without opening later clinical modules.

## Core users
- Dentist
- Assistant
- Tenant Admin

## Current access note
- `clinical.read` and `clinical.write` are granted to `PlatformAdmin` and `TenantAdmin`
- `TenantUser` does not receive clinical permissions in the currently accepted Release 3 slices

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
- Release 4 is open and in progress
- Release 4.1 — Odontogram Foundation is accepted
- Release 4.2 — Odontogram Surface Foundation is accepted
- Release 4.3 — Basic Dental Findings Foundation is accepted
- Release 4.4 — Dental Findings Change History is accepted

## Scope
Release 4 is being delivered in bounded slices. The currently accepted slices are Release 4.1, Release 4.2, Release 4.3, and Release 4.4, and they cover:

- explicit odontogram creation
- exactly one odontogram per patient per tenant
- explicit `GET` returning `404` when missing
- no autocreation
- permanent adult FDI/ISO two-digit tooth numbering only (`11-18`, `21-28`, `31-38`, `41-48`)
- tooth-level current state
- minimal status catalog `Unknown` / `Healthy` / `Missing` / `Restored` / `Caries`
- explicit single-tooth status updates
- minimal O/M/D/B/L surface detail inside the existing odontogram
- minimal surface status catalog `Unknown` / `Healthy` / `Restored` / `Caries`
- explicit single-surface status updates
- no tooth-status auto-aggregation from surface changes in the accepted slices
- basic per-surface findings on top of the accepted surface foundation
- minimal findings catalog `Caries` / `Restoration` / `MissingStructure` / `Sealant`
- explicit single-surface finding add/remove operations
- findings returned inside the existing odontogram read model
- no tooth-status or surface-status auto-aggregation from findings in the accepted slices
- bounded findings change history for those accepted basic findings only
- `FindingAdded` / `FindingRemoved` entries returned newest-first inside the existing odontogram read model
- findings history kept separate from any future dental timeline
- minimal audit metadata on the odontogram and each tooth state
- minimal patient-context UI for empty state, creation, visualization, single-tooth editing, single-surface editing, single-surface findings editing, and selected-surface findings history inside the same odontogram page

## Expected outcome
A user operating under the current odontogram permissions can initialize, consult, and update a bounded odontogram in the patient context, including minimal surface detail, basic surface findings, and bounded add/remove findings history, without opening later dental modules.

## Core users
- Dentist
- Assistant
- Tenant Admin

## Current access note
- `odontogram.read` and `odontogram.write` are granted to `PlatformAdmin` and `TenantAdmin`
- `TenantUser` does not receive odontogram permissions in the currently accepted Release 4.4 slice

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
- Release 5.1 — Treatment Plan Foundation is accepted
- Release 5.2 — Quote Basics is also accepted on top of Release 5.1
- Release 5 remains in progress; advanced pricing, taxes, discounts, billing linkage, and wider commercial workflows remain deferred beyond the accepted 5.2 slice

## Scope
Release 5 is being delivered in bounded slices. The currently accepted slices are Release 5.1 and Release 5.2. Together they currently cover:
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
- minimal patient-context UI for explicit quote creation, empty state, no-treatment-plan message, line pricing, totals, and quote status changes
- `treatmentquote.read` and `treatmentquote.write` restricted to `PlatformAdmin` and `TenantAdmin`
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
- `treatmentplan.read` and `treatmentplan.write` are granted to `PlatformAdmin` and `TenantAdmin`
- `TenantUser` does not receive treatment plan permissions in the accepted Release 5.1 slice
- `treatmentquote.read` and `treatmentquote.write` are granted to `PlatformAdmin` and `TenantAdmin`
- `TenantUser` does not receive treatment quote permissions in the accepted Release 5.2 slice

## Key UX goals
- easy transition from diagnosis to treatment
- clear plan status
- low-friction item capture
- understandable plan status
- simple treatment lifecycle

## Out of scope
- advanced pricing beyond the bounded 5.2 slice
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
- Release 6 is now open in the repository
- Release 6.1 — Billing Foundation is accepted as the current bounded slice of Release 6
- The current accepted scope is limited to explicit billing document creation from an accepted quote, `GET` returning `404` when missing, no autocreation, exactly one billing document per quote, snapshot-only billing lines, inherited simple currency handling from the accepted quote, bounded `Draft` / `Issued` status, and read-only behavior once issued
- `billing.read` and `billing.write` are currently restricted to `PlatformAdmin` and `TenantAdmin`
- Payments, balances, receipts, taxes, discounts, cancellations, CFDI/PAC, multi-billing, and advanced billing workflows remain deferred beyond the accepted Release 6.1 slice

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
- Release 7 is now open in the repository
- Release 7.1 — Documents Foundation is accepted as a bounded slice of Release 7
- Release 7.2 — Dashboard Foundation is accepted as a bounded slice of Release 7
- Release 7 complete is not closed
- The accepted Release 7.1 scope is limited to tenant-owned and patient-owned `PatientDocument` records with explicit patient-scoped upload, active listing, authorized download, logical retire, private local storage, allowlist `application/pdf` / `image/jpeg` / `image/png`, a simple 10 MB maximum size, and no autocreation
- `document.read` and `document.write` are currently restricted to `PlatformAdmin` and `TenantAdmin`; `TenantUser` does not receive document permissions in this slice
- The accepted Release 7.2 scope is limited to a tenant-scoped dashboard summary, pure read-model aggregation, `GET /api/dashboard/summary`, KPI cards for active patients, today appointments, today pending appointments, active documents, active treatment plans, accepted quotes, issued billing documents, and `generatedAtUtc`, with no dashboard table and no persisted snapshots
- `dashboard.read` is currently restricted to `TenantAdmin`; `TenantUser` and `PlatformAdmin` do not receive dashboard permissions in this slice
- OCR, rich preview, versioning, external sharing, templates, generated PDFs, advanced analytics, charts, complex filters, branch dashboard, doctor dashboard, advanced reporting, exports, and advanced document workflows remain deferred beyond the current accepted scope

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
- accepted quotes
- issued billing documents
- advanced analytics, charts, complex filters, daily income summaries, balances, branch dashboards, doctor dashboards, exports, and advanced reporting deferred beyond Release 7.2

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
- Phase 2 is open in progress
- Phase 2.1 — Appointment Confirmation Foundation is accepted
- Phase 2.2 — Manual Reminder Log Foundation is accepted
- Phase 2.3 — Reminder Scheduling Preparation is accepted
- Phase 2.4 — Reminder Worklist Follow-up Actions is accepted
- Phase 2.5 — Reminder Template Draft Foundation is accepted
- Phase 2.6 — Reminder Template Usage Traceability is accepted
- Phase 2 complete is not closed
- The accepted Phase 2.1 slice only adds appointment confirmation as an operational signal on existing Scheduling
- Appointment confirmation status remains separate from `AppointmentStatus` and uses the minimal `Pending` / `Confirmed` catalog
- Phase 2.1 exposes the minimal mutation endpoint `PUT /api/appointments/{id}/confirmation`
- Phase 2.1 reuses `scheduling.read` for reads and `scheduling.write` for mutations; it adds no new permissions
- Phase 2.2 adds only manual contact-attempt logging per existing appointment through `GET /api/appointments/{id}/reminder-log` and `POST /api/appointments/{id}/reminder-log`, with minimal channel/outcome catalogs, optional short notes, creator/timestamp metadata, newest-first reads, existing `scheduling.read` / `scheduling.write`, and no new permissions
- Phase 2.2 does not change `AppointmentStatus` or `AppointmentConfirmationStatus`
- Phase 2.3 adds only manual reminder preparation for existing appointments through explicit set/clear/complete operations, preferred manual channel, due date/time, completion/update metadata, and a branch-aware pending/due reminder list in Scheduling
- Phase 2.3 does not send messages, does not automatically change `AppointmentStatus` or `AppointmentConfirmationStatus`, does not automatically create reminder log entries, reuses `scheduling.read` / `scheduling.write`, and adds no new permissions
- Phase 2.4 adds only an explicit manual follow-up action from the pending/due reminder worklist, creating exactly one reminder log entry and changing reminder completion or appointment confirmation only when explicit request flags ask for those changes
- Phase 2.4 does not auto-complete or auto-confirm when `outcome = Reached`
- Phase 2.4 does not change `AppointmentStatus`, does not change `ReminderDueAtUtc` or `ReminderChannel`, does not add permissions, and does not add persistence schema
- Phase 2.5 adds only internal tenant-owned text template drafts for manual reminder work, active listing, create/update/deactivate, preview/render against an existing appointment in current tenant/branch scope, controlled placeholders `{{patientName}}`, `{{appointmentDate}}`, `{{appointmentTime}}`, `{{branchName}}` and `{{tenantName}}`, unknown placeholders preserved and reported, and manual use of rendered preview as suggested follow-up note text
- Phase 2.5 adds no new permissions and reuses `scheduling.read` / `scheduling.write`
- Phase 2.5 preview does not save anything and does not mutate appointment, template, reminder log, or reminder schedule state
- Phase 2.6 adds only internal traceability when a manual follow-up originated from a Phase 2.5 template source: nullable `ReminderTemplateId` and `ReminderTemplateNameSnapshot` on `AppointmentReminderLogEntry`
- Phase 2.6 keeps both fields null when there is no template source, derives `ReminderTemplateNameSnapshot` server-side from the active template, and does not accept `ReminderTemplateNameSnapshot` from the client
- Phase 2.6 keeps `notes` as the final manual text and does not store a separate template body snapshot, rendered body snapshot, `TemplateVersion`, `DeliveryTemplateId`, `ProviderTemplateId`, or delivery metadata
- Phase 2.6 adds no new permissions and reuses `scheduling.read` / `scheduling.write`
- WhatsApp, email, SMS sending, automatic reminders, provider integrations, jobs, queues, webhooks, online booking, patient portal, external delivery templates, campaigns, retry automation, real reminder scheduler, delivery status, and advanced dashboard behavior remain outside Phase 2.6

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
