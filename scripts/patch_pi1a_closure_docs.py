from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace_once(relative_path: str, old: str, new: str) -> None:
    path = ROOT / relative_path
    text = path.read_text(encoding="utf-8")
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"Expected one match in {relative_path}, found {count}: {old[:100]!r}")
    path.write_text(text.replace(old, new, 1), encoding="utf-8")


replace_once(
    "STATE — BigSmile.md",
    "[Hecho] PI-1 — Access and Invitation Foundation está activa. El sub-slice actual es PI-1A — Account and Invitation Domain/Persistence, issue #22.\n\n[Hecho] PI-1A agrega únicamente entidades tenant-owned, invariantes, índices, filtros, write enforcement, concurrencia, migración y pruebas. No expone activación/login, no crea un JWT de paciente y no captura cuestionario/intake.\n\n[Hecho] PI-1B (#23), PI-1C (#24) y PI-1D (#25) permanecen pendientes y deben ejecutarse en ese orden. PI-2 a PI-4 continúan no iniciados.",
    "[Hecho] PI-1 — Access and Invitation Foundation está activa. PI-1A — Account and Invitation Domain/Persistence quedó completado mediante PR #26 y merge commit `43ddb2e008ce07b4798c21409e3fe58b4839668d`.\n\n[Hecho] PI-1A incorporó `PatientPortalAccount`, `PatientPortalInvitation`, tenant-scoped login uniqueness, one-patient linkage, hash-only invitation persistence, lockout/session metadata, rowversion, filtros/write enforcement, migración y pruebas. No expone activación/login, JWT de paciente ni captura de intake.\n\n[PENDIENTE POR DECISIÓN] PI-1B (#23) es el siguiente sub-slice, pero no se abre hasta aprobar el permiso staff que podrá emitir/revocar invitaciones. PI-1C (#24), PI-1D (#25) y PI-2 a PI-4 permanecen pendientes.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A en implementación; sin superficie pública todavía.",
    "**Estado** — [Hecho] fase abierta; PI-1 activa; PI-1A completado; PI-1B pendiente de decisión de autorización; sin superficie pública todavía.",
)
replace_once(
    "STATE — BigSmile.md",
    "1. Completar PI-1A (#22) con dominio/persistencia tenant-aware, migración, pruebas y CI verde, sin endpoints.\n\n2. Abrir PI-1B (#23) solo después del cierre de PI-1A para emisión/revocación staff con token hash-at-rest y auditoría.",
    "1. Preservar PI-1A (#22) como dominio/persistencia completado, sin endpoints ni auth pública.\n\n2. Resolver explícitamente la autorización de PI-1B (#23). Recomendación técnica: permiso dedicado `patientportal.invitation.manage`, asignado inicialmente solo a `TenantAdmin`, sin platform override ni reutilizar `patient.write`.\n\n3. Abrir PI-1B únicamente después de aprobar esa decisión, limitado a emisión/revocación staff, token hash-at-rest y auditoría.",
)
replace_once(
    "STATE — BigSmile.md",
    "3. Mantener PI-1C (#24) y PI-1D (#25) bloqueados hasta sus gates previos; no exponer auth pública antes de hashing versionado, anti-enumeración, rate limiting y concurrencia.\n\n4. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.\n\n5. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.",
    "4. Mantener PI-1C (#24) y PI-1D (#25) bloqueados hasta sus gates previos; no exponer auth pública antes de hashing versionado, anti-enumeración, rate limiting y concurrencia.\n\n5. Mantener PI-2, PI-3 y PI-4 no iniciados hasta el cierre formal de PI-1.\n\n6. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.",
)
replace_once(
    "STATE — BigSmile.md",
    "6. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n7. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n8. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n9. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
    "7. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.\n\n8. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.\n\n9. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.\n\n10. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.\n\n11. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-1.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 012; Phase 2.1 abierta y PI-1A activa.",
    "**Estado:** Nota canónica actualizada con ADR 006 a ADR 012; Phase 2.1 abierta, PI-1A completado y PI-1B pendiente de decisión de autorización.",
)
replace_once(
    "STATE — BigSmile.md",
    "**Consecuencias:** El repositorio incorpora primero dominio/persistencia tenant-aware sin superficie pública. Activación, JWT, endpoints, frontend e intake permanecen bloqueados por los gates de PI-1B a PI-2. Payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
    "**Consecuencias:** El repositorio ya tiene dominio/persistencia tenant-aware sin superficie pública. El siguiente paso es aprobar quién puede gestionar invitaciones antes de abrir PI-1B. Activación, JWT, frontend e intake permanecen bloqueados por los gates posteriores. Payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal siguen diferidos.",
)

replace_once(
    "README.md",
    "* **Current slice:** **PI-1A — Patient portal account and invitation domain/persistence**\n* **Public patient runtime:** not exposed; activation/login/frontend/intake remain pending",
    "* **Latest Phase 2.1 slice completed:** **PI-1A — Patient portal account and invitation domain/persistence**\n* **Next slice:** **PI-1B — Staff invitation lifecycle**, pending explicit staff-permission approval\n* **Public patient runtime:** not exposed; activation/login/frontend/intake remain pending",
)
replace_once(
    "README.md",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. Phase 2.1 is active only through PI-1A; public activation/login, intake, review/apply and audit hardening remain pending.",
    "The initial operational MVP is accepted, but Bigsmile is not feature-complete. PI-1A is completed; PI-1B is the next gated slice. Public activation/login, intake, review/apply and audit hardening remain pending.",
)
replace_once(
    "README.md",
    "* PI-1A introduces account/invitation domain and persistence only; it does not expose patient endpoints",
    "* PI-1A completed account/invitation domain and persistence only; it exposes no patient endpoint",
)

replace_once(
    "AGENTS.md",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active and PI-1A in implementation",
    "- Current phase: `Phase 2.1 — Patient Intake and Portal Foundation`; PI-1 active, PI-1A completed, PI-1B pending authorization decision",
)
replace_once(
    "AGENTS.md",
    "- PI-1 is active; PI-1A (#22) is the current sub-slice\n- PI-1B (#23), PI-1C (#24) and PI-1D (#25) remain sequentially gated",
    "- PI-1 is active; PI-1A (#22) is completed through PR #26\n- PI-1B (#23) is next but requires explicit staff-permission approval\n- PI-1C (#24) and PI-1D (#25) remain sequentially gated",
)
replace_once(
    "AGENTS.md",
    "# Immediate objective\nComplete `PI-1A — Patient portal account and invitation domain/persistence` without exposing patient authentication or intake before its security gates.",
    "# Immediate objective\nPreserve completed PI-1A and resolve the explicit authorization decision required before opening `PI-1B — Staff-issued patient portal invitation lifecycle`.",
)
replace_once(
    "AGENTS.md",
    "- enforce tenant ownership, tenant-scoped login uniqueness, one-patient linkage, hash-only invitation persistence and rowversion concurrency in PI-1A\n- do not add staff/public endpoints, patient JWTs, frontend auth or intake to PI-1A\n- move to PI-1B only after PI-1A migration/tests/docs/CI are accepted",
    "- preserve PI-1A tenant ownership, tenant-scoped login uniqueness, one-patient linkage, hash-only invitation persistence and rowversion concurrency\n- do not add activation/login, patient JWTs, frontend auth or intake to PI-1B\n- approve a dedicated invitation-management permission before PI-1B; do not reuse broad `patient.write` silently",
)

replace_once(
    "PROJECT_MAP.md",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**, opened through active PI-1A",
    "* **Current phase:** **Phase 2.1 — Patient Intake and Portal Foundation**; PI-1A completed, PI-1B next and permission-gated",
)
replace_once(
    "PROJECT_MAP.md",
    "Preserve Releases 1 through 7 while executing PI-1A as the first Phase 2.1 runtime slice:",
    "Preserve Releases 1 through 7 and completed PI-1A while preparing the permission-gated PI-1B slice:",
)
replace_once(
    "PROJECT_MAP.md",
    "* implement only tenant-owned patient portal account/invitation domain and persistence in PI-1A\n* keep staff/public endpoints, JWTs, frontend auth and intake outside PI-1A",
    "* preserve tenant-owned patient portal account/invitation domain and persistence completed in PI-1A\n* approve PI-1B staff authorization explicitly before adding invitation endpoints\n* keep public endpoints, JWTs, frontend auth and intake outside PI-1B",
)
replace_once(
    "PROJECT_MAP.md",
    "* move to PI-1B only after PI-1A is accepted with migration, tests, docs and CI",
    "* PI-1A is accepted with migration, tests, docs and CI; PI-1B is the next gated step",
)
replace_once(
    "PROJECT_MAP.md",
    "* Phase 2.1 Patient Intake and Portal Foundation under ADR 006/012 — active through PI-1A; later PI slices gated",
    "* Phase 2.1 Patient Intake and Portal Foundation under ADR 006/012 — PI-1A completed; PI-1B next and permission-gated",
)

replace_once(
    "docs/product-roadmap.md",
    "- **Phase 2 Expansion — Modern Operations** — active through Phase 2.1 / PI-1A",
    "- **Phase 2 Expansion — Modern Operations** — active; PI-1A completed, PI-1B next",
)
replace_once(
    "docs/product-roadmap.md",
    "### Status\nActive after formal MVP acceptance. Phase 2.1 is opened and PI-1A is the current implementation slice.",
    "### Status\nActive after formal MVP acceptance. PI-1A is completed; PI-1B is next and awaits the explicit staff-permission decision.",
)
replace_once(
    "docs/product-roadmap.md",
    "#### Status\nActive under ADR 006 and ADR 012. PI-1 is active; PI-1A (#22) is in implementation. No public patient runtime is accepted yet.",
    "#### Status\nActive under ADR 006 and ADR 012. PI-1A (#22) is completed; PI-1B (#23) is next but not opened. No public patient runtime is accepted yet.",
)
replace_once(
    "docs/product-roadmap.md",
    "   1. PI-1A domain/persistence — #22 — current\n   2. PI-1B staff invitation lifecycle — #23",
    "   1. PI-1A domain/persistence — #22 — completed\n   2. PI-1B staff invitation lifecycle — #23 — next, permission decision pending",
)

replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- **Status:** In progress; PI-1 active; PI-1A current",
    "- **Status:** In progress; PI-1 active; PI-1A completed; PI-1B next and permission-gated",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "- Phase 2.1: active; PI-1 opened; PI-1A domain/persistence is the current slice.",
    "- Phase 2.1: active; PI-1A domain/persistence completed; PI-1B is the next gated slice.",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "| PI-1 access/invitations | Active; PI-1A in implementation | Issues #4 and #22–#25 |",
    "| PI-1 access/invitations | Active; PI-1A completed; PI-1B next | Issues #4 and #22–#25 |",
)
replace_once(
    "docs/patient-intake-and-portal-plan.md",
    "1. Complete PI-1A / issue #22 with migration, tenant tests, ADR 012, canonical docs and CI.\n2. Do not add endpoints, JWTs, frontend auth or intake to PI-1A.\n3. After PI-1A acceptance, continue with PI-1B / issue #23 only.\n4. Keep PI-1C/#24 and PI-1D/#25 sequentially gated.\n5. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
    "1. Preserve PI-1A / issue #22 as completed through PR #26.\n2. Approve the PI-1B staff permission before implementation; recommendation: dedicated `patientportal.invitation.manage`, initially `TenantAdmin` only, no platform override.\n3. After approval, open only PI-1B / issue #23.\n4. Keep PI-1C/#24 and PI-1D/#25 sequentially gated.\n5. Keep PI-2 through PI-4 pending until formal PI-1 closure.",
)

replace_once(
    "docs/decisions/006-patient-intake-and-portal-foundation.md",
    "- PI-1: active; PI-1A domain/persistence in implementation.\n- PI-1B–PI-1D and PI-2–PI-4: not implemented.",
    "- PI-1: active; PI-1A domain/persistence completed through PR #26.\n- PI-1B is next and awaits its staff-authorization decision.\n- PI-1C–PI-1D and PI-2–PI-4: not implemented.",
)

replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "The initial operational MVP is accepted. **Phase 2.1 — Patient Intake and Portal Foundation** is active through PI-1A.",
    "The initial operational MVP is accepted. **Phase 2.1 — Patient Intake and Portal Foundation** is active; PI-1A is completed and PI-1B is next.",
)
replace_once(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "- PI-1A domain/persistence is active;\n- no public patient auth or intake UI is available yet;",
    "- PI-1A domain/persistence is completed;\n- PI-1B is permission-gated and not opened;\n- no public patient auth or intake UI is available yet;",
)

print("PI-1A closure documentation reconciled.")
