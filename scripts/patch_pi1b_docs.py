from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace_once(relative_path: str, old: str, new: str) -> None:
    path = ROOT / relative_path
    text = path.read_text(encoding="utf-8")
    if new in text:
        return
    count = text.count(old)
    if count != 1:
        raise RuntimeError(
            f"Expected exactly one occurrence in {relative_path}, found {count}: {old[:120]!r}"
        )
    path.write_text(text.replace(old, new), encoding="utf-8")


# STATE — canonical phase and backlog.
replace_once(
    "STATE — BigSmile.md",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake y ADR 012 fija el baseline de acceso aprobado: activación single-use + password, `LoginName` tenant-scoped, TTL 24 h/30 min, entrega piloto por recepción, lockout 5 intentos/15 min y recovery asistido. Phase 2.1 queda abierta explícitamente; PI-1 está activa y PI-1A introduce solo dominio/persistencia, sin endpoints públicos ni captura de intake.",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 fija el baseline de acceso; y ADR 013 fija la gestión de invitaciones staff mediante `patientportal.invitation.manage` solo para `TenantAdmin`, sin `TenantUser`, `PlatformAdmin` ni platform override. Phase 2.1 está activa; PI-1A y PI-1B quedan completados sin abrir todavía activación/login públicos ni captura de intake.",
)
replace_once(
    "STATE — BigSmile.md",
    "[PENDIENTE POR DECISIÓN] PI-1B (#23) es el siguiente sub-slice, pero no se abre hasta aprobar el permiso staff que podrá emitir/revocar invitaciones. PI-1C (#24), PI-1D (#25) y PI-2 a PI-4 permanecen pendientes.",
    "[Hecho] PI-1B — Staff Invitation Lifecycle (#23) queda completado mediante PR #28: permiso dedicado solo para `TenantAdmin`, endpoints staff tenant-scoped de emisión/listado/revocación, token criptográfico one-time con hash-at-rest, TTL configurable de 24 horas, replacement explícito y bitácora append-only. PI-1C (#24) es el siguiente gate; PI-1D (#25) y PI-2 a PI-4 permanecen pendientes.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A completado; PI-1B pendiente de decisión de autorización; sin superficie pública todavía.",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A y PI-1B completados; PI-1C es el siguiente gate; sin activación/login público, JWT de paciente, frontend paciente ni intake todavía.",
)
replace_once(
    "STATE — BigSmile.md",
    "- ADR 012 — `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`.",
    "- ADR 012 — `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`.\n- ADR 013 — `docs/decisions/013-patient-portal-invitation-management.md`.",
)
replace_once(
    "STATE — BigSmile.md",
    "1. Preservar PI-1A (#22) como dominio/persistencia completado, sin endpoints ni auth pública.\n\n2. Resolver explícitamente la autorización de PI-1B (#23). Recomendación técnica: permiso dedicado `patientportal.invitation.manage`, asignado inicialmente solo a `TenantAdmin`, sin platform override ni reutilizar `patient.write`.\n\n3. Abrir PI-1B únicamente después de aprobar esa decisión, limitado a emisión/revocación staff, token hash-at-rest y auditoría.\n\n4. Mantener PI-1C (#24) y PI-1D (#25) bloqueados hasta sus gates previos; no exponer auth pública antes de hashing versionado, anti-enumeración, rate limiting y concurrencia.\n\n5. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.\n\n6. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n7. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n8. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n9. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n10. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n11. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
    "1. Preservar PI-1A (#22) y PI-1B (#23) como foundations completados, sin activación/login público ni intake.\n\n2. Preparar PI-1C (#24) únicamente después de decidir explícitamente el formato versionado de password hash, audience/scope y lifetime del JWT de paciente, comparación de token, rate limits y enforcement de `SessionVersion`.\n\n3. Mantener PI-1D (#25) bloqueado hasta aceptar PI-1C; no construir frontend sobre contratos de auth no cerrados.\n\n4. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.\n\n5. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n6. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n7. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n8. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n9. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Patient-facing identity** — [Hecho] La frontera ya está abierta de forma acotada en PI-1A, pero todavía no es pública. No reutiliza staff membership, no acepta `PatientId`/`TenantId` como autoridad, no permite platform override y no aplica cambios canónicos.",
    "**Patient-facing identity** — [Hecho] PI-1A y PI-1B ya establecen persistencia e invitaciones staff, pero la frontera todavía no permite autenticación de pacientes. No reutiliza staff membership, no acepta `PatientId`/`TenantId` como autoridad, no permite platform override, guarda solo hashes de invitación y no aplica cambios canónicos.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 012; Phase 2.1 abierta, PI-1A completado y PI-1B pendiente de decisión de autorización.\n\n**Contexto:** El MVP ya estaba aceptado. El cliente aprobó el baseline de acceso necesario para iniciar la identidad de paciente sin reutilizar staff auth ni introducir un proveedor externo.\n\n**Decisión:** Abrir Phase 2.1; fijar mediante ADR 012 activación single-use + password, `LoginName` tenant-scoped, TTL 24 h/30 min, entrega por recepción, lockout 5/15 y recovery asistido; ejecutar PI-1 en cuatro sub-slices y comenzar únicamente por PI-1A.\n\n**Consecuencias:** El repositorio ya tiene dominio/persistencia tenant-aware sin superficie pública. El siguiente paso es aprobar quién puede gestionar invitaciones antes de abrir PI-1B. Activación, JWT, frontend e intake permanecen bloqueados por los gates posteriores. Payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 013; Phase 2.1 abierta; PI-1A y PI-1B completados; PI-1C pendiente de decisión de auth/session.\n\n**Contexto:** PI-1A ya establecía cuentas e invitaciones tenant-owned. El cliente autorizó explícitamente que la gestión de invitaciones use un permiso dedicado solo para `TenantAdmin`, sin `TenantUser`, `PlatformAdmin` ni platform override.\n\n**Decisión:** Aceptar PI-1B mediante ADR 013 con emisión/listado/revocación staff tenant-scoped, token one-time de 256 bits, SHA-256 hash-at-rest, TTL configurable, replacement determinista y bitácora append-only.\n\n**Consecuencias:** El repositorio ya puede generar y entregar manualmente una invitación a un paciente existente, pero todavía no puede consumirla ni autenticar al paciente. PI-1C debe cerrar password hashing, JWT/audience/scope, comparación constante, consumo transaccional, anti-enumeración, rate limiting, lockout y session invalidation. Payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
)

# README.
replace_once(
    "README.md",
    "* **Latest Phase 2.1 slice completed:** **PI-1A — Patient portal account and invitation domain/persistence**\n* **Next slice:** **PI-1B — Staff invitation lifecycle**, pending explicit staff-permission approval\n* **Public patient runtime:** not exposed; activation/login/frontend/intake remain pending",
    "* **Latest Phase 2.1 slice completed:** **PI-1B — Staff-issued patient portal invitation lifecycle**\n* **Next slice:** **PI-1C — Patient activation, login and self-session boundary**, pending auth/session decisions\n* **Public patient runtime:** not exposed; activation/login/JWT/frontend/intake remain pending",
)
replace_once(
    "README.md",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. PI-1A is completed; PI-1B is the next gated slice. Public activation/login, intake, review/apply and audit hardening remain pending. Payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics and the full Patient Portal remain future bounded work.",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. PI-1A and PI-1B are completed; PI-1C is the next gated slice. Public activation/login, patient JWT/session, intake, review/apply and final audit hardening remain pending. Payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics and the full Patient Portal remain future bounded work.",
)
replace_once(
    "README.md",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006 and ADR 012\n* PI-1A completed account/invitation domain and persistence only; it exposes no patient endpoint\n* PI-1 proceeds through PI-1A (#22), PI-1B (#23), PI-1C (#24) and PI-1D (#25) before intake begins",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006, ADR 012 and ADR 013\n* PI-1A completed account/invitation domain and persistence; PI-1B completed tenant-admin invitation issuance/list/revoke with hash-at-rest and append-only audit\n* PI-1 proceeds next through PI-1C (#24) and PI-1D (#25) before intake begins",
)

# AGENTS.
replace_once(
    "AGENTS.md",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active, PI-1A completed, PI-1B pending authorization decision",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active, PI-1A and PI-1B completed, PI-1C next and auth/session-gated",
)
replace_once(
    "AGENTS.md",
    "- ADR 012 — `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`\n- PI-1A — issue #22 / PR #26",
    "- ADR 012 — `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`\n- ADR 013 — `docs/decisions/013-patient-portal-invitation-management.md`\n- PI-1A — issue #22 / PR #26\n- PI-1B — issue #23 / PR #28",
)
replace_once(
    "AGENTS.md",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006 and access baseline/opening accepted in ADR 012\n- PI-1 is active; PI-1A (#22) is completed through PR #26\n- PI-1B (#23) is next but requires explicit staff-permission approval\n- PI-1C (#24) and PI-1D (#25) remain sequentially gated\n- PI-2 to PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability\n\n# Immediate objective\nPreserve completed PI-1A and resolve the explicit authorization decision required before opening `PI-1B — Staff-issued patient portal invitation lifecycle`.\n\nImmediate priorities:\n- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`\n- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard behavior\n- keep Documents upload validation, storage containment and tenant-local Dashboard dates intact\n- preserve PI-1A tenant ownership, tenant-scoped login uniqueness, one-patient linkage, hash-only invitation persistence and rowversion concurrency\n- do not add activation/login, patient JWTs, frontend auth or intake to PI-1B\n- approve a dedicated invitation-management permission before PI-1B; do not reuse broad `patient.write` silently",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006, access baseline/opening accepted in ADR 012 and invitation management accepted in ADR 013\n- PI-1 is active; PI-1A (#22) and PI-1B (#23) are completed\n- PI-1C (#24) is next and requires explicit password/JWT/session decisions\n- PI-1D (#25) remains sequentially gated\n- PI-2 to PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability\n\n# Immediate objective\nPreserve completed PI-1A/PI-1B and resolve the auth/session decisions required before opening `PI-1C — Patient activation, login and self-session boundary`.\n\nImmediate priorities:\n- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`\n- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard behavior\n- keep Documents upload validation, storage containment and tenant-local Dashboard dates intact\n- preserve PI-1A tenant ownership, tenant-scoped login uniqueness, one-patient linkage and rowversion concurrency\n- preserve PI-1B `TenantAdmin`-only permission, no platform override, token hash-at-rest, replacement semantics and append-only audit\n- do not open activation/login until password-hash versioning, patient JWT audience/scope/lifetime, comparison, rate limits and session invalidation are explicitly accepted",
)

# PROJECT_MAP.
replace_once(
    "PROJECT_MAP.md",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1A completed, PI-1B next and permission-gated",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1A and PI-1B completed, PI-1C next and auth/session-gated",
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7 and completed PI-1A while preparing the permission-gated PI-1B slice:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* preserve tenant-owned patient portal account/invitation domain and persistence completed in PI-1A\n* approve PI-1B staff authorization explicitly before adding invitation endpoints\n* keep public endpoints, JWTs, frontend auth and intake outside PI-1B\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1A is accepted with migration, tests, docs and CI; PI-1B is the next gated step",
    "Preserve Releases 1 through 7 and completed PI-1A/PI-1B while preparing the auth/session-gated PI-1C slice:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* preserve tenant-owned patient portal account/invitation persistence completed in PI-1A\n* preserve PI-1B staff invitation endpoints, `TenantAdmin`-only permission, no platform override, token hash-at-rest and append-only audit\n* keep activation/login, patient JWT, frontend auth and intake outside accepted PI-1B\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1A and PI-1B are accepted with migrations, tests, docs and CI; PI-1C is the next gated step",
)
replace_once(
    "PROJECT_MAP.md",
    "Patient-facing identity is an active bounded boundary under ADR 006/012. PI-1A owns `PatientPortalAccount` and `PatientPortalInvitation` persistence and must not reuse staff membership semantics.",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation issuance/list/revoke and security audit. Neither may reuse staff membership semantics for patient access.",
)

# Roadmap.
replace_once(
    "docs/product-roadmap.md",
    "Active after formal MVP acceptance. PI-1A is completed; PI-1B is next and awaits the explicit staff-permission decision.",
    "Active after formal MVP acceptance. PI-1A and PI-1B are completed; PI-1C is next and awaits explicit auth/session decisions.",
)
replace_once(
    "docs/product-roadmap.md",
    "Active under ADR 006 and ADR 012. PI-1A (#22) is completed; PI-1B (#23) is next but not opened. No public patient runtime is accepted yet.",
    "Active under ADR 006, ADR 012 and ADR 013. PI-1A (#22) and PI-1B (#23) are completed; PI-1C (#24) is next but not opened. No public patient runtime is accepted yet.",
)
replace_once(
    "docs/product-roadmap.md",
    "   2. PI-1B staff invitation lifecycle — #23 — next, permission decision pending",
    "   2. PI-1B staff invitation lifecycle — #23 — completed",
)
replace_once(
    "docs/product-roadmap.md",
    "- ADR 012: `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`",
    "- ADR 012: `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`\n- ADR 013: `docs/decisions/013-patient-portal-invitation-management.md`",
)

# General plan.
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** In progress; PI-1 active; PI-1A completed; PI-1B next and permission-gated\n- **Roadmap placement:** Phase 2.1 — Patient Intake and Portal Foundation\n- **Start gate:** Satisfied through MVP acceptance and explicit client authorization on 2026-07-24\n- **Architecture decisions:** ADR 006 and ADR 012",
    "- **Status:** In progress; PI-1 active; PI-1A and PI-1B completed; PI-1C next and auth/session-gated\n- **Roadmap placement:** Phase 2.1 — Patient Intake and Portal Foundation\n- **Start gate:** Satisfied through MVP acceptance and explicit client authorization on 2026-07-24\n- **Architecture decisions:** ADR 006, ADR 012 and ADR 013",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: active; PI-1A domain/persistence completed; PI-1B is the next gated slice.",
    "- Phase 2.1: active; PI-1A domain/persistence and PI-1B staff invitation lifecycle completed; PI-1C is the next gated slice.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-1 access/invitations | Active; PI-1A completed; PI-1B next | Issues #4 and #22–#25 |",
    "| PI-1 access/invitations | Active; PI-1A and PI-1B completed; PI-1C next | Issues #4 and #22–#25 / PRs #26 and #28 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "### Approved for PI-1 under ADR 012\n\n- Activation single-use followed by password access.",
    "### Approved for PI-1 under ADR 012 and ADR 013\n\n- Invitation management uses `patientportal.invitation.manage` for `TenantAdmin` only; no `TenantUser`, `PlatformAdmin` or platform override.\n- Invitation issuance returns the raw token once, stores only its SHA-256 hash, supersedes outstanding invitations and records append-only audit.\n- Activation single-use followed by password access.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "1. Preserve PI-1A / issue #22 as completed through PR #26.\n2. Approve the PI-1B staff permission before implementation; recommendation: dedicated `patientportal.invitation.manage`, initially `TenantAdmin` only, no platform override.\n3. After approval, open only PI-1B / issue #23.\n4. Keep PI-1C/#24 and PI-1D/#25 sequentially gated.\n5. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
    "1. Preserve PI-1A / issue #22 and PI-1B / issue #23 as completed through PRs #26 and #28.\n2. Resolve PI-1C decisions for password-hash format/versioning, patient JWT audience/scope/lifetime, token comparison, rate limiting, lockout enforcement and `SessionVersion`.\n3. Open only PI-1C / issue #24 after those decisions are accepted.\n4. Keep PI-1D/#25 sequentially gated.\n5. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "**Consequence:** PI-1A may add tenant-owned account/invitation persistence, but public auth and intake remain unavailable until their explicit gates. PI-2 to PI-4 remain unimplemented.",
    "**Consequence:** PI-1A and PI-1B now provide tenant-owned account/invitation persistence plus staff issuance/revocation and audit, but public auth and intake remain unavailable until PI-1C/PI-1D and later gates. PI-2 to PI-4 remain unimplemented.",
)

# ADR 006.
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "Phase 2.1 — planned, explicit opening pending",
    "Phase 2.1 — active; PI-1A and PI-1B completed; PI-1C next",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "On 2026-07-24 the client additionally approved the ADR 012 baseline: password after single-use activation, tenant-scoped `LoginName`, 24 h/30 min TTL defaults, reception delivery, 5/15 lockout and assisted recovery.",
    "On 2026-07-24 the client additionally approved the ADR 012 baseline: password after single-use activation, tenant-scoped `LoginName`, 24 h/30 min TTL defaults, reception delivery, 5/15 lockout and assisted recovery.\n\nOn 2026-07-25 the client approved ADR 013: `patientportal.invitation.manage` only for `TenantAdmin`, no `TenantUser`, `PlatformAdmin` or platform override, with one-time token issuance and append-only invitation audit.",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- PI-1: active; PI-1A domain/persistence completed through PR #26.\n- PI-1B is next and awaits its staff-authorization decision.\n- PI-1C–PI-1D and PI-2–PI-4: not implemented.",
    "- PI-1: active; PI-1A domain/persistence completed through PR #26.\n- PI-1B staff invitation lifecycle completed through PR #28 under ADR 013.\n- PI-1C is next; PI-1D and PI-2–PI-4 are not implemented.",
)

# Tenant model and architecture.
replace_once(
    "docs/tenant-model.md",
    "- `PatientPortalAccount` and `PatientPortalInvitation` are tenant-owned records\n- `LoginName` uniqueness is scoped by `TenantId`",
    "- `PatientPortalAccount`, `PatientPortalInvitation` and patient-portal security audit entries are tenant-owned records\n- invitation management requires `patientportal.invitation.manage`, initially only for `TenantAdmin`, with no platform override\n- `LoginName` uniqueness is scoped by `TenantId`",
)
replace_once(
    "docs/architecture.md",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary and ADR 012 defines the approved pilot access baseline.",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary, ADR 012 defines the approved pilot access baseline, and ADR 013 restricts invitation management to tenant-scoped `TenantAdmin` with token hash-at-rest and append-only audit.",
)

# UX reconciliation.
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "The initial operational MVP is accepted. **Phase 2.1 — Patient Intake and Portal Foundation** is active; PI-1A is completed and PI-1B is next.",
    "The initial operational MVP is accepted. **Phase 2.1 — Patient Intake and Portal Foundation** is active; PI-1A and PI-1B are completed and PI-1C is next.",
)
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "- architecture accepted in ADR 006 and access baseline/opening accepted in ADR 012;\n- PI-1A domain/persistence is completed;\n- PI-1B is permission-gated and not opened;\n- no public patient auth or intake UI is available yet;\n- PI-1B to PI-1D and PI-2 to PI-4 remain pending;",
    "- architecture accepted in ADR 006, access baseline/opening accepted in ADR 012 and invitation management accepted in ADR 013;\n- PI-1A domain/persistence and PI-1B staff invitation lifecycle are completed;\n- no public patient auth or intake UI is available yet;\n- PI-1C to PI-1D and PI-2 to PI-4 remain pending;",
)

print("PI-1B documentation reconciled.")
