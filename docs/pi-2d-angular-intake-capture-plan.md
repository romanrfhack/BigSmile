# PI-2D — Angular Patient Intake Capture Plan

- **Status:** Active
- **Parent:** PI-2 issue #5
- **Tracking:** PI-2D #44
- **Dependencies:** PI-2A #31, PI-2B #33 and completed PI-2C #35
- **Latest completed increment:** PI-2D1 #45 / PR #50 / CI #459
- **Current active increment:** PI-2D2 #46
- **Required sequence:** #45 → #46 → #47 → #48
- **Architecture:** ADR 006 and ADR 012–019
- **Risk:** High but bounded — browser capture of personal and patient-reported medical data

## 1. Objective

Provide a responsive Angular workspace in which:

- an existing linked patient with `scope=patient` can explicitly create, load and save their own intake draft;
- a new waiting-room patient with `scope=patient_intake` can load and save only the intake created during one-time activation;
- both modes use the same accepted `PatientIntake` API and form contract;
- no patient-facing endpoint creates or modifies canonical `Patient`, `ClinicalRecord` or `ClinicalMedicalAnswer` data.

PI-2D completes capture only. Submission, staff review, duplicate resolution and canonical application remain PI-3.

## 2. Existing accepted backend contract

```http
POST /api/patient-portal/intake
GET  /api/patient-portal/intake
PUT  /api/patient-portal/intake
```

Ownership is resolved server-side:

- `scope=patient`: portal account plus linked Patient;
- `scope=patient_intake`: unlinked portal account plus exact `intake_id` claim.

Rules already accepted:

- POST is linked-patient-only;
- GET has no write side effects;
- new-patient draft is created only during waiting-room activation;
- request routes/bodies do not select Tenant, Patient, account or intake ownership;
- responses are `no-store`;
- PUT sends a complete snapshot and concurrency token;
- unchanged save is a no-op;
- effective save creates one append-only revision and extends sliding expiry;
- stale writes do not overwrite;
- no canonical writes.

## 3. Exact capture fields

PI-2D maps the current API contract; it does not expand backend schema.

### Identity and demographics

- `firstName`;
- `lastName`;
- `dateOfBirth`;
- `sex`;
- `occupation`;
- `maritalStatus`;
- `referredBy`.

Age is derived and read-only. Middle-name and maternal-surname fields are not in the current transport/domain contract and remain outside PI-2D.

### Contact proposals

- `preferredPhone`;
- `mobilePhone`;
- `homePhone`;
- `workPhone`;
- `email`;
- `responsiblePartyName`;
- `responsiblePartyRelationship`;
- `responsiblePartyPhone`.

These remain patient proposals. They do not change canonical Patient contact storage.

### Visit context

- `reasonForVisit`, maximum 500 characters.

It remains a patient declaration and does not automatically become a clinical encounter complaint.

### Medical questionnaire

- exactly 39 existing keys;
- six accepted thematic groups;
- `Unknown / Yes / No` with `Unknown` displayed as `Sin respuesta`;
- optional bounded details;
- progress count excludes `Unknown`;
- no automatic synchronization to clinical allergies, alerts, medications or timeline.

## 4. Increment sequence

### PI-2D1 — Routes, sessions and self-only data access — #45 — completed

Scope:

- add intake-only recurrent login route;
- add shared intake-workspace route;
- preserve linked-patient login/home compatibility;
- keep linked-patient and intake-only stores distinct and mutually exclusive;
- attach exactly one applicable bearer to `/api/patient-portal/intake`;
- never attach patient/intake bearers to staff APIs;
- implement exact frontend DTOs and POST/GET/PUT data access;
- handle loading, missing draft and explicit linked-patient create without rendering the full form.

Exit gate:

- both scope modes reach only their own workspace;
- no bearer ambiguity or browser persistence;
- GET remains side-effect-free;
- CI #459 green; PR #50 merged as `b11d6af1e3e77e325e38a7022059106d3afa23bd`; method-aware bearer hardening merged through PR #51.

### PI-2D2 — Demographics, contact and reason-for-visit — #46 — active

Scope:

- add responsive form sections for exact supported non-medical fields;
- derive age locally without submitting it;
- mirror bounded validation and enum values;
- preserve all 39 loaded medical answers unchanged;
- explicit full-snapshot save only;
- replace local authoritative snapshot/token after a successful response;
- represent `changed=true` versus no-op `changed=false` accurately.

Exit gate:

- both scope modes can save exact supported non-medical fields;
- untouched medical answers remain intact;
- no canonical writes;
- CI green.

### PI-2D3 — Familiar 39-question medical history capture — #47

Scope:

- extract the existing static questionnaire grouping/key metadata into a narrow shared frontend catalog rather than duplicate or deep-import another feature;
- preserve six accepted groups and exact 39-key order;
- visible `Sí / No / Sin respuesta` radio controls;
- optional details and progress;
- full-snapshot explicit save, no per-question request or autosave;
- preserve non-medical fields.

Exit gate:

- exactly 39 unique keys rendered and submitted once;
- clinical and intake consumers share catalog parity;
- accessible responsive behavior and CI green.

### PI-2D4 — Conflict/expiry UX, validation and PI-2 closure — #48

Scope:

- complete dirty/saving/saved/no-op/conflict/expired/unauthorized states;
- stale 409 never force-overwrites;
- retain local edits in memory until explicit reload decision;
- linked-patient replacement path and intake-only reissue guidance;
- unsaved-navigation protection without browser storage;
- accessibility, responsive and representative e2e/smoke validation;
- reconcile canonical docs and close PI-2 only with evidence.

Exit gate:

- conflicts, expiry and session invalidation fail safely;
- operational smoke flows pass;
- canonical data remains unchanged;
- CI green and docs aligned.

## 5. Route and session boundary

Recommended routes:

```text
/patient-portal/:tenantSubdomain/intake-login
/patient-portal/:tenantSubdomain/intake
```

The shared intake workspace accepts one of two mutually exclusive valid session shapes:

```text
scope=patient
  sub + tenant_id + patient_id + session_version

scope=patient_intake
  sub + tenant_id + intake_id + session_version
```

The browser must never make `patient_id` optional inside patient scope or infer ownership from route parameters.

## 6. Frontend ownership

Recommended feature structure:

```text
frontend/src/app/features/patient-intake/
  pages/
  components/
  facades/
  data-access/
  models/
  guards/
```

Responsibilities:

- pages: route-level orchestration;
- components: form sections and presentation only;
- facade: authoritative snapshot, form state, save transitions and conflict state;
- data-access: POST/GET/PUT HTTP only;
- guards/session services: realm and mutually exclusive session checks;
- shared questionnaire catalog: static keys/groups/labels reused by Clinical and Intake.

Do not place intake business orchestration in the patient auth feature or staff shell.

## 7. Explicit-save state model

```text
loading
  → missing / create-available / ready
ready-clean
  → dirty
  → saving
  → saved-changed | saved-noop | conflict | expired | unauthorized | error
```

Rules:

- no autosave;
- no form data in localStorage/sessionStorage/IndexedDB/cookies;
- complete normalized snapshot per PUT;
- current concurrency token always comes from last authoritative response;
- conflict disables repeated stale save;
- no field-level merge in PI-2D;
- unchanged save does not claim a new revision.

## 8. Security requirements

- `TenantId` remains the primary security boundary;
- Branch remains optional operational context;
- no platform override;
- no arbitrary ownership ids in route/body;
- server-side session revalidation remains mandatory;
- staff bearer never reaches patient APIs;
- patient/intake bearer never reaches staff APIs;
- public auth calls carry no bearer;
- sensitive responses remain no-store;
- sessions and form state remain memory-only;
- no canonical data application.

## 9. Validation matrix

| Dimension | Required evidence |
| --- | --- |
| Ownership | linked and intake-only self access; mismatch denied |
| Tenant isolation | cross-tenant access denied |
| Bearer separation | staff/patient/intake boundaries do not cross |
| Routing | realm mismatch and missing session fail closed |
| API | exact DTO mapping; POST/GET/PUT contracts |
| Save semantics | explicit save, no-op, effective revision, stale conflict |
| Questionnaire | six groups, 39 unique keys, Unknown distinct from No |
| Privacy | no browser persistence; no identifiers leaked |
| Accessibility | labels, radio groups, focus, live regions, keyboard path |
| Responsive | mobile, tablet and desktop |
| Regression | accepted staff/clinical flows unchanged |
| Operations | production build, tests, CI and smoke evidence |

## 10. Non-goals

- backend/schema field expansion;
- autosave, offline mode or browser draft persistence;
- configurable questionnaire builder;
- canonical contact migration;
- submit/review/apply or duplicate handling;
- clinical interpretation/synchronization;
- vitals, diagnoses or clinician notes;
- billing/fiscal fields;
- full patient portal browsing;
- refresh tokens or remote recovery;
- final retention/privacy/incident-response hardening.

## 11. Closure rule

PI-2D and PI-2 close only after #45, #46, #47 and #48 complete sequentially with repository-wide CI and canonical documentation aligned. Completion of PI-2 does not automatically open PI-3; review/apply requires a separate explicit decision.