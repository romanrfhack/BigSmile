# ADR 014 — Patient Portal Authentication and Session Boundary

- **Status:** Accepted
- **Date:** 2026-07-25
- **Decision Type:** Public authentication, session security, abuse protection and assisted recovery
- **Scope:** Phase 2.1 / PI-1C — Patient activation, login and self-session boundary
- **Applies To:** API, Application, Domain, Infrastructure, tenant model, security and operations
- **Tracking:** epic #2; PI-1 #4; PI-1C #24; PR #29

## Context

ADR 006 separated patient-facing identity from staff identity. ADR 012 fixed the pilot access baseline, and ADR 013 completed staff-managed invitations with one-time token delivery, hash-at-rest and append-only audit.

PI-1C is the first public authentication surface. It must allow an invited existing patient to activate an account and return later without:

- reusing `User`, `UserTenantMembership`, staff roles or staff permissions;
- accepting an internal `TenantId` as a public selector;
- allowing a patient token to authenticate staff routes;
- exposing tenant, account, Patient or invitation existence through response differences;
- leaving revoked sessions valid until token expiry;
- adding intake or canonical clinical writes before their owning slices.

The client explicitly approved the concrete authentication/session baseline before implementation.

## Decision

### 1. Tenant login realm

Recurrent login uses the tenant's existing unique `Tenant.Subdomain` as the public realm:

```text
tenantSubdomain + LoginName + password
```

Rules:

- activation derives Tenant, Patient and invitation ownership from the one-time token;
- login never accepts an internal `TenantId`;
- realm and login normalization are server-side;
- inactive tenants, missing realms, unknown realms and unknown accounts return the same generic authentication failure;
- phone, email, date of birth and demographic data never prove ownership.

### 2. Dedicated patient password hashing

Patient credentials use a patient-specific abstraction over ASP.NET Core Identity password hashing:

- Identity V3 encoded format;
- PBKDF2 with an explicit initial iteration count of `100000`;
- minimum password length `12` and maximum `128` by default;
- configuration is bounded and validated;
- successful verification can return `SuccessRehashNeeded` and upgrade the stored hash;
- unknown accounts execute a dummy password verification to reduce timing differences;
- plaintext passwords are never stored, audited or logged.

The existing staff password hasher and staff credential format remain unchanged.

### 3. Separate patient bearer scheme

Patient access uses a dedicated bearer scheme with:

- distinct signing secret;
- distinct issuer;
- distinct audience;
- default access-token lifetime of 60 minutes;
- no refresh token in PI-1C.

Required patient claims:

- `sub` / portal account id;
- `tenant_id`;
- `patient_id`;
- `scope = patient`;
- `session_version`;
- `jti`.

Forbidden patient claims:

- staff role;
- staff permission;
- branch id/name;
- platform scope or platform override.

A policy-scheme selector routes `/api/patient-portal/*` to the patient bearer scheme and preserves the staff bearer scheme for existing APIs. Because the secrets, issuer/audience and route-selected schemes are separate, a patient token cannot authenticate a staff endpoint and a staff token cannot authenticate a patient self endpoint.

### 4. Single-use activation

Activation:

- accepts the raw invitation token only in the request body;
- computes SHA-256 and verifies fixed-length bytes with `CryptographicOperations.FixedTimeEquals`;
- loads the invitation independently of tenant request input;
- rejects unknown, expired, revoked, consumed or otherwise invalid invitations with one generic response;
- requires active Tenant and Patient records;
- creates a new linked account or completes an explicitly started recovery;
- consumes the invitation and writes append-only audit in one persistence transaction;
- uses rowversion and relational constraints so concurrent attempts produce at most one success;
- returns a generic conflict for tenant-scoped `LoginName` collision or concurrency conflict.

### 5. Login, lockout and session invalidation

Login behavior:

- invalid realm, login, password, inactive account, inactive Patient and lockout all return the same generic response;
- failed attempts are persisted and audited;
- the default lockout remains five failed attempts for 15 minutes;
- a successful login clears failure/lockout state;
- stronger password-hash settings can rehash after successful verification.

Every patient-authenticated request validates server-side:

- account id, tenant id and linked Patient id;
- account active state;
- Patient active state;
- Tenant active state;
- exact `SessionVersion` match.

Logout and assisted recovery increment `SessionVersion`, immediately invalidating previously issued access tokens even before their JWT expiration.

### 6. Public abuse controls

Named ASP.NET Core fixed-window rate-limit policies are applied:

- activation: five requests per minute per normalized remote IP by default;
- login: ten requests per minute per normalized remote IP plus tenant realm by default;
- no request queue;
- generic `429` response;
- passwords and raw tokens are not used as partition keys and are not logged.

Limits and window are configurable within bounded ranges.

Deployment behind a reverse proxy must configure trusted forwarded headers before relying on client-IP partitioning. Untrusted forwarded headers must not be accepted implicitly.

### 7. Assisted recovery

Recovery uses a new staff permission:

```text
patientportal.account.recover
```

Initial mapping:

- `TenantAdmin`: allowed only inside its resolved tenant;
- `TenantUser`: denied;
- `PlatformAdmin`: denied;
- platform override: unavailable.

Recovery is one transaction that:

1. deactivates the account and increments `SessionVersion`;
2. supersedes outstanding activation invitations;
3. creates a new one-time invitation;
4. records invitation and authentication audit;
5. returns the new raw token once with no-store response headers.

There is no public forgot-password or demographic recovery endpoint.

### 8. Append-only authentication audit

`PatientPortalAuthenticationAuditEntry` is tenant-owned and records relevant lifecycle evidence, including:

- account activation or recovery completion;
- invitation consumption;
- login success and failure;
- lockout;
- session revocation;
- assisted recovery start.

Each entry records Tenant, Patient, portal account, optional invitation, actor type/id, UTC timestamp and correlation id. Normal modification and deletion are blocked centrally by `AppDbContext`. Tokens, hashes, passwords and authorization headers are excluded.

## API boundary

Public/patient endpoints:

```http
POST /api/patient-portal/auth/activate
POST /api/patient-portal/auth/realms/{tenantSubdomain}/login
GET  /api/patient-portal/auth/me
POST /api/patient-portal/auth/logout
```

Staff recovery endpoint:

```http
POST /api/patients/{patientId}/portal-account/recovery
```

PI-1C does not expose Patient, Clinical, Odontogram, Treatment, Billing, Document, Scheduling or Dashboard data to the patient.

## Alternatives considered

### Reuse staff JWT and authorization

**Rejected.** It mixes trust boundaries and creates a direct escalation path to tenant-wide permissions.

### Use a globally unique login name

**Rejected.** ADR 012 explicitly defines patient identity as tenant-owned; global uniqueness creates unnecessary cross-tenant coupling and disclosure.

### Use the invitation token in the URL

**Rejected.** URLs are commonly retained in browser history, reverse-proxy logs, analytics and referrers.

### Refresh tokens in the pilot

**Deferred.** They add rotation, storage, reuse detection and revocation complexity. The pilot uses short-lived access tokens plus server-side `SessionVersion` validation.

### Public email/SMS password recovery

**Deferred.** No trusted delivery provider or production recovery policy is accepted. Staff-assisted recovery is the bounded pilot path.

### Trust JWT until expiration

**Rejected.** Recovery and deactivation must revoke access immediately; every request therefore validates `SessionVersion` and active state.

## Consequences

### Positive

- patient and staff identities remain cryptographically and semantically separated;
- invitation replay and concurrent consumption are bounded;
- account recovery invalidates sessions immediately;
- tenant/account enumeration is reduced through generic responses and dummy verification;
- the pilot requires no external messaging provider;
- existing staff auth contracts remain backward compatible.

### Trade-offs

- every authenticated patient request performs a database-backed session validation;
- no refresh token means the patient must log in again after access-token expiry;
- reception/TenantAdmin remains involved in recovery;
- reverse-proxy IP handling requires an explicit deployment configuration;
- the patient-facing Angular experience is governed separately by ADR 015 and remains memory-only without refresh tokens.

## Non-goals

- questionnaire or intake data;
- waiting-room new-patient link;
- Angular patient auth UI;
- refresh tokens;
- remote self-service recovery;
- dependents or multiple patients per account;
- patient access to canonical clinical or commercial modules;
- platform support override.

## Exit condition

PI-1C is accepted only after:

- additive migration is committed;
- build, architecture validation, backend unit/integration tests and frontend regression tests are green;
- staff auth regression remains green;
- security-focused activation, replay, tenant realm, lockout, recovery and session-revocation coverage exists;
- canonical STATE and project documentation are reconciled.

PI-1D is accepted through ADR 015 and PR #30 with the separate Angular patient-auth area, token-boundary tests and assisted-recovery runbook. PI-1 is complete; PI-2 remains separately gated.
