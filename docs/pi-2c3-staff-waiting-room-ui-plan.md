# PI-2C3 — Staff Waiting-Room Handoff UI Plan

- **Status:** Active
- **Parent:** PI-2C issue #35
- **Tracking:** issue #38
- **Dependencies:** PI-2C1 PR #41 and PI-2C2 PR #42
- **Architecture:** ADR 006 and ADR 012–019
- **Risk:** High but bounded — one-time credential handling in the browser

## 1. Objective

Provide a minimal `TenantAdmin` workflow for reception to generate, copy, print, locally render and revoke a one-time waiting-room link. The UI must use the accepted `patientportal.intake.manage` permission and must never persist or transmit the raw token outside the current handoff operation.

This slice closes PI-2C only. It does not implement the patient demographic/medical questionnaire, submission, clinic review or canonical application.

## 2. Existing accepted backend

Staff management API:

```http
POST   /api/patient-intake-links
GET    /api/patient-intake-links
DELETE /api/patient-intake-links/{linkId}
```

Public intake authentication API:

```http
POST /api/patient-portal/intake-auth/activate
POST /api/patient-portal/intake-auth/realms/{tenantSubdomain}/login
GET  /api/patient-portal/intake-auth/me
POST /api/patient-portal/intake-auth/logout
```

The staff API returns the raw token only from POST. GET returns metadata and status only. The token cannot be reconstructed from persisted data.

## 3. Routes

### Staff

```text
/patient-intake-links
```

- authenticated staff shell;
- `authGuard`;
- route data `requiredPermissions: ['patientportal.intake.manage']`;
- navigation label: **Sala de espera**;
- navigation item visible only when the current staff session carries the permission.

### Patient handoff

```text
/patient-portal/intake-activate#token=<one-time-token>
```

- patient shell, never staff shell;
- token read once from the fragment;
- fragment removed immediately through `history.replaceState`;
- activation request sends the token only in the request body;
- the page may collect `LoginName`, password and confirmation;
- successful activation establishes an in-memory intake-only session and shows a bounded continuation state;
- demographic/medical capture remains PI-2D.

## 4. Frontend ownership

```text
frontend/src/app/features/patient-intake-links/
  pages/
    patient-intake-links.page.ts
  components/
    patient-intake-link-form.component.ts
    patient-intake-link-handoff.component.ts
    patient-intake-link-list.component.ts
    patient-intake-link-print-sheet.component.ts
  facades/
    patient-intake-links.facade.ts
  data-access/
    patient-intake-links.api.ts
  models/
    patient-intake-link.models.ts
```

Patient intake-auth additions remain inside the existing patient portal boundary or a narrowly owned `patient-intake-auth` feature. Staff data access must not be placed under `/patient-portal/*` frontend infrastructure.

## 5. State model

The facade owns:

- active/resolved link metadata;
- optional selected Branch;
- issue/list/revoke loading and errors;
- current one-time raw token and generated URL;
- copy state;
- print state;
- current expiry countdown/display state.

The raw token and URL are signals held only in memory. They are cleared on:

- navigation away;
- explicit dismiss;
- new issue operation;
- logout;
- component/facade destruction where applicable.

They must never be written to:

- `localStorage`;
- `sessionStorage`;
- IndexedDB;
- cookies;
- query string or route path;
- logs, analytics or error telemetry.

## 6. Generate flow

1. Load available Branch metadata from the current staff context or existing Branch data-access.
2. Allow no Branch or one active Branch inside the tenant.
3. POST the optional `BranchId`.
4. Build the public URL locally from the returned raw token.
5. Display the URL once with expiration.
6. Offer copy, print and local QR.
7. After refresh, show only metadata; never imply the raw URL can be recovered.

Multiple active links remain valid because each represents a distinct prospective patient/session. The UI must not silently replace another active link.

## 7. Local QR decision gate

The repository currently has no QR dependency.

Before implementation, compare:

1. a small browser-local QR library with no network/runtime service;
2. an in-repository QR encoder, only if it remains small, standards-correct and testable.

Recommended direction: use a narrowly scoped, actively maintained QR encoder package that renders locally to canvas/SVG and performs no network requests. Pin its version, review license/supply-chain impact, and add tests proving copy/print/QR do not call HTTP. Do not use a remote QR image endpoint.

This dependency decision is implementation-level unless it introduces an external service, telemetry or a new cross-cutting frontend state strategy.

## 8. Print contract

The printable handoff sheet contains only:

- BigSmile/clinic identity already available to the session;
- optional Branch display name;
- QR and human-readable activation URL;
- expiration time;
- concise Spanish instructions;
- warning that the link is one-time and personal.

It excludes:

- token hash;
- internal tenant/account/intake ids;
- staff actor ids;
- patient/medical data;
- staff navigation and operational diagnostics.

Use print-specific CSS and an isolated printable element. `window.print()` must not trigger a network request.

## 9. Link list and revoke

- default list emphasizes active links;
- bounded optional resolved history;
- statuses mapped to Spanish user-facing copy: `Activo`, `Expirado`, `Revocado`, `Utilizado`;
- expiration displayed in tenant-local time where the existing frontend convention supports it;
- revoke requires confirmation;
- only active links expose revoke;
- metadata rows never expose or reconstruct raw token/URL.

## 10. Interceptor and shell boundaries

- staff bearer may call `/api/patient-intake-links` only;
- staff bearer must not be attached to `/api/patient-portal/intake-auth/*`;
- patient/intake bearer must never be attached to staff management APIs;
- public activation/login requests carry no bearer;
- patient portal routes remain shellless from the staff app;
- opening a patient-portal route clears any conflicting in-memory staff/patient session according to accepted boundaries.

## 11. Required tests

### Routing and authorization

- route requires `patientportal.intake.manage`;
- navigation hidden without permission;
- `TenantUser` and `PlatformAdmin` cannot enter;
- patient/intake sessions cannot enter staff route.

### Data access and facade

- POST/GET/DELETE contracts;
- optional Branch payload only;
- raw token retained only in one-time response state;
- metadata refresh cannot restore URL;
- revoke/loading/error transitions;
- no browser-storage writes.

### Handoff security

- fragment token parsed once and removed before activation request;
- copy uses the in-memory URL;
- print contains the intended URL/expiry and no internal ids/hash;
- QR generation is local and makes no remote HTTP request;
- navigation/dismiss clears token state.

### Regression and UX

- staff and patient interceptors remain separated;
- patient activation/login boundaries remain intact;
- responsive layout on mobile/tablet/desktop;
- Spanish-first copy and keyboard/focus behavior;
- Angular production build and repository-wide CI green.

## 12. Documentation and exit gate

Before merge:

- update `STATE — BigSmile.md`;
- update `docs/product-roadmap.md`;
- update `docs/patient-intake-and-portal-plan.md`;
- update README/AGENTS/PROJECT_MAP where their current-state summaries require it;
- record PI-2C1, PI-2C2 and PI-2C3 as completed only with evidence;
- leave PI-2D as the single next intake slice.

PI-2C closes when a TenantAdmin can generate, copy, print, locally render and revoke a one-time handoff, and a new patient can safely consume that link through the accepted intake-only boundary without creating canonical clinical data.

## 13. Explicit non-goals

- patient demographic/medical questionnaire UI;
- static reusable QR;
- remote delivery through email/SMS/WhatsApp;
- public patient directory or lookup;
- canonical Patient/ClinicalRecord writes;
- duplicate matching;
- submit/review/apply;
- full staff intake-review worklist;
- refresh tokens or remote recovery.
