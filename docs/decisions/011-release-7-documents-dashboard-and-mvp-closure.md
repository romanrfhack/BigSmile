# ADR 011 — Release 7 Documents/Dashboard and MVP Closure

- **Status:** Accepted
- **Date:** 2026-07-23
- **Decision Type:** Release-scope and MVP reconciliation
- **Scope:** Documents, Dashboard and initial operational MVP
- **Applies To:** Product state, roadmap, Documents, Dashboard, tenant configuration, frontend and canonical documentation
- **Audit evidence:** `docs/release-7-documents-and-dashboard-audit-and-closure.md`
- **Tracking:** issue #15

## Context

Release 7 is the final planned release of the initial operational MVP.

The repository already contained coherent Documents and Dashboard implementations, but the initial audit identified two bounded blockers:

1. Patient document upload trusted a client-declared MIME type without verifying the binary signature and did not tightly bound multipart parsing near the 10 MB product limit.
2. Dashboard presented a UTC calendar date as clinic `Today` while Tenant had no server-authoritative time zone.

Those blockers were addressed separately:

- PR #19 / issue #16 hardened Patient Document upload and storage-containment evidence;
- PR #20 / issue #17 introduced ADR 010, `Tenant.TimeZoneId`, an additive migration and tenant-local Dashboard day boundaries.

Both implementation PRs passed repository-wide CI. No broad rewrite was necessary.

## Decision

Accept **Release 7 — Documents and Dashboard** through two bounded slices:

- **Release 7.1 — Patient Documents Foundation**;
- **Release 7.2 — Dashboard Read Model Foundation**.

With Release 7 accepted, also accept the **initial operational MVP** as complete.

This ADR is accepted together with canonical state, roadmap and base-document reconciliation.

## 1. Release 7.1 boundary

The accepted Patient Documents foundation includes:

- tenant-owned/patient-owned `PatientDocument` metadata;
- explicit authorized upload;
- private local storage outside public web roots;
- server-generated storage keys;
- original-file-name normalization;
- PDF/JPEG/PNG declared-type allowlist;
- matching PDF/JPEG/PNG binary-signature verification;
- 10 MB authoritative file limit;
- bounded multipart request/form limit;
- active newest-first list;
- authorized download;
- logical retire;
- UTC/actor metadata;
- explicit `document.read` / `document.write`;
- tenant/cross-tenant/platform-support automated evidence;
- bounded Angular patient-context workflow.

The accepted signature check is a format gate only. It is not a malware-scanning guarantee.

## 2. Release 7.2 boundary

The accepted Dashboard foundation includes:

- read-only `GET /api/dashboard/summary`;
- required resolved tenant context;
- `dashboard.read` under the conservative current role mapping;
- server-authoritative tenant-local operational day;
- active patient count;
- tenant-local today appointment count;
- scheduled/pending today appointment count;
- active document count;
- existing treatment plan count;
- accepted quote count;
- issued Billing document count;
- generated-at UTC;
- tenant-isolation and platform-without-tenant automated evidence;
- bounded Angular summary-card workflow.

Dashboard owns no business state and performs no synchronization or mutation.

## 3. Tenant time-zone relationship

ADR 010 remains the controlling decision for tenant operational time:

- Tenant owns `TimeZoneId`;
- the browser cannot choose the operational day;
- existing/bootstrap tenants receive the documented Mexican-pilot default;
- each tenant can persist a distinct value;
- Branch time zones remain deferred;
- Appointment persistence remains under the accepted wall-clock model.

Release 7.2 consumes that foundation; it does not own it.

## 4. Explicitly deferred Documents scope

Release 7.1 does not include:

- OCR;
- rich previews;
- versioning;
- public/external sharing;
- generated PDFs/templates;
- electronic signatures or advanced consent workflows;
- advanced imaging viewer;
- external antivirus/malware provider integration;
- automated retention/physical deletion;
- Patient Portal document access.

## 5. Explicitly deferred Dashboard scope

Release 7.2 does not include:

- revenue/payment/balance metrics;
- charts, trends or conversion analytics;
- exports;
- branch dashboards;
- doctor/provider dashboards;
- real-time updates;
- BI integrations;
- scheduled/materialized aggregation;
- AI recommendations;
- Patient Portal dashboard access;
- hidden platform support impersonation.

## 6. MVP interpretation

The initial operational MVP is accepted because the following are now formally closed with aligned code, tests and documentation:

- Foundation / Release 0;
- tenant-aware authorization and roles/permissions foundation;
- Release 1 — Patients;
- Release 2 — Scheduling;
- Release 3 — Clinical Records;
- Release 4 — Odontogram;
- Release 5 — Treatments and Quotes;
- Release 6 — Billing Document Foundation;
- Release 7 — Patient Documents and Dashboard foundations.

MVP acceptance means the bounded operational backbone is coherent and ready for pilot validation. It does not mean every broad roadmap phrase or future commercial capability is implemented.

In particular, the MVP does not include:

- payment registration, allocations, balances, receipts or cash sessions;
- CFDI/fiscal invoicing;
- provider/doctor assignment and doctor calendars;
- automated notification providers/jobs/queues/retries;
- online booking;
- advanced analytics;
- full Patient Portal;
- Phase 2.1 patient self-service implementation.

## 7. Phase 2.1 gate

MVP acceptance satisfies the normal roadmap prerequisite for **Phase 2.1 — Patient Intake and Portal Foundation**.

It does not automatically implement or open PI-1.

The next-phase decision must still preserve ADR 006 and resolve the bounded choices already tracked in issue #2, including:

- patient access identifier;
- password versus magic-link bootstrap;
- invitation/waiting-room link TTL;
- pilot delivery method;
- recovery/lockout baseline.

When Phase 2.1 is explicitly opened, implementation starts with issue #4 only and canonical state is updated in the same change.

## 8. Alternatives considered

### Keep Release 7 open until advanced Documents/Dashboard capabilities exist

Rejected. OCR, sharing, charts, revenue metrics and advanced workflows are explicitly deferred and are not necessary for the bounded MVP foundation.

### Close Release 7 without the two hardening corrections

Rejected. It would accept a spoofable upload allowlist and an operationally incorrect tenant date boundary.

### Expand Release 7 into Payments or advanced analytics

Rejected. Payments remain a separate future aggregate/capability and advanced analytics exceed the bounded Dashboard read model.

### Open Phase 2.1 automatically in the same closure

Not selected. The MVP gate is satisfied, but public patient identity remains high risk and requires explicit resolution of the issue #2 access/bootstrap choices before PI-1 implementation.

## 9. Consequences

### Positive

- all initial MVP releases have explicit acceptance evidence;
- sensitive document upload fails closed on format mismatch;
- operational `Today` is correct per tenant;
- module boundaries and tenant isolation remain intact;
- the roadmap can advance without pretending deferred financial or portal capabilities exist;
- Phase 2.1 becomes the next planned product phase under an already accepted architecture.

### Trade-offs

- the accepted Documents feature remains intentionally simple;
- Dashboard remains a snapshot read model without drill-downs or advanced analytics;
- the MVP has issued Billing documents but not payment collection flows;
- tenant time-zone configuration lacks self-service UI;
- clinic-facing copy and visual debt remain in some screens;
- public patient identity decisions remain pending.

## 10. Validation evidence

- PR #19 CI #149: backend build, architecture validation, unit tests, integration tests, Angular build and frontend tests passed.
- PR #20 CI #151: backend build, architecture validation, unit tests, integration tests, Angular build, frontend tests and generated migration compilation passed.
- The Release 7 closure documentation change must pass repository-wide CI again before merge.

## 11. Resulting state

- Latest completed functional release: **Release 7 — Documents and Dashboard**.
- Initial operational MVP: **accepted**.
- Next planned phase: **Phase 2.1 — Patient Intake and Portal Foundation**.
- PI-1 through PI-4: **not implemented**.
- Next implementation candidate after explicit opening: **PI-1 — Access and Invitation Foundation (issue #4)**.
