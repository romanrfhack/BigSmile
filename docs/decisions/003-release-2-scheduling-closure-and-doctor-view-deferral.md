# ADR 003 — Release 2 Scheduling Closure and Doctor-Based View Deferral

- **Status:** Accepted
- **Date:** 2026-04-17
- **Decision Type:** Release-scope reconciliation
- **Scope:** Scheduling roadmap closure, release state, bounded future scheduling work
- **Applies To:** STATE, roadmap, base project documentation

---

## Context

Release 2 — Scheduling already has explicit evidence in code, tests, and release review for:

- daily and weekly branch-aware calendar views
- appointment creation, editing, rescheduling, and cancellation
- appointment notes
- blocked time slots
- attended and no-show states

The remaining ambiguity came from `doctor-based views` still being listed inside the Release 2 roadmap scope even though the current Scheduling implementation does not include:

- provider/doctor assignment on `Appointment`
- provider/doctor-specific read models or calendar filters
- a dedicated bounded slice for provider/doctor scheduling ownership

Because of that, doctor-based views are not a small UI patch. They require additional domain modeling and a dedicated implementation slice.

---

## Decision

Release 2 — Scheduling closes without doctor-based views.

Doctor-based views are explicitly deferred to a future bounded Scheduling slice.

That future slice must introduce provider/doctor assignment deliberately, including at minimum:

- appointment-to-provider/doctor association
- provider/doctor-aware calendar queries and read models
- frontend filtering and/or dedicated views
- any required authorization or membership adjustments

This deferral does not change current tenant isolation, branch-aware enforcement, or authorization behavior.

---

## Consequences

- Release 2 can be marked as completed in canonical state and base documentation.
- Release 3 — Clinical Records becomes the next planned phase.
- Doctor-based views must not be reintroduced later as an incidental UI enhancement without the dedicated provider/doctor assignment slice.
