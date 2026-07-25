# ADR 013 — Patient Portal Invitation Management

- **Status:** Accepted
- **Date:** 2026-07-25
- **Decision Type:** Authorization, credential bootstrap, security audit
- **Scope:** PI-1B — Staff-issued patient portal invitation lifecycle
- **Applies To:** Identity, Patients, Application, API, persistence, authorization and operations
- **Tracking:** Phase 2.1 epic #2; PI-1 #4; PI-1B #23; implementation PR #28

## Context

PI-1A established tenant-owned `PatientPortalAccount` and `PatientPortalInvitation` persistence without exposing a public patient runtime.

PI-1B introduces the first staff operation capable of issuing a credential-bootstrap secret linked to an existing Patient. Reusing `patient.write` would grant this security-sensitive capability to every current `TenantUser`, which is broader than the pilot requires. Allowing platform override would also introduce cross-tenant credential issuance into the patient identity boundary.

The client explicitly approved a dedicated permission restricted to `TenantAdmin`, with no `TenantUser`, `PlatformAdmin` or platform-override access.

## Decision

### 1. Dedicated staff permission

BigSmile introduces:

```text
patientportal.invitation.manage
```

Initial role mapping:

- `TenantAdmin`: granted inside its resolved tenant;
- `TenantUser`: not granted;
- `PlatformAdmin`: not granted;
- platform override: unavailable.

The policy requires an authenticated, resolved tenant context. It does not enable platform override.

A future reception-specific authorization model may grant this same permission to selected users through a bounded role/permission refinement. PI-1B does not add a new role or grant the capability to all regular tenant users.

### 2. Staff-only API boundary

PI-1B exposes only authenticated staff routes:

```text
GET    /api/patients/{patientId}/portal-invitations
POST   /api/patients/{patientId}/portal-invitations
DELETE /api/patients/{patientId}/portal-invitations/{invitationId}
```

The Patient is resolved through the current tenant-filtered repository path. Request bodies do not choose `TenantId`, actor, token hash or ownership.

No public activation, login, claim or Patient-search endpoint is introduced.

### 3. One-time token handling

Invitation issuance uses 32 cryptographically random bytes encoded as Base64URL.

- the raw token is returned only in the successful issue response;
- the raw token is never persisted or included in list/revoke DTOs;
- persistence stores only the uppercase hexadecimal SHA-256 hash;
- token comparison and transactional consumption remain PI-1C responsibilities;
- logs and audit records must never contain the raw token.

The 256-bit random token provides sufficient entropy for hash-at-rest without relying on patient demographics or contact data.

### 4. Lifetime and replacement

Existing-patient activation invitations use a configurable lifetime:

- default: 24 hours;
- allowed configuration range in PI-1B: 1 to 168 hours.

Issuing a replacement invitation explicitly revokes every outstanding, unconsumed invitation for the same tenant, Patient and purpose before inserting the new invitation.

A filtered unique index enforces at most one outstanding invitation for:

```text
TenantId + PatientId + Purpose
```

where `RevokedAtUtc` and `ConsumedAtUtc` are null.

Expired but unconsumed invitations are superseded explicitly during replacement so the filtered uniqueness rule remains deterministic.

### 5. Append-only security audit

PI-1B adds tenant-owned `PatientPortalSecurityAuditEntry` records for:

- `InvitationIssued`;
- `InvitationRevoked`;
- `InvitationSuperseded`.

Each entry records:

- tenant;
- Patient;
- invitation;
- staff actor;
- action;
- UTC timestamp;
- correlation id.

Normal persistence rejects modification or deletion of these entries. The audit does not contain token values, password material or authorization headers.

### 6. Transaction and concurrency boundary

Relational issuance runs inside a transaction:

1. revoke and audit outstanding invitations;
2. persist those transitions;
3. insert the replacement invitation and issued audit entry;
4. commit.

The existing invitation `RowVersion` remains the concurrency guard for later revocation and PI-1C consumption. Database uniqueness provides an additional race guard for concurrent issue operations.

## Alternatives considered

### Reuse `patient.write`

**Rejected.** `TenantUser` already holds that permission, which would expose credential issuance too broadly.

### Grant the dedicated permission to all `TenantUser`

**Rejected for the pilot.** It preserves the same excessive access under a different name.

### Add a `FrontDesk` role now

**Deferred.** It would expand role seed, membership semantics and authorization scope beyond PI-1B. A later bounded role-refinement slice can grant the dedicated permission selectively.

### Allow `PlatformAdmin` or platform override

**Rejected.** Cross-tenant patient credential issuance is not required for the pilot and would weaken the strict patient identity boundary.

### Store encrypted raw tokens

**Rejected.** Activation needs token verification, not recovery. Hash-at-rest avoids retaining a reusable credential.

### Use email, phone or date of birth to claim an account

**Rejected.** Contact and demographic data are not reliable proof of ownership and would create enumeration/account-takeover risk.

## Consequences

### Positive

- invitation management follows least privilege;
- tenant ownership remains server-authoritative;
- raw activation credentials are not recoverable from storage;
- replacement and revocation are explicit and auditable;
- staff authentication and existing Patient contracts remain backward compatible;
- PI-1C can build transactional activation on a stable invitation lifecycle.

### Trade-offs

- only `TenantAdmin` can operate the pilot invitation flow;
- reception requires an explicitly authorized tenant-admin account until granular roles exist;
- the additional audit table and filtered index require an additive migration;
- public activation remains unavailable until PI-1C;
- full audit visibility remains PI-4 work.

## Non-goals

- public patient activation or login;
- patient JWT/session scheme;
- password hashing/verification;
- waiting-room link runtime;
- patient frontend;
- intake/questionnaire capture;
- automatic email/SMS/WhatsApp delivery;
- platform support impersonation;
- role redesign;
- canonical Patient or ClinicalRecord changes.

## Validation requirements

PI-1B must retain automated evidence for:

- TenantAdmin-only permission mapping;
- no platform override;
- token generation and hash-at-rest;
- absence of token/hash from safe list contracts;
- tenant/cross-tenant behavior;
- replacement, revocation and lifecycle conflicts;
- append-only, tenant-filtered audit;
- migration/model guardrails;
- repository-wide backend, architecture and frontend CI.

## Follow-up

After PI-1B is accepted, the only next PI-1 slice is PI-1C (#24): patient activation, login and self-session boundary. PI-1C must separately decide and validate password-hash versioning, patient JWT audience/scope, token comparison, transactional single-use consumption, anti-enumeration, rate limiting, lockout enforcement and session invalidation.
