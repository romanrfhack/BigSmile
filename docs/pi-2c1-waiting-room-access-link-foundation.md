# PI-2C1 — Waiting-Room Access Link Foundation

- **Status:** Accepted through PR #40
- **Phase:** 2.1 — Patient Intake and Portal Foundation
- **Parent:** PI-2C #35
- **Issue:** #36
- **Decision:** ADR 017
- **Migration:** `20260725204625_AddPatientIntakeAccessLinkFoundation`

## Objective

Provide a tenant-owned, short-lived and one-time waiting-room credential that authorized clinic staff can issue, list and revoke without creating a canonical Patient or exposing an anonymous consume path yet.

## Accepted boundary

### Domain

`PatientIntakeAccessLink`:

- belongs to one Tenant;
- may reference one active same-tenant Branch as operational context;
- contains no PatientId;
- stores only the SHA-256 token hash;
- expires after 30 minutes by default;
- supports explicit revocation;
- models later transactional consumption by one unlinked account and one waiting-room intake;
- uses `RowVersion` for concurrency;
- rejects revoke/consume after terminal or expired state.

`PatientIntakeAccessLinkAuditEntry`:

- is tenant-owned and append-only;
- records `Issued`, `Revoked` and the future `Consumed` action;
- records actor type/id, optional Branch, UTC timestamp and correlation id;
- never stores raw token, token hash, password, JWT or clinical content.

### Token handling

- token generation reuses the accepted cryptographic patient-portal generator;
- entropy is 256 random bits;
- transport representation is URL-safe Base64;
- persistence uses SHA-256 hash only;
- the raw bootstrap token appears only in the successful issue response;
- list and revoke contracts expose metadata only.

### Authorization

Permission:

```text
patientportal.intake.manage
```

Initial mapping:

| Role/scope | Access |
| --- | --- |
| `TenantAdmin` with resolved tenant | Allowed |
| `TenantUser` | Denied |
| `PlatformAdmin` | Denied |
| Platform override | Unavailable |

The Application service also rejects platform scope and active platform override.

### Staff API

```http
POST   /api/patient-intake-links
GET    /api/patient-intake-links
DELETE /api/patient-intake-links/{accessLinkId}
```

The route remains outside `/api/patient-portal/*`, so the existing bearer selector uses the staff scheme rather than the patient scheme.

- POST accepts only optional `BranchId`;
- POST returns raw `BootstrapToken` once and uses no-store headers;
- GET returns at most 100 metadata records, newest first;
- DELETE revokes a current active link;
- cross-tenant links are not visible;
- no public consume endpoint exists in PI-2C1.

### Multiple waiting patients

The model deliberately allows multiple active links per Tenant and Branch. One receptionist action must not invalidate the credential already handed to another patient.

Uniqueness is limited to:

- token hash;
- later consumed account;
- later consumed intake.

## Persistence

The additive migration creates:

```text
PatientIntakeAccessLinks
PatientIntakeAccessLinkAuditEntries
```

Guardrails include:

- global tenant filters on link and audit;
- centralized tenant write enforcement;
- append-only audit guard;
- unique token-hash index;
- indexes by Tenant, Branch, creation and expiry;
- `rowversion` concurrency;
- checks for expiry ordering, revocation state, consumption state, terminal-state exclusivity and temporal windows;
- restrictive relationships to Branch, future account/intake and audit history.

## Validation

Automated coverage includes:

- active same-tenant Branch validation;
- cross-tenant and inactive Branch rejection;
- 30-minute default and configuration bounds;
- cryptographic raw-token/hash separation;
- simultaneous active links;
- tenant-filtered issue/list/revoke;
- cross-tenant write denial;
- TenantUser/PlatformAdmin permission exclusion;
- platform scope and override rejection;
- append-only audit enforcement;
- no token/hash field in audit or list DTO;
- controller route outside the patient bearer prefix;
- no-store token response;
- EF indexes, checks, query filters and concurrency metadata;
- repository-wide backend/frontend CI.

## Deployment

The migration is additive. Safe order:

```text
backup / deployment verification
  -> apply 20260725204625_AddPatientIntakeAccessLinkFoundation
  -> deploy API
  -> reauthenticate TenantAdmin sessions to obtain the new permission claim
  -> smoke-test issue/list/revoke
```

No Patient, ClinicalRecord or existing auth data migration is required.

## Preserved non-goals

- anonymous token consumption;
- unlinked account creation;
- `patient_intake` JWT/policy/session;
- Angular staff handoff UI;
- Angular patient intake form;
- canonical Patient or ClinicalRecord creation/application;
- static reusable QR;
- external QR/email/SMS/WhatsApp provider;
- final retention/privacy policy.

## Result

PI-2C1 closes the staff-managed credential foundation. PI-2C2 is the next gate and may add transactional consume plus the isolated `patient_intake` session, but it must not include the staff UI or patient questionnaire UI.
