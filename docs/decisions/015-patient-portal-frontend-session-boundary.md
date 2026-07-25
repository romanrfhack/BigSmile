# ADR 015 — Patient Portal Frontend Session Boundary

- **Status:** Accepted
- **Date:** 2026-07-25
- **Decision Type:** Frontend authentication boundary, browser session security, PI-1 closure
- **Scope:** Phase 2.1 / PI-1D — patient activation/login frontend and security closure
- **Applies To:** Angular routing, auth state, HTTP interceptors, UX, operations and test strategy
- **Tracking:** Phase 2.1 epic #2; PI-1 #4; PI-1D #25; implementation PR #30

## Context

PI-1A established tenant-owned patient portal accounts and invitations. PI-1B added staff-managed one-time invitations. PI-1C added the separate backend patient activation/login/session boundary under ADR 014.

The remaining PI-1 risk is the browser boundary. Reusing the staff shell, staff interceptor or persistent browser storage would create token confusion, accidental credential disclosure and a wider attack surface. The patient-facing frontend must therefore preserve the same least-privilege separation already enforced by the backend.

## Decision

### 1. Separate patient route area

The Angular application exposes a dedicated route tree outside the staff shell:

```text
/patient-portal/activate
/patient-portal/:tenantSubdomain/login
/patient-portal/:tenantSubdomain/home
```

Patient routes:

- never render staff navigation, staff access context or staff operational modules;
- use a dedicated patient-facing shell;
- clear any in-memory staff session when the patient shell opens;
- do not expose Patient, account or Tenant identifiers as route authority.

### 2. Activation token transport

The manual pilot activation URL uses the URL fragment:

```text
https://<host>/patient-portal/activate#token=<one-time-token>
```

The frontend:

1. reads the fragment once;
2. keeps the token only in component memory;
3. removes the fragment from the address bar immediately through `replaceState`;
4. submits the token only in the activation request body;
5. never writes the token to local storage, session storage, logs, analytics or patient notes.

A query-string or path token is rejected as the standard delivery pattern because those values are more likely to reach server logs, proxy logs, browser history and referrer metadata.

### 3. In-memory patient session

The patient access token, expiry and current-session DTO live only in an Angular in-memory store.

- no patient token in `localStorage` or `sessionStorage`;
- no account id, Patient id, Tenant id or session version persisted in the browser;
- a full page refresh intentionally requires login again during the pilot;
- no refresh token is introduced by PI-1D;
- logout clears local in-memory state even when server confirmation is unavailable.

The existing non-sensitive UI language preference remains the only permitted browser-persisted preference in this boundary.

### 4. HTTP token separation

Two interceptors remain explicit and non-overlapping:

- the staff interceptor never attaches the staff bearer token to `/api/patient-portal/*`;
- the patient interceptor attaches the patient bearer token only to protected `/api/patient-portal/*` endpoints;
- patient activation and login requests are anonymous and receive no bearer token;
- the patient interceptor never attaches a token to staff, clinical or commercial endpoints;
- a protected patient `401` clears the patient in-memory session.

This frontend rule complements, but does not replace, the backend scheme/audience separation from ADR 014.

### 5. Patient-only guards and realm binding

Patient guards are independent from staff guards.

- an unauthenticated user is redirected to the login route for the requested tenant realm;
- an authenticated patient cannot navigate into another tenant realm through route manipulation;
- login redirects an already authenticated patient to their own realm home;
- activation remains reachable even when a stale in-memory patient token exists so assisted recovery can complete.

`Tenant.Subdomain` remains a public realm identifier only. It is never treated as proof of ownership.

### 6. Bounded UX

PI-1D includes only:

- activation with `LoginName`, password and confirmation;
- tenant-realm login;
- current-session refresh;
- a bounded authenticated home;
- logout;
- generic error states.

The UI does not distinguish whether a credential failed because of an unknown tenant, unknown account, incorrect password, lockout, expired invitation, consumed invitation or revoked invitation. The authenticated home explicitly states that intake/medical-information capture is not yet available.

### 7. PI-1 closure evidence

PI-1 closes only when the following are aligned:

- PI-1A through PI-1D code and migrations;
- backend and frontend automated tests;
- staff/patient token-separation evidence;
- activation fragment cleanup evidence;
- reception-assisted recovery runbook;
- STATE, roadmap and patient-intake plan;
- repository-wide CI.

PI-1 closure does not open PI-2 automatically inside the same implementation diff.

## Alternatives considered

### Reuse the staff shell and `AuthService`

**Rejected.** It would mix two identities, token stores, guards and operational navigation surfaces.

### Persist the patient token in local or session storage

**Rejected for the pilot.** It would improve refresh convenience but increase credential exposure and conflict with the accepted memory-only security baseline.

### Put the activation token in the query string

**Rejected.** Query values are more likely to be retained by history, analytics, reverse proxies and referrer metadata.

### Use an HttpOnly cookie immediately

**Deferred.** A cookie-based session would require CSRF, cookie-domain, same-site and deployment decisions beyond the bounded PI-1 contract.

### Introduce refresh tokens

**Deferred.** Refresh-token rotation, persistence and revocation are not required for the waiting-room/existing-patient pilot.

### Build intake in the authenticated home

**Rejected.** Intake ownership, draft lifecycle and questionnaire writes belong to PI-2 and must remain independently reviewable.

## Consequences

### Positive

- staff and patient browser identities remain least-privilege and visibly separate;
- neither bearer token is sent to the other API boundary;
- activation credentials leave the address bar immediately;
- browser persistence does not retain sensitive patient auth state;
- the UI is ready for PI-2 without granting access to accepted clinical/commercial modules;
- the pilot has a documented recovery operation.

### Trade-offs

- page refresh requires the patient to log in again;
- reception must continue delivering activation/recovery links manually;
- the same browser tab cannot preserve a staff session while entering the patient surface;
- there is no remote self-service recovery;
- intake remains unavailable until PI-2.

## Security requirements preserved

- Tenant remains the primary boundary;
- patient policies have no platform override;
- route realm is not authorization authority;
- no public patient search or demographic claim flow;
- no token/password in logs or audit;
- generic public errors and rate limiting remain backend-enforced;
- frontend authorization is defense in depth, not the source of truth.

## Follow-up

After PI-1 is accepted, the next bounded slice is PI-2 — Intake Draft and Self-Service Capture (#5). PI-2 must separately decide editable proposal fields, phone/contact scope, waiting-room link ownership, draft expiry and save behavior before adding questionnaire writes.
