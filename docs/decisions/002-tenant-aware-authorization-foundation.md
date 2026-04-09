# ADR 002 — Tenant-Aware Authorization Foundation

- **Status:** Accepted
- **Date:** 2026-04-08
- **Decision Type:** Foundational
- **Scope:** Identity, platform access, tenant access, branch access
- **Applies To:** Backend, frontend, request context, persistence enforcement

---

## Context

Bigsmile already had JWT authentication, durable identity persistence, tenant and branch entities, and request-scoped tenant resolution middleware.

What was still missing was the part that makes Release 1 safe to start:

- authorization decisions based on scope, membership, role, and permission
- explicit platform override behavior instead of silent platform bypass
- centralized enforcement for tenant-owned reads and writes
- a frontend-ready current-user context contract
- automated coverage for cross-tenant and branch-aware access decisions

Without this foundation, tenant isolation would still depend too much on endpoint discipline and ad hoc checks.

---

## Decision

Bigsmile formalizes tenant-aware authorization through a single coherent foundation.

### 1. Request Access Context

Every authenticated request now resolves and carries:

- current user id
- current tenant id when tenant-scoped
- current branch id when branch-scoped
- current access scope: `platform`, `tenant`, or `branch`
- explicit platform override activation state

`TenantContext` remains request-scoped and becomes the single runtime source used by authorization handlers and EF Core enforcement.

For authenticated requests, tenant and branch context come from JWT claims.

Development headers remain a local fallback only for anonymous requests and never override an authenticated identity.

### 2. Permission Foundation

Bigsmile keeps the initial role set but stops making authorization decisions by role name alone.

The current foundational permission catalog is:

- `auth.self.read`
- `platform.tenants.read`
- `tenant.read`
- `branch.read.any`
- `branch.read.assigned`

Initial role mapping:

- `PlatformAdmin` -> platform-wide tenant inspection plus tenant and branch read permissions
- `TenantAdmin` -> tenant read and unrestricted branch read inside the tenant
- `TenantUser` -> tenant read and assigned-branch read inside the tenant

This is intentionally small and explicit. The catalog can grow later without changing the access model shape.

### 3. Policy-Gated Platform Override

Platform scope alone does not automatically bypass tenant filters.

Instead:

- authorization handlers decide whether a specific policy allows platform override
- when allowed, the request-scoped context activates platform override explicitly
- the handler logs the override activation for traceability

This keeps privileged paths explicit and auditable.

### 4. Centralized Persistence Enforcement

Tenant-owned access is enforced centrally in EF Core:

- authenticated tenant-scoped reads are filtered through global query filters
- the current foundation filters `Tenant`, `Branch`, and `UserTenantMembership`
- authenticated writes are validated in `SaveChanges` and blocked when the target tenant does not match the current tenant context
- platform override can bypass these rules only when it was explicitly activated by authorization

Internal or bootstrap flows outside an authenticated request remain possible without inventing fake tenant scope.

### 5. Current User Context Surface

The backend now exposes:

- `POST /api/auth/login` returning the JWT plus the resolved current access context
- `GET /api/auth/me` returning the current user, tenant, branch, role, scope, and permissions

The frontend keeps session state in memory only and uses:

- an authenticated route guard
- an anonymous-only route guard
- the `/api/auth/me` contract to refresh the current access context

No persistent browser storage is introduced by this foundation.

---

## Consequences

### Positive

- tenant-scoped and branch-aware authorization now has a clear runtime model
- platform support access is explicit instead of implicit
- tenant-owned reads and writes have centralized protection
- frontend feature work can now build on a stable current-user/scope contract
- Release 1 can start without reopening core authorization design

### Tradeoffs

- the current permission catalog is intentionally minimal and will need expansion as business modules arrive
- branch scope selection is still basic; the foundation supports it, but richer branch-switching UX can come later
- platform administrators are still bootstrapped from the current identity persistence model and not yet fully separated into a dedicated platform-only identity path

---

## Follow-up

Expected future work on top of this ADR includes:

- extending the permission catalog per module
- adding business-module policies on top of the same access context
- improving platform support/audit workflows
- refining branch selection and branch-aware operational UX where modules need it

