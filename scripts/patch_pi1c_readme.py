from pathlib import Path

path = Path(__file__).resolve().parents[1] / "README.md"
text = path.read_text(encoding="utf-8")

old_phase = """* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006, ADR 012 and ADR 013
* PI-1A completed account/invitation domain and persistence; PI-1B completed tenant-admin invitation issuance/list/revoke with hash-at-rest and append-only audit
* PI-1 proceeds next through PI-1C (#24) and PI-1D (#25) before intake begins
* PI-2 to PI-4 remain pending for intake, clinic review/application and audit hardening"""
new_phase = """* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006, ADR 012, ADR 013 and ADR 014
* PI-1A completed account/invitation persistence; PI-1B completed tenant-admin invitation lifecycle; PI-1C completed the separate patient activation/login/self-session backend
* PI-1 proceeds next through PI-1D (#25) for Angular patient auth, e2e and the assisted-recovery runbook before intake begins
* PI-2 to PI-4 remain pending for intake, clinic review/application and audit hardening"""

old_environment = """> Update this section to match the current environment contract of the repository; the base foundation is already established.

Expected configuration includes:

* SQL Server connection string
* Authentication secrets
* Tenant and platform configuration
* File storage configuration
* Notifications configuration"""
new_environment = """Expected configuration includes:

* SQL Server connection string
* Existing staff JWT secret, issuer and audience
* A **distinct** patient-portal JWT secret, issuer and audience:
  * `PatientPortal__Jwt__Secret`
  * `PatientPortal__Jwt__Issuer`
  * `PatientPortal__Jwt__Audience`
  * optional `PatientPortal__Jwt__AccessTokenLifetimeMinutes` (default `60`)
* Optional bounded patient-auth settings under `PatientPortal__Authentication__*` for PBKDF2 iterations, password length, failed attempts and lockout duration
* Optional bounded public rate-limit settings under `PatientPortal__RateLimits__*`
* Tenant and platform configuration
* File storage configuration
* Notifications configuration

Production must provide a patient-portal signing secret different from the staff JWT secret. The API fails fast when required patient JWT settings are absent or unsafe. Development placeholders are not production credentials."""

for old, new in ((old_phase, new_phase), (old_environment, new_environment)):
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"Expected exactly one README occurrence, found {count}: {old[:100]!r}")
    text = text.replace(old, new, 1)

path.write_text(text, encoding="utf-8")
print("PI-1C README reconciled.")
