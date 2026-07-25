from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace_once(relative_path: str, old: str, new: str) -> None:
    path = ROOT / relative_path
    text = path.read_text(encoding="utf-8")
    count = text.count(old)
    if count != 1:
        raise RuntimeError(
            f"Expected exactly one occurrence in {relative_path}, found {count}: {old[:120]!r}"
        )
    path.write_text(text.replace(old, new, 1), encoding="utf-8")


# Canonical STATE.
replace_once(
    "STATE — BigSmile.md",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 fija el baseline de acceso; y ADR 013 fija la gestión de invitaciones staff mediante `patientportal.invitation.manage` solo para `TenantAdmin`, sin `TenantUser`, `PlatformAdmin` ni platform override. Phase 2.1 está activa; PI-1A y PI-1B quedan completados sin abrir todavía activación/login públicos ni captura de intake.",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 fija el baseline de acceso; ADR 013 fija la gestión de invitaciones staff; y ADR 014 acepta la autenticación/sesión pública de paciente con realm por `Tenant.Subdomain`, password hash versionado, bearer scheme separado, `SessionVersion`, rate limiting y recovery asistido. Phase 2.1 está activa; PI-1A, PI-1B y PI-1C quedan completados. PI-1D sigue pendiente y no existe todavía frontend paciente ni captura de intake.",
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] PI-1B — Staff Invitation Lifecycle (#23) queda completado mediante PR #28: permiso dedicado solo para `TenantAdmin`, endpoints staff tenant-scoped de emisión/listado/revocación, token criptográfico one-time con hash-at-rest, TTL configurable de 24 horas, replacement explícito y bitácora append-only. PI-1C (#24) es el siguiente gate; PI-1D (#25) y PI-2 a PI-4 permanecen pendientes.",
    "[Hecho] PI-1B — Staff Invitation Lifecycle (#23) queda completado mediante PR #28: permiso dedicado solo para `TenantAdmin`, endpoints staff tenant-scoped de emisión/listado/revocación, token criptográfico one-time con hash-at-rest, TTL configurable de 24 horas, replacement explícito y bitácora append-only.\n\n[Hecho] PI-1C — Patient Activation, Login and Self-Session (#24) queda completado mediante PR #29: realm por `Tenant.Subdomain`, activación single-use transaccional, Identity V3/PBKDF2 con parámetros explícitos, JWT patient-only separado, validación server-side de `SessionVersion`, lockout/rate limiting/anti-enumeración, recovery asistido solo `TenantAdmin` y auditoría append-only. PI-1D (#25) es el siguiente gate; PI-2 a PI-4 permanecen pendientes.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A y PI-1B completados; PI-1C es el siguiente gate; sin activación/login público, JWT de paciente, frontend paciente ni intake todavía.",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A, PI-1B y PI-1C completados; PI-1D es el siguiente gate. La autenticación backend de pacientes existe, pero no hay frontend paciente ni intake todavía.",
)
replace_once(
    "STATE — BigSmile.md",
    "- ADR 013 — `docs/decisions/013-patient-portal-invitation-management.md`.\n- Plan general — `docs/patient-intake-and-portal-plan.md`.",
    "- ADR 013 — `docs/decisions/013-patient-portal-invitation-management.md`.\n- ADR 014 — `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`.\n- Plan general — `docs/patient-intake-and-portal-plan.md`.",
)
replace_once(
    "STATE — BigSmile.md",
    "1. Preservar PI-1A (#22) y PI-1B (#23) como foundations completados, sin activación/login público ni intake.\n\n2. Preparar PI-1C (#24) únicamente después de decidir explícitamente el formato versionado de password hash, audience/scope y lifetime del JWT de paciente, comparación de token, rate limits y enforcement de `SessionVersion`.\n\n3. Mantener PI-1D (#25) bloqueado hasta aceptar PI-1C; no construir frontend sobre contratos de auth no cerrados.\n\n4. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.\n\n5. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n6. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n7. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n8. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n9. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
    "1. Preservar PI-1A (#22), PI-1B (#23) y PI-1C (#24) como foundations completados, sin intake ni acceso paciente a módulos canónicos.\n\n2. Abrir PI-1D (#25) únicamente para el frontend Angular paciente separado, estados de activación/login/session, e2e y runbook de recovery asistido.\n\n3. Mantener el access token solo en memoria del frontend; no introducir `localStorage`, refresh token ni recuperación remota en PI-1D.\n\n4. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.\n\n5. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n6. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n7. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n8. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n9. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Patient-facing identity** — [Hecho] PI-1A y PI-1B ya establecen persistencia e invitaciones staff, pero la frontera todavía no permite autenticación de pacientes. No reutiliza staff membership, no acepta `PatientId`/`TenantId` como autoridad, no permite platform override, guarda solo hashes de invitación y no aplica cambios canónicos.",
    "**Patient-facing identity** — [Hecho] PI-1A a PI-1C ya establecen persistencia, invitaciones staff y autenticación/sesión backend de paciente. La frontera usa realm por subdominio, scheme/issuer/audience/secret separados, no emite roles/permisos staff, valida `SessionVersion` en cada request, no acepta `PatientId`/`TenantId` como autoridad pública, no permite platform override y no aplica cambios canónicos. El frontend paciente y el intake siguen pendientes.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 013; Phase 2.1 abierta; PI-1A y PI-1B completados; PI-1C pendiente de decisión de auth/session.\n\n**Contexto:** PI-1A ya establecía cuentas e invitaciones tenant-owned. El cliente autorizó explícitamente que la gestión de invitaciones use un permiso dedicado solo para `TenantAdmin`, sin `TenantUser`, `PlatformAdmin` ni platform override.\n\n**Decisión:** Aceptar PI-1B mediante ADR 013 con emisión/listado/revocación staff tenant-scoped, token one-time de 256 bits, SHA-256 hash-at-rest, TTL configurable, replacement determinista y bitácora append-only.\n\n**Consecuencias:** El repositorio ya puede generar y entregar manualmente una invitación a un paciente existente, pero todavía no puede consumirla ni autenticar al paciente. PI-1C debe cerrar password hashing, JWT/audience/scope, comparación constante, consumo transaccional, anti-enumeración, rate limiting, lockout y session invalidation. Payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 014; Phase 2.1 abierta; PI-1A, PI-1B y PI-1C completados; PI-1D es el siguiente gate.\n\n**Contexto:** PI-1A/PI-1B ya establecían cuentas e invitaciones tenant-owned. El cliente autorizó explícitamente el realm, password hashing, JWT/session, anti-abuse y recovery necesarios para abrir el primer runtime público.\n\n**Decisión:** Aceptar PI-1C mediante ADR 014 con activación single-use transaccional, password hash Identity V3/PBKDF2 versionado, bearer scheme separado, token de 60 minutos sin refresh, `SessionVersion` server-side, lockout 5/15, rate limiting configurable, recovery `TenantAdmin`-only y auditoría append-only.\n\n**Consecuencias:** El backend ya puede activar y autenticar pacientes existentes sin otorgar permisos staff ni acceso canónico. PI-1 permanece abierto porque PI-1D debe entregar el frontend paciente, e2e y runbook. Intake, revisión/aplicación, payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
)

# README.
replace_once(
    "README.md",
    "Phase 2.1 is now explicitly opened through PI-1A, but only its account/invitation domain and persistence foundation is active. No public patient auth or intake capability is accepted yet. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
    "Phase 2.1 is active through PI-1C. Account/invitation persistence, tenant-admin invitation management and the separate patient activation/login/self-session backend are accepted. The patient Angular experience and intake remain unavailable until PI-1D and PI-2. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
)
replace_once(
    "README.md",
    "* **Latest Phase 2.1 slice completed:** **PI-1B — Staff-issued patient portal invitation lifecycle**\n* **Next slice:** **PI-1C — Patient activation, login and self-session boundary**, pending auth/session decisions\n* **Public patient runtime:** not exposed; activation/login/JWT/frontend/intake remain pending",
    "* **Latest Phase 2.1 slice completed:** **PI-1C — Patient activation, login and self-session boundary**\n* **Next slice:** **PI-1D — Patient auth frontend and security closure**\n* **Public patient runtime:** backend activation/login/self-session accepted; Angular patient UI and intake remain pending",
)
replace_once(
    "README.md",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. PI-1A and PI-1B are completed; PI-1C is the next gated slice. Public activation/login, patient JWT/session, intake, review/apply and final audit hardening remain pending. Payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics and the full Patient Portal remain future bounded work.",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. PI-1A through PI-1C are completed; PI-1D is the next gated slice. Patient activation/login and self-session exist only as backend contracts; Angular patient UX, intake, review/apply and final audit hardening remain pending. Payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics and the full Patient Portal remain future bounded work.",
)

# AGENTS.
replace_once(
    "AGENTS.md",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active, PI-1A and PI-1B completed, PI-1C next and auth/session-gated",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active, PI-1A through PI-1C completed, PI-1D next",
)
replace_once(
    "AGENTS.md",
    "- ADR 013 — `docs/decisions/013-patient-portal-invitation-management.md`\n- PI-1A — issue #22 / PR #26\n- PI-1B — issue #23 / PR #28",
    "- ADR 013 — `docs/decisions/013-patient-portal-invitation-management.md`\n- ADR 014 — `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`\n- PI-1A — issue #22 / PR #26\n- PI-1B — issue #23 / PR #28\n- PI-1C — issue #24 / PR #29",
)
replace_once(
    "AGENTS.md",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006, access baseline/opening accepted in ADR 012 and invitation management accepted in ADR 013\n- PI-1 is active; PI-1A (#22) and PI-1B (#23) are completed\n- PI-1C (#24) is next and requires explicit password/JWT/session decisions\n- PI-1D (#25) remains sequentially gated\n- PI-2 to PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006, access/opening in ADR 012, invitation management in ADR 013 and patient auth/session in ADR 014\n- PI-1 is active; PI-1A (#22), PI-1B (#23) and PI-1C (#24) are completed\n- PI-1D (#25) is the next slice and must close frontend/e2e/runbook without adding intake\n- PI-2 to PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability",
)
replace_once(
    "AGENTS.md",
    "# Immediate objective\nPreserve completed PI-1A/PI-1B and resolve the auth/session decisions required before opening `PI-1C — Patient activation, login and self-session boundary`.",
    "# Immediate objective\nPreserve completed PI-1A through PI-1C and open only `PI-1D — Patient auth frontend and security closure` before any intake work.",
)
replace_once(
    "AGENTS.md",
    "- preserve PI-1B `TenantAdmin`-only permission, no platform override, token hash-at-rest, replacement semantics and append-only audit\n- do not open activation/login until password-hash versioning, patient JWT audience/scope/lifetime, comparison, rate limits and session invalidation are explicitly accepted",
    "- preserve PI-1B `TenantAdmin`-only invitation permission, no platform override, token hash-at-rest, replacement semantics and append-only audit\n- preserve PI-1C separate patient bearer scheme/secret/audience, fixed-time token verification, `SessionVersion`, generic errors, rate limiting, lockout and assisted recovery\n- keep patient access tokens in frontend memory only and keep intake/canonical modules outside PI-1D",
)

# PROJECT_MAP.
replace_once(
    "PROJECT_MAP.md",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1A and PI-1B completed, PI-1C next and auth/session-gated",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1A through PI-1C completed, PI-1D next",
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7 and completed PI-1A/PI-1B while preparing the auth/session-gated PI-1C slice:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* preserve tenant-owned patient portal account/invitation persistence completed in PI-1A\n* preserve PI-1B staff invitation endpoints, `TenantAdmin`-only permission, no platform override, token hash-at-rest and append-only audit\n* keep activation/login, patient JWT, frontend auth and intake outside accepted PI-1B\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1A and PI-1B are accepted with migrations, tests, docs and CI; PI-1C is the next gated step",
    "Preserve Releases 1 through 7 and completed PI-1A through PI-1C while preparing PI-1D:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* preserve tenant-owned patient portal account/invitation persistence completed in PI-1A\n* preserve PI-1B staff invitation endpoints, `TenantAdmin`-only permission, no platform override, token hash-at-rest and append-only audit\n* preserve PI-1C activation/login/self-session backend, separate bearer scheme, `SessionVersion`, rate limits, lockout and recovery permission\n* keep intake and canonical module access outside PI-1D\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1A through PI-1C are accepted with migrations, tests, docs and CI; PI-1D is the next gated step",
)
replace_once(
    "PROJECT_MAP.md",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation issuance/list/revoke and security audit. Neither may reuse staff membership semantics for patient access.",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013/014. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation management; PI-1C owns the separate patient activation/login/self-session backend, password hashing, JWT scheme, lockout, rate limiting, session validation and assisted recovery. It never reuses staff membership semantics or staff permissions.",
)

# Roadmap.
replace_once(
    "docs/product-roadmap.md",
    "Active after formal MVP acceptance. PI-1A and PI-1B are completed; PI-1C is next and awaits explicit auth/session decisions.",
    "Active after formal MVP acceptance. PI-1A, PI-1B and PI-1C are completed; PI-1D is next.",
)
replace_once(
    "docs/product-roadmap.md",
    "Active under ADR 006, ADR 012 and ADR 013. PI-1A (#22) and PI-1B (#23) are completed; PI-1C (#24) is next but not opened. No public patient runtime is accepted yet.",
    "Active under ADR 006, ADR 012, ADR 013 and ADR 014. PI-1A (#22), PI-1B (#23) and PI-1C (#24) are completed; PI-1D (#25) is next. The backend public auth boundary is accepted, but the patient Angular experience and intake are not.",
)
replace_once(
    "docs/product-roadmap.md",
    "   3. PI-1C activation/login/self-session — #24\n   4. PI-1D patient auth frontend/security closure — #25",
    "   3. PI-1C activation/login/self-session — #24 — completed\n   4. PI-1D patient auth frontend/security closure — #25 — next",
)
replace_once(
    "docs/product-roadmap.md",
    "- ADR 013: `docs/decisions/013-patient-portal-invitation-management.md`\n- plan: `docs/patient-intake-and-portal-plan.md`",
    "- ADR 013: `docs/decisions/013-patient-portal-invitation-management.md`\n- ADR 014: `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`\n- plan: `docs/patient-intake-and-portal-plan.md`",
)

# General plan.
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** In progress; PI-1 active; PI-1A and PI-1B completed; PI-1C next and auth/session-gated",
    "- **Status:** In progress; PI-1 active; PI-1A through PI-1C completed; PI-1D next",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Architecture decisions:** ADR 006, ADR 012 and ADR 013",
    "- **Architecture decisions:** ADR 006, ADR 012, ADR 013 and ADR 014",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Last updated:** 2026-07-24",
    "- **Last updated:** 2026-07-25",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: active; PI-1A domain/persistence and PI-1B staff invitation lifecycle completed; PI-1C is the next gated slice.",
    "- Phase 2.1: active; PI-1A domain/persistence, PI-1B staff invitations and PI-1C patient auth/session completed; PI-1D is next.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-1 access/invitations | Active; PI-1A and PI-1B completed; PI-1C next | Issues #4 and #22–#25 / PRs #26 and #28 |\n| PI-2 intake draft | Planned; not implemented | Issue #5 |\n| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |\n| PI-4 audit/hardening | Planned; not implemented | Issue #7 |\n| Patient-facing backend/API/database/frontend | Not started | No implementation PR |",
    "| PI-1 access/invitations | Active; PI-1A through PI-1C completed; PI-1D next | Issues #4 and #22–#25 / PRs #26, #28 and #29 |\n| PI-2 intake draft | Planned; not implemented | Issue #5 |\n| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |\n| PI-4 audit/hardening | Planned; not implemented | Issue #7 |\n| Patient-facing backend/API/database | Auth foundation implemented through PI-1C | PRs #26, #28 and #29 |\n| Patient-facing frontend/intake | Not implemented | PI-1D / PI-2 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "### Approved for PI-1 under ADR 012 and ADR 013",
    "### Approved for PI-1 under ADR 012, ADR 013 and ADR 014",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Assisted recovery through session revocation and invitation reissue.",
    "- Assisted recovery through session revocation and invitation reissue.\n- Patient recurrent login uses `Tenant.Subdomain + LoginName + password`; internal `TenantId` is never a public selector.\n- Patient password hashes use a dedicated versioned Identity V3/PBKDF2 format with explicit work factor and rehash support.\n- Patient JWT uses a separate scheme/secret/issuer/audience, 60-minute access token, no refresh token and server-side `SessionVersion` validation.\n- Activation/login use generic responses and configurable fixed-window rate limits.\n- Assisted recovery uses `patientportal.account.recover` for `TenantAdmin` only, without platform override.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "1. Preserve PI-1A / issue #22 and PI-1B / issue #23 as completed through PRs #26 and #28.\n2. Resolve PI-1C decisions for password-hash format/versioning, patient JWT audience/scope/lifetime, token comparison, rate limiting, lockout enforcement and `SessionVersion`.\n3. Open only PI-1C / issue #24 after those decisions are accepted.\n4. Keep PI-1D/#25 sequentially gated.\n5. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
    "1. Preserve PI-1A / #22, PI-1B / #23 and PI-1C / #24 as completed through PRs #26, #28 and #29.\n2. Open only PI-1D / #25 for the separate Angular patient-auth area, in-memory session state, e2e and recovery runbook.\n3. Do not add intake/questionnaire or canonical module access to PI-1D.\n4. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "**Consequence:** PI-1A and PI-1B now provide tenant-owned account/invitation persistence plus staff issuance/revocation and audit, but public auth and intake remain unavailable until PI-1C/PI-1D and later gates. PI-2 to PI-4 remain unimplemented.",
    "**Consequence:** PI-1A through PI-1C now provide tenant-owned account/invitation persistence, staff invitation lifecycle and a separate patient activation/login/self-session backend. The capability is not operational for patients until PI-1D supplies frontend/e2e/runbook, and intake remains unavailable until PI-2. PI-2 to PI-4 remain unimplemented.",
)

# Tenant and architecture boundaries.
replace_once(
    "docs/tenant-model.md",
    "Patient-facing identity is separate from staff identity under ADR 006 and ADR 012.",
    "Patient-facing identity is separate from staff identity under ADR 006, ADR 012, ADR 013 and ADR 014.",
)
replace_once(
    "docs/tenant-model.md",
    "- `PatientPortalAccount`, `PatientPortalInvitation` and patient-portal security audit entries are tenant-owned records\n- invitation management requires `patientportal.invitation.manage`, initially only for `TenantAdmin`, with no platform override\n- `LoginName` uniqueness is scoped by `TenantId`\n- a portal account links to at most one canonical Patient in Phase 2.1\n- patient accounts do not use `UserTenantMembership`, staff roles or tenant-wide permissions\n- patient policies have no platform override\n- `TenantId`, portal account and linked Patient come from verified server context\n- phone, date of birth and contact data cannot be used to claim a record publicly\n- Phase 2.1 proceeds through PI-1A to PI-1D before intake is opened",
    "- `PatientPortalAccount`, `PatientPortalInvitation` and patient-portal security/authentication audit entries are tenant-owned records\n- invitation management requires `patientportal.invitation.manage`; assisted recovery requires `patientportal.account.recover`; both are initially `TenantAdmin`-only with no platform override\n- `LoginName` uniqueness is scoped by `TenantId`; recurrent public login selects the tenant by unique `Tenant.Subdomain`, never by internal `TenantId`\n- a portal account links to at most one canonical Patient in Phase 2.1\n- patient accounts do not use `UserTenantMembership`, staff roles or tenant-wide permissions\n- patient tokens use a distinct scheme/secret/issuer/audience and contain only account, Tenant, Patient, patient scope, session version and token id\n- every patient-authenticated request revalidates active Tenant/Patient/account plus `SessionVersion`\n- patient policies have no platform override\n- `TenantId`, portal account and linked Patient come from verified server context\n- phone, date of birth and contact data cannot be used to claim a record publicly\n- Phase 2.1 proceeds through PI-1A to PI-1D before intake is opened",
)
replace_once(
    "docs/architecture.md",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary, ADR 012 defines the approved pilot access baseline, and ADR 013 restricts invitation management to tenant-scoped `TenantAdmin` with token hash-at-rest and append-only audit.",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary, ADR 012 defines the pilot access baseline, ADR 013 restricts invitation management, and ADR 014 establishes the separate patient bearer scheme, versioned password hashing, tenant realm, `SessionVersion`, abuse controls, recovery and authentication audit.",
)

# Parent ADR updates.
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "Phase 2.1 — active; PI-1A and PI-1B completed; PI-1C next",
    "Phase 2.1 — active; PI-1A through PI-1C completed; PI-1D next",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "On 2026-07-25 the client approved ADR 013: `patientportal.invitation.manage` only for `TenantAdmin`, no `TenantUser`, `PlatformAdmin` or platform override, with one-time token issuance and append-only invitation audit.",
    "On 2026-07-25 the client approved ADR 013: `patientportal.invitation.manage` only for `TenantAdmin`, no `TenantUser`, `PlatformAdmin` or platform override, with one-time token issuance and append-only invitation audit.\n\nOn 2026-07-25 the client approved ADR 014: tenant realm by subdomain, dedicated versioned password hashing, separate patient JWT/scheme, transactional activation, rate limiting, lockout, server-side session invalidation and TenantAdmin-only assisted recovery.",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- PI-1B staff invitation lifecycle completed through PR #28 under ADR 013.\n- PI-1C is next; PI-1D and PI-2–PI-4 are not implemented.\n- Public patient API/auth/frontend and intake: not started.",
    "- PI-1B staff invitation lifecycle completed through PR #28 under ADR 013.\n- PI-1C patient activation/login/self-session completed through PR #29 under ADR 014.\n- PI-1D and PI-2–PI-4 are not implemented.\n- Public patient auth API exists; patient frontend and intake are not implemented.",
)
replace_once(
    "docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md",
    "- exact patient password hashing implementation and version marker — PI-1C;\n- staff permission for invitation management — PI-1B;\n- waiting-room token entity/runtime and draft ownership — PI-2;",
    "- patient password hashing/JWT/session baseline — resolved in ADR 014 / PI-1C;\n- staff permission for invitation management — resolved in ADR 013 / PI-1B;\n- waiting-room token entity/runtime and draft ownership — PI-2;",
)
replace_once(
    "docs/decisions/013-patient-portal-invitation-management.md",
    "After PI-1B is accepted, the only next PI-1 slice is PI-1C (#24): patient activation, login and self-session boundary. PI-1C must separately decide and validate password-hash versioning, patient JWT audience/scope, token comparison, transactional single-use consumption, anti-enumeration, rate limiting, lockout enforcement and session invalidation.",
    "PI-1C (#24) is accepted through ADR 014 and PR #29. It implements password-hash versioning, separate patient JWT/scheme, fixed-time token verification, transactional single-use activation, anti-enumeration, rate limiting, lockout, server-side `SessionVersion` validation and assisted recovery. PI-1D (#25) is the only next PI-1 slice.",
)

# UX reconciliation.
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "- architecture accepted in ADR 006, access baseline/opening accepted in ADR 012 and invitation management accepted in ADR 013;\n- PI-1A domain/persistence and PI-1B staff invitation lifecycle are completed;\n- no public patient auth or intake UI is available yet;\n- PI-1C to PI-1D and PI-2 to PI-4 remain pending;",
    "- architecture accepted in ADR 006, access baseline/opening in ADR 012, invitation management in ADR 013 and patient auth/session in ADR 014;\n- PI-1A domain/persistence, PI-1B staff invitations and PI-1C backend auth/session are completed;\n- no patient Angular UI or intake UI is available yet;\n- PI-1D and PI-2 to PI-4 remain pending;",
)
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "| Patient Intake/Portal | no | no | none accepted | none | none | none | Planned Phase 2.1 | not implemented |",
    "| Patient Intake/Portal | auth backend yes | no | invitation management + activation/login/me/logout/recovery | dedicated invitation/recovery + patient self policy | yes | yes | PI-1A–PI-1C accepted; PI-1D/PI-2 pending | patient UI not implemented |",
)

print("PI-1C documentation reconciled.")
