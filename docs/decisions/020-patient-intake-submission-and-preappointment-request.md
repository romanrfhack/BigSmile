# ADR 020 — Patient Intake Submission and Pre-Appointment Request

- **Status:** Accepted
- **Date:** 2026-08-06
- **Decision Type:** Patient-intake lifecycle, scheduling UX and authorization
- **Scope:** PI-3A — final patient submission plus optional pre-appointment access preparation
- **Applies To:** API, Application, Domain, Infrastructure, Scheduling UI, patient intake UI and authorization

## Context

PI-2 closed explicit draft capture for linked and waiting-room patient sessions, but a saved draft did not mean that the patient had completed the medical history. Reception also had to wait until the patient reached the waiting room before preparing access.

The clinic needs an optional action from the appointment module that can prepare patient access early, including a manual WhatsApp handoff. It must not repeatedly request the form after the patient has completed it, must not confuse a partial draft with completion, and must not give reception the broader waiting-room or portal-administration permissions.

The clinic-review and canonical-apply portion of PI-3 remains a separate safety boundary. This decision does not authorize patient-originated writes to canonical `Patient`, `ClinicalRecord` or `ClinicalMedicalAnswer` data.

## Decision

### 1. Explicit final submission

`PatientIntakeStatus` adds `Submitted` and the self-only patient endpoint adds:

```http
POST /api/patient-portal/intake/submit
```

The request carries only the current concurrency token. Tenant, account, Patient and intake ownership continue to come from the validated patient or `patient_intake` session.

Submission is accepted only when:

- the intake is a current, non-expired `Draft`;
- the caller owns the intake through the validated session;
- first name, last name and date of birth are present;
- all 39 fixed medical-history questions have an explicit `Yes` or `No` answer;
- the optimistic concurrency token still matches.

An accepted submission:

- changes `Draft` to `Submitted`;
- records `SubmittedAtUtc`;
- appends one immutable revision whose changed field is `status`;
- makes the intake non-editable and non-expiring;
- is idempotently readable by the same current session.

A linked canonical Patient can have at most one submitted patient intake. A saved or partially completed draft never counts as completed.

### 2. One-time request rule

Appointment request status uses the submitted intake as the durable completion marker:

```text
NotStarted -> InProgress -> Completed
```

- `NotStarted`: no current draft and no submitted intake;
- `InProgress`: a current draft exists but has not been submitted;
- `Completed`: a submitted intake exists for the linked Patient.

Reception may remind a patient while status is `NotStarted` or `InProgress`. Once it is `Completed`, BigSmile disables preparation and explicitly says not to send another request. If a draft expires or remains incomplete, the next appointment can offer access again.

### 3. Access status and prepared handoff

The appointment-scoped staff route is:

```http
GET  /api/appointments/{appointmentId}/patient-intake-request
POST /api/appointments/{appointmentId}/patient-intake-request
```

It derives Patient, Tenant and Branch from the accessible appointment. It does not accept PatientId, TenantId or account identifiers in the body.
Access can be prepared only while the appointment is `Scheduled`; cancelled, attended and no-show appointments remain visible for traceability but cannot issue a new request.

Portal access is presented as:

```text
NotActivated | Active | RecoveryRequired
```

- no account: prepare a normal single-use existing-patient activation invitation;
- active account: prepare the tenant-realm patient login URL;
- inactive account: require assisted recovery and prepare no link.

The raw activation token is returned only by the explicit prepare action, stays in component memory and is placed in the URL fragment. Status reads never return a raw token.

### 4. Independent reception permission

The dedicated permission is:

```text
patientportal.intake.request
```

It requires a resolved tenant context and does not allow platform override. It is granted to `TenantAdmin` and the current `TenantUser` reception role mapping. It does not grant:

- `patientportal.intake.manage`;
- `patientportal.invitation.manage`;
- `patientportal.account.recover`;
- clinical review or canonical apply.

The current authorization model assigns permissions by role catalog. Per-membership custom permission assignment remains future work.

### 5. Manual WhatsApp boundary

BigSmile prepares a click-to-chat URL only after the staff user explicitly prepares access. The UI:

- displays the generated access URL for review;
- supports copy-to-clipboard;
- normalizes a supported patient phone for WhatsApp;
- opens WhatsApp in a new tab with a prefilled message;
- requires the receptionist to review and press send.

BigSmile does not claim delivery, create an outbound-message audit event or call a messaging provider. Automated WhatsApp delivery, templates, consent, retries and provider webhooks remain deferred.

## Persistence

The additive migration is:

```text
20260806131500_AddPatientIntakeSubmissionBoundary
```

It adds nullable `SubmittedAtUtc`, enforces status/submission metadata consistency, and creates a filtered unique index for one submitted linked-patient intake per Tenant and Patient.

## Security and tenancy boundaries

- Every appointment is checked against the resolved Tenant and accessible Branch.
- Platform override cannot satisfy the request service or policy.
- Status and prepare responses use `Cache-Control: no-store`.
- No public or staff request can select another Patient through a body identifier.
- Completion is based on persisted `Submitted`, never UI state or the presence of any draft.
- Patient data remains a proposal; no canonical clinical write occurs.
- Activation tokens are not logged, persisted in plaintext or placed in query strings.

## Consequences

### Positive

- Reception can request the form when creating/following up an appointment instead of waiting for arrival.
- The UI clearly separates portal access from medical-history completion.
- Patients who already submitted are not asked to fill the history again.
- Partially completed patients can be reminded at a later appointment.
- The new staff capability follows least privilege.

### Trade-offs

- Completion currently means one lifetime submitted intake, not a configurable periodic health-history refresh.
- Active accounts receive a login URL rather than a new secret link.
- An inactive account needs the existing assisted-recovery workflow.
- WhatsApp delivery remains manual and is not auditable as sent/delivered by BigSmile.

## Deferred

- PI-3B clinic review, duplicate resolution, accept/reject and canonical apply.
- Periodic re-attestation or clinic-configurable history refresh intervals.
- Per-user custom permission assignment beyond role catalog mapping.
- Automated WhatsApp provider integration, consent and delivery tracking.
- Applying waiting-room submissions to a newly created canonical Patient.

## Validation gate

This decision is accepted only with:

- domain tests for completeness, ownership, immutability and expiry behavior;
- service tests for idempotence, concurrency and blocking subsequent linked-patient drafts;
- appointment tests for Branch/Tenant isolation and all access/completion states;
- permission-policy tests proving no platform override and no broader reception permissions;
- frontend tests for URL/token handling, phone normalization and save-before-submit;
- migration/model-snapshot alignment;
- production frontend build and repository CI green.
