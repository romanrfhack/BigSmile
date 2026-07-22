# Patient Intake and Portal General Plan

- **Status:** Planned; implementation not opened
- **Roadmap placement:** Phase 2.1 — Patient Intake and Portal Foundation
- **Start gate:** After the initial MVP is formally accepted and stable, unless a future explicit roadmap decision reprioritizes it
- **Architecture decision:** ADR 004
- **Parent tracking:** GitHub issue #2
- **Implementation tracking:** issues #4, #5, #6, and #7
- **Last updated:** 2026-07-22

## 1. Purpose

This document keeps the patient-facing intake requirement visible and actionable without mixing planned work with completed functionality.

The client wants:

1. New patients in the waiting room to register and begin completing their information.
2. Existing patients to activate access and complement information that the clinic has not captured.
3. Patient-originated changes to remain traceable.
4. Clinic staff to review changes before they are applied to canonical Patient or Clinical Records data.

This plan turns that requirement into sequential, auditable slices while preserving BigSmile's staff authorization model, tenant isolation, clinical provenance, and current roadmap order.

## 2. Roadmap decision

The bounded capability is placed in **Phase 2.1 — Patient Intake and Portal Foundation**, inside **Phase 2 Expansion — Modern Operations**, after the initial operational MVP.

The placement is deliberate:

- intake forms and digital patient updates belong to modern operational workflows;
- the workflow depends on stable Patients and Clinical Records modules;
- clinic review and canonical application depend on the operational system remaining authoritative;
- a public authentication boundary is high-risk and should not interrupt Release 4 through Release 7 without an explicit reprioritization decision;
- the client confirmed the capability is desired but did not request that it displace the existing roadmap.

The broader patient portal remains deferred to Phase 4. Phase 2.1 does not include professional clinical-record browsing, online booking, billing, documents, treatments, odontogram access, or automated messaging.

## 3. Status summary

| Item | Status | Evidence / tracking |
| --- | --- | --- |
| Fixed 39-question medical questionnaire backend | Completed as Release 3.5 | Existing Clinical Records code and tests |
| Internal staff questionnaire UI | Completed | Existing Clinical Records UI |
| Familiar `Sí / No / Sin respuesta` capture redesign | Completed and merged | PR #1 |
| Patient-facing architecture decision | Accepted in ADR 004; pending merge of PR #3 | PR #3 |
| Parent product backlog | Open | Issue #2 |
| PI-1 access/invitations | Planned; not implemented | Issue #4 |
| PI-2 intake draft | Planned; not implemented | Issue #5 |
| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |
| PI-4 audit/hardening | Planned; not implemented | Issue #7 |
| Patient-facing backend/API/database/frontend | Not started | No implementation PR |

## 4. Scope boundary

### Included in Phase 2.1

- Separate patient identity and session boundary.
- Existing-patient activation through a staff-issued invitation.
- Waiting-room registration through a short-lived clinic QR/link.
- Self-only intake draft capture.
- Demographic/contact information selected for the intake contract.
- The existing fixed medical-question catalog.
- Explicit draft saves and immutable submitted revisions.
- Clinic review, duplicate handling, link/create decision, and canonical apply.
- Append-only domain audit and staff-visible traceability.
- Security hardening required for public patient-facing access.

### Not included in Phase 2.1

- Patient access to professional diagnoses, notes, encounters, vital signs, odontogram, treatment plans, quotes, billing, payments, or documents.
- Online booking.
- Automated email, SMS, or WhatsApp delivery.
- Family or dependent accounts.
- Multiple Patient records per portal account.
- Advanced consent or electronic signature.
- Configurable form builder.
- Automatic interpretation of questionnaire responses.
- Automatic allergy or clinical-alert synchronization without a separate decision.

## 5. Architectural boundaries to preserve

### 5.1 Staff identity remains unchanged

Patients must not become `TenantUser` and must not receive staff permissions. Existing `/api/auth` behavior, staff JWTs, `UserTenantMembership`, roles, and permissions remain backward compatible.

### 5.2 Patient access is self-scoped

Every patient-facing read/write derives tenant, portal-account, and linked-patient ownership from verified server-issued context. Request bodies and route identifiers are not authority sources for choosing another patient.

### 5.3 Canonical data remains clinic-controlled

Anonymous or patient-authenticated endpoints never write directly to canonical `Patient` or `ClinicalRecord` data. Patient input moves through `Draft -> Submitted -> Reviewed -> Applied/Rejected`.

### 5.4 Audit is domain-specific and append-only

The system records effective saves and lifecycle transitions, not every keystroke. Audit records exclude passwords, raw tokens, authorization headers, and unnecessary sensitive source metadata.

### 5.5 Tenant is the primary boundary

All new account, invitation, intake, revision, and audit records are tenant-owned. Branch is optional operational context only when a future use case proves it is required.

## 6. Planned domain model

The following names are accepted as the initial design direction. Detailed properties and invariants must be finalized inside the owning implementation slice rather than in one large up-front schema change.

### `PatientPortalAccount`

Purpose: patient-facing identity separate from staff `User` membership semantics.

Initial invariants:

- belongs to exactly one tenant;
- links to zero or one canonical Patient during activation/new-intake processing, and at most one after activation in Phase 2.1;
- unique normalized login identifier according to the final account strategy;
- explicit active/locked state;
- no staff roles or tenant-wide permissions;
- session/recovery metadata is security-owned and never exposed as clinical data.

### `PatientPortalInvitation`

Purpose: secure activation or intake entry.

Initial invariants:

- tenant-owned;
- purpose-specific;
- existing-patient invitations are bound to a Patient server-side;
- token stored only as a cryptographic hash;
- single-use;
- expirable;
- revocable;
- consumption protected against concurrent replay.

### `PatientIntake`

Purpose: patient-originated draft and workflow aggregate.

Initial invariants:

- tenant-owned;
- linked to a portal account or validated short-lived intake session;
- optionally linked to a canonical Patient;
- explicit lifecycle state;
- optimistic concurrency token;
- no direct ownership of professional clinical data.

### `PatientIntakeRevision`

Purpose: immutable snapshot/diff for effective saves and submissions.

Initial invariants:

- monotonic revision number within the intake;
- immutable after creation;
- actor and timestamp required;
- changed field identifiers or normalized diff retained;
- submitted revisions remain available after apply or reject.

### `PatientIntakeAuditEntry`

Purpose: append-only lifecycle/security trace.

Initial invariants:

- tenant, intake, action, actor type, actor id, timestamp and correlation id;
- linked Patient when known;
- no secrets;
- no normal update/delete workflow.

## 7. Implementation sequence

The slices are strictly ordered. A later slice may be designed while an earlier slice is under review, but it must not bypass the earlier security or data-integrity gates.

### PI-1 — Access and Invitation Foundation — issue #4

#### Goal

Create the minimum safe patient identity and invitation boundary without allowing intake writes.

#### Deliverables

- Domain entities and invariants for account and invitation.
- EF Core configuration, indexes, additive migration, and tenant filters.
- Patient-specific authentication scheme, claims, audience/scope, and policies.
- Staff invitation issue/revoke use cases and endpoints.
- Patient activation/login/current-session use cases and endpoints.
- Token hashing, expiration, revocation, single-use consumption, and rate limiting.
- Generic public errors and anti-enumeration behavior.
- Integration and security tests.

#### Exit gate

PI-1 is complete only when cross-tenant access, IDOR, replay, concurrent consumption, expiry, revocation, and absence of staff permissions are automatically verified.

### PI-2 — Intake Draft and Self-Service Capture — issue #5

#### Goal

Allow a patient to save only their own intake draft without changing canonical records.

#### Deliverables

- `PatientIntake` aggregate and initial draft contract.
- Existing-patient prelinked intake.
- New-patient QR/link intake session.
- Reuse of the 39 medical question keys and answer values.
- Explicit draft save with optimistic concurrency.
- Effective-save revision and audit creation.
- Dedicated Angular patient route area, auth/session service, guard, data-access, facade, and mobile/tablet-first UI.
- Expired/revoked session handling.

#### Exit gate

PI-2 is complete only when a patient can read/write only their own draft, identical saves create no false revisions, and no canonical Patient or ClinicalRecord data changes.

### PI-3 — Submit, Clinic Review, and Canonical Apply — issue #6

#### Goal

Move patient input through review and safely apply accepted fields.

#### Deliverables

- Immutable submitted revision.
- Staff pending-intake worklist.
- Review, reject, and apply use cases.
- Duplicate candidate presentation and explicit link/create decision.
- Canonical Patient application through Patients use cases.
- Canonical questionnaire application through Clinical Records use cases.
- Explicit clinical-record creation behavior compatible with existing no-autocreation rules.
- Conflict detection and optimistic concurrency against canonical changes.
- Idempotent application and transactional boundaries where required.
- Provenance that distinguishes patient input from staff application.

#### Exit gate

PI-3 is complete only when patient endpoints cannot apply canonical changes, authorized staff application is conflict-aware and auditable, and rejected/submitted revisions remain immutable.

### PI-4 — Audit Visibility and Security Hardening — issue #7

#### Goal

Make the bounded capability operationally reviewable and safe for production-facing use.

#### Deliverables

- Staff audit timeline and provenance view.
- Account lockout and approved recovery mechanism.
- Session, invitation, and intake-link revocation.
- Additional rate limits and abuse telemetry.
- Privacy/retention configuration and documented legal/product gaps.
- Full e2e workflow coverage.
- Operational runbook for invitation issuance, patient support, account deactivation, failed activation, conflict handling, and incidents.

#### Exit gate

PI-4 is complete only when the full bounded flow is e2e validated and the public boundary has recovery, revocation, audit visibility, abuse controls, and an operational runbook.

## 8. Cross-slice validation matrix

| Dimension | Required evidence |
| --- | --- |
| Architecture | Layer/module placement, no staff/patient auth mixing, architecture validation green |
| Tenant safety | Query filters or equivalent centralized enforcement, write validation, cross-tenant tests |
| Authorization | Self-only policies, no platform override, IDOR tests, no staff permissions in patient tokens |
| Token security | Hash-at-rest, expiry, revocation, single-use, constant-time comparison, replay/concurrency tests |
| Data integrity | Draft/canonical separation, immutable submissions, idempotent apply, conflict detection |
| Audit | Effective saves and transitions recorded, no secrets, immutable normal flow |
| Frontend | Separate route area, guards, facade/data-access separation, mobile/tablet UX, expired/error states |
| API | Generic public errors, no arbitrary tenant/patient authority from client input, bounded contracts |
| Database | Additive migrations, tenant indexes, uniqueness constraints, rollback/deploy notes |
| CI | Backend build, architecture validation, unit tests, integration tests, frontend build/tests, e2e when opened |
| Documentation | STATE, roadmap, ADR, plan, API/security notes and runbook aligned with delivered slice |

## 9. Deployment strategy

Each implementation slice must be deployable independently and must prefer additive, backward-compatible changes.

### PI-1 deployment

- Add tables, indexes, policies and endpoints without changing staff auth contracts.
- Keep patient-facing functionality unavailable until required configuration and routes are intentionally enabled.
- Validate invitation consumption transactionality before enabling external access.

### PI-2 deployment

- Add draft tables and patient-facing UI without canonical write behavior.
- Existing staff questionnaire and patient workflows remain unchanged.

### PI-3 deployment

- Introduce review/apply behind explicit staff operations.
- Do not auto-apply existing drafts/submissions during deployment.
- Provide migration and rollback notes for any new canonical provenance fields.

### PI-4 deployment

- Treat completion as the production-readiness gate for remote patient access.
- Waiting-room-only controlled use may be evaluated earlier only if the implemented slice's security controls and operational limitations are explicit.

## 10. Decisions required before implementation opens

These points are intentionally pending and must be resolved in the relevant slice, not guessed globally:

### Before PI-1

- Patient login identifier: email, phone, username, or a bounded combination.
- Initial authentication UX: password, magic link, or another approved compatible approach.
- Invitation and waiting-room link expiration defaults.
- Minimum password and lockout policy if passwords are selected.
- Development/pilot method for delivering activation links without an external provider.

### Before PI-2

- Exact demographic/contact fields a patient may propose.
- Whether phone slots remain a separate Patient Contact Details slice or are included in intake as proposed values only.
- Draft expiration/abandonment policy.
- Whether explicit save or bounded debounced autosave is used; every persisted effective save remains auditable.

### Before PI-3

- Staff roles/permissions allowed to review and apply.
- Duplicate-candidate criteria and what data may be shown to reviewers.
- Field-level conflict resolution UX.
- Whether questionnaire application can explicitly create a missing ClinicalRecord during staff apply or must require prior record creation.
- Exact provenance fields retained in canonical models versus intake history only.

### Before PI-4

- Recovery channel and production support flow.
- Retention duration for invitations, abandoned drafts, revisions, and audit events.
- Privacy notice and consent text approved by product/legal stakeholders.
- Incident-response and account-deactivation procedures.

## 11. Definition of Phase 2.1 complete

Phase 2.1 is not complete merely because a patient can log in or submit a form.

It is complete only when:

- PI-1 through PI-4 are accepted with aligned code, tests, and documentation;
- existing and new patients can use the intended bounded workflows;
- no patient receives tenant-wide staff access;
- clinic review precedes canonical application;
- duplicate and conflict paths are operationally usable;
- every effective patient-originated change is traceable;
- cross-tenant, IDOR, replay, concurrency, expiry, revocation, and recovery abuse have automated coverage;
- public endpoints have rate limiting and generic errors;
- staff auth and existing clinical workflows remain backward compatible;
- an operational runbook exists;
- `STATE — BigSmile.md` and base documentation are reconciled before marking the phase complete.

## 12. Current next action

The current repository remains positioned at Release 4 — Odontogram as the next planned functional phase.

For Patient Intake and Portal, the next action is organizational rather than implementation:

1. Merge ADR 004 and this plan.
2. Keep issues #4 through #7 open and ordered.
3. Do not start PI-1 until Phase 2.1 is explicitly opened after MVP acceptance or a future documented roadmap reprioritization.
4. When Phase 2.1 opens, start only with issue #4 and update canonical state in the same PR that opens the slice.

## 13. Decision note

**Context:** the client wants waiting-room registration and ongoing patient updates, but did not assign a priority that should displace the current operational-core roadmap.

**Decision:** plan the bounded self-service capability as Phase 2.1 after the initial MVP, with a separate patient identity boundary, staged clinic review, and append-only audit.

**Consequence:** the requirement remains visible, decomposed, testable, and traceable without weakening current tenant-aware auth or prematurely opening a full patient portal.