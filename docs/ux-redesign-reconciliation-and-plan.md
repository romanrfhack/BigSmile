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

### Release 4 reconciliation

Odontogram is no longer only `implemented but not formally accepted/reconciled`.

The module received a specific audit covering:

- domain invariants;
- tenant ownership;
- application services;
- API contracts;
- EF Core configuration and migrations;
- permissions and policies;
- Angular data-access/facade/page/components;
- backend and frontend automated tests.

Closure evidence:

- `docs/release-4-odontogram-audit-and-closure.md`;
- ADR 007 — `docs/decisions/007-release-4-odontogram-closure.md`.

Accepted Release 4 slices:

1. Odontogram Foundation.
2. Odontogram Surface Foundation.
3. Basic Dental Findings Foundation.
4. Dental Findings Change History.

### Current roadmap frontier

The next planned functional phase is **Release 5 — Treatments and Quotes**.

Treatments/Quotes code exists, but it remains `implemented but not formally accepted/reconciled` until its own module audit occurs.

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
| Treatments/Quotes | yes | yes | plan, items, status, quote | treatment permissions | yes | yes | Implemented but not reconciled; next audit target | partially redesigned |
| Billing | yes | yes | quote/billing paths | `billing.read/write` | yes | yes | Implemented but not reconciled | partially redesigned |
| Documents | yes | yes | upload/list/download/retire | `document.read/write` | yes | yes | Implemented but not reconciled | partially redesigned |
| Dashboard | yes | yes | summary | `dashboard.read` | read-model based | yes | Implemented but not reconciled | partially redesigned |
| Patient Intake/Portal | no | no | none accepted | none | none | none | Planned Phase 2.1 | not implemented |

## 4. Drift and resolution

### Resolved drift: Odontogram

Previously, canonical docs correctly said Release 4 was not open while code already contained its implementation. The module-specific audit now proves the bounded foundation and closes Release 4.

This does not mean all future Odontogram features are accepted. Deferred scope remains:

- child/mixed dentition;
- bulk editing;
- full tooth/surface history;
- restore/versioning;
- treatment/diagnosis/document/imaging linkage;
- advanced orthodontic/periodontal charting;
- AI detection;
- patient-portal access.

### Remaining drift: later modules

Treatments/Quotes, Billing, Documents, Dashboard and reminder helpers contain real code beyond the formal release frontier.

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
- `Release 6.1`;
- `Release 7.1`;
- `slice`;
- `foundation`;
- roadmap-oriented technical explanations.

This copy is useful for review but not for clinic operations. Replace it progressively with user-facing language without changing functional behavior.

### Large UI files

Dense files include Scheduling, Clinical Records, the medical questionnaire, encounter/vitals, Odontogram grid/editor and later-module pages.

File size alone is not a defect, but it is a signal to:

- separate presentation responsibilities when a bounded slice justifies it;
- avoid giant mixed-purpose diffs;
- preserve feature facade/data-access boundaries;
- prefer tabs, drawers and focused components where they reduce operational friction.

### Visual tokens

`--bsm-*` tokens are the accepted visual base. Residual hardcoded colors remain in Odontogram and later modules.

When touching those screens:

- migrate colors to existing semantic tokens;
- add a new token only for a real reusable need;
- preserve contrast/focus/status semantics;
- do not perform unrelated domain/API refactors.

## 5. Correct interpretation of current work

- Releases 1 to 4 are closed and preserved.
- Release 5 is next, but must begin with an audit of existing code.
- Do not add new Treatments/Quotes functionality until the audit identifies a concrete accepted-scope gap.
- Do not reopen Clinical Records or Odontogram through incidental linkage.
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
- maintain permission-aware links to accepted clinical and Odontogram contexts.

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
- audit functionality before visual expansion;
- keep plan/quote state and next action visible;
- do not imply accepted pricing, billing or execution behavior before formal Release 5 reconciliation.

### Billing/Documents/Dashboard
- preserve existing code as unaccepted until module audit;
- avoid presenting advanced deferred behavior;
- use visual-only slices only when contracts and scope stay unchanged.

## 8. Recommended sequence

1. Preserve Release 4 closure documents and canonical state.
2. Audit existing Release 5 Treatments/Quotes code.
3. Classify each planned 5.x slice as:
   - satisfied and acceptable;
   - partially satisfied with a bounded gap;
   - out of accepted scope.
4. Make only the smallest necessary implementation changes.
5. Run repository-wide CI.
6. Reconcile STATE and base docs before advancing the release frontier.
7. Continue visual debt separately from functional acceptance when possible.

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

### Operational friction
Mitigation: review common clinic workflows, not only visual appearance.

## 11. Decision note

**Context:** Repository code is ahead of formal state in several modules, and the client also requested progressive visual improvement.

**Decision:** Accept Release 4 only after its specific audit; keep later modules unaccepted until equivalent evidence exists; continue visual work as bounded non-functional slices.

**Consequence:** Release 5 becomes the next audit target, while Odontogram visual debt remains valid follow-up without reopening the accepted functional boundary.
