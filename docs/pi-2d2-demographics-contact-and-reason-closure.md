# PI-2D2 — Demographics, Contact and Reason-for-Visit Closure

- **Status:** Completed
- **Issue:** #46
- **Pull request:** #53
- **Merge commit:** `ade674eb7f52be366f8c2970539966c10e3d0f52`
- **Validation:** CI #470
- **Architecture:** ADR 006 and ADR 012–019; PI-2D plan

## Accepted scope

PI-2D2 adds the responsive patient-facing form for the non-medical fields already supported by `PatientIntake`:

- first and last name;
- date of birth with derived read-only age;
- sex, occupation, marital status and referred-by;
- preferred, mobile, home and work phones;
- email;
- responsible-party name, relationship and phone;
- patient-reported reason for visit.

The same form works for linked `scope=patient` and unlinked `scope=patient_intake` sessions.

## Save and validation contract

- explicit save only; no autosave;
- complete snapshot PUT using the current concurrency token;
- blank optional strings normalized to `null`;
- frontend constraints mirror the backend while backend validation remains authoritative;
- date of birth cannot be in the future;
- responsible-party name is required when relationship or phone is supplied;
- age is derived and is not transported;
- all 39 medical answers are preserved unchanged and in server order;
- `changed=true` replaces the local authoritative revision/token;
- `changed=false` is represented as a no-op and does not invent a revision.

## Security and compatibility

- no Tenant, Patient, account or intake identifiers were added to route/body authority;
- patient and intake-only tokens remain memory-only and preserve PI-2D1 bearer separation;
- no platform override;
- no backend, database, migration, permission or API contract change;
- no canonical `Patient`, `ClinicalRecord` or `ClinicalMedicalAnswer` write.

## Deferred

- interactive fixed 39-question medical questionnaire — PI-2D3;
- final conflict, expiry, unsaved-navigation and closure UX — PI-2D4;
- submit/review/apply and canonical contact storage — PI-3 prerequisites;
- retention/privacy production hardening — PI-4.

## Decision note

PI-2D2 implements an already accepted capture contract and does not change architecture, ownership, authentication or session strategy. No new ADR is required. PI-2D3 is the next dependency-satisfied gate but remains unimplemented until explicitly authorized.
