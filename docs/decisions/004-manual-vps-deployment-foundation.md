# ADR 004 — Manual VPS Deployment Foundation

- **Status:** Accepted
- **Date:** 2026-05-05
- **Decision Type:** Operational deployment foundation
- **Scope:** VPS deployment, Nginx, SQL Server, manual release process, future GitHub Actions automation
- **Applies To:** Deployment, Operations, Frontend, Backend, Database

---

## Context

BigSmile now has its first manual VPS deployment.

The deployment target is a VPS running Ubuntu with:

- Nginx as reverse proxy
- SQL Server 2022 local instance on `127.0.0.1,14330`
- BigSmile API running behind Nginx on `127.0.0.1:5010`
- BigSmile frontend served by Nginx
- HTTPS enabled for `bigsmile.com.mx` and `www.bigsmile.com.mx`
- Basic Auth enabled as temporary pilot protection

The current deployment was done manually and validated before automating it.

This manual process intentionally avoided touching other existing VPS sites, services, databases, or ports.

---

## Decision

BigSmile will use an incremental release directory deployment model on the VPS.

Canonical VPS paths:

```text
/var/www/bigsmile/
  current -> /var/www/bigsmile/releases/<timestamp>
  releases/
  shared/
    appsettings.Production.json
    logs/
    uploads/
  backups/

Runtime configuration lives outside the repository:

/etc/bigsmile/bigsmile-api.env
/var/www/bigsmile/shared/appsettings.Production.json

The API runs as:

bigsmile-api.service
127.0.0.1:5010

Nginx serves:

https://bigsmile.com.mx
https://www.bigsmile.com.mx

The initial pilot deployment remains protected by Basic Auth while the default seeded admin credential exists.

Current Operational State

The active release after the frontend localization deployment is:

/var/www/bigsmile/releases/20260505151958

The previous release is preserved for rollback.

The first accepted manual deployment validated:

frontend served over HTTPS
backend reachable behind Nginx
/api/auth/me returns 401 Unauthorized without bearer token
SQL Server database BigSmile created
EF migrations applied
seed data created through a controlled temporary bootstrap
service reverted to Production after bootstrap
Basic Auth enabled for pilot protection
Constraints

Do not commit production secrets.

Do not store these in the repository:

SQL passwords
JWT secrets
private keys
certificates
VPS credentials
Basic Auth password

Do not expose the pilot publicly without Basic Auth while the seed admin account exists.

Do not reuse occupied ports from other VPS services.

The reserved API port for BigSmile is:

127.0.0.1:5010
Deployment Strategy

The current safe manual deployment flow is:

Build artifacts outside the VPS.
Upload packaged artifacts to /tmp.
Create a new timestamped release directory.
Copy or extract backend/frontend artifacts.
Link production config from shared.
Move /var/www/bigsmile/current to the new release.
Validate Nginx.
Reload Nginx.
Restart API only when backend changed.
Validate HTTPS and API.

Frontend-only deployments do not require API restart.

Backend deployments require service restart and migration planning.

Rollback Strategy

Rollback is symlink-based:

/var/www/bigsmile/current -> previous release

For frontend-only changes, rollback requires:

restoring the previous current symlink
reloading Nginx

For backend/database changes, rollback may also require:

service restart
database backup/restore decision
migration compatibility review
Future GitHub Actions Automation

GitHub Actions should automate the already validated manual flow.

Initial automation should:

build frontend and backend
run tests
package artifacts
upload artifacts to the VPS over SSH
create timestamped releases
preserve rollback files
reload Nginx
restart API only when backend artifacts change

GitHub Actions must use repository secrets for:

VPS host
VPS user
SSH private key
optional deployment settings if needed

Production secrets must remain on the VPS and not be copied into GitHub Actions unless explicitly needed.

Risks and Follow-up

Current known operational follow-ups:

Replace or disable the seeded admin@bigsmile.local / Admin123! account before removing Basic Auth.
Persist ASP.NET DataProtection keys instead of using ephemeral keys.
Decide migration strategy before backend automation.
Remove db_ddladmin from bigsmile_app once migrations are no longer applied by app/runtime.
Review EF warnings about required relationships with global query filters.
Remove or replace residual non-BigSmile frontend branding assets.
Add GitHub Actions deployment only after this manual flow remains stable.
Consequences

The VPS now has a safe, reversible deployment foundation.

The next deployment automation should not invent a new strategy. It should encode this manual process with the same release/rollback model.
