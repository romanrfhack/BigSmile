# PI-2 — Patient Intake Draft and Self-Service Capture Closure

- **Status:** Completed
- **Parent issue:** #5
- **PI-2D issue:** #44
- **Final increment:** PI-2D4 #48
- **Final runtime PR:** #57
- **Merge commit:** `4b8cb66163948c5b69ff6c3c0027d01e105ce1fb`
- **Validation:** CI #485
- **Architecture:** ADR 006 and ADR 012–019

## 1. Closed scope

PI-2 is formally closed through four sequential boundaries:

1. PI-2A — tenant-owned draft persistence, fixed 39-answer set, immutable effective-save revisions, expiry and optimistic concurrency.
2. PI-2B — existing-patient self-only create/get/save API with id-less ownership, no-store responses and no canonical writes.
3. PI-2C — short-lived waiting-room credential, `patient_intake` scope, transactional activation and staff handoff UI.
4. PI-2D — Angular capture for both scopes, including supported non-medical fields, the shared six-section/39-key questionnaire, explicit full-snapshot save and lifecycle hardening.

The result is a bounded capture capability. It is not clinic review, canonical application or a full patient portal.

## 2. Final lifecycle hardening

PI-2D4 closes the unsafe or ambiguous edges of capture:

- additive stable `ProblemDetails.code` values distinguish concurrency conflict from expiry without breaking existing HTTP 409/title/detail clients;
- stale `409` retains the current authoritative snapshot and local form values in memory;
- repeated writes with the stale concurrency token are blocked;
- latest-version reload is explicit and warns that local edits will be discarded;
- linked patients can explicitly create a replacement after expiry;
- intake-only sessions clear only their affected store and direct the patient to reception reissue guidance;
- unauthorized sessions retain only the non-sensitive mode required to route to the correct tenant-scoped login;
- `canDeactivate` and `beforeunload` protect both form sections from accidental unsaved navigation;
- no autosave, force overwrite, client-side merge or browser persistence was added.

## 3. Security and ownership

- `TenantId` remains the primary security boundary.
- Branch remains subordinate operational context and is not introduced into self-only intake ownership.
- Tenant, Patient, account and intake identifiers are not accepted as request authority.
- `patient` and `patient_intake` sessions remain separate, mutually exclusive and memory-only.
- Patient-facing paths have no platform override.
- No canonical `Patient`, `ClinicalRecord`, `ClinicalMedicalAnswer`, allergy, alert, medication or timeline write occurs in PI-2.
- The fixed questionnaire remains patient-reported proposal data until a future clinic-controlled review/application flow.

## 4. Automated validation evidence

CI #485 passed on the final runtime head:

- backend restore and release build;
- architecture validation;
- 382/382 backend unit tests;
- 219/219 backend integration tests;
- Angular production build;
- 271/271 frontend tests across 64 files.

Focused automated smoke/regression coverage includes:

- linked-patient and intake-only workspace resolution;
- self-only create restrictions;
- tenant-realm mismatch fail-closed behavior;
- complete 39-answer snapshot preservation;
- sibling-section unsaved-value preservation;
- no-op save semantics;
- stable conflict/expiry problem codes without ownership disclosure;
- stale-write blocking and explicit latest reload;
- linked replacement and intake-only reissue recovery;
- affected-session-only clearing;
- route-only `canDeactivate`, `beforeunload` and logout discard confirmation;
- Spanish-first lifecycle copy;
- absence of form/token browser storage.

## 5. Smoke boundary and post-deploy checklist

[Hecho] The repository has no Playwright/Cypress dependency or e2e script. PI-2D4 therefore uses the existing backend integration plus Angular/Vitest component-router-facade stack for automated smoke evidence rather than introducing a new transversal test platform.

[Pendiente operativo] The following manual post-deploy checklist is not claimed as executed by CI and must be run in the target environment:

```text
existing patient login -> create/load draft -> edit both sections -> save -> reload
waiting-room link -> activate -> edit -> save -> logout -> recurrent intake login -> reload
stale concurrency token -> local edits remain -> second write blocked -> explicit latest reload
expired linked draft -> explicit replacement
expired/revoked intake-only session -> reception reissue guidance
mobile/tablet/desktop -> keyboard navigation, visible focus, live-region announcements
```

Deployment verification must also confirm that no canonical Patient or Clinical record changes occurred before staff review.

## 6. Deferred and not opened

- PI-3 — Submit, Clinic Review and Canonical Apply: not started.
- PI-4 — Audit Visibility and Security Hardening: not started.
- duplicate matching and canonical contact migration: deferred to PI-3 decisions.
- consent, retention and incident-response production closure: deferred to PI-4.
- full patient portal, clinical browsing, odontogram, treatments, billing and documents: outside this bounded capability.

## 7. Decision note

PI-2D4 implements the already accepted ownership and session boundaries. The additive problem codes and frontend lifecycle state do not change tenant resolution, authentication strategy, authorization semantics or persistence architecture; no new ADR is required.

PI-2 and PI-2D are closed. Phase 2.1 remains active because PI-3 and PI-4 are still planned but unopened. Opening PI-3 requires separate explicit authorization.
