# Patient Portal Assisted Activation and Recovery Runbook

- **Status:** Pilot operational baseline
- **Scope:** Existing-patient activation and staff-assisted recovery
- **Applies To:** PI-1B, PI-1C and PI-1D
- **Out of scope:** remote recovery provider, waiting-room new-patient intake, questionnaire capture

## 1. Purpose

Provide clinic staff with a safe, repeatable procedure to:

1. issue initial patient access;
2. deliver the one-time activation link manually;
3. revoke an unused invitation when necessary;
4. recover an existing account without using demographic data as identity proof;
5. avoid leaking tokens into logs, notes or messaging automation.

## 2. Required conditions

Before using the workflow, verify:

- the Tenant is active;
- the Tenant has an active unique `Subdomain`;
- the Patient exists and is active inside the current Tenant;
- the staff user is a `TenantAdmin` with a freshly issued staff JWT;
- the API and Angular frontend use HTTPS in production;
- the patient JWT secret, issuer and audience are configured independently from staff JWT settings;
- database migrations through `AddPatientPortalAuthenticationBoundary` are applied;
- reverse-proxy forwarded headers are restricted to trusted proxies before relying on client IP rate limiting.

A staff JWT issued before the portal permissions were deployed may not contain the required permission claims. Sign out and sign in again before troubleshooting authorization.

## 3. Issue initial access

### Staff API

```http
POST /api/patients/{patientId}/portal-invitations
Authorization: Bearer <staff-token>
```

Required permission:

```text
patientportal.invitation.manage
```

Initial mapping: `TenantAdmin` only, inside the resolved tenant, with no platform override.

### Expected response

The successful response contains the raw activation token exactly once together with invitation metadata.

Immediately construct the patient URL:

```text
https://<public-bigsmile-host>/patient-portal/activate#token=<raw-activation-token>
```

The fragment form is mandatory for the pilot. Do not convert it to a query parameter or route segment.

### Delivery methods allowed in the pilot

- show a QR code generated locally at reception;
- print the link/QR and hand it directly to the patient;
- copy the link directly to the patient's device while present;
- use another clinic-controlled manual method approved for the pilot.

Automated email, SMS or WhatsApp delivery is not part of PI-1.

## 4. Token handling rules

Treat the raw invitation token as a temporary password.

Never:

- save it in Patient notes or ClinicalRecord;
- paste it into issue trackers, logs, analytics or screenshots;
- store it in local/session storage;
- send it in a URL query string;
- attempt to retrieve it later from the database;
- reuse an older token after issuing a replacement.

Only the token hash is persisted. Staff cannot recover the raw token after the issue response is gone. Issue a replacement instead.

## 5. Patient activation steps

1. Patient opens the activation link.
2. Angular reads the fragment and immediately removes it from the address bar.
3. Patient chooses a `LoginName` and a password of at least 12 characters.
4. Patient confirms the password.
5. The frontend submits the token only in the request body.
6. On success, the patient receives an in-memory access token and reaches their bounded patient home.
7. The patient should record their `LoginName` privately; the clinic must not store their password.

The browser does not persist patient authentication. A page refresh or browser restart requires recurrent login.

## 6. Recurrent login

The recurrent login URL is tenant-realm scoped:

```text
https://<public-bigsmile-host>/patient-portal/<tenant-subdomain>/login
```

The patient enters:

- `LoginName`;
- password.

Do not provide or request an internal `TenantId`.

All invalid credential states use a generic message. Staff should not infer account existence from the public response.

## 7. Revoke an unused invitation

### Staff API

```http
DELETE /api/patients/{patientId}/portal-invitations/{invitationId}
Authorization: Bearer <staff-token>
```

Use this when:

- the printed link was lost;
- the link may have been exposed;
- the wrong device received the link;
- the patient no longer needs initial access.

After revocation, issue a new invitation. Do not tell the patient to retry the revoked token.

## 8. Assisted account recovery

Use recovery only after clinic staff verifies the patient through the clinic's approved in-person/support procedure. Phone, email, date of birth or demographic data alone are not sufficient proof inside the product.

### Staff API

```http
POST /api/patients/{patientId}/portal-account/recovery
Authorization: Bearer <staff-token>
```

Required permission:

```text
patientportal.account.recover
```

Initial mapping: `TenantAdmin` only, inside the resolved tenant, with no platform override.

### Recovery effects

The operation:

1. invalidates existing patient sessions by incrementing `SessionVersion`;
2. puts the account into recovery state;
3. supersedes outstanding invitations;
4. creates a new one-time invitation;
5. records append-only security audit entries;
6. returns the new raw token exactly once.

Construct and deliver a new fragment link using the same rules as initial activation.

The patient may choose a replacement `LoginName` and password during recovery activation, subject to tenant-scoped uniqueness.

## 9. Generic failure handling

The public UI intentionally does not distinguish:

- unknown token;
- expired token;
- revoked token;
- consumed token;
- unknown tenant realm;
- unknown login name;
- incorrect password;
- inactive Tenant/Patient/account;
- active lockout.

Operational response:

1. confirm the patient is using the intended clinic URL;
2. wait for the 15-minute lockout window when repeated failures are suspected;
3. never ask for the current password;
4. revoke exposed invitations;
5. use assisted recovery when access cannot be restored safely.

Do not weaken generic errors for support convenience.

## 10. Logout and shared devices

Patients should use **End session** when leaving a shared device.

Logout:

- calls the server to increment `SessionVersion`;
- clears the in-memory patient token and session;
- invalidates prior patient access tokens immediately.

The local session is cleared even if server confirmation fails. In that case, staff may initiate assisted recovery if the device or token is suspected to be compromised.

## 11. Smoke test after deployment

Use a dedicated non-production Patient record or approved pilot record.

1. Sign in as `TenantAdmin` after deploy.
2. Issue an invitation.
3. Confirm response headers prevent caching of the raw token.
4. Open the fragment activation URL in a private browser window.
5. Confirm the fragment disappears immediately.
6. Activate with a new login/password.
7. Confirm the patient home loads and contains no staff navigation.
8. Confirm a patient token cannot call `/api/patients` or other staff endpoints.
9. Confirm a staff token is not attached to `/api/patient-portal/*` from Angular.
10. Log out and verify `/api/patient-portal/auth/me` rejects the prior token.
11. Start assisted recovery and verify the prior session remains invalid.
12. Verify the new recovery invitation activates once and rejects replay.

## 12. Incident response baseline

When an activation token or patient session may be exposed:

1. record the incident through the clinic's approved internal process without copying the secret;
2. revoke the invitation if it is still unused;
3. start assisted recovery for an activated account;
4. verify old sessions no longer pass `/me`;
5. issue a fresh link directly to the verified patient;
6. review append-only portal audit entries when staff visibility becomes available under PI-4.

PI-1 does not include remote account deactivation tooling, long-term retention rules or external incident automation. Those remain PI-4 decisions.

## 13. Deployment and rollback

Recommended deployment order:

```text
verify backup and configuration
  -> apply additive migrations
  -> deploy API
  -> deploy Angular frontend
  -> run smoke test
```

Required production environment values:

```text
PatientPortal__Jwt__Secret
PatientPortal__Jwt__Issuer
PatientPortal__Jwt__Audience
```

The patient secret must differ from the staff JWT secret.

Rollback of the frontend removes the patient browser surface but does not invalidate already issued patient JWTs. For a security rollback, also disable the patient API exposure or rotate the patient JWT secret according to the incident procedure. Do not drop audit tables as an operational rollback step.

## 14. Next product boundary

This runbook closes only the access foundation. Medical-information capture, waiting-room registration and patient-originated draft changes begin in PI-2 and remain subject to clinic review before canonical application.
