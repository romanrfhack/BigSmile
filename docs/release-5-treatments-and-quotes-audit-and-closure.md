# Release 5 — Treatments and Quotes Audit and Closure

- **Status:** Accepted closure evidence
- **Audit date:** 2026-07-22
- **Scope:** Release 5.1 and Release 5.2
- **Result:** Release 5 can close as the foundational treatment-planning and quote release
- **Next planned functional phase:** Release 6 — Billing

## 1. Objective

Audit the existing Treatment Plans and Treatment Quotes implementation against the bounded Release 5 roadmap before adding or rewriting functionality.

The audit distinguishes:

1. code presence;
2. satisfaction of a bounded contract;
3. formal release acceptance.

The repository already contained domain, application, API, persistence, authorization, Angular and automated-test coverage for Treatments/Quotes. Until this audit, that code correctly remained classified as `implemented but not formally accepted/reconciled`.

## 2. Reconciliation result

The current implementation satisfies the foundational Release 5 contract through:

1. **Release 5.1 — Treatment Plan Foundation**;
2. **Release 5.2 — Quote Foundation**.

No new backend behavior, API contract, migration, permission or frontend feature is required to accept these slices. Release closure is a documentation and product-state reconciliation over existing tested code.

## 3. Release 5.1 — Treatment Plan Foundation

### Accepted behavior

- `TreatmentPlan` is tenant-owned and patient-owned.
- The current slice allows exactly one treatment plan per `TenantId + PatientId`.
  - This is stricter than “one active plan” because archive/versioning is not implemented.
  - Additional or archived plans remain future scope.
- Creation is explicit through `POST /api/patients/{patientId}/treatment-plan`.
- `GET /api/patients/{patientId}/treatment-plan` returns `404` when missing.
- Reads, item operations and status operations do not auto-create a plan.
- A plan starts in `Draft`.
- Basic items support:
  - required normalized title, maximum 200 characters;
  - optional category, maximum 100 characters;
  - positive integer quantity;
  - optional short notes, maximum 500 characters;
  - optional permanent-adult FDI tooth code;
  - optional `O/M/D/B/L` surface code that requires a tooth code.
- Item add and remove are explicit.
- Status is bounded to `Draft`, `Proposed` and `Accepted`.
- Allowed transitions are:
  - `Draft -> Proposed`;
  - `Proposed -> Draft`;
  - `Proposed -> Accepted`.
- `Accepted` plans are read-only for item mutations and status changes.
- `Draft` and `Proposed` plans remain editable in this bounded implementation.
- Aggregate and item metadata preserve UTC timestamps and actor attribution.

### Evidence

- Domain:
  - `backend/src/BigSmile.Domain/Entities/TreatmentPlan.cs`;
  - `TreatmentPlanItem.cs`;
  - `TreatmentPlanStatus.cs`.
- Application:
  - `backend/src/BigSmile.Application/Features/TreatmentPlans/Commands/TreatmentPlanCommandService.cs`;
  - `Queries/TreatmentPlanQueryService.cs`;
  - DTOs and mappings.
- API:
  - `backend/src/BigSmile.Api/Controllers/PatientTreatmentPlansController.cs`.
- Persistence:
  - `TreatmentPlanConfiguration`;
  - `TreatmentPlanItemConfiguration`;
  - `EfTreatmentPlanRepository`;
  - migration `20260421055439_AddTreatmentPlanFoundation`.
- Tests:
  - `TreatmentPlanTests`;
  - `TreatmentPlanServicesTests`;
  - `PatientTreatmentPlansControllerTests`;
  - Angular `treatment-plan.page.spec.ts`.
- Frontend:
  - treatment-plan models, API service and facade;
  - explicit missing/create state;
  - item form/list;
  - status editor;
  - patient-context route.

## 4. Release 5.2 — Quote Foundation

### Accepted behavior

- `TreatmentQuote` is tenant-owned and patient-owned.
- A quote references an existing treatment plan.
- Quote creation requires the source plan to contain at least one item.
- Creation is explicit through `POST /api/patients/{patientId}/treatment-plan/quote`.
- `GET /api/patients/{patientId}/treatment-plan/quote` returns `404` when missing.
- Reads, price operations and status operations do not auto-create a quote or plan.
- Exactly one quote is allowed per treatment plan in this slice.
- Quote items are snapshot copies of the treatment-plan items at creation time.
- Later plan item changes do not mutate the quote snapshot.
- Snapshot lines preserve:
  - source treatment-plan item id;
  - title;
  - category;
  - quantity;
  - notes;
  - optional tooth and surface references.
- The accepted public application/API path fixes currency to `MXN`.
  - Currency is not supplied by the request contract.
  - Future multi-currency behavior requires a dedicated slice.
- Each line exposes `UnitPrice` and calculated `LineTotal`.
- The quote exposes a calculated total.
- Unit prices use SQL precision `decimal(18,2)`.
- Draft prices may be zero while pricing is incomplete; negative prices are rejected.
- Status is bounded to `Draft`, `Proposed` and `Accepted`.
- Allowed transitions are:
  - `Draft -> Proposed`;
  - `Proposed -> Draft`;
  - `Proposed -> Accepted`.
- Every line must have a positive price before `Draft -> Proposed`.
- A proposed quote cannot be changed so that any line becomes non-positive.
- Positive pricing is revalidated on `Proposed -> Accepted`.
- `Accepted` quotes are read-only.
- There is no regenerate, replacement or multi-quote negotiation flow.

### Evidence

- Domain:
  - `backend/src/BigSmile.Domain/Entities/TreatmentQuote.cs`;
  - `TreatmentQuoteItem.cs`;
  - `TreatmentQuoteStatus.cs`.
- Application:
  - `backend/src/BigSmile.Application/Features/TreatmentQuotes/Commands/TreatmentQuoteCommandService.cs`;
  - `Queries/TreatmentQuoteQueryService.cs`;
  - DTOs and mappings.
- API:
  - `backend/src/BigSmile.Api/Controllers/PatientTreatmentQuotesController.cs`.
- Persistence:
  - `TreatmentQuoteConfiguration`;
  - `TreatmentQuoteItemConfiguration`;
  - `EfTreatmentQuoteRepository`;
  - migration `20260421180553_AddTreatmentQuoteBasics`.
- Tests:
  - `TreatmentQuoteTests`;
  - `TreatmentQuoteServicesTests`;
  - `PatientTreatmentQuotesControllerTests`;
  - Angular `treatment-quote.page.spec.ts`.
- Frontend:
  - quote models, API service and facade;
  - no-plan and missing-quote states;
  - explicit snapshot creation;
  - price editor;
  - status editor;
  - total display;
  - patient-context route.

## 5. API contract review

### Treatment plan

- `GET /api/patients/{patientId}/treatment-plan`.
- `POST /api/patients/{patientId}/treatment-plan`.
- `POST /api/patients/{patientId}/treatment-plan/items`.
- `DELETE /api/patients/{patientId}/treatment-plan/items/{itemId}`.
- `PUT /api/patients/{patientId}/treatment-plan/status`.

### Treatment quote

- `GET /api/patients/{patientId}/treatment-plan/quote`.
- `POST /api/patients/{patientId}/treatment-plan/quote`.
- `PUT /api/patients/{patientId}/treatment-plan/quote/items/{quoteItemId}/price`.
- `PUT /api/patients/{patientId}/treatment-plan/quote/status`.

### Contract constraints

- Requests do not accept `TenantId`, owner id or actor id as authority.
- Patient ownership is resolved server-side inside the current tenant.
- Invalid item/status/price/dental-location values are rejected by backend validation/domain rules.
- Missing roots produce `404` through the controller contracts.

## 6. Authorization review

### Permissions

- Treatment plan reads require `treatmentplan.read`.
- Treatment plan writes require `treatmentplan.write`.
- Quote reads require `treatmentquote.read`.
- Quote writes require `treatmentquote.write`.

### Current role mapping

- `PlatformAdmin` receives treatment-plan and quote permissions.
- `TenantAdmin` receives treatment-plan and quote permissions.
- `TenantUser` does not receive treatment-plan or quote permissions.

This closure preserves the current conservative access model. It does not introduce Dentist, FrontDesk or other new role semantics. Any broader operational-role mapping requires an explicit authorization slice.

Patient-scoped Treatment operations require a resolved tenant context. Platform scope without a resolved tenant path does not silently bypass tenant isolation.

No permission, policy or platform-override behavior changes in this closure.

## 7. Tenant-safety review

### Confirmed controls

- `TreatmentPlan` and `TreatmentQuote` implement tenant ownership.
- `AppDbContext` applies global tenant query filters to both aggregate roots.
- `SaveChanges` validates tenant-owned writes against the resolved tenant context.
- Command/query services require a resolved tenant context.
- The Patient is resolved through the tenant-filtered repository before each operation.
- Plan and quote creation derive `TenantId` and actor from server context.
- Cross-tenant reads return unavailable/`null` behavior.
- Cross-tenant writes are rejected before mutation.
- Platform-scoped requests without tenant context are blocked in patient-scoped services.

### Child-table rule

`TreatmentPlanItem` and `TreatmentQuoteItem` do not carry their own `TenantId`; accepted reads/writes consume them through their tenant-owned aggregate roots.

Any future direct query over those child tables must use an explicit tenant-aware join or remodel the child as tenant-owned.

### Relationship-integrity rule

The accepted application paths validate the Patient in the current tenant and obtain the source TreatmentPlan through tenant-filtered repositories. Future internal/platform import paths must preserve the same tenant/patient/plan consistency instead of constructing roots from arbitrary identifiers.

### Result

**Tenant-safe for the accepted Release 5 application and API paths.**

## 8. Domain-boundary review

Treatment item dental references reuse the accepted permanent-adult FDI and `O/M/D/B/L` validation exposed by the Odontogram domain types.

This avoids duplicating incompatible dental-code catalogs, but creates a direct same-assembly dependency from Treatments to Odontogram validation helpers.

For the current modular monolith and bounded primitive references, this is accepted. A shared dental-location value object should be considered only if additional modules need the same validation or the current dependency becomes difficult to maintain. No large refactor is justified for this closure.

Release 5 does not write to the Odontogram, Clinical Records or Scheduling aggregates.

## 9. Frontend review

### Confirmed structure

- Routes are lazy-loaded and permission-guarded.
- Patient-profile navigation is permission-aware.
- HTTP remains in Treatments data-access services.
- UI orchestration remains in TreatmentPlan/TreatmentQuote facades and route pages.
- Missing, loading, error, read-only and saving states exist.
- Treatment-plan UI supports explicit creation, item add/remove and status change.
- Quote UI supports missing-plan guidance, explicit snapshot creation, line pricing, totals and status change.
- Accepted plans/quotes become read-only in UI consistently with domain rules.

### Existing later-module navigation

The quote page contains permission-gated navigation to the existing Billing screen. That route and Billing implementation remain outside Release 5 acceptance and do not close or open Release 6.

No Billing write or lifecycle behavior is accepted as part of this release.

### Non-blocking visual debt

- visible internal copy such as `Release 5.1`, `Release 5.2`, `foundation` and `slice` should be replaced with clinic-facing language;
- residual hardcoded colors should move to `--bsm-*` semantic tokens when those screens are touched;
- large presentation files may be decomposed only through bounded visual slices;
- generic mutation errors can later expose safe validation details without changing domain contracts.

These items do not block functional closure.

## 10. Automated evidence

Existing automated coverage verifies:

- explicit treatment-plan create/get;
- `404`/`null` when plan or quote is missing;
- no autocreation from item, price or status operations;
- unique plan per patient/tenant behavior;
- item title, quantity and dental-location validation;
- item add/remove;
- plan lifecycle and accepted-plan immutability;
- explicit quote creation from a non-empty plan;
- one quote per treatment plan;
- fixed `MXN` public path;
- snapshot independence after plan changes;
- unit-price, line-total and quote-total calculation;
- positive pricing gates for Proposed/Accepted;
- accepted-quote immutability;
- forbidden cross-tenant reads/writes;
- blocked platform access without tenant context;
- controller response contracts;
- guarded frontend routes;
- frontend missing/create/item/price/status/read-only flows.

The closure PR must also pass repository-wide CI: backend build, architecture validation, backend unit/integration tests, frontend build and frontend tests.

## 11. Accepted Release 5 boundary

Release 5 closes with:

- Release 5.1 — Treatment Plan Foundation;
- Release 5.2 — Quote Foundation.

This is sufficient for a foundational operational/commercial proposal release. It does not imply treatment execution or billing.

## 12. Explicitly deferred scope

The following remain outside Release 5 closure:

- treatment catalog administration;
- multiple active or archived plans;
- plan archive/versioning;
- quote regeneration/versioning;
- multiple quotes or negotiation;
- taxes and discounts;
- billing/payment linkage;
- scheduling linkage;
- treatment execution or progress tracking;
- automatic status synchronization between plan, quote and later Billing records;
- insurance processing;
- financing/installments;
- advanced approval workflows;
- automated treatment follow-up;
- diagnosis/finding linkage beyond optional FDI tooth/surface references;
- Patient Portal access to plans or quotes.

These require dedicated future slices and must not be inferred from the accepted foundation.

## 13. Non-blocking hardening follow-ups

The audit identified improvements that are valid but not required by the current bounded contract:

- convert unique-constraint races during concurrent create into an explicit stable conflict response if real concurrent usage demonstrates the need;
- define a shared dental-location primitive only if cross-module reuse grows;
- add direct-child-query tenant guardrails before introducing any such queries;
- decide whether future Proposed plans should be immutable; current accepted behavior keeps them editable;
- decide future currency expansion only through an explicit quote/pricing decision;
- review decimal input normalization if pricing with more than two decimal places becomes a supported requirement.

None of these justify a broad rewrite now.

## 14. Closure decision

**Decision:** Close Release 5 — Treatments and Quotes as a foundational release through accepted slices 5.1 and 5.2.

**Reason:** The bounded roadmap contract is implemented across domain, application, API, persistence, authorization, frontend and automated tests. Remaining items belong to later commercial, execution or Billing capabilities.

**Consequence:** Release 6 — Billing becomes the next planned functional phase. Existing Billing code must undergo its own module-specific audit before acceptance.

## 15. Review packet

### Objective

Reconcile existing tested Treatments/Quotes code with formal roadmap state.

### Runtime files changed

None. This closure does not alter product behavior.

### Architecture

Layered backend and feature-based frontend ownership are preserved. The existing bounded dental-code dependency is documented without introducing a premature shared abstraction.

### Tenant/security

Tenant ownership, global filters, server-derived context and cross-tenant protections remain unchanged. No bypass or permission expansion is introduced.

### Validation

Existing targeted tests plus repository-wide CI.

### Risk

- Runtime risk: low, because the change is documentary.
- Product-governance risk: medium, because it advances canonical release state.

### Recommendation

Accept and close Release 5, preserve deferred execution/Billing scope, and begin Release 6 only through a separate audit/opening slice.
