# ADR 006 — Patient Intake and Portal Foundation

- **Status:** Accepted
- **Date:** 2026-07-22
- **Decision Type:** Product scope, authentication boundary, clinical-data workflow
- **Scope:** Patient self-registration, existing-patient activation, medical-history intake and audit
- **Applies To:** Identity, Patients, Clinical Records, API, frontend, persistence, security and roadmap
- **Roadmap Placement:** Phase 2.1 after the initial MVP
- **Tracking:** issue #2; implementation issues #4, #5, #6 and #7

## Context

The client confirmed three requirements:

1. A new patient waiting at the clinic can register and begin completing demographic/contact data and the fixed medical-history questionnaire.
2. An existing patient can activate access and complement information not yet captured by the clinic.
3. Patient-originated changes remain traceable and require clinic review before canonical application.

Current authentication is staff-oriented. `User` signs in through `/api/auth`, requires `UserTenantMembership` and receives tenant operational permissions from a staff role. Reusing that boundary for patients would violate least privilege.

The current clinical questionnaire stores latest values and updater metadata but is not an immutable patient-originated intake history. Canonical `Patient` and `ClinicalRecord` data must not be created or overwritten directly from anonymous/public endpoints.

This decision formalizes a bounded intake/update foundation for Phase 2. It does not open a full patient portal.

## Decision

BigSmile will introduce a separate **Patient Intake and Portal** boundary.

### 1. Separate patient identity

A tenant-owned `PatientPortalAccount` will:

- remain separate from `UserTenantMembership` and staff roles;
- link to at most one canonical Patient in Phase 2.1;
- carry patient-specific activation, lockout, recovery and session metadata;
- authenticate through dedicated patient endpoints/scheme;
- use patient-specific JWT audience/scope and self-only policies;
- never receive `patient.read`, `patient.write`, `clinical.read`, `clinical.write`, scheduling, billing, document or platform permissions.

`TenantId`, portal-account id and linked `PatientId` come from verified server-issued context. Request bodies and arbitrary route identifiers are not authority sources.

### 2. Existing-patient activation

Existing patients activate through a staff-issued `PatientPortalInvitation` bound server-side to:

- `TenantId`;
- `PatientId`;
- purpose;
- short expiry;
- single-use state;
- revocation state.

Only a cryptographic token hash is stored. Consumption, expiry and revocation are enforced transactionally. Public patient search/claim by name, email, phone or date of birth is forbidden.

### 3. New-patient waiting-room entry

Staff may generate a short-lived tenant-scoped QR/link.

The public flow creates a `PatientIntake` draft, not a canonical Patient or ClinicalRecord. Staff later resolves duplicates and decides whether to link an existing Patient or create a new one.

### 4. Staged data workflow

Patient-originated data follows:

```text
Draft -> Submitted -> Reviewed -> Applied
                           \-> Rejected
```

- `Draft`: patient can save effective changes.
- `Submitted`: immutable revision awaiting review.
- `Reviewed`: staff inspected identity/data/conflicts.
- `Applied`: accepted fields copied through explicit canonical use cases.
- `Rejected`: submission retained without canonical application.

Patient/public endpoints do not directly modify:

- diagnoses;
- clinical notes;
- encounters/vitals;
- current clinical alerts;
- current allergies without explicit review;
- odontogram;
- treatments/quotes;
- billing;
- documents.

Questionnaire answers remain attributed as patient-originated. Staff application remains separately attributable.

### 5. Append-only revisions and audit

Every effective save or lifecycle/security transition records at minimum:

- tenant;
- intake and linked patient when available;
- actor type (`Patient`, `Staff`, `System`);
- actor id;
- action;
- UTC timestamp;
- revision number;
- changed field identifiers or normalized diff;
- correlation id;
- only justified source metadata.

Passwords, token values, raw authorization headers and secrets are never audited. The system does not log every keystroke.

### 6. Frontend separation

Angular adds a patient-specific route/feature area separate from the staff shell, with:

- patient auth/session service;
- patient route guards;
- feature-local facade/data-access/models;
- mobile/tablet-first waiting-room forms;
- the same fixed medical-question catalog;
- explicit save/submit states;
- clear clinic-review messaging.

The existing staff clinical-record questionnaire remains the internal operational view.

### 7. Security requirements

Phase 2.1 requires:

- rate limiting on anonymous activation/registration;
- generic anti-enumeration errors;
- secure password hashing or an approved stronger compatible method;
- token hashing and constant-time comparison;
- transactional single-use/expiry/revocation;
- lockout/recovery before remote production readiness;
- server-side ownership checks on every patient read/write;
- centralized tenant enforcement for all new tenant-owned entities;
- no platform override in patient policies;
- automated IDOR, replay, cross-tenant, expiry, revocation and concurrency coverage.

### 8. Roadmap placement and gate

Phase 2.1 starts only after the initial MVP is formally accepted and stable unless a future explicit decision reprioritizes it.

Current roadmap frontier after Release 4 closure:

```text
Release 5 -> Release 6 -> Release 7 -> Phase 2.1
```

The current next planned functional phase is Release 5 — Treatments and Quotes.

The broader patient portal remains Phase 4 work. Phase 2.1 is limited to activation, self-service intake/update, clinic review/application and audit.

## Implementation slices

### PI-1 — Access and Invitation Foundation — issue #4

- account/invitation entities;
- persistence/indexes/migration/tenant filters;
- staff issue/revoke endpoints;
- patient activation/login/current-session endpoints;
- dedicated claims/scheme/policies/rate limiting;
- replay/expiry/revocation/concurrency tests;
- no questionnaire writes.

### PI-2 — Intake Draft — issue #5

- `PatientIntake` and fixed questionnaire draft answers;
- new-patient and prelinked existing-patient intake;
- self-only read/save;
- optimistic concurrency;
- append-only effective-save revisions;
- mobile/tablet frontend.

### PI-3 — Submit, Review and Apply — issue #6

- immutable submitted revision;
- staff review worklist;
- duplicate/link/create decisions;
- explicit canonical application;
- provenance/conflict/idempotency handling;
- transition audit.

### PI-4 — Audit Visibility and Hardening — issue #7

- staff-visible audit timeline;
- lockout/recovery/session revocation;
- invitation/session revocation;
- privacy/retention controls;
- e2e and operational runbook.

## Alternatives considered

### Reuse `User` / `TenantUser`

**Rejected.** It mixes staff membership with patient self-service and risks tenant-wide operational permissions.

### Create Patient/ClinicalRecord directly from public registration

**Rejected.** It creates duplicate, spam, ownership and clinical-integrity risks.

### Let patients write canonical questionnaire answers directly

**Rejected for Phase 2.1.** It blurs patient statements with clinic-reviewed data and weakens conflict/provenance handling.

### One-time forms without accounts

**Not selected as target model.** Useful for a kiosk-only flow but insufficient for returning existing patients.

### Generic application audit only

**Insufficient.** Intake requires domain-level immutable revisions and lifecycle provenance.

### Implement before remaining MVP releases

**Not selected.** The client confirmed the need but did not reprioritize it ahead of Release 5 to Release 7.

## Consequences

### Positive

- strict least-privilege patient access;
- staff auth remains backward compatible;
- public registration does not pollute canonical records;
- existing patients link without public search;
- patient-originated updates have immutable provenance;
- implementation can proceed in bounded slices.

### Trade-offs

- second auth boundary increases implementation/operations complexity;
- staff review adds an explicit step;
- existing-patient activation requires invitation;
- remote activation eventually requires delivery/recovery decisions;
- dependents/multiple patients per account are deferred;
- capability remains planned until the MVP gate or reprioritization.

## Non-goals

- online booking;
- patient access to professional clinical records or odontogram;
- treatment/quote/billing/document access;
- automated email/SMS/WhatsApp;
- digital signature/advanced consent;
- family/dependent accounts;
- configurable form builder;
- automatic clinical interpretation or allergy/alert synchronization.

## Product confirmation

Product confirmed on 2026-07-22 that:

1. clinic review precedes canonical application;
2. existing-patient access begins through staff-issued single-use invitation;
3. waiting-room pilot may use clinic-generated QR/link without external provider;
4. the capability does not displace the current MVP roadmap;
5. full patient portal remains separate future scope.

## Implementation status

- Decision: accepted.
- General plan: `docs/patient-intake-and-portal-plan.md`.
- Parent: issue #2.
- PI-1 to PI-4: planned, not implemented.
- Backend/API/database/frontend patient-facing implementation: not started.
