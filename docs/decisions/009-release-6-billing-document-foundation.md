# ADR 009 — Release 6 Billing Document Foundation

- **Status:** Proposed
- **Date:** 2026-07-22
- **Decision Type:** Release-scope reconciliation
- **Scope:** Billing commercial-record foundation
- **Applies To:** Billing domain, application, API, persistence, authorization, frontend, roadmap and canonical state
- **Audit evidence:** `docs/release-6-billing-audit-and-closure.md`
- **Tracking:** issue #13

## Context

Release 5 accepted an explicit TreatmentQuote snapshot with line pricing, calculated totals, bounded `Draft / Proposed / Accepted` lifecycle and read-only behavior after acceptance.

The repository already contains Billing code that creates a second commercial snapshot from an accepted quote. That implementation includes:

- tenant-owned and patient-owned `BillingDocument`;
- server-resolved link to an accepted `TreatmentQuote`;
- immutable snapshot lines;
- explicit creation;
- bounded `Draft -> Issued` lifecycle;
- issue actor/time metadata;
- permission-gated API and Angular flow;
- tenant and cross-tenant test evidence.

Billing had not been formally accepted because code presence alone does not close a release. It also does not yet contain payments, balances, receipts, cash management or fiscal invoicing.

The module-specific audit concludes that the existing bounded implementation satisfies a Release 6.1 foundation without requiring new runtime behavior.

## Proposed decision

Accept **Release 6 — Billing** through one bounded slice:

- **Release 6.1 — Billing Document Foundation**.

The ADR becomes `Accepted` only when canonical state, roadmap and base documentation are reconciled in the accepted closure change.

## 1. BillingDocument boundary

`BillingDocument` is the aggregate root for the accepted Release 6.1 scope.

It is:

- tenant-owned;
- patient-owned;
- linked to one accepted TreatmentQuote;
- a snapshot commercial record;
- separate from Payment, Receipt, CashSession and fiscal invoice concepts.

The accepted aggregate does not mutate TreatmentPlan or TreatmentQuote.

## 2. Creation rule

Billing creation is explicit.

The supported application flow must:

1. require a resolved tenant and actor;
2. resolve the Patient in the current tenant;
3. resolve the Patient's TreatmentQuote through the tenant-filtered path;
4. require the quote to be `Accepted`;
5. require at least one source line;
6. require positive source pricing;
7. reject a second Billing document for the same quote;
8. create the Billing snapshot with server-derived ownership and actor metadata.

Public/API contracts do not accept `TenantId`, quote ownership or actor as authority from the client.

## 3. Snapshot rule

Billing lines copy the accepted quote values needed by the bounded commercial record:

- source quote item id;
- title;
- category;
- quantity;
- notes;
- optional dental location;
- unit price;
- line total.

The root preserves currency and total amount.

Changes to upstream plan or quote data do not resynchronize or rewrite an existing Billing document.

## 4. Lifecycle

The accepted lifecycle is:

```text
Draft -> Issued
```

Issue is explicit and records:

- issued-at UTC;
- issued-by actor;
- last-updated UTC;
- last-updated actor.

An issued Billing document is read-only in this slice.

There is no cancellation, void, replacement, regeneration or reversal workflow in Release 6.1.

## 5. API boundary

Accepted endpoints:

- `GET /api/patients/{patientId}/treatment-plan/quote/billing`;
- `POST /api/patients/{patientId}/treatment-plan/quote/billing`;
- `PUT /api/patients/{patientId}/treatment-plan/quote/billing/status`.

Missing roots return `404`. Reads and status operations never auto-create Billing data.

## 6. Authorization and tenant safety

Accepted permissions:

- `billing.read`;
- `billing.write`.

Current conservative mapping is preserved:

- `PlatformAdmin`: Billing read/write permission;
- `TenantAdmin`: Billing read/write permission;
- `TenantUser`: no Billing permission.

Patient-scoped Billing operations require a resolved tenant context. The Billing policies do not activate platform override.

`BillingDocument` uses the global tenant query filter and centralized tenant-owned write validation.

`BillingDocumentItem` remains a child without its own `TenantId`; direct future queries must use a tenant-aware join or explicit tenant ownership.

## 7. Frontend boundary

The accepted Angular feature remains inside `features/billing` with:

- models;
- data-access service;
- facade;
- patient-context page and focused components;
- permission-guarded lazy route;
- loading, prerequisite, missing, error and saving states;
- explicit create and issue actions;
- snapshot lines and totals;
- issued read-only presentation.

The UI must not imply payment, balance, receipt or CFDI capabilities that are outside the accepted release.

## 8. Explicitly deferred scope

Release 6.1 does not include:

- payment registration or allocation;
- partial or total payment lifecycle;
- balance ledger;
- receipts;
- refunds, reversals, cancellations or voiding;
- cash sessions or daily cash closing;
- taxes or discounts;
- CFDI/PAC or electronic invoicing;
- insurance;
- multi-currency behavior;
- accounting/ERP workflows;
- multiple Billing documents per quote;
- Billing regeneration/versioning;
- automatic mutation of accepted quote state;
- Patient Portal access to Billing.

These require dedicated future decisions and slices.

## 9. Alternatives considered

### Expand Release 6 immediately to payments and balances

**Rejected.** The current repository does not contain an accepted Payment aggregate, allocation model, ledger, receipt or reversal semantics. Adding them now would turn a bounded reconciliation into a broad financial redesign.

### Store payment state directly on BillingDocument

**Rejected.** It would mix an issued commercial snapshot with mutable cash movement and make future allocation, reversal and audit rules difficult to model.

### Treat BillingDocument as a fiscal invoice

**Rejected.** The current record has no tax, legal folio, issuer/receiver fiscal data, CFDI, PAC or cancellation semantics.

### Mutate TreatmentQuote when Billing is issued

**Rejected.** Release 5 accepted the quote as a read-only snapshot after acceptance. Billing must not introduce silent upstream synchronization.

### Require a large shared commercial-document abstraction now

**Not selected.** TreatmentQuote and BillingDocument share snapshot concepts, but their lifecycle and ownership purposes differ. A generic abstraction would create premature coupling without current business value.

### Delay closure until concurrency hardening is implemented

**Not selected for the bounded foundation.** Unique indexes and server-side checks protect the supported path. Stable concurrent-conflict normalization and optimistic issue concurrency remain explicit hardening follow-ups rather than justification for a broad runtime change.

## 10. Consequences

### Positive

- accepted quote and Billing remain separate snapshots;
- tenant ownership and least-privilege permissions remain explicit;
- the clinic gains a bounded issue step without pretending payments exist;
- Release 7 can become the next audit target after formal acceptance;
- future Payment work can receive its own aggregate and audit rules.

### Trade-offs

- the accepted Billing capability is narrower than the original broad Billing roadmap wording;
- there is no balance or collection visibility yet;
- repeated issue semantics are validation-oriented rather than explicitly idempotent;
- concurrent create/issue conflicts are not normalized through a dedicated contract;
- actor ids are still presented raw in parts of the current UI;
- payment and fiscal features remain visibly pending.

## 11. Hardening follow-ups

Non-blocking follow-ups include:

- normalize concurrent unique conflicts when real usage requires it;
- add optimistic concurrency before multiple roles issue the same record concurrently;
- decide repeated-issue idempotency explicitly;
- add relational SQL Server coverage for unique/precision behavior when supported by CI;
- replace internal release/slice copy and raw actor ids in clinic-facing UI;
- consider an issue confirmation interaction as a bounded UX slice;
- design Payment separately before introducing balances or receipts.

## 12. Acceptance condition

Change this ADR from `Proposed` to `Accepted` only when the same closure change:

- updates `STATE — BigSmile.md`;
- updates `docs/product-roadmap.md`;
- reconciles README, AGENTS and PROJECT_MAP;
- records Release 6.1 as completed;
- identifies Release 7 as the next planned functional phase;
- preserves Phase 2.1 after the remaining MVP gate;
- passes repository-wide CI.

## 13. Proposed consequence after acceptance

- Latest completed functional release: Release 6 — Billing.
- Next planned functional phase: Release 7 — Documents and Dashboard.
- Payments, balances, receipts and fiscal invoicing remain unimplemented/unaccepted.
- Patient Intake and Portal remains planned after MVP completion under ADR 006.
