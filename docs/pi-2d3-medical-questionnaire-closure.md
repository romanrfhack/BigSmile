# PI-2D3 — Familiar Fixed Medical Questionnaire Capture Closure

- **Status:** Completed
- **Issue:** #47
- **Pull request:** #55
- **Merge commit:** `378ddb255b975efc98d1902a054bf5568f39857c`
- **Validation:** CI #479
- **Architecture:** ADR 006 and ADR 012–019; PI-2D plan

## Accepted scope

PI-2D3 adds the patient-facing interaction for the fixed medical questionnaire already accepted by Clinical Records and Patient Intake:

- one narrow shared frontend catalog for static groups, question keys, labels and answer options;
- six familiar thematic sections;
- exactly 39 unique `QuestionKey` values in accepted order and casing;
- visible `Sí / No / Sin respuesta` controls;
- `Unknown` as the safe initial value, never implicit `No`;
- optional details up to 500 characters;
- progress based only on answers different from `Unknown`;
- responsive cards, labelled radio groups, keyboard support and visible focus.

The Clinical Records consumer uses an adapter over the shared catalog. Its feature-local transport models and runtime behavior remain unchanged.

## Save contract

- explicit full-snapshot save only;
- no autosave, debounce HTTP or per-question request;
- exactly 39 answers submitted once in server order;
- missing or duplicate answers rejected before transport;
- all non-medical PI-2D2 values preserved;
- either section's save action includes current unsaved values from the sibling section;
- details are normalized at save time and retained when changing away from `Yes` to prevent accidental loss;
- authoritative response, revision and concurrency token continue to replace local state after success.

## Security and compatibility

- same self-only `patient` / `patient_intake` workspace and bearer boundary;
- no Tenant, Patient, account or intake identifiers added as request authority;
- no browser storage for auth/session or form data;
- no platform override;
- no backend, API, EF Core, migration, permission, auth or session change;
- no canonical `Patient`, `ClinicalRecord`, allergy, alert, medication or timeline write.

## Validation evidence

CI #479 passed:

- backend restore/build;
- architecture validation;
- backend unit tests;
- backend integration tests;
- frontend dependency install;
- Angular production build;
- frontend tests.

Focused coverage includes shared-catalog parity, six groups, 39 unique ordered keys, 117 radio controls, `Unknown` progress semantics, details preservation/length, explicit complete payloads, sibling-section preservation, no-op/conflict behavior and absence of browser storage/autosave.

## Deferred

- complete conflict/expiry/session-invalidated UX — PI-2D4;
- unsaved-navigation protection and representative operational smoke validation — PI-2D4;
- PI-2 closure — PI-2D4;
- submit/review/apply and canonical contact storage — PI-3 prerequisites;
- retention/privacy production hardening — PI-4.

## Decision note

PI-2D3 implements the accepted ADR 016 questionnaire contract without changing architecture, ownership, authentication, authorization or persistence. No new ADR is required. PI-2D4 is the next dependency-satisfied gate but remains unimplemented until explicitly authorized.
