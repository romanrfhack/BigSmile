# ADR 018 — Patient Intake Waiting-Room Link Management

- **Status:** Accepted
- **Date:** 2026-07-25
- **Decision Type:** Public-bootstrap credential, staff authorization, tenant isolation and audit
- **Scope:** PI-2C1 — Waiting-room credential foundation and TenantAdmin management API
- **Tracking:** Phase 2.1 #2; PI-2 #5; PI-2C #35; PI-2C1 #36; PR #41
- **Depends On:** ADR 006, ADR 012–017

## Context

PI-2A established tenant-owned intake drafts and immutable effective-save revisions. PI-2B exposed self-only create/read/save for an already linked patient account. The remaining client requirement includes a new patient in the clinic waiting room starting an intake without creating a canonical `Patient` or `ClinicalRecord` before staff review.

A static reusable clinic QR would be operationally simple but would create replay, uncontrolled sharing, spam and attribution risks. A public request also cannot accept an internal `TenantId`, `PatientId` or account identifier as authority.

PI-2C is therefore split into sequential sub-slices. This ADR accepts only PI-2C1: credential ownership, staff management, persistence and audit. Public consumption and the `patient_intake` session boundary remain PI-2C2. The Angular staff link/print/QR workflow remains PI-2C3.

## Decision

### 1. Unique link per prospective session

BigSmile creates a distinct `PatientIntakeAccessLink` for each waiting-room registration attempt.

The credential is:

- tenant-owned;
- optionally associated with an active Branch from the same tenant;
- purpose-bound to `NewPatientWaitingRoomRegistration`;
- generated from 256 random bits;
- returned raw exactly once on issue;
- persisted only as a SHA-256 hash;
- expirable, revocable and single-use;
- protected by `rowversion` and database constraints.

Multiple active links are allowed because each represents a different prospective patient/session. No static reusable tenant or branch QR is introduced.

### 2. Expiry

The default lifetime is 30 minutes and is configured by:

```text
PatientPortal:Intake:WaitingRoomLinkLifetimeMinutes
```

The accepted initial bounds are 5 to 120 minutes. Configuration outside those bounds fails explicitly during dependency construction.

### 3. Staff authorization

A dedicated permission is introduced:

```text
patientportal.intake.manage
```

Initial mapping:

| Role / scope | Access |
| --- | --- |
| `TenantAdmin` with resolved tenant | Allowed |
| `TenantUser` | Denied |
| `PlatformAdmin` | Denied |
| Platform override | Not available |

The Application service independently rejects platform scope and platform override. The capability is not inferred from `patient.write` or `patientportal.invitation.manage`.

### 4. Staff API

The accepted staff endpoints are outside `/api/patient-portal/*`, so the existing staff bearer selector applies:

```http
POST   /api/patient-intake-links
GET    /api/patient-intake-links
DELETE /api/patient-intake-links/{linkId}
```

Rules:

- `POST` may accept only optional `BranchId`; Tenant and actor come from verified context;
- the raw token appears only in the successful issue response;
- all responses set `Cache-Control: no-store`;
- list responses contain metadata only and never return raw token or hash;
- revoke is tenant-scoped and only succeeds for an active link;
- no endpoint in PI-2C1 consumes the credential or creates an account/intake.

### 5. Append-only audit

`PatientIntakeAccessLinkAuditEntry` records:

- tenant;
- optional Branch;
- link identifier;
- action `Issued`, `Revoked` or future `Consumed`;
- actor type and actor identifier;
- UTC timestamp;
- correlation id.

The audit does not contain raw token, token hash, password, JWT, authorization header or clinical answers. `AppDbContext` rejects normal modification or deletion of audit entries.

### 6. Tenant and Branch boundaries

`TenantId` remains the primary ownership and security boundary. Optional `BranchId` is operational context only and must resolve to an active Branch inside the same tenant.

Global query filters and centralized `ITenantOwnedEntity` write enforcement apply to links and audit entries. No route or request body selects another tenant.

### 7. Prepared but unopened consumption metadata

The schema includes nullable consumption metadata and an optional consuming `PatientPortalAccount` reference so PI-2C2 can consume the token transactionally without a second additive reshape of the link table.

This does not open public consumption. PI-2C2 must still add:

- fixed-time token verification in the public path;
- atomic unlinked account plus intake creation;
- `scope=patient_intake` and a separate self policy;
- replay, rate-limit, session-version and partial-failure coverage.

## Alternatives considered

### Static reusable clinic or branch QR

**Rejected.** It is easy to copy and replay and cannot provide per-session revocation or attribution.

### Reuse `patient.write`

**Rejected.** `TenantUser` currently receives it, which would overgrant credential issuance.

### Reuse `patientportal.invitation.manage`

**Rejected.** Existing-patient account linking and anonymous new-patient bootstrap are different security capabilities and require distinct audit semantics.

### Allow PlatformAdmin support override

**Rejected.** Cross-tenant issuance of public bootstrap credentials is not required for the pilot and increases account/bootstrap risk.

### Create `Patient` during link issuance

**Rejected.** The link represents a prospective session, not an accepted canonical patient. Creation remains behind PI-3 staff review.

## Consequences

### Positive

- per-session replay and revocation boundary;
- hash-only secret persistence;
- least-privilege staff capability;
- explicit tenant and optional Branch ownership;
- immutable issue/revoke evidence;
- no change to staff auth, patient auth or canonical clinical modules;
- schema ready for transactional PI-2C2 consumption.

### Trade-offs

- reception must generate a new link for each prospective patient;
- a raw link cannot be recovered after the issue response;
- staff UI is not available until PI-2C3;
- public new-patient registration remains unavailable until PI-2C2;
- tokens currently require manual in-clinic delivery; no external provider is introduced.

## Non-goals

- anonymous token consumption;
- unlinked account creation;
- `scope=patient_intake`;
- patient-facing intake UI;
- static QR;
- email/SMS/WhatsApp delivery;
- Patient or ClinicalRecord creation/update;
- submit/review/apply;
- remote recovery;
- final retention/privacy policy.

## Result

PI-2C1 establishes the controlled staff side of waiting-room bootstrap. PI-2C remains incomplete until PI-2C2 and PI-2C3 are accepted. PI-2D remains responsible for the patient intake capture UI and formal PI-2 closure.
