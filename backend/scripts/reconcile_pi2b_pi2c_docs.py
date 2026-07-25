from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_once(path: str, old: str, new: str) -> None:
    file_path = ROOT / path
    text = file_path.read_text(encoding="utf-8")
    old_count = text.count(old)
    if old_count == 1:
        file_path.write_text(text.replace(old, new, 1), encoding="utf-8")
        return
    if old_count == 0 and new in text:
        return
    raise RuntimeError(
        f"Expected exactly one source occurrence in {path}; found {old_count}.\nSOURCE:\n{old}"
    )


# STATE — canonical phase status, tracking, backlog and decision note.
replace_once(
    "STATE — BigSmile.md",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 a ADR 015 cierran el acceso, invitaciones, autenticación/sesión y frontera Angular; ADR 016 fija el baseline de datos, draft lifecycle, revisión append-only y futuro bootstrap de sala de espera. PI-1 queda completado. PI-2 está activa: PI-2A incorpora dominio/persistencia tenant-aware sin endpoints ni captura Angular; PI-2B es el siguiente gate.",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 a ADR 015 cierran el acceso, invitaciones, autenticación/sesión y frontera Angular; ADR 016 fija el baseline de datos y draft lifecycle; ADR 017 acepta la API self-only de paciente existente y abre el bootstrap de sala de espera. PI-1 queda completado. PI-2 está activa: PI-2A y PI-2B están completados; PI-2C está abierta mediante #35 y se implementa secuencialmente como #36 → #37 → #38.",
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] PI-2A — Patient Intake Domain and Persistence (#31) queda completado mediante PR #32: `PatientIntake`, 39 respuestas separadas de Clinical, revisiones inmutables por guardado efectivo, expiración `Draft / Expired`, baseline canónico para conflictos futuros, `RowVersion`, filtros/write enforcement tenant-aware, restricciones SQL e integración EF mediante migración `20260725182044_AddPatientIntakeDraftFoundation`. No agrega endpoints, JWT scope, staff permission, UI de intake ni writes canónicos. PI-2B es el siguiente gate.",
    "[Hecho] PI-2A — Patient Intake Domain and Persistence (#31) queda completado mediante PR #32: `PatientIntake`, 39 respuestas separadas de Clinical, revisiones inmutables por guardado efectivo, expiración `Draft / Expired`, baseline canónico para conflictos futuros, `RowVersion`, filtros/write enforcement tenant-aware, restricciones SQL e integración EF mediante migración `20260725182044_AddPatientIntakeDraftFoundation`. No agrega endpoints, JWT scope, staff permission, UI de intake ni writes canónicos.\n\n[Hecho] PI-2B — Existing-Patient Self-Service Draft (#33) queda completado mediante PR #34 y merge commit `7325a73e7f86ae0e6f0557574fe9d9756a89293f`: `POST / GET / PUT /api/patient-portal/intake`, ownership derivado de sesión patient-only, GET sin side effects, no-store, 39 respuestas `Unknown`, save explícito con optimistic concurrency, no-op sin revisión y cambio efectivo con una revisión append-only. CI #315 quedó verde y no se modifican datos canónicos.\n\n[Hecho] PI-2C — Waiting-Room Link and Intake-Only Scope (#35) queda abierta con autorización explícita y alcance limitado: credencial single-use/hash-only/30 minutos, permiso `patientportal.intake.manage` solo `TenantAdmin`, cuenta unlinked con `scope=patient_intake` y UI staff mínima para generar/copiar/imprimir/QR local. Se implementa únicamente mediante PI-2C1 #36 → PI-2C2 #37 → PI-2C3 #38. PI-2D permanece pendiente.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] fase abierta; PI-1 completado mediante PI-1A a PI-1D; PI-2 activa bajo ADR 016; PI-2A completado y PI-2B es el siguiente gate. Existe persistencia de draft, pero todavía no hay endpoints ni captura Angular de intake.",
    "**Estado** — [Hecho] fase abierta; PI-1 completado mediante PI-1A a PI-1D; PI-2 activa bajo ADR 016/017; PI-2A y PI-2B completados. La API self-only para pacientes existentes existe; PI-2C está activa para bootstrap de sala de espera y PI-2D conserva la captura Angular del cuestionario.",
)
replace_once(
    "STATE — BigSmile.md",
    "- ADR 016 — `docs/decisions/016-patient-intake-draft-baseline.md`.\n- Cierre PI-1",
    "- ADR 016 — `docs/decisions/016-patient-intake-draft-baseline.md`.\n- ADR 017 — `docs/decisions/017-existing-patient-intake-api-and-waiting-room-bootstrap.md`.\n- Cierre PI-1",
)
replace_once(
    "STATE — BigSmile.md",
    "- PI-2A Domain/Persistence — issue #31 / PR #32.\n- PI-3 Submit, Review and Apply",
    "- PI-2A Domain/Persistence — issue #31 / PR #32.\n- PI-2B Existing-Patient Self-Service — issue #33 / PR #34.\n- PI-2C Waiting-Room Bootstrap — issue #35.\n- PI-2C1 Credential/Staff API — issue #36.\n- PI-2C2 Intake-Only Session — issue #37.\n- PI-2C3 Staff Link/Print/QR UI — issue #38.\n- PI-3 Submit, Review and Apply",
)
replace_once(
    "STATE — BigSmile.md",
    "2. Preservar PI-2A (#31) como foundation de dominio/persistencia completado y abrir únicamente PI-2B para create/get/save self-only de paciente existente, sin waiting-room bootstrap ni aplicación canónica.\n\n3. Mantener la secuencia PI-2A → PI-2B → PI-2C → PI-2D; `patientportal.intake.manage`, link de 30 minutos y scope `patient_intake` pertenecen a PI-2C.",
    "2. Preservar PI-2B (#33 / PR #34) como API self-only completada para pacientes existentes y mantener sus contratos id-less, no-store, GET sin side effects y optimistic concurrency.\n\n3. Implementar PI-2C solo mediante #36 → #37 → #38: credencial de 30 minutos, `patientportal.intake.manage`, scope `patient_intake` y UI staff mínima; PI-2D conserva la captura Angular del paciente.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Patient-facing identity e intake** — [Hecho] PI-1A a PI-1D establecen acceso separado. PI-2A agrega únicamente persistencia tenant-owned de draft, respuestas y revisiones; no cambia claims ni expone endpoints. La frontera no acepta `PatientId`/`TenantId` como autoridad pública, no permite platform override, mantiene tokens solo en memoria y no aplica cambios canónicos. PI-2B debe derivar ownership de la cuenta autenticada.",
    "**Patient-facing identity e intake** — [Hecho] PI-1A a PI-1D establecen acceso separado. PI-2A agrega persistencia tenant-owned de draft, respuestas y revisiones. PI-2B expone create/get/save id-less para la cuenta vinculada y deriva Tenant/Patient/intake de la sesión validada, sin platform override ni aplicación canónica. PI-2C debe mantener separado el trust mode unlinked mediante `patient_intake`, token single-use y validación server-side de account/Tenant/intake/SessionVersion.",
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Phase 2.1 Patient Intake and Portal Foundation está activa; PI-1 y PI-2A están completados, PI-2B es el siguiente gate y el portal amplio permanece en Phase 4.",
    "[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Phase 2.1 Patient Intake and Portal Foundation está activa; PI-1, PI-2A y PI-2B están completados, PI-2C está activa y el portal amplio permanece en Phase 4.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 016; Phase 2.1 abierta; PI-1 y PI-2A completados; PI-2B es el siguiente gate.\n\n**Contexto:** PI-1 cerró la identidad/sesión de paciente. El cliente autorizó el alcance de datos, teléfonos como propuestas, expiración, guardado explícito, link de sala de espera y scope intake-only antes de persistir información médica declarada.\n\n**Decisión:** Aceptar ADR 016 y PI-2A con `PatientIntake` tenant-owned, 39 respuestas separadas de Clinical, revisiones append-only por cambios efectivos, baseline canónico, expiración sliding de 30 días, `RowVersion`, restricciones SQL y migración aditiva; sin endpoints ni writes canónicos.\n\n**Consecuencias:** La base de draft ya existe de forma trazable y aislada. PI-2B puede abrir create/get/save para paciente existente. Waiting-room link, `patient_intake`, UI Angular, submit/review/apply, payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen pendientes.",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 017; Phase 2.1 abierta; PI-1, PI-2A y PI-2B completados; PI-2C activa.\n\n**Contexto:** PI-2B expone por primera vez información médica declarada mediante una API patient-self. El cliente autorizó continuar exclusivamente con bootstrap de sala de espera, identidad intake-only y handoff staff mínimo.\n\n**Decisión:** Aceptar PI-2B mediante PR #34 con contratos id-less/no-store y abrir PI-2C bajo ADR 017 como secuencia #36 → #37 → #38, usando credencial hash-only de 30 minutos, permiso TenantAdmin-only, scope `patient_intake` sin `patient_id` y QR generado localmente.\n\n**Consecuencias:** Pacientes existentes ya pueden crear/leer/guardar su propio draft sin writes canónicos. Waiting-room bootstrap, UI staff de handoff y sesión intake-only siguen pendientes dentro de PI-2C; la captura Angular del cuestionario permanece PI-2D y submit/review/apply permanece PI-3.",
)

# README status and roadmap position.
replace_once(
    "README.md",
    "Phase 2.1 is active with PI-1 and PI-2A completed. ADR 016 accepts the intake baseline, and the repository now contains tenant-owned draft, fixed-answer and immutable-revision persistence without public intake endpoints or canonical writes. PI-2B is next. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
    "Phase 2.1 is active with PI-1, PI-2A and PI-2B completed. ADR 016 accepts the intake baseline and ADR 017 accepts the existing-patient self-only API while opening the waiting-room bootstrap boundary. PI-2C is active; the questionnaire UI remains PI-2D. Code in reminders/manual reminders, providers, jobs, online booking, later PI slices or advanced analytics still does not imply acceptance.",
)
replace_once(
    "README.md",
    "* **Latest Phase 2.1 slice completed:** **PI-2A — Patient Intake Domain and Persistence**\n* **Next slice:** **PI-2B — Existing-Patient Self-Service Draft**\n* **Public patient runtime:** activation/login/session available; intake endpoints and capture UI remain pending",
    "* **Latest Phase 2.1 slice completed:** **PI-2B — Existing-Patient Self-Service Draft**\n* **Current slice:** **PI-2C — Waiting-Room Link and Intake-Only Scope**\n* **Public patient runtime:** activation/login/session and linked-patient intake API available; waiting-room bootstrap and intake capture UI remain pending",
)
replace_once(
    "README.md",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006 and ADR 012–016\n* PI-1 is completed through account/invitation persistence, tenant-admin invitation lifecycle, separate patient auth/session backend and bounded Angular patient auth\n* PI-2 is active; PI-2A completed the tenant-owned draft/fixed-answer/revision persistence foundation without endpoints or canonical writes\n* PI-2B is next for existing-patient self-only create/get/save; PI-2C retains waiting-room link, `patientportal.intake.manage` and `patient_intake`; PI-2D retains Angular capture\n* PI-3 and PI-4 remain pending for clinic review/application, audit visibility and production hardening\n* PI-1 closure: `docs/pi-1-patient-portal-access-and-security-closure.md`; PI-2 baseline: `docs/decisions/016-patient-intake-draft-baseline.md`",
    "* **Phase 2.1 — Patient Intake and Portal Foundation** is the active phase under ADR 006 and ADR 012–017\n* PI-1 is completed through account/invitation persistence, tenant-admin invitation lifecycle, separate patient auth/session backend and bounded Angular patient auth\n* PI-2A completed the tenant-owned draft/fixed-answer/revision persistence foundation\n* PI-2B completed linked-patient self-only `POST / GET / PUT /api/patient-portal/intake` with no-store, no GET side effects and optimistic concurrency\n* PI-2C is active only for the one-time waiting-room credential, `patientportal.intake.manage`, unlinked `patient_intake` session and minimal staff copy/print/local-QR UI; PI-2D retains patient capture\n* PI-3 and PI-4 remain pending for clinic review/application, audit visibility and production hardening\n* PI-1 closure: `docs/pi-1-patient-portal-access-and-security-closure.md`; PI-2 decisions: ADR 016 and ADR 017",
)

# AGENTS operational direction.
replace_once(
    "AGENTS.md",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 and PI-2A completed, PI-2B next",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1, PI-2A and PI-2B completed, PI-2C active",
)
replace_once(
    "AGENTS.md",
    "- ADR 016 — `docs/decisions/016-patient-intake-draft-baseline.md`\n- PI-1 closure",
    "- ADR 016 — `docs/decisions/016-patient-intake-draft-baseline.md`\n- ADR 017 — `docs/decisions/017-existing-patient-intake-api-and-waiting-room-bootstrap.md`\n- PI-1 closure",
)
replace_once(
    "AGENTS.md",
    "- PI-2A — issue #31 / PR #32",
    "- PI-2A — issue #31 / PR #32\n- PI-2B — issue #33 / PR #34\n- PI-2C — issue #35; sub-slices #36, #37 and #38",
)
replace_once(
    "AGENTS.md",
    "- PI-2 (#5) is active; PI-2A (#31) domain/persistence is completed and PI-2B is next\n- PI-2C retains waiting-room link, dedicated staff permission and `patient_intake` scope; PI-2D retains Angular capture",
    "- PI-2 (#5) is active; PI-2A (#31) and PI-2B (#33) are completed\n- PI-2C (#35) is active only through #36 → #37 → #38 for waiting-room credential, dedicated staff permission, `patient_intake` scope and minimal staff handoff UI; PI-2D retains patient Angular capture",
)
replace_once(
    "AGENTS.md",
    "# Immediate objective\nPreserve completed PI-1 and PI-2A, and open only `PI-2B — Existing-Patient Self-Service Draft` before waiting-room bootstrap or Angular intake capture.",
    "# Immediate objective\nPreserve completed PI-1, PI-2A and PI-2B, and implement only `PI-2C1 — Waiting-Room Credential Foundation and TenantAdmin Management API` before anonymous consume, intake-only JWT or Angular handoff UI.",
)
replace_once(
    "AGENTS.md",
    "- keep PI-2B limited to linked existing-patient self-only create/get/save with no canonical Patient/Clinical writes\n- keep waiting-room link, `patientportal.intake.manage` and `patient_intake` scope in PI-2C; keep Angular intake capture in PI-2D",
    "- preserve PI-2B as linked existing-patient self-only create/get/save with id-less ownership, no-store, optimistic concurrency and no canonical Patient/Clinical writes\n- implement PI-2C sequentially: #36 credential/staff API, #37 transactional activation/`patient_intake`, #38 staff copy/print/local-QR UI; keep patient Angular intake capture in PI-2D",
)

# PROJECT_MAP status and module ownership.
replace_once(
    "PROJECT_MAP.md",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1 and PI-2A completed, PI-2B next",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1, PI-2A and PI-2B completed, PI-2C active",
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7, completed PI-1 and completed PI-2A while preparing PI-2B:",
    "Preserve Releases 1 through 7 and completed PI-1/PI-2A/PI-2B while implementing only PI-2C:",
)
replace_once(
    "PROJECT_MAP.md",
    "* preserve PI-2A tenant-owned draft, fixed-answer, immutable-revision, expiry and concurrency semantics\n* keep PI-2B limited to linked-account current-intake create/get/save and keep canonical writes prohibited\n* reserve waiting-room link/permission/scope for PI-2C and Angular capture for PI-2D\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1 and PI-2A are accepted with migrations, tests and ADR evidence; PI-2B is the next bounded step",
    "* preserve PI-2A tenant-owned draft, fixed-answer, immutable-revision, expiry and concurrency semantics\n* preserve PI-2B id-less self-only create/get/save, GET without side effects, no-store and canonical-write prohibition\n* implement PI-2C only as #36 credential/staff API → #37 intake-only session → #38 staff handoff UI; reserve patient capture for PI-2D\n* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries\n* PI-1, PI-2A and PI-2B are accepted with tests/ADR evidence; PI-2C1 is the next bounded step",
)
replace_once(
    "PROJECT_MAP.md",
    "Patient-facing identity is an active bounded boundary under ADR 006/012/013/014/015. PI-1A owns account/invitation persistence; PI-1B owns tenant-admin-only invitation management; PI-1C owns the separate patient activation/login/self-session backend; and PI-1D owns the separate Angular route/shell/session/interceptor boundary plus operational recovery runbook. ADR 016 and PI-2A add a separate tenant-owned intake draft/revision model without changing staff identity or canonical Patient/Clinical ownership.",
    "Patient-facing identity is an active bounded boundary under ADR 006 and ADR 012–017. PI-1 owns linked-patient access; PI-2A owns tenant-owned intake persistence; PI-2B owns linked-patient self-only create/get/save; PI-2C owns the separate waiting-room credential and intake-only trust mode. Staff identity remains unchanged, waiting-room management is TenantAdmin-only, and canonical Patient/Clinical ownership is not transferred to patient endpoints.",
)
replace_once(
    "PROJECT_MAP.md",
    "Accepted PI-1 and PI-2A ownership:\n\n* tenant-owned portal accounts and invitations separate from staff identity\n* patient-only authentication/session and Angular boundary\n* tenant-owned `PatientIntake` current draft\n* separate fixed questionnaire answer rows using the existing 39-key catalog\n* immutable effective-save revisions with changed-field ids and versioned snapshot JSON\n* optional same-tenant Branch operational context\n* `Draft / Expired`, 30-day effective-save expiry and `RowVersion`\n\nPI-2A exposes no endpoint and never modifies canonical Patient or ClinicalRecord. PI-2B owns existing-patient self-service endpoints; PI-2C owns waiting-room links and intake-only scope; PI-2D owns capture UI.",
    "Accepted PI-1, PI-2A and PI-2B ownership:\n\n* tenant-owned portal accounts and existing-patient invitations separate from staff identity\n* patient-only authentication/session and Angular auth boundary\n* tenant-owned `PatientIntake` current draft\n* separate fixed questionnaire answer rows using the existing 39-key catalog\n* immutable effective-save revisions with changed-field ids and versioned snapshot JSON\n* optional same-tenant Branch operational context\n* `Draft / Expired`, 30-day effective-save expiry and `RowVersion`\n* id-less self-only create/get/save for linked patients with no-store and optimistic concurrency\n\nPI-2C owns a separate no-Patient waiting-room credential, `patient_intake` session and minimal staff handoff UI; PI-2D owns patient capture UI. No PI-2 endpoint applies canonical Patient or ClinicalRecord data.",
)
replace_once(
    "PROJECT_MAP.md",
    "* patient portal account/invitation -> tenant-owned identity/bootstrap records; patient linkage and login uniqueness remain tenant-scoped\n* patient intake/answer/revision -> tenant-owned proposal records; optional Branch is operational only and revisions are append-only",
    "* patient portal account/invitation -> tenant-owned identity/bootstrap records; patient linkage and login uniqueness remain tenant-scoped\n* waiting-room intake credential -> tenant-owned, optional same-tenant Branch context, no PatientId, hash-only and single-use\n* patient intake/answer/revision -> tenant-owned proposal records; optional Branch is operational only and revisions are append-only",
)

# Architecture and tenant model boundaries.
replace_once(
    "docs/architecture.md",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary; ADR 012–015 establish access and browser-session separation; ADR 016 establishes patient-proposed fields, typed-phone proposal ownership, fixed-question reuse, explicit-save revisions, 30-day expiry and the future waiting-room/intake-only scope. PI-2A implements only domain/persistence and no public endpoint.",
    "It must not reuse staff membership/permissions, enable platform override in patient policies, or expose accepted Clinical/Treatment/Billing/Documents aggregates directly. ADR 006 defines the boundary; ADR 012–015 establish linked-patient access and browser-session separation; ADR 016 establishes the intake model; ADR 017 accepts the linked-patient API and opens a separate waiting-room credential plus `patient_intake` policy. PI-2B never accepts ownership ids or applies canonical data, and PI-2C must keep staff management outside the patient bearer prefix.",
)
replace_once(
    "docs/tenant-model.md",
    "- patient portal accounts and invitations\n- patient intake drafts, fixed answers and immutable revisions",
    "- patient portal accounts and invitations\n- waiting-room intake credentials and append-only audit\n- patient intake drafts, fixed answers and immutable revisions",
)
replace_once(
    "docs/tenant-model.md",
    "Patient-facing identity and intake are separate from staff identity and canonical clinical ownership under ADR 006 and ADR 012–016.",
    "Patient-facing identity and intake are separate from staff identity and canonical clinical ownership under ADR 006 and ADR 012–017.",
)
replace_once(
    "docs/tenant-model.md",
    "- `PatientIntake`, its medical answers and revisions carry `TenantId`; optional `BranchId` remains operational and same-tenant\n- PI-2A provides one active `Draft` per account, soft `Expired`, `RowVersion` and append-only effective-save revisions without endpoints or canonical writes\n- PI-2B is the next gate; waiting-room `patient_intake` scope remains PI-2C",
    "- `PatientIntake`, its medical answers and revisions carry `TenantId`; optional `BranchId` remains operational and same-tenant\n- PI-2A provides one active `Draft` per account, soft `Expired`, `RowVersion` and append-only effective-save revisions\n- PI-2B exposes id-less self-only create/get/save for linked accounts; Tenant/Patient/intake come from the validated session\n- PI-2C waiting-room credentials carry `TenantId`, optional same-tenant Branch and no PatientId; management is TenantAdmin-only with no platform override\n- unlinked accounts use an explicit `patient_intake` scope with `intake_id` and no `patient_id`",
)

# Product roadmap.
replace_once(
    "docs/product-roadmap.md",
    "Active after formal MVP acceptance. PI-1 and PI-2A are completed; PI-2B is next.",
    "Active after formal MVP acceptance. PI-1, PI-2A and PI-2B are completed; PI-2C is active.",
)
replace_once(
    "docs/product-roadmap.md",
    "Active under ADR 006 and ADR 012–016. PI-1 (#4) is completed. PI-2 (#5) is active; PI-2A (#31) completed domain/persistence for tenant-owned drafts, fixed answers and immutable revisions. PI-2B is next; intake endpoints and Angular capture are not yet implemented.",
    "Active under ADR 006 and ADR 012–017. PI-1 (#4) is completed. PI-2 (#5) is active; PI-2A (#31) completed domain/persistence and PI-2B (#33 / PR #34) completed existing-patient self-only create/get/save. PI-2C (#35) is active; waiting-room bootstrap and patient Angular capture are not yet implemented.",
)
replace_once(
    "docs/product-roadmap.md",
    "   1. PI-2A domain/persistence — #31 — completed\n   2. PI-2B existing-patient self-service draft — next\n   3. PI-2C waiting-room link and intake-only scope — pending\n   4. PI-2D Angular intake capture/closure — pending",
    "   1. PI-2A domain/persistence — #31 — completed\n   2. PI-2B existing-patient self-service draft — #33 / PR #34 — completed\n   3. PI-2C waiting-room link and intake-only scope — #35 — active through #36 → #37 → #38\n   4. PI-2D Angular intake capture/closure — pending",
)
replace_once(
    "docs/product-roadmap.md",
    "- ADR 016: `docs/decisions/016-patient-intake-draft-baseline.md`\n- PI-1 closure",
    "- ADR 016: `docs/decisions/016-patient-intake-draft-baseline.md`\n- ADR 017: `docs/decisions/017-existing-patient-intake-api-and-waiting-room-bootstrap.md`\n- PI-1 closure",
)

# General implementation plan.
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** In progress; PI-1 and PI-2A completed; PI-2B next",
    "- **Status:** In progress; PI-1, PI-2A and PI-2B completed; PI-2C active",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Architecture decisions:** ADR 006 and ADR 012–016",
    "- **Architecture decisions:** ADR 006 and ADR 012–017",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: active; PI-1 and PI-2A are completed; PI-2B is next.",
    "- Phase 2.1: active; PI-1, PI-2A and PI-2B are completed; PI-2C is active.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-2 intake draft | Active; PI-2A domain/persistence completed; PI-2B next | Issue #5 / #31 / PR #32 |\n| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |\n| PI-4 audit/hardening | Planned; not implemented | Issue #7 |\n| Patient-facing backend/API/database | Access/auth through PI-1 plus intake persistence through PI-2A | PRs #26, #28, #29 and #32 |\n| Patient-facing frontend | Bounded activation/login/session implemented | PR #30 / ADR 015 |\n| Patient-facing intake | Domain/persistence only; no API/UI | PI-2A / PR #32 |",
    "| PI-2 intake draft | Active; PI-2A and PI-2B completed; PI-2C active | Issue #5 / #31 / #33 / #35 |\n| PI-3 submit/review/apply | Planned; not implemented | Issue #6 |\n| PI-4 audit/hardening | Planned; not implemented | Issue #7 |\n| Patient-facing backend/API/database | Access/auth, intake persistence and linked-patient self-only API implemented | PRs #26, #28, #29, #32 and #34 |\n| Patient-facing frontend | Bounded activation/login/session implemented | PR #30 / ADR 015 |\n| Patient-facing intake | Existing-patient API implemented; waiting-room bootstrap and patient capture UI pending | PI-2B / PI-2C / PI-2D |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "2. **PI-2B — Existing-Patient Self-Service Draft — next**\n   - explicit create/get/save from the linked authenticated account;\n   - no arbitrary intake/Patient selector and no GET side effects;\n   - concurrency conflict and current-draft expiry handling.\n3. **PI-2C — Waiting-Room Link and Intake-Only Account Scope — pending**\n   - 30-minute hash-only link, `patientportal.intake.manage`, unlinked account and `patient_intake`.\n4. **PI-2D — Angular Intake Capture and PI-2 Closure — pending**",
    "2. **PI-2B — Existing-Patient Self-Service Draft — #33 / PR #34 — completed**\n   - explicit id-less create/get/save from the linked authenticated account;\n   - GET has no side effects; all sensitive responses are no-store;\n   - no-op/effective-save, expiry and optimistic concurrency contracts.\n3. **PI-2C — Waiting-Room Link and Intake-Only Account Scope — #35 — active**\n   - PI-2C1 #36: credential/persistence and TenantAdmin management API;\n   - PI-2C2 #37: transactional activation and `patient_intake` session;\n   - PI-2C3 #38: staff generate/copy/print/local-QR UI and closure.\n4. **PI-2D — Angular Patient Intake Capture and PI-2 Closure — pending**",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Waiting-room link single-use/hash-only/30 minutes, future `patientportal.intake.manage` TenantAdmin-only and no platform override.\n- Future unlinked account uses `scope=patient_intake` with `intake_id`, no `patient_id`.",
    "- Waiting-room link single-use/hash-only/30 minutes; `patientportal.intake.manage` is TenantAdmin-only with no platform override.\n- PI-2C uses an unlinked account with `scope=patient_intake`, `intake_id` and no `patient_id`; QR generation remains local and the raw link stays memory-only.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "1. Preserve PI-1 / #4 and PI-2A / #31 as completed through PRs #26, #28, #29, #30 and #32.\n2. Open only PI-2B for existing-patient self-only create/get/save over the accepted aggregate.\n3. Keep waiting-room link, staff permission and `patient_intake` scope in PI-2C; keep Angular intake capture in PI-2D.\n4. Do not add canonical application to PI-2; keep PI-3 and PI-4 pending until their own gates.",
    "1. Preserve PI-1 / #4, PI-2A / #31 and PI-2B / #33 as completed through PRs #26, #28, #29, #30, #32 and #34.\n2. Implement only PI-2C1 / #36 next: waiting-room credential, append-only audit, `patientportal.intake.manage` and staff issue/list/revoke API.\n3. Keep anonymous consume and `patient_intake` session in PI-2C2 / #37; keep staff copy/print/local-QR UI in PI-2C3 / #38; keep patient capture in PI-2D.\n4. Do not add canonical application to PI-2; keep PI-3 and PI-4 pending until their own gates.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "**Decision:** Implement PI-1 under ADR 012–015, then implement PI-2 only in the ADR 016 sequence PI-2A → PI-2B → PI-2C → PI-2D.\n\n**Consequence:** PI-1 and PI-2A now provide secure access plus tenant-owned intake draft/answer/revision persistence. No intake endpoint or UI exists yet. PI-2B is next; PI-2C/PI-2D, review/apply and final hardening remain unimplemented.",
    "**Decision:** Implement PI-1 under ADR 012–015 and PI-2 under ADR 016/017. Accept PI-2B as the linked-patient self-only API and implement PI-2C only through #36 → #37 → #38 before PI-2D.\n\n**Consequence:** PI-1, PI-2A and PI-2B now provide secure linked-patient access and intake create/get/save without canonical writes. Waiting-room credential/session/staff handoff remain PI-2C; patient questionnaire capture remains PI-2D; review/apply and final hardening remain unimplemented.",
)

# ADR 006 and ADR 016 tracking/status.
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- **Tracking:** issue #2; PI-1 #4; PI-1A–PI-1D #22–#25; PI-2 #5; PI-2A #31; PI-3–PI-4 #6–#7",
    "- **Tracking:** issue #2; PI-1 #4; PI-1A–PI-1D #22–#25; PI-2 #5; PI-2A #31; PI-2B #33; PI-2C #35–#38; PI-3–PI-4 #6–#7",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "On 2026-07-25 the client approved ADR 016: exact proposal fields including `ReasonForVisit`, typed phones as intake proposals, 30-day effective-save expiry, explicit save without autosave, a future single-use 30-minute waiting-room link, `patientportal.intake.manage` for `TenantAdmin` only, unlinked `patient_intake` scope and PI-2A → PI-2B → PI-2C → PI-2D sequencing.",
    "On 2026-07-25 the client approved ADR 016: exact proposal fields including `ReasonForVisit`, typed phones as intake proposals, 30-day effective-save expiry, explicit save without autosave, a future single-use 30-minute waiting-room link, `patientportal.intake.manage` for `TenantAdmin` only, unlinked `patient_intake` scope and PI-2A → PI-2B → PI-2C → PI-2D sequencing.\n\nOn 2026-07-25 the client authorized closing PI-2B and opening PI-2C exclusively for the waiting-room credential, intake-only account/session and minimal staff generate/copy/print/local-QR UI. ADR 017 records the accepted boundary and #36 → #37 → #38 sequence.",
)
replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- PI-2A domain/persistence completed through PR #32 with migration `20260725182044_AddPatientIntakeDraftFoundation`.\n- Public patient auth API and bounded frontend exist; intake API/UI, PI-2B–PI-2D, PI-3 and PI-4 remain pending.",
    "- PI-2A domain/persistence completed through PR #32 with migration `20260725182044_AddPatientIntakeDraftFoundation`.\n- PI-2B existing-patient self-only API completed through PR #34 / `7325a73e7f86ae0e6f0557574fe9d9756a89293f`; CI #315 green.\n- PI-2C is active under ADR 017 through issues #35–#38; PI-2D, PI-3 and PI-4 remain pending.",
)
replace_once(
    "docs/decisions/016-patient-intake-draft-baseline.md",
    "- **Tracking:** epic #2; PI-2 #5; PI-2A #31; PR #32",
    "- **Tracking:** epic #2; PI-2 #5; PI-2A #31 / PR #32; PI-2B #33 / PR #34; PI-2C #35–#38; ADR 017",
)
replace_once(
    "docs/decisions/016-patient-intake-draft-baseline.md",
    "### PI-2 overall\n\nPI-2 closes only after PI-2A through PI-2D are accepted with code, tests, runbooks and aligned documentation. PI-3 remains responsible for submit, review and canonical application.",
    "### PI-2B\n\n- accepted through PR #34 / `7325a73e7f86ae0e6f0557574fe9d9756a89293f`;\n- linked-patient `POST / GET / PUT /api/patient-portal/intake`;\n- id-less ownership from validated patient session;\n- GET without side effects and no-store responses;\n- optimistic concurrency, no-op semantics and atomic effective-save revision;\n- no canonical write; CI #315 green.\n\n### PI-2C\n\n- active under ADR 017 and issue #35;\n- delivered only through PI-2C1 #36 → PI-2C2 #37 → PI-2C3 #38;\n- patient questionnaire UI remains PI-2D.\n\n### PI-2 overall\n\nPI-2 closes only after PI-2A through PI-2D are accepted with code, tests, runbooks and aligned documentation. PI-3 remains responsible for submit, review and canonical application.",
)

print("PI-2B closure and PI-2C opening documentation reconciled.")
