# Bigsmile Product Roadmap

## Purpose

This document defines the controlled product sequence for Bigsmile, a multi-tenant SaaS platform for dental clinics and private practices.

The roadmap exists to:
- validate business value early
- protect tenant isolation and architecture
- keep scope explicit
- preserve a coherent operational workflow
- distinguish implemented code from formally accepted product scope
- support future SaaS growth without premature complexity

## 1. Product Direction

Bigsmile supports the clinic workflow:

**appointment → patient record → clinical record → odontogram → treatment plan → quote → payment → follow-up**

The operational core comes before automation-heavy or premium capabilities.

## 2. Roadmap Principles

### 2.1 Operational core first
The initial releases must make day-to-day clinic work possible before adding broad portals, automation or advanced analytics.

### 2.2 Protect the foundation
No feature may compromise:
- tenant isolation
- security
- maintainability
- modularity
- reviewability
- operational UX

### 2.3 Grow by business capability
Deliver bounded domain value, not disconnected technical tasks.

### 2.4 Small but real MVP
Each accepted release must be useful, tested and explicit about deferred scope.

### 2.5 Code presence is not acceptance
A module is not complete merely because entities, endpoints, UI, migrations or tests exist. Formal acceptance requires a module-specific audit and aligned documentation.

## 3. Product Phases Overview

- **Release 0 — Foundation** — completed
- **Release 1 — Patients** — completed
- **Release 2 — Scheduling** — completed
- **Release 3 — Clinical Records** — completed
- **Release 4 — Odontogram** — completed
- **Release 5 — Treatments and Quotes** — completed
- **Release 6 — Billing** — completed
- **Release 7 — Documents and Dashboard** — next planned functional phase
- **Phase 2 Expansion — Modern Operations**
  - **Phase 2.1 — Patient Intake and Portal Foundation**
- **Phase 3 Expansion — SaaS Growth**
- **Phase 4 Expansion — Advanced Product Capabilities**

Phase 2.1 is a bounded intake/update foundation and does not replace the full patient portal planned for Phase 4.

---

## 4. Release 0 — Foundation

### Status
Completed.

### Goal
Build the technical and architectural base before business modules.

### Accepted scope
- solution structure
- modular monolith baseline
- multi-tenancy foundation
- authentication and tenant-aware authorization
- roles and permissions baseline
- tenant/branch context
- EF Core + SQL Server persistence
- migrations and durable seed
- auditing/logging/error-handling foundations
- architecture tests
- CI/CD foundation

### Outcome
Bigsmile has a compilable, testable and tenant-safe foundation for business releases.

### Deferred
- microservices
- database-per-tenant
- distributed infrastructure without demonstrated need

---

## 5. Release 1 — Patients

### Status
Completed and preserved.

### Goal
Register, maintain and search patients safely within tenant scope.

### Accepted scope
- patient registration/update/search/profile
- responsible-party data
- active/inactive status
- basic clinical alerts
- basic validations including future date-of-birth rejection
- tenant-owned, branch-neutral patient model
- `patient.read` / `patient.write`
- operational Angular flows

### Deferred
- full clinical record
- odontogram
- treatment workflow
- patient portal
- advanced segmentation

---

## 6. Release 2 — Scheduling

### Status
Completed and preserved.

### Goal
Provide a practical branch-aware operational calendar.

### Accepted scope
- day/week calendar
- create/edit/reschedule/cancel appointments
- appointment notes
- `Scheduled` / `Cancelled` / `Attended` / `NoShow`
- tenant-owned and branch-aware appointments
- blocked slots
- collision validation
- `scheduling.read` / `scheduling.write`
- operational Angular calendar/form flows

### Deferred
- doctor-based views and provider assignment
- online booking
- automated reminder delivery
- advanced waiting-room workflows

ADR 003 records the explicit doctor-view deferral.

---

## 7. Release 3 — Clinical Records

### Status
Completed as the foundational clinical release.

### Closure evidence
- Release 3.1 — Clinical Record Foundation
- Release 3.2 — Basic Diagnoses Foundation
- Release 3.3 — Clinical Timeline Read Model
- Release 3.4 — Clinical Snapshot Change History
- Release 3.5 — Medical Questionnaire Backend
- Release 3.6 — Clinical Encounter / Vitals Backend

### Accepted scope
- tenant-owned/patient-owned `ClinicalRecord`
- exactly one active record per patient/tenant
- explicit creation; `GET` returns `404` when missing
- no autocreation
- medical background/current medications/current allergies
- append-only notes, newest-first
- basic diagnoses add/resolve
- bounded timeline from note/diagnosis activity
- separate bounded snapshot history
- fixed medical-question catalog with `Unknown` / `Yes` / `No` and optional details
- clinical encounters with reason/type and optional vitals
- server-derived tenant and actor
- `clinical.read` / `clinical.write`
- bounded Angular UI through data-access/facade

### Current access
- `PlatformAdmin` and `TenantAdmin`: clinical read/write
- `TenantUser`: no clinical permissions

### Deferred
- advanced/cross-module clinical timeline
- full versioning, restore or rich diff
- configurable form builder
- automatic allergy synchronization
- encounter edit/delete
- provider/doctor assignment
- patient self-service
- cross-module Clinical ownership

---

## 8. Release 4 — Odontogram

### Status
Completed as the foundational Odontogram release.

### Closure evidence
- Release 4.1 — Odontogram Foundation
- Release 4.2 — Odontogram Surface Foundation
- Release 4.3 — Basic Dental Findings Foundation
- Release 4.4 — Dental Findings Change History
- Audit: `docs/release-4-odontogram-audit-and-closure.md`
- ADR 007: `docs/decisions/007-release-4-odontogram-closure.md`

### Goal
Provide a bounded visual dental chart in patient context.

### Accepted scope
- tenant-owned/patient-owned `Odontogram`
- exactly one odontogram per patient/tenant
- explicit creation; `GET` returns `404` when missing
- no autocreation from reads/updates
- 32 permanent adult FDI teeth:
  - `11-18`
  - `21-28`
  - `31-38`
  - `41-48`
- tooth status:
  - `Unknown`
  - `Healthy`
  - `Missing`
  - `Restored`
  - `Caries`
- explicit single-tooth updates
- five surfaces per tooth: `O/M/D/B/L`
- surface status: `Unknown` / `Healthy` / `Restored` / `Caries`
- explicit surface updates
- basic finding catalog:
  - `Caries`
  - `Restoration`
  - `MissingStructure`
  - `Sealant`
- explicit finding add/remove and duplicate prevention
- append-only finding add/remove history, newest-first
- UTC/actor metadata
- `odontogram.read` / `odontogram.write`
- patient-context Angular chart/editor
- tenant/cross-tenant/backend/frontend tests

### Current access
- `PlatformAdmin` and `TenantAdmin`: odontogram read/write permissions
- `TenantUser`: no odontogram permissions
- patient-scoped operations require resolved tenant context

### Deferred
- child/mixed dentition
- bulk editing
- complex/configurable findings
- treatment/diagnosis/document/imaging linkage
- complete tooth/surface history
- full dental timeline
- restore/revert/full versioning
- orthodontic/periodontal charting
- imaging overlays
- AI-assisted findings
- patient-portal access

### Non-blocking UX debt
- replace internal release/slice copy with clinic-facing language
- migrate residual hardcoded colors to `--bsm-*` tokens
- split large UI components only through bounded visual slices

---

## 9. Release 5 — Treatments and Quotes

### Status
Completed as the foundational treatment-planning and quote release.

### Closure evidence
- Release 5.1 — Treatment Plan Foundation
- Release 5.2 — Quote Foundation
- Audit: `docs/release-5-treatments-and-quotes-audit-and-closure.md`
- ADR 008: `docs/decisions/008-release-5-treatments-and-quotes-closure.md`

### Goal
Connect patient/clinical/odontogram context to an operational treatment proposal and bounded commercial quote workflow.

### Release 5.1 accepted scope
- tenant-owned/patient-owned `TreatmentPlan`
- exactly one plan per patient/tenant in the current slice
- explicit creation; `GET` returns `404` when missing
- no autocreation from reads/items/status operations
- basic items:
  - required normalized title
  - optional category
  - positive integer quantity
  - optional short notes
  - optional permanent-adult FDI tooth reference
  - optional `O/M/D/B/L` surface requiring a tooth
- explicit add/remove items
- lifecycle `Draft` / `Proposed` / `Accepted`
- `Proposed -> Draft` allowed
- accepted plan read-only
- tenant/actor metadata
- bounded Angular patient-context UI

### Release 5.2 accepted scope
- explicit quote creation from an existing non-empty treatment plan
- one quote per treatment plan
- no autocreation of plan or quote
- snapshot-only quote lines
- fixed `MXN` public application/API path
- line unit price, line total and quote total
- SQL decimal precision `18,2`
- lifecycle `Draft` / `Proposed` / `Accepted`
- positive price required before Proposed and Accepted
- proposed quote cannot retain non-positive pricing
- accepted quote read-only
- bounded Angular quote/pricing UI

### Current access
- `PlatformAdmin` and `TenantAdmin`: treatment-plan/quote read/write permissions
- `TenantUser`: no treatment-plan or quote permissions
- patient-scoped operations require resolved tenant context

### Deferred
- treatment catalog administration
- multiple or archived treatment plans
- plan archive/versioning
- quote regenerate/versioning
- multiple quotes/negotiation
- taxes and discounts
- Billing/payment linkage
- scheduling linkage
- treatment execution/progress
- automatic plan/quote/Billing status synchronization
- insurance and financing
- advanced approval workflows
- automated treatment follow-up
- patient-portal access

### Non-blocking hardening/UX debt
- normalize concurrent-create uniqueness conflicts if demonstrated in real usage
- introduce a shared dental-location primitive only if cross-module reuse grows
- replace internal `Release 5.1/5.2`, `foundation` and `slice` copy with clinic-facing language
- migrate residual hardcoded colors to `--bsm-*` tokens

---

## 10. Release 6 — Billing

### Status
Completed through Release 6.1 — Billing Document Foundation.

### Closure evidence
- Release 6.1 — Billing Document Foundation
- Audit: `docs/release-6-billing-audit-and-closure.md`
- ADR 009: `docs/decisions/009-release-6-billing-document-foundation.md`

### Goal
Provide a bounded commercial record created from an accepted treatment quote while preserving future payment and fiscal workflows as separate capabilities.

### Accepted scope
- tenant-owned/patient-owned `BillingDocument`
- explicit creation from an existing `Accepted` TreatmentQuote
- `GET` returns `404` when missing
- no autocreation from reads/status operations
- one Billing document per TreatmentQuote
- snapshot-only lines preserving source item, description, optional dental location, quantity, unit price and line total
- preserved currency and total with SQL precision `decimal(18,2)`
- lifecycle `Draft -> Issued`
- issue UTC/actor metadata
- issued document read-only
- `billing.read` / `billing.write`
- bounded Angular patient-context create/read/issue workflow
- tenant/cross-tenant/backend/frontend tests

### Current access
- `PlatformAdmin` and `TenantAdmin`: Billing read/write permissions
- `TenantUser`: no Billing permissions
- patient-scoped operations require resolved tenant context

### Deferred
- payment registration/allocation
- partial/total payment lifecycle and balances/ledger
- receipts, refunds, reversals and cancellations
- cash sessions and daily closing
- taxes, discounts and CFDI/PAC
- insurance and multi-currency
- accounting/ERP workflows
- multiple Billing documents, regeneration or versioning
- automatic mutation of accepted TreatmentQuote state
- Patient Portal access

### Non-blocking hardening/UX debt
- normalize concurrent-create unique conflicts if real use requires it
- add optimistic concurrency before expanding concurrent issue roles
- decide repeated-issue idempotency explicitly
- add relational SQL Server constraint/precision coverage when CI supports it
- replace internal release/slice copy and raw ids in clinic-facing UI

---

## 11. Release 7 — Documents and Dashboard

### Status
Next planned functional phase; not formally opened or accepted.

### Documents planned scope
- tenant-owned/patient-owned document records
- explicit authorized upload
- private storage
- allowlisted PDF/JPG/PNG
- size limits
- active list
- authorized download
- logical retire

### Dashboard planned scope
- tenant-scoped read model
- active patients
- today/pending appointments
- active documents
- active treatment plans
- accepted quotes
- issued Billing documents from accepted Release 6.1

### Deferred
- OCR/rich preview/versioning
- public/external sharing
- generated PDFs/templates
- advanced analytics/charts/exports
- branch/doctor dashboards
- BI-level reporting

Existing Documents/Dashboard code requires module-specific audit before acceptance.

---

## 12. MVP Definition

The initial operational MVP is complete only after formal acceptance of:
- Patients
- Scheduling
- Clinical Records
- Odontogram
- Treatments and Quotes
- Billing
- Documents
- Roles and Permissions
- Basic Dashboard

Current accepted frontier: **Release 6**.

Remaining MVP release acceptance: **Release 7**.

---

## 13. Phase 2 Expansion — Modern Operations

### Status
Later phase after the initial MVP is accepted and stable.

### Phase 2.1 — Patient Intake and Portal Foundation

#### Status
Architecturally accepted in ADR 006; implementation not opened.

#### Goal
Allow new and existing patients to propose or complement information through least-privilege self-only flows while keeping canonical data under clinic review.

#### Accepted architecture
- patient identity separate from staff membership/permissions
- existing-patient activation through staff-issued single-use invitation
- new-patient waiting-room QR/link creates intake draft, not canonical Patient/ClinicalRecord
- `Draft -> Submitted -> Reviewed -> Applied/Rejected`
- clinic review before canonical application
- append-only revisions/audit
- no platform override in patient-facing policies

#### Sequential implementation
1. PI-1 — Access and Invitation Foundation — issue #4
2. PI-2 — Intake Draft and Self-Service Capture — issue #5
3. PI-3 — Submit, Clinic Review and Canonical Apply — issue #6
4. PI-4 — Audit Visibility and Security Hardening — issue #7

#### Included
- patient portal account/invitation foundation
- waiting-room intake link
- existing-patient activation
- demographic/contact proposals
- existing fixed medical-question catalog
- draft/submission/review/apply
- append-only audit
- rate limiting, anti-enumeration, replay/revocation/recovery controls

#### Not included
- professional clinical-record browsing
- odontogram access
- treatments/quotes/billing/documents access
- online booking
- automated email/SMS/WhatsApp
- dependents/multiple patients per account
- configurable form builder
- automatic clinical interpretation or allergy/alert synchronization

#### Tracking
- ADR 006: `docs/decisions/006-patient-intake-and-portal-foundation.md`
- plan: `docs/patient-intake-and-portal-plan.md`
- parent issue #2

### Other Phase 2 candidates
- reminders/confirmations with real providers
- online booking
- branch refinements
- dashboard metric improvements
- treatment follow-up visibility

---

## 14. Phase 3 Expansion — SaaS Growth

Candidate capabilities:
- advanced multi-branch administration
- tenant settings and branding
- feature flags/plans/subscriptions
- onboarding automation
- tenant support tooling
- platform administration improvements
- tenant-level analytics

Goal: strengthen commercial SaaS operation without weakening tenant isolation.

---

## 15. Phase 4 Expansion — Advanced Product Capabilities

Candidate capabilities:
- full patient portal beyond Phase 2.1
- approved patient access to selected records/documents
- advanced analytics/conversion metrics
- recall and automation campaigns
- inventory basics
- electronic invoicing
- future AI-assisted features

Phase 2.1 does not imply the broader Phase 4 portal is implemented or accepted.

---

## 16. Release Dependencies

- Foundation is required by all releases.
- Patients precedes Clinical.
- Scheduling depends on tenant/user/branch foundations.
- Clinical depends on Patients.
- Odontogram depends on Patients and the clinical context foundation.
- Treatments/Quotes depends on stable patient/clinical/odontogram context.
- Billing depends on accepted TreatmentPlan/TreatmentQuote contracts.
- Documents depend on Patients and secure storage/access.
- Dashboard depends on accepted upstream read models.
- Phase 2.1 depends on stable Patients/Clinical behavior and the accepted MVP.
- Full patient portal remains a separate future capability.

This dependency chain guides order unless an explicit documented reprioritization changes it.

---

## 17. Non-Goals for Initial Product Stages

- microservices
- database-per-tenant from day one
- multi-region infrastructure
- full ERP/accounting
- advanced insurance claims
- excessive reporting depth
- advanced AI workflows
- enterprise integrations without validated need

---

## 18. Validation Priorities

### Product
- Are registration/search flows fast?
- Does scheduling work daily?
- Does Clinical feel natural and safe?
- Is the accepted Odontogram understandable and efficient?
- Do treatment plans and quotes support real patient conversations without implying execution or Billing?
- Can Billing remain simple and traceable for front desk?
- When Phase 2.1 opens, can patients complete intake without confusing declarations with reviewed canonical data?

### SaaS/security
- Is tenant isolation preserved?
- Is Branch used only as subordinate operational scope?
- Are permissions practical and explicit?
- Are module boundaries sustainable?
- Are accepted commercial snapshots protected from silent downstream mutation?
- Are public/patient-facing identities strictly self-scoped?

---

## 19. Risks to Control

- scope growth beyond bounded releases
- accepting modules only because code exists
- hidden cross-module ownership
- weakening tenant isolation
- branch used as tenant replacement
- silent synchronization between treatment, quote, Billing and payment states
- staff permissions reused for patients
- public enumeration/IDOR/replay
- silent canonical overwrite of patient/clinical data
- visual complexity that degrades operational speed
- documentation drifting behind code

---

## 20. Success Criteria by Stage

### Foundation
- stable architecture/auth/tenant model
- CI and tests operational

### Release 5 closure
- bounded plan and quote behavior accepted
- tenant/cross-tenant and pricing evidence
- Billing and execution explicitly deferred
- base documentation aligned

### MVP
- clinic can operate the full core workflow
- Releases 1 to 7 formally accepted
- product ready for pilot validation

### Phase 2.1
- new/existing patient workflows work
- no staff permissions granted to patients
- clinic review precedes canonical application
- duplicate/conflict/recovery/revocation paths usable
- every effective patient-originated change traceable

### SaaS growth
- multiple tenants operate safely
- onboarding/platform administration remain sustainable

---

## 21. Guiding Principle

Every feature should answer:

**Does this strengthen the operational core or SaaS foundation at the right time, with explicit tenant-safe evidence?**
