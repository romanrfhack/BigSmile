from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace_once(path: Path, old: str, new: str, label: str) -> None:
    text = path.read_text(encoding="utf-8")
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected exactly one match, found {count}")
    path.write_text(text.replace(old, new, 1), encoding="utf-8")


# STATE — BigSmile.md
state = ROOT / "STATE — BigSmile.md"
replace_once(
    state,
    """**Release 6 — Billing** — [Hecho] ADR 009 acepta el cierre fundacional de Billing mediante Release 6.1 — Billing Document Foundation, preservando payments, balances, receipts, cash management y CFDI como scopes posteriores.
""",
    """**Release 6 — Billing** — [Hecho] ADR 009 acepta el cierre fundacional de Billing mediante Release 6.1 — Billing Document Foundation, preservando payments, balances, receipts, cash management y CFDI como scopes posteriores.

**Tenant Time Zone Foundation** — [Hecho] ADR 010 fija `Tenant.TimeZoneId` como fuente server-authoritative de la fecha operativa local, con default de migración `America/Mexico_City` para el piloto actual, sin convertir Branch en boundary temporal independiente ni reescribir Appointment.

**Release 7 — Documents and Dashboard** — [Hecho] ADR 011 acepta Release 7 mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation, y formaliza el cierre del MVP operativo inicial.
""",
    "STATE decisions",
)
replace_once(
    state,
    """[Hecho] Release 6 — Billing — completada como release fundacional mediante Release 6.1 — Billing Document Foundation.
""",
    """[Hecho] Release 6 — Billing — completada como release fundacional mediante Release 6.1 — Billing Document Foundation.

[Hecho] Release 7 — Documents and Dashboard — completada mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation.

[Hecho] El MVP operativo inicial queda formalmente aceptado con Releases 1 a 7 y la fundación de roles/permisos cerradas mediante evidencia de código, pruebas y documentación alineada.
""",
    "STATE completed phases",
)
replace_once(
    state,
    """[Hecho] El cierre formal de Release 6 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para `BillingDocument` explícito desde una cotización aceptada, líneas snapshot, moneda/totales preservados, lifecycle `Draft -> Issued` y documento emitido read-only.
""",
    """[Hecho] El cierre formal de Release 6 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para `BillingDocument` explícito desde una cotización aceptada, líneas snapshot, moneda/totales preservados, lifecycle `Draft -> Issued` y documento emitido read-only.

[Hecho] El cierre formal de Release 7 se apoya en la auditoría de Documents/Dashboard, el hardening de upload binario de PR #19, la fundación tenant-owned de zona horaria de PR #20, CI completas y ADR 010/011.
""",
    "STATE closure evidence sentence",
)
replace_once(
    state,
    """[Hecho] La última fase funcional marcada como completada es Release 6 — Billing.

[Hecho] Release 6 queda cerrada y preservada como release fundacional de documento comercial mediante Release 6.1 — Billing Document Foundation.

[Hecho] La siguiente fase funcional prevista por el roadmap es Release 7 — Documents and Dashboard.

[Hecho] Release 7 no queda abierta por el cierre de Release 6. Cualquier aceptación debe realizarse mediante auditoría/slices explícitos, con alcance, pruebas y documentación propios.

[Hecho] Phase 2 Expansion — Modern Operations pertenece al roadmap posterior al MVP operativo inicial. No debe abrirse antes de completar y aceptar Release 7, salvo repriorización futura explícita y documentada.

[Hecho] Phase 2.1 — Patient Intake and Portal Foundation permanece planificada dentro de Phase 2. Su arquitectura y plan general están aceptados, pero PI-1 a PI-4 no están implementados ni abiertos como fase activa.
""",
    """[Hecho] La última fase funcional marcada como completada es Release 7 — Documents and Dashboard.

[Hecho] Release 7 queda cerrada y preservada mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation.

[Hecho] El MVP operativo inicial queda formalmente aceptado. Esta aceptación cubre los boundaries fundacionales documentados y no implica payments, cash management, CFDI, doctor views, automatizaciones, advanced analytics ni full Patient Portal.

[Hecho] La siguiente fase prevista por el roadmap es Phase 2.1 — Patient Intake and Portal Foundation.

[Hecho] El gate normal de MVP para Phase 2.1 ya está satisfecho, pero la fase no se abre ni implementa automáticamente. Antes de PI-1 se requiere decisión explícita de apertura y resolver los choices de acceso/bootstrap registrados en issue #2.

[Hecho] PI-1 a PI-4 permanecen no implementados. Cuando Phase 2.1 se abra explícitamente, el primer slice será PI-1 — Access and Invitation Foundation, issue #4.
""",
    "STATE current phase",
)
replace_once(
    state,
    """[Hecho] El repositorio contiene código funcional en módulos posteriores al estado formal, incluyendo Documents, Dashboard y recordatorios/manual reminders.

[Hecho] La presencia de código, rutas, permisos, migrations o tests no implica por sí misma aceptación o cierre. Hasta una auditoría específica, esos módulos se clasifican como `implemented but not formally accepted/reconciled`.

[Hecho] Odontogram, Treatments/Quotes y Billing dejan esa clasificación porque recibieron auditorías específicas y cierres formales mediante ADR 007/008/009 y sus documentos de evidencia.
""",
    """[Hecho] El repositorio contiene código funcional posterior o lateral al MVP aceptado, incluyendo recordatorios/manual reminders.

[Hecho] La presencia de código, rutas, permisos, migrations o tests sigue sin implicar por sí misma aceptación de una fase futura. Cada capability posterior requiere auditoría, alcance y documentación explícitos.

[Hecho] Odontogram, Treatments/Quotes, Billing, Documents y Dashboard dejan la clasificación `implemented but not formally accepted/reconciled` porque recibieron auditorías específicas y cierres formales mediante ADR 007/008/009/011 y sus documentos de evidencia.
""",
    "STATE reconciliation note",
)
replace_once(
    state,
    """**Ubicación** — [Hecho] primer capability acotado de Phase 2 — Modern Operations después del MVP inicial; no desplaza Release 7 como siguiente fase funcional actual.
""",
    """**Ubicación** — [Hecho] siguiente fase prevista después del MVP aceptado; permanece planificada y no desplaza la necesidad de una apertura explícita antes de implementar PI-1.
""",
    "STATE phase 2 placement",
)
replace_once(
    state,
    """## 9. Releases previos cerrados
""",
    """## 9. Release 7 — Documents and Dashboard

**Estado operativo actual** — [Hecho] completada como la última release del MVP operativo inicial.

**Evidencia de cierre** — [Hecho]

- Release 7.1 — Patient Documents Foundation.
- Release 7.2 — Dashboard Read Model Foundation.
- Auditoría/cierre — `docs/release-7-documents-and-dashboard-audit-and-closure.md`.
- Decisión de release/MVP — ADR 011 `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`.
- Decisión temporal — ADR 010 `docs/decisions/010-tenant-time-zone-foundation.md`.
- Hardening Documents — PR #19 / CI #149.
- Tenant time zone y Dashboard local day — PR #20 / CI #151.

**Alcance cerrado de Documents** — [Hecho]

- `PatientDocument` tenant-owned y patient-owned.
- Upload/list/download/logical retire explícitos y autorizados.
- Storage privado, storage keys server-generated y root containment.
- PDF/JPEG/PNG con declared-type allowlist y matching binary signature.
- Límite de archivo 10 MB y multipart acotado.
- `document.read` / `document.write`, sin ampliar `TenantUser`.
- Flujos cross-tenant bloqueados y platform support explícito.
- UI Angular patient-scoped con loading/error/upload/list/download/retire.

**Alcance cerrado de Dashboard** — [Hecho]

- `GET /api/dashboard/summary` read-only y tenant-scoped.
- Active patients, tenant-local today/pending appointments, active documents, treatment plans, accepted quotes e issued Billing documents.
- `Tenant.TimeZoneId` server-authoritative para el día operativo; `GeneratedAtUtc` permanece UTC.
- `dashboard.read` bajo el mapeo conservador actual de `TenantAdmin`; sin acceso oculto de plataforma ni ampliación de `TenantUser`.
- UI Angular acotada con loading/error/empty/summary cards.

**Fuera del cierre** — [Hecho]

- OCR, rich preview, versioning, public sharing, generated PDFs, e-signatures y Patient Portal document access.
- Revenue/balance metrics, charts/trends, exports, branch/doctor dashboards, BI, real-time y AI recommendations.
- External antivirus provider, retention/physical-delete automation y platform dashboard impersonation.
- Payments, receipts, cash management y fiscalización.

## 10. Releases previos cerrados
""",
    "STATE Release 7 section",
)
replace_once(state, "## 10. Backlog inmediato", "## 11. Backlog inmediato", "STATE backlog heading")
replace_once(state, "## 11. Riesgos y temas a vigilar", "## 12. Riesgos y temas a vigilar", "STATE risks heading")
replace_once(state, "## 12. Criterios para no perder el rumbo", "## 13. Criterios para no perder el rumbo", "STATE criteria heading")
replace_once(state, "## 13. Nota tipo ADR resumida", "## 14. Nota tipo ADR resumida", "STATE ADR heading")
replace_once(
    state,
    """1. Preservar Releases 1 a 6 ya cerrados sin debilitar la fundación tenant-aware.

2. Auditar el código existente de `Release 7 — Documents and Dashboard` contra el roadmap y aceptar únicamente slices con evidencia explícita.

3. No tratar Documents o Dashboard como aceptados solo porque existen código, endpoints, permissions, storage/read models o tests.

4. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera de Release 6.1 hasta slices dedicados.

5. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.

6. Mantener fuera de los agregados cerrados cualquier linkage cross-module no aceptado.

7. Si se consultan directamente tablas hijas de Clinical, Odontogram, TreatmentPlan, TreatmentQuote o BillingDocument, usar join tenant-aware o modelarlas como tenant-owned.

8. Mantener Phase 2.1 visible mediante ADR 006, su plan e issues #2/#4/#5/#6/#7, sin iniciar PI-1 antes del gate de MVP o repriorización explícita.

9. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS y product-roadmap cuando cambie el estado de Release 7 o Phase 2.1.
""",
    """1. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.

2. Tratar Phase 2.1 — Patient Intake and Portal Foundation como la siguiente fase prevista, no como implementación ya abierta.

3. Resolver explícitamente en issue #2 el identificador de acceso, password vs magic link, TTL, entrega piloto y baseline de lockout/recovery antes de abrir PI-1.

4. Cuando Phase 2.1 se abra, iniciar únicamente PI-1 — Access and Invitation Foundation, issue #4, y actualizar STATE en el mismo PR.

5. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.

6. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.

7. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.

8. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.

9. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap y ADRs cuando se abra Phase 2.1 o cambie el estado del producto.
""",
    "STATE backlog",
)
replace_once(
    state,
    """**Query filters y acceso a datos** — [Hecho] No degradar filtros globales y write enforcement con filtros manuales dispersos.
""",
    """**Query filters y acceso a datos** — [Hecho] No degradar filtros globales y write enforcement con filtros manuales dispersos.

**Tenant operational time** — [Hecho] `Tenant.TimeZoneId` es la fuente server-side del día operativo; no confiar en fecha/timezone del browser ni introducir una timezone global que rompa el modelo multi-tenant.

**Binary document input** — [Hecho] La allowlist de documentos requiere matching binary signature, límites de transporte y storage containment; no debe presentarse como antivirus o malware scanning.
""",
    "STATE risk additions",
)
replace_once(
    state,
    """[Hecho] El orden actual es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Después del MVP, Phase 2.1 cubre el bounded Patient Intake and Portal Foundation; el portal amplio permanece en Phase 4.
""",
    """[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. La siguiente fase prevista es Phase 2.1 Patient Intake and Portal Foundation; el portal amplio permanece en Phase 4.
""",
    "STATE roadmap criterion",
)
replace_once(
    state,
    """**Estado:** Nota canónica actualizada con ADR 006, ADR 007, ADR 008 y ADR 009.

**Contexto:** El código de Billing estaba implementado pero correctamente clasificado como no reconciliado hasta una auditoría específica.

**Decisión:** Cerrar Release 6 mediante Release 6.1 — Billing Document Foundation, preservar el documento emitido como snapshot separado de TreatmentQuote y de futuros Payment/Receipt/Cash/Fiscal aggregates, y mover la frontera del roadmap a Release 7. Mantener Patient Intake and Portal como Phase 2.1 posterior al MVP.

**Consecuencias:** Documents and Dashboard es el siguiente módulo a auditar; no se acepta automáticamente. Payments, balances, receipts, cash management y CFDI permanecen diferidos, y la deuda visual/hardening se atiende mediante slices separados.
""",
    """**Estado:** Nota canónica actualizada con ADR 006, ADR 007, ADR 008, ADR 009, ADR 010 y ADR 011.

**Contexto:** Documents y Dashboard tenían implementación coherente, pero la auditoría detectó una allowlist de upload spoofable y una fecha `Today` basada en UTC. Ambos gaps se corrigieron mediante PR #19 y PR #20 con CI verde.

**Decisión:** Cerrar Release 7 mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation; aceptar el MVP operativo inicial; preservar Documents como attachment foundation privada y Dashboard como read model tenant-scoped; mover la siguiente fase prevista a Phase 2.1 sin abrir PI-1 automáticamente.

**Consecuencias:** El MVP queda listo para validación piloto bajo su scope acotado. Phase 2.1 requiere una decisión explícita de apertura y resolver los choices de issue #2; payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal permanecen diferidos.
""",
    "STATE ADR note",
)

# README.md
readme = ROOT / "README.md"
replace_once(readme, "- Billing / cash management", "- Billing document issuance", "README initial Billing scope")
replace_once(
    readme,
    """Release 4 — Odontogram, Release 5 — Treatments and Quotes, and Release 6 — Billing are now formally accepted after module-specific audits of their domain, API, persistence, permissions, frontend and automated tests.

The repository still contains functional code in modules later than the formal roadmap frontier, including Documents, Dashboard and reminders/manual reminders. Code, routes, permissions, storage/read models or tests in those modules do not by themselves mean Release 7 or Phase 2 are open, accepted or closed.

Until each later module receives its own audit and acceptance pass, it remains `implemented but not formally accepted/reconciled`. Visual slices may improve presentation, copy, color, microinteractions, modals, drawers, tabs, sticky action bars and UX debt without changing backend behavior, APIs, permissions, auth, tenant context, branch context, migrations or functional scope.
""",
    """Releases 4 — Odontogram, 5 — Treatments and Quotes, 6 — Billing, and 7 — Documents and Dashboard are formally accepted after module-specific audits of domain, application/API, persistence, permissions, frontend and automated tests.

Release 7 closure also formally accepts the initial operational MVP. This is a bounded product milestone: issued Billing documents do not imply payments/cash/CFDI, Documents do not imply OCR/sharing, and Dashboard does not imply advanced analytics.

Code in later capabilities such as reminders/manual reminders, providers, jobs, online booking, Phase 2 patient intake or advanced analytics does not imply acceptance. Visual slices may improve presentation and UX debt without changing backend behavior, APIs, permissions, auth, tenant context, branch context, migrations or functional scope.
""",
    "README UX reconciliation",
)
replace_once(
    readme,
    """* **Release 6 — Billing**

Current roadmap position:

* **Latest completed delivery phase:** **Release 6 — Billing**
* **Next planned functional phase:** **Release 7 — Documents and Dashboard**
* **Phase 2.1 — Patient Intake and Portal Foundation:** planned after the initial MVP; architecture accepted in ADR 006; implementation not opened
""",
    """* **Release 6 — Billing**
* **Release 7 — Documents and Dashboard**

Current roadmap position:

* **Latest completed delivery phase:** **Release 7 — Documents and Dashboard**
* **Initial operational MVP:** **formally accepted**
* **Next planned phase:** **Phase 2.1 — Patient Intake and Portal Foundation**
* **Phase 2.1 runtime status:** architecture accepted in ADR 006; PI-1 to PI-4 not implemented or automatically opened
""",
    "README current roadmap",
)
replace_once(
    readme,
    """Release 6 closure evidence:

* `docs/release-6-billing-audit-and-closure.md`
* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

The current authorization foundation includes scope-aware JWT claims, explicit permission policies, policy-gated platform override, centralized tenant read/write enforcement in EF Core, `/api/auth/me`, and frontend session state in memory.

The repository remains established but not functionally complete. Documents/Dashboard and Phase 2 work must not be assumed accepted until code and documentation explicitly prove it.
""",
    """Release 6 closure evidence:

* `docs/release-6-billing-audit-and-closure.md`
* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

Release 7 is formally complete through:

* **Release 7.1 — Patient Documents Foundation**
* **Release 7.2 — Dashboard Read Model Foundation**

The accepted Documents boundary includes tenant/patient-owned metadata, explicit authorized upload/list/download/logical retire, private root-contained storage, PDF/JPEG/PNG type-plus-signature validation, bounded upload size and tenant/cross-tenant protection.

The accepted Dashboard boundary is read-only and tenant-scoped, with active patients, tenant-local today/pending appointments, active documents, treatment plans, accepted quotes and issued Billing documents. ADR 010 makes `Tenant.TimeZoneId` the server-authoritative source of the clinic operational day.

Release 7 closure evidence:

* `docs/release-7-documents-and-dashboard-audit-and-closure.md`
* ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`
* ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`

The current authorization foundation includes scope-aware JWT claims, explicit permission policies, policy-gated platform override, centralized tenant read/write enforcement in EF Core, `/api/auth/me`, and frontend session state in memory.

The initial operational MVP is accepted, but Bigsmile is not feature-complete. Payments/cash/CFDI, provider views, automated messaging, online booking, Phase 2.1 implementation, advanced analytics and the full Patient Portal remain future bounded work.
""",
    "README Release 7 evidence",
)
replace_once(
    readme,
    """### Release 7 — Documents and Dashboard

* Next planned functional phase after Release 6 closure
* Existing code remains `implemented but not formally accepted/reconciled` until module-specific audits
* Tenant-owned/patient-owned documents with authorized upload/download/retire
* Basic dashboard remains read-model oriented
* OCR, external sharing, generated PDFs, advanced analytics and reporting remain deferred

### Phase 2 Expansion — Modern Operations

* Later roadmap phase after the MVP is stable
* **Phase 2.1 — Patient Intake and Portal Foundation** is planned and architecturally accepted in ADR 006, but PI-1 to PI-4 are not implemented
""",
    """### Release 7 — Documents and Dashboard

* Completed through Release 7.1 — Patient Documents Foundation and Release 7.2 — Dashboard Read Model Foundation
* Private tenant/patient-owned documents with explicit upload/list/download/logical retire
* PDF/JPEG/PNG declared-type plus binary-signature validation, 10 MB file limit and root-contained storage
* Read-only tenant-scoped Dashboard with tenant-local operational day from `Tenant.TimeZoneId`
* OCR, public sharing, generated PDFs, payments/revenue metrics, charts, exports and branch/doctor dashboards remain deferred

### Phase 2 Expansion — Modern Operations

* Next planned phase after formal MVP acceptance; not automatically opened
* **Phase 2.1 — Patient Intake and Portal Foundation** is architecturally accepted in ADR 006, but PI-1 to PI-4 are not implemented
""",
    "README roadmap Release 7",
)

# AGENTS.md
agents = ROOT / "AGENTS.md"
replace_once(
    agents,
    """- `Release 6 — Billing`: completed through accepted Release 6.1 — Billing Document Foundation
- Next planned functional phase: `Release 7 — Documents and Dashboard`
""",
    """- `Release 6 — Billing`: completed through accepted Release 6.1 — Billing Document Foundation
- `Release 7 — Documents and Dashboard`: completed through accepted Release 7.1 and 7.2
- Initial operational MVP: formally accepted
- Next planned phase: `Phase 2.1 — Patient Intake and Portal Foundation`; implementation not opened
""",
    "AGENTS status",
)
replace_once(
    agents,
    """Release 6 closure evidence:
- `docs/release-6-billing-audit-and-closure.md`
- ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`
""",
    """Release 6 closure evidence:
- `docs/release-6-billing-audit-and-closure.md`
- ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

Release 7 / MVP closure evidence:
- `docs/release-7-documents-and-dashboard-audit-and-closure.md`
- ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`
- ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`
""",
    "AGENTS closure evidence",
)
replace_once(
    agents,
    """Do not reopen advanced Billing scope incidentally. Payments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency, accounting and Patient Portal access remain future bounded work. Payment must be designed as a separate aggregate rather than mutable fields on `BillingDocument`.

Repository code also exists in later modules, including Documents, Dashboard and reminders. Until each module receives a specific audit and acceptance pass, classify it as `implemented but not formally accepted/reconciled`.

Phase 2.1 — Patient Intake and Portal Foundation is planned after the initial MVP:
- architecture accepted in ADR 006
- implementation issues #4 to #7 remain open
- PI-1 to PI-4 are not implemented or active
- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability
""",
    """Do not reopen advanced Billing scope incidentally. Payments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency, accounting and Patient Portal access remain future bounded work. Payment must be designed as a separate aggregate rather than mutable fields on `BillingDocument`.

Treat Release 7 as the accepted bounded Documents/Dashboard boundary:
- tenant-owned/patient-owned PatientDocument metadata
- explicit authorized upload/list/download/logical retire
- private root-contained storage and server-generated storage keys
- PDF/JPEG/PNG declared-type plus binary-signature validation and bounded upload size
- read-only tenant-scoped Dashboard over accepted aggregate roots
- tenant-local operational day from server-authoritative `Tenant.TimeZoneId`
- explicit `document.*` and `dashboard.read` permissions under the current conservative mapping

Do not reopen advanced Release 7 scope incidentally. OCR, sharing, generated PDFs, antivirus providers, retention automation, revenue/balance metrics, charts, exports, branch/doctor dashboards, BI, real-time and Patient Portal access remain future bounded work.

Repository code also exists in later capabilities, including reminders/manual reminders. Code presence alone does not accept providers, jobs, online booking, Phase 2 or advanced analytics.

Phase 2.1 — Patient Intake and Portal Foundation is the next planned phase after the accepted MVP:
- architecture accepted in ADR 006
- implementation issues #4 to #7 remain open
- PI-1 to PI-4 are not implemented or active
- opening PI-1 requires an explicit phase-opening decision and resolution of the pending access/bootstrap choices in issue #2
- full patient portal remains deferred beyond the bounded Phase 2.1 intake/update capability
""",
    "AGENTS Release 7 boundary",
)
replace_once(
    agents,
    """# Immediate objective
Preserve Releases 1 to 6 and audit the existing `Release 7 — Documents and Dashboard` implementation against the bounded roadmap before accepting or changing it.

Immediate priorities:
- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`
- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes and Billing behavior
- audit Documents and Dashboard domain/application/API/persistence/storage/read models, permissions, frontend, migrations and tests
- distinguish code presence from accepted Release 7 scope
- avoid reopening Billing, TreatmentPlan, TreatmentQuote, Odontogram or Clinical Records through incidental linkage
- keep doctor-based views deferred until provider/doctor assignment is intentionally opened
- keep Phase 2.1 planned and inactive until the MVP gate or explicit reprioritization
- keep privileged/platform paths explicit and auditable
- maintain automated coverage for cross-tenant, branch-aware and allowed platform scenarios
- update canonical documentation whenever a release or architectural decision changes
""",
    """# Immediate objective
Preserve Releases 1 to 7 and the accepted MVP while preparing the explicit opening decision for `Phase 2.1 — Patient Intake and Portal Foundation`.

Immediate priorities:
- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`
- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard behavior
- keep Documents upload validation, storage containment and tenant-local Dashboard dates intact
- resolve issue #2 choices before opening PI-1: patient identifier, password vs magic link, TTL, pilot delivery and lockout/recovery baseline
- when explicitly opened, start only with PI-1 / issue #4 and update canonical state in the same PR
- avoid reopening accepted aggregates through incidental Patient Portal linkage
- keep doctor-based views deferred until provider/doctor assignment is intentionally opened
- keep privileged/platform paths explicit and auditable; patient-facing policies must have no platform override
- maintain automated coverage for cross-tenant, IDOR, replay, expiry, revocation, concurrency and existing staff regression scenarios
- update canonical documentation whenever the phase is opened or an architectural decision changes
""",
    "AGENTS immediate objective",
)

# PROJECT_MAP.md
project_map = ROOT / "PROJECT_MAP.md"
replace_once(
    project_map,
    """* **Release 6 — Billing:** completed through **Release 6.1 — Billing Document Foundation**
* **Next planned functional phase:** **Release 7 — Documents and Dashboard**
""",
    """* **Release 6 — Billing:** completed through **Release 6.1 — Billing Document Foundation**
* **Release 7 — Documents and Dashboard:** completed through **Release 7.1 — Patient Documents Foundation** and **Release 7.2 — Dashboard Read Model Foundation**
* **Initial operational MVP:** formally accepted
* **Next planned phase:** **Phase 2.1 — Patient Intake and Portal Foundation**, not yet opened
""",
    "PROJECT_MAP status",
)
replace_once(
    project_map,
    """### Release 6 closure evidence

* `docs/release-6-billing-audit-and-closure.md`
* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`
""",
    """### Release 6 closure evidence

* `docs/release-6-billing-audit-and-closure.md`
* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

### Release 7 / MVP closure evidence

* `docs/release-7-documents-and-dashboard-audit-and-closure.md`
* ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`
* ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`
""",
    "PROJECT_MAP evidence",
)
replace_once(
    project_map,
    """Odontogram, Treatments/Quotes and Billing are now `Accepted / preserved` after module-specific audits of domain, application, API, persistence, permissions, frontend, migrations and tests.

The repository still contains functional code in later roadmap modules, including Documents, Dashboard, and reminders/manual reminders. Code, routes, permissions, storage/read models, or tests in those modules do not by themselves open, accept, or close Release 7 or Phase 2.

Until a module-specific audit and acceptance pass happens, those later modules are `implemented but not formally accepted/reconciled`.
""",
    """Odontogram, Treatments/Quotes, Billing, Documents and Dashboard are `Accepted / preserved` after module-specific audits of domain/application, API, persistence/storage/read models, permissions, frontend, migrations and tests.

The initial operational MVP is formally accepted. The repository still contains later capabilities such as reminders/manual reminders, but code presence does not open or accept providers, jobs, online booking, Phase 2 or advanced analytics.

Every later capability still requires an explicit bounded audit and acceptance pass.
""",
    "PROJECT_MAP reconciliation",
)
replace_once(
    project_map,
    """### Current expected priority

Preserve Releases 1 through 6 and audit the existing Documents/Dashboard implementation before accepting or adding functionality:

* preserve the accepted Release 3.1 to 3.6 clinical boundary
* preserve the accepted Release 4.1 to 4.4 odontogram boundary
* preserve the accepted Release 5.1 and 5.2 treatment-plan/quote boundary
* preserve the accepted Release 6.1 BillingDocument boundary
* keep advanced Odontogram and Treatments/Quotes scope deferred
* keep payments, balances, receipts, cash management, fiscal/CFDI and automatic quote mutation outside Release 6.1
* inspect Documents and Dashboard ownership, API/storage/read models, permissions, migrations, frontend and tests against Release 7
* do not treat existing Documents/Dashboard code as accepted solely because it exists
* keep automated messaging/providers/jobs/queues/retries, online booking and advanced dashboards deferred until their owning phases are explicitly accepted
* keep doctor-based views deferred until provider/doctor assignment is intentionally opened
* preserve scope-aware authorization, explicit platform override behavior and centralized tenant safety
* continue validation through CI, tests, logging, auditing and architectural guardrails
* keep Phase 2.1 Patient Intake and Portal planned under ADR 006 without opening PI-1 to PI-4 before the MVP gate or an explicit reprioritization
""",
    """### Current expected priority

Preserve Releases 1 through 7 and prepare the explicit Phase 2.1 opening decision:

* preserve the accepted Clinical, Odontogram, Treatments/Quotes, Billing, Documents and Dashboard boundaries
* keep payments, balances, receipts, cash management, fiscal/CFDI and automatic quote mutation outside Release 6.1
* keep OCR/sharing/versioning and advanced Dashboard analytics outside Release 7
* preserve server-side document signature validation, storage containment and tenant-local Dashboard day boundaries
* resolve the patient access/bootstrap choices tracked in issue #2 before starting PI-1
* start only PI-1 / issue #4 after an explicit Phase 2.1 opening and update STATE in the same PR
* keep automated messaging/providers/jobs/queues/retries, online booking and full Patient Portal deferred
* keep doctor-based views deferred until provider/doctor assignment is intentionally opened
* preserve scope-aware authorization, explicit platform override behavior and centralized tenant safety
* continue validation through CI, tests, logging, auditing and architectural guardrails
""",
    "PROJECT_MAP priority",
)
replace_once(
    project_map,
    """* `docs/release-5-treatments-and-quotes-audit-and-closure.md`
* `docs/patient-intake-and-portal-plan.md`
""",
    """* `docs/release-5-treatments-and-quotes-audit-and-closure.md`
* `docs/release-6-billing-audit-and-closure.md`
* `docs/release-7-documents-and-dashboard-audit-and-closure.md`
* `docs/decisions/010-tenant-time-zone-foundation.md`
* `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`
* `docs/patient-intake-and-portal-plan.md`
""",
    "PROJECT_MAP docs map",
)
replace_once(
    project_map,
    """* tenant settings
* platform administration
""",
    """* tenant settings, including server-authoritative `TimeZoneId`
* platform administration
""",
    "PROJECT_MAP Platform ownership",
)
replace_once(
    project_map,
    """### 7.9 Documents

Owns:

* file attachments
* patient documents
* radiographies
* consent-related file storage

Existing code remains unaccepted until Release 7 audit.
""",
    """### 7.9 Documents

Accepted Release 7.1 ownership:

* tenant-owned/patient-owned `PatientDocument` metadata
* explicit authorized upload/list/download/logical retire
* private root-contained binary storage
* server-generated storage keys
* PDF/JPEG/PNG declared-type and binary-signature validation
* bounded upload limits and UTC/actor metadata
* patient-context Angular Documents workflow

OCR, rich preview, versioning, public sharing, generated PDFs, advanced consent/e-signature, antivirus providers, retention automation and Patient Portal access remain deferred.
""",
    "PROJECT_MAP Documents",
)
replace_once(
    project_map,
    """### 7.11 Reporting

Owns:

* dashboards
* operational metrics
* treatment metrics
* scheduling metrics
* billing summaries
""",
    """### 7.11 Reporting

Accepted Release 7.2 Dashboard ownership:

* tenant-scoped read-only operational summary
* active patients
* tenant-local today/pending appointments
* active documents
* treatment-plan and accepted-quote counts
* issued Billing-document count
* generated-at UTC
* Angular summary-card workflow

Future Reporting owns deeper treatment/scheduling/billing metrics, charts, exports and analytics. Revenue/balance metrics, branch/doctor dashboards, BI, real-time and AI recommendations remain deferred.
""",
    "PROJECT_MAP Reporting",
)
replace_once(
    project_map,
    """* billing record -> tenant, patient-owned and quote-linked where the accepted Billing model requires it
""",
    """* billing record -> tenant, patient-owned and quote-linked where the accepted Billing model requires it
* patient document -> tenant + patient; binary access follows authorized metadata lookup
* dashboard summary -> tenant-scoped read model; operational day derives from tenant-owned `TimeZoneId`
""",
    "PROJECT_MAP examples",
)
replace_once(
    project_map,
    """* new Billing rule -> Billing domain/application after the Release 6 audit identifies an accepted gap
* new payment persistence adapter -> Infrastructure
""",
    """* new Billing-document rule -> Billing domain/application only when it preserves the accepted Release 6.1 boundary
* new Payment/Receipt/Cash behavior -> a separate future aggregate/slice, not incidental BillingDocument fields
* new Documents rule -> Documents domain/application/storage only within an explicit accepted or future slice
* new Dashboard metric -> Reporting read model over accepted tenant-owned roots with explicit metric semantics
* new payment persistence adapter -> Infrastructure after its owning aggregate is accepted
""",
    "PROJECT_MAP new code",
)
replace_once(project_map, "* accepted Release 1 through Release 6 boundaries", "* accepted Release 1 through Release 7 boundaries and MVP scope", "PROJECT_MAP stable releases")
replace_once(project_map, "* Release 7+ formal scope acceptance", "* Phase 2.1+ formal scope acceptance", "PROJECT_MAP evolving releases")
replace_once(
    project_map,
    """### Release 7

Documents and Dashboard — next; existing code must be audited before acceptance.

### Later phases

* Phase 2.1 Patient Intake and Portal Foundation under ADR 006
""",
    """### Release 7

Documents and Dashboard — completed through Release 7.1 and 7.2; initial operational MVP accepted.

### Later phases

* Phase 2.1 Patient Intake and Portal Foundation under ADR 006 — next planned, not opened
""",
    "PROJECT_MAP release map",
)

# docs/product-roadmap.md
roadmap = ROOT / "docs/product-roadmap.md"
replace_once(
    roadmap,
    """- **Release 7 — Documents and Dashboard** — next planned functional phase
- **Phase 2 Expansion — Modern Operations**
  - **Phase 2.1 — Patient Intake and Portal Foundation**
""",
    """- **Release 7 — Documents and Dashboard** — completed; initial operational MVP accepted
- **Phase 2 Expansion — Modern Operations** — next planned phase, not opened
  - **Phase 2.1 — Patient Intake and Portal Foundation**
""",
    "roadmap overview",
)
replace_once(
    roadmap,
    """## 11. Release 7 — Documents and Dashboard

### Status
Next planned functional phase; not formally opened or accepted.

### Documents planned scope
- tenant-owned/patient-owned document records
- explicit authorized upload
- private storage
- allowlisted PDF/JPG/PNG
- size limits
- active list
- authorized download
- logical retire

### Dashboard planned scope
- tenant-scoped read model
- active patients
- today/pending appointments
- active documents
- active treatment plans
- accepted quotes
- issued Billing documents from accepted Release 6.1

### Deferred
- OCR/rich preview/versioning
- public/external sharing
- generated PDFs/templates
- advanced analytics/charts/exports
- branch/doctor dashboards
- BI-level reporting

Existing Documents/Dashboard code requires module-specific audit before acceptance.
""",
    """## 11. Release 7 — Documents and Dashboard

### Status
Completed through Release 7.1 and Release 7.2. This closure formally accepts the initial operational MVP.

### Closure evidence
- Release 7.1 — Patient Documents Foundation
- Release 7.2 — Dashboard Read Model Foundation
- Audit: `docs/release-7-documents-and-dashboard-audit-and-closure.md`
- ADR 010: `docs/decisions/010-tenant-time-zone-foundation.md`
- ADR 011: `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`

### Release 7.1 accepted scope
- tenant-owned/patient-owned `PatientDocument`
- explicit authorized upload/list/download/logical retire
- private storage outside public web roots
- server-generated root-contained storage keys
- PDF/JPEG/PNG declared-type plus binary-signature validation
- 10 MB authoritative file limit and bounded multipart overhead
- UTC/actor metadata
- `document.read` / `document.write`
- bounded Angular patient-context Documents workflow

### Release 7.2 accepted scope
- read-only tenant-scoped Dashboard summary
- active patients
- tenant-local today/pending appointments
- active documents
- existing treatment plans
- accepted quotes
- issued Billing documents from accepted Release 6.1
- `Tenant.TimeZoneId` as server-authoritative operational-day source
- `GeneratedAtUtc` preserved in UTC
- `dashboard.read` and bounded Angular summary cards

### Deferred
- OCR/rich preview/versioning and public/external sharing
- generated PDFs/templates, advanced consent/e-signature and antivirus providers
- retention/physical-delete automation and Patient Portal document access
- revenue/payment/balance metrics
- advanced analytics/charts/exports
- branch/doctor dashboards, real-time, BI and AI recommendations
""",
    "roadmap Release 7",
)
replace_once(
    roadmap,
    """The initial operational MVP is complete only after formal acceptance of:
- Patients
- Scheduling
- Clinical Records
- Odontogram
- Treatments and Quotes
- Billing
- Documents
- Roles and Permissions
- Basic Dashboard

Current accepted frontier: **Release 6**.

Remaining MVP release acceptance: **Release 7**.
""",
    """The initial operational MVP is formally accepted through:
- Patients
- Scheduling
- Clinical Records
- Odontogram
- Treatments and Quotes
- Billing Document Foundation
- Patient Documents Foundation
- Roles and Permissions
- Dashboard Read Model Foundation

Current accepted frontier: **Release 7**.

MVP status: **accepted**. This bounded milestone does not include payments/cash/CFDI, provider views, automated messaging, online booking, advanced analytics or full Patient Portal.
""",
    "roadmap MVP",
)
replace_once(
    roadmap,
    """### Status
Later phase after the initial MVP is accepted and stable.

### Phase 2.1 — Patient Intake and Portal Foundation

#### Status
Architecturally accepted in ADR 006; implementation not opened.
""",
    """### Status
Next planned phase after formal initial MVP acceptance; implementation not opened automatically.

### Phase 2.1 — Patient Intake and Portal Foundation

#### Status
Architecturally accepted in ADR 006; MVP gate satisfied; explicit phase opening and PI-1 implementation still pending.
""",
    "roadmap Phase 2 status",
)

# docs/ux-redesign-reconciliation-and-plan.md
ux = ROOT / "docs/ux-redesign-reconciliation-and-plan.md"
replace_once(ux, "- Release 6 — Billing through Release 6.1.", "- Release 6 — Billing through Release 6.1.\n- Release 7 — Documents and Dashboard through Release 7.1 and 7.2; initial operational MVP accepted.", "UX completed list")
replace_once(
    ux,
    """Payments, balances, receipts, cash sessions, fiscal/CFDI behavior and Patient Portal access remain outside Release 6.1.

### Current roadmap frontier

The next planned functional phase is **Release 7 — Documents and Dashboard**.

Documents and Dashboard code exists, but remains `implemented but not formally accepted/reconciled` until module-specific audits occur.
""",
    """Payments, balances, receipts, cash sessions, fiscal/CFDI behavior and Patient Portal access remain outside Release 6.1.

### Release 7 reconciliation

Documents and Dashboard are no longer only `implemented but not formally accepted/reconciled`.

The module-specific audit and bounded corrections accepted:

- private tenant/patient-owned Documents upload/list/download/logical retire;
- PDF/JPEG/PNG binary-signature validation and bounded multipart input;
- root-contained private storage;
- read-only tenant-scoped Dashboard metrics;
- tenant-local `Today` through `Tenant.TimeZoneId`;
- conservative permissions and cross-tenant tests;
- bounded Angular Documents and Dashboard flows.

Closure evidence:

- `docs/release-7-documents-and-dashboard-audit-and-closure.md`;
- ADR 010 — `docs/decisions/010-tenant-time-zone-foundation.md`;
- ADR 011 — `docs/decisions/011-release-7-documents-dashboard-and-mvp-closure.md`.

OCR/sharing/versioning, payments/revenue metrics, charts/exports, branch/doctor dashboards and Patient Portal access remain outside Release 7.

### Current roadmap frontier

The initial operational MVP is accepted. The next planned phase is **Phase 2.1 — Patient Intake and Portal Foundation**, but implementation is not opened automatically.
""",
    "UX Release 7 reconciliation",
)
replace_once(
    ux,
    """| Documents | yes | yes | upload/list/download/retire | `document.read/write` | yes | yes | Implemented but not reconciled | partially redesigned |
| Dashboard | yes | yes | summary | `dashboard.read` | read-model based | yes | Implemented but not reconciled | partially redesigned |
""",
    """| Documents | yes | yes | upload/list/download/retire | `document.read/write` | yes | yes | Accepted / preserved through Release 7.1 | partially redesigned; debt remains |
| Dashboard | yes | yes | summary | `dashboard.read` | read-model + tenant timezone migration | yes | Accepted / preserved through Release 7.2 | partially redesigned; debt remains |
""",
    "UX module table",
)
replace_once(
    ux,
    """### Remaining drift: later modules

Documents, Dashboard and reminder helpers contain real code beyond the formal release frontier.
""",
    """### Remaining drift: later capabilities

Reminder helpers and other future capabilities contain real code beyond the accepted MVP frontier.
""",
    "UX remaining drift",
)
replace_once(
    ux,
    """- Releases 1 to 6 are closed and preserved.
- Release 7 is next, but must begin with audits of existing Documents and Dashboard code.
- Do not add new Release 7 functionality until the audits identify a concrete accepted-scope gap.
- Do not reopen Clinical Records, Odontogram or Treatments/Quotes through incidental linkage.
- Existing permission-gated navigation from Quote to Billing does not accept Billing behavior.
- Visual-only work remains allowed on accepted modules when bounded and non-breaking.
- Phase 2 is not active.
- Phase 2.1 Patient Intake and Portal is planned, not implemented.
""",
    """- Releases 1 to 7 are closed and preserved; the initial operational MVP is accepted.
- Documents and Dashboard are accepted only within their bounded Release 7.1/7.2 contracts.
- Do not reopen Clinical Records, Odontogram, Treatments/Quotes or Billing through incidental linkage.
- Visual-only work remains allowed on accepted modules when bounded and non-breaking.
- Phase 2.1 Patient Intake and Portal is the next planned phase, but remains not implemented and requires explicit opening before PI-1.
""",
    "UX current work",
)
replace_once(
    ux,
    """### Documents/Dashboard
- preserve existing code as unaccepted until module audit;
- avoid presenting advanced deferred behavior;
- use visual-only slices only when contracts and scope stay unchanged.
""",
    """### Documents/Dashboard
- preserve the accepted upload/list/download/retire and read-only summary contracts;
- keep tenant/patient context, signature validation, upload limits and tenant-local dates clear;
- avoid presenting OCR, sharing, payments/revenue metrics, advanced analytics or doctor dashboards as implemented;
- replace internal release/slice copy and raw actor ids through bounded UX work;
- use visual-only slices only when contracts and scope stay unchanged.
""",
    "UX module direction",
)
replace_once(
    ux,
    """1. Preserve Release 6 closure documents and canonical state.
2. Audit existing Release 7 Documents code as a bounded storage/access slice.
3. Audit existing Release 7 Dashboard code as a separate read-model slice.
4. Classify each behavior as satisfied, bounded gap or out of accepted scope.
5. Make only the smallest necessary implementation changes.
6. Run repository-wide CI.
7. Reconcile STATE and base docs before advancing the release frontier.
8. Continue visual debt separately from functional acceptance when possible.
""",
    """1. Preserve Release 7/MVP closure documents and canonical state.
2. Keep Phase 2.1 planned until an explicit opening decision resolves issue #2 access/bootstrap choices.
3. When opened, start only PI-1 / issue #4 and preserve staff auth backward compatibility.
4. Keep visual debt separate from functional/security changes.
5. Continue bounded UX cleanup on accepted modules without reopening scope.
6. Run repository-wide CI for every phase-opening or cross-cutting change.
""",
    "UX sequence",
)
replace_once(
    ux,
    """**Decision:** Accept Releases 4, 5 and 6 only after their specific audits; keep later modules unaccepted until equivalent evidence exists; continue visual work as bounded non-functional slices.

**Consequence:** Release 7 becomes the next audit target, while Odontogram, Treatments/Quotes and Billing visual debt remains valid follow-up without reopening accepted functional boundaries.
""",
    """**Decision:** Accept Releases 4 through 7 only after their specific audits and bounded corrections; accept the initial operational MVP; continue visual work as bounded non-functional slices.

**Consequence:** Phase 2.1 becomes the next planned phase without automatic implementation. Visual debt in Odontogram, Treatments/Quotes, Billing, Documents and Dashboard remains valid follow-up without reopening accepted functional boundaries.
""",
    "UX decision note",
)

# docs/patient-intake-and-portal-plan.md
patient_plan = ROOT / "docs/patient-intake-and-portal-plan.md"
replace_once(patient_plan, "- **Start gate:** After the initial MVP is formally accepted and stable, unless a future explicit decision reprioritizes it", "- **Start gate:** Initial MVP accepted; explicit Phase 2.1 opening and issue #2 access/bootstrap decisions still required", "patient plan gate")
replace_once(patient_plan, "- **Last updated:** 2026-07-22", "- **Last updated:** 2026-07-23", "patient plan date")
replace_once(
    patient_plan,
    """- Release 6 — Billing: completed through Release 6.1.
- Release 7 — Documents and Dashboard: next planned functional phase.
- Phase 2.1: planned after the remaining MVP release is accepted and stable.
""",
    """- Release 6 — Billing: completed through Release 6.1.
- Release 7 — Documents and Dashboard: completed through Release 7.1 and 7.2.
- Initial operational MVP: formally accepted under ADR 011.
- Phase 2.1: next planned phase; explicit opening and PI-1 implementation still pending.
""",
    "patient plan roadmap",
)
replace_once(
    patient_plan,
    """| Release 6 Billing foundation | Completed | ADR 009 / Release 6 audit |
| Patient-facing architecture decision | Accepted and merged | ADR 006 / PR #3 |
""",
    """| Release 6 Billing foundation | Completed | ADR 009 / Release 6 audit |
| Release 7 Documents/Dashboard foundations | Completed; MVP accepted | ADR 010/011 / Release 7 audit |
| Patient-facing architecture decision | Accepted and merged | ADR 006 / PR #3 |
""",
    "patient plan status table",
)
replace_once(
    patient_plan,
    """The current repository is positioned at **Release 7 — Documents and Dashboard** as the next planned functional phase.

For Patient Intake and Portal:

1. Keep issues #4 through #7 open and ordered.
2. Do not start PI-1 until Phase 2.1 is explicitly opened after MVP acceptance or documented reprioritization.
3. When Phase 2.1 opens, start only with issue #4.
4. Update canonical state in the same PR that opens PI-1.
""",
    """The current repository has an accepted initial operational MVP. **Phase 2.1 — Patient Intake and Portal Foundation** is the next planned phase, but is not yet opened for implementation.

For Patient Intake and Portal:

1. Keep issues #4 through #7 open and ordered.
2. Resolve issue #2 choices for patient identifier, password vs magic link, TTL, pilot delivery and lockout/recovery baseline.
3. Record an explicit Phase 2.1 opening decision before implementation.
4. When Phase 2.1 opens, start only with issue #4.
5. Update canonical state in the same PR that opens PI-1.
""",
    "patient plan next action",
)
replace_once(
    patient_plan,
    """**Consequence:** The requirement stays visible, decomposed, testable and traceable while Release 7 remains the final pending MVP release before the normal Phase 2.1 gate.
""",
    """**Consequence:** The MVP gate is satisfied and the requirement remains visible, decomposed, testable and traceable. Phase 2.1 still requires an explicit opening decision; PI-1 to PI-4 remain unimplemented.
""",
    "patient plan consequence",
)

# ADR 006
adr006 = ROOT / "docs/decisions/006-patient-intake-and-portal-foundation.md"
replace_once(
    adr006,
    """Phase 2.1 starts only after the initial MVP is formally accepted and stable unless a future explicit decision reprioritizes it.

Current roadmap frontier after Release 6 closure:

```text
Release 7 -> Phase 2.1
```

The current next planned functional phase is Release 7 — Documents and Dashboard.

The broader patient portal remains Phase 4 work. Phase 2.1 is limited to activation, self-service intake/update, clinic review/application and audit.
""",
    """The initial MVP is formally accepted through Release 7 under ADR 011.

Current roadmap frontier after MVP closure:

```text
Phase 2.1 — planned, explicit opening pending
```

Phase 2.1 is the next planned phase, but this ADR and the MVP gate do not automatically open PI-1. The access/bootstrap choices tracked in issue #2 must be resolved and canonical state updated when the phase is explicitly opened.

The broader patient portal remains Phase 4 work. Phase 2.1 is limited to activation, self-service intake/update, clinic review/application and audit.
""",
    "ADR006 roadmap gate",
)

print("Release 7 canonical documentation reconciled")
