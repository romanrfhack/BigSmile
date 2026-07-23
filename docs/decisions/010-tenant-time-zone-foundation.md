# ADR 010 — Tenant Time Zone Foundation

- **Status:** Accepted
- **Date:** 2026-07-23
- **Decision Type:** Cross-cutting tenant configuration
- **Scope:** Tenant configuration and operational date boundaries
- **Applies To:** Tenant domain, EF Core persistence, tenant read contracts, Dashboard read models and tests
- **Tracking:** issue #17

## Context

The accepted Scheduling foundation stores and queries appointment start/end values as clinic wall-clock `DateTime` values. The existing Dashboard read model defined the cards labelled `Today appointments` and `Today pending` from `DateTime.UtcNow.Date`.

That behavior is correct only for a tenant whose operational day matches UTC. For clinics outside UTC, the Dashboard can cross into the next UTC date while the clinic is still operating on the previous local date.

BigSmile already defines Tenant as the owner of clinic-level configuration. Branch remains an operational scope subordinate to Tenant and does not currently require a separate time-zone model.

## Decision

Introduce a required, server-authoritative `TimeZoneId` on `Tenant` and use it to resolve tenant-local operational dates.

### Tenant model

- `Tenant.TimeZoneId` is required and limited to 100 characters.
- Values are normalized and validated with `TimeZoneInfo.FindSystemTimeZoneById`.
- New tenants use `America/Mexico_City` when no explicit value is supplied.
- Existing tenants receive the same explicit value through the additive migration default.
- Tenant read contracts expose the configured identifier.

The default reflects the current Mexican pilot context. It is a migration/bootstrap default, not an application-global time zone; each tenant owns its persisted value.

### Dashboard day boundary

For tenant-scoped Dashboard reads:

1. resolve the tenant from the authenticated `TenantContext`;
2. load its persisted `TimeZoneId` through the tenant-filtered repository path;
3. obtain the current UTC instant from `TimeProvider`;
4. convert that instant to the tenant time zone;
5. derive the tenant-local `DateOnly`;
6. build the existing wall-clock `[day start, next day start)` range;
7. execute the existing tenant-scoped metrics without changing their definitions.

`GeneratedAtUtc` remains UTC.

### Scope and ownership

- The browser does not select the Dashboard time zone or day boundary.
- A request cannot provide another tenant's `TimeZoneId`.
- Dashboard remains read-only and requires a resolved tenant.
- Platform scope or platform override without a resolved tenant remains blocked.
- Branch-specific time zones are deferred.

## Persistence and compatibility

The EF Core change is additive:

- add `Tenants.TimeZoneId` as required `nvarchar(100)`;
- use `America/Mexico_City` as the SQL default for existing and newly inserted records that do not provide a value;
- preserve existing tenant ids, relationships, auth claims and query filters;
- preserve all existing API routes.

Adding `timeZoneId` to the existing tenant read DTO is backward compatible for JSON consumers that ignore unknown properties.

## Appointment temporal semantics

This decision does not rewrite Appointment persistence or reinterpret historical values.

Appointments continue to use the accepted clinic wall-clock `DateTime` model. The new tenant time zone answers only which wall-clock date is operationally `today` for server-side reads.

A future decision may introduce stronger instant/time-zone primitives if online booking, cross-zone operations, background jobs or external calendar integrations require them. That broader redesign is outside this slice.

## Alternatives considered

### Keep UTC as `today`

Rejected. It has no schema cost but is operationally incorrect and misleading for non-UTC clinics.

### Accept a browser-provided date or time zone

Rejected. It would make operational results client-controlled, device-dependent and unsuitable for background/server execution.

### Use one application-global clinic time zone

Rejected. It would introduce a single-clinic shortcut into a multi-tenant product.

### Add branch-specific time zones now

Not selected. Branch-specific zones add configuration, authorization and reporting complexity without a demonstrated current requirement.

### Convert every Appointment to UTC now

Not selected. It would be a broad temporal-model migration unrelated to the smallest gap required for the current Dashboard foundation.

## Consequences

### Positive

- `Today` is tenant-correct and server-authoritative.
- Tenant isolation remains explicit.
- Dashboard tests can use deterministic time through `TimeProvider`.
- Future tenant-level operational defaults have a clear owner.
- The change is additive and deploy-safe for current tenants.

### Trade-offs

- The default time zone is product-context specific and must be reviewed during onboarding for tenants outside the pilot market.
- Valid time-zone identifiers depend on the runtime platform time-zone database.
- Appointment values remain wall-clock rather than globally normalized instants.
- No tenant settings UI is introduced in this slice.

## Validation requirements

Acceptance requires automated evidence that:

- invalid or unavailable time-zone identifiers are rejected;
- the migration gives existing tenants the documented default;
- a tenant whose local date differs from UTC receives counts for its local day;
- two tenants can use different day boundaries without cross-tenant leakage;
- platform scope without a resolved tenant remains blocked;
- existing Dashboard metric definitions remain unchanged;
- repository-wide CI passes.

## Non-goals

- branch-specific time zones;
- tenant self-service settings UI;
- appointment storage rewrite;
- full temporal-domain redesign;
- browser-controlled date boundaries;
- external time-zone services;
- doctor dashboards;
- real-time or scheduled aggregation;
- Patient Portal time-zone settings.
