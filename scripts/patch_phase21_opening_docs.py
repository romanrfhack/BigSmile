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


# STATE — canonical phase opening and active slice.
replace_once(
    "STATE — BigSmile.md",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 queda aceptado como decisión futura: identidad de paciente separada del acceso interno; sin `TenantUser`, `UserTenantMembership` ni permisos tenant-wide; invitaciones single-use para pacientes existentes; enlace/QR tenant-scoped para pacientes nuevos; flujo `Draft -> Submitted -> Reviewed -> Applied/Rejected`; revisión clínica antes de aplicación canónica; y bitácora append-only. Se ubica en Phase 2.1 después del MVP inicial y no abre implementación actual.",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake y ADR 012 fija el baseline de acceso aprobado: activación single-use + password, `LoginName` tenant-scoped, TTL 24 h/30 min, entrega piloto por recepción, lockout 5 intentos/15 min y recovery asistido. Phase 2.1 queda abierta explícitamente; PI-1 está activa y PI-1A introduce solo dominio/persistencia, sin endpoints públicos ni captura de intake.",
)

replace_once(
    "STATE — BigSmile.md",
    "[Hecho] La última fase funcional marcada como completada es Release 7 — Documents and Dashboard.\n\n[Hecho] Release 7 queda cerrada y preservada mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation.\n\n[Hecho] El MVP operativo inicial queda formalmente aceptado. Esta aceptación cubre los boundaries fundacionales documentados y no implica payments, cash management, CFDI, doctor views, automatizaciones, advanced analytics ni full Patient Portal.\n\n[Hecho] La siguiente fase prevista por el roadmap es Phase 2.1 — Patient Intake and Portal Foundation.\n\n[Hecho] El gate normal de MVP para Phase 2.1 ya está satisfecho, pero la fase no se abre ni implementa automáticamente. Antes de PI-1 se requiere decisión explícita de apertura y resolver los choices de acceso/bootstrap registrados en issue #2.\n\n[Hecho] PI-1 a PI-4 permanecen no implementados. Cuando Phase 2.1 se abra explícitamente, el primer slice será PI-1 — Access and Invitation Foundation, issue #4.",
    "[Hecho] La última fase funcional completada es Release 7 — Documents and Dashboard y el MVP operativo inicial permanece formalmente aceptado.\n\n[Hecho] Phase 2.1 — Patient Intake and Portal Foundation es la fase funcional actual; fue abierta explícitamente el 2026-07-24 después de aprobar su baseline de acceso.\n\n[Hecho] PI-1 — Access and Invitation Foundation está activa. El sub-slice actual es PI-1A — Account and Invitation Domain/Persistence, issue #22.\n\n[Hecho] PI-1A agrega únicamente entidades tenant-owned, invariantes, índices, filtros, write enforcement, concurrencia, migración y pruebas. No expone activación/login, no crea un JWT de paciente y no captura cuestionario/intake.\n\n[Hecho] PI-1B (#23), PI-1C (#24) y PI-1D (#25) permanecen pendientes y deben ejecutarse en ese orden. PI-2 a PI-4 continúan no iniciados.\n\n[Hecho] El MVP aceptado sigue sin implicar payments, cash management, CFDI, doctor views, automatizaciones, advanced analytics ni full Patient Portal.",
)

replace_once(
    "STATE — BigSmile.md",
    "## 4.2 Plan futuro — Phase 2.1 Patient Intake and Portal Foundation",
    "## 4.2 Fase actual — Phase 2.1 Patient Intake and Portal Foundation",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] decisión aceptada y trabajo planificado; implementación no iniciada.",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A en implementación; sin superficie pública todavía.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Ubicación** — [Hecho] siguiente fase prevista después del MVP aceptado; permanece planificada y no desplaza la necesidad de una apertura explícita antes de implementar PI-1.",
    "**Ubicación** — [Hecho] fase actual posterior al MVP aceptado; se implementa mediante PI-1A → PI-1B → PI-1C → PI-1D antes de abrir PI-2.",
)
replace_once(
    "STATE — BigSmile.md",
    "- ADR 006 — `docs/decisions/006-patient-intake-and-portal-foundation.md`.\n- Plan general — `docs/patient-intake-and-portal-plan.md`.\n- Parent issue — #2.\n- PI-1 Access and Invitation Foundation — issue #4.\n- PI-2 Intake Draft — issue #5.\n- PI-3 Submit, Review and Apply — issue #6.\n- PI-4 Audit Visibility and Hardening — issue #7.",
    "- ADR 006 — `docs/decisions/006-patient-intake-and-portal-foundation.md`.\n- ADR 012 — `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`.\n- Plan general — `docs/patient-intake-and-portal-plan.md`.\n- Parent issue — #2.\n- PI-1 Access and Invitation Foundation — issue #4.\n- PI-1A Domain/Persistence — issue #22.\n- PI-1B Staff Invitation Lifecycle — issue #23.\n- PI-1C Patient Activation/Login — issue #24.\n- PI-1D Patient Auth Frontend/Closure — issue #25.\n- PI-2 Intake Draft — issue #5.\n- PI-3 Submit, Review and Apply — issue #6.\n- PI-4 Audit Visibility and Hardening — issue #7.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Reglas cerradas** — [Hecho]",
    "**Baseline de acceso aprobado** — [Hecho]\n\n- Activación single-use y password para accesos posteriores.\n- `LoginName` único por tenant; teléfono/fecha de nacimiento/contacto no prueban ownership.\n- TTL configurable: 24 horas para invitación existente y 30 minutos para link de sala de espera.\n- Entrega piloto por recepción, sin proveedor automático.\n- Lockout configurable de 5 intentos / 15 minutos.\n- Recovery asistido mediante revocación de sesiones y reemisión de invitación.\n\n**Reglas cerradas** — [Hecho]",
)
replace_once(
    "STATE — BigSmile.md",
    "1. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n2. Tratar Phase 2.1 — Patient Intake and Portal Foundation como la siguiente fase prevista, no como implementación ya abierta.\n\n3. Resolver explícitamente en issue #2 el identificador de acceso, password vs magic link, TTL, entrega piloto y baseline de lockout/recovery antes de abrir PI-1.\n\n4. Cuando Phase 2.1 se abra, iniciar únicamente PI-1 — Access and Invitation Foundation, issue #4, y actualizar STATE en el mismo PR.\n\n5. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n6. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n7. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n8. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n9. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap y ADRs cuando se abra Phase 2.1 o cambie el estado del producto.",
    "1. Completar PI-1A (#22) con dominio/persistencia tenant-aware, migración, pruebas y CI verde, sin endpoints.\n\n2. Abrir PI-1B (#23) solo después del cierre de PI-1A para emisión/revocación staff con token hash-at-rest y auditoría.\n\n3. Mantener PI-1C (#24) y PI-1D (#25) bloqueados hasta sus gates previos; no exponer auth pública antes de hashing versionado, anti-enumeración, rate limiting y concurrencia.\n\n4. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.\n\n5. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n6. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n7. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n8. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n9. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Patient-facing identity** — [Hecho] La futura frontera pública no debe reutilizar staff membership, aceptar `PatientId`/`TenantId` como autoridad, permitir platform override ni aplicar cambios canónicos sin revisión.",
    "**Patient-facing identity** — [Hecho] La frontera ya está abierta de forma acotada en PI-1A, pero todavía no es pública. No reutiliza staff membership, no acepta `PatientId`/`TenantId` como autoridad, no permite platform override y no aplica cambios canónicos.",
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. La siguiente fase prevista es Phase 2.1 Patient Intake and Portal Foundation; el portal amplio permanece en Phase 4.",
    "[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Phase 2.1 Patient Intake and Portal Foundation está activa mediante PI-1; el portal amplio permanece en Phase 4.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado:** Nota canónica actualizada con ADR 006, ADR 007, ADR 008, ADR 009, ADR 010 y ADR 011.\n\n**Contexto:** Documents y Dashboard tenían implementación coherente, pero la auditoría detectó una allowlist de upload spoofable y una fecha `Today` basada en UTC. Ambos gaps se corrigieron mediante PR #19 y PR #20 con CI verde.\n\n**Decisión:** Cerrar Release 7 mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation; aceptar el MVP operativo inicial; preservar Documents como attachment foundation privada y Dashboard como read model tenant-scoped; mover la siguiente fase prevista a Phase 2.1 sin abrir PI-1 automáticamente.\n\n**Consecuencias:** El MVP queda listo para validación piloto bajo su scope acotado. Phase 2.1 requiere una decisión explícita de apertura y resolver los choices de issue #2; payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal permanecen diferidos.",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 012; Phase 2.1 abierta y PI-1A activa.\n\n**Contexto:** El MVP ya estaba aceptado. El cliente aprobó el baseline de acceso necesario para iniciar la identidad de paciente sin reutilizar staff auth ni introducir un proveedor externo.\n\n**Decisión:** Abrir Phase 2.1; fijar mediante ADR 012 activación single-use + password, `LoginName` tenant-scoped, TTL 24 h/30 min, entrega por recepción, lockout 5/15 y recovery asistido; ejecutar PI-1 en cuatro sub-slices y comenzar únicamente por PI-1A.\n\n**Consecuencias:** El repositorio incorpora primero dominio/persistencia tenant-aware sin superficie pública. Activación, JWT, endpoints, frontend e intake permanecen bloqueados por los gates de PI-1B a PI-2. Payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
)

# README.
replace_once(
    "README.md",
    "Code in later capabilities such as reminders/manual reminders, providers, jobs, online booking, Phase 2 patient intake or advanced analytics does not imply acceptance. Visual slices may improve presentation and UX debt without changing backend behavior, APIs, permissions, auth, tenant context, branch context, migrations or functional scope.",
    "Phase 2.1 is now explicitly opened through PI-1A, but only its account/invitation domain and persistence foundation is active. No public patient auth or intake capability is accepted yet. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
)
replace_once(
    "README.md",
    "* **Latest completed delivery phase:** **Release 7 — Documents and Dashboard**\n* **Initial operational MVP:** **formally accepted**\n* **Next planned phase:** **Phase 2.1 — Patient Intake and Portal Foundation**\n* **Phase 2.1 runtime status:** architecture accepted in ADR 006; PI-1 to PI-4 not implemented or automatically opened",
    "* **Latest completed delivery phase:** **Release 7 — Documents and Dashboard**\n* **Initial operational MVP:** **formally accepted**\n* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**\n* **Current slice:** **PI-1A — Patient portal account and invitation domain/persistence**\n* **Public patient runtime:** not exposed; activation/login/frontend/intake remain pending",
)
replace_once(
    "README.md",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. Payments/cash/CFDI, provider views, automated messaging, online booking, Phase 2.1 implementation, advanced analytics and the full Patient Portal remain future bounded work.",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. Phase 2.1 is active only through PI-1A; public activation/login, intake, review/apply and audit hardening remain pending. Payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics and the full Patient Portal remain future bounded work.",
)
replace_once(
    "README.md",
    "### Phase 2 Expansion — Modern Operations\n\n* Next planned phase after formal MVP acceptance; not automatically opened\n* **Phase 2.1 — Patient Intake and Portal Foundation** is architecturally accepted in ADR 006, but PI-1 to PI-4 are not implemented\n* The bounded capability includes patient activation, intake/update, clinic review/application and append-only audit\n* The full patient portal, automated messaging, online booking, providers, jobs, queues, campaigns and advanced dashboards remain deferred",
    "### Phase 2 Expansion — Modern Operations\n\n* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006 and ADR 012\n* PI-1A introduces account/invitation domain and persistence only; it does not expose patient endpoints\n* PI-1 proceeds through PI-1A (#22), PI-1B (#23), PI-1C (#24) and PI-1D (#25) before intake begins\n* PI-2 to PI-4 remain pending for intake, clinic review/application and audit hardening\n* The full patient portal, automated messaging, online booking, providers, jobs, queues, campaigns and advanced dashboards remain deferred",
)

# AGENTS.
replace_once(
    "AGENTS.md",
    "- Next planned phase: `Phase 2.1 — Patient Intake and Portal Foundation`; implementation not opened",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active and PI-1A in implementation",
)
replace_once(
    "AGENTS.md",
    "Release 7 / MVP closure evidence:\n- `docs/release-7-documents-and-dashboard-audit-and-closure.md`\n- ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`\n- ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`",
    "Release 7 / MVP closure evidence:\n- `docs/release-7-documents-and-dashboard-audit-and-closure.md`\n- ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`\n- ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`\n\nPhase 2.1 opening evidence:\n- ADR 006 — `docs/decisions/006-patient-intake-and-portal-foundation.md`\n- ADR 012 — `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`\n- PI-1A — issue #22 / PR #26",
)
replace_once(
    "AGENTS.md",
    "Phase 2.1 — Patient Intake and Portal Foundation is the next planned phase after the accepted MVP:\n- architecture accepted in ADR 006\n- implementation issues #4 to #7 remain open\n- PI-1 to PI-4 are not implemented or active\n- opening PI-1 requires an explicit phase-opening decision and resolution of the pending access/bootstrap choices in issue #2\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006 and access baseline/opening accepted in ADR 012\n- PI-1 is active; PI-1A (#22) is the current sub-slice\n- PI-1B (#23), PI-1C (#24) and PI-1D (#25) remain sequentially gated\n- PI-2 to PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability",
)
replace_once(
    "AGENTS.md",
    "# Immediate objective\nPreserve Releases 1 to 7 and the accepted MVP while preparing the explicit opening decision for `Phase 2.1 — Patient Intake and Portal Foundation`.\n\nImmediate priorities:\n- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`\n- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard behavior\n- keep Documents upload validation, storage containment and tenant-local Dashboard dates intact\n- resolve issue #2 choices before opening PI-1: patient identifier, password vs magic link, TTL, pilot delivery and lockout/recovery baseline\n- when explicitly opened, start only with PI-1 / issue #4 and update canonical state in the same PR\n- avoid reopening accepted aggregates through incidental Patient Portal linkage",
    "# Immediate objective\nComplete `PI-1A — Patient portal account and invitation domain/persistence` without exposing patient authentication or intake before its security gates.\n\nImmediate priorities:\n- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`\n- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard behavior\n- keep Documents upload validation, storage containment and tenant-local Dashboard dates intact\n- enforce tenant ownership, tenant-scoped login uniqueness, one-patient linkage, hash-only invitation persistence and rowversion concurrency in PI-1A\n- do not add staff/public endpoints, patient JWTs, frontend auth or intake to PI-1A\n- move to PI-1B only after PI-1A migration/tests/docs/CI are accepted\n- avoid reopening accepted aggregates through incidental Patient Portal linkage",
)

# PROJECT_MAP.
replace_once(
    "PROJECT_MAP.md",
    "* **Next planned phase:** **Phase 2.1 — Patient Intake and Portal Foundation**, not yet opened",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**, opened through active PI-1A",
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7 and prepare the explicit Phase 2.1 opening decision:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* keep payments, balances, receipts, cash management, fiscal/CFDI and automatic quote mutation outside Release 6.1\n* keep OCR/sharing/versioning and advanced Dashboard analytics outside Release 7\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* resolve the patient access/bootstrap choices tracked in issue #2 before starting PI-1\n* start only PI-1 / issue #4 after an explicit Phase 2.1 opening and update STATE in the same PR\n* keep automated messaging/providers/jobs/queues/retries, online booking and full Patient Portal deferred",
    "Preserve Releases 1 through 7 while executing PI-1A as the first Phase 2.1 runtime slice:\n\n* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries\n* implement only tenant-owned patient portal account/invitation domain and persistence in PI-1A\n* keep staff/public endpoints, JWTs, frontend auth and intake outside PI-1A\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* move to PI-1B only after PI-1A is accepted with migration, tests, docs and CI\n* keep payments, balances, receipts, cash management, fiscal/CFDI and automatic quote mutation outside Release 6.1\n* keep OCR/sharing/versioning and advanced Dashboard analytics outside Release 7\n* keep automated messaging/providers/jobs/queues/retries, online booking and full Patient Portal deferred",
)
replace_once(
    "PROJECT_MAP.md",
    "Patient-facing identity is a separate future boundary under ADR 006 and must not reuse staff membership semantics.",
    "Patient-facing identity is an active bounded boundary under ADR 006/012. PI-1A owns `PatientPortalAccount` and `PatientPortalInvitation` persistence and must not reuse staff membership semantics.",
)
replace_once(
    "PROJECT_MAP.md",
    "* dashboard summary -> tenant-scoped read model; operational day derives from tenant-owned `TimeZoneId`",
    "* dashboard summary -> tenant-scoped read model; operational day derives from tenant-owned `TimeZoneId`\n* patient portal account/invitation -> tenant-owned identity/bootstrap records; patient linkage and login uniqueness remain tenant-scoped",
)
replace_once(
    "PROJECT_MAP.md",
    "* Phase 2.1 Patient Intake and Portal Foundation under ADR 006 — next planned, not opened",
    "* Phase 2.1 Patient Intake and Portal Foundation under ADR 006/012 — active through PI-1A; later PI slices gated",
)

# Product roadmap.
replace_once(
    "docs/product-roadmap.md",
    "- **Phase 2 Expansion — Modern Operations** — next planned phase, not opened",
    "- **Phase 2 Expansion — Modern Operations** — active through Phase 2.1 / PI-1A",
)
replace_once(
    "docs/product-roadmap.md",
    "### Status\nNext planned phase after formal initial MVP acceptance; implementation not opened automatically.",
    "### Status\nActive after formal MVP acceptance. Phase 2.1 is opened and PI-1A is the current implementation slice.",
)
replace_once(
    "docs/product-roadmap.md",
    "#### Status\nArchitecturally accepted in ADR 006; MVP gate satisfied; explicit phase opening and PI-1 implementation still pending.",
    "#### Status\nActive under ADR 006 and ADR 012. PI-1 is active; PI-1A (#22) is in implementation. No public patient runtime is accepted yet.",
)
replace_once(
    "docs/product-roadmap.md",
    "#### Sequential implementation\n1. PI-1 — Access and Invitation Foundation — issue #4\n2. PI-2 — Intake Draft and Self-Service Capture — issue #5\n3. PI-3 — Submit, Clinic Review and Canonical Apply — issue #6\n4. PI-4 — Audit Visibility and Security Hardening — issue #7",
    "#### Sequential implementation\n1. PI-1 — Access and Invitation Foundation — issue #4\n   1. PI-1A domain/persistence — #22 — current\n   2. PI-1B staff invitation lifecycle — #23\n   3. PI-1C activation/login/self-session — #24\n   4. PI-1D patient auth frontend/security closure — #25\n2. PI-2 — Intake Draft and Self-Service Capture — issue #5\n3. PI-3 — Submit, Clinic Review and Canonical Apply — issue #6\n4. PI-4 — Audit Visibility and Security Hardening — issue #7",
)
replace_once(
    "docs/product-roadmap.md",
    "- ADR 006: `docs/decisions/006-patient-intake-and-portal-foundation.md`\n- plan: `docs/patient-intake-and-portal-plan.md`\n- parent issue #2",
    "- ADR 006: `docs/decisions/006-patient-intake-and-portal-foundation.md`\n- ADR 012: `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`\n- plan: `docs/patient-intake-and-portal-plan.md`\n- parent issue #2",
)

# Patient Intake plan.
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** Planned; implementation not opened\n- **Roadmap placement:** Phase 2.1 — Patient Intake and Portal Foundation\n- **Start gate:** Initial MVP accepted; explicit Phase 2.1 opening and issue #2 access/bootstrap decisions still required\n- **Architecture decision:** ADR 006",
    "- **Status:** In progress; PI-1 active; PI-1A current\n- **Roadmap placement:** Phase 2.1 — Patient Intake and Portal Foundation\n- **Start gate:** Satisfied through MVP acceptance and explicit client authorization on 2026-07-24\n- **Architecture decisions:** ADR 006 and ADR 012",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Last updated:** 2026-07-23",
    "- **Last updated:** 2026-07-24",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: next planned phase; explicit opening and PI-1 implementation still pending.",
    "- Phase 2.1: active; PI-1 opened; PI-1A domain/persistence is the current slice.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-1 access/invitations | Planned; not implemented | Issue #4 |\n| PI-2 intake draft | Planned; not implemented | Issue #5 |",
    "| PI-1 access/invitations | Active; PI-1A in implementation | Issues #4 and #22–#25 |\n| PI-2 intake draft | Planned; not implemented | Issue #5 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "## 10. Decisions required before implementation\n\n### Before PI-1\n\n- Login identifier: email, phone, username or bounded combination.\n- Password vs magic link or approved alternative.\n- Invitation and waiting-room-link TTL defaults.\n- Password/lockout policy if passwords are used.\n- Pilot link-delivery method without external provider.",
    "## 10. Decisions and remaining gates\n\n### Approved for PI-1 under ADR 012\n\n- Activation single-use followed by password access.\n- Tenant-scoped `LoginName`; email or username allowed, but phone/DOB/contact do not prove ownership.\n- Existing-patient invitation TTL: 24 hours, configurable.\n- Waiting-room link TTL: 30 minutes, configurable; runtime deferred to the intake slice.\n- Pilot delivery by reception without external provider.\n- Lockout baseline: 5 attempts / 15 minutes, configurable.\n- Assisted recovery through session revocation and invitation reissue.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "## 12. Current next action\n\nThe current repository has an accepted initial operational MVP. **Phase 2.1 — Patient Intake and Portal Foundation** is the next planned phase, but is not yet opened for implementation.\n\nFor Patient Intake and Portal:\n\n1. Keep issues #4 through #7 open and ordered.\n2. Resolve issue #2 choices for patient identifier, password vs magic link, TTL, pilot delivery and lockout/recovery baseline.\n3. Record an explicit Phase 2.1 opening decision before implementation.\n4. When Phase 2.1 opens, start only with issue #4.\n5. Update canonical state in the same PR that opens PI-1.",
    "## 12. Current next action\n\nThe current repository has an accepted MVP and an explicitly opened **Phase 2.1 — Patient Intake and Portal Foundation**.\n\nFor Patient Intake and Portal:\n\n1. Complete PI-1A / issue #22 with migration, tenant tests, ADR 012, canonical docs and CI.\n2. Do not add endpoints, JWTs, frontend auth or intake to PI-1A.\n3. After PI-1A acceptance, continue with PI-1B / issue #23 only.\n4. Keep PI-1C/#24 and PI-1D/#25 sequentially gated.\n5. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "**Decision:** Plan the bounded self-service capability as Phase 2.1 after the initial MVP, using separate patient identity, staged clinic review and append-only audit.\n\n**Consequence:** The MVP gate is satisfied and the requirement remains visible, decomposed, testable and traceable. Phase 2.1 still requires an explicit opening decision; PI-1 to PI-4 remain unimplemented.",
    "**Decision:** Open Phase 2.1 under ADR 012 and implement the approved access baseline through PI-1A to PI-1D before opening intake.\n\n**Consequence:** PI-1A may add tenant-owned account/invitation persistence, but public auth and intake remain unavailable until their explicit gates. PI-2 to PI-4 remain unimplemented.",
)

# ADR 006 status reconciliation.
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- **Tracking:** issue #2; implementation issues #4, #5, #6 and #7",
    "- **Tracking:** issue #2; PI-1 #4; PI-1A–PI-1D #22–#25; PI-2–PI-4 #5–#7\n- **Access baseline/opening:** ADR 012",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "Phase 2.1 is the next planned phase, but this ADR and the MVP gate do not automatically open PI-1. The access/bootstrap choices tracked in issue #2 must be resolved and canonical state updated when the phase is explicitly opened.",
    "Phase 2.1 was explicitly opened on 2026-07-24 after the access/bootstrap choices were approved in ADR 012. PI-1 is active and begins with PI-1A; later slices remain gated and no public patient runtime is accepted by the opening alone.",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- capability remains planned until the MVP gate or reprioritization.",
    "- Phase 2.1 is active, but the runtime remains incomplete until PI-1 through PI-4 are accepted.",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "Product confirmed on 2026-07-22 that:\n\n1. clinic review precedes canonical application;\n2. existing-patient access begins through staff-issued single-use invitation;\n3. waiting-room pilot may use clinic-generated QR/link without external provider;\n4. the capability does not displace the current MVP roadmap;\n5. full patient portal remains separate future scope.",
    "Product confirmed on 2026-07-22 that:\n\n1. clinic review precedes canonical application;\n2. existing-patient access begins through staff-issued single-use invitation;\n3. waiting-room pilot may use clinic-generated QR/link without external provider;\n4. the capability does not displace the current MVP roadmap;\n5. full patient portal remains separate future scope.\n\nOn 2026-07-24 the client additionally approved the ADR 012 baseline: password after single-use activation, tenant-scoped `LoginName`, 24 h/30 min TTL defaults, reception delivery, 5/15 lockout and assisted recovery.",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- Decision: accepted.\n- General plan: `docs/patient-intake-and-portal-plan.md`.\n- Parent: issue #2.\n- PI-1 to PI-4: planned, not implemented.\n- Backend/API/database/frontend patient-facing implementation: not started.",
    "- Decision: accepted.\n- General plan: `docs/patient-intake-and-portal-plan.md`.\n- Parent: issue #2.\n- Phase 2.1: opened.\n- PI-1: active; PI-1A domain/persistence in implementation.\n- PI-1B–PI-1D and PI-2–PI-4: not implemented.\n- Public patient API/auth/frontend and intake: not started.",
)

# Tenant model and architecture boundary.
replace_once(
    "docs/tenant-model.md",
    "A role alone is not enough; it must be evaluated together with its scope and membership.\n\n---\n\n## 8. Request Tenant Resolution",
    "A role alone is not enough; it must be evaluated together with its scope and membership.\n\n### 7.4 Patient-Facing Identity Boundary\n\nPatient-facing identity is separate from staff identity under ADR 006 and ADR 012.\n\nCurrent rules:\n- `PatientPortalAccount` and `PatientPortalInvitation` are tenant-owned records\n- `LoginName` uniqueness is scoped by `TenantId`\n- a portal account links to at most one canonical Patient in Phase 2.1\n- patient accounts do not use `UserTenantMembership`, staff roles or tenant-wide permissions\n- patient policies have no platform override\n- `TenantId`, portal account and linked Patient come from verified server context\n- phone, date of birth and contact data cannot be used to claim a record publicly\n- Phase 2.1 proceeds through PI-1A to PI-1D before intake is opened\n\n---\n\n## 8. Request Tenant Resolution",
)
replace_once(
    "docs/tenant-model.md",
    "- tenant settings\n- tenant users",
    "- tenant settings\n- patient portal accounts and invitations\n- tenant users",
)
replace_once(
    "docs/architecture.md",
    "* treatment conversion metrics\n\n---\n\n## 10. Domain Design Direction",
    "* treatment conversion metrics\n\n### 9.12 Patient Intake and Portal\n\nResponsible for the bounded Phase 2.1 patient-facing boundary:\n\n* patient portal account and invitation ownership\n* separate patient authentication/session policy\n* waiting-room and existing-patient intake drafts\n* patient-originated revisions and audit\n* clinic review before canonical application\n\nIt must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary and ADR 012 defines the approved pilot access baseline.\n\n---\n\n## 10. Domain Design Direction",
)

# UX reconciliation.
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "The initial operational MVP is accepted. The next planned phase is **Phase 2.1 — Patient Intake and Portal Foundation**, but implementation is not opened automatically.\n\n### Phase 2.1\n\nPatient Intake and Portal Foundation is planned after the initial MVP:\n\n- architecture accepted in ADR 006;\n- implementation tracked in issues #4 to #7;\n- no patient-facing runtime implementation opened;\n- full patient portal remains outside the bounded Phase 2.1 scope.",
    "The initial operational MVP is accepted. **Phase 2.1 — Patient Intake and Portal Foundation** is active through PI-1A.\n\n### Phase 2.1\n\nPatient Intake and Portal Foundation is now being implemented in bounded gates:\n\n- architecture accepted in ADR 006 and access baseline/opening accepted in ADR 012;\n- PI-1A domain/persistence is active;\n- no public patient auth or intake UI is available yet;\n- PI-1B to PI-1D and PI-2 to PI-4 remain pending;\n- full patient portal remains outside the bounded Phase 2.1 scope.",
)

print("Phase 2.1 documentation reconciled.")
