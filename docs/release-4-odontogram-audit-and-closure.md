# Release 4 — Odontogram Audit and Closure

- **Status:** Accepted closure evidence
- **Audit date:** 2026-07-22
- **Scope:** Release 4.1 through Release 4.4
- **Result:** Release 4 can close as the foundational Odontogram release
- **Next planned functional phase:** Release 5 — Treatments and Quotes

## 1. Objective

Audit the current Odontogram implementation against the bounded Release 4 roadmap and determine whether the existing code can be formally accepted without reimplementing work that is already present.

The audit distinguishes three concepts:

1. code exists;
2. a bounded slice satisfies its contract;
3. the release is formally accepted and documented.

The repository already contained functional Odontogram code, but the canonical documents intentionally classified it as `implemented but not formally accepted/reconciled` until a module-specific audit occurred.

## 2. Reconciliation result

The current implementation satisfies the accepted foundational scope through four bounded slices:

1. **Release 4.1 — Odontogram Foundation**;
2. **Release 4.2 — Odontogram Surface Foundation**;
3. **Release 4.3 — Basic Dental Findings Foundation**;
4. **Release 4.4 — Dental Findings Change History**.

No new backend behavior, API contract, migration, permission or frontend feature is required to accept these slices. The closure is therefore a documentation and release-state reconciliation over existing tested code.

## 3. Release 4.1 — Odontogram Foundation

### Accepted behavior

- `Odontogram` is tenant-owned and patient-owned.
- Exactly one odontogram is allowed per `TenantId + PatientId`.
- Creation is explicit through `POST /api/patients/{patientId}/odontogram`.
- `GET /api/patients/{patientId}/odontogram` returns `404` when no odontogram exists.
- Reads and updates never auto-create an odontogram.
- The initial chart contains the 32 permanent adult teeth using FDI numbering:
  - `11-18`;
  - `21-28`;
  - `31-38`;
  - `41-48`.
- Tooth status is bounded to:
  - `Unknown`;
  - `Healthy`;
  - `Missing`;
  - `Restored`;
  - `Caries`.
- Tooth state updates are explicit and preserve updater metadata.
- The aggregate and each tooth include UTC timestamps and actor attribution.

### Evidence

- Domain: `backend/src/BigSmile.Domain/Entities/Odontogram.cs`.
- Tooth model: `backend/src/BigSmile.Domain/Entities/OdontogramToothState.cs` and `OdontogramToothStatus.cs`.
- Application commands/queries: `backend/src/BigSmile.Application/Features/Odontograms`.
- Controller: `backend/src/BigSmile.Api/Controllers/PatientOdontogramsController.cs`.
- Persistence: `OdontogramConfiguration`, `OdontogramToothStateConfiguration`, repository and migration `20260421012625_AddOdontogramFoundation`.
- Backend tests: `OdontogramServicesTests`, `PatientOdontogramsControllerTests`.
- Frontend: patient route, explicit empty/create state, odontogram grid and tooth editor.

## 4. Release 4.2 — Odontogram Surface Foundation

### Accepted behavior

- Each of the 32 teeth initializes five bounded surfaces:
  - `O`;
  - `M`;
  - `D`;
  - `B`;
  - `L`.
- Surface status is bounded to:
  - `Unknown`;
  - `Healthy`;
  - `Restored`;
  - `Caries`.
- Surface updates require an existing odontogram and a valid permanent-adult tooth/surface pair.
- Surface metadata preserves UTC update time and actor.
- Uniqueness is enforced per `OdontogramId + ToothCode + SurfaceCode`.
- The frontend exposes selected-tooth surface detail without introducing a second API path or duplicate state owner.

### Evidence

- Domain: `OdontogramSurfaceState.cs` and `OdontogramSurfaceStatus.cs`.
- Persistence: `OdontogramSurfaceStateConfiguration` and migration `20260421021019_AddOdontogramSurfaceFoundation`.
- API: `PUT /api/patients/{patientId}/odontogram/teeth/{toothCode}/surfaces/{surfaceCode}`.
- Tests cover valid updates, invalid tooth/surface codes, missing odontogram and cross-tenant writes.

## 5. Release 4.3 — Basic Dental Findings Foundation

### Accepted behavior

- Findings are attached to a specific valid tooth surface.
- The bounded catalog is:
  - `Caries`;
  - `Restoration`;
  - `MissingStructure`;
  - `Sealant`.
- Findings coexist with tooth and surface status; they do not implicitly recalculate either state.
- Exact duplicate findings are rejected for the same odontogram, tooth, surface and type.
- Add and remove are explicit operations.
- Current findings include creation timestamp and actor.
- The read model returns findings nested under the corresponding surface.

### Evidence

- Domain: `OdontogramSurfaceFinding.cs` and finding catalog enum.
- Persistence: `OdontogramSurfaceFindingConfiguration` and migration `20260421034813_AddOdontogramSurfaceFindingsFoundation`.
- API:
  - `POST .../surfaces/{surfaceCode}/findings`;
  - `DELETE .../surfaces/{surfaceCode}/findings/{findingId}`.
- Tests cover add/remove, invalid catalog values, duplicates, missing odontogram and cross-tenant access.

## 6. Release 4.4 — Dental Findings Change History

### Accepted behavior

- Finding add/remove actions create append-only history entries.
- History records:
  - entry type;
  - tooth and surface;
  - finding type;
  - changed-at UTC;
  - actor;
  - summary;
  - referenced finding id when available.
- History is returned newest-first.
- Current findings and findings history remain separate concepts.
- Findings history does not become a global dental or cross-module timeline.
- There is no restore/replay operation.

### Evidence

- Domain: `OdontogramSurfaceFindingHistoryEntry.cs`.
- Persistence: `OdontogramSurfaceFindingHistoryEntryConfiguration` and migration `20260421051135_AddOdontogramSurfaceFindingHistory`.
- Mapping tests verify newest-first ordering.
- Integration and frontend tests verify add/remove history and surface-scoped rendering.

## 7. API and authorization review

### Contracts confirmed

- `GET /api/patients/{patientId}/odontogram`.
- `POST /api/patients/{patientId}/odontogram`.
- `PUT /api/patients/{patientId}/odontogram/teeth/{toothCode}`.
- `PUT /api/patients/{patientId}/odontogram/teeth/{toothCode}/surfaces/{surfaceCode}`.
- `POST /api/patients/{patientId}/odontogram/teeth/{toothCode}/surfaces/{surfaceCode}/findings`.
- `DELETE /api/patients/{patientId}/odontogram/teeth/{toothCode}/surfaces/{surfaceCode}/findings/{findingId}`.

### Access confirmed

- Reads require `odontogram.read`.
- Writes require `odontogram.write`.
- `PlatformAdmin` and `TenantAdmin` receive the odontogram permissions in the current catalog.
- `TenantUser` does not receive odontogram permissions.
- A platform-scoped request without a resolved tenant context is blocked from patient-scoped Odontogram operations.

No new permission or role semantic is introduced by this closure.

## 8. Tenant-safety review

### Confirmed controls

- `Odontogram` implements tenant ownership.
- `AppDbContext` applies the centralized tenant query filter to the aggregate root.
- `SaveChanges` validates tenant-owned writes against the resolved request context.
- Command/query services require a resolved tenant context.
- Patient availability is resolved inside the current tenant before any odontogram operation.
- Cross-tenant reads return unavailable/null behavior.
- Cross-tenant writes are rejected.
- `TenantId`, actor and ownership are never accepted as authority from odontogram request bodies.

### Child-table rule

Tooth, surface, finding and finding-history rows are consumed through the tenant-owned `Odontogram` aggregate. If a future use case queries those child tables directly, it must add an explicit tenant-aware join or remodel the child as tenant-owned. This is a future guardrail, not a blocker for the current aggregate access path.

### Result

**Tenant-safe for the accepted Release 4 access paths.**

## 9. Frontend review

### Confirmed structure

- Route protection uses `odontogram.read`.
- Patient profile navigation is permission-aware.
- HTTP remains in `features/odontogram/data-access`.
- Feature orchestration remains in `OdontogramsFacade`.
- Route page handles loading, missing, error and explicit creation states.
- The dental chart uses permanent adult FDI layout and keyboard-operable tooth buttons.
- Selected tooth, surfaces, findings and surface-scoped history are available in the patient context.
- Mutations update the facade state from API responses rather than duplicating server business rules.

### Non-blocking visual debt

The following remain valid UX follow-ups and do not block functional closure:

- replace visible internal copy such as `Release 4.4` with clinic-facing language;
- migrate residual hardcoded colors to `--bsm-*` tokens;
- continue splitting large presentation components only when a bounded visual slice justifies it;
- improve dense responsive presentation without changing API or domain scope.

## 10. Test evidence

Existing automated evidence covers:

- explicit create/get;
- `404`/null when missing;
- one odontogram per patient/tenant;
- initialization of 32 teeth and five surfaces per tooth;
- tooth and surface state updates;
- valid FDI and surface catalogs;
- basic finding catalog and duplicate rejection;
- finding add/remove and append-only history;
- newest-first history mapping;
- no autocreation from update operations;
- forbidden cross-tenant reads/writes;
- blocked platform access without tenant context;
- controller response contracts;
- frontend empty/create/read/update/findings/history flows.

The closure PR must also pass the repository-wide CI workflow: backend build, architecture validation, backend unit/integration tests, frontend build and frontend tests.

## 11. Accepted Release 4 boundary

Release 4 closes with:

- Release 4.1 — Odontogram Foundation;
- Release 4.2 — Odontogram Surface Foundation;
- Release 4.3 — Basic Dental Findings Foundation;
- Release 4.4 — Dental Findings Change History.

This is sufficient for a foundational odontogram release and does not require every possible dental chart capability.

## 12. Explicitly deferred scope

The following remain outside Release 4 closure:

- child or mixed dentition;
- bulk editing;
- complex or tenant-configurable finding catalogs;
- treatment-plan linkage;
- diagnosis linkage;
- document or imaging linkage;
- full dental timeline;
- tooth/surface state history beyond the accepted finding history;
- restore/revert;
- full odontogram versioning;
- orthodontic or periodontal charting;
- imaging overlays;
- AI-assisted detection;
- patient-portal access to the odontogram.

These items require dedicated future slices and must not be inferred from the accepted foundational chart.

## 13. Closure decision

**Decision:** Close Release 4 — Odontogram as a foundational release through accepted slices 4.1 to 4.4.

**Reason:** The bounded roadmap contract is implemented across domain, application, API, persistence, authorization, frontend and automated tests. Remaining items are explicitly advanced or cross-module capabilities rather than blockers to the foundation.

**Consequence:** Release 5 — Treatments and Quotes becomes the next planned functional phase. Existing Treatments/Quotes code must still undergo its own module-specific audit before it can be accepted or closed.

## 14. Review packet

### Objective

Reconcile tested existing Odontogram code with formal roadmap state.

### Files functionally changed

None. This closure does not alter runtime behavior.

### Architecture

Module ownership, layered backend structure and feature-based frontend structure are preserved.

### Tenant/security

Tenant ownership and centralized enforcement remain unchanged. No new bypass or permission is introduced.

### Validation

Existing targeted tests plus repository-wide CI.

### Risk

Low for runtime because the change is documentary; medium product-governance impact because it advances canonical release state.

### Recommendation

Accept and close Release 4, preserve the documented deferred scope, and begin Release 5 only through a separate audit/opening slice.
