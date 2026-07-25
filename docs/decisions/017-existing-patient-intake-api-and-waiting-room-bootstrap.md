# ADR 017 — Existing-Patient Intake API and Waiting-Room Bootstrap Boundary

- **Status:** Accepted
- **Date:** 2026-07-25
- **Decision Type:** Sensitive self-service API closure, public bootstrap, authorization and browser handoff
- **Scope:** Phase 2.1 / PI-2B closure and PI-2C opening
- **Applies To:** Patient Intake, patient authentication, staff authorization, persistence, API, Angular and operations
- **Tracking:** epic #2; PI-2 #5; PI-2B #33 / PR #34; PI-2C #35; PI-2C1 #36; PI-2C2 #37; PI-2C3 #38

## Context

ADR 016 accepted the patient-proposed field set, the fixed 39-question catalog, explicit-save semantics, 30-day draft expiry, the future 30-minute waiting-room link, the `patientportal.intake.manage` permission and the `patient_intake` scope.

PI-2A implemented the tenant-owned draft, answer and revision persistence foundation. PI-2B then exposed the first authenticated API for patient-reported medical information. The client explicitly authorized closing PI-2B and opening PI-2C only for waiting-room bootstrap, intake-only identity/session and a minimal staff link/QR workflow.

The existing patient bearer selector routes `/api/patient-portal/*` to the patient scheme. Therefore, staff management endpoints for waiting-room credentials must remain outside that prefix, and intake-only access must have an explicit scope and policy rather than making `patient_id` optional inside the existing patient identity.

## Decision

### 1. Accept PI-2B as the existing-patient self-service API boundary

The accepted contracts are:

```http
POST /api/patient-portal/intake
GET  /api/patient-portal/intake
PUT  /api/patient-portal/intake
```

Rules:

- the existing patient bearer scheme and `scope=patient` are required;
- account, Tenant and Patient ownership come only from the validated session;
- request routes and bodies contain no `TenantId`, account id, Patient id or intake id authority;
- `GET` has no write side effects;
- `POST` explicitly creates the current linked-patient draft;
- `PUT` replaces the complete normalized snapshot with optimistic concurrency;
- sensitive responses use `Cache-Control: no-store`;
- an identical save creates no revision and does not extend expiry;
- an effective save creates exactly one immutable revision atomically;
- stale writes return conflict and never overwrite;
- canonical Patient and Clinical data remain unchanged.

PI-2B is accepted through PR #34 and merge commit `7325a73e7f86ae0e6f0557574fe9d9756a89293f`, with CI #315 green.

### 2. PI-2C uses a separate tenant-owned waiting-room credential

PI-2C introduces a credential that is not a `PatientPortalInvitation`, because a new waiting-room visitor has no canonical Patient.

The credential:

- belongs to exactly one `TenantId`;
- may carry an optional active same-tenant `BranchId` as operational context;
- contains no `PatientId`;
- uses 256 random bits;
- returns the raw token once;
- persists only a SHA-256 hash;
- expires after 30 minutes by default, within bounded configuration;
- is single-use, revocable and transactionally consumed;
- records append-only issue, revoke and consume audit;
- is unique per prospective patient/session;
- is never a static reusable clinic QR.

Multiple active credentials may exist for different waiting patients. PI-2C must not use a single-active-link-per-tenant rule that would cause one receptionist action to invalidate another patient's handoff.

### 3. Staff management uses a dedicated permission and staff-only API prefix

Permission:

```text
patientportal.intake.manage
```

Initial mapping:

- `TenantAdmin`: allowed inside its resolved tenant;
- `TenantUser`: denied;
- `PlatformAdmin`: denied;
- platform override: unavailable.

The management API remains outside `/api/patient-portal/*`, for example:

```http
POST   /api/patient-intake-links
GET    /api/patient-intake-links
DELETE /api/patient-intake-links/{linkId}
```

The raw token appears only in the successful create response. List and revoke contracts expose metadata only and never expose token hashes.

### 4. New-patient access uses an explicit intake-only identity

Transactional activation creates:

1. an unlinked `PatientPortalAccount`;
2. one `PatientIntake` with origin `NewPatientWaitingRoom`;
3. the fixed 39 answers initialized as `Unknown`;
4. consume metadata and append-only audit;
5. an intake-only access token.

The access token uses the existing patient issuer, audience, signing secret and short-lived access-token policy, but a separate scope, claims parser, authorization policy and server-side session validator:

```text
scope = patient_intake
```

Required claims:

```text
sub
 tenant_id
 intake_id
 scope = patient_intake
 session_version
 jti
```

It contains no `patient_id`, staff role, staff permission, Branch or platform claim. It can read and save only the intake owned by the current account and cannot call existing-patient, staff, clinical or commercial APIs.

### 5. The staff handoff UI is minimal and keeps the token local

The TenantAdmin UI may:

- select optional Branch context;
- generate one credential;
- show the resulting URL once;
- copy the URL;
- print a compact handoff sheet;
- render a QR locally in the browser/application bundle;
- list or revoke active credentials when operationally required.

The raw URL/token remains only in current frontend memory. It is not stored in `localStorage`, `sessionStorage`, logs or analytics. QR generation must not call an external web service.

The patient questionnaire UI remains PI-2D.

### 6. PI-2C is delivered sequentially

```text
PI-2C1 — credential domain/persistence and TenantAdmin API (#36)
  -> PI-2C2 — transactional activation and patient_intake session (#37)
  -> PI-2C3 — staff generate/copy/print/local-QR UI and closure (#38)
```

No sub-slice bypasses the previous gate.

## Alternatives considered

### Reuse `PatientPortalInvitation`

**Rejected.** That aggregate requires a canonical Patient and its audit is patient-owned. Waiting-room bootstrap must not invent a Patient before review.

### Make `patient_id` optional in the existing patient token

**Rejected.** It mixes linked and unlinked trust modes and weakens policy clarity. `patient_intake` is explicit and least-privilege.

### Put staff management under `/api/patient-portal/*`

**Rejected.** The bearer selector reserves that prefix for patient authentication; placing staff routes there risks scheme confusion.

### Static reusable clinic QR

**Rejected.** It enables replay, uncontrolled draft creation and weak attribution.

### External QR-generation service

**Rejected.** It would disclose a sensitive one-time URL to an unnecessary third party and introduce vendor/runtime dependency.

### Reuse `patient.write`

**Rejected.** It is too broad and currently belongs to regular tenant users.

## Consequences

### Positive

- PI-2B becomes a stable, self-only and non-canonical existing-patient API;
- waiting-room visitors can be modeled without premature Patient records;
- staff and patient/intake bearer boundaries remain explicit;
- credential delivery remains simple for the pilot without external providers;
- tenant isolation, replay protection and audit are first-class;
- PI-2C remains reviewable through three bounded sub-slices.

### Trade-offs

- a second patient scope and validator increase authentication complexity;
- page refresh continues to require login during the memory-only pilot;
- reception must generate a new credential per waiting patient;
- raw links cannot be recovered after the one-time response;
- retention, privacy text and remote delivery remain PI-4 concerns.

## Non-goals

- patient-facing Angular questionnaire;
- canonical Patient or ClinicalRecord creation/application;
- duplicate matching and staff review queue;
- static QR or public directory;
- email/SMS/WhatsApp delivery;
- external QR service;
- refresh tokens;
- dependents/family accounts;
- final retention or privacy hardening.

## Implementation status

- PI-2B: completed through PR #34 / `7325a73e7f86ae0e6f0557574fe9d9756a89293f`; CI #315 green.
- PI-2C parent: issue #35 active.
- PI-2C1: completed through issue #36 / PR #40 with migration `20260725204625_AddPatientIntakeAccessLinkFoundation`; closure evidence: `docs/pi-2c1-waiting-room-access-link-foundation.md`.
- PI-2C2: issue #37 is the next gate.
- PI-2C3: issue #38 remains blocked by PI-2C2.
- PI-2D, PI-3 and PI-4: not implemented.
