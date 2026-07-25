# ADR 012 — Patient Portal Access Baseline and Phase 2.1 Opening

- **Status:** Accepted
- **Date:** 2026-07-24
- **Decision Type:** Authentication/session baseline, security policy, roadmap opening
- **Scope:** Phase 2.1 / PI-1 — Patient Intake and Portal access foundation
- **Applies To:** Domain, Application, API, Infrastructure, frontend, security, tenant model and roadmap
- **Tracking:** epic #2; PI-1 #4; PI-1A #22; PI-1B #23; PI-1C #24; PI-1D #25

## Context

ADR 006 established that patient-facing identity must remain separate from staff `User`, `UserTenantMembership`, roles and tenant-wide permissions. ADR 011 formally accepted the initial operational MVP, satisfying the normal gate for Phase 2.1.

The client explicitly authorized the concrete access baseline needed to open PI-1:

- single-use activation followed by password-based access;
- tenant-scoped `LoginName`;
- 24-hour existing-patient invitation;
- 30-minute waiting-room link;
- reception-assisted delivery;
- five failed attempts followed by a 15-minute lockout;
- assisted recovery through session revocation and invitation reissue.

These choices affect authentication, session invalidation, persistence and public security. They must be recorded before runtime endpoints are introduced.

## Decision

Phase 2.1 is explicitly opened. PI-1 becomes the active implementation slice and must proceed through PI-1A to PI-1D in order.

### 1. Activation and recurrent access

- The bootstrap credential is a cryptographically strong, single-use, expirable and revocable invitation or waiting-room link.
- An existing-patient invitation is server-bound to `TenantId + PatientId`.
- During activation, the patient chooses a `LoginName` and password.
- Subsequent access uses the patient's own credential.
- Recurrent magic links are not part of the pilot baseline.

### 2. Tenant-scoped login identifier

- `LoginName` is unique inside a tenant, not globally.
- It may contain an email address or an approved username.
- Normalization and uniqueness are enforced server-side and in SQL Server.
- Phone number, date of birth, email/contact data and patient demographics do not prove ownership and cannot be used to claim a record publicly.
- No public patient search or account-claim endpoint is allowed.

### 3. Time-to-live defaults

Initial defaults are configurable and do not become hardcoded product limits:

- existing-patient invitation: 24 hours;
- waiting-room link: 30 minutes.

The 24-hour invitation is implemented inside PI-1. The waiting-room link runtime remains deferred until the slice that creates `PatientIntake`; this ADR fixes its default and security direction only.

### 4. Pilot delivery

- Reception generates and explicitly displays, prints or copies the QR/link.
- PI-1 does not add automatic email, SMS or WhatsApp delivery.
- Raw tokens are returned only at creation time and are never persisted or logged.
- Only token hashes are stored.

### 5. Lockout and assisted recovery

Initial configurable defaults:

- maximum failed attempts: 5;
- lockout duration: 15 minutes.

Pilot recovery is staff-assisted:

1. authorized staff revokes existing patient sessions;
2. existing activation artifacts are revoked where applicable;
3. staff issues a new single-use invitation;
4. the patient defines a replacement password during reactivation/recovery.

There is no public recovery based only on demographic or unverified contact data.

### 6. Separate identity and authorization boundary

A `PatientPortalAccount`:

- is tenant-owned;
- links to zero or one canonical Patient during the bounded workflow and at most one after linking;
- never becomes a staff `User` or `TenantUser`;
- never receives staff permissions such as `patient.*`, `clinical.*`, scheduling, odontogram, treatment, billing, document, dashboard or platform permissions;
- uses a dedicated patient auth scheme, audience, scope and self-only policies in PI-1C;
- has no platform override path.

`TenantId`, portal account id and linked Patient id are derived from verified server-issued context. Request bodies and arbitrary route values are not authority sources.

### 7. Sequential PI-1 implementation

#### PI-1A — #22

- account and invitation domain models;
- tenant ownership and patient-link invariants;
- normalized tenant-scoped login key;
- password-hash storage, lockout metadata and session version;
- invitation hash/expiry/revoke/consume metadata;
- rowversion concurrency;
- EF configurations, indexes, filters, write enforcement, migration and tests;
- no endpoint.

#### PI-1B — #23

- authorized staff issue/revoke use cases;
- cryptographic token generation and hashing;
- 24-hour default through configuration;
- append-only issue/revoke audit;
- no automatic delivery.

#### PI-1C — #24

- single-use activation;
- dedicated versioned password hashing;
- patient JWT/audience/scope/claims;
- login/current session;
- rate limiting, anti-enumeration, lockout and session invalidation;
- concurrent-consumption, replay, expiry, revocation, IDOR and cross-tenant tests.

#### PI-1D — #25

- Angular patient auth area separate from staff shell;
- activation/login/session states;
- expired/revoked/consumed/locked generic UX;
- e2e and recovery runbook;
- formal PI-1 closure.

No later sub-slice may bypass an earlier exit gate.

## Security requirements

- Token material must have sufficient entropy and be compared through a fixed-time hash path.
- Passwords are never stored or audited in plaintext.
- A dedicated versioned password format is required before patient login is exposed.
- Single-use consumption must be transaction-safe and protected through concurrency control.
- Public responses must be generic enough to prevent tenant, patient, account and invitation enumeration.
- Patient policies must never enable platform override.
- Staff authentication and `/api/auth` remain backward compatible.
- Every security lifecycle action must become traceable before its runtime path is accepted.

## Alternatives considered

### Recurrent magic link

**Rejected for the pilot.** It requires a trusted delivery provider and makes every return visit dependent on delivery availability. It may be reconsidered through a later ADR.

### Email-only identifier

**Rejected as a requirement.** Existing patient records may lack verified email and the clinic explicitly needs to complement incomplete data.

### Phone or date of birth as identity proof

**Rejected.** Both are guessable, shared, recycled or incomplete and create account-takeover/enumeration risk.

### Global login-name uniqueness

**Rejected.** The patient identity is tenant-owned. Global uniqueness would create unnecessary cross-tenant coupling and information leakage.

### Public password recovery immediately

**Rejected for PI-1.** A secure remote recovery channel/provider and privacy policy are not yet accepted. Assisted recovery is safer for the pilot.

### One large PI-1 implementation

**Rejected.** The boundary is high risk and must remain independently reviewable through PI-1A to PI-1D.

## Consequences

### Positive

- Phase 2.1 can start without hidden authentication assumptions.
- Patient identity remains least-privilege and tenant-scoped.
- The pilot does not depend on an external messaging provider.
- Session revocation and assisted recovery are designed before public access.
- The work remains small, sequential and traceable.

### Trade-offs

- Reception must deliver initial/recovery links manually.
- Patients must remember a password after activation.
- Remote self-service recovery is deferred.
- The waiting-room link runtime is not completed by PI-1A.

### Deferred decisions

- patient password hashing/JWT/session baseline — resolved in ADR 014 / PI-1C;
- staff permission for invitation management — resolved in ADR 013 / PI-1B;
- waiting-room token entity/runtime and draft ownership — PI-2;
- remote recovery provider, retention and privacy policy — PI-4;
- frontend patient session boundary — resolved in ADR 015 / PI-1D;
- full patient portal access to records/documents/commercial modules — Phase 4.

## Exit condition

This ADR is implemented through PI-1A to PI-1D. PI-1 is complete, but Phase 2.1 remains incomplete until PI-2 through PI-4 satisfy their own exit gates under ADR 006.
