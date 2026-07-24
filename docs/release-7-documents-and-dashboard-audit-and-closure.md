# Release 7 — Documents and Dashboard Audit and Closure

- **Status:** Accepted closure evidence
- **Audit opened:** 2026-07-22
- **Closure date:** 2026-07-23
- **Scope:** Release 7.1 — Patient Documents Foundation; Release 7.2 — Dashboard Read Model Foundation
- **Result:** Release 7 closes through two bounded foundational slices
- **MVP result:** Initial operational MVP formally accepted
- **Tracking:** issue #15
- **Blocking gaps resolved:** issues #16 and #17

## 1. Objective

Audit the existing Documents and Dashboard implementations without treating code presence as release acceptance, correct only the bounded security and operational gaps that block the planned foundation, and close the final release of the initial operational MVP with explicit evidence.

The audit distinguishes:

1. pre-existing implementation;
2. bounded contract satisfaction;
3. identified release-blocking gaps;
4. focused corrections;
5. formal release and MVP acceptance.

## 2. Reconciliation result

The initial audit found coherent implementations in both modules and two bounded blockers:

| Slice | Initial blocker | Resolution |
| --- | --- | --- |
| Release 7.1 — Patient Documents Foundation | Upload trusted client-declared MIME; multipart parsing was not bounded near the 10 MB product limit | PR #19 / issue #16 added binary-signature validation, request/form limits and storage-containment evidence |
| Release 7.2 — Dashboard Read Model Foundation | `Today` used UTC date and Tenant had no server-authoritative time zone | PR #20 / issue #17 added `Tenant.TimeZoneId`, migration, tenant-local day resolution and deterministic tests |

After those corrections, the repository satisfies both bounded slices without a broad rewrite.

Accepted slices:

- **Release 7.1 — Patient Documents Foundation**;
- **Release 7.2 — Dashboard Read Model Foundation**.

## 3. Release 7.1 — Patient Documents Foundation

### 3.1 Accepted domain and ownership

`PatientDocument` is the aggregate root for the accepted Documents foundation.

It is:

- tenant-owned;
- patient-owned;
- an attachment metadata record separate from ClinicalRecord, Odontogram, Treatments and Billing;
- logically retired rather than physically deleted through the normal application flow.

Accepted metadata:

- original file name;
- normalized allowlisted content type;
- file size;
- server-generated storage key;
- uploaded-at UTC and actor;
- optional retired-at UTC and actor.

Accepted invariant limits:

- content type must be `application/pdf`, `image/jpeg` or `image/png`;
- file size must be greater than zero and no greater than 10 MB;
- tenant, patient, actor and storage key are required;
- repeated retire is rejected.

### 3.2 Explicit upload workflow

The accepted upload flow:

1. requires an authenticated actor;
2. resolves the Patient through the current tenant or explicit platform-support path;
3. derives `TenantId` from verified context/Patient ownership;
4. normalizes the original name with `Path.GetFileName`;
5. generates a random internal storage key server-side;
6. validates declared type and size;
7. verifies the binary signature before storage;
8. rewinds the stream so storage receives the complete content;
9. writes the binary;
10. writes metadata;
11. removes the just-written binary if metadata persistence fails.

Public/API input does not provide `TenantId`, actor, storage key or physical path as authority.

### 3.3 Accepted binary verification

The server verifies the bounded signatures:

- PDF: `%PDF-`;
- JPEG: `FF D8 FF`;
- PNG: `89 50 4E 47 0D 0A 1A 0A`.

The validator also requires a readable, seekable stream and verifies the declared length against the actual stream length.

A MIME/signature mismatch, truncated header or empty content fails before file or metadata persistence.

This is a bounded format gate, not an antivirus or malware-scanning claim.

### 3.4 Transport and storage boundary

Accepted transport behavior:

- one multipart file request;
- authoritative business file limit: 10 MB;
- request/form multipart limit: 10 MB plus bounded 1 MB transport overhead.

Accepted private local storage behavior:

- default root under `App_Data/PatientDocuments`, outside `wwwroot`;
- `FileMode.CreateNew`;
- normalized full paths;
- save/open/delete rejected when a key resolves outside the configured root;
- no public storage URL;
- authorized download only after metadata lookup.

### 3.5 API boundary

Accepted endpoints:

- `GET /api/patients/{patientId}/documents`;
- `POST /api/patients/{patientId}/documents`;
- `GET /api/patients/{patientId}/documents/{documentId}/download`;
- `DELETE /api/patients/{patientId}/documents/{documentId}`.

Accepted behavior:

- active list only;
- newest-first ordering;
- authorized binary download with recorded name/type;
- retired documents unavailable through normal list/download paths;
- missing Patient/document returns `404` through the current contract;
- missing physical binary is surfaced as an integrity error.

### 3.6 Persistence and tenant safety

Accepted persistence:

- `PatientDocuments` stores explicit `TenantId` and `PatientId`;
- `StorageKey` is unique;
- ownership and active-query indexes are present;
- Patient deletion is restricted;
- Tenant deletion follows the established cascade model;
- centralized global tenant filters and write enforcement apply.

Accepted authorization:

- reads use `document.read`;
- writes use `document.write`;
- `PlatformAdmin` and `TenantAdmin` retain the current document permissions;
- `TenantUser` does not receive document permissions;
- platform support access activates override only through the explicit patient-scoped policy path;
- cross-tenant metadata and binary reads/writes remain blocked.

### 3.7 Angular boundary

The accepted Angular feature remains in `features/documents` with:

- lazy permission-guarded route;
- models;
- data-access service;
- facade;
- patient-context page;
- upload form, active list and empty state components;
- loading/error/saving states;
- permission-aware retire;
- authorized browser download.

The UI does not claim OCR, preview, external sharing or versioning.

### 3.8 Explicitly deferred Documents scope

Release 7.1 does not include:

- OCR;
- rich preview;
- document versioning;
- public/external sharing;
- generated PDFs/templates;
- electronic signatures or advanced consent workflows;
- advanced imaging viewer;
- external antivirus/malware provider integration;
- retention/physical-delete policy automation;
- Patient Portal document access.

Non-blocking UX/hardening debt includes retire confirmation, clinic-facing copy, actor display names, residual visual tokens and preservation of the original exception if cleanup also fails.

## 4. Release 7.2 — Dashboard Read Model Foundation

### 4.1 Accepted read-only boundary

The accepted Dashboard foundation is a tenant-scoped read model.

Accepted endpoint:

- `GET /api/dashboard/summary`.

The endpoint performs no mutation and returns:

- active patients;
- appointments whose start falls within the tenant-local operational day;
- those appointments still in `Scheduled` status;
- active/non-retired documents;
- existing treatment plans;
- accepted treatment quotes;
- issued Billing documents;
- generated-at UTC.

### 4.2 Metric definitions

Accepted semantics remain explicit:

- `ActivePatientsCount`: `Patient.IsActive`;
- `TodayAppointmentsCount`: every appointment state whose `StartsAt` falls in the tenant-local `[day start, next day start)` range;
- `TodayPendingAppointmentsCount`: `AppointmentStatus.Scheduled` in the same range;
- `ActiveDocumentsCount`: `DeletedAtUtc == null`;
- `ActiveTreatmentPlansCount`: every existing plan because archive/versioning is outside Release 5;
- `AcceptedQuotesCount`: `TreatmentQuoteStatus.Accepted`;
- `IssuedBillingDocumentsCount`: `BillingDocumentStatus.Issued`.

Cancelled appointments remain included in total-today and excluded from pending by design of the accepted current metric contract.

### 4.3 Tenant Time Zone Foundation

ADR 010 accepts `Tenant.TimeZoneId` as the server-authoritative owner of clinic-level operational dates.

Accepted model:

- required `TimeZoneId`;
- maximum length 100;
- validation through `TimeZoneInfo.FindSystemTimeZoneById`;
- persisted default `America/Mexico_City` for existing/bootstrap tenants in the current Mexican pilot context;
- each tenant owns its own persisted value;
- no branch-specific time zone in this slice.

Accepted Dashboard flow:

1. require a resolved tenant context;
2. load the tenant through the tenant-filtered repository path;
3. obtain current UTC instant from `TimeProvider`;
4. convert to the tenant time zone;
5. derive the tenant-local date;
6. query the existing wall-clock appointment range;
7. keep `GeneratedAtUtc` in UTC.

The browser cannot choose the time zone or day boundary.

### 4.4 Temporal compatibility

Release 7.2 does not rewrite Appointment persistence or historical scheduling values.

Appointments continue under the accepted clinic wall-clock `DateTime` model. Tenant time zone determines which wall-clock date is operationally `today` for server-side reads.

A broader instant/time-zone redesign remains a future decision for capabilities such as online booking, external calendars, background jobs or cross-zone operations.

### 4.5 Authorization and tenant safety

Accepted authorization:

- endpoint requires `dashboard.read`;
- current conservative role mapping grants it to `TenantAdmin`;
- `TenantUser` does not receive it;
- `PlatformAdmin` does not receive hidden Dashboard support access in the current catalog;
- policy and application service require a resolved tenant;
- platform scope/override without a resolved tenant remains blocked.

Every count explicitly includes current `tenantId` and reads accepted aggregate roots. Dashboard has no write path and does not synchronize module state.

### 4.6 Angular boundary

The accepted Angular feature remains in `features/dashboard` with:

- permission-guarded lazy route;
- data-access service;
- facade;
- bounded page;
- loading skeletons;
- error feedback;
- empty state;
- summary cards with helper copy;
- generated-at display.

### 4.7 Explicitly deferred Dashboard scope

Release 7.2 does not include:

- revenue/income totals or payment balances;
- charts/trends/conversion analytics;
- exports;
- branch dashboards;
- doctor/provider dashboards;
- real-time push;
- BI integrations;
- scheduled/materialized aggregates;
- AI recommendations;
- Patient Portal dashboard access;
- platform support dashboard impersonation.

Non-blocking follow-ups include naming the treatment-plan metric more precisely and adding drill-down links only through bounded read-model/UX slices.

## 5. Cross-module and architecture result

Documents remains the owner of patient attachment metadata and binary-storage access. It does not become part of ClinicalRecord and does not introduce incidental linkage to Odontogram, Treatments or Billing.

Dashboard remains a read model over accepted roots. It does not own or mutate Patients, Scheduling, Documents, Treatments or Billing.

Tenant time zone is owned by Tenant configuration rather than hidden in Dashboard or supplied by the client.

The modular monolith, backend layer direction, Angular feature boundaries and centralized tenant enforcement remain unchanged.

## 6. Validation evidence

### Documents correction — PR #19 / CI #149

Passed:

- backend restore/build;
- architecture validation;
- backend unit tests;
- backend integration tests;
- frontend dependency install;
- Angular build;
- frontend tests.

Targeted evidence includes valid PDF/JPEG/PNG signatures, mismatch/truncated/empty rejection, size/rewind behavior, spoofed-content no-persistence and save/open/delete root containment.

### Dashboard/time-zone correction — PR #20 / CI #151

Passed:

- backend restore/build;
- architecture validation;
- backend unit tests;
- backend integration tests;
- frontend dependency install;
- Angular build;
- frontend tests;
- generated migration/designer/model snapshot compilation.

Targeted evidence includes tenant default/validation/update, local date differing from UTC, distinct tenant day boundaries, no cross-tenant leakage and blocked platform access without tenant context.

### Closure validation

The Release 7 closure change must run repository-wide CI again after canonical documentation reconciliation.

## 7. Release and MVP decision

**Decision:** Accept Release 7 through Release 7.1 and Release 7.2.

**MVP decision:** With Releases 1 through 7 and the authorization/roles foundation formally accepted, the initial operational MVP is complete.

This is an acceptance of the bounded implemented product, not a claim that every originally imagined advanced capability exists. In particular:

- Billing remains the bounded issued commercial document foundation; payments/balances/receipts/CFDI remain deferred;
- Documents remains a private attachment foundation;
- Dashboard remains a bounded read model;
- doctor/provider views remain deferred;
- automated reminders/providers, online booking, advanced analytics and full Patient Portal remain future work.

## 8. Roadmap consequence

After Release 7 closure:

- latest completed functional release: **Release 7 — Documents and Dashboard**;
- initial operational MVP: **formally accepted**;
- next planned phase: **Phase 2.1 — Patient Intake and Portal Foundation**;
- ADR 006 and issues #2/#4/#5/#6/#7 remain the controlling plan;
- PI-1 is not implemented merely because the MVP gate is satisfied;
- opening PI-1 requires an explicit next-phase decision and resolution of the pending access/bootstrap choices recorded in issue #2.

## 9. Review packet

### Objective

Close Release 7 and the MVP only after resolving the two bounded blockers identified by the module audit.

### Architecture

Module ownership, layered backend, Angular feature structure and modular-monolith direction are preserved.

### Tenant/security

- Documents ownership and binary access remain tenant/patient scoped;
- upload content is validated server-side;
- transport and storage paths are bounded;
- Dashboard remains tenant-scoped/read-only;
- tenant-local day is server-authoritative;
- no new hidden platform bypass exists.

### Compatibility

- Documents hardening preserves routes, DTOs, permissions and storage layout;
- tenant time zone uses an additive migration and additive tenant read property;
- Scheduling storage is not rewritten;
- no closed aggregate is reopened.

### Risk

- Release closure documentation: low runtime risk;
- Documents runtime correction: medium bounded security risk, already validated;
- Tenant time-zone correction: medium-high configuration/date-interpretation risk, already validated;
- future public patient identity remains high risk and outside this closure.

### Recommendation

Accept Release 7 and the MVP, reconcile canonical documentation, keep deferred scope explicit, and treat Phase 2.1 as the next planned but not yet implemented phase.
