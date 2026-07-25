from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_once(relative_path: str, old: str, new: str) -> None:
    path = ROOT / relative_path
    text = path.read_text(encoding="utf-8")
    if old not in text:
        raise RuntimeError(f"Expected text not found in {relative_path}: {old[:120]!r}")
    updated = text.replace(old, new, 1)
    path.write_text(updated, encoding="utf-8")


# STATE — BigSmile.md
replace_once(
    "STATE — BigSmile.md",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 fija el baseline de acceso; ADR 013 fija la gestión de invitaciones staff; y ADR 014 acepta la autenticación/sesión pública de paciente con realm por `Tenant.Subdomain`, password hash versionado, bearer scheme separado, `SessionVersion`, rate limiting y recovery asistido. Phase 2.1 está activa; PI-1A, PI-1B y PI-1C quedan completados. PI-1D sigue pendiente y no existe todavía frontend paciente ni captura de intake.",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 fija el baseline de acceso; ADR 013 fija la gestión de invitaciones staff; ADR 014 acepta la autenticación/sesión pública de paciente; y ADR 015 fija la separación Angular, token en memoria y activación por fragment. PI-1 queda completado mediante PI-1A a PI-1D. Phase 2.1 continúa activa con PI-2 como siguiente slice sujeto a decisiones de alcance; la captura de intake todavía no está implementada."
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] PI-1 — Access and Invitation Foundation está activa. PI-1A — Account and Invitation Domain/Persistence quedó completado mediante PR #26 y merge commit `43ddb2e008ce07b4798c21409e3fe58b4839668d`.",
    "[Hecho] PI-1 — Access and Invitation Foundation queda completado mediante PI-1A a PI-1D. PI-1A — Account and Invitation Domain/Persistence quedó completado mediante PR #26 y merge commit `43ddb2e008ce07b4798c21409e3fe58b4839668d`."
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] PI-1C — Patient Activation, Login and Self-Session (#24) queda completado mediante PR #29: realm por `Tenant.Subdomain`, activación single-use transaccional, Identity V3/PBKDF2 con parámetros explícitos, JWT patient-only separado, validación server-side de `SessionVersion`, lockout/rate limiting/anti-enumeración, recovery asistido solo `TenantAdmin` y auditoría append-only. PI-1D (#25) es el siguiente gate; PI-2 a PI-4 permanecen pendientes.",
    "[Hecho] PI-1C — Patient Activation, Login and Self-Session (#24) queda completado mediante PR #29: realm por `Tenant.Subdomain`, activación single-use transaccional, Identity V3/PBKDF2 con parámetros explícitos, JWT patient-only separado, validación server-side de `SessionVersion`, lockout/rate limiting/anti-enumeración, recovery asistido solo `TenantAdmin` y auditoría append-only.\n\n[Hecho] PI-1D — Patient Auth Frontend and Security Closure (#25) queda completado mediante PR #30: route tree `/patient-portal/*` fuera del staff shell, activación con token en fragment y limpieza inmediata, sesión/token solo en memoria, interceptores/guards separados, login/home/logout acotados, pruebas frontend y runbook de recovery. PI-1 queda cerrado; PI-2 (#5) es el siguiente slice y todavía no está abierto."
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A, PI-1B y PI-1C completados; PI-1D es el siguiente gate. La autenticación backend de pacientes existe, pero no hay frontend paciente ni intake todavía.",
    "**Estado** — [Hecho] fase abierta; PI-1 completado mediante PI-1A a PI-1D; PI-2 es el siguiente slice sujeto a decisiones explícitas. La autenticación backend y el frontend Angular acotado de pacientes existen, pero no hay captura de intake todavía."
)
replace_once(
    "STATE — BigSmile.md",
    "- ADR 014 — `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`.\n- Plan general — `docs/patient-intake-and-portal-plan.md`.",
    "- ADR 014 — `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`.\n- ADR 015 — `docs/decisions/015-patient-portal-frontend-session-boundary.md`.\n- Cierre PI-1 — `docs/pi-1-patient-portal-access-and-security-closure.md`.\n- Runbook — `docs/patient-portal-assisted-recovery-runbook.md`.\n- Plan general — `docs/patient-intake-and-portal-plan.md`."
)
replace_once(
    "STATE — BigSmile.md",
    "1. Preservar PI-1A (#22), PI-1B (#23) y PI-1C (#24) como foundations completados, sin intake ni acceso paciente a módulos canónicos.\n\n2. Abrir PI-1D (#25) únicamente para el frontend Angular paciente separado, estados de activación/login/session, e2e y runbook de recovery asistido.\n\n3. Mantener el access token solo en memoria del frontend; no introducir `localStorage`, refresh token ni recuperación remota en PI-1D.\n\n4. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.",
    "1. Preservar PI-1 (#4) como foundation completado mediante PI-1A a PI-1D, sin acceso paciente a módulos canónicos.\n\n2. Resolver antes de abrir PI-2 (#5) los campos demográficos/contacto editables, el tratamiento de teléfonos, el ownership/lifecycle del link de sala de espera, la expiración de drafts y save explícito vs autosave acotado.\n\n3. Mantener tokens de paciente solo en memoria; no introducir `localStorage`, refresh token ni recuperación remota sin una decisión posterior.\n\n4. Mantener PI-3 y PI-4 no iniciados y prohibir aplicación canónica desde endpoints de paciente."
)
replace_once(
    "STATE — BigSmile.md",
    "**Patient-facing identity** — [Hecho] PI-1A a PI-1C ya establecen persistencia, invitaciones staff y autenticación/sesión backend de paciente. La frontera usa realm por subdominio, scheme/issuer/audience/secret separados, no emite roles/permisos staff, valida `SessionVersion` en cada request, no acepta `PatientId`/`TenantId` como autoridad pública, no permite platform override y no aplica cambios canónicos. El frontend paciente y el intake siguen pendientes.",
    "**Patient-facing identity** — [Hecho] PI-1A a PI-1D establecen persistencia, invitaciones staff, autenticación/sesión backend y frontend Angular separado. La frontera usa realm por subdominio, scheme/issuer/audience/secret separados, no emite roles/permisos staff, valida `SessionVersion` en cada request, no acepta `PatientId`/`TenantId` como autoridad pública, no permite platform override, mantiene tokens solo en memoria y no aplica cambios canónicos. El intake sigue pendiente."
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 014; Phase 2.1 abierta; PI-1A, PI-1B y PI-1C completados; PI-1D es el siguiente gate.\n\n**Contexto:** PI-1A/PI-1B ya establecían cuentas e invitaciones tenant-owned. El cliente autorizó explícitamente el realm, password hashing, JWT/session, anti-abuse y recovery necesarios para abrir el primer runtime público.\n\n**Decisión:** Aceptar PI-1C mediante ADR 014 con activación single-use transaccional, password hash Identity V3/PBKDF2 versionado, bearer scheme separado, token de 60 minutos sin refresh, `SessionVersion` server-side, lockout 5/15, rate limiting configurable, recovery `TenantAdmin`-only y auditoría append-only.\n\n**Consecuencias:** El backend ya puede activar y autenticar pacientes existentes sin otorgar permisos staff ni acceso canónico. PI-1 permanece abierto porque PI-1D debe entregar el frontend paciente, e2e y runbook. Intake, revisión/aplicación, payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 015; Phase 2.1 abierta; PI-1 completado; PI-2 es el siguiente slice sujeto a decisiones de alcance.\n\n**Contexto:** PI-1A a PI-1C ya establecían cuentas, invitaciones y auth/session backend tenant-owned. PI-1D debía cerrar el riesgo de browser session sin mezclar staff/patient ni abrir intake.\n\n**Decisión:** Aceptar PI-1D mediante ADR 015 con route tree y shell separados, activación por fragment con limpieza inmediata, token/session solo en memoria, interceptores/guards no superpuestos, UX genérica, pruebas y runbook de recovery.\n\n**Consecuencias:** Pacientes existentes ya pueden activar, iniciar/cerrar sesión y recuperar acceso con asistencia sin permisos staff ni acceso canónico. PI-1 queda cerrado. Intake, revisión/aplicación, payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos."
)

# README.md
replace_once(
    "README.md",
    "Phase 2.1 is active through PI-1C. Account/invitation persistence, tenant-admin invitation management and the separate patient activation/login/self-session backend are accepted. The patient Angular experience and intake remain unavailable until PI-1D and PI-2. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
    "Phase 2.1 is active with PI-1 completed through PI-1A to PI-1D. Account/invitation persistence, tenant-admin invitation management, separate patient authentication/session and the bounded Angular patient-auth experience are accepted. Intake remains unavailable until PI-2. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance."
)
replace_once(
    "README.md",
    "      auth/\n      platform/",
    "      auth/\n      patient-portal-auth/\n      platform/"
)
replace_once(
    "README.md",
    "* **Latest Phase 2.1 slice completed:** **PI-1C — Patient activation, login and self-session boundary**\n* **Next slice:** **PI-1D — Patient auth frontend and security closure**\n* **Public patient runtime:** backend activation/login/self-session accepted; Angular patient UI and intake remain pending",
    "* **Latest Phase 2.1 milestone completed:** **PI-1 — Access and Invitation Foundation** through PI-1A to PI-1D\n* **Next slice:** **PI-2 — Intake Draft and Self-Service Capture**, pending explicit intake-scope decisions\n* **Public patient runtime:** bounded backend and Angular activation/login/session available; intake remains pending"
)
replace_once(
    "README.md",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. PI-1A through PI-1C are completed; PI-1D is the next gated slice. Patient activation/login and self-session exist only as backend contracts; Angular patient UX, intake, review/apply and final audit hardening remain pending. Payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics and the full Patient Portal remain future bounded work.",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. PI-1 is completed through PI-1A to PI-1D, including the bounded Angular patient-auth surface. Intake, review/apply and final audit hardening remain pending through PI-2 to PI-4. Payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics and the full Patient Portal remain future bounded work."
)
replace_once(
    "README.md",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006, ADR 012, ADR 013 and ADR 014\n* PI-1A completed account/invitation persistence; PI-1B completed tenant-admin invitation lifecycle; PI-1C completed the separate patient activation/login/self-session backend\n* PI-1 proceeds next through PI-1D (#25) for Angular patient auth, e2e and the assisted-recovery runbook before intake begins\n* PI-2 to PI-4 remain pending for intake, clinic review/application and audit hardening",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015\n* PI-1 is completed through account/invitation persistence, tenant-admin invitation lifecycle, separate patient auth/session backend and bounded Angular patient auth\n* PI-2 (#5) is next only after explicit decisions for proposal fields, contact/phone scope, waiting-room link lifecycle, draft expiry and save behavior\n* PI-3 and PI-4 remain pending for clinic review/application, audit visibility and production hardening\n* PI-1 closure evidence: `docs/pi-1-patient-portal-access-and-security-closure.md`; recovery runbook: `docs/patient-portal-assisted-recovery-runbook.md`"
)

# AGENTS.md
replace_once(
    "AGENTS.md",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active, PI-1A through PI-1C completed, PI-1D next",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 completed through PI-1A to PI-1D, PI-2 next and decision-gated"
)
replace_once(
    "AGENTS.md",
    "- ADR 014 — `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`\n- PI-1A — issue #22 / PR #26\n- PI-1B — issue #23 / PR #28\n- PI-1C — issue #24 / PR #29",
    "- ADR 014 — `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`\n- ADR 015 — `docs/decisions/015-patient-portal-frontend-session-boundary.md`\n- PI-1 closure — `docs/pi-1-patient-portal-access-and-security-closure.md`\n- PI-1 runbook — `docs/patient-portal-assisted-recovery-runbook.md`\n- PI-1A — issue #22 / PR #26\n- PI-1B — issue #23 / PR #28\n- PI-1C — issue #24 / PR #29\n- PI-1D — issue #25 / PR #30"
)
replace_once(
    "AGENTS.md",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006, access/opening in ADR 012, invitation management in ADR 013 and patient auth/session in ADR 014\n- PI-1 is active; PI-1A (#22), PI-1B (#23) and PI-1C (#24) are completed\n- PI-1D (#25) is the next slice and must close frontend/e2e/runbook without adding intake\n- PI-2 to PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability\n\n# Immediate objective\nPreserve completed PI-1A through PI-1C and open only `PI-1D — Patient auth frontend and security closure` before any intake work.",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006; PI-1 access decisions are accepted in ADR 012 through ADR 015\n- PI-1 (#4) is completed through PI-1A to PI-1D\n- PI-2 (#5) is next but requires explicit product/data-lifecycle decisions before implementation\n- PI-3 and PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability\n\n# Immediate objective\nPreserve completed PI-1 and resolve the bounded product decisions required before opening `PI-2 — Intake Draft and Self-Service Capture`."
)
replace_once(
    "AGENTS.md",
    "- preserve PI-1C separate patient bearer scheme/secret/audience, fixed-time token verification, `SessionVersion`, generic errors, rate limiting, lockout and assisted recovery\n- keep patient access tokens in frontend memory only and keep intake/canonical modules outside PI-1D",
    "- preserve PI-1C separate patient bearer scheme/secret/audience, fixed-time token verification, `SessionVersion`, generic errors, rate limiting, lockout and assisted recovery\n- preserve PI-1D route/shell/interceptor separation, activation fragment cleanup and memory-only patient session\n- do not open PI-2 until editable fields, phones/contact ownership, waiting-room link lifecycle, draft expiry and save behavior are explicitly decided"
)

# PROJECT_MAP.md
replace_once(
    "PROJECT_MAP.md",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1A through PI-1C completed, PI-1D next",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1 completed through PI-1A to PI-1D, PI-2 next and decision-gated"
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7 and completed PI-1A through PI-1C while preparing PI-1D:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* preserve tenant-owned patient portal account/invitation persistence completed in PI-1A\n* preserve PI-1B staff invitation endpoints, `TenantAdmin`-only permission, no platform override, token hash-at-rest and append-only audit\n* preserve PI-1C activation/login/self-session backend, separate bearer scheme, `SessionVersion`, rate limits, lockout and recovery permission\n* keep intake and canonical module access outside PI-1D\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1A through PI-1C are accepted with migrations, tests, docs and CI; PI-1D is the next gated step",
    "Preserve Releases 1 through 7 and completed PI-1 while preparing decision-gated PI-2:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* preserve PI-1A tenant-owned account/invitation persistence\n* preserve PI-1B tenant-admin-only invitation lifecycle, no platform override, hash-at-rest and append-only audit\n* preserve PI-1C separate patient bearer/session boundary, `SessionVersion`, rate limits, lockout and recovery permission\n* preserve PI-1D separate Angular route/shell/interceptors, fragment cleanup and memory-only session\n* keep intake and canonical module access outside accepted PI-1\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1 is accepted with migrations, backend/frontend tests, ADRs, closure evidence, runbook and CI; PI-2 is the next decision-gated step"
)
replace_once(
    "PROJECT_MAP.md",
    "* `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`\n* `docs/patient-intake-and-portal-plan.md`",
    "* `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`\n* `docs/decisions/015-patient-portal-frontend-session-boundary.md`\n* `docs/pi-1-patient-portal-access-and-security-closure.md`\n* `docs/patient-portal-assisted-recovery-runbook.md`\n* `docs/patient-intake-and-portal-plan.md`"
)
replace_once(
    "PROJECT_MAP.md",
    "      auth/\n      platform/",
    "      auth/\n      patient-portal-auth/\n      platform/"
)
replace_once(
    "PROJECT_MAP.md",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013/014. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation management; PI-1C owns the separate patient activation/login/self-session backend, password hashing, JWT scheme, lockout, rate limiting, session validation and assisted recovery. It never reuses staff membership semantics or staff permissions.",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013/014/015. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation management; PI-1C owns the separate patient activation/login/self-session backend; and PI-1D owns the separate Angular route/shell/session/interceptor boundary plus operational recovery runbook. PI-1 never reuses staff membership semantics or staff permissions."
)

# docs/product-roadmap.md
replace_once(
    "docs/product-roadmap.md",
    "Active after formal MVP acceptance. PI-1A, PI-1B and PI-1C are completed; PI-1D is next.",
    "Active after formal MVP acceptance. PI-1 is completed through PI-1A to PI-1D; PI-2 is next and decision-gated."
)
replace_once(
    "docs/product-roadmap.md",
    "Active under ADR 006, ADR 012, ADR 013 and ADR 014. PI-1A (#22), PI-1B (#23) and PI-1C (#24) are completed; PI-1D (#25) is next. The backend public auth boundary is accepted, but the patient Angular experience and intake are not.",
    "Active under ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015. PI-1 (#4) is completed through PI-1A to PI-1D. The bounded backend and Angular patient-auth boundary is accepted; PI-2 (#5) is next, but intake is not yet implemented."
)
replace_once(
    "docs/product-roadmap.md",
    "   3. PI-1C activation/login/self-session — #24 — completed\n   4. PI-1D patient auth frontend/security closure — #25 — next\n2. PI-2 — Intake Draft and Self-Service Capture — issue #5",
    "   3. PI-1C activation/login/self-session — #24 — completed\n   4. PI-1D patient auth frontend/security closure — #25 — completed\n2. PI-2 — Intake Draft and Self-Service Capture — issue #5 — next, decision-gated"
)
replace_once(
    "docs/product-roadmap.md",
    "- ADR 014: `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`\n- plan: `docs/patient-intake-and-portal-plan.md`",
    "- ADR 014: `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`\n- ADR 015: `docs/decisions/015-patient-portal-frontend-session-boundary.md`\n- PI-1 closure: `docs/pi-1-patient-portal-access-and-security-closure.md`\n- recovery runbook: `docs/patient-portal-assisted-recovery-runbook.md`\n- plan: `docs/patient-intake-and-portal-plan.md`"
)

# General Patient Intake plan
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** In progress; PI-1 active; PI-1A through PI-1C completed; PI-1D next",
    "- **Status:** In progress; PI-1 completed through PI-1A to PI-1D; PI-2 next and decision-gated"
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Architecture decisions:** ADR 006, ADR 012, ADR 013 and ADR 014",
    "- **Architecture decisions:** ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015"
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: active; PI-1A domain/persistence, PI-1B staff invitations and PI-1C patient auth/session completed; PI-1D is next.",
    "- Phase 2.1: active; PI-1 is completed through PI-1A to PI-1D; PI-2 is next and decision-gated."
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-1 access/invitations | Active; PI-1A through PI-1C completed; PI-1D next | Issues #4 and #22–#25 / PRs #26, #28 and #29 |\n| PI-2 intake draft | Planned; not implemented | Issue #5 |\n| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |\n| PI-4 audit/hardening | Planned; not implemented | Issue #7 |\n| Patient-facing backend/API/database | Auth foundation implemented through PI-1C | PRs #26, #28 and #29 |\n| Patient-facing frontend/intake | Not implemented | PI-1D / PI-2 |",
    "| PI-1 access/invitations | Completed through PI-1A to PI-1D | Issues #4 and #22–#25 / PRs #26, #28, #29 and #30 |\n| PI-2 intake draft | Next; decision-gated; not implemented | Issue #5 |\n| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |\n| PI-4 audit/hardening | Planned; not implemented | Issue #7 |\n| Patient-facing backend/API/database | Access/auth foundation implemented through PI-1 | PRs #26, #28 and #29 |\n| Patient-facing frontend | Bounded activation/login/session implemented | PR #30 / ADR 015 |\n| Patient-facing intake | Not implemented | PI-2 |"
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- replay/expiry/revocation/concurrency tests;\n- no questionnaire/intake writes.\n\nExit gate:\n- patient identity cannot obtain staff permissions;\n- ownership comes from verified context;\n- invitation cannot be replayed or cross tenants;\n- CI green.",
    "- replay/expiry/revocation/concurrency tests;\n- separate Angular route/shell/guard/interceptor boundary;\n- memory-only patient session and fragment cleanup;\n- assisted activation/recovery runbook;\n- no questionnaire/intake writes.\n\nExit gate:\n- patient identity cannot obtain staff permissions;\n- ownership comes from verified context;\n- invitation cannot be replayed or cross tenants;\n- staff and patient bearer tokens never cross API boundaries;\n- CI green."
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "### Approved for PI-1 under ADR 012, ADR 013 and ADR 014",
    "### Approved and completed for PI-1 under ADR 012, ADR 013, ADR 014 and ADR 015"
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Assisted recovery uses `patientportal.account.recover` for `TenantAdmin` only, without platform override.\n\n### Before PI-2",
    "- Assisted recovery uses `patientportal.account.recover` for `TenantAdmin` only, without platform override.\n- Angular patient routes are outside the staff shell; activation token uses a URL fragment and is removed immediately.\n- Patient access token/session remain in memory only; staff and patient interceptors never cross API boundaries.\n- PI-1 operational recovery follows `docs/patient-portal-assisted-recovery-runbook.md`.\n\n### Before PI-2"
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "1. Preserve PI-1A / #22, PI-1B / #23 and PI-1C / #24 as completed through PRs #26, #28 and #29.\n2. Open only PI-1D / #25 for the separate Angular patient-auth area, in-memory session state, e2e and recovery runbook.\n3. Do not add intake/questionnaire or canonical module access to PI-1D.\n4. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
    "1. Preserve PI-1 / #4 as completed through PI-1A to PI-1D and PRs #26, #28, #29 and #30.\n2. Resolve PI-2 decisions for editable proposal fields, phones/contact ownership, waiting-room link lifecycle, draft expiry and save behavior.\n3. Open PI-2 only after those decisions are accepted; do not add canonical application to PI-2.\n4. Keep PI-3 and PI-4 pending until their own gates."
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "**Consequence:** PI-1A through PI-1C now provide tenant-owned account/invitation persistence, staff invitation lifecycle and a separate patient activation/login/self-session backend. The capability is not operational for patients until PI-1D supplies frontend/e2e/runbook, and intake remains unavailable until PI-2. PI-2 to PI-4 remain unimplemented.",
    "**Consequence:** PI-1A through PI-1D now provide tenant-owned account/invitation persistence, staff invitation lifecycle, separate patient auth/session backend and bounded Angular activation/login/session with an assisted-recovery runbook. PI-1 is complete, while intake remains unavailable until PI-2. PI-2 to PI-4 remain unimplemented."
)

# Architecture / tenant model / UX map
replace_once(
    "docs/architecture.md",
    "      auth/\n      platform/",
    "      auth/\n      patient-portal-auth/\n      platform/"
)
replace_once(
    "docs/architecture.md",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary, ADR 012 defines the pilot access baseline, ADR 013 restricts invitation management, and ADR 014 establishes the separate patient bearer scheme, versioned password hashing, tenant realm, `SessionVersion`, abuse controls, recovery and authentication audit.",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary, ADR 012 defines the pilot access baseline, ADR 013 restricts invitation management, ADR 014 establishes the separate backend bearer/session boundary, and ADR 015 establishes the separate Angular route/shell/interceptor boundary with fragment activation and memory-only session state."
)
replace_once(
    "docs/tenant-model.md",
    "Patient-facing identity is separate from staff identity under ADR 006, ADR 012, ADR 013 and ADR 014.",
    "Patient-facing identity is separate from staff identity under ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015."
)
replace_once(
    "docs/tenant-model.md",
    "- patient policies have no platform override\n- `TenantId`, portal account and linked Patient come from verified server context\n- phone, date of birth and contact data cannot be used to claim a record publicly\n- Phase 2.1 proceeds through PI-1A to PI-1D before intake is opened",
    "- patient policies have no platform override\n- Angular patient routes/shell/guards/interceptors remain separate from staff auth and keep token/session state in memory only\n- activation links use a fragment that is removed immediately before the token is submitted in the request body\n- `TenantId`, portal account and linked Patient come from verified server context\n- phone, date of birth and contact data cannot be used to claim a record publicly\n- PI-1 is completed through PI-1A to PI-1D; intake opens only through PI-2 after its own decisions"
)
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "The initial operational MVP is accepted. **Phase 2.1 — Patient Intake and Portal Foundation** is active; PI-1A and PI-1B are completed and PI-1C is next.",
    "The initial operational MVP is accepted. **Phase 2.1 — Patient Intake and Portal Foundation** is active; PI-1 is completed through PI-1A to PI-1D and PI-2 is next."
)
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "- architecture accepted in ADR 006, access baseline/opening in ADR 012, invitation management in ADR 013 and patient auth/session in ADR 014;\n- PI-1A domain/persistence, PI-1B staff invitations and PI-1C backend auth/session are completed;\n- no patient Angular UI or intake UI is available yet;\n- PI-1D and PI-2 to PI-4 remain pending;",
    "- architecture accepted in ADR 006 and the access boundary accepted through ADR 012 to ADR 015;\n- PI-1A through PI-1D are completed, including the bounded Angular patient auth surface;\n- no patient intake UI is available yet;\n- PI-2 to PI-4 remain pending;"
)
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "| Patient Intake/Portal | auth backend yes | no | invitation management + activation/login/me/logout/recovery | dedicated invitation/recovery + patient self policy | yes | yes | PI-1A–PI-1C accepted; PI-1D/PI-2 pending | patient UI not implemented |",
    "| Patient Intake/Portal | auth backend yes | bounded auth UI yes | invitation management + activation/login/me/logout/recovery | dedicated invitation/recovery + patient self policy | yes | yes | PI-1 accepted; PI-2 pending | activation/login/session implemented; intake UI pending |"
)

# ADR follow-up reconciliation
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "Phase 2.1 — active; PI-1A through PI-1C completed; PI-1D next",
    "Phase 2.1 — active; PI-1 completed through PI-1A to PI-1D; PI-2 next"
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "On 2026-07-25 the client approved ADR 014: tenant realm by subdomain, dedicated versioned password hashing, separate patient JWT/scheme, transactional activation, rate limiting, lockout, server-side session invalidation and TenantAdmin-only assisted recovery.\n\n## Implementation status",
    "On 2026-07-25 the client approved ADR 014: tenant realm by subdomain, dedicated versioned password hashing, separate patient JWT/scheme, transactional activation, rate limiting, lockout, server-side session invalidation and TenantAdmin-only assisted recovery.\n\nOn 2026-07-25 the client authorized continuing with PI-1D. ADR 015 accepts the separate Angular route/shell/interceptor boundary, activation token fragment cleanup, memory-only session and assisted-recovery runbook.\n\n## Implementation status"
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- PI-1: active; PI-1A domain/persistence completed through PR #26.\n- PI-1B staff invitation lifecycle completed through PR #28 under ADR 013.\n- PI-1C patient activation/login/self-session completed through PR #29 under ADR 014.\n- PI-1D and PI-2–PI-4 are not implemented.\n- Public patient auth API exists; patient frontend and intake are not implemented.",
    "- PI-1: completed through PI-1A to PI-1D.\n- PI-1A domain/persistence completed through PR #26.\n- PI-1B staff invitation lifecycle completed through PR #28 under ADR 013.\n- PI-1C patient activation/login/self-session completed through PR #29 under ADR 014.\n- PI-1D Angular patient auth/security closure completed through PR #30 under ADR 015.\n- Public patient auth API and bounded frontend exist; PI-2–PI-4 and intake are not implemented."
)
replace_once(
    "docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md",
    "- full patient portal access to records/documents/commercial modules — Phase 4.\n\n## Exit condition\n\nThis ADR is implemented incrementally. It does not mean PI-1 or Phase 2.1 is complete. Completion requires the exit gates and aligned code/tests/docs for PI-1A through PI-1D, followed by PI-2 through PI-4 under ADR 006.",
    "- frontend patient session boundary — resolved in ADR 015 / PI-1D;\n- full patient portal access to records/documents/commercial modules — Phase 4.\n\n## Exit condition\n\nThis ADR is implemented through PI-1A to PI-1D. PI-1 is complete, but Phase 2.1 remains incomplete until PI-2 through PI-4 satisfy their own exit gates under ADR 006."
)
replace_once(
    "docs/decisions/013-patient-portal-invitation-management.md",
    "PI-1C (#24) is accepted through ADR 014 and PR #29. It implements password-hash versioning, separate patient JWT/scheme, fixed-time token verification, transactional single-use activation, anti-enumeration, rate limiting, lockout, server-side `SessionVersion` validation and assisted recovery. PI-1D (#25) is the only next PI-1 slice.",
    "PI-1C (#24) is accepted through ADR 014 and PR #29. PI-1D (#25) is accepted through ADR 015 and PR #30 with the separate Angular patient auth/session boundary and recovery runbook. PI-1 is complete; PI-2 remains the next decision-gated slice."
)
replace_once(
    "docs/decisions/014-patient-portal-authentication-and-session-boundary.md",
    "- the patient-facing Angular experience remains unavailable until PI-1D.",
    "- the patient-facing Angular experience is governed separately by ADR 015 and remains memory-only without refresh tokens."
)
replace_once(
    "docs/decisions/014-patient-portal-authentication-and-session-boundary.md",
    "PI-1 remains open after PI-1C. PI-1D must deliver the separate Angular patient-auth area, e2e coverage and the assisted-recovery runbook before PI-1 can close.",
    "PI-1D is accepted through ADR 015 and PR #30 with the separate Angular patient-auth area, token-boundary tests and assisted-recovery runbook. PI-1 is complete; PI-2 remains separately gated."
)

print("PI-1D documentation reconciliation completed.")
