# ADR 007 — Release 4 Odontogram Closure

- **Status:** Accepted
- **Date:** 2026-07-22
- **Decision Type:** Release-scope reconciliation
- **Scope:** Odontogram foundation, surfaces, basic findings and bounded finding history
- **Applies To:** STATE, roadmap, base project documentation, future Odontogram work
- **Evidence:** `docs/release-4-odontogram-audit-and-closure.md`

## Context

BigSmile canonical documentation intentionally kept Release 4 — Odontogram unopened after Release 3 closure, even though the repository already contained functional Odontogram code.

That conservative state was correct until a module-specific audit could confirm:

- the implemented domain boundary;
- tenant isolation;
- API contracts;
- authorization;
- persistence and migrations;
- frontend ownership and operational flow;
- automated validation;
- the difference between foundational scope and advanced deferred capabilities.

The audit confirms existing implementation across four bounded slices:

1. Release 4.1 — Odontogram Foundation;
2. Release 4.2 — Odontogram Surface Foundation;
3. Release 4.3 — Basic Dental Findings Foundation;
4. Release 4.4 — Dental Findings Change History.

## Decision

Release 4 — Odontogram closes as the foundational Odontogram release through accepted slices 4.1 to 4.4.

### Accepted foundation

The accepted release includes:

- one tenant-owned and patient-owned odontogram per `TenantId + PatientId`;
- explicit creation and `GET` returning `404` when missing;
- no autocreation from reads or update operations;
- 32 permanent adult teeth using FDI numbering;
- bounded tooth status and explicit single-tooth updates;
- five bounded surfaces `O/M/D/B/L` per tooth;
- bounded surface status and explicit surface updates;
- a small basic surface-finding catalog;
- explicit finding add/remove;
- append-only finding add/remove history returned newest-first;
- audit metadata and actor attribution;
- `odontogram.read` / `odontogram.write` authorization;
- tenant-scoped application and persistence enforcement;
- patient-context Angular UI through feature data-access and facade;
- automated backend and frontend evidence for the accepted flows.

### Access decision

The current permission catalog grants `odontogram.read` and `odontogram.write` to `PlatformAdmin` and `TenantAdmin`. `TenantUser` does not receive Odontogram permissions.

Patient-scoped Odontogram operations require a resolved tenant context. Platform scope without an explicitly resolved tenant path does not silently bypass tenant isolation.

This ADR does not add roles, permissions or platform override behavior.

### Child-table decision

Tooth, surface, finding and history records remain children of the tenant-owned `Odontogram` aggregate for the accepted access paths.

Future direct queries against child tables must use an explicit tenant-aware join or remodel the child as tenant-owned. The current aggregate-root access path is accepted and remains the supported route.

## Deferred scope

Release 4 closure does not include:

- child or mixed dentition;
- bulk editing;
- complex or tenant-configurable findings;
- treatment, diagnosis, document or imaging linkage;
- a full dental timeline;
- complete tooth/surface change history;
- restore/revert;
- full versioning;
- advanced orthodontic or periodontal charts;
- imaging overlays;
- AI-assisted detection;
- patient-portal Odontogram access.

These remain future bounded work and must not be added incidentally while starting Release 5.

## Alternatives considered

### Reimplement Release 4.1 from scratch

**Rejected.** The existing implementation already satisfies and exceeds the bounded 4.1 contract. Reimplementation would create unnecessary migration, API and regression risk.

### Keep Release 4 indefinitely unaccepted because advanced capabilities are missing

**Rejected.** The roadmap explicitly defers advanced charting, cross-module linkage, full history and versioning. Requiring them would convert a bounded foundational release into an open-ended program.

### Accept only Release 4.1 and ignore implemented surfaces/findings/history

**Rejected.** Existing 4.2 to 4.4 behavior is coherently modeled, persisted, exposed and tested. Artificially excluding it would preserve avoidable documentation drift.

### Treat code presence as automatic acceptance

**Rejected.** Formal acceptance relies on the module-specific audit and evidence, not merely on files or endpoints existing.

## Consequences

### Positive

- Canonical state matches tested repository behavior.
- No duplicate implementation or schema work is introduced.
- The accepted Odontogram boundary is explicit and reviewable.
- Advanced features remain safely deferred.
- Release 5 can start from a known patient/clinical/dental foundation.

### Trade-offs

- Existing visual debt remains: internal release copy, large presentation components and residual hardcoded colors.
- Finding history is intentionally narrower than a complete dental timeline.
- Child rows rely on aggregate-root access for tenant enforcement.
- The next module contains existing unaccepted code and requires its own audit rather than automatic promotion.

## Validation

Acceptance requires:

- targeted Odontogram unit/integration/frontend tests already present;
- repository-wide CI green on the closure change;
- aligned updates to `STATE — BigSmile.md`, `README.md`, `PROJECT_MAP.md`, `AGENTS.md`, `docs/product-roadmap.md` and the UX reconciliation plan.

## Resulting roadmap state

- Latest completed functional release: **Release 4 — Odontogram**.
- Next planned functional phase: **Release 5 — Treatments and Quotes**.
- Phase 2.1 — Patient Intake and Portal Foundation remains planned after the initial MVP and is not opened by this closure.

## Decision summary

**Context:** Odontogram code existed beyond canonical state and required explicit audit.

**Decision:** Accept slices 4.1 through 4.4 and close Release 4 as the foundational Odontogram release.

**Consequence:** Move the roadmap frontier to Release 5 while retaining advanced Odontogram capabilities as separately scoped future work.
