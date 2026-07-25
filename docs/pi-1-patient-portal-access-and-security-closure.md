# PI-1 — Patient Portal Access and Invitation Foundation Closure

- **Status:** Accepted with PI-1D merge
- **Phase:** Phase 2.1 — Patient Intake and Portal Foundation
- **Slices:** PI-1A, PI-1B, PI-1C and PI-1D
- **Tracking:** issues #4, #22, #23, #24 and #25
- **Decisions:** ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015

## 1. Objective

Establish a least-privilege patient identity and browser-session boundary that allows an existing patient to:

1. receive a staff-issued one-time invitation;
2. activate a tenant-scoped account;
3. log in through the clinic realm;
4. obtain a self-only session;
5. log out or recover access with staff assistance;

without becoming a staff user, receiving tenant-wide permissions or writing intake/clinical information yet.

## 2. Accepted slice evidence

### PI-1A — Account and invitation domain/persistence

Accepted through PR #26.

- `PatientPortalAccount` and `PatientPortalInvitation` are tenant-owned;
- `LoginName` uniqueness is tenant-scoped;
- one portal account links to at most one Patient in Phase 2.1;
- invitation storage is hash-only;
- lockout and `SessionVersion` metadata exist;
- `RowVersion`, indexes, query filters and centralized write enforcement are present;
- additive migration and automated tests are included;
- no endpoint was exposed by PI-1A.

### PI-1B — Staff invitation lifecycle

Accepted through PR #28 and ADR 013.

- `patientportal.invitation.manage` belongs initially only to `TenantAdmin`;
- `TenantUser`, `PlatformAdmin` and platform override are denied;
- issue/list/revoke operations are tenant-scoped;
- 256-bit Base64URL token is returned only at issuance;
- only SHA-256 hash is persisted;
- 24-hour lifetime is configurable;
- a replacement supersedes outstanding invitations deterministically;
- invitation issue/revoke/supersede audit is tenant-owned and append-only.

### PI-1C — Activation, login and self-session backend

Accepted through PR #29 and ADR 014.

- activation token is submitted in the body and verified through a fixed-length comparison path;
- activation and recovery consumption are transactional;
- replay, expiry and revocation are rejected;
- patient passwords use a separate Identity V3/PBKDF2 hasher with explicit work factor and rehash support;
- login realm is the active `Tenant.Subdomain`, not an internal Tenant id;
- patient JWT uses separate issuer, audience, secret and bearer scheme;
- patient claims exclude staff role, permissions, branch and platform scope;
- every patient-authenticated request validates Tenant, Patient, account and `SessionVersion` server-side;
- lockout remains 5 attempts / 15 minutes by default;
- activation/login rate limits and generic public errors are enabled;
- `patientportal.account.recover` belongs initially only to `TenantAdmin`;
- auth/recovery/session audit is tenant-owned and append-only.

### PI-1D — Angular patient auth surface and operational closure

Accepted through PR #30 and ADR 015.

- patient routes exist outside the staff shell;
- opening the patient shell clears any in-memory staff session;
- patient token/session state remains in memory only;
- staff token is never sent to patient APIs;
- patient token is never sent to staff APIs;
- activation token is delivered in the URL fragment, read once and removed immediately;
- activation, tenant-realm login, current session and logout are implemented;
- patient guards enforce the current realm without treating it as ownership proof;
- public UX remains generic and non-enumerating;
- patient home exposes no clinical or commercial module;
- reception-assisted activation/recovery runbook is documented;
- frontend tests cover state, token separation, routing, fragment parsing and shell separation.

## 3. Accepted API boundary

### Staff

```http
GET    /api/patients/{patientId}/portal-invitations
POST   /api/patients/{patientId}/portal-invitations
DELETE /api/patients/{patientId}/portal-invitations/{invitationId}
POST   /api/patients/{patientId}/portal-account/recovery
```

### Patient/public

```http
POST /api/patient-portal/auth/activate
POST /api/patient-portal/auth/realms/{tenantSubdomain}/login
GET  /api/patient-portal/auth/me
POST /api/patient-portal/auth/logout
```

No patient-facing route accepts arbitrary `TenantId`, account id or Patient id as authority.

## 4. Accepted Angular boundary

```text
/patient-portal/activate
/patient-portal/:tenantSubdomain/login
/patient-portal/:tenantSubdomain/home
```

The patient area owns:

- models;
- API data-access;
- facade;
- in-memory session store;
- patient-only interceptor;
- patient-only guards;
- shell, activation, login and home components.

It does not reuse the staff `AuthService`, staff guard or staff shell for patient authorization.

## 5. Security acceptance matrix

| Threat / requirement | Accepted control |
| --- | --- |
| Cross-tenant account access | Tenant-owned records, server-derived claims/context, query filters and session validation |
| Public Patient enumeration | No public Patient search/claim; generic activation/login errors |
| Invitation replay | single-use domain invariant, transaction, rowversion and uniqueness guards |
| Raw-token recovery from DB | hash-only invitation persistence |
| Token leakage through URL logs | fragment delivery and immediate browser cleanup |
| Staff token sent to patient API | staff interceptor excludes `/api/patient-portal/*` |
| Patient token sent to staff API | patient interceptor is patient-API-only |
| Patient JWT used as staff JWT | separate scheme, issuer, audience, secret and claim shape |
| Session remains valid after logout/recovery | `SessionVersion` increment and validation on every request |
| Password brute force | configurable lockout plus login rate limiting |
| Token guessing | 256-bit random token plus activation rate limiting |
| Account/tenant timing differences | generic responses and dummy password verification for unknown identities |
| Browser credential persistence | patient token/session memory-only |
| Platform cross-tenant support path | no platform override for patient policies or portal credential operations |
| Audit tampering | append-only persistence guards for security/auth audit entries |

## 6. Compatibility

PI-1 preserves:

- `/api/auth` and existing staff JWT contracts;
- `User`, `UserTenantMembership`, roles and staff permissions;
- accepted Patients, Scheduling, Clinical, Odontogram, Treatments, Billing, Documents and Dashboard contracts;
- Tenant/Branch ownership semantics;
- Angular staff routes and in-memory staff session baseline;
- canonical Patient and ClinicalRecord data.

No PI-1 endpoint writes questionnaire answers, demographics, contact proposals or clinical data.

## 7. Deployment requirements

- apply PI-1A, PI-1B and PI-1C additive migrations;
- configure a patient JWT secret/issuer/audience distinct from staff values;
- configure trusted forwarded headers behind the production reverse proxy;
- deploy API before the Angular patient routes;
- require affected `TenantAdmin` users to sign in again after new permission claims are deployed;
- run the assisted activation/recovery smoke test.

## 8. Deferred scope

PI-1 does not include:

- waiting-room new-patient link runtime;
- demographic/contact intake proposals;
- medical-questionnaire draft capture;
- submission, clinic review or canonical apply;
- refresh tokens;
- remote recovery provider;
- family/dependent accounts;
- patient browsing of professional clinical records, documents, odontogram, treatments, billing, scheduling or dashboard;
- production retention/privacy/consent policy;
- staff-visible full audit timeline.

## 9. Exit decision

PI-1 is complete when PR #30 is merged with repository-wide CI green and canonical documentation aligned.

The next bounded slice is **PI-2 — Intake Draft and Self-Service Capture (#5)**. Opening PI-2 requires explicit decisions for editable proposal fields, contact/phone ownership, waiting-room link lifecycle, draft expiry and save behavior. PI-2 remains prohibited from applying changes directly to canonical `Patient` or `ClinicalRecord` data.
