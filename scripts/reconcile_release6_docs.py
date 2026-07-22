from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
changed_files: list[str] = []


def load(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def save(path: str, original: str, updated: str) -> None:
    if original == updated:
        raise RuntimeError(f"No changes produced for {path}")
    (ROOT / path).write_text(updated, encoding="utf-8")
    changed_files.append(path)


def replace_once(text: str, old: str, new: str, path: str) -> str:
    count = text.count(old)
    if count != 1:
        raise RuntimeError(f"Expected exactly one match in {path}; found {count}: {old[:100]!r}")
    return text.replace(old, new, 1)


def replace_optional(text: str, old: str, new: str) -> str:
    return text.replace(old, new, 1) if old in text else text


def sub_once(text: str, pattern: str, replacement: str, path: str, flags: int = re.S) -> str:
    updated, count = re.subn(pattern, replacement, text, count=1, flags=flags)
    if count != 1:
        raise RuntimeError(f"Expected exactly one regex match in {path}; found {count}: {pattern}")
    return updated


# ADR 009: proposed -> accepted after canonical reconciliation in this same change.
path = "docs/decisions/009-release-6-billing-document-foundation.md"
original = text = load(path)
text = replace_once(text, "- **Status:** Proposed", "- **Status:** Accepted", path)
text = replace_once(text, "## Proposed decision", "## Decision", path)
text = replace_once(
    text,
    "The ADR becomes `Accepted` only when canonical state, roadmap and base documentation are reconciled in the accepted closure change.",
    "This ADR is accepted together with the canonical state, roadmap and base-document reconciliation in the Release 6 closure change.",
    path,
)
text = sub_once(
    text,
    r"## 12\. Acceptance condition\n.*\Z",
    """## 12. Acceptance evidence

The Release 6 closure change satisfies the acceptance condition by:

- updating `STATE — BigSmile.md`;
- updating `docs/product-roadmap.md`;
- reconciling README, AGENTS and PROJECT_MAP;
- recording Release 6.1 as completed;
- identifying Release 7 as the next planned functional phase;
- preserving Phase 2.1 after the remaining MVP gate;
- passing repository-wide CI.

## 13. Consequence

- Latest completed functional release: Release 6 — Billing.
- Next planned functional phase: Release 7 — Documents and Dashboard.
- Payments, balances, receipts and fiscal invoicing remain unimplemented/unaccepted.
- Patient Intake and Portal remains planned after MVP completion under ADR 006.
""",
    path,
)
save(path, original, text)


# Release 6 audit: closure candidate -> accepted closure evidence.
path = "docs/release-6-billing-audit-and-closure.md"
original = text = load(path)
text = replace_once(
    text,
    "- **Status:** Closure candidate; canonical release-state reconciliation pending",
    "- **Status:** Accepted closure evidence",
    path,
)
text = replace_once(
    text,
    "- **Result:** Existing code satisfies the bounded Billing Document foundation\n- **Current canonical frontier before closure:** Release 5 — Treatments and Quotes\n- **Proposed next planned functional phase after closure:** Release 7 — Documents and Dashboard",
    "- **Result:** Release 6 closes through the bounded Billing Document foundation\n- **Next planned functional phase:** Release 7 — Documents and Dashboard",
    path,
)
text = replace_once(
    text,
    "Formal Release 6 closure still requires the release-state documents to be updated in the same accepted change. This audit therefore records a closure candidate rather than silently advancing `STATE — BigSmile.md` by code presence alone.",
    "The canonical release-state documents are reconciled in the same accepted closure change. No runtime behavior is added or rewritten by the closure.",
    path,
)
text = replace_once(
    text,
    "## 11. Accepted Release 6 boundary proposed by the audit",
    "## 11. Accepted Release 6 boundary",
    path,
)
text = sub_once(
    text,
    r"## 14\. Closure decision candidate\n.*?(?=\n## 15\. Review packet)",
    """## 14. Closure decision

**Decision:** Close Release 6 — Billing through Release 6.1 — Billing Document Foundation.

**Reason:** The bounded roadmap contract is implemented across domain, application, API, persistence, authorization, frontend and automated tests. Remaining work belongs to payments, cash, fiscal or hardening slices.

**Consequence:** Release 7 — Documents and Dashboard becomes the next planned functional phase. Patient Intake and Portal remains planned after MVP completion under ADR 006.
""",
    path,
)
text = replace_once(
    text,
    "Accept the Release 6.1 foundation only with aligned canonical documentation, preserve all deferred financial scope, and open Release 7 through a separate audit rather than extending Billing opportunistically.",
    "Preserve the accepted Release 6.1 foundation and all deferred financial scope, and open Release 7 through a separate audit rather than extending Billing opportunistically.",
    path,
)
save(path, original, text)


# Canonical STATE.
path = "STATE — BigSmile.md"
original = text = load(path)
release5_decision = "**Release 5 — Treatments and Quotes** — [Hecho] ADR 008 acepta el cierre fundacional de planes de tratamiento y cotizaciones mediante Release 5.1 y 5.2, preservando Billing, ejecución de tratamientos y pricing avanzado como scopes posteriores."
text = replace_once(
    text,
    release5_decision,
    release5_decision + "\n\n**Release 6 — Billing** — [Hecho] ADR 009 acepta el cierre fundacional de Billing mediante Release 6.1 — Billing Document Foundation, preservando payments, balances, receipts, cash management y CFDI como scopes posteriores.",
    path,
)
release5_completed = "[Hecho] Release 5 — Treatments and Quotes — completada como release fundacional mediante Release 5.1 y Release 5.2."
text = replace_once(
    text,
    release5_completed,
    release5_completed + "\n\n[Hecho] Release 6 — Billing — completada como release fundacional mediante Release 6.1 — Billing Document Foundation.",
    path,
)
release5_evidence = "[Hecho] El cierre formal de Release 5 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para plan de tratamiento explícito, items básicos con referencia dental opcional, lifecycle `Draft / Proposed / Accepted`, cotización snapshot explícita, pricing por línea, total calculado y gates de precio positivo."
text = replace_once(
    text,
    release5_evidence,
    release5_evidence + "\n\n[Hecho] El cierre formal de Release 6 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para `BillingDocument` explícito desde una cotización aceptada, líneas snapshot, moneda/totales preservados, lifecycle `Draft -> Issued` y documento emitido read-only.",
    path,
)
text = sub_once(
    text,
    r"## 4\. Fase actual\n.*?(?=\n## 4\.1 Nota de reconciliación UX / código existente)",
    """## 4. Fase actual

[Hecho] La última fase funcional marcada como completada es Release 6 — Billing.

[Hecho] Release 6 queda cerrada y preservada como release fundacional de documento comercial mediante Release 6.1 — Billing Document Foundation.

[Hecho] La siguiente fase funcional prevista por el roadmap es Release 7 — Documents and Dashboard.

[Hecho] Release 7 no queda abierta por el cierre de Release 6. Cualquier aceptación debe realizarse mediante auditoría/slices explícitos, con alcance, pruebas y documentación propios.

[Hecho] Phase 2 Expansion — Modern Operations pertenece al roadmap posterior al MVP operativo inicial. No debe abrirse antes de completar y aceptar Release 7, salvo repriorización futura explícita y documentada.

[Hecho] Phase 2.1 — Patient Intake and Portal Foundation permanece planificada dentro de Phase 2. Su arquitectura y plan general están aceptados, pero PI-1 a PI-4 no están implementados ni abiertos como fase activa.
""",
    path,
)
text = replace_once(
    text,
    "[Hecho] El repositorio contiene código funcional en módulos posteriores al estado formal, incluyendo Billing, Documents, Dashboard y recordatorios/manual reminders.\n\n[Hecho] La presencia de código, rutas, permisos, migrations o tests no implica por sí misma aceptación o cierre. Hasta una auditoría específica, esos módulos se clasifican como `implemented but not formally accepted/reconciled`.\n\n[Hecho] Odontogram y Treatments/Quotes dejan esa clasificación porque recibieron auditorías específicas y cierres formales mediante ADR 007/008 y sus documentos de evidencia.",
    "[Hecho] El repositorio contiene código funcional en módulos posteriores al estado formal, incluyendo Documents, Dashboard y recordatorios/manual reminders.\n\n[Hecho] La presencia de código, rutas, permisos, migrations o tests no implica por sí misma aceptación o cierre. Hasta una auditoría específica, esos módulos se clasifican como `implemented but not formally accepted/reconciled`.\n\n[Hecho] Odontogram, Treatments/Quotes y Billing dejan esa clasificación porque recibieron auditorías específicas y cierres formales mediante ADR 007/008/009 y sus documentos de evidencia.",
    path,
)
text = replace_once(
    text,
    "**Ubicación** — [Hecho] primer capability acotado de Phase 2 — Modern Operations después del MVP inicial; no desplaza Release 6 como siguiente fase funcional actual.",
    "**Ubicación** — [Hecho] primer capability acotado de Phase 2 — Modern Operations después del MVP inicial; no desplaza Release 7 como siguiente fase funcional actual.",
    path,
)
release6_section = """## 8. Release 6 — Billing

**Estado operativo actual** — [Hecho] completada como release fundacional de documento comercial.

**Evidencia de cierre** — [Hecho]

- Release 6.1 — Billing Document Foundation.
- Auditoría — `docs/release-6-billing-audit-and-closure.md`.
- Decisión — ADR 009 `docs/decisions/009-release-6-billing-document-foundation.md`.

**Alcance cerrado** — [Hecho]

- `BillingDocument` tenant-owned y patient-owned.
- Creación explícita desde una `TreatmentQuote` existente, `Accepted`, con líneas y precios positivos.
- `GET` devuelve `404` cuando falta; reads/status no autocrean Billing.
- Exactamente un Billing document por TreatmentQuote en este slice.
- Líneas snapshot-only con source quote item, datos descriptivos, referencia dental opcional, `UnitPrice` y `LineTotal`.
- Currency y total preservados con precisión SQL `decimal(18,2)`.
- Lifecycle `Draft -> Issued`.
- Emisión con timestamp UTC y actor; documento emitido read-only.
- `billing.read` / `billing.write`; `TenantUser` sin permisos de Billing.
- UI Angular patient-scoped con prerrequisitos, create, líneas/totales, issue y read-only.

**Tenant safety** — [Hecho] El aggregate root usa filtro global tenant-aware y write enforcement. `BillingDocumentItem` se consume mediante el root; cualquier query directa futura requiere join tenant-aware u ownership explícito.

**Fuera del cierre** — [Hecho]

- Payments, allocations, partial/total payments y balance ledger.
- Receipts, refunds, reversals, cancellations y cash sessions.
- Taxes, discounts, CFDI/PAC, insurance y accounting/ERP.
- Multi-currency, múltiples Billing documents por quote y regeneration/versioning.
- Sincronización automática que modifique una TreatmentQuote aceptada.
- Patient Portal access a Billing.

**Hardening/UX no bloqueante** — [Hecho]

- Normalizar carreras de unique constraint en create cuando el uso concurrente lo requiera.
- Añadir optimistic concurrency antes de ampliar roles operativos de emisión.
- Decidir idempotencia de repeated issue explícitamente.
- Incorporar cobertura relacional SQL Server de índices/precision cuando CI lo soporte.
- Reemplazar copy interno y actor/source ids crudos por lenguaje y affordances operativas.

"""
text = replace_once(text, "## 8. Releases previos cerrados", release6_section + "## 9. Releases previos cerrados", path)
text = replace_once(text, "## 9. Backlog inmediato", "## 10. Backlog inmediato", path)
text = replace_once(text, "## 10. Riesgos y temas a vigilar", "## 11. Riesgos y temas a vigilar", path)
text = replace_once(text, "## 11. Criterios para no perder el rumbo", "## 12. Criterios para no perder el rumbo", path)
text = replace_once(text, "## 12. Nota tipo ADR resumida", "## 13. Nota tipo ADR resumida", path)
text = sub_once(
    text,
    r"## 10\. Backlog inmediato\n\nLista priorizada:\n.*?(?=\n## 11\. Riesgos y temas a vigilar)",
    """## 10. Backlog inmediato

Lista priorizada:

1. Preservar Releases 1 a 6 ya cerrados sin debilitar la fundación tenant-aware.

2. Auditar el código existente de `Release 7 — Documents and Dashboard` contra el roadmap y aceptar únicamente slices con evidencia explícita.

3. No tratar Documents o Dashboard como aceptados solo porque existen código, endpoints, permissions, storage/read models o tests.

4. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera de Release 6.1 hasta slices dedicados.

5. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.

6. Mantener fuera de los agregados cerrados cualquier linkage cross-module no aceptado.

7. Si se consultan directamente tablas hijas de Clinical, Odontogram, TreatmentPlan, TreatmentQuote o BillingDocument, usar join tenant-aware o modelarlas como tenant-owned.

8. Mantener Phase 2.1 visible mediante ADR 006, su plan e issues #2/#4/#5/#6/#7, sin iniciar PI-1 antes del gate de MVP o repriorización explícita.

9. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS y product-roadmap cuando cambie el estado de Release 7 o Phase 2.1.
""",
    path,
)
text = replace_once(
    text,
    "**Child tables** — [Hecho] Los accesos directos futuros a hijos de Clinical, Odontogram, TreatmentPlan o TreatmentQuote necesitan tenant-aware joins u ownership explícito.",
    "**Child tables** — [Hecho] Los accesos directos futuros a hijos de Clinical, Odontogram, TreatmentPlan, TreatmentQuote o BillingDocument necesitan tenant-aware joins u ownership explícito.",
    path,
)
text = replace_once(
    text,
    "**Pricing y estados comerciales** — [Hecho] Billing no debe alterar silenciosamente la inmutabilidad de cotizaciones aceptadas ni asumir sincronización automática no definida entre plan, cotización, cargos o pagos.",
    "**Pricing y estados comerciales** — [Hecho] BillingDocument preserva la cotización aceptada como snapshot separado. Payments, balances, receipts y fiscalización requieren agregados/slices explícitos y no deben añadirse como campos mutables incidentales del documento emitido.",
    path,
)
text = sub_once(
    text,
    r"## 13\. Nota tipo ADR resumida\n.*\Z",
    """## 13. Nota tipo ADR resumida

**Estado:** Nota canónica actualizada con ADR 006, ADR 007, ADR 008 y ADR 009.

**Contexto:** El código de Billing estaba implementado pero correctamente clasificado como no reconciliado hasta una auditoría específica.

**Decisión:** Cerrar Release 6 mediante Release 6.1 — Billing Document Foundation, preservar el documento emitido como snapshot separado de TreatmentQuote y de futuros Payment/Receipt/Cash/Fiscal aggregates, y mover la frontera del roadmap a Release 7. Mantener Patient Intake and Portal como Phase 2.1 posterior al MVP.

**Consecuencias:** Documents and Dashboard es el siguiente módulo a auditar; no se acepta automáticamente. Payments, balances, receipts, cash management y CFDI permanecen diferidos, y la deuda visual/hardening se atiende mediante slices separados.
""",
    path,
)
save(path, original, text)


# README status and roadmap reconciliation.
path = "README.md"
original = text = load(path)
text = replace_once(
    text,
    "Release 4 — Odontogram and Release 5 — Treatments and Quotes are now formally accepted after module-specific audits of their domain, API, persistence, permissions, frontend and automated tests.\n\nThe repository still contains functional code in modules later than the formal roadmap frontier, including Billing, Documents, Dashboard and reminders/manual reminders. Code, routes, permissions, migrations or tests in those modules do not by themselves mean Release 6, Release 7 or Phase 2 are open, accepted or closed.",
    "Release 4 — Odontogram, Release 5 — Treatments and Quotes, and Release 6 — Billing are now formally accepted after module-specific audits of their domain, API, persistence, permissions, frontend and automated tests.\n\nThe repository still contains functional code in modules later than the formal roadmap frontier, including Documents, Dashboard and reminders/manual reminders. Code, routes, permissions, storage/read models or tests in those modules do not by themselves mean Release 7 or Phase 2 are open, accepted or closed.",
    path,
)
text = replace_once(text, "* **Release 5 — Treatments and Quotes**", "* **Release 5 — Treatments and Quotes**\n* **Release 6 — Billing**", path)
text = replace_once(
    text,
    "* **Latest completed delivery phase:** **Release 5 — Treatments and Quotes**\n* **Next planned functional phase:** **Release 6 — Billing**",
    "* **Latest completed delivery phase:** **Release 6 — Billing**\n* **Next planned functional phase:** **Release 7 — Documents and Dashboard**",
    path,
)
release6_readme = """Release 6 is formally complete through:

* **Release 6.1 — Billing Document Foundation**

The accepted Billing boundary includes explicit snapshot creation from an existing accepted quote, one Billing document per quote, no autocreation, preserved currency and `decimal(18,2)` line/total amounts, a bounded `Draft -> Issued` lifecycle, issue actor/time metadata and issued-document immutability.

Billing reads/writes use `billing.read` / `billing.write`. `TenantUser` does not receive those permissions. Billing lines remain child records accessed through the tenant-owned aggregate root.

Payments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency, accounting and Patient Portal access remain deferred.

Release 6 closure evidence:

* `docs/release-6-billing-audit-and-closure.md`
* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`

"""
text = replace_once(
    text,
    "The current authorization foundation includes scope-aware JWT claims, explicit permission policies, policy-gated platform override, centralized tenant read/write enforcement in EF Core, `/api/auth/me`, and frontend session state in memory.",
    release6_readme + "The current authorization foundation includes scope-aware JWT claims, explicit permission policies, policy-gated platform override, centralized tenant read/write enforcement in EF Core, `/api/auth/me`, and frontend session state in memory.",
    path,
)
text = replace_once(
    text,
    "The repository remains established but not functionally complete. Billing, Documents/Dashboard and Phase 2 work must not be assumed accepted until code and documentation explicitly prove it.",
    "The repository remains established but not functionally complete. Documents/Dashboard and Phase 2 work must not be assumed accepted until code and documentation explicitly prove it.",
    path,
)
text = replace_once(
    text,
    "### Release 6 — Billing\n\n* Next planned functional phase after Release 5 closure\n* Existing code remains `implemented but not formally accepted/reconciled` until a dedicated audit\n* Billing should build only on accepted treatment-plan/quote contracts\n* Payments, balances, receipts, taxes, discounts, CFDI, cancellations and advanced workflows remain unaccepted until explicitly reconciled",
    "### Release 6 — Billing\n\n* Completed through Release 6.1 — Billing Document Foundation\n* Explicit creation from an accepted quote with `GET` returning `404` when missing and no autocreation\n* One Billing document per quote and snapshot-only lines\n* Preserved currency, unit price, line total and total with SQL precision `18,2`\n* `Draft -> Issued` lifecycle with issue metadata and issued read-only behavior\n* Payments, balances, receipts, cash management, taxes/discounts and CFDI remain deferred",
    path,
)
text = replace_once(
    text,
    "### Release 7 — Documents and Dashboard\n\n* Planned after Release 6",
    "### Release 7 — Documents and Dashboard\n\n* Next planned functional phase after Release 6 closure\n* Existing code remains `implemented but not formally accepted/reconciled` until module-specific audits",
    path,
)
save(path, original, text)


# AGENTS operational guidance.
path = "AGENTS.md"
original = text = load(path)
text = replace_once(
    text,
    "- `Release 5 — Treatments and Quotes`: completed through accepted slices 5.1 and 5.2\n- Next planned functional phase: `Release 6 — Billing`",
    "- `Release 5 — Treatments and Quotes`: completed through accepted slices 5.1 and 5.2\n- `Release 6 — Billing`: completed through accepted Release 6.1 — Billing Document Foundation\n- Next planned functional phase: `Release 7 — Documents and Dashboard`",
    path,
)
text = replace_once(
    text,
    "Release 5 closure evidence:\n- `docs/release-5-treatments-and-quotes-audit-and-closure.md`\n- ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`",
    "Release 5 closure evidence:\n- `docs/release-5-treatments-and-quotes-audit-and-closure.md`\n- ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`\n\nRelease 6 closure evidence:\n- `docs/release-6-billing-audit-and-closure.md`\n- ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`",
    path,
)
release6_agents = """Treat Release 6 as the accepted foundational Billing boundary:
- explicit Billing creation from an existing accepted quote
- one tenant-owned/patient-owned Billing document per quote
- snapshot-only lines with preserved currency and totals
- bounded `Draft -> Issued` lifecycle and issue metadata
- issued Billing document read-only
- tenant-aware access with explicit `billing.read` / `billing.write`

Do not reopen advanced Billing scope incidentally. Payments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency, accounting and Patient Portal access remain future bounded work. Payment must be designed as a separate aggregate rather than mutable fields on `BillingDocument`.

"""
text = replace_once(
    text,
    "Repository code also exists in later modules, including Billing, Documents, Dashboard and reminders. Until each module receives a specific audit and acceptance pass, classify it as `implemented but not formally accepted/reconciled`.",
    release6_agents + "Repository code also exists in later modules, including Documents, Dashboard and reminders. Until each module receives a specific audit and acceptance pass, classify it as `implemented but not formally accepted/reconciled`.",
    path,
)
text = replace_once(
    text,
    "# Immediate objective\nPreserve Releases 1 to 5 and audit the existing `Release 6 — Billing` implementation against the bounded roadmap before accepting or changing it.\n\nImmediate priorities:\n- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`\n- preserve completed Patients, Scheduling, Clinical Records, Odontogram and Treatments/Quotes behavior\n- audit Billing domain, application, API, persistence, permissions, frontend, migrations and tests\n- distinguish code presence from accepted Release 6 scope\n- avoid reopening TreatmentPlan, TreatmentQuote, Odontogram or Clinical Records through incidental linkage",
    "# Immediate objective\nPreserve Releases 1 to 6 and audit the existing `Release 7 — Documents and Dashboard` implementation against the bounded roadmap before accepting or changing it.\n\nImmediate priorities:\n- preserve tenant-aware authorization aligned with `TenantContext` and, where applicable, `BranchContext`\n- preserve completed Patients, Scheduling, Clinical Records, Odontogram, Treatments/Quotes and Billing behavior\n- audit Documents and Dashboard domain/application/API/persistence/storage/read models, permissions, frontend, migrations and tests\n- distinguish code presence from accepted Release 7 scope\n- avoid reopening Billing, TreatmentPlan, TreatmentQuote, Odontogram or Clinical Records through incidental linkage",
    path,
)
save(path, original, text)


# PROJECT_MAP current state, ownership and next target.
path = "PROJECT_MAP.md"
original = text = load(path)
text = replace_once(
    text,
    "* **Release 5 — Treatments and Quotes:** completed as the foundational treatment-planning and quote release through accepted slices **Release 5.1 — Treatment Plan Foundation** and **Release 5.2 — Quote Foundation**\n* **Next planned functional phase:** **Release 6 — Billing**",
    "* **Release 5 — Treatments and Quotes:** completed as the foundational treatment-planning and quote release through accepted slices **Release 5.1 — Treatment Plan Foundation** and **Release 5.2 — Quote Foundation**\n* **Release 6 — Billing:** completed through **Release 6.1 — Billing Document Foundation**\n* **Next planned functional phase:** **Release 7 — Documents and Dashboard**",
    path,
)
text = replace_once(
    text,
    "### Release 5 closure evidence\n\n* `docs/release-5-treatments-and-quotes-audit-and-closure.md`\n* ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`",
    "### Release 5 closure evidence\n\n* `docs/release-5-treatments-and-quotes-audit-and-closure.md`\n* ADR 008 — `docs/decisions/008-release-5-treatments-and-quotes-closure.md`\n\n### Release 6 closure evidence\n\n* `docs/release-6-billing-audit-and-closure.md`\n* ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`",
    path,
)
text = replace_once(
    text,
    "Odontogram and Treatments/Quotes are now `Accepted / preserved` after module-specific audits of domain, application, API, persistence, permissions, frontend, migrations and tests.\n\nThe repository still contains functional code in later roadmap modules, including Billing, Documents, Dashboard, and reminders/manual reminders. Code, routes, permissions, migrations, or tests in those modules do not by themselves open, accept, or close Release 6, Release 7, or Phase 2.\n\nUntil a module-specific audit and acceptance pass happens, those later modules are `implemented but not formally accepted/reconciled`.",
    "Odontogram, Treatments/Quotes and Billing are now `Accepted / preserved` after module-specific audits of domain, application, API, persistence, permissions, frontend, migrations and tests.\n\nThe repository still contains functional code in later roadmap modules, including Documents, Dashboard, and reminders/manual reminders. Code, routes, permissions, storage/read models, or tests in those modules do not by themselves open, accept, or close Release 7 or Phase 2.\n\nUntil a module-specific audit and acceptance pass happens, those later modules are `implemented but not formally accepted/reconciled`.",
    path,
)
text = sub_once(
    text,
    r"### Current expected priority\n.*?(?=\nDo not treat the repository as feature-complete)",
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
    path,
)
text = replace_once(
    text,
    "### 7.8 Billing\n\nOwns:\n\n* charges\n* payments\n* balances\n* receipts\n* cash sessions\n* future invoicing integration\n\nExisting code is the next audit target and remains unaccepted until Release 6 reconciliation.",
    "### 7.8 Billing\n\nAccepted Release 6 ownership:\n\n* tenant-owned/patient-owned `BillingDocument` aggregate\n* explicit snapshot creation from an accepted TreatmentQuote\n* snapshot-only Billing lines with currency, unit price, line total and total\n* bounded `Draft -> Issued` lifecycle\n* issue timestamp/actor metadata\n* issued-document read-only behavior\n* patient-context Angular Billing workflow\n\nPayments, allocations, balances, receipts, cash sessions, refunds/reversals, taxes/discounts, CFDI/PAC, multi-currency and accounting remain deferred and must not be modeled as incidental mutable fields on `BillingDocument`.",
    path,
)
text = replace_once(text, "* accepted Release 1 through Release 5 boundaries", "* accepted Release 1 through Release 6 boundaries", path)
text = replace_once(text, "* Release 6+ formal scope acceptance", "* Release 7+ formal scope acceptance", path)
text = replace_once(
    text,
    "### Release 6\n\nBilling — next; existing code must be audited before acceptance.\n\n### Release 7\n\nDocuments and Dashboard — planned after Release 6.",
    "### Release 6\n\nBilling — completed through Release 6.1 — Billing Document Foundation.\n\n### Release 7\n\nDocuments and Dashboard — next; existing code must be audited before acceptance.",
    path,
)
text = replace_optional(
    text,
    "For Release 6, audit existing Billing code before adding new behavior. Reuse valid implementation rather than rebuilding it. If a specific area is sparse, prefer local foundations and minimal vertical slices rather than broad expansion.",
    "For Release 7, audit existing Documents and Dashboard code before adding new behavior. Reuse valid implementation rather than rebuilding it. Keep document storage/access and dashboard read models as distinct bounded audit slices.",
)
save(path, original, text)


# Product roadmap.
path = "docs/product-roadmap.md"
original = text = load(path)
text = replace_once(
    text,
    "- **Release 6 — Billing** — planned after Release 5\n- **Release 7 — Documents and Dashboard** — planned after Release 6",
    "- **Release 6 — Billing** — completed\n- **Release 7 — Documents and Dashboard** — next planned functional phase",
    path,
)
text = sub_once(
    text,
    r"## 10\. Release 6 — Billing\n.*?(?=\n## 11\. Release 7 — Documents and Dashboard)",
    """## 10. Release 6 — Billing

### Status
Completed through Release 6.1 — Billing Document Foundation.

### Closure evidence
- Release 6.1 — Billing Document Foundation
- Audit: `docs/release-6-billing-audit-and-closure.md`
- ADR 009: `docs/decisions/009-release-6-billing-document-foundation.md`

### Goal
Provide a bounded commercial record created from an accepted treatment quote while preserving future payment and fiscal workflows as separate capabilities.

### Accepted scope
- tenant-owned/patient-owned `BillingDocument`
- explicit creation from an existing `Accepted` TreatmentQuote
- `GET` returns `404` when missing
- no autocreation from reads/status operations
- one Billing document per TreatmentQuote
- snapshot-only lines preserving source item, description, optional dental location, quantity, unit price and line total
- preserved currency and total with SQL precision `decimal(18,2)`
- lifecycle `Draft -> Issued`
- issue UTC/actor metadata
- issued document read-only
- `billing.read` / `billing.write`
- bounded Angular patient-context create/read/issue workflow
- tenant/cross-tenant/backend/frontend tests

### Current access
- `PlatformAdmin` and `TenantAdmin`: Billing read/write permissions
- `TenantUser`: no Billing permissions
- patient-scoped operations require resolved tenant context

### Deferred
- payment registration/allocation
- partial/total payment lifecycle and balances/ledger
- receipts, refunds, reversals and cancellations
- cash sessions and daily closing
- taxes, discounts and CFDI/PAC
- insurance and multi-currency
- accounting/ERP workflows
- multiple Billing documents, regeneration or versioning
- automatic mutation of accepted TreatmentQuote state
- Patient Portal access

### Non-blocking hardening/UX debt
- normalize concurrent-create unique conflicts if real use requires it
- add optimistic concurrency before expanding concurrent issue roles
- decide repeated-issue idempotency explicitly
- add relational SQL Server constraint/precision coverage when CI supports it
- replace internal release/slice copy and raw ids in clinic-facing UI

---
""",
    path,
)
text = replace_once(
    text,
    "### Status\nPlanned after Release 6.\n\n### Documents planned scope",
    "### Status\nNext planned functional phase; not formally opened or accepted.\n\n### Documents planned scope",
    path,
)
text = replace_once(text, "- issued billing records after Release 6 acceptance", "- issued Billing documents from accepted Release 6.1", path)
text = replace_once(
    text,
    "Current accepted frontier: **Release 5**.\n\nRemaining MVP release acceptance: **Release 6 and Release 7**.",
    "Current accepted frontier: **Release 6**.\n\nRemaining MVP release acceptance: **Release 7**.",
    path,
)
save(path, original, text)


# UX reconciliation plan.
path = "docs/ux-redesign-reconciliation-and-plan.md"
original = text = load(path)
text = replace_once(
    text,
    "- Release 5 — Treatments and Quotes through slices 5.1 and 5.2.",
    "- Release 5 — Treatments and Quotes through slices 5.1 and 5.2.\n- Release 6 — Billing through Release 6.1.",
    path,
)
release6_ux = """### Release 6 reconciliation

Billing is no longer only `implemented but not formally accepted/reconciled`.

The module-specific audit accepted:

- explicit Billing creation from an accepted quote;
- one Billing document per quote and no autocreation;
- snapshot-only lines with preserved currency and totals;
- `Draft -> Issued` lifecycle and issue metadata;
- issued-document immutability;
- tenant/permission protection;
- patient-context Angular create/read/issue flows.

Closure evidence:

- `docs/release-6-billing-audit-and-closure.md`;
- ADR 009 — `docs/decisions/009-release-6-billing-document-foundation.md`.

Payments, balances, receipts, cash sessions, fiscal/CFDI behavior and Patient Portal access remain outside Release 6.1.

"""
text = replace_once(
    text,
    "### Current roadmap frontier\n\nThe next planned functional phase is **Release 6 — Billing**.\n\nBilling code exists, but it remains `implemented but not formally accepted/reconciled` until its own module audit occurs.",
    release6_ux + "### Current roadmap frontier\n\nThe next planned functional phase is **Release 7 — Documents and Dashboard**.\n\nDocuments and Dashboard code exists, but remains `implemented but not formally accepted/reconciled` until module-specific audits occur.",
    path,
)
text = replace_once(
    text,
    "| Billing | yes | yes | quote/billing paths | `billing.read/write` | yes | yes | Implemented but not reconciled; next audit target | partially redesigned |",
    "| Billing | yes | yes | quote/billing paths | `billing.read/write` | yes | yes | Accepted / preserved through Release 6.1 | partially redesigned; debt remains |",
    path,
)
text = replace_once(
    text,
    "### Remaining drift: later modules\n\nBilling, Documents, Dashboard and reminder helpers contain real code beyond the formal release frontier.",
    "### Resolved drift: Billing\n\nThe Release 6 audit proved the bounded BillingDocument snapshot/issue foundation without accepting payments or fiscal behavior.\n\n### Remaining drift: later modules\n\nDocuments, Dashboard and reminder helpers contain real code beyond the formal release frontier.",
    path,
)
text = replace_once(
    text,
    "- Releases 1 to 5 are closed and preserved.\n- Release 6 is next, but must begin with an audit of existing Billing code.\n- Do not add new Billing functionality until the audit identifies a concrete accepted-scope gap.",
    "- Releases 1 to 6 are closed and preserved.\n- Release 7 is next, but must begin with audits of existing Documents and Dashboard code.\n- Do not add new Release 7 functionality until the audits identify a concrete accepted-scope gap.",
    path,
)
text = replace_once(
    text,
    "### Billing\n- audit functionality before visual expansion;\n- keep source quote, issue state, monetary totals and next action visible;\n- do not imply payments, balances, receipts, tax/CFDI or cancellation behavior before Release 6 reconciliation;\n- never visually suggest that Billing edits the accepted quote snapshot.",
    "### Billing\n- preserve explicit snapshot creation and issue semantics;\n- keep source quote, issue state, monetary totals and next action visible;\n- keep issued documents visibly read-only;\n- do not imply payments, balances, receipts, tax/CFDI or cancellation behavior;\n- never visually suggest that Billing edits the accepted quote snapshot;\n- replace internal release/slice copy and raw ids through bounded UX work.",
    path,
)
text = sub_once(
    text,
    r"## 8\. Recommended sequence\n.*?(?=\n## 9\. Visual acceptance criteria)",
    """## 8. Recommended sequence

1. Preserve Release 6 closure documents and canonical state.
2. Audit existing Release 7 Documents code as a bounded storage/access slice.
3. Audit existing Release 7 Dashboard code as a separate read-model slice.
4. Classify each behavior as satisfied, bounded gap or out of accepted scope.
5. Make only the smallest necessary implementation changes.
6. Run repository-wide CI.
7. Reconcile STATE and base docs before advancing the release frontier.
8. Continue visual debt separately from functional acceptance when possible.
""",
    path,
)
text = replace_once(
    text,
    "**Decision:** Accept Release 4 and Release 5 only after their specific audits; keep later modules unaccepted until equivalent evidence exists; continue visual work as bounded non-functional slices.\n\n**Consequence:** Release 6 becomes the next audit target, while Odontogram and Treatments/Quotes visual debt remains valid follow-up without reopening accepted functional boundaries.",
    "**Decision:** Accept Releases 4, 5 and 6 only after their specific audits; keep later modules unaccepted until equivalent evidence exists; continue visual work as bounded non-functional slices.\n\n**Consequence:** Release 7 becomes the next audit target, while Odontogram, Treatments/Quotes and Billing visual debt remains valid follow-up without reopening accepted functional boundaries.",
    path,
)
save(path, original, text)


# Patient Intake plan and ADR gate references.
path = "docs/patient-intake-and-portal-plan.md"
original = text = load(path)
text = replace_once(
    text,
    "- Release 5 — Treatments and Quotes: completed.\n- Release 6 — Billing: next planned functional phase.\n- Release 7 — Documents and Dashboard: pending.\n- Phase 2.1: planned after the remaining MVP releases are accepted and stable.",
    "- Release 5 — Treatments and Quotes: completed.\n- Release 6 — Billing: completed through Release 6.1.\n- Release 7 — Documents and Dashboard: next planned functional phase.\n- Phase 2.1: planned after the remaining MVP release is accepted and stable.",
    path,
)
text = replace_once(
    text,
    "| Release 5 Treatments and Quotes foundation | Completed | ADR 008 / Release 5 audit |",
    "| Release 5 Treatments and Quotes foundation | Completed | ADR 008 / Release 5 audit |\n| Release 6 Billing foundation | Completed | ADR 009 / Release 6 audit |",
    path,
)
text = replace_once(
    text,
    "The current repository is positioned at **Release 6 — Billing** as the next planned functional phase.",
    "The current repository is positioned at **Release 7 — Documents and Dashboard** as the next planned functional phase.",
    path,
)
text = replace_once(
    text,
    "**Consequence:** The requirement stays visible, decomposed, testable and traceable while Release 6 remains the current product frontier and Release 7 remains pending before the Phase 2.1 gate.",
    "**Consequence:** The requirement stays visible, decomposed, testable and traceable while Release 7 remains the final pending MVP release before the normal Phase 2.1 gate.",
    path,
)
save(path, original, text)

path = "docs/decisions/006-patient-intake-and-portal-foundation.md"
original = text = load(path)
text = replace_once(
    text,
    "Current roadmap frontier after Release 5 closure:\n\n```text\nRelease 6 -> Release 7 -> Phase 2.1\n```\n\nThe current next planned functional phase is Release 6 — Billing.",
    "Current roadmap frontier after Release 6 closure:\n\n```text\nRelease 7 -> Phase 2.1\n```\n\nThe current next planned functional phase is Release 7 — Documents and Dashboard.",
    path,
)
save(path, original, text)

print("Updated files:")
for file_name in changed_files:
    print(f"- {file_name}")
