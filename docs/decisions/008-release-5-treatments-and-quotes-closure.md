# ADR 008 — Release 5 Treatments and Quotes Closure

- **Status:** Accepted
- **Date:** 2026-07-22
- **Decision Type:** Release-scope reconciliation
- **Scope:** Treatment-plan foundation and bounded quote foundation
- **Applies To:** STATE, roadmap, base project documentation, future Treatments/Quotes and Billing work
- **Evidence:** `docs/release-5-treatments-and-quotes-audit-and-closure.md`

## Context

BigSmile canonical documentation placed Release 5 — Treatments and Quotes after the accepted Odontogram foundation, while the repository already contained functional TreatmentPlan and TreatmentQuote code.

Code presence was intentionally not treated as automatic release acceptance. A module-specific audit was required to verify:

- domain invariants and lifecycle rules;
- tenant/patient ownership;
- API contracts and no-autocreation behavior;
- persistence, indexes and migrations;
- permission and role behavior;
- Angular feature ownership and workflow;
- automated tenant, contract and UI evidence;
- separation from Billing and treatment execution.

The audit confirms two coherent bounded slices:

1. Release 5.1 — Treatment Plan Foundation;
2. Release 5.2 — Quote Foundation.

## Decision

Release 5 — Treatments and Quotes closes as the foundational treatment-planning and commercial-quote release through accepted slices 5.1 and 5.2.

### Accepted treatment-plan boundary

The release accepts:

- one tenant-owned and patient-owned treatment plan per patient/tenant in the current slice;
- explicit creation and `GET` returning `404` when missing;
- no autocreation from reads, item operations or status operations;
- bounded items with required title, optional category, positive quantity, optional short notes and optional permanent-adult FDI tooth/surface references;
- explicit item add/remove;
- `Draft`, `Proposed` and `Accepted` lifecycle;
- accepted-plan immutability;
- UTC and actor metadata;
- permission-gated Angular workflow in patient context.

The current one-plan constraint is intentionally stricter than “one active plan.” Archive/versioning and multiple plans are not introduced by this decision.

### Accepted quote boundary

The release accepts:

- explicit quote creation from an existing non-empty treatment plan;
- one quote per treatment plan;
- no quote or plan autocreation;
- snapshot-only quote lines copied from treatment-plan items;
- fixed `MXN` currency in the public application/API path;
- line-level unit price and calculated line/quote totals;
- `Draft`, `Proposed` and `Accepted` lifecycle;
- positive pricing before proposal and acceptance;
- positive-price preservation while proposed;
- accepted-quote immutability;
- no quote regenerate/versioning or negotiation workflow.

### Access decision

The current permission catalog grants:

- `treatmentplan.read` / `treatmentplan.write`;
- `treatmentquote.read` / `treatmentquote.write`.

`PlatformAdmin` and `TenantAdmin` receive these permissions. `TenantUser` does not.

This conservative access model is preserved. The ADR does not introduce Dentist, FrontDesk or other role semantics, and it does not add platform override behavior.

### Tenant-safety decision

TreatmentPlan and TreatmentQuote remain tenant-owned aggregate roots with centralized EF Core filters and write enforcement.

Items remain child rows accessed through their aggregate roots. Future direct child-table queries require tenant-aware joins or explicit tenant ownership.

Public contracts do not accept `TenantId`, owner or actor as authority. Patient, plan and quote ownership is resolved server-side through tenant-filtered paths.

### Domain-boundary decision

Treatment dental references reuse the accepted permanent-adult FDI and surface-code validation from the Odontogram domain.

This same-assembly dependency is accepted for the bounded modular-monolith implementation. A shared dental-location value object should be introduced only if reuse grows enough to justify the abstraction.

Release 5 does not write to Clinical Records, Odontogram, Scheduling or Billing aggregates.

## Deferred scope

Release 5 closure does not include:

- treatment catalog administration;
- multiple/archived treatment plans;
- plan versioning;
- quote regeneration/versioning;
- multiple quotes or negotiation;
- discounts or taxes;
- billing/payments;
- scheduling linkage;
- treatment execution/progress;
- automatic plan/quote/Billing status synchronization;
- insurance or financing;
- advanced approvals;
- automated follow-up;
- Patient Portal access;
- diagnosis/finding linkage beyond optional dental-location references.

Existing permission-gated navigation to implemented Billing code does not accept or close Release 6.

## Alternatives considered

### Reimplement Treatments/Quotes from the roadmap

**Rejected.** Existing code already satisfies the bounded contracts. Reimplementation would create avoidable API, migration and regression risk.

### Accept code solely because it exists

**Rejected.** Acceptance is based on the module-specific audit and automated evidence.

### Require Billing or treatment execution before closing Release 5

**Rejected.** Those are explicitly later capabilities and would collapse bounded release ownership.

### Introduce a shared dental-location abstraction now

**Not selected.** Reuse is currently bounded and explicit. A new abstraction would add churn without changing product behavior.

### Expand treatment permissions to TenantUser now

**Rejected for this closure.** That would change authorization semantics and could expose commercial/clinical planning to a broad role. Role expansion requires an explicit access decision.

## Consequences

### Positive

- Canonical state matches tested TreatmentPlan and TreatmentQuote behavior.
- Existing implementation is reused rather than rewritten.
- The boundary between treatment planning, quoting and Billing is explicit.
- Tenant and permission behavior remain unchanged.
- Release 6 can be audited against stable upstream contracts.

### Trade-offs

- Proposed plans remain editable in the accepted implementation.
- One plan total is enforced rather than a future active/archive model.
- Treatment/quote child rows rely on aggregate-root tenant access.
- Existing UI still contains internal release terminology and hardcoded visual values.
- Concurrency/conflict normalization and wider role mapping remain future hardening decisions.

## Validation

Acceptance requires:

- existing targeted domain, service, controller and frontend tests;
- repository-wide CI green on the closure change;
- aligned updates to `STATE — BigSmile.md`, `README.md`, `PROJECT_MAP.md`, `AGENTS.md`, `docs/product-roadmap.md` and relevant planning documents.

## Resulting roadmap state

- Latest completed functional release: **Release 5 — Treatments and Quotes**.
- Next planned functional phase: **Release 6 — Billing**.
- Phase 2.1 — Patient Intake and Portal Foundation remains planned after completion and acceptance of the initial MVP.

## Decision summary

**Context:** Tested Treatments/Quotes code existed beyond the formal release frontier.

**Decision:** Accept Release 5.1 and 5.2 and close Release 5 as the bounded treatment-plan and quote foundation.

**Consequence:** Advance the roadmap frontier to Release 6 while keeping Billing, treatment execution and advanced commercial workflows outside the accepted Release 5 boundary.
