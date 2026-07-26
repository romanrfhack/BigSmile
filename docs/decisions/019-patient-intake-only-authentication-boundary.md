# ADR 019 — Patient Intake-Only Authentication Boundary

- **Status:** Accepted
- **Date:** 2026-07-26
- **Decision Type:** Security and patient-intake architecture
- **Scope:** PI-2C2 — waiting-room activation, unlinked account sessions and self-only intake access
- **Applies To:** API, Application, Domain, Infrastructure, JWT claims, authorization, persistence and audit

## Context

PI-2C1 established tenant-owned, short-lived and single-use waiting-room credentials. A new patient still needed a safe way to consume one credential, create an intake draft and return later without creating a canonical `Patient` before clinic review.

Reusing normal patient scope with an optional `patient_id` would weaken ownership semantics. Creating a provisional canonical Patient would introduce duplicate and unreviewed-data risk. Reusing staff identity or permissions would violate least privilege.

## Decision

BigSmile introduces a separate intake-only authentication boundary for unlinked portal accounts.

### 1. Atomic waiting-room activation

The public activation contract is:

```http
POST /api/patient-portal/intake-auth/activate
```

The request carries only:

- the raw waiting-room token in the body;
- tenant-scoped `LoginName`;
- password.

Tenant, optional Branch and purpose come from the persisted credential. The request does not select `TenantId`, `BranchId`, `PatientId`, account id or intake id.

One accepted transaction:

1. verifies the token hash using fixed-time comparison;
2. creates an unlinked `PatientPortalAccount`;
3. creates one `PatientIntake` with origin `NewPatientWaitingRoom`;
4. creates exactly 39 medical answers initialized to `Unknown`;
5. consumes the waiting-room credential;
6. appends link and authentication audit entries;
7. returns an intake-only access token.

Expired, revoked, consumed, replayed and unknown credentials use a generic failure contract. Concurrency or unique-constraint races allow at most one successful activation.

### 2. Dedicated session scope

The access scope is:

```text
patient_intake
```

Its bounded claims are:

```text
sub
 tenant_id
 intake_id
 scope = patient_intake
 session_version
 jti
```

The token must not contain:

- `patient_id`;
- staff roles or permissions;
- Branch claims;
- platform scope or override metadata.

The token uses the existing patient-portal issuer, audience, signing secret and short access-token lifetime, but a separate authorization policy, claims parser and session identity.

### 3. Server-side session revalidation

Every authenticated intake-only request revalidates:

- active Tenant;
- active, unlinked portal account;
- exact tenant/account/intake ownership;
- intake origin `NewPatientWaitingRoom`;
- intake status `Draft` and non-expired state;
- exact `SessionVersion`.

Logout increments `SessionVersion`, immediately invalidating previously issued access tokens. No refresh token is introduced in this pilot.

### 4. Recurrent access

The intake-only authentication surface is:

```http
POST /api/patient-portal/intake-auth/realms/{tenantSubdomain}/login
GET  /api/patient-portal/intake-auth/me
POST /api/patient-portal/intake-auth/logout
```

Login reuses the dedicated patient password hasher, tenant-subdomain realm, generic anti-enumeration responses, the approved five-attempt/fifteen-minute lockout and rate limiting. Unknown, inactive and locked account paths perform bounded dummy password verification to reduce timing-based account disclosure.

### 5. Intake ownership contracts

The existing route remains:

```text
/api/patient-portal/intake
```

- `scope=patient` resolves ownership from account plus linked Patient.
- `scope=patient_intake` resolves ownership from account plus exact `intake_id`.
- `GET` and `PUT` can operate only on the current draft.
- `POST` remains linked-patient-only; the new-patient draft is created only during activation.
- No route or body accepts arbitrary ownership identifiers.
- Existing optimistic concurrency, explicit-save, no-op and append-only revision semantics remain unchanged.

### 6. Audit and persistence

`PatientIntakeAuthenticationAuditEntry` records bounded security events such as activation, credential consumption, login success/failure, lockout and session revocation.

Audit entries are tenant-owned and append-only. They never contain raw tokens, token hashes, passwords, JWTs, authorization headers or medical answers.

The additive migration is:

```text
20260726031143_AddPatientIntakeOnlyAuthenticationBoundary
```

## Security boundaries

- Intake-only tokens cannot call linked-patient or staff APIs.
- Linked-patient tokens cannot satisfy intake-only auth routes.
- Staff tokens cannot satisfy patient/intake policies.
- No platform override is available.
- `TenantId` remains the primary security boundary; Branch remains optional operational context.
- Activation creates no canonical `Patient`, `ClinicalRecord` or `ClinicalMedicalAnswer`.
- Sensitive responses use `Cache-Control: no-store`.

## Consequences

### Positive

- New patients can begin a traceable draft without contaminating canonical records.
- Ownership remains explicit instead of making `patient_id` ambiguously optional.
- Replay, cross-tenant access and session revocation have dedicated controls.
- Existing patient authentication and staff authentication remain backward compatible.

### Trade-offs

- Every intake-only request performs server-side session validation.
- A browser reload still requires login because the patient frontend session remains memory-only.
- Recovery, refresh tokens and remote delivery remain deferred.
- The patient questionnaire UI is not part of this decision.

## Deferred

- TenantAdmin generate/copy/print/local-QR UI — PI-2C3.
- Angular patient intake form — PI-2D.
- Submit, clinic review, duplicate handling and canonical apply — PI-3.
- Canonical typed contact fields.
- Retention, privacy, incident-response and remote-access hardening — PI-4.

## Validation gate

This decision is accepted only with:

- repository-wide CI green;
- activation/replay/expiry/revocation/concurrency coverage;
- tenant/account/intake mismatch coverage;
- JWT claim-separation tests;
- session-version invalidation tests;
- append-only audit and tenant-filter tests;
- proof that canonical Patient and Clinical Records remain unchanged.
