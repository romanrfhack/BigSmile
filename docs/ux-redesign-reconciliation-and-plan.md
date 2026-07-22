# BigSmile UX Redesign Reconciliation and Plan

## 1. Objective

Reconcile canonical release state, actual repository code and the progressive visual redesign requested by the client.

This document distinguishes:

- functional implementation;
- formal release acceptance;
- visual/operational UX debt;
- future roadmap scope.

Visual work does not automatically open or close releases and must not change backend behavior, APIs, permissions, auth, tenant context, branch context, migrations or accepted functional scope unless a separate functional slice explicitly does so.

## 2. Canonical product state

### Completed and preserved

- Foundation / Release 0 base.
- Pre-auth hardening.
- Identity + Persistence Foundation.
- Tenant-Aware Authorization Foundation.
- Release 1 — Patients.
- Release 2 — Scheduling.
- Release 3 — Clinical Records through slices 3.1 to 3.6.
- Release 4 — Odontogram through slices 4.1 to 4.4.
- Release 5 — Treatments and Quotes through slices 5.1 and 5.2.
- Release 6 — Billing through Release 6.1.

### Release 4 reconciliation

Odontogram is no longer only `implemented but not formally accepted/reconciled`.

Closure evidence:

- `docs/release-4-odontogram-audit-and-closure.md`;
- ADR 007 — `docs/decisions/007-release-4-odontogram-closure.md`.

Accepted Release 4 slices:

1. Odontogram Foundation.
2. Odontogram Surface Foundation.
3. Basic Dental Findings Foundation.
4. Dental Findings Change History.

### Release 5 reconciliation

Treatments/Quotes is no longer only `implemented but not formally accepted/reconciled`.

The module received a specific audit covering:

- plan and quote domain invariants;
- tenant/patient ownership;
- application services and no-autocreation behavior;
- API contracts;
- EF Core configuration and migrations;
- permissions and role mappings;
- Angular data-access/facades/pages/components;
- tenant, pricing, lifecycle, controller and frontend tests;
- separation from Billing and treatment execution.

Closure evidence:

- `docs/release-5-treatments-and-quotes-audit-and-closure.md`;
- ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`.

Accepted Release 5 slices:

1. Treatment Plan Foundation.
2. Quote Foundation.

### Release 6 reconciliation

Billing is no longer only `implemented but not formally accepted/reconciled`.

The module-specific audit accepted:

- explicit Billing creation from an accepted quote;
- one Billing document per quote and no autocreation;
- snapshot-only lines with preserved currency and totals;
- `Draft -> Issued` lifecycle and issue metadata;
- issued-document immutability;
- tenant/permission protection;
- patient-context Angular create/read/issue flows.

Closure evidence:

- `docs/release-6-billing-audit-and-closure.md`;
- ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`.

Payments, balances, receipts, cash sessions, fiscal/CFDI behavior and Patient Portal access remain outside Release 6.1.

### Current roadmap frontier

The next planned functional phase is **Release 7 — Documents and Dashboard**.

Documents and Dashboard code exists, but remains `implemented but not formally accepted/reconciled` until module-specific audits occur.

### Phase 2.1

Patient Intake and Portal Foundation is planned after the initial MVP:

- architecture accepted in ADR 006;
- implementation tracked in issues #4 to #7;
- no patient-facing runtime implementation opened;
- full patient portal remains outside the bounded Phase 2.1 scope.

## 3. Current code by module

Legend:

- **Accepted / preserved:** release boundary formally closed.
- **Implemented but not reconciled:** code exists; formal audit/acceptance pending.
- **Visual-only pending:** functional boundary is stable; presentation debt remains.
- **Planned:** no accepted runtime capability yet.

| Module | Backend | Frontend | Endpoints | Permissions | Migrations | Tests | Functional state | Visual state |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Login/Shell | yes | yes | `/api/auth/login`, `/api/auth/me` | auth/scope foundation | identity base | yes | Accepted / preserved | partially redesigned |
| Patients | yes | yes | `api/patients` | `patient.read/write` | yes | yes | Accepted / preserved | partially redesigned |
| Scheduling | yes | yes | appointments, blocks, manual reminder helpers | `scheduling.read/write` | yes | yes | Accepted / preserved; automated delivery not accepted | partially redesigned |
| Clinical Records | yes | yes | record, questionnaire, encounters, notes, diagnoses | `clinical.read/write` | yes | yes | Accepted / preserved | partially redesigned |
| Odontogram | yes | yes | chart, teeth, surfaces, findings | `odontogram.read/write` | yes | yes | Accepted / preserved through Release 4.4 | partially redesigned; debt remains |
| Treatments/Quotes | yes | yes | plan, items, status, quote, pricing | treatment permissions | yes | yes | Accepted / preserved through Release 5.2 | partially redesigned; debt remains |
| Billing | yes | yes | quote/billing paths | `billing.read/write` | yes | yes | Accepted / preserved through Release 6.1 | partially redesigned; debt remains |
| Documents | yes | yes | upload/list/download/retire | `document.read/write` | yes | yes | Implemented but not reconciled | partially redesigned |
| Dashboard | yes | yes | summary | `dashboard.read` | read-model based | yes | Implemented but not reconciled | partially redesigned |
| Patient Intake/Portal | no | no | none accepted | none | none | none | Planned Phase 2.1 | not implemented |

## 4. Drift and resolution

### Resolved drift: Odontogram

The module-specific audit proved the bounded foundation and closed Release 4. Advanced Odontogram scope remains deferred:

- child/mixed dentition;
- bulk editing;
- full tooth/surface history;
- restore/versioning;
- treatment/diagnosis/document/imaging linkage;
- advanced orthodontic/periodontal charting;
- AI detection;
- patient-portal access.

### Resolved drift: Treatments/Quotes

The Release 5 audit proved:

- explicit treatment-plan creation and no autocreation;
- bounded items with optional FDI/surface references;
- plan lifecycle and accepted-plan immutability;
- explicit quote snapshot creation from a non-empty plan;
- fixed `MXN` public path;
- line pricing and calculated totals;
- positive-pricing gates;
- accepted-quote immutability;
- tenant and permission protection.

This does not accept:

- treatment execution/progress;
- multiple/archived plans;
- quote versioning or negotiation;
- taxes/discounts;
- Billing or payment behavior;
- Patient Portal access.

### Resolved drift: Billing

The Release 6 audit proved the bounded BillingDocument snapshot/issue foundation without accepting payments or fiscal behavior.

### Remaining drift: later modules

Documents, Dashboard and reminder helpers contain real code beyond the formal release frontier.

They must remain unaccepted until their own audit verifies:

- bounded roadmap fit;
- domain ownership;
- tenant isolation;
- permission behavior;
- migrations/contracts;
- UI structure;
- tests;
- explicit deferred scope.

### Internal terminology visible to clinic users

Several screens still expose development/release language such as:

- `Release 4.4`;
- `Release 5.1`;
- `Release 5.2`;
- `Release 6.1`;
- `Release 7.1`;
- `slice`;
- `foundation`;
- roadmap-oriented technical explanations.

This copy is useful for review but not for clinic operations. Replace it progressively with user-facing language without changing functional behavior.

### Large UI files

Dense files include Scheduling, Clinical Records, the medical questionnaire, encounter/vitals, Odontogram grid/editor, Treatments/Quotes pages/components and later-module pages.

File size alone is not a defect, but it is a signal to:

- separate presentation responsibilities when a bounded slice justifies it;
- avoid giant mixed-purpose diffs;
- preserve feature facade/data-access boundaries;
- prefer tabs, drawers and focused components where they reduce operational friction.

### Visual tokens

`--bsm-*` tokens are the accepted visual base. Residual hardcoded colors remain in Odontogram, Treatments/Quotes and later modules.

When touching those screens:

- migrate colors to existing semantic tokens;
- add a new token only for a real reusable need;
- preserve contrast/focus/status semantics;
- do not perform unrelated domain/API refactors.

## 5. Correct interpretation of current work

- Releases 1 to 6 are closed and preserved.
- Release 7 is next, but must begin with audits of existing Documents and Dashboard code.
- Do not add new Release 7 functionality until the audits identify a concrete accepted-scope gap.
- Do not reopen Clinical Records, Odontogram or Treatments/Quotes through incidental linkage.
- Existing permission-gated navigation from Quote to Billing does not accept Billing behavior.
- Visual-only work remains allowed on accepted modules when bounded and non-breaking.
- Phase 2 is not active.
- Phase 2.1 Patient Intake and Portal is planned, not implemented.
- Manual reminders/templates do not imply real email/SMS/WhatsApp delivery, jobs, queues, retries or campaigns.
- Doctor-based views remain deferred until provider/doctor assignment is intentionally modeled.

## 6. Operational UX rules

- Keep primary actions visible in the first viewport when practical.
- Use compact headers and summary strips.
- Use tabs for peer sections, not to hide the primary workflow.
- Use drawers for contextual detail that should preserve page position.
- Use modals for focused create/edit/confirm tasks.
- Use sticky action bars for long editable forms.
- Use loading, empty and error states in the region they affect.
- Preserve entered data on validation errors.
- Keep keyboard access, visible focus and `prefers-reduced-motion` support.
- Never communicate status by color alone.
- Keep visible copy behind i18n.
- Do not add a UI library without an explicit decision.

## 7. Module-specific UX direction

### Patients
- preserve fast search and registration;
- keep identity/status/alerts visible;
- maintain permission-aware links to accepted clinical, Odontogram and Treatments contexts.

### Scheduling
- keep branch context visible;
- protect low-friction create/reschedule flows;
- keep manual reminders explicitly manual;
- do not add doctor views without provider assignment.

### Clinical Records
- preserve tabs/sections for summary, background, consultations, notes, diagnoses and history;
- keep timeline and snapshot history separate;
- preserve questionnaire provenance and current-allergy separation.

### Odontogram
- keep the chart as the central work area;
- keep selected tooth/surface context visible;
- preserve explicit create/update behavior;
- replace internal release copy;
- migrate residual hardcoded colors;
- do not introduce treatment linkage as a visual shortcut.

### Treatments/Quotes
- preserve explicit creation and snapshot semantics;
- keep plan/quote state and next action visible;
- keep accepted plans/quotes visibly read-only;
- replace internal release/slice copy;
- do not imply execution, payment or tax behavior;
- keep Billing navigation visibly separate from accepted quote behavior.

### Billing
- preserve explicit snapshot creation and issue semantics;
- keep source quote, issue state, monetary totals and next action visible;
- keep issued documents visibly read-only;
- do not imply payments, balances, receipts, tax/CFDI or cancellation behavior;
- never visually suggest that Billing edits the accepted quote snapshot;
- replace internal release/slice copy and raw ids through bounded UX work.

### Documents/Dashboard
- preserve existing code as unaccepted until module audit;
- avoid presenting advanced deferred behavior;
- use visual-only slices only when contracts and scope stay unchanged.

## 8. Recommended sequence

1. Preserve Release 6 closure documents and canonical state.
2. Audit existing Release 7 Documents code as a bounded storage/access slice.
3. Audit existing Release 7 Dashboard code as a separate read-model slice.
4. Classify each behavior as satisfied, bounded gap or out of accepted scope.
5. Make only the smallest necessary implementation changes.
6. Run repository-wide CI.
7. Reconcile STATE and base docs before advancing the release frontier.
8. Continue visual debt separately from functional acceptance when possible.

## 9. Visual acceptance criteria

A visual slice is acceptable when:

- primary workflow is clear;
- actions are visible or sticky;
- tenant/branch/patient context remains clear;
- loading/empty/error/read-only states work;
- responsive behavior is usable;
- keyboard/focus/contrast are preserved;
- user-facing copy contains no unnecessary internal release terminology;
- no backend/API/auth/permission/migration behavior changes accidentally;
- relevant frontend tests and CI pass.

## 10. Risks

### Product-state drift
Mitigation: require module audit and aligned base docs before closure.

### Visual work hiding functional changes
Mitigation: separate visual-only and functional diffs; state scope explicitly.

### Tenant/security regression
Mitigation: never change tenant/auth paths as incidental UX work; run backend/integration tests.

### Closed-scope leakage
Mitigation: defer cross-module linkage and advanced features to dedicated slices.

### Commercial-state confusion
Mitigation: preserve TreatmentPlan/TreatmentQuote snapshots and document any Billing/payment transition explicitly rather than inferring it in UI.

### Operational friction
Mitigation: review common clinic workflows, not only visual appearance.

## 11. Decision note

**Context:** Repository code is ahead of formal state in several modules, and the client also requested progressive visual improvement.

**Decision:** Accept Releases 4, 5 and 6 only after their specific audits; keep later modules unaccepted until equivalent evidence exists; continue visual work as bounded non-functional slices.

**Consequence:** Release 7 becomes the next audit target, while Odontogram, Treatments/Quotes and Billing visual debt remains valid follow-up without reopening accepted functional boundaries.
