# PI-3A Production Deployment Record

- **Status:** Technical production deployment accepted; controlled authenticated functional UAT pending
- **Deployment date:** 2026-08-27
- **Change:** PR #60 — Add pre-appointment patient intake requests
- **Target commit:** `67e29f6712e4d23c9ca6eaaa1cbda29c484687b4`
- **Decision boundary:** ADR 020
- **Production release:** `20260827064136-67e29f6712e4`
- **Previous release:** `20260806012859-451df25709c1`

## 1. Purpose

This document records the coordinated backend, frontend and database deployment of PI-3A, the evidence used to approve the technical promotion, the operational defects recovered during the run, and the remaining acceptance gate.

It is an operational deployment record, not an architectural replacement for ADR 020 and not a formal acceptance of PI-3B or Phase 2.1.

## 2. Deployed boundary

The deployed PI-3A boundary includes:

- explicit patient-self final submission to `Submitted`;
- identity and all 39 fixed-answer completion gates;
- immutable submitted state and one submitted intake per linked Patient/Tenant;
- appointment-scoped access and completion status;
- least-privilege `patientportal.intake.request` reception permission;
- optional activation/login preparation from an accessible appointment;
- manual WhatsApp click-to-chat handoff;
- additive migration `20260806131500_AddPatientIntakeSubmissionBoundary`.

The deployment does not include:

- PI-3B clinic review, duplicate resolution or canonical apply;
- patient-originated writes to canonical `Patient` or `ClinicalRecord`;
- automated WhatsApp/SMS/email delivery, providers, queues, retries or campaigns;
- full Patient Portal, online booking or PI-4 closure.

## 3. Build and CI evidence

| Check | Result |
| --- | --- |
| Backend tests | 616 passed, 0 failed |
| Frontend tests | 281 passed, 0 failed |
| Frontend production build | Passed |
| Pull-request CI | Passed |
| Post-merge CI | Passed, run 31273385658 |
| EF model drift | No pending model changes |
| Packaged migrations | 34; target migration last |
| Clean source | Exact target commit from a clean worktree |

Production package:

- file: `BigSmile_release_67e29f6712e4_20260827T021356Z.tar.gz`;
- size: `70452421` bytes;
- SHA-256: `fd19415f6a6a75d2a50d02e45cea49928901f3707dbf4749f630a9c06cc63c91`;
- staging path: `/var/www/bigsmile/staging/67e29f6712e4-20260827T021356Z`;
- internal checksum manifest: all 163 declared files passed.

Primary staged hashes:

- backend `BigSmile.Api.dll`: `faadd94e49b097a5d00612acd27cccdfef25fcb94e747a30883f3cbcc4eb31ff`;
- frontend `index.html`: `01cf64848f1cee488926b1d05d7fc0f467f969628e826da50236807611b3e403`;
- Linux x64 self-contained `efbundle`: `84bfc00d70b901aa4dc896a89aca0a610e7cd9838da9c64767ffa224a1029209`.

## 4. Database safety evidence

Before migration, production contained 33 migrations and ended at `20260726031143_AddPatientIntakeOnlyAuthenticationBoundary`.

The deployment used three independent database checks:

1. A pre-migration `COPY_ONLY` backup with checksum and `RESTORE VERIFYONLY`:
   - `BigSmile_COPY_ONLY_20260827T052331Z.bak`;
   - SHA-256 `68b5033d0e84f678f436155cc9a08b87fe58908e172bb47a3897c1ef8a36b85e`.
2. A restore drill into a temporary database:
   - restore completed;
   - `DBCC CHECKDB` passed;
   - 41 user tables and 33 migrations were observed;
   - the expected last migration was present;
   - the temporary database and files were removed.
3. A new-bundle no-op execution against the current 33-migration database:
   - bundle configuration and production connection were valid;
   - no migrations were applied;
   - migration history remained unchanged.

During the maintenance window, after stopping application writes, a fresh `COPY_ONLY` backup was created and verified:

- `BigSmile_COPY_ONLY_20260827T060505Z.bak`;
- SHA-256 `628b77de25633c891d2590a52da6561cbe289b7ed4d861a7d5c60e4f74ac74e1`;
- checksum enabled;
- `RESTORE VERIFYONLY` passed.

The bundle then applied exactly one migration. SQL history was independently re-read and confirmed:

- applied migration count: 34;
- last migration: `20260806131500_AddPatientIntakeSubmissionBoundary`;
- database state: online.

## 5. Recovered operational defects

### 5.1 Stale migration postcheck

The release-specific migration script inherited an old assertion requiring exactly 10 `Applying migration` log entries. The new release correctly required one.

The bundle applied the target migration successfully, but the stale postcheck failed immediately afterward. Cleanup restarted the previous API release. There was no migration rollback and no data loss.

Recovery actions:

- independently confirmed 34 migrations and the expected last migration in SQL;
- confirmed the bundle log contained exactly one target `Applying migration` line and `Done`;
- confirmed the maintenance backup and API health;
- corrected the postcheck from 10 to 1 and the expected after-count message from 33 to 34;
- did not rerun the already-applied migration;
- created `migration-recovery-manifest-20260827T060503Z.txt` with SHA-256 `2cfd8888c68417487b74a1bd2a071f0716fb25bba3b52a61bb23c0c94eafb031`.

Conclusion: migration applied and validated; the failure belonged to post-execution evidence validation, not to EF migration execution.

### 5.2 Restrictive staging permissions

Package extraction preserved root-only modes for staged backend files. The first sidecar attempt could not read the staged API and rolled back the environment file exactly.

Recovery actions:

- verified the environment rollback was byte-for-byte exact;
- changed only staged ownership/modes to `root:www-data`, directories `0750`, regular files `0640`, and required executables `0750`;
- revalidated every internal checksum after the permission change;
- reran the staged API sidecar on port 55110;
- received expected staff and patient authentication `401` responses;
- stopped and removed the sidecar and released port 55110.

No live service restart, database write or release promotion occurred during the failed sidecar attempt.

### 5.3 Inspection and script hygiene

Additional non-production-changing corrections were required during preparation:

- use the configured VPS IPv4 SSH endpoint instead of relying on hostname resolution;
- normalize archive members that include a leading `./`;
- avoid early-closing pipeline consumers under `set -o pipefail`;
- replace stale release paths, hashes, migration counts/lists and evidence references in release-specific scripts;
- make backup manifests report the actual current migration instead of a hard-coded historical value.

These are runbook and automation defects. They did not change the PI-3A product contract.

## 6. Sidecar and promotion evidence

The corrected sidecar preflight confirmed:

- staged backend hash matched;
- existing patient JWT secret was reused, not regenerated;
- staff and patient signing secrets remained distinct;
- staged staff and patient auth endpoints returned `401`;
- live API remained active;
- current release remained unchanged;
- no database, Nginx or live-service modification occurred.

Atomic promotion then confirmed:

- `current` points to `/var/www/bigsmile/releases/20260827064136-67e29f6712e4`;
- `.latest` is `20260827064136-67e29f6712e4`;
- `.previous` preserves `20260806012859-451df25709c1`;
- API process working directory is the new release backend;
- local and public staff auth endpoints return `401`;
- local and public patient auth endpoints return `401`;
- frontend returns `200`;
- public frontend SHA-256 matches the staged artifact;
- startup error count is zero;
- migration history remains at 34 with the target migration last;
- Nginx was not reloaded;
- rollback was not performed.

Technical promotion completed at `2026-08-27T06:44:44+00:00`.

A subsequent read-only post-promotion recheck reconfirmed:

- `current`, `.latest` and `.previous` matched the expected immutable releases;
- the API process remained active/enabled with its working directory in the new release;
- active backend and frontend hashes still matched the staged artifacts;
- local/public staff and patient authentication endpoints still returned the expected `401`;
- the public frontend still returned `200`;
- SQL history remained at 34 migrations with the target migration last;
- the API journal contained no error-priority entries after promotion;
- no migration, sidecar or promotion process remained active.

The post-promotion technical recheck passed without modifying the database, service configuration or release pointers.

## 7. Rollback boundary

Application rollback remains available through the preserved previous immutable release. The database migration is additive and was applied before promotion.

A release rollback must not blindly restore the pre-migration database after production writes resume. Database restore would discard later writes and requires an explicit incident decision. Prefer application rollback only when the previous release is compatible with the additive schema; otherwise prepare a forward fix.

## 8. Remaining acceptance gate

PI-3A is deployed but not yet formally product-accepted. Controlled authenticated UAT must use an authorized test tenant, appointment and patient and must verify:

1. a staff user with `patientportal.intake.request` can read appointment intake status;
2. a user without that permission is denied;
3. a request can be prepared only for an accessible appointment/Branch;
4. manual activation/login or WhatsApp handoff exposes no token in logs or durable browser storage;
5. the patient can complete and submit all required data;
6. appointment status changes to completed after persisted `Submitted`;
7. a second request for the same linked Patient is blocked;
8. no canonical `Patient` or `ClinicalRecord` fields are changed by submission;
9. existing staff scheduling and patient-intake flows remain usable.

Record the UAT result in a separate acceptance update. Only a passing result may change PI-3A from acceptance-pending to formally accepted.

## 9. Follow-up operational debt

The following work is intentionally separate from the deployed feature:

- version and parameterize deployment scripts in the repository instead of cloning server-local release-specific scripts;
- derive expected migration deltas and backup metadata from manifests/runtime history;
- validate runtime ownership/traversal modes during staging;
- add shell syntax and invariant tests for deployment scripts;
- review production Data Protection key persistence warnings;
- review forwarded-header/HTTPS-redirection configuration behind Nginx;
- remediate known high-severity dependency warnings in a dedicated dependency change.

