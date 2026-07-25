# ADR 016 — Patient Intake Draft and Self-Service Baseline

- **Status:** Accepted
- **Date:** 2026-07-25
- **Decision Type:** Sensitive patient-input domain, draft lifecycle, future public bootstrap and authorization scope
- **Scope:** Phase 2.1 / PI-2 — Patient Intake Draft and Self-Service Capture
- **Applies To:** Domain, persistence, API, patient authentication, staff authorization, Angular, audit and operations
- **Tracking:** epic #2; PI-2 #5; PI-2A #31; PR #32

## Context

PI-1 established and closed the patient access foundation through:

- tenant-owned portal accounts and invitations;
- tenant-admin-only invitation management;
- separate patient activation/login/session backend;
- memory-only Angular patient auth outside the staff shell;
- assisted recovery and append-only security audit.

The next client requirement is to let:

1. an existing linked patient complement missing information;
2. a new patient in the waiting room begin registration and medical history;
3. every effective patient-originated change remain traceable;
4. clinic staff review proposals before canonical `Patient` or `ClinicalRecord` data changes.

The repository already owns the fixed 39-question medical catalog and a bounded Patient demographic model. It does not yet own typed mobile/home/work phone slots, waiting-room intake links, intake drafts or patient-originated revisions.

The client explicitly approved the concrete PI-2 baseline before implementation.

## Decision

### 1. Patient intake is a separate tenant-owned aggregate

`PatientIntake` is a mutable self-only working draft. It does not replace or directly mutate canonical `Patient`, `ClinicalRecord`, `ClinicalMedicalAnswer`, allergy, alert, diagnosis, note, encounter or vital-sign data.

Every intake is owned by:

- exactly one `TenantId`;
- exactly one `PatientPortalAccount`;
- zero or one canonical Patient;
- optional Branch operational context that must belong to the same tenant.

Branch remains subordinate to Tenant and is never the ownership boundary.

### 2. Approved patient-proposed fields

#### Identity and demographics

- `FirstName`;
- `LastName`;
- `DateOfBirth`;
- `Sex` using the existing Patient enum;
- `Occupation`;
- `MaritalStatus` using the existing Patient enum;
- `ReferredBy`.

Age is derived in the UI and is never persisted.

#### Contact and responsible party

- `PreferredPhone`;
- `MobilePhone`;
- `HomePhone`;
- `WorkPhone`;
- `Email`;
- `ResponsiblePartyName`;
- `ResponsiblePartyRelationship`;
- `ResponsiblePartyPhone`.

Typed phones are intake proposals only during PI-2. Canonical `Patient.PrimaryPhone` remains backward compatible. Before PI-3 applies reviewed contact changes, a bounded additive Patient Contact Details prerequisite must define canonical typed phone storage without a large Patients rewrite.

#### Patient-reported visit context

- `ReasonForVisit`, bounded to 500 characters.

This is a patient declaration for review. PI-2 never creates or updates `ClinicalEncounter`, `ChiefComplaint`, consultation type or vitals.

#### Explicitly excluded

Patients cannot edit through intake:

- `IsActive`;
- clinical alerts or alert summary;
- professional current-allergy records;
- diagnoses, notes, encounters or vitals;
- consultation type;
- billing/fiscal data;
- odontogram, treatment, quote, Billing, document, scheduling or dashboard data.

### 3. Fixed medical questionnaire reuse

PI-2 reuses exactly `ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys`:

- 39 fixed keys;
- values `Unknown / Yes / No`;
- UI labels `Sin respuesta / Sí / No`;
- optional details bounded to 500 characters;
- `Unknown` remains the safe default and never becomes implicit `No`;
- unknown, missing or duplicate keys are rejected server-side.

Intake answers are separate from canonical `ClinicalMedicalAnswer` rows.

### 4. Existing-patient draft creation

- `GET` has no write side effect;
- explicit `POST` creates the active draft;
- one active draft is allowed per portal account;
- approved Patient fields may be prefilled and a normalized canonical baseline is retained for later PI-3 conflict review;
- questionnaire answers are not silently copied from professional Clinical records in PI-2A;
- ownership comes from the authenticated account, never an arbitrary Patient/intake id from the client.

### 5. New-patient waiting-room bootstrap

PI-2C will use a unique link per prospective patient/session, not a static reusable clinic QR.

`PatientIntakeLink` baseline:

- tenant-owned;
- optional same-tenant Branch context;
- purpose `NewPatientWaitingRoomRegistration`;
- 256-bit random token;
- only SHA-256 hash persisted;
- raw token returned once;
- default TTL 30 minutes, configurable;
- single-use, revocable and transactionally consumed;
- generic errors and rate limiting;
- no canonical Patient or ClinicalRecord creation.

The browser receives the token through a URL fragment and removes it immediately, following ADR 015.

### 6. Intake-only account scope

A new waiting-room patient starts with an unlinked `PatientPortalAccount` and intake created atomically. It must not pretend that a canonical Patient already exists.

PI-2C extends the accepted patient issuer/audience/signing boundary with:

```text
scope = patient_intake
```

Required claims:

- account `sub`;
- `tenant_id`;
- `intake_id`;
- `scope = patient_intake`;
- `session_version`;
- `jti`.

`patient_id` is absent until PI-3 links or creates the canonical Patient. A separate `PatientIntakeSelf` policy authorizes only the account's own active intake. Linking later increments `SessionVersion` so intake-only tokens become invalid.

No staff roles, permissions, Branch claims or platform override are introduced.

### 7. Staff authorization for waiting-room links

PI-2C introduces:

```text
patientportal.intake.manage
```

Initial mapping:

- `TenantAdmin`: allowed inside its resolved tenant;
- `TenantUser`: denied;
- `PlatformAdmin`: denied;
- platform override: unavailable.

This permission is not reused for patient self-service and does not grant canonical apply authority.

### 8. Draft lifecycle and expiry

PI-2 uses only:

```text
Draft -> Expired
```

Rules:

- one active draft per portal account;
- default expiry is 30 days after creation or the last effective save;
- expiry is configurable within bounded values;
- each effective save extends expiry;
- an identical normalized save does not extend expiry;
- expiry is soft and terminal in PI-2;
- no hard delete or retention policy is introduced before PI-4;
- submit/review/apply/reject transitions belong to PI-3.

### 9. Explicit save and immutable revisions

PI-2 selects explicit save and rejects autosave for the first implementation.

Each effective save:

- receives a complete normalized snapshot;
- uses optimistic concurrency;
- updates current draft state;
- creates exactly one immutable `PatientIntakeRevision` in the same transaction;
- records a monotonically increasing revision number, actor account, UTC timestamp, changed-field identifiers, correlation id and versioned normalized snapshot;
- extends draft expiry.

An identical save:

- creates no revision;
- changes no current state;
- does not extend expiry.

Every-keystroke auditing and debounced autosave are deferred.

### 10. Sequential delivery

PI-2 is delivered only in this order:

1. **PI-2A — Intake Domain and Persistence**
2. **PI-2B — Existing-Patient Self-Service Draft**
3. **PI-2C — Waiting-Room Link and Intake-Only Account Scope**
4. **PI-2D — Angular Intake Capture and PI-2 Closure**

No later sub-slice bypasses an earlier gate.

## PI-2A accepted boundary

PI-2A adds only:

- `PatientIntake`;
- current fixed medical answer rows;
- immutable effective-save revisions;
- linked/unlinked origin invariants;
- approved proposal fields;
- optional same-tenant Branch context;
- draft/expiry/revision behavior;
- rowversion, indexes, query filters, centralized write enforcement and additive migration;
- unit/integration/model tests.

PI-2A adds no endpoint, public token, staff permission, JWT claim, Angular intake UI or canonical write.

## Alternatives considered

### Create a provisional Patient during public bootstrap

**Rejected.** It creates duplicates and canonical data before clinic review.

### Reuse a linked-patient token with optional `patient_id`

**Rejected.** It makes two different trust modes ambiguous. Unlinked intake accounts require explicit `patient_intake` scope and policy.

### Static reusable clinic QR

**Rejected.** It increases replay, spam and uncontrolled draft creation risk.

### Reuse `patient.write` for waiting-room links

**Rejected.** It grants credential/bootstrap capability too broadly and currently includes regular tenant users.

### Debounced autosave

**Deferred.** It adds request, concurrency and audit noise before the workflow is validated.

### Generic contact-point subsystem in PI-2

**Rejected for now.** Typed phone proposals satisfy the familiar form requirement without a large Patients rewrite. Canonical contact storage is a bounded prerequisite before PI-3 apply.

## Consequences

### Positive

- patient declarations remain separate from professional canonical data;
- existing and new patients can share one bounded intake model;
- one active draft and self-only ownership are explicit;
- typed phone information can be captured without breaking current Patient contracts;
- effective changes are traceable without keystroke-level noise;
- new-patient bootstrap avoids premature canonical records;
- implementation remains sequential and reviewable.

### Trade-offs

- a second patient scope is required in PI-2C;
- explicit save can lose unsaved browser edits, so PI-2D must provide dirty/leave-warning UX;
- 30-day soft-expired rows remain stored until PI-4 defines retention;
- reception remains responsible for generating and delivering waiting-room links;
- typed phones cannot be applied canonically until Patient Contact Details is defined.

## Non-goals

- canonical Patient/ClinicalRecord mutation;
- submit, clinic review, duplicate resolution or apply;
- professional clinical browsing;
- static public QR;
- remote email/SMS/WhatsApp delivery;
- refresh tokens;
- hard-delete/retention policy;
- configurable form builder;
- automatic allergy/alert interpretation or synchronization;
- full patient portal.

## Exit conditions

### PI-2A

- domain/persistence constraints and additive migration committed;
- exact catalog and effective-save semantics tested;
- revisions append-only;
- tenant filters/write enforcement and concurrency metadata verified;
- no endpoint or canonical write added;
- repository CI green;
- canonical docs distinguish PI-2A completed from PI-2B pending.

### PI-2 overall

PI-2 closes only after PI-2A through PI-2D are accepted with code, tests, runbooks and aligned documentation. PI-3 remains responsible for submit, review and canonical application.
