# BigSmile — Estado actual canónico

## 1. Resumen ejecutivo

[Hecho] BigSmile es una plataforma SaaS para la gestión de clínicas y consultorios dentales, concebida desde el inicio como producto comercial multi-tenant y no como sistema interno de una sola clínica.

[Hecho] El objetivo del producto es cubrir el backbone operativo de la clínica en un flujo continuo: cita → expediente/paciente → expediente clínico → odontograma → plan de tratamiento → cotización → pago → seguimiento, preservando seguridad, mantenibilidad y UX operativa rápida.

[Hecho] Stack principal: backend .NET 10 + ASP.NET Core Web API + EF Core + SQL Server; frontend Angular 21 + TypeScript; calidad/operación con GitHub Actions, pruebas automatizadas, logging estructurado, health checks y auditoría.

[Hecho] Modelo SaaS: Tenant = clínica/consultorio cliente; Branch = sucursal/ubicación interna del tenant. `TenantId` es la frontera primaria de seguridad y propiedad; `BranchId` es scope operativo subordinado, no el boundary principal.

## 2. Decisiones arquitectónicas cerradas

**Arquitectura general** — [Hecho] BigSmile inicia como modular monolith, con fronteras explícitas de backend (Api / Application / Domain / Infrastructure / SharedKernel) y frontend (core / shell / shared / features). No hay decisión de microservicios para la etapa inicial.

**Multitenancy** — [Hecho] La estrategia base es shared database + shared schema + `TenantId` como discriminador transversal, con `TenantContext` por request, enforcement centralizado y bypass solo para operaciones de plataforma explícitas y auditables.

**Auth** — [Hecho] La base de identidad y autenticación está establecida sobre JWT y autorización tenant-aware: claims de scope/permiso, `TenantContext` enriquecido, policies/handlers por scope, `/api/auth/me`, override de plataforma explícito y auditable, y contexto frontend en memoria.

**Persistencia** — [Hecho] La persistencia base es EF Core sobre SQL Server, con `AppDbContext`, migraciones, seed durable y login real contra SQL Server. `BranchId` solo se usa cuando el dominio lo requiere y permanece subordinado a `TenantId`.

**Frontend** — [Hecho] El frontend es feature-based, con separación entre páginas, componentes, facades, data-access y modelos. Las llamadas HTTP permanecen en data-access y se prioriza UX operativa rápida.

**Patient Intake and Portal Foundation** — [Hecho] ADR 006 queda aceptado como decisión futura: identidad de paciente separada del acceso interno; sin `TenantUser`, `UserTenantMembership` ni permisos tenant-wide; invitaciones single-use para pacientes existentes; enlace/QR tenant-scoped para pacientes nuevos; flujo `Draft -> Submitted -> Reviewed -> Applied/Rejected`; revisión clínica antes de aplicación canónica; y bitácora append-only. Se ubica en Phase 2.1 después del MVP inicial y no abre implementación actual.

**Release 4 — Odontogram** — [Hecho] ADR 007 acepta el cierre del Odontogram fundacional mediante los slices 4.1 a 4.4, sin exigir funcionalidades avanzadas expresamente diferidas.

**Release 5 — Treatments and Quotes** — [Hecho] ADR 008 acepta el cierre fundacional de planes de tratamiento y cotizaciones mediante Release 5.1 y 5.2, preservando Billing, ejecución de tratamientos y pricing avanzado como scopes posteriores.

**Release 6 — Billing** — [Hecho] ADR 009 acepta el cierre fundacional de Billing mediante Release 6.1 — Billing Document Foundation, preservando payments, balances, receipts, cash management y CFDI como scopes posteriores.

**Tenant Time Zone Foundation** — [Hecho] ADR 010 fija `Tenant.TimeZoneId` como fuente server-authoritative de la fecha operativa local, con default de migración `America/Mexico_City` para el piloto actual, sin convertir Branch en boundary temporal independiente ni reescribir Appointment.

**Release 7 — Documents and Dashboard** — [Hecho] ADR 011 acepta Release 7 mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation, y formaliza el cierre del MVP operativo inicial.

## 3. Fases completadas

[Hecho] Foundation / Release 0 base — completada.

[Hecho] Pre-auth hardening — completada.

[Hecho] Identity + Persistence Foundation — completada.

[Hecho] Tenant-Aware Authorization Foundation — completada.

[Hecho] Release 1 — Patients — completada.

[Hecho] Release 2 — Scheduling — completada.

[Hecho] Release 3 — Clinical Records — completada.

[Hecho] Release 4 — Odontogram — completada como release fundacional mediante Release 4.1, 4.2, 4.3 y 4.4.

[Hecho] Release 5 — Treatments and Quotes — completada como release fundacional mediante Release 5.1 y Release 5.2.

[Hecho] Release 6 — Billing — completada como release fundacional mediante Release 6.1 — Billing Document Foundation.

[Hecho] Release 7 — Documents and Dashboard — completada mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation.

[Hecho] El MVP operativo inicial queda formalmente aceptado con Releases 1 a 7 y la fundación de roles/permisos cerradas mediante evidencia de código, pruebas y documentación alineada.

[Hecho] El cierre formal de Release 2 cubre calendario diario/semanal branch-aware, creación/edición/reprogramación/cancelación, appointment notes, blocked slots y estados `Attended` / `NoShow`. `doctor-based views` permanece diferido porque requiere provider/doctor assignment.

[Hecho] El cierre formal de Release 3 cubre creación explícita de expediente clínico, snapshot base, alergias actuales, notas append-only, diagnósticos básicos, timeline clínica acotada, snapshot history, cuestionario médico fijo, consulta/signos vitales, atribución por usuario y protección con `clinical.read` / `clinical.write`.

[Hecho] El cierre formal de Release 4 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para odontograma explícito, 32 dientes FDI permanentes adultos, estados de diente, superficies `O/M/D/B/L`, hallazgos básicos y change history append-only de hallazgos.

[Hecho] El cierre formal de Release 5 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para plan de tratamiento explícito, items básicos con referencia dental opcional, lifecycle `Draft / Proposed / Accepted`, cotización snapshot explícita, pricing por línea, total calculado y gates de precio positivo.

[Hecho] El cierre formal de Release 6 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para `BillingDocument` explícito desde una cotización aceptada, líneas snapshot, moneda/totales preservados, lifecycle `Draft -> Issued` y documento emitido read-only.

[Hecho] El cierre formal de Release 7 se apoya en la auditoría de Documents/Dashboard, el hardening de upload binario de PR #19, la fundación tenant-owned de zona horaria de PR #20, CI completas y ADR 010/011.

## 4. Fase actual

[Hecho] La última fase funcional marcada como completada es Release 7 — Documents and Dashboard.

[Hecho] Release 7 queda cerrada y preservada mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation.

[Hecho] El MVP operativo inicial queda formalmente aceptado. Esta aceptación cubre los boundaries fundacionales documentados y no implica payments, cash management, CFDI, doctor views, automatizaciones, advanced analytics ni full Patient Portal.

[Hecho] La siguiente fase prevista por el roadmap es Phase 2.1 — Patient Intake and Portal Foundation.

[Hecho] El gate normal de MVP para Phase 2.1 ya está satisfecho, pero la fase no se abre ni implementa automáticamente. Antes de PI-1 se requiere decisión explícita de apertura y resolver los choices de acceso/bootstrap registrados en issue #2.

[Hecho] PI-1 a PI-4 permanecen no implementados. Cuando Phase 2.1 se abra explícitamente, el primer slice será PI-1 — Access and Invitation Foundation, issue #4.

## 4.1 Nota de reconciliación UX / código existente

[Hecho] El repositorio contiene código funcional posterior o lateral al MVP aceptado, incluyendo recordatorios/manual reminders.

[Hecho] La presencia de código, rutas, permisos, migrations o tests sigue sin implicar por sí misma aceptación de una fase futura. Cada capability posterior requiere auditoría, alcance y documentación explícitos.

[Hecho] Odontogram, Treatments/Quotes, Billing, Documents y Dashboard dejan la clasificación `implemented but not formally accepted/reconciled` porque recibieron auditorías específicas y cierres formales mediante ADR 007/008/009/011 y sus documentos de evidencia.

[Hecho] Los slices visuales pueden mejorar presentación, organización, copy, color, microinteracciones, modales/drawers/tabs/sticky action bars y deuda UX sin cambiar backend, APIs, permissions, auth, tenant context, branch context, migrations ni alcance funcional.

[Hecho] El ajuste UX del cuestionario médico solicitado por el cliente quedó integrado mediante PR #1: opciones visibles `Sí / No / Sin respuesta`, avance de captura y preservación de `Unknown` como estado seguro.

## 4.2 Plan futuro — Phase 2.1 Patient Intake and Portal Foundation

**Estado** — [Hecho] decisión aceptada y trabajo planificado; implementación no iniciada.

**Ubicación** — [Hecho] siguiente fase prevista después del MVP aceptado; permanece planificada y no desplaza la necesidad de una apertura explícita antes de implementar PI-1.

**Tracking** — [Hecho]

- ADR 006 — `docs/decisions/006-patient-intake-and-portal-foundation.md`.
- Plan general — `docs/patient-intake-and-portal-plan.md`.
- Parent issue — #2.
- PI-1 Access and Invitation Foundation — issue #4.
- PI-2 Intake Draft — issue #5.
- PI-3 Submit, Review and Apply — issue #6.
- PI-4 Audit Visibility and Hardening — issue #7.

**Reglas cerradas** — [Hecho]

- El paciente no es un usuario interno del tenant.
- El acceso del paciente es self-scoped y sin platform override.
- `TenantId`, portal account y Patient vinculado se derivan de contexto verificado.
- Registro público crea intake pendiente, no `Patient`/`ClinicalRecord` canónicos directamente.
- Cambios enviados por el paciente requieren revisión de la clínica antes de aplicación canónica.
- Cada guardado efectivo y transición relevante deja revisión/bitácora append-only.
- Full patient portal permanece diferido a Phase 4.

## 5. Release 3 — Clinical Records

**Estado operativo actual** — [Hecho] completada y preservada como release clínica fundacional.

**Evidencia de cierre** — [Hecho]

- Release 3.1 — Clinical Record Foundation.
- Release 3.2 — Basic Diagnoses Foundation.
- Release 3.3 — Clinical Timeline Read Model.
- Release 3.4 — Clinical Snapshot Change History.
- Release 3.5 — Medical Questionnaire Backend.
- Release 3.6 — Clinical Encounter / Vitals Backend.

**Alcance cerrado** — [Hecho]

- `ClinicalRecord` tenant-owned y patient-owned, exactamente uno activo por Patient/Tenant.
- Creación explícita; `GET` devuelve `404` cuando no existe; sin autocreación.
- Medical background, medicamentos actuales y alergias actuales.
- Notas append-only y diagnósticos básicos add/resolve.
- Timeline acotada basada en notas/diagnósticos y snapshot history separado.
- Cuestionario fijo `Unknown` / `Yes` / `No` con `Details` opcional.
- `ClinicalEncounter` con motivo, tipo y signos vitales opcionales.
- `TenantId` y actor derivados del contexto.
- `clinical.read` / `clinical.write` para `PlatformAdmin` y `TenantAdmin`; sin acceso clínico para `TenantUser`.

**Fuera del cierre** — [Hecho]

- Timeline clínica avanzada o cross-module.
- Restore/versionado completo/rich diff.
- Form builder configurable y auto-sync de alergias.
- Edición/borrado de encounters.
- Doctor/provider assignment.
- Patient self-service y portal.
- Scheduling, Billing, Odontogram, Treatments o Documents como parte del agregado Clinical.

## 6. Release 4 — Odontogram

**Estado operativo actual** — [Hecho] completada como release odontológica fundacional.

**Evidencia de cierre** — [Hecho]

- Release 4.1 — Odontogram Foundation.
- Release 4.2 — Odontogram Surface Foundation.
- Release 4.3 — Basic Dental Findings Foundation.
- Release 4.4 — Dental Findings Change History.
- Auditoría — `docs/release-4-odontogram-audit-and-closure.md`.
- Decisión — ADR 007 `docs/decisions/007-release-4-odontogram-closure.md`.

**Alcance cerrado** — [Hecho]

- `Odontogram` tenant-owned y patient-owned, exactamente uno por `TenantId + PatientId`.
- Creación explícita; `GET` devuelve `404` cuando falta; sin autocreación.
- 32 dientes permanentes adultos mediante FDI `11-18`, `21-28`, `31-38`, `41-48`.
- Estados acotados de diente y superficie.
- Cinco superficies `O/M/D/B/L` por diente.
- Hallazgos básicos `Caries` / `Restoration` / `MissingStructure` / `Sealant`.
- Add/remove explícito de hallazgos y finding history append-only, newest-first.
- `odontogram.read` / `odontogram.write`; `TenantUser` sin permisos de Odontogram.
- UI Angular en contexto de paciente, con HTTP en data-access y orquestación en facade.

**Tenant safety** — [Hecho] El agregado raíz tiene filtro tenant-aware y writes centralizados. Las tablas hijas se consumen mediante `Odontogram`; cualquier query directa futura debe usar join tenant-aware o modelar ownership explícito.

**Fuera del cierre** — [Hecho]

- Dentición infantil o mixta y bulk editing.
- Catálogo complejo/configurable de hallazgos.
- Linkage con diagnósticos, tratamientos, documentos o imágenes.
- Timeline dental completa e historial completo de estados.
- Restore/revert y versionado completo.
- Ortodoncia, periodoncia, overlays de imagen o AI-assisted detection.
- Acceso del patient portal al odontograma.

## 7. Release 5 — Treatments and Quotes

**Estado operativo actual** — [Hecho] completada como release fundacional de planeación y cotización.

**Evidencia de cierre** — [Hecho]

- Release 5.1 — Treatment Plan Foundation.
- Release 5.2 — Quote Foundation.
- Auditoría — `docs/release-5-treatments-and-quotes-audit-and-closure.md`.
- Decisión — ADR 008 `docs/decisions/008-release-5-treatments-and-quotes-closure.md`.

**Alcance cerrado del plan** — [Hecho]

- `TreatmentPlan` tenant-owned y patient-owned.
- Exactamente un plan por `TenantId + PatientId` en el slice actual; múltiples/archivados permanecen diferidos.
- Creación explícita; `GET` devuelve `404` cuando falta; sin autocreación.
- Items con title obligatorio, category opcional, quantity positiva, notes acotadas y referencia FDI/surface opcional.
- Add/remove explícito de items.
- Lifecycle `Draft -> Proposed -> Accepted`, con retorno `Proposed -> Draft`.
- Plan aceptado read-only.
- Metadata UTC/actor.

**Alcance cerrado de la cotización** — [Hecho]

- Creación explícita desde un plan existente con al menos un item.
- Exactamente una cotización por TreatmentPlan; sin autocreación.
- Items snapshot-only del plan al momento de creación.
- Path público acotado a `MXN`.
- `UnitPrice`, `LineTotal` y total calculado.
- Precios positivos obligatorios para `Proposed` y revalidados para `Accepted`.
- Cotización aceptada read-only.
- Sin regenerate/versioning ni negociación multi-cotización.

**Acceso** — [Hecho] `treatmentplan.read/write` y `treatmentquote.read/write` se conceden a `PlatformAdmin` y `TenantAdmin`; `TenantUser` no recibe esos permisos. El cierre no expande roles ni platform override.

**Tenant safety** — [Hecho] Plan y cotización son aggregate roots tenant-owned con filtros globales y write enforcement. Items se consumen mediante los agregados; cualquier query directa futura requiere join tenant-aware u ownership explícito.

**Fuera del cierre** — [Hecho]

- Treatment catalog administration.
- Múltiples planes, archive o versionado.
- Quote regenerate/versioning, múltiples cotizaciones o negociación.
- Billing queda fuera de Release 5 y se acepta por separado en Release 6.1; payments y scheduling linkage permanecen diferidos.
- Treatment execution/progress y sincronización automática de estados.
- Insurance, financing y advanced approvals.
- Automated treatment follow-up.
- Patient Portal access a planes/cotizaciones.

**Deuda UX no bloqueante** — [Hecho]

- Reemplazar copy interno `Release 5.1/5.2`, `foundation` y `slice` por lenguaje operativo.
- Migrar colores hardcodeados residuales a tokens `--bsm-*`.
- Mantener la navegación hacia Billing alineada con Release 6.1, sin implicar payments, balances o fiscalización.

## 8. Release 6 — Billing

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

## 9. Release 7 — Documents and Dashboard

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

**Release 1 — Patients** — [Hecho] registro, actualización, búsqueda tenant-scoped, perfil, responsible party, estatus, alertas clínicas básicas y permisos `patient.read` / `patient.write`.

**Release 2 — Scheduling** — [Hecho] citas tenant-owned/branch-aware, calendario day/week, create/edit/reschedule/cancel, notes, blocked slots, `Attended` / `NoShow` y permisos `scheduling.read` / `scheduling.write`.

## 11. Backlog inmediato

Lista priorizada:

1. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.

2. Tratar Phase 2.1 — Patient Intake and Portal Foundation como la siguiente fase prevista, no como implementación ya abierta.

3. Resolver explícitamente en issue #2 el identificador de acceso, password vs magic link, TTL, entrega piloto y baseline de lockout/recovery antes de abrir PI-1.

4. Cuando Phase 2.1 se abra, iniciar únicamente PI-1 — Access and Invitation Foundation, issue #4, y actualizar STATE en el mismo PR.

5. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.

6. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.

7. Mantener fuera de agregados cerrados cualquier linkage cross-module no aceptado y preservar joins tenant-aware para accesos directos a tablas hijas.

8. Mantener recordatorios/providers/jobs/queues, online booking, advanced analytics y full Patient Portal como capabilities futuras no aceptadas.

9. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap y ADRs cuando se abra Phase 2.1 o cambie el estado del producto.

## 12. Riesgos y temas a vigilar

**Tenant isolation** — [Hecho] Sigue siendo el riesgo estructural principal.

**Authorization model** — [Hecho] Permisos nuevos deben evolucionar junto con módulos reales, sin cambiar silenciosamente scopes o roles.

**Patient-facing identity** — [Hecho] La futura frontera pública no debe reutilizar staff membership, aceptar `PatientId`/`TenantId` como autoridad, permitir platform override ni aplicar cambios canónicos sin revisión.

**Query filters y acceso a datos** — [Hecho] No degradar filtros globales y write enforcement con filtros manuales dispersos.

**Tenant operational time** — [Hecho] `Tenant.TimeZoneId` es la fuente server-side del día operativo; no confiar en fecha/timezone del browser ni introducir una timezone global que rompa el modelo multi-tenant.

**Binary document input** — [Hecho] La allowlist de documentos requiere matching binary signature, límites de transporte y storage containment; no debe presentarse como antivirus o malware scanning.

**Child tables** — [Hecho] Los accesos directos futuros a hijos de Clinical, Odontogram, TreatmentPlan, TreatmentQuote o BillingDocument necesitan tenant-aware joins u ownership explícito.

**Pricing y estados comerciales** — [Hecho] BillingDocument preserva la cotización aceptada como snapshot separado. Payments, balances, receipts y fiscalización requieren agregados/slices explícitos y no deben añadirse como campos mutables incidentales del documento emitido.

**Privileged/platform paths** — [Hecho] Toda operación fuera de tenant scope normal debe ser explícita, mínima y auditable.

**Clinical provenance** — [Hecho] Las declaraciones de pacientes no equivalen a observaciones profesionales.

**UX operativa** — [Hecho] Seguridad y estructura no deben degradar velocidad ni claridad.

**Alineación documental futura** — [Hecho] Cada apertura/cierre debe actualizar código, pruebas y documentación en el mismo cambio.

## 13. Criterios para no perder el rumbo

[Hecho] BigSmile es un producto SaaS multi-tenant; cualquier atajo que debilite tenant isolation, mantenibilidad o reviewability rompe el rumbo.

[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. La siguiente fase prevista es Phase 2.1 Patient Intake and Portal Foundation; el portal amplio permanece en Phase 4.

[Hecho] Ningún release funcional ni Phase 2.1 se considera completado sin evidencia explícita en código, pruebas y documentación alineada.

[Hecho] Restricciones a preservar: modular monolith, ownership explícito, `TenantId` como boundary primario, `BranchId` subordinado, shared DB/schema, sin bypass oculto y sin autorización crítica solo en UI.

## 14. Nota tipo ADR resumida

**Estado:** Nota canónica actualizada con ADR 006, ADR 007, ADR 008, ADR 009, ADR 010 y ADR 011.

**Contexto:** Documents y Dashboard tenían implementación coherente, pero la auditoría detectó una allowlist de upload spoofable y una fecha `Today` basada en UTC. Ambos gaps se corrigieron mediante PR #19 y PR #20 con CI verde.

**Decisión:** Cerrar Release 7 mediante Release 7.1 — Patient Documents Foundation y Release 7.2 — Dashboard Read Model Foundation; aceptar el MVP operativo inicial; preservar Documents como attachment foundation privada y Dashboard como read model tenant-scoped; mover la siguiente fase prevista a Phase 2.1 sin abrir PI-1 automáticamente.

**Consecuencias:** El MVP queda listo para validación piloto bajo su scope acotado. Phase 2.1 requiere una decisión explícita de apertura y resolver los choices de issue #2; payments/cash/CFDI, doctor views, automatizaciones, advanced analytics y full Patient Portal permanecen diferidos.
