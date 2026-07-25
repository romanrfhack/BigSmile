from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def replace_once(path: str, old: str, new: str) -> None:
    file_path = ROOT / path
    text = file_path.read_text(encoding="utf-8")
    if old in text:
        if text.count(old) != 1:
            raise RuntimeError(f"Expected one occurrence in {path}: {old}")
        file_path.write_text(text.replace(old, new, 1), encoding="utf-8")
        return
    if new in text:
        return
    raise RuntimeError(f"Source text was not found in {path}: {old}")


replace_once(
    "STATE — BigSmile.md",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 a ADR 015 cierran el acceso, invitaciones, autenticación/sesión y frontera Angular; ADR 016 fija el baseline de datos y draft lifecycle; ADR 017 acepta la API self-only de paciente existente y abre el bootstrap de sala de espera. PI-1 queda completado. PI-2 está activa: PI-2A y PI-2B están completados; PI-2C está abierta mediante #35 y se implementa secuencialmente como #36 → #37 → #38.",
    "**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 a ADR 015 cierran el acceso, invitaciones, autenticación/sesión y frontera Angular; ADR 016 fija el baseline de datos y draft lifecycle; ADR 017 acepta la API self-only y la frontera de sala de espera. PI-1, PI-2A, PI-2B y PI-2C1 están completados. PI-2C continúa activa con PI-2C2 #37 como siguiente gate y PI-2C3 #38 después.",
)
replace_once(
    "STATE — BigSmile.md",
    "[Hecho] PI-2C — Waiting-Room Link and Intake-Only Scope (#35) queda abierta con autorización explícita y alcance limitado: credencial single-use/hash-only/30 minutos, permiso `patientportal.intake.manage` solo `TenantAdmin`, cuenta unlinked con `scope=patient_intake` y UI staff mínima para generar/copiar/imprimir/QR local. Se implementa únicamente mediante PI-2C1 #36 → PI-2C2 #37 → PI-2C3 #38. PI-2D permanece pendiente.",
    "[Hecho] PI-2C — Waiting-Room Link and Intake-Only Scope (#35) permanece activa y se implementa únicamente mediante PI-2C1 #36 → PI-2C2 #37 → PI-2C3 #38.\n\n[Hecho] PI-2C1 — Waiting-Room Access Link Foundation (#36) queda completado mediante PR #40: `PatientIntakeAccessLink` tenant-owned sin `PatientId`, Branch opcional same-tenant, token de 256 bits con SHA-256 hash-at-rest, TTL configurable de 30 minutos, estados terminales, bitácora append-only, permiso `patientportal.intake.manage` solo `TenantAdmin`, API staff `POST / GET / DELETE /api/patient-intake-links`, migración `20260725204625_AddPatientIntakeAccessLinkFoundation` y pruebas de aislamiento/concurrencia/modelo. No agrega consume público, cuenta unlinked, JWT `patient_intake` ni UI Angular. PI-2C2 #37 es el siguiente gate; PI-2C3 #38 y PI-2D permanecen pendientes.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] fase abierta; PI-1 completado mediante PI-1A a PI-1D; PI-2 activa bajo ADR 016/017; PI-2A y PI-2B completados. La API self-only para pacientes existentes existe; PI-2C está activa para bootstrap de sala de espera y PI-2D conserva la captura Angular del cuestionario.",
    "**Estado** — [Hecho] fase abierta; PI-1, PI-2A, PI-2B y PI-2C1 completados bajo ADR 016/017. La API self-only de pacientes existentes y la gestión staff de credenciales de sala de espera existen. PI-2C2 debe implementar consume transaccional y sesión `patient_intake`; PI-2C3 conserva la UI staff y PI-2D la captura Angular del paciente.",
)
replace_once(
    "STATE — BigSmile.md",
    "- PI-2C1 Credential/Staff API — issue #36.\n- PI-2C2 Intake-Only Session — issue #37.",
    "- PI-2C1 Credential/Staff API — issue #36 / PR #40 / `docs/pi-2c1-waiting-room-access-link-foundation.md`.\n- PI-2C2 Intake-Only Session — issue #37.",
)
replace_once(
    "STATE — BigSmile.md",
    "3. Implementar PI-2C solo mediante #36 → #37 → #38: credencial de 30 minutos, `patientportal.intake.manage`, scope `patient_intake` y UI staff mínima; PI-2D conserva la captura Angular del paciente.",
    "3. Preservar PI-2C1 como foundation completado y abrir únicamente PI-2C2 #37 para consume transaccional, cuenta unlinked y sesión `patient_intake`; PI-2C3 #38 conserva la UI staff y PI-2D la captura Angular del paciente.",
)

replace_once(
    "README.md",
    "Phase 2.1 is active with PI-1, PI-2A and PI-2B completed. ADR 016 accepts the intake baseline and ADR 017 accepts the existing-patient self-only API while opening the waiting-room bootstrap boundary. PI-2C is active; the questionnaire UI remains PI-2D.",
    "Phase 2.1 is active with PI-1, PI-2A, PI-2B and PI-2C1 completed. ADR 017 now has an implemented TenantAdmin-only waiting-room credential foundation; PI-2C2 is next for transactional consume and the isolated `patient_intake` session. The questionnaire UI remains PI-2D.",
)
replace_once(
    "README.md",
    "* **Current slice:** **PI-2C — Waiting-Room Link and Intake-Only Scope**\n* **Public patient runtime:** activation/login/session and linked-patient intake API available; waiting-room bootstrap and intake capture UI remain pending",
    "* **Latest Phase 2.1 sub-slice completed:** **PI-2C1 — Waiting-Room Access Link Foundation**\n* **Next slice:** **PI-2C2 — Transactional Waiting-Room Activation and `patient_intake` Session**\n* **Public patient runtime:** linked-patient auth/intake API available; staff can issue/revoke waiting-room credentials, but anonymous consume and intake capture UI remain pending",
)
replace_once(
    "README.md",
    "* PI-2C is active only for the one-time waiting-room credential, `patientportal.intake.manage`, unlinked `patient_intake` session and minimal staff copy/print/local-QR UI; PI-2D retains patient capture",
    "* PI-2C1 completed the one-time waiting-room credential, `patientportal.intake.manage`, staff issue/list/revoke API and additive migration; PI-2C2 retains unlinked `patient_intake` activation/session and PI-2C3 the copy/print/local-QR UI; PI-2D retains patient capture",
)

replace_once(
    "AGENTS.md",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1, PI-2A and PI-2B completed, PI-2C active",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1, PI-2A, PI-2B and PI-2C1 completed; PI-2C2 next",
)
replace_once(
    "AGENTS.md",
    "- PI-2C — issue #35; sub-slices #36, #37 and #38",
    "- PI-2C — issue #35; PI-2C1 #36 / PR #40 completed; PI-2C2 #37 next; PI-2C3 #38 pending",
)
replace_once(
    "AGENTS.md",
    "# Immediate objective\nPreserve completed PI-1, PI-2A and PI-2B, and implement only `PI-2C1 — Waiting-Room Credential Foundation and TenantAdmin Management API` before anonymous consume, intake-only JWT or Angular handoff UI.",
    "# Immediate objective\nPreserve completed PI-1, PI-2A, PI-2B and PI-2C1, and implement only `PI-2C2 — Transactional Waiting-Room Activation and patient_intake Session` before the staff handoff UI or patient questionnaire UI.",
)
replace_once(
    "AGENTS.md",
    "- implement PI-2C sequentially: #36 credential/staff API, #37 transactional activation/`patient_intake`, #38 staff copy/print/local-QR UI; keep patient Angular intake capture in PI-2D",
    "- preserve PI-2C1 hash-only credential, TenantAdmin-only permission, staff route outside `/api/patient-portal/*`, append-only audit and migration; implement #37 transactional activation/`patient_intake` next, then #38 staff copy/print/local-QR UI; keep patient Angular capture in PI-2D",
)

replace_once(
    "PROJECT_MAP.md",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1, PI-2A and PI-2B completed, PI-2C active",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1, PI-2A, PI-2B and PI-2C1 completed; PI-2C2 next",
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7 and completed PI-1/PI-2A/PI-2B while implementing only PI-2C:",
    "Preserve Releases 1 through 7 and completed PI-1/PI-2A/PI-2B/PI-2C1 while implementing only PI-2C2:",
)
replace_once(
    "PROJECT_MAP.md",
    "* implement PI-2C only as #36 credential/staff API → #37 intake-only session → #38 staff handoff UI; reserve patient capture for PI-2D",
    "* preserve completed #36 credential/staff API and implement only #37 intake-only activation/session next; #38 retains staff handoff UI and PI-2D patient capture",
)
replace_once(
    "PROJECT_MAP.md",
    "* waiting-room intake credential -> tenant-owned, optional same-tenant Branch context, no PatientId, hash-only and single-use",
    "* waiting-room intake credential -> tenant-owned, optional same-tenant Branch context, no PatientId, hash-only, 30-minute default, staff-managed through `patientportal.intake.manage` and prepared for single-use consume in PI-2C2",
)

replace_once(
    "docs/architecture.md",
    "ADR 017 accepts the linked-patient API and opens a separate waiting-room credential plus `patient_intake` policy. PI-2B never accepts ownership ids or applies canonical data, and PI-2C must keep staff management outside the patient bearer prefix.",
    "ADR 017 accepts the linked-patient API and separate waiting-room trust mode. PI-2C1 implements the tenant-owned hash-only credential and TenantAdmin staff API outside the patient bearer prefix. PI-2C2 must add transactional consume and a separate `patient_intake` policy without making `patient_id` optional in the existing patient identity.",
)
replace_once(
    "docs/tenant-model.md",
    "- PI-2C waiting-room credentials carry `TenantId`, optional same-tenant Branch and no PatientId; management is TenantAdmin-only with no platform override\n- unlinked accounts use an explicit `patient_intake` scope with `intake_id` and no `patient_id`",
    "- PI-2C1 waiting-room credentials carry `TenantId`, optional same-tenant Branch and no PatientId; token hashes are persisted, audit is append-only and management is TenantAdmin-only with no platform override\n- PI-2C2 unlinked accounts use an explicit `patient_intake` scope with `intake_id` and no `patient_id`; consume must be transactional and session-version validated",
)

replace_once(
    "docs/product-roadmap.md",
    "Active after formal MVP acceptance. PI-1, PI-2A and PI-2B are completed; PI-2C is active.",
    "Active after formal MVP acceptance. PI-1, PI-2A, PI-2B and PI-2C1 are completed; PI-2C2 is next.",
)
replace_once(
    "docs/product-roadmap.md",
    "Active under ADR 006 and ADR 012–017. PI-1 (#4) is completed. PI-2 (#5) is active; PI-2A (#31) completed domain/persistence and PI-2B (#33 / PR #34) completed existing-patient self-only create/get/save. PI-2C (#35) is active; waiting-room bootstrap and patient Angular capture are not yet implemented.",
    "Active under ADR 006 and ADR 012–017. PI-1 (#4), PI-2A (#31), PI-2B (#33 / PR #34) and PI-2C1 (#36 / PR #40) are completed. PI-2C2 (#37) is next for transactional waiting-room activation and `patient_intake`; staff handoff UI and patient capture are not yet implemented.",
)
replace_once(
    "docs/product-roadmap.md",
    "   3. PI-2C waiting-room link and intake-only scope — #35 — active through #36 → #37 → #38",
    "   3. PI-2C waiting-room link and intake-only scope — #35 — active: #36 completed, #37 next, #38 pending",
)

replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** In progress; PI-1, PI-2A and PI-2B completed; PI-2C active",
    "- **Status:** In progress; PI-1, PI-2A, PI-2B and PI-2C1 completed; PI-2C2 next",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: active; PI-1, PI-2A and PI-2B are completed; PI-2C is active.",
    "- Phase 2.1: active; PI-1, PI-2A, PI-2B and PI-2C1 are completed; PI-2C2 is next.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-2 intake draft | Active; PI-2A and PI-2B completed; PI-2C active | Issue #5 / #31 / #33 / #35 |",
    "| PI-2 intake draft | Active; PI-2A, PI-2B and PI-2C1 completed; PI-2C2 next | Issues #5 / #31 / #33 / #35–#38 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "3. **PI-2C — Waiting-Room Link and Intake-Only Account Scope — #35 — active**\n   - PI-2C1 #36: credential/persistence and TenantAdmin management API;\n   - PI-2C2 #37: transactional activation and `patient_intake` session;\n   - PI-2C3 #38: staff generate/copy/print/local-QR UI and closure.",
    "3. **PI-2C — Waiting-Room Link and Intake-Only Account Scope — #35 — active**\n   - PI-2C1 #36 / PR #40: credential/persistence and TenantAdmin management API — completed;\n   - PI-2C2 #37: transactional activation and `patient_intake` session — next;\n   - PI-2C3 #38: staff generate/copy/print/local-QR UI and closure — pending.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "2. Implement only PI-2C1 / #36 next: waiting-room credential, append-only audit, `patientportal.intake.manage` and staff issue/list/revoke API.\n3. Keep anonymous consume and `patient_intake` session in PI-2C2 / #37; keep staff copy/print/local-QR UI in PI-2C3 / #38; keep patient capture in PI-2D.",
    "2. Preserve PI-2C1 / #36 / PR #40 as completed: waiting-room credential, append-only audit, `patientportal.intake.manage`, migration and staff issue/list/revoke API.\n3. Implement only anonymous consume and `patient_intake` session in PI-2C2 / #37 next; keep staff copy/print/local-QR UI in PI-2C3 / #38 and patient capture in PI-2D.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "**Consequence:** PI-1, PI-2A and PI-2B now provide secure linked-patient access and intake create/get/save without canonical writes. Waiting-room credential/session/staff handoff remain PI-2C; patient questionnaire capture remains PI-2D; review/apply and final hardening remain unimplemented.",
    "**Consequence:** PI-1, PI-2A, PI-2B and PI-2C1 now provide secure linked-patient access, intake create/get/save and TenantAdmin-managed waiting-room credentials without canonical writes. Transactional consume and `patient_intake` remain PI-2C2; staff handoff UI remains PI-2C3; patient questionnaire capture remains PI-2D.",
)

replace_once(
    "docs/decisions/017-existing-patient-intake-api-and-waiting-room-bootstrap.md",
    "- PI-2C parent: issue #35 active.\n- PI-2C1: issue #36 active and next.\n- PI-2C2: issue #37 blocked by PI-2C1.\n- PI-2C3: issue #38 blocked by PI-2C1 and PI-2C2.\n- PI-2D, PI-3 and PI-4: not implemented.",
    "- PI-2C parent: issue #35 active.\n- PI-2C1: completed through issue #36 / PR #40 with migration `20260725204625_AddPatientIntakeAccessLinkFoundation`; closure evidence: `docs/pi-2c1-waiting-room-access-link-foundation.md`.\n- PI-2C2: issue #37 is the next gate.\n- PI-2C3: issue #38 remains blocked by PI-2C2.\n- PI-2D, PI-3 and PI-4: not implemented.",
)

print("PI-2C1 documentation reconciled.")
