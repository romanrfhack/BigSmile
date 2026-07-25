# Patient Intake and Portal General Plan

- **Status:** In progress; PI-1, PI-2A and PI-2B completed; PI-2C active
- **Roadmap placement:** Phase 2.1 — Patient Intake and Portal Foundation
- **Start gate:** Satisfied through MVP acceptance and explicit client authorization on 2026-07-24
- **Architecture decisions:** ADR 006 and ADR 012–017
- **Canonical ADR:** `docs/decisions/006-patient-intake-and-portal-foundation.md`
- **Parent tracking:** GitHub issue #2
- **Implementation tracking:** issues #4, #5, #6 and #7
- **Last updated:** 2026-07-25

## 1. Purpose

Keep the patient-facing intake requirement visible and actionable without mixing planned work with completed functionality.

The client wants:

1. New patients in the waiting room to register and begin completing their information.
2. Existing patients to activate access and complement information the clinic has not captured.
3. Patient-originated changes to remain traceable.
4. Clinic staff to review changes before canonical `Patient` or `ClinicalRecord` data is modified.

The plan decomposes the capability into sequential, testable slices while preserving staff authorization, tenant isolation, clinical provenance and roadmap order.

## 2. Roadmap decision

The bounded capability is placed in **Phase 2.1 — Patient Intake and Portal Foundation**, inside Phase 2 — Modern Operations, after the initial operational MVP.

Current accepted roadmap frontier:

- Release 4 — Odontogram: completed.
- Release 5 — Treatments and Quotes: completed.
- Release 6 — Billing: completed through Release 6.1.
- Release 7 — Documents and Dashboard: completed through Release 7.1 and 7.2.
- Initial operational MVP: formally accepted under ADR 011.
- Phase 2.1: active; PI-1, PI-2A and PI-2B are completed; PI-2C is active.

This placement is deliberate:

- intake and digital patient updates are modern operational capabilities;
- the workflow depends on stable Patients and Clinical Records;
- clinic review/application depends on canonical operational modules remaining authoritative;
- a public authentication boundary is high risk;
- the client confirmed the capability is desired but did not ask it to displace the current roadmap.

The broader patient portal remains deferred to Phase 4. Phase 2.1 does not include professional clinical-record browsing, online booking, billing, documents, treatments, odontogram access or automated messaging.

## 3. Status summary

| Item | Status | Evidence / tracking |
| --- | --- | --- |
| Fixed 39-question medical questionnaire backend | Completed as Release 3.5 | Clinical Records code/tests |
| Internal staff questionnaire UI | Completed | Clinical Records frontend |
| Familiar `Sí / No / Sin respuesta` redesign | Completed and merged | PR #1 |
| Release 4 Odontogram foundation | Completed | ADR 007 / Release 4 audit |
| Release 5 Treatments and Quotes foundation | Completed | ADR 008 / Release 5 audit |
| Release 6 Billing foundation | Completed | ADR 009 / Release 6 audit |
| Release 7 Documents/Dashboard foundations | Completed; MVP accepted | ADR 010/011 / Release 7 audit |
| Patient-facing architecture decision | Accepted and merged | ADR 006 / PR #3 |
| Parent product backlog | Open | Issue #2 |
| PI-1 access/invitations | Completed through PI-1A to PI-1D | Issues #4 and #22–#25 / PRs #26, #28, #29 and #30 |
| PI-2 intake draft | Active; PI-2A and PI-2B completed; PI-2C active | Issue #5 / #31 / #33 / #35 |
| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |
| PI-4 audit/hardening | Planned; not implemented | Issue #7 |
| Patient-facing backend/API/database | Access/auth, intake persistence and linked-patient self-only API implemented | PRs #26, #28, #29, #32 and #34 |
| Patient-facing frontend | Bounded activation/login/session implemented | PR #30 / ADR 015 |
| Patient-facing intake | Existing-patient API implemented; waiting-room bootstrap and patient capture UI pending | PI-2B / PI-2C / PI-2D |

## 4. Scope boundary

### Included in Phase 2.1

- Separate patient identity and session boundary.
- Existing-patient activation through staff-issued invitation.
- Waiting-room registration through short-lived clinic QR/link.
- Self-only intake draft capture.
- Approved demographic/contact proposal fields.
- Existing fixed medical-question catalog.
- Explicit saves and immutable submitted revisions.
- Clinic review, duplicate handling, link/create decision and canonical apply.
- Append-only domain audit and staff-visible traceability.
- Security hardening required for public patient-facing access.

### Not included

- Patient access to diagnoses, notes, encounters, vitals, odontogram, treatment plans, quotes, billing, payments or documents.
- Online booking.
- Automated email, SMS or WhatsApp delivery.
- Family/dependent accounts.
- Multiple patients per portal account.
- Advanced consent/electronic signature.
- Configurable form builder.
- Automatic interpretation of questionnaire responses.
- Automatic allergy/clinical-alert synchronization without separate decision.

## 5. Architectural boundaries

### 5.1 Staff identity remains unchanged

Patients must not become `TenantUser` and must not receive staff permissions. Existing `/api/auth`, staff JWTs, `UserTenantMembership`, roles and permissions remain backward compatible.

### 5.2 Patient access is self-scoped

Every patient-facing read/write derives tenant, portal account and linked patient ownership from verified server-issued context. Request bodies and route identifiers are not authority sources for selecting another patient.

### 5.3 Canonical data remains clinic-controlled

Anonymous or patient-authenticated endpoints never write directly to canonical `Patient` or `ClinicalRecord`. Patient input follows:

```text
Draft -> Submitted -> Reviewed -> Applied
                           \-> Rejected
```

### 5.4 Audit is domain-specific and append-only

Record effective saves and lifecycle transitions, not every keystroke. Exclude passwords, raw tokens, authorization headers and unnecessary sensitive source metadata.

### 5.5 Tenant is the primary boundary

Account, invitation, intake, revision and audit entities are tenant-owned. Branch is optional operational context only when a concrete use case proves it is required.

### 5.6 No platform override

Patient-facing policies do not inherit the normal privileged platform override path. Support actions require explicit staff/platform use cases with audit.

## 6. Planned domain model

Names are accepted as the initial direction. Exact properties/invariants are finalized in the owning slice rather than through one large schema change.

### `PatientPortalAccount`

Purpose: patient-facing identity separate from staff users.

Initial invariants:
- exactly one tenant;
- zero or one patient while activation/intake is pending, at most one after activation in Phase 2.1;
- unique normalized login identifier according to final auth strategy;
- explicit active/locked state;
- no staff roles or tenant-wide permissions;
- security metadata is not clinical data.

### `PatientPortalInvitation`

Purpose: secure activation for an existing patient.

Initial invariants:
- tenant-owned;
- server-bound to patient and purpose;
- only token hash persisted;
- short expiry;
- single use;
- revocable;
- transactional consume;
- no public patient lookup.

### `PatientIntakeLink` or equivalent waiting-room token

Purpose: create a new-patient intake draft inside a known tenant.

Initial invariants:
- issued by authenticated clinic staff;
- tenant/purpose/expiry bound server-side;
- no canonical Patient/ClinicalRecord creation on public access;
- revocable and replay-protected according to PI-1/PI-2 design.

### `PatientIntake`

Purpose: mutable self-only working draft.

Initial invariants:
- tenant-owned;
- linked to portal account/session;
- optionally linked to a canonical patient for existing-patient flows;
- explicit status and optimistic concurrency token;
- contains only approved proposal fields and fixed questionnaire answers;
- does not own professional notes, diagnoses, encounters or alerts.

### `PatientIntakeRevision`

Purpose: immutable snapshot/diff for effective saves and submissions.

Initial invariants:
- tenant-owned;
- monotonically ordered per intake;
- immutable through normal application flow;
- records actor, timestamp, changed field identifiers and correlation id;
- submitted revision remains unchanged after submission.

### `PatientIntakeAuditEntry`

Purpose: append-only lifecycle/security evidence.

Actions include:
- invitation created/revoked/consumed;
- account activated/locked/recovered;
- draft saved;
- submitted;
- reviewed;
- linked/created;
- applied;
- rejected;
- session/invitation revoked.

## 7. Workflow

### Existing patient

```text
Staff selects Patient
  -> creates invitation
  -> patient consumes invitation
  -> account activated and linked
  -> patient saves intake draft
  -> patient submits immutable revision
  -> staff reviews/conflict-checks
  -> staff applies or rejects
```

### New patient in waiting room

```text
Staff creates short-lived tenant QR/link
  -> patient starts intake draft
  -> patient submits immutable revision
  -> staff reviews duplicate candidates
  -> staff links existing Patient or creates new Patient
  -> staff applies accepted data
```

No public endpoint searches or claims patients by name, date of birth, phone or email.

## 8. Sequential implementation slices

### PI-1 — Access and Invitation Foundation — issue #4

Scope:
- account/invitation entities;
- persistence/indexes/migration/tenant filters;
- staff issue/revoke endpoints;
- patient activation/login/current-session endpoints;
- dedicated JWT audience/scope/claims/policies;
- rate limiting and anti-enumeration;
- replay/expiry/revocation/concurrency tests;
- separate Angular route/shell/guard/interceptor boundary;
- memory-only patient session and fragment cleanup;
- assisted activation/recovery runbook;
- no questionnaire/intake writes.

Exit gate:
- patient identity cannot obtain staff permissions;
- ownership comes from verified context;
- invitation cannot be replayed or cross tenants;
- staff and patient bearer tokens never cross API boundaries;
- CI green.

### PI-2 — Intake Draft and Self-Service Capture — issue #5

Approved sequence under ADR 016:

1. **PI-2A — Intake Domain and Persistence — #31 — completed through PR #32**
   - tenant-owned `PatientIntake`, fixed answer rows and immutable revisions;
   - linked/unlinked origin, optional same-tenant Branch context and Patient baseline;
   - approved proposal fields, 39 exact keys, `Draft / Expired`, 30-day expiry and `RowVersion`;
   - effective save creates one revision; identical save creates none;
   - additive migration, filters/write enforcement and tests;
   - no endpoint or canonical write.
2. **PI-2B — Existing-Patient Self-Service Draft — #33 / PR #34 — completed**
   - explicit id-less create/get/save from the linked authenticated account;
   - GET has no side effects; all sensitive responses are no-store;
   - no-op/effective-save, expiry and optimistic concurrency contracts.
3. **PI-2C — Waiting-Room Link and Intake-Only Account Scope — #35 — active**
   - PI-2C1 #36: credential/persistence and TenantAdmin management API;
   - PI-2C2 #37: transactional activation and `patient_intake` session;
   - PI-2C3 #38: staff generate/copy/print/local-QR UI and closure.
4. **PI-2D — Angular Patient Intake Capture and PI-2 Closure — pending**
   - mobile/tablet sections, explicit save, dirty/saved/conflict/expired states and fixed questionnaire UX.

PI-2 exit gate:
- no canonical Patient/ClinicalRecord changes;
- identical save creates no revision;
- unknown/duplicate question keys rejected;
- self-scope/cross-tenant/concurrency tests and frontend states green.

### PI-3 — Submit, Review and Canonical Apply — issue #6

Scope:
- immutable submission;
- staff review worklist;
- duplicate/link/create decisions;
- explicit application to canonical Patient/ClinicalRecord;
- provenance and conflict handling;
- append-only transition audit.

Exit gate:
- patient/public endpoint cannot apply canonical data;
- staff authorization explicit;
- conflicts never silently overwrite data;
- application is idempotent/transaction-safe to accepted extent;
- cross-tenant/duplicate/concurrency/partial-failure tests green.

### PI-4 — Audit Visibility and Security Hardening — issue #7

Scope:
- staff-visible audit timeline;
- lockout/recovery/session revocation;
- invitation/session revocation;
- additional abuse telemetry/rate limits;
- privacy/retention controls;
- e2e and runbook.

Exit gate:
- full immutable history visible to authorized staff;
- recovery does not enumerate patients/tenants;
- sessions/invitations revoke predictably;
- public security/e2e/operational validation green.

## 9. Cross-cutting validation matrix

Every PI slice must consider:

| Dimension | Required evidence |
| --- | --- |
| Tenant isolation | cross-tenant read/write denial |
| IDOR | no arbitrary patient/account selection |
| Authorization | patient self-only; staff actions explicit |
| Token security | hashing, expiry, single-use, revocation, replay tests |
| Data integrity | concurrency/idempotency/transaction behavior |
| Audit | immutable effective-change/lifecycle entries |
| API | generic public errors and bounded contracts |
| Frontend | guards, ownership-safe routing, loading/error/expired states |
| Compatibility | existing staff auth and clinical flows unchanged |
| Operations | CI, migration/deploy/rollback and runbook evidence |

## 10. Decisions and remaining gates

### Approved and completed for PI-1 under ADR 012, ADR 013, ADR 014 and ADR 015

- Invitation management uses `patientportal.invitation.manage` for `TenantAdmin` only; no `TenantUser`, `PlatformAdmin` or platform override.
- Invitation issuance returns the raw token once, stores only its SHA-256 hash, supersedes outstanding invitations and records append-only audit.
- Activation single-use followed by password access.
- Tenant-scoped `LoginName`; email or username allowed, but phone/DOB/contact do not prove ownership.
- Existing-patient invitation TTL: 24 hours, configurable.
- Waiting-room link TTL: 30 minutes, configurable; runtime deferred to the intake slice.
- Pilot delivery by reception without external provider.
- Lockout baseline: 5 attempts / 15 minutes, configurable.
- Assisted recovery through session revocation and invitation reissue.
- Patient recurrent login uses `Tenant.Subdomain + LoginName + password`; internal `TenantId` is never a public selector.
- Patient password hashes use a dedicated versioned Identity V3/PBKDF2 format with explicit work factor and rehash support.
- Patient JWT uses a separate scheme/secret/issuer/audience, 60-minute access token, no refresh token and server-side `SessionVersion` validation.
- Activation/login use generic responses and configurable fixed-window rate limits.
- Assisted recovery uses `patientportal.account.recover` for `TenantAdmin` only, without platform override.
- Angular patient routes are outside the staff shell; activation token uses a URL fragment and is removed immediately.
- Patient access token/session remain in memory only; staff and patient interceptors never cross API boundaries.
- PI-1 operational recovery follows `docs/patient-portal-assisted-recovery-runbook.md`.

### Approved PI-2 baseline under ADR 016

- Proposed identity/demographic/contact/responsible-party fields plus `ReasonForVisit`.
- `Preferred / Mobile / Home / Work` phones remain intake proposals; canonical typed contacts are a prerequisite before PI-3 apply.
- Exact existing 39-key questionnaire, `Unknown / Yes / No`, optional 500-character details.
- One active draft per account; 30-day sliding expiry after effective saves; soft expiry only.
- Explicit save; no autosave; identical save creates no revision and does not extend expiry.
- Waiting-room link single-use/hash-only/30 minutes; `patientportal.intake.manage` is TenantAdmin-only with no platform override.
- PI-2C uses an unlinked account with `scope=patient_intake`, `intake_id` and no `patient_id`; QR generation remains local and the raw link stays memory-only.
- Mandatory sequence PI-2A → PI-2B → PI-2C → PI-2D.

### Before PI-3

- Staff roles/permissions allowed to review/apply.
- Duplicate-candidate criteria and reviewer-visible data.
- Field-level conflict UX.
- Whether staff apply may explicitly create a missing ClinicalRecord.
- Provenance retained in canonical models versus intake history only.

### Before PI-4

- Recovery channel and production support flow.
- Retention duration for invitations, abandoned drafts, revisions and audit.
- Approved privacy/consent text.
- Incident response and account deactivation procedures.

These points are intentionally pending and must be decided inside the owning slice rather than guessed globally.

## 11. Definition of Phase 2.1 complete

Phase 2.1 is not complete merely because a patient can log in or submit a form.

It is complete only when:

- PI-1 through PI-4 are accepted with aligned code, tests and docs;
- new and existing patients can use the bounded workflows;
- no patient receives tenant-wide staff access;
- clinic review precedes canonical application;
- duplicate and conflict paths are operationally usable;
- every effective patient-originated change is traceable;
- cross-tenant, IDOR, replay, concurrency, expiry, revocation and recovery abuse have automated coverage;
- public endpoints have rate limiting and generic errors;
- staff auth and existing clinical workflows remain backward compatible;
- operational runbook exists;
- STATE and base docs are reconciled.

## 12. Current next action

The current repository has an accepted MVP and an explicitly opened **Phase 2.1 — Patient Intake and Portal Foundation**.

For Patient Intake and Portal:

1. Preserve PI-1 / #4, PI-2A / #31 and PI-2B / #33 as completed through PRs #26, #28, #29, #30, #32 and #34.
2. Implement only PI-2C1 / #36 next: waiting-room credential, append-only audit, `patientportal.intake.manage` and staff issue/list/revoke API.
3. Keep anonymous consume and `patient_intake` session in PI-2C2 / #37; keep staff copy/print/local-QR UI in PI-2C3 / #38; keep patient capture in PI-2D.
4. Do not add canonical application to PI-2; keep PI-3 and PI-4 pending until their own gates.

## 13. Decision note

**Context:** The client wants waiting-room registration and ongoing patient updates, but did not assign priority over the operational-core roadmap.

**Decision:** Implement PI-1 under ADR 012–015 and PI-2 under ADR 016/017. Accept PI-2B as the linked-patient self-only API and implement PI-2C only through #36 → #37 → #38 before PI-2D.

**Consequence:** PI-1, PI-2A and PI-2B now provide secure linked-patient access and intake create/get/save without canonical writes. Waiting-room credential/session/staff handoff remain PI-2C; patient questionnaire capture remains PI-2D; review/apply and final hardening remain unimplemented.
