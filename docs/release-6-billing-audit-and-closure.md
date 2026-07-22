# Release 6 — Billing Audit and Closure

- **Status:** Closure candidate; canonical release-state reconciliation pending
- **Audit date:** 2026-07-22
- **Scope:** Release 6.1 — Billing Document Foundation
- **Result:** Existing code satisfies the bounded Billing Document foundation
- **Current canonical frontier before closure:** Release 5 — Treatments and Quotes
- **Proposed next planned functional phase after closure:** Release 7 — Documents and Dashboard

## 1. Objective

Audit the existing Billing implementation against the bounded Release 6 roadmap before adding payments, balances, receipts, fiscal behavior or other financial scope.

The audit distinguishes:

1. code presence;
2. satisfaction of a bounded contract;
3. formal release acceptance and documentation alignment.

The repository already contains domain, application, API, persistence, authorization, Angular and automated-test coverage for a `BillingDocument` capability. Until this audit and the pending canonical reconciliation are merged, Billing remains `implemented but not formally accepted/reconciled`.

## 2. Reconciliation result

The current implementation satisfies one bounded slice:

- **Release 6.1 — Billing Document Foundation**.

No new runtime behavior, API contract, migration, permission or frontend feature is required to satisfy this bounded slice.

Formal Release 6 closure still requires the release-state documents to be updated in the same accepted change. This audit therefore records a closure candidate rather than silently advancing `STATE — BigSmile.md` by code presence alone.

## 3. Release 6.1 — Billing Document Foundation

### Accepted behavior supported by current code

- `BillingDocument` is tenant-owned and patient-owned.
- A Billing document references exactly one `TreatmentQuote`.
- Creation is explicit through `POST /api/patients/{patientId}/treatment-plan/quote/billing`.
- `GET /api/patients/{patientId}/treatment-plan/quote/billing` returns `404` when no Billing document exists.
- Reads and status operations do not auto-create Billing data.
- Creation requires an existing quote in the current tenant and patient context.
- The source quote must be `Accepted`.
- The source quote must contain at least one item.
- Every source quote item must have a positive unit price.
- Exactly one Billing document is allowed per `TreatmentQuote` in this slice.
- Billing lines are snapshot copies of accepted quote lines.
- Later upstream quote changes do not alter the Billing snapshot.
- Snapshot lines preserve:
  - source quote item id;
  - title;
  - category;
  - quantity;
  - notes;
  - optional tooth and surface references;
  - unit price;
  - calculated line total.
- The Billing root preserves the quote currency and calculated total.
- Monetary persistence uses SQL precision `decimal(18,2)`.
- Status is bounded to `Draft` and `Issued`.
- The only state transition is `Draft -> Issued`.
- Issue captures UTC timestamp and actor metadata.
- An issued document rejects further status mutations and is read-only in the current slice.
- The Angular workflow is patient-scoped and exposes explicit prerequisites, create and issue operations.

### Evidence

#### Domain

- `backend/src/BigSmile.Domain/Entities/BillingDocument.cs`
- `backend/src/BigSmile.Domain/Entities/BillingDocumentItem.cs`
- `backend/src/BigSmile.Domain/Entities/BillingDocumentStatus.cs`

#### Application

- `backend/src/BigSmile.Application/Features/BillingDocuments/Commands/BillingDocumentCommandService.cs`
- `backend/src/BigSmile.Application/Features/BillingDocuments/Queries/BillingDocumentQueryService.cs`
- `backend/src/BigSmile.Application/Features/BillingDocuments/Dtos/BillingDocumentDtos.cs`
- `backend/src/BigSmile.Application/Features/BillingDocuments/Dtos/BillingDocumentMappings.cs`

#### API

- `backend/src/BigSmile.Api/Controllers/PatientBillingDocumentsController.cs`

#### Persistence

- `backend/src/BigSmile.Infrastructure/Data/Configurations/BillingDocumentConfiguration.cs`
- `backend/src/BigSmile.Infrastructure/Data/Configurations/BillingDocumentItemConfiguration.cs`
- `backend/src/BigSmile.Infrastructure/Data/Repositories/EfBillingDocumentRepository.cs`
- migration `20260422013159_AddBillingFoundation`
- current `AppDbContext` model snapshot

#### Automated tests

- `backend/tests/BigSmile.UnitTests/BillingDocuments/BillingDocumentTests.cs`
- `backend/tests/BigSmile.UnitTests/BillingDocuments/PatientBillingDocumentsControllerTests.cs`
- `backend/tests/BigSmile.IntegrationTests/BillingDocuments/BillingDocumentServicesTests.cs`
- `frontend/src/app/features/billing/pages/billing-document.page.spec.ts`
- guarded-route coverage in `frontend/src/app/app.routes.spec.ts`

#### Frontend

- Billing models, API data-access and facade
- patient-context Billing route
- no-quote state
- quote-not-accepted state
- explicit missing/create state
- snapshot line list and total
- issue action and issued read-only state

## 4. Domain and invariant review

### Ownership

The root requires non-empty tenant, patient and quote identifiers and implements `ITenantOwnedEntity`.

`TenantId`, `PatientId` and `TreatmentQuoteId` are immutable through the exposed domain behavior. Billing items are created only inside the root constructor and remain child records of the Billing aggregate.

### Accepted-quote prerequisite

The cross-aggregate rule is coordinated in the Application layer:

1. resolve the Patient through the tenant-filtered repository;
2. resolve the TreatmentQuote through the current patient path;
3. require `TreatmentQuoteStatus.Accepted`;
4. require a non-empty item snapshot;
5. construct the Billing root from the accepted quote.

The `BillingDocument` constructor independently rejects empty snapshots, duplicate source item references and non-positive source prices.

Keeping the accepted-quote check in the application use case is acceptable because it is a cross-aggregate invariant. It must not be bypassed by future import, support or platform paths.

### Snapshot behavior

The Billing root copies quote values rather than holding mutable references as its financial representation. It calculates line totals at copy time and stores a root total.

Tests confirm that later quote-price changes do not resynchronize the Billing document.

### Lifecycle

The bounded lifecycle is:

```text
Draft -> Issued
```

Issue records:

- `Status = Issued`;
- `IssuedAtUtc`;
- `IssuedByUserId`;
- `LastUpdatedAtUtc`;
- `LastUpdatedByUserId`.

Once issued, the current aggregate exposes no line mutation and rejects further status changes.

## 5. API contract review

### Contracts

- `GET /api/patients/{patientId}/treatment-plan/quote/billing`
- `POST /api/patients/{patientId}/treatment-plan/quote/billing`
- `PUT /api/patients/{patientId}/treatment-plan/quote/billing/status`

### Contract constraints

- Requests do not accept `TenantId`.
- Requests do not accept owner or actor identifiers.
- Creation has no client-supplied quote id or financial lines.
- The quote, currency, snapshot and actor are resolved server-side.
- Status input is bounded to the Billing status enum.
- A missing Billing root produces `404` for reads and status operations.
- Invalid prerequisites or transitions use the existing validation-problem contract.

### Separation from payments

There is no endpoint in this slice for:

- payment registration;
- payment allocation;
- balance mutation;
- receipt generation;
- refund or reversal;
- tax or CFDI operations.

A Billing document is therefore an issued commercial snapshot, not a payment ledger or fiscal invoice.

## 6. Persistence review

### Confirmed model

- `BillingDocuments` stores tenant, patient and quote ownership.
- `BillingDocumentItems` stores immutable snapshot lines.
- `TreatmentQuoteId` has a unique index.
- `PatientId` and `TenantId` are indexed.
- Root and lines use explicit monetary precision `decimal(18,2)`.
- Quote and Patient deletes are restricted while tenant deletion cascades according to the existing tenancy model.
- Billing-line deletion cascades with the root.

### Tenant filter and writes

`AppDbContext` applies the global tenant query filter to `BillingDocument` and validates tenant-owned writes in `SaveChanges`.

Billing items do not carry their own `TenantId`; accepted paths consume them through the tenant-owned Billing root.

Any future direct query over `BillingDocumentItems` must include an explicit tenant-aware join or remodel the child as tenant-owned.

### Relationship integrity

The supported application path obtains Patient and quote through tenant-filtered repositories and does not accept arbitrary ownership identifiers from the request.

The relational model does not use composite tenant foreign keys across Patient, quote and Billing. Future bulk import, platform support or bypass code must preserve `TenantId + PatientId + TreatmentQuoteId` consistency explicitly instead of constructing roots from arbitrary identifiers.

## 7. Authorization review

### Permissions

- Billing reads require `billing.read`.
- Billing writes require `billing.write`.

### Current role mapping

- `PlatformAdmin` receives Billing permissions.
- `TenantAdmin` receives Billing permissions.
- `TenantUser` does not receive Billing permissions.

The Billing policies do not activate platform override. Patient-scoped Billing services require a resolved tenant context, so a platform-scoped request without a tenant context is blocked.

This audit does not change roles, permissions, policies or platform-override behavior.

## 8. Tenant-safety review

### Confirmed controls

- `BillingDocument` implements tenant ownership.
- `AppDbContext` applies a global tenant filter to Billing roots.
- `SaveChanges` blocks mismatched tenant-owned writes.
- command/query services require a resolved tenant context;
- Patient is resolved inside the current tenant before each operation;
- quote lookup follows the current tenant/patient path;
- tenant and actor come from server context;
- cross-tenant reads return unavailable/`null` behavior;
- cross-tenant create and issue operations are rejected;
- platform scope without a resolved tenant is blocked.

### Result

**Tenant-safe for the accepted Release 6.1 application and API paths.**

## 9. Frontend review

### Confirmed structure

- The Billing route is lazy-loaded and permission-guarded.
- HTTP remains in `features/billing/data-access`.
- state and orchestration remain in `BillingDocumentsFacade` and the route page.
- UI covers loading, missing, error and saving states.
- A missing quote is presented separately from a non-accepted quote.
- Billing creation is available only after the displayed quote is accepted.
- Billing creation remains an explicit action.
- Lines, currency and total are shown from the Billing snapshot.
- Issue is explicit and permission-aware.
- Issued state removes the issue action and presents read-only behavior.

### Non-blocking UX debt

- visible `Release 6.1`, `foundation` and `slice` terminology should be replaced with clinic-facing language;
- raw user and source ids should eventually use safe display names or a dedicated detail affordance;
- residual hardcoded colors should migrate to `--bsm-*` semantic tokens when the screen is touched;
- generic mutation errors can later expose safe validation details;
- a confirmation step for irreversible issue may be evaluated through a bounded UX slice.

These items do not block the functional foundation.

## 10. Automated evidence

Existing tests verify:

- explicit create/get in tenant scope;
- missing Billing returns `null`/`404` behavior;
- no creation when a quote is missing;
- no creation when the quote is not accepted;
- no creation when the source quote has no lines;
- duplicate Billing creation is rejected;
- snapshot line values and total calculation;
- Billing remains independent from later quote-price changes;
- `Draft -> Issued` and issue metadata;
- issued read-only behavior;
- cross-tenant read denial;
- cross-tenant create denial;
- cross-tenant issue denial;
- platform scope without tenant context is blocked;
- controller create/read/status result contracts;
- frontend prerequisite, missing, create, line, total, issue and read-only states.

The closure change must also pass repository-wide CI:

- backend restore/build;
- architecture validation;
- backend unit tests;
- backend integration tests;
- frontend dependency install;
- Angular build;
- frontend tests.

## 11. Accepted Release 6 boundary proposed by the audit

Release 6 can close with:

- **Release 6.1 — Billing Document Foundation**.

This is sufficient for a foundational commercial-record step between accepted quote and future payment workflows.

It does not mean the clinic can yet register payments, calculate balances or issue fiscal invoices.

## 12. Explicitly deferred scope

The following remain outside Release 6.1:

- payment registration or allocation;
- partial and total payment lifecycle;
- balance or ledger calculation;
- receipts;
- refunds, reversals or cancellations;
- cash sessions and daily cash closing;
- taxes and discounts;
- CFDI/PAC or electronic invoicing;
- insurance claims;
- multi-currency behavior;
- accounting or ERP workflows;
- automatic status synchronization that mutates an accepted quote;
- multiple Billing documents per quote;
- Billing regeneration or versioning;
- Patient Portal access to Billing.

These capabilities require dedicated later slices and must not be inferred from this foundation.

## 13. Non-blocking hardening follow-ups

The audit identified bounded improvements that do not justify delaying the current foundation:

- normalize the unique-index race during concurrent create into a stable conflict response if demonstrated by real concurrent use;
- add optimistic concurrency before multiple operational roles can issue the same document concurrently;
- decide whether repeated `Issued` requests should remain a validation error or become an explicit idempotent no-op;
- add relational-provider coverage for unique and precision behavior when the CI database strategy supports it;
- add database check constraints only if direct/bulk ingestion paths are introduced;
- replace raw actor ids in UI without weakening audit attribution;
- introduce a dedicated payment aggregate rather than adding mutable payment fields to `BillingDocument`.

Current automated integration tests use the repository's existing in-memory strategy and therefore do not prove SQL Server race behavior. The migration and EF model still provide the intended unique and precision constraints.

## 14. Closure decision candidate

**Candidate decision:** Close Release 6 — Billing through Release 6.1 — Billing Document Foundation.

**Reason:** The bounded roadmap contract is implemented across domain, application, API, persistence, authorization, frontend and automated tests. Remaining work belongs to payments, cash, fiscal or hardening slices.

**Condition:** Update `STATE — BigSmile.md`, roadmap and base repository documentation in the same accepted closure PR before marking Release 6 completed.

**Consequence after acceptance:** Release 7 — Documents and Dashboard becomes the next planned functional phase. Patient Intake and Portal remains planned after MVP completion under ADR 006.

## 15. Review packet

### Objective

Audit existing Billing code before adding financial scope.

### Runtime files changed

None in this audit slice.

### Architecture

The existing Billing root remains separate from TreatmentQuote and future Payment aggregates. Layered backend and feature-based frontend ownership are preserved.

### Tenant/security

Tenant ownership, centralized filtering, server-derived context, permission policies and cross-tenant protections remain unchanged.

### Validation

Existing targeted tests plus repository-wide CI for this documentation PR.

### Risk

- Runtime risk: low because this audit changes no runtime behavior.
- Product-governance risk: medium because accepting the candidate would advance the canonical roadmap frontier.
- Future financial risk: high if payments, balances or fiscal behavior are later added without dedicated domain and audit boundaries.

### Recommendation

Accept the Release 6.1 foundation only with aligned canonical documentation, preserve all deferred financial scope, and open Release 7 through a separate audit rather than extending Billing opportunistically.
