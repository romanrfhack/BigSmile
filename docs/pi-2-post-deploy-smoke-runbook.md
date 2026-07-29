# PI-2 — Post-Deploy Smoke Validation Runbook

- **Scope:** PI-2 — Patient Intake Draft and Self-Service Capture
- **Runtime baseline:** `4b8cb66163948c5b69ff6c3c0027d01e105ce1fb`
- **Canonical baseline:** `48a6840ea88a55626e246bdcf5c9a4b3b1d8d7b5`
- **Status:** Pending execution in a deployed target environment
- **Tracking:** dedicated operational issue

## 1. Purpose

Validate the deployed PI-2 patient-intake workflow using real HTTP/browser boundaries without confusing CI evidence with deployment evidence.

This runbook does not open PI-3 and does not authorize submit, staff review or canonical application.

## 2. Required prerequisites

Do not place secrets, passwords, raw invitation tokens, raw waiting-room tokens or JWTs in GitHub comments or screenshots.

Required before execution:

- target environment name and base URL;
- deployed commit/version at least `48a6840ea88a55626e246bdcf5c9a4b3b1d8d7b5`;
- database migrations applied successfully;
- HTTPS and trusted reverse-proxy/forwarded-header configuration confirmed;
- active tenant with an active `Tenant.Subdomain`;
- active Branch for the waiting-room test;
- TenantAdmin test identity with `patientportal.intake.manage` after a fresh login;
- existing linked patient portal identity;
- disposable waiting-room patient identity/link;
- authorized read-only means to compare canonical Patient/Clinical data before and after;
- desktop and mobile/tablet browser access.

## 3. Evidence header

Record:

```text
Environment:
Base URL:
Deployed commit/version:
Execution UTC:
Executor:
Tenant realm:
Branch:
Browsers/devices:
Database migration status:
```

Use sanitized identifiers only.

## 4. Existing-patient flow

1. Sign in through `/patient-portal/{tenantSubdomain}/login`.
2. Open `/patient-portal/{tenantSubdomain}/intake`.
3. If no draft exists, create it explicitly.
4. Edit at least one non-medical field and one medical answer.
5. Confirm the page reports unsaved changes.
6. Attempt route navigation and verify the discard warning.
7. Cancel navigation and save explicitly.
8. Verify revision/concurrency metadata refreshes after an effective save.
9. Save again without changes and verify no-op behavior.
10. Sign out, sign in again and reload the same draft.

Expected:

- only the authenticated patient's intake is available;
- exactly 39 medical answers remain present;
- `Unknown` remains distinct from `No`;
- no autosave occurs;
- no browser storage contains tokens or form values;
- canonical Patient/Clinical data remains unchanged.

## 5. Waiting-room flow

1. Sign in as TenantAdmin and open `/patient-intake-links`.
2. Issue one Branch-scoped link.
3. Copy or scan its locally generated QR.
4. Open `/patient-portal/intake-activate#token=...`.
5. Verify the fragment is removed immediately.
6. Activate the intake-only account.
7. Edit both intake sections and save explicitly.
8. Sign out and use `/patient-portal/{tenantSubdomain}/intake-login` for recurrent login.
9. Reload the same intake.
10. Verify the original link is consumed and replay fails generically.

Expected:

- no canonical Patient or ClinicalRecord is created;
- the account remains intake-only;
- only the exact `intake_id` draft is accessible;
- staff bearer never reaches patient APIs and patient/intake bearer never reaches staff APIs.

## 6. Concurrency conflict

Use two authenticated browser contexts for the same test account:

1. Load the same draft in both contexts.
2. Save an effective change in context A.
3. Attempt a different save from stale context B.
4. Verify HTTP 409 with `patient_intake.concurrency_conflict`.
5. Verify B retains local edits in memory and blocks a second stale write.
6. Choose the explicit latest-version reload and confirm discard.

Expected:

- no force overwrite;
- no client-side merge;
- latest authoritative snapshot loads only after explicit confirmation.

## 7. Expiry and session recovery

### Linked patient

1. Use an expired draft fixture or controlled clock/data preparation.
2. Attempt save.
3. Verify `patient_intake.expired` and disabled stale writes.
4. Create a replacement explicitly.

### Intake-only patient

1. Use an expired draft or revoked/session-version-invalid fixture.
2. Attempt load/save.
3. Verify only the intake-only session is cleared.
4. Verify guidance directs the patient to reception for a new credential.

Expected:

- no redirect to the wrong patient login;
- no account/tenant/intake enumeration details;
- reads and no-op saves do not extend expiry.

## 8. Accessibility and responsive pass

Validate at minimum:

- desktop viewport;
- tablet viewport;
- narrow mobile viewport;
- keyboard-only navigation;
- visible focus;
- semantic headings and labels;
- live-region announcements for saving, saved, conflict, expiry and unauthorized states;
- action controls do not cover fields or browser UI;
- reduced-motion preference does not degrade usability.

## 9. Canonical-data verification

Compare before and after:

- Patient demographic/contact record;
- ClinicalRecord;
- ClinicalMedicalAnswer;
- allergies and alerts;
- encounters, diagnoses, vitals and timeline.

Expected: no canonical changes caused by PI-2 patient endpoints.

## 10. Result recording

For each scenario record:

```text
PASS / FAIL / BLOCKED
Observed result:
Sanitized evidence:
Related log/correlation id:
Follow-up issue:
```

Any failure involving tenant isolation, bearer leakage, replay, canonical writes or silent overwrite is release-blocking and requires a dedicated security/defect issue.

## 11. Closure gate

The operational smoke issue may close only when:

- all required scenarios are PASS;
- blocked prerequisites are resolved;
- failures have been fixed and revalidated;
- evidence is sanitized and attached;
- the exact deployed version is recorded.

PI-3 remains unopened after this runbook. A separate explicit product and architecture decision is required.