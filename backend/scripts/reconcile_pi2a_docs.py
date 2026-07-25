from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_once(path: str, old: str, new: str) -> None:
    target = ROOT / path
    text = target.read_text(encoding="utf-8-sig")
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"Expected exactly one match in {path}, found {count}: {old[:120]!r}")
    target.write_text(text.replace(old, new, 1), encoding="utf-8")


replace_once(
    "STATE — BigSmile.md",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 fija el baseline de acceso; ADR 013 fija la gestión de invitaciones staff; ADR 014 acepta la autenticación/sesión pública de paciente; y ADR 015 fija la separación Angular, token en memoria y activación por fragment. PI-1 queda completado mediante PI-1A a PI-1D. Phase 2.1 continúa activa con PI-2 como siguiente slice sujeto a decisiones de alcance; la captura de intake todavía no está implementada.",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 a ADR 015 cierran el acceso, invitaciones, autenticación/sesión y frontera Angular; ADR 016 fija el baseline de datos, draft lifecycle, revisión append-only y futuro bootstrap de sala de espera. PI-1 queda completado. PI-2 está activa: PI-2A incorpora dominio/persistencia tenant-aware sin endpoints ni captura Angular; PI-2B es el siguiente gate.",
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] PI-1D — Patient Auth Frontend and Security Closure (#25) queda completado mediante PR #30: route tree `/patient-portal/*` fuera del staff shell, activación con token en fragment y limpieza inmediata, sesión/token solo en memoria, interceptores/guards separados, login/home/logout acotados, pruebas frontend y runbook de recovery. PI-1 queda cerrado; PI-2 (#5) es el siguiente slice y todavía no está abierto.",
    "[Hecho] PI-1D — Patient Auth Frontend and Security Closure (#25) queda completado mediante PR #30: route tree `/patient-portal/*` fuera del staff shell, activación con token en fragment y limpieza inmediata, sesión/token solo en memoria, interceptores/guards separados, login/home/logout acotados, pruebas frontend y runbook de recovery. PI-1 queda cerrado.\n\n[Hecho] El cliente aprobó el baseline de PI-2 el 2026-07-25: campos de propuesta —incluido motivo de visita—, teléfonos tipificados como intake, expiración sliding de 30 días, guardado explícito sin autosave, link de sala de espera single-use de 30 minutos, permiso futuro `patientportal.intake.manage` solo para `TenantAdmin`, scope futuro `patient_intake` y secuencia PI-2A → PI-2B → PI-2C → PI-2D. ADR 016 registra la decisión.\n\n[Hecho] PI-2A — Patient Intake Domain and Persistence (#31) queda completado mediante PR #32: `PatientIntake`, 39 respuestas separadas de Clinical, revisiones inmutables por guardado efectivo, expiración `Draft / Expired`, baseline canónico para conflictos futuros, `RowVersion`, filtros/write enforcement tenant-aware, restricciones SQL e integración EF mediante migración `20260725182044_AddPatientIntakeDraftFoundation`. No agrega endpoints, JWT scope, staff permission, UI de intake ni writes canónicos. PI-2B es el siguiente gate.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] fase abierta; PI-1 completado mediante PI-1A a PI-1D; PI-2 es el siguiente slice sujeto a decisiones explícitas. La autenticación backend y el frontend Angular acotado de pacientes existen, pero no hay captura de intake todavía.",
    "**Estado** — [Hecho] fase abierta; PI-1 completado mediante PI-1A a PI-1D; PI-2 activa bajo ADR 016; PI-2A completado y PI-2B es el siguiente gate. Existe persistencia de draft, pero todavía no hay endpoints ni captura Angular de intake.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Ubicación** — [Hecho] fase actual posterior al MVP aceptado; se implementa mediante PI-1A → PI-1B → PI-1C → PI-1D antes de abrir PI-2.",
    "**Ubicación** — [Hecho] fase actual posterior al MVP aceptado; PI-1 está cerrado y PI-2 se implementa de forma obligatoria mediante PI-2A → PI-2B → PI-2C → PI-2D antes de abrir PI-3.",
)
replace_once(
    "STATE — BigSmile.md",
    "- ADR 015 — `docs/decisions/015-patient-portal-frontend-session-boundary.md`.",
    "- ADR 015 — `docs/decisions/015-patient-portal-frontend-session-boundary.md`.\n- ADR 016 — `docs/decisions/016-patient-intake-draft-baseline.md`.",
)
replace_once(
    "STATE — BigSmile.md",
    "- PI-2 Intake Draft — issue #5.",
    "- PI-2 Intake Draft — issue #5.\n- PI-2A Domain/Persistence — issue #31 / PR #32.",
)
replace_once(
    "STATE — BigSmile.md",
    "1. Preservar PI-1 (#4) como foundation completado mediante PI-1A a PI-1D, sin acceso paciente a módulos canónicos.\n\n2. Resolver antes de abrir PI-2 (#5) los campos demográficos/contacto editables, el tratamiento de teléfonos, el ownership/lifecycle del link de sala de espera, la expiración de drafts y save explícito vs autosave acotado.\n\n3. Mantener tokens de paciente solo en memoria; no introducir `localStorage`, refresh token ni recuperación remota sin una decisión posterior.\n\n4. Mantener PI-3 y PI-4 no iniciados y prohibir aplicación canónica desde endpoints de paciente.\n\n5. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n6. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n7. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n8. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n9. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
    "1. Preservar PI-1 (#4) como foundation completado mediante PI-1A a PI-1D, sin acceso paciente a módulos canónicos.\n\n2. Preservar PI-2A (#31) como foundation de dominio/persistencia completado y abrir únicamente PI-2B para create/get/save self-only de paciente existente, sin waiting-room bootstrap ni aplicación canónica.\n\n3. Mantener la secuencia PI-2A → PI-2B → PI-2C → PI-2D; `patientportal.intake.manage`, link de 30 minutos y scope `patient_intake` pertenecen a PI-2C.\n\n4. Mantener guardado explícito, no-op sin revisión, expiración sliding de 30 días, `Unknown` distinto de `No` y revisiones append-only.\n\n5. Mantener tokens de paciente solo en memoria; no introducir `localStorage`, refresh token ni recuperación remota sin una decisión posterior.\n\n6. Mantener PI-3 y PI-4 no iniciados y prohibir aplicación canónica desde endpoints de paciente.\n\n7. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.\n\n8. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n9. Mantener diferidas las `doctor-based views` y cualquier linkage cross-module no aceptado.\n\n10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-2.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Patient-facing identity** — [Hecho] PI-1A a PI-1D establecen persistencia, invitaciones staff, autenticación/sesión backend y frontend Angular separado. La frontera usa realm por subdominio, scheme/issuer/audience/secret separados, no emite roles/permisos staff, valida `SessionVersion` en cada request, no acepta `PatientId`/`TenantId` como autoridad pública, no permite platform override, mantiene tokens solo en memoria y no aplica cambios canónicos. El intake sigue pendiente.",
    "**Patient-facing identity e intake** — [Hecho] PI-1A a PI-1D establecen acceso separado. PI-2A agrega únicamente persistencia tenant-owned de draft, respuestas y revisiones; no cambia claims ni expone endpoints. La frontera no acepta `PatientId`/`TenantId` como autoridad pública, no permite platform override, mantiene tokens solo en memoria y no aplica cambios canónicos. PI-2B debe derivar ownership de la cuenta autenticada.",
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Phase 2.1 Patient Intake and Portal Foundation está activa mediante PI-1; el portal amplio permanece en Phase 4.",
    "[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Phase 2.1 Patient Intake and Portal Foundation está activa; PI-1 y PI-2A están completados, PI-2B es el siguiente gate y el portal amplio permanece en Phase 4.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 015; Phase 2.1 abierta; PI-1 completado; PI-2 es el siguiente slice sujeto a decisiones de alcance.\n\n**Contexto:** PI-1A a PI-1C ya establecían cuentas, invitaciones y auth/session backend tenant-owned. PI-1D debía cerrar el riesgo de browser session sin mezclar staff/patient ni abrir intake.\n\n**Decisión:** Aceptar PI-1D mediante ADR 015 con route tree y shell separados, activación por fragment con limpieza inmediata, token/session solo en memoria, interceptores/guards no superpuestos, UX genérica, pruebas y runbook de recovery.\n\n**Consecuencias:** Pacientes existentes ya pueden activar, iniciar/cerrar sesión y recuperar acceso con asistencia sin permisos staff ni acceso canónico. PI-1 queda cerrado. Intake, revisión/aplicación, payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 016; Phase 2.1 abierta; PI-1 y PI-2A completados; PI-2B es el siguiente gate.\n\n**Contexto:** PI-1 cerró la identidad/sesión de paciente. El cliente autorizó el alcance de datos, teléfonos como propuestas, expiración, guardado explícito, link de sala de espera y scope intake-only antes de persistir información médica declarada.\n\n**Decisión:** Aceptar ADR 016 y PI-2A con `PatientIntake` tenant-owned, 39 respuestas separadas de Clinical, revisiones append-only por cambios efectivos, baseline canónico, expiración sliding de 30 días, `RowVersion`, restricciones SQL y migración aditiva; sin endpoints ni writes canónicos.\n\n**Consecuencias:** La base de draft ya existe de forma trazable y aislada. PI-2B puede abrir create/get/save para paciente existente. Waiting-room link, `patient_intake`, UI Angular, submit/review/apply, payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen pendientes.",
)

replace_once(
    "README.md",
    "Phase 2.1 is active with PI-1 completed through PI-1A to PI-1D. Account/invitation persistence, tenant-admin invitation management, separate patient authentication/session and the bounded Angular patient-auth experience are accepted. Intake remains unavailable until PI-2. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
    "Phase 2.1 is active with PI-1 and PI-2A completed. ADR 016 accepts the intake baseline, and the repository now contains tenant-owned draft, fixed-answer and immutable-revision persistence without public intake endpoints or canonical writes. PI-2B is next. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
)
replace_once(
    "README.md",
    "* **Latest Phase 2.1 milestone completed:** **PI-1 — Access and Invitation Foundation** through PI-1A to PI-1D\n* **Next slice:** **PI-2 — Intake Draft and Self-Service Capture**, pending explicit intake-scope decisions\n* **Public patient runtime:** bounded backend and Angular activation/login/session available; intake remains pending",
    "* **Latest Phase 2.1 slice completed:** **PI-2A — Patient Intake Domain and Persistence**\n* **Next slice:** **PI-2B — Existing-Patient Self-Service Draft**\n* **Public patient runtime:** activation/login/session available; intake endpoints and capture UI remain pending",
)
replace_once(
    "README.md",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015\n* PI-1 is completed through account/invitation persistence, tenant-admin invitation lifecycle, separate patient auth/session backend and bounded Angular patient auth\n* PI-2 (#5) is next only after explicit decisions for proposal fields, contact/phone scope, waiting-room link lifecycle, draft expiry and save behavior\n* PI-3 and PI-4 remain pending for clinic review/application, audit visibility and production hardening\n* PI-1 closure evidence: `docs/pi-1-patient-portal-access-and-security-closure.md`; recovery runbook: `docs/patient-portal-assisted-recovery-runbook.md`",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006 and ADR 012–016\n* PI-1 is completed through account/invitation persistence, tenant-admin invitation lifecycle, separate patient auth/session backend and bounded Angular patient auth\n* PI-2 is active; PI-2A completed the tenant-owned draft/fixed-answer/revision persistence foundation without endpoints or canonical writes\n* PI-2B is next for existing-patient self-only create/get/save; PI-2C retains waiting-room link, `patientportal.intake.manage` and `patient_intake`; PI-2D retains Angular capture\n* PI-3 and PI-4 remain pending for clinic review/application, audit visibility and production hardening\n* PI-1 closure: `docs/pi-1-patient-portal-access-and-security-closure.md`; PI-2 baseline: `docs/decisions/016-patient-intake-draft-baseline.md`",
)

replace_once(
    "AGENTS.md",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 completed through PI-1A to PI-1D, PI-2 next and decision-gated",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 and PI-2A completed, PI-2B next",
)
replace_once(
    "AGENTS.md",
    "- ADR 015 — `docs/decisions/015-patient-portal-frontend-session-boundary.md`\n- PI-1 closure — `docs/pi-1-patient-portal-access-and-security-closure.md`",
    "- ADR 015 — `docs/decisions/015-patient-portal-frontend-session-boundary.md`\n- ADR 016 — `docs/decisions/016-patient-intake-draft-baseline.md`\n- PI-1 closure — `docs/pi-1-patient-portal-access-and-security-closure.md`",
)
replace_once(
    "AGENTS.md",
    "- PI-1D — issue #25 / PR #30",
    "- PI-1D — issue #25 / PR #30\n- PI-2A — issue #31 / PR #32",
)
replace_once(
    "AGENTS.md",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006; PI-1 access decisions are accepted in ADR 012 through ADR 015\n- PI-1 (#4) is completed through PI-1A to PI-1D\n- PI-2 (#5) is next but requires explicit product/data-lifecycle decisions before implementation\n- PI-3 and PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability",
    "Phase 2.1 — Patient Intake and Portal Foundation is active after the accepted MVP:\n- architecture accepted in ADR 006; PI-1 access decisions are accepted in ADR 012 through ADR 015; PI-2 baseline is accepted in ADR 016\n- PI-1 (#4) is completed through PI-1A to PI-1D\n- PI-2 (#5) is active; PI-2A (#31) domain/persistence is completed and PI-2B is next\n- PI-2C retains waiting-room link, dedicated staff permission and `patient_intake` scope; PI-2D retains Angular capture\n- PI-3 and PI-4 remain not started\n- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability",
)
replace_once(
    "AGENTS.md",
    "# Immediate objective\nPreserve completed PI-1 and resolve the bounded product decisions required before opening `PI-2 — Intake Draft and Self-Service Capture`.",
    "# Immediate objective\nPreserve completed PI-1 and PI-2A, and open only `PI-2B — Existing-Patient Self-Service Draft` before waiting-room bootstrap or Angular intake capture.",
)
replace_once(
    "AGENTS.md",
    "- do not open PI-2 until editable fields, phones/contact ownership, waiting-room link lifecycle, draft expiry and save behavior are explicitly decided",
    "- preserve ADR 016 fields, typed-phone proposal ownership, 30-day effective-save expiry, explicit save and append-only revisions\n- keep PI-2B limited to linked existing-patient self-only create/get/save with no canonical Patient/Clinical writes\n- keep waiting-room link, `patientportal.intake.manage` and `patient_intake` scope in PI-2C; keep Angular intake capture in PI-2D",
)

replace_once(
    "PROJECT_MAP.md",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1 completed through PI-1A to PI-1D, PI-2 next and decision-gated",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1 and PI-2A completed, PI-2B next",
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7 and completed PI-1 while preparing decision-gated PI-2:",
    "Preserve Releases 1 through 7, completed PI-1 and completed PI-2A while preparing PI-2B:",
)
replace_once(
    "PROJECT_MAP.md",
    "* keep intake and canonical module access outside accepted PI-1\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1 is accepted with migrations, backend/frontend tests, ADRs, closure evidence, runbook and CI; PI-2 is the next decision-gated step",
    "* preserve PI-2A tenant-owned draft, fixed-answer, immutable-revision, expiry and concurrency semantics\n* keep PI-2B limited to linked-account current-intake create/get/save and keep canonical writes prohibited\n* reserve waiting-room link/permission/scope for PI-2C and Angular capture for PI-2D\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1 and PI-2A are accepted with migrations, tests and ADR evidence; PI-2B is the next bounded step",
)
replace_once(
    "PROJECT_MAP.md",
    "* BillingDocument / future Payment records",
    "* BillingDocument / future Payment records\n* PatientIntake",
)
replace_once(
    "PROJECT_MAP.md",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013/014/015. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation management; PI-1C owns the separate patient activation/login/self-session backend; and PI-1D owns the separate Angular route/shell/session/interceptor boundary plus operational recovery runbook. PI-1 never reuses staff membership semantics or staff permissions.",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013/014/015. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation management; PI-1C owns the separate patient activation/login/self-session backend; and PI-1D owns the separate Angular route/shell/session/interceptor boundary plus operational recovery runbook. ADR 016 and PI-2A add a separate tenant-owned intake draft/revision model without changing staff identity or canonical Patient/Clinical ownership.",
)
replace_once(
    "PROJECT_MAP.md",
    "Future Reporting owns deeper treatment/scheduling/billing metrics, charts, exports and analytics. Revenue/balance metrics, branch/doctor dashboards, BI, real-time and AI recommendations remain deferred.\n\n---\n\n## 8. Tenant and Branch Map",
    "Future Reporting owns deeper treatment/scheduling/billing metrics, charts, exports and analytics. Revenue/balance metrics, branch/doctor dashboards, BI, real-time and AI recommendations remain deferred.\n\n### 7.12 Patient Intake and Portal\n\nAccepted PI-1 and PI-2A ownership:\n\n* tenant-owned portal accounts and invitations separate from staff identity\n* patient-only authentication/session and Angular boundary\n* tenant-owned `PatientIntake` current draft\n* separate fixed questionnaire answer rows using the existing 39-key catalog\n* immutable effective-save revisions with changed-field ids and versioned snapshot JSON\n* optional same-tenant Branch operational context\n* `Draft / Expired`, 30-day effective-save expiry and `RowVersion`\n\nPI-2A exposes no endpoint and never modifies canonical Patient or ClinicalRecord. PI-2B owns existing-patient self-service endpoints; PI-2C owns waiting-room links and intake-only scope; PI-2D owns capture UI.\n\n---\n\n## 8. Tenant and Branch Map",
)
replace_once(
    "PROJECT_MAP.md",
    "* patient portal account/invitation -> tenant-owned identity/bootstrap records; patient linkage and login uniqueness remain tenant-scoped",
    "* patient portal account/invitation -> tenant-owned identity/bootstrap records; patient linkage and login uniqueness remain tenant-scoped\n* patient intake/answer/revision -> tenant-owned proposal records; optional Branch is operational only and revisions are append-only",
)

replace_once(
    "docs/product-roadmap.md",
    "Active after formal MVP acceptance. PI-1 is completed through PI-1A to PI-1D; PI-2 is next and decision-gated.",
    "Active after formal MVP acceptance. PI-1 and PI-2A are completed; PI-2B is next.",
)
replace_once(
    "docs/product-roadmap.md",
    "Active under ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015. PI-1 (#4) is completed through PI-1A to PI-1D. The bounded backend and Angular patient-auth boundary is accepted; PI-2 (#5) is next, but intake is not yet implemented.",
    "Active under ADR 006 and ADR 012–016. PI-1 (#4) is completed. PI-2 (#5) is active; PI-2A (#31) completed domain/persistence for tenant-owned drafts, fixed answers and immutable revisions. PI-2B is next; intake endpoints and Angular capture are not yet implemented.",
)
replace_once(
    "docs/product-roadmap.md",
    "2. PI-2 — Intake Draft and Self-Service Capture — issue #5 — next, decision-gated\n3. PI-3 — Submit, Clinic Review and Canonical Apply — issue #6",
    "2. PI-2 — Intake Draft and Self-Service Capture — issue #5 — active\n   1. PI-2A domain/persistence — #31 — completed\n   2. PI-2B existing-patient self-service draft — next\n   3. PI-2C waiting-room link and intake-only scope — pending\n   4. PI-2D Angular intake capture/closure — pending\n3. PI-3 — Submit, Clinic Review and Canonical Apply — issue #6",
)
replace_once(
    "docs/product-roadmap.md",
    "- ADR 015: `docs/decisions/015-patient-portal-frontend-session-boundary.md`",
    "- ADR 015: `docs/decisions/015-patient-portal-frontend-session-boundary.md`\n- ADR 016: `docs/decisions/016-patient-intake-draft-baseline.md`",
)

replace_once(
    "docs/architecture.md",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary, ADR 012 defines the pilot access baseline, ADR 013 restricts invitation management, ADR 014 establishes the separate backend bearer/session boundary, and ADR 015 establishes the separate Angular route/shell/interceptor boundary with fragment activation and memory-only session state.",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary; ADR 012–015 establish access and browser-session separation; ADR 016 establishes patient-proposed fields, typed-phone proposal ownership, fixed-question reuse, explicit-save revisions, 30-day expiry and the future waiting-room/intake-only scope. PI-2A implements only domain/persistence and no public endpoint.",
)
replace_once(
    "docs/architecture.md",
    "* `TreatmentPlan`\n* `Payment`",
    "* `TreatmentPlan`\n* `PatientIntake`\n* `Payment`",
)

replace_once(
    "docs/tenant-model.md",
    "- patient portal accounts and invitations\n- tenant users",
    "- patient portal accounts and invitations\n- patient intake drafts, fixed answers and immutable revisions\n- tenant users",
)
replace_once(
    "docs/tenant-model.md",
    "Patient-facing identity is separate from staff identity under ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015.",
    "Patient-facing identity and intake are separate from staff identity and canonical clinical ownership under ADR 006 and ADR 012–016.",
)
replace_once(
    "docs/tenant-model.md",
    "- `PatientId`, portal account and linked Patient come from verified server context\n- phone, date of birth and contact data cannot be used to claim a record publicly\n- PI-1 is completed through PI-1A to PI-1D; intake opens only through PI-2 after its own decisions",
    "- `TenantId`, portal account and linked Patient come from verified server context\n- phone, date of birth and contact data cannot be used to claim a record publicly\n- `PatientIntake`, its medical answers and revisions carry `TenantId`; optional `BranchId` remains operational and same-tenant\n- PI-2A provides one active `Draft` per account, soft `Expired`, `RowVersion` and append-only effective-save revisions without endpoints or canonical writes\n- PI-2B is the next gate; waiting-room `patient_intake` scope remains PI-2C",
)

replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** In progress; PI-1 completed through PI-1A to PI-1D; PI-2 next and decision-gated",
    "- **Status:** In progress; PI-1 and PI-2A completed; PI-2B next",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Architecture decisions:** ADR 006, ADR 012, ADR 013, ADR 014 and ADR 015",
    "- **Architecture decisions:** ADR 006 and ADR 012–016",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: active; PI-1 is completed through PI-1A to PI-1D; PI-2 is next and decision-gated.",
    "- Phase 2.1: active; PI-1 and PI-2A are completed; PI-2B is next.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-2 intake draft | Next; decision-gated; not implemented | Issue #5 |",
    "| PI-2 intake draft | Active; PI-2A domain/persistence completed; PI-2B next | Issue #5 / #31 / PR #32 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| Patient-facing backend/API/database | Access/auth foundation implemented through PI-1 | PRs #26, #28 and #29 |",
    "| Patient-facing backend/API/database | Access/auth through PI-1 plus intake persistence through PI-2A | PRs #26, #28, #29 and #32 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| Patient-facing intake | Not implemented | PI-2 |",
    "| Patient-facing intake | Domain/persistence only; no API/UI | PI-2A / PR #32 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "### PI-2 — Intake Draft and Self-Service Capture — issue #5\n\nScope:\n- `PatientIntake` and fixed questionnaire draft answers;\n- existing-patient and new-patient flows;\n- self-only read/save;\n- explicit effective-save revisions;\n- optimistic concurrency;\n- mobile/tablet Angular feature outside staff shell.\n\nExit gate:\n- no canonical Patient/ClinicalRecord changes;\n- identical save creates no revision;\n- unknown/duplicate question keys rejected;\n- self-scope/cross-tenant/concurrency tests and frontend states green.",
    "### PI-2 — Intake Draft and Self-Service Capture — issue #5\n\nApproved sequence under ADR 016:\n\n1. **PI-2A — Intake Domain and Persistence — #31 — completed through PR #32**\n   - tenant-owned `PatientIntake`, fixed answer rows and immutable revisions;\n   - linked/unlinked origin, optional same-tenant Branch context and Patient baseline;\n   - approved proposal fields, 39 exact keys, `Draft / Expired`, 30-day expiry and `RowVersion`;\n   - effective save creates one revision; identical save creates none;\n   - additive migration, filters/write enforcement and tests;\n   - no endpoint or canonical write.\n2. **PI-2B — Existing-Patient Self-Service Draft — next**\n   - explicit create/get/save from the linked authenticated account;\n   - no arbitrary intake/Patient selector and no GET side effects;\n   - concurrency conflict and current-draft expiry handling.\n3. **PI-2C — Waiting-Room Link and Intake-Only Account Scope — pending**\n   - 30-minute hash-only link, `patientportal.intake.manage`, unlinked account and `patient_intake`.\n4. **PI-2D — Angular Intake Capture and PI-2 Closure — pending**\n   - mobile/tablet sections, explicit save, dirty/saved/conflict/expired states and fixed questionnaire UX.\n\nPI-2 exit gate:\n- no canonical Patient/ClinicalRecord changes;\n- identical save creates no revision;\n- unknown/duplicate question keys rejected;\n- self-scope/cross-tenant/concurrency tests and frontend states green.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "### Before PI-2\n\n- Exact demographic/contact fields the patient may propose.\n- Whether phone slots are intake proposals or a separate Patient Contact Details slice.\n- Draft expiration/abandonment policy.\n- Explicit save vs bounded debounced autosave.",
    "### Approved PI-2 baseline under ADR 016\n\n- Proposed identity/demographic/contact/responsible-party fields plus `ReasonForVisit`.\n- `Preferred / Mobile / Home / Work` phones remain intake proposals; canonical typed contacts are a prerequisite before PI-3 apply.\n- Exact existing 39-key questionnaire, `Unknown / Yes / No`, optional 500-character details.\n- One active draft per account; 30-day sliding expiry after effective saves; soft expiry only.\n- Explicit save; no autosave; identical save creates no revision and does not extend expiry.\n- Waiting-room link single-use/hash-only/30 minutes, future `patientportal.intake.manage` TenantAdmin-only and no platform override.\n- Future unlinked account uses `scope=patient_intake` with `intake_id`, no `patient_id`.\n- Mandatory sequence PI-2A → PI-2B → PI-2C → PI-2D.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "1. Preserve PI-1 / #4 as completed through PI-1A to PI-1D and PRs #26, #28, #29 and #30.\n2. Resolve PI-2 decisions for editable proposal fields, phones/contact ownership, waiting-room link lifecycle, draft expiry and save behavior.\n3. Open PI-2 only after those decisions are accepted; do not add canonical application to PI-2.\n4. Keep PI-3 and PI-4 pending until their own gates.",
    "1. Preserve PI-1 / #4 and PI-2A / #31 as completed through PRs #26, #28, #29, #30 and #32.\n2. Open only PI-2B for existing-patient self-only create/get/save over the accepted aggregate.\n3. Keep waiting-room link, staff permission and `patient_intake` scope in PI-2C; keep Angular intake capture in PI-2D.\n4. Do not add canonical application to PI-2; keep PI-3 and PI-4 pending until their own gates.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "**Decision:** Open Phase 2.1 under ADR 012 and implement the approved access baseline through PI-1A to PI-1D before opening intake.\n\n**Consequence:** PI-1A through PI-1D now provide tenant-owned account/invitation persistence, staff invitation lifecycle, separate patient auth/session backend and bounded Angular activation/login/session with an assisted-recovery runbook. PI-1 is complete, while intake remains unavailable until PI-2. PI-2 to PI-4 remain unimplemented.",
    "**Decision:** Implement PI-1 under ADR 012–015, then implement PI-2 only in the ADR 016 sequence PI-2A → PI-2B → PI-2C → PI-2D.\n\n**Consequence:** PI-1 and PI-2A now provide secure access plus tenant-owned intake draft/answer/revision persistence. No intake endpoint or UI exists yet. PI-2B is next; PI-2C/PI-2D, review/apply and final hardening remain unimplemented.",
)

replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- **Tracking:** issue #2; PI-1 #4; PI-1A–PI-1D #22–#25; PI-2–PI-4 #5–#7",
    "- **Tracking:** issue #2; PI-1 #4; PI-1A–PI-1D #22–#25; PI-2 #5; PI-2A #31; PI-3–PI-4 #6–#7",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "On 2026-07-25 the client authorized continuing with PI-1D. ADR 015 accepts the separate Angular route/shell/interceptor boundary, activation token fragment cleanup, memory-only session and assisted-recovery runbook.",
    "On 2026-07-25 the client authorized continuing with PI-1D. ADR 015 accepts the separate Angular route/shell/interceptor boundary, activation token fragment cleanup, memory-only session and assisted-recovery runbook.\n\nOn 2026-07-25 the client approved ADR 016: exact proposal fields including `ReasonForVisit`, typed phones as intake proposals, 30-day effective-save expiry, explicit save without autosave, a future single-use 30-minute waiting-room link, `patientportal.intake.manage` for `TenantAdmin` only, unlinked `patient_intake` scope and PI-2A → PI-2B → PI-2C → PI-2D sequencing.",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- PI-1D Angular patient auth/security closure completed through PR #30 under ADR 015.\n- Public patient auth API and bounded frontend exist; PI-2–PI-4 and intake are not implemented.",
    "- PI-1D Angular patient auth/security closure completed through PR #30 under ADR 015.\n- PI-2 baseline accepted under ADR 016.\n- PI-2A domain/persistence completed through PR #32 with migration `20260725182044_AddPatientIntakeDraftFoundation`.\n- Public patient auth API and bounded frontend exist; intake API/UI, PI-2B–PI-2D, PI-3 and PI-4 remain pending.",
)
