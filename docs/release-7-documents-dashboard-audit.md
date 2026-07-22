# Release 7 — Documents and Dashboard Audit

- **Status:** Audit complete; release closure blocked by bounded gaps
- **Audit date:** 2026-07-22
- **Scope:** Release 7.1 — Patient Documents Foundation; Release 7.2 — Dashboard Read Model Foundation
- **Current canonical frontier:** Release 6 — Billing
- **Tracking:** issue #15
- **Blocking follow-ups:** issues #16 and #17

## 1. Objective

Audit the existing Documents and Dashboard implementation before accepting or adding Release 7 functionality.

The audit distinguishes:

1. code presence;
2. satisfaction of a bounded contract;
3. release-blocking gaps;
4. formal release acceptance.

Release 7 is the final planned MVP release before the normal Phase 2.1 Patient Intake and Portal gate. This audit must therefore remain conservative: existing code is reused where valid, but sensitive-file and operational-date gaps are not accepted silently.

## 2. Reconciliation result

The repository contains substantial, coherent implementations for both planned slices, but neither slice is ready for formal acceptance without one bounded correction:

| Slice | Classification | Blocking gap |
| --- | --- | --- |
| Release 7.1 — Patient Documents Foundation | Partially satisfied | Actual binary format is not verified; the server trusts the client-declared MIME type. Multipart parsing is not explicitly bounded near the 10 MB product limit. |
| Release 7.2 — Dashboard Read Model Foundation | Partially satisfied | “Today” is calculated from the UTC date, while Tenant has no server-authoritative clinic time zone. |

No broad rewrite is justified.

Recommended sequence:

1. implement issue #16 — document upload hardening;
2. implement issue #17 — tenant time-zone foundation and dashboard day boundary;
3. rerun the complete Release 7 audit;
4. accept 7.1 and 7.2 only if both bounded contracts and repository-wide CI are green;
5. reconcile STATE, roadmap and base documentation in the closure PR;
6. determine explicitly whether the initial MVP is then formally accepted.

## 3. Release 7.1 — Patient Documents Foundation

### 3.1 Existing behavior that satisfies the candidate contract

#### Domain and metadata

- `PatientDocument` is tenant-owned and patient-owned.
- Upload metadata contains:
  - original file name;
  - content type;
  - size;
  - server storage key;
  - upload UTC timestamp and actor;
  - optional logical-retire UTC timestamp and actor.
- Declared content types are bounded to:
  - `application/pdf`;
  - `image/jpeg`;
  - `image/png`.
- The domain file-size limit is 10 MB.
- Logical retire is explicit and does not change tenant or patient ownership.
- Repeated retire is rejected.

Evidence:

- `backend/src/BigSmile.Domain/Entities/PatientDocument.cs`.
- `backend/tests/BigSmile.UnitTests/Documents/PatientDocumentTests.cs`.

#### Explicit application workflow

- Upload requires a resolved actor and an available Patient.
- Normal tenant flow verifies that the Patient belongs to the current tenant.
- Explicit platform override can resolve the execution tenant from the Patient.
- Original file names are reduced with `Path.GetFileName`; client path segments are not persisted.
- The storage key is generated server-side from tenant, patient, random GUID and an extension derived from the declared allowlisted content type.
- Binary content is saved before metadata.
- If metadata persistence fails, the application attempts to remove the just-written binary.
- Retire updates metadata only; normal active list/download paths stop exposing the retired document.

Evidence:

- `backend/src/BigSmile.Application/Features/PatientDocuments/Commands/PatientDocumentCommandService.cs`.

#### Query behavior

- Active documents are returned newest-first.
- List and download first verify that the Patient is available through the current tenant/platform path.
- Download resolves metadata by `PatientId + DocumentId` and uses only the internal storage key.
- Retired records return unavailable through the normal flow.
- Missing binary content is surfaced as an explicit server-side integrity error rather than an empty file.

Evidence:

- `backend/src/BigSmile.Application/Features/PatientDocuments/Queries/PatientDocumentQueryService.cs`.
- `backend/src/BigSmile.Infrastructure/Data/Repositories/EfPatientDocumentRepository.cs`.

#### Private local storage

- The default root is outside `wwwroot`: `App_Data/PatientDocuments` under the API content root.
- Files use `FileMode.CreateNew`.
- The binary store normalizes the requested path with `Path.GetFullPath`.
- The resolved path must equal or remain below the configured root.
- A traversal key resolving outside the root is rejected.
- Binary open/delete operations use the same root-containment rule.

Evidence:

- `backend/src/BigSmile.Infrastructure/Services/LocalPatientDocumentBinaryStore.cs`.
- `backend/src/BigSmile.Infrastructure/Options/PatientDocumentStorageOptions.cs`.

#### API contracts

- `GET /api/patients/{patientId}/documents` — active list.
- `POST /api/patients/{patientId}/documents` — multipart upload.
- `GET /api/patients/{patientId}/documents/{documentId}/download` — authorized download.
- `DELETE /api/patients/{patientId}/documents/{documentId}` — logical retire.
- Requests do not accept `TenantId`, actor id, storage key or storage path.
- Download returns the recorded original file name and content type.
- Missing Patient/document behavior is `404` through the current controller contract.

Evidence:

- `backend/src/BigSmile.Api/Controllers/PatientDocumentsController.cs`.

#### Persistence

- `PatientDocuments` stores tenant and patient ownership.
- `StorageKey` is unique.
- `TenantId + PatientId` and `PatientId` are indexed.
- Patient delete is restricted; tenant delete cascades according to the current tenancy model.
- The root has the centralized tenant query filter and tenant-owned write enforcement.

Evidence:

- `backend/src/BigSmile.Infrastructure/Data/Configurations/PatientDocumentConfiguration.cs`.
- migration `20260422043017_AddPatientDocumentsFoundation`.
- `backend/src/BigSmile.Infrastructure/Data/AppDbContext.cs`.

#### Authorization and tenant safety

- Reads require `document.read`.
- Writes require `document.write`.
- `PlatformAdmin` and `TenantAdmin` receive document permissions.
- `TenantUser` does not receive document permissions.
- Document policies enable platform override only through the explicit patient route value.
- Normal cross-tenant metadata and binary access returns unavailable or is rejected.
- Platform override upload/list/download/retire has integration coverage.

Evidence:

- `backend/src/BigSmile.Application/Authorization/Permissions.cs`.
- `backend/src/BigSmile.Application/Authorization/RolePermissionCatalog.cs`.
- `backend/src/BigSmile.Api/Authorization/AuthorizationPolicies.cs`.
- `backend/tests/BigSmile.IntegrationTests/PatientDocuments/PatientDocumentServicesTests.cs`.

#### Angular flow

- The route is lazy-loaded and permission-guarded.
- HTTP remains in `features/documents/data-access`.
- State and orchestration remain in `PatientDocumentsFacade` and the route page.
- The UI supports:
  - loading/error states;
  - empty state;
  - explicit upload;
  - active list;
  - download;
  - permission-aware retire;
  - per-operation saving state.
- The file input advertises PDF/JPG/PNG and 10 MB.

Evidence:

- `frontend/src/app/features/documents`.
- route coverage in `frontend/src/app/app.routes.spec.ts`.

### 3.2 Release-blocking gap: declared MIME is trusted

The current implementation validates only the client-supplied `ContentType` and file length.

It does not inspect the file header or another server-observed format marker before storage. Existing integration tests demonstrate this directly: arbitrary text such as `pdf-binary` is accepted when the command declares `application/pdf`.

Consequences:

- the PDF/JPG/PNG allowlist is not an actual content allowlist;
- an arbitrary binary can be stored by spoofing an accepted MIME type;
- the server can later return an incorrect content type for that binary;
- the accepted security boundary would be weaker than the user-visible and roadmap claim.

**Classification:** release-blocking for 7.1.

**Required bounded correction:** issue #16.

The correction should verify a minimal server-side signature before persistence:

- PDF `%PDF-`;
- JPEG `FF D8 FF`;
- PNG `89 50 4E 47 0D 0A 1A 0A`.

The input stream must be reset/preserved so the existing binary store receives the complete content.

### 3.3 Release-blocking gap: multipart transport limit

The authoritative domain limit is 10 MB, but the upload endpoint has no explicit request/form multipart limit near that value. The file-size invariant is evaluated after multipart model binding has produced an `IFormFile`.

This still rejects an oversized file from the business use case, but it does not bound request parsing as tightly as the advertised product limit.

**Classification:** bounded hardening required in the same 7.1 correction.

A request/form limit should allow the 10 MB file plus controlled multipart overhead while preserving `PatientDocument.MaxFileSizeBytes` as the file-level authority.

### 3.4 Non-blocking Documents follow-ups

The following are valid but do not block the bounded foundation after issue #16:

- explicit root-containment tests for save/open/delete;
- retire confirmation in the UI;
- replace internal `Release 7.1`, `foundation` and `slice` copy;
- replace raw uploader ids with safe display names or detail affordance;
- migrate residual hardcoded colors to `--bsm-*` tokens;
- preserve the original exception if cleanup after metadata failure also fails;
- malware/antivirus scanning only through a future provider/infrastructure decision;
- define retention and physical-delete policy in a later operational/privacy slice.

### 3.5 Scope that remains outside Release 7.1

- OCR;
- rich document preview;
- document versioning;
- public/external sharing;
- generated PDFs/templates;
- electronic signatures and advanced consent;
- advanced imaging viewer;
- antivirus-provider integration;
- Patient Portal document access.

## 4. Release 7.2 — Dashboard Read Model Foundation

### 4.1 Existing behavior that satisfies the candidate contract

#### Read-only contract

- `GET /api/dashboard/summary` is the only Dashboard API path in this slice.
- The controller exposes no mutation.
- The use case returns one bounded summary DTO.

Metrics:

- active patients;
- appointments whose start falls in the calculated current day;
- those appointments still in `Scheduled` status;
- active/non-retired documents;
- existing treatment plans;
- accepted treatment quotes;
- issued Billing documents;
- generated-at UTC.

Evidence:

- `backend/src/BigSmile.Application/Features/Dashboard/Dtos/DashboardSummaryDto.cs`.
- `backend/src/BigSmile.Api/Controllers/DashboardController.cs`.

#### Tenant resolution and authorization

- The query service requires a resolved `TenantId`.
- Dashboard requires `dashboard.read`.
- The policy also requires resolved tenant context.
- Platform scope without a tenant is blocked.
- Platform override without a resolved tenant remains blocked.
- The current conservative role catalog grants Dashboard to `TenantAdmin`; it does not expand `TenantUser`.

Evidence:

- `backend/src/BigSmile.Application/Features/Dashboard/Queries/DashboardSummaryQueryService.cs`.
- authorization catalog/policies.
- `backend/tests/BigSmile.IntegrationTests/Dashboard/DashboardSummaryQueryServiceTests.cs`.

#### Read-model queries

Every query explicitly includes the current `tenantId`, even though tenant-owned root filters also exist.

Current metric semantics are explicit:

- active patients: `Patient.IsActive`;
- today appointments: all appointment states whose start is in the range;
- today pending: `AppointmentStatus.Scheduled` in the range;
- active documents: `DeletedAtUtc == null`;
- treatment plans: every existing plan, because archive/versioning is outside Release 5;
- accepted quotes: `TreatmentQuoteStatus.Accepted`;
- issued Billing: `BillingDocumentStatus.Issued`.

The queries use accepted aggregate roots rather than unfiltered child-table reads.

Evidence:

- `backend/src/BigSmile.Infrastructure/Data/Repositories/EfDashboardSummaryRepository.cs`.

#### Automated evidence

Tests verify:

- exact tenant-scoped counts;
- exclusion of data from a second tenant;
- the current UTC-date appointment boundary;
- scheduled-only pending count;
- active/non-retired document count;
- accepted quote count;
- issued Billing count;
- blocked platform access without tenant context.

Evidence:

- `backend/tests/BigSmile.IntegrationTests/Dashboard/DashboardSummaryQueryServiceTests.cs`.
- `backend/tests/BigSmile.UnitTests/Dashboard/DashboardControllerTests.cs`.

#### Angular flow

- The route is lazy-loaded and permission-guarded.
- HTTP remains in Dashboard data-access.
- `DashboardFacade` owns loading, summary and error state.
- The page renders loading skeletons, error feedback, summary cards and a bounded empty state.
- Card helper copy documents the current metric meanings.

Evidence:

- `frontend/src/app/features/dashboard`.
- route coverage in `frontend/src/app/app.routes.spec.ts`.

### 4.2 Release-blocking gap: UTC is presented as clinic “Today”

`DashboardSummaryQueryService` currently does:

```text
generatedAtUtc = DateTime.UtcNow
todayStartUtc = generatedAtUtc.Date
tomorrowStartUtc = todayStartUtc + 1 day
```

The frontend labels the resulting metrics `Today appointments` and `Today pending`.

The Tenant model has no `TimeZoneId`. Appointment calendar ranges elsewhere are based on clinic wall-clock `DateOnly` values, not on a universal UTC calendar day.

For a clinic outside UTC, the dashboard's date can advance before the clinic's local day ends. This is operationally incorrect and can cause reception to see tomorrow's appointments as “today” while the current local day is still active.

**Classification:** release-blocking for 7.2.

**Required bounded correction:** issue #17.

### 4.3 Alternative analysis

#### Accept UTC semantics and change copy

Advantages:

- no migration;
- current implementation already tested.

Disadvantages:

- “UTC today” is not a meaningful operational clinic dashboard;
- it conflicts with the product goal of fast local operations;
- it postpones the same problem to reminders, booking, reports and jobs.

**Decision:** rejected.

#### Trust browser local date/time zone

Advantages:

- small server change;
- no tenant migration.

Disadvantages:

- client-controlled and inconsistent across devices;
- unusable for background jobs or non-browser clients;
- not a tenant source of truth.

**Decision:** rejected.

#### Use one application-global time zone

Advantages:

- works for one pilot clinic;
- small configuration change.

Disadvantages:

- introduces a single-clinic shortcut into a multi-tenant product;
- prevents tenants in other regions from receiving correct operational dates.

**Decision:** rejected.

#### Add tenant-owned `TimeZoneId`

Advantages:

- server-authoritative;
- consistent across users/devices;
- aligns with tenant configuration ownership;
- reusable by future reminders, booking, reporting and jobs;
- does not require rewriting appointment storage in this slice.

Trade-offs:

- additive domain/configuration/API/migration work;
- existing tenants need a documented default;
- invalid/platform-unsupported identifiers need deterministic validation.

**Decision direction:** recommended through issue #17.

### 4.4 Non-blocking Dashboard follow-ups

After the tenant date boundary is corrected, the following remain non-blocking:

- rename `ActiveTreatmentPlansCount` because the current metric counts all existing plans; UI copy already says `Treatment plans`;
- decide later whether cancelled appointments should be excluded from the total-today card; current tests intentionally count all states and pending is separate;
- add drill-down links only through bounded UX/read-model slices;
- branch-specific and doctor-specific dashboards require separate scope;
- charts, trends, conversion metrics and exports remain advanced analytics;
- platform support dashboard access requires an explicit tenant-selection/support use case rather than a hidden override.

### 4.5 Scope outside Release 7.2

- charts and trends;
- revenue/income totals or payment balances;
- conversion analytics;
- exports;
- branch/doctor dashboards;
- real-time push;
- BI integrations;
- scheduled materialized aggregates;
- AI recommendations;
- Patient Portal dashboard access.

## 5. Cross-module and architecture review

### Documents

- Owns metadata and binary-storage access for patient attachments.
- Does not become part of the ClinicalRecord aggregate.
- Does not expose public storage URLs.
- Does not add document linkage to Odontogram, Treatments or Billing.

### Dashboard

- Remains a read model over accepted root modules.
- Does not own Patients, Scheduling, Documents, Treatments or Billing state.
- Does not issue writes or synchronize statuses.

### Time zone

Tenant-owned time zone is a cross-cutting operational setting. It should be implemented as a small foundation rather than hidden inside Dashboard or supplied by the browser.

No microservice, external provider or distributed job infrastructure is justified.

## 6. Tenant and security review

### Confirmed

- PatientDocument root is tenant-owned and globally filtered.
- Document metadata reads/writes are patient-scoped.
- Binary access is reachable only after authorized metadata lookup.
- Storage keys are server-generated and root-contained.
- Cross-tenant document metadata and binary operations are covered.
- Dashboard requires a resolved tenant and every count is tenant-scoped.
- Dashboard has no write path.

### Pending before closure

- actual uploaded format must be verified server-side;
- multipart parsing must be explicitly bounded;
- Dashboard current day must be derived from a server-authoritative tenant time zone.

## 7. Validation status

### Existing evidence reviewed

- Documents domain tests.
- Documents controller tests.
- Documents service/storage integration tests.
- Documents frontend page/route tests.
- Dashboard controller tests.
- Dashboard tenant/read-model integration tests.
- Dashboard frontend page/card/route tests.
- Release 6 CI baseline before opening this audit.

### Validation not yet possible

Release 7 closure CI cannot be claimed because the blocking changes in #16 and #17 are not implemented.

Each implementation PR must run:

- backend restore/build;
- architecture validation;
- backend unit tests;
- backend integration tests;
- frontend install/build/tests;
- migration validation for the time-zone change;
- targeted binary signature/storage tests.

## 8. Release decision

**Decision:** Do not close Release 7 yet.

**Reason:** Both existing modules are structurally strong, but accepting a spoofable file allowlist and an incorrect clinic-day boundary would create security and operational defects in the final MVP release.

**Required work:**

- #16 — harden patient document upload validation;
- #17 — add tenant time-zone foundation and correct Dashboard “Today”.

**Consequence:**

- Release 6 remains the latest completed release;
- Release 7 remains active as the current audit/implementation phase;
- Phase 2.1 Patient Intake and Portal remains planned and unopened;
- no broad rewrite is approved;
- after #16 and #17, the release must receive a focused re-audit and closure PR.

## 9. Review packet

### Objective

Audit existing Documents and Dashboard code without mistaking code presence for release acceptance.

### Runtime files changed

None in this audit document.

### Architecture

Existing module ownership and layered/feature-based structure are preserved. The proposed corrections are bounded to binary-input validation and tenant operational time.

### Tenant/security

Current tenant controls are strong. Release closure is blocked specifically to avoid accepting a MIME-spoofing upload path and client-independent clinic-date ambiguity.

### Risk

- Documents gap: high security sensitivity, medium bounded implementation risk.
- Dashboard time-zone gap: medium-high operational/data-interpretation risk.
- Audit documentation: low runtime risk.

### Recommendation

Implement #16 first because it is local and does not require a migration. Implement #17 in a separate PR because it changes tenant configuration and persisted data. Then re-audit and close Release 7 only with explicit evidence.
