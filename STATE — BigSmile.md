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

**Patient Intake and Portal Foundation** — [Hecho] ADR 006 define la frontera separada de identidad/intake; ADR 012 a ADR 015 cierran el acceso, invitaciones, autenticación/sesión y frontera Angular; ADR 016 fija el baseline de datos y draft lifecycle; ADR 017 acepta la API self-only de paciente existente; ADR 018 y ADR 019 cierran la credencial de sala de espera y el boundary `patient_intake`. PI-1 y PI-2C quedan completados. PI-2D está activa mediante #44 y se implementa secuencialmente como #45 → #46 → #47 → #48.

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

[Hecho] La última fase funcional completada es Release 7 — Documents and Dashboard y el MVP operativo inicial permanece formalmente aceptado.

[Hecho] Phase 2.1 — Patient Intake and Portal Foundation es la fase funcional actual; fue abierta explícitamente el 2026-07-24 después de aprobar su baseline de acceso.

[Hecho] PI-1 — Access and Invitation Foundation queda completado mediante PI-1A a PI-1D. PI-1A — Account and Invitation Domain/Persistence quedó completado mediante PR #26 y merge commit `43ddb2e008ce07b4798c21409e3fe58b4839668d`.

[Hecho] PI-1A incorporó `PatientPortalAccount`, `PatientPortalInvitation`, tenant-scoped login uniqueness, one-patient linkage, hash-only invitation persistence, lockout/session metadata, rowversion, filtros/write enforcement, migración y pruebas. No expone activación/login, JWT de paciente ni captura de intake.

[Hecho] PI-1B — Staff Invitation Lifecycle (#23) queda completado mediante PR #28: permiso dedicado solo para `TenantAdmin`, endpoints staff tenant-scoped de emisión/listado/revocación, token criptográfico one-time con hash-at-rest, TTL configurable de 24 horas, replacement explícito y bitácora append-only.

[Hecho] PI-1C — Patient Activation, Login and Self-Session (#24) queda completado mediante PR #29: realm por `Tenant.Subdomain`, activación single-use transaccional, Identity V3/PBKDF2 con parámetros explícitos, JWT patient-only separado, validación server-side de `SessionVersion`, lockout/rate limiting/anti-enumeración, recovery asistido solo `TenantAdmin` y auditoría append-only.

[Hecho] PI-1D — Patient Auth Frontend and Security Closure (#25) queda completado mediante PR #30: route tree `/patient-portal/*` fuera del staff shell, activación con token en fragment y limpieza inmediata, sesión/token solo en memoria, interceptores/guards separados, login/home/logout acotados, pruebas frontend y runbook de recovery. PI-1 queda cerrado.

[Hecho] El cliente aprobó el baseline de PI-2 el 2026-07-25: campos de propuesta —incluido motivo de visita—, teléfonos tipificados como intake, expiración sliding de 30 días, guardado explícito sin autosave, link de sala de espera single-use de 30 minutos, permiso `patientportal.intake.manage` solo para `TenantAdmin`, scope `patient_intake` y secuencia PI-2A → PI-2B → PI-2C → PI-2D. ADR 016 registra el baseline; PI-2C materializó el link, permiso y scope.

[Hecho] PI-2A — Patient Intake Domain and Persistence (#31) queda completado mediante PR #32: `PatientIntake`, 39 respuestas separadas de Clinical, revisiones inmutables por guardado efectivo, expiración `Draft / Expired`, baseline canónico para conflictos futuros, `RowVersion`, filtros/write enforcement tenant-aware, restricciones SQL e integración EF mediante migración `20260725182044_AddPatientIntakeDraftFoundation`. No agrega endpoints, JWT scope, staff permission, UI de intake ni writes canónicos.

[Hecho] PI-2B — Existing-Patient Self-Service Draft (#33) queda completado mediante PR #34 y merge commit `7325a73e7f86ae0e6f0557574fe9d9756a89293f`: `POST / GET / PUT /api/patient-portal/intake`, ownership derivado de sesión patient-only, GET sin side effects, no-store, 39 respuestas `Unknown`, save explícito con optimistic concurrency, no-op sin revisión y cambio efectivo con una revisión append-only. CI #315 quedó verde y no se modifican datos canónicos.

[Hecho] PI-2C — Waiting-Room Link and Intake-Only Scope (#35) queda completado mediante PI-2C1 #36 / PR #41, PI-2C2 #37 / PR #42 y PI-2C3 #38 / PR #43. El cierre incluye credencial single-use/hash-only de 30 minutos, permiso `patientportal.intake.manage` solo `TenantAdmin`, activación transaccional, cuenta unlinked con `scope=patient_intake`, UI staff para generar/copiar/imprimir/revocar y QR local, fragment cleanup y sesión browser memory-only. CI #455 quedó verde y no se crean datos canónicos.

[Hecho] PI-2D — Angular Patient Intake Capture and PI-2 Closure (#44) queda completado mediante PI-2D1 a PI-2D4. PI-2D1 #45 cerró rutas/sesiones/data-access mediante PR #50 y el hardening method-aware de PR #51; PI-2D2 #46 cerró captura no médica mediante PR #53; PI-2D3 #47 cerró el catálogo compartido y las 39 preguntas mediante PR #55; PI-2D4 #48 quedó completado mediante PR #57, merge commit `4b8cb66163948c5b69ff6c3c0027d01e105ce1fb` y CI #485 con conflicto/expiración fail-safe, sesión scope-correct, navegación sin guardar y smoke automatizado. Con ello PI-2 — Intake Draft and Self-Service Capture queda formalmente cerrado sin writes canónicos.

[Hecho] El MVP aceptado sigue sin implicar payments, cash management, CFDI, doctor views, automatizaciones, advanced analytics ni full Patient Portal.

## 4.1 Nota de reconciliación UX / código existente

[Hecho] El repositorio contiene código funcional posterior o lateral al MVP aceptado, incluyendo recordatorios/manual reminders.

[Hecho] La presencia de código, rutas, permisos, migrations o tests sigue sin implicar por sí misma aceptación de una fase futura. Cada capability posterior requiere auditoría, alcance y documentación explícitos.

[Hecho] Odontogram, Treatments/Quotes, Billing, Documents y Dashboard dejan la clasificación `implemented but not formally accepted/reconciled` porque recibieron auditorías específicas y cierres formales mediante ADR 007/008/009/011 y sus documentos de evidencia.

[Hecho] Los slices visuales pueden mejorar presentación, organización, copy, color, microinteracciones, modales/drawers/tabs/sticky action bars y deuda UX sin cambiar backend, APIs, permissions, auth, tenant context, branch context, migrations ni alcance funcional.

[Hecho] El ajuste UX del cuestionario médico solicitado por el cliente quedó integrado mediante PR #1: opciones visibles `Sí / No / Sin respuesta`, avance de captura y preservación de `Unknown` como estado seguro.

## 4.2 Fase actual — Phase 2.1 Patient Intake and Portal Foundation

**Estado** — [Hecho] fase abierta con PI-1 y PI-2 completados. PI-2A a PI-2D quedaron cerrados con persistencia, APIs self-only, waiting-room scope, captura Angular completa y hardening de conflicto/expiración/navegación. PI-3 — Submit, Clinic Review and Canonical Apply y PI-4 — Audit Visibility and Security Hardening permanecen sin iniciar; abrir PI-3 requiere una decisión explícita separada.

**Ubicación** — [Hecho] fase actual posterior al MVP aceptado; PI-1 y PI-2 están cerrados. PI-3 no se abre automáticamente por este cierre y no existen todavía submit/review/apply ni writes canónicos desde el intake.

**Tracking** — [Hecho]

- ADR 006 — `docs/decisions/006-patient-intake-and-portal-foundation.md`.
- ADR 012 — `docs/decisions/012-patient-portal-access-baseline-and-phase-opening.md`.
- ADR 013 — `docs/decisions/013-patient-portal-invitation-management.md`.
- ADR 014 — `docs/decisions/014-patient-portal-authentication-and-session-boundary.md`.
- ADR 015 — `docs/decisions/015-patient-portal-frontend-session-boundary.md`.
- ADR 016 — `docs/decisions/016-patient-intake-draft-baseline.md`.
- ADR 017 — `docs/decisions/017-existing-patient-intake-api-and-waiting-room-bootstrap.md`.
- ADR 018 — `docs/decisions/018-patient-intake-waiting-room-link-management.md`.
- ADR 019 — `docs/decisions/019-patient-intake-only-authentication-boundary.md`.
- Plan PI-2D — `docs/pi-2d-angular-intake-capture-plan.md`.
- Cierre PI-2D3 — `docs/pi-2d3-medical-questionnaire-closure.md`.
- Cierre PI-2 — `docs/pi-2-patient-intake-capture-closure.md`.
- Cierre PI-1 — `docs/pi-1-patient-portal-access-and-security-closure.md`.
- Runbook — `docs/patient-portal-assisted-recovery-runbook.md`.
- Plan general — `docs/patient-intake-and-portal-plan.md`.
- Parent issue — #2.
- PI-1 Access and Invitation Foundation — issue #4.
- PI-1A Domain/Persistence — issue #22.
- PI-1B Staff Invitation Lifecycle — issue #23.
- PI-1C Patient Activation/Login — issue #24.
- PI-1D Patient Auth Frontend/Closure — issue #25.
- PI-2 Intake Draft — issue #5.
- PI-2A Domain/Persistence — issue #31 / PR #32.
- PI-2B Existing-Patient Self-Service — issue #33 / PR #34.
- PI-2C Waiting-Room Bootstrap — issue #35.
- PI-2C1 Credential/Staff API — issue #36.
- PI-2C2 Intake-Only Session — issue #37.
- PI-2C3 Staff Link/Print/QR UI — issue #38 / PR #43.
- PI-2D Angular Capture/Closure — issue #44.
- PI-2D1 Routes/Session/Data-Access — issue #45 / PR #50 — completado; hardening PR #51.
- PI-2D2 Demographics/Contact/Reason — issue #46.
- PI-2D3 Medical Questionnaire — issue #47.
- PI-2D4 Conflict/Expiry/Closure — issue #48.
- PI-3 Submit, Review and Apply — issue #6.
- PI-4 Audit Visibility and Hardening — issue #7.

**Baseline de acceso aprobado** — [Hecho]

- Activación single-use y password para accesos posteriores.
- `LoginName` único por tenant; teléfono/fecha de nacimiento/contacto no prueban ownership.
- TTL configurable: 24 horas para invitación existente y 30 minutos para link de sala de espera.
- Entrega piloto por recepción, sin proveedor automático.
- Lockout configurable de 5 intentos / 15 minutos.
- Recovery asistido mediante revocación de sesiones y reemisión de invitación.

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

1. Preservar PI-1 (#4) como foundation completado mediante PI-1A a PI-1D, sin acceso paciente a módulos canónicos.

2. Preservar PI-2B (#33 / PR #34) como API self-only completada para pacientes existentes y mantener sus contratos id-less, no-store, GET sin side effects y optimistic concurrency.

3. Implementar PI-2C solo mediante #36 → #37 → #38: credencial de 30 minutos, `patientportal.intake.manage`, scope `patient_intake` y UI staff mínima; PI-2D conserva la captura Angular del paciente.

4. Mantener guardado explícito, no-op sin revisión, expiración sliding de 30 días, `Unknown` distinto de `No` y revisiones append-only.

5. Mantener tokens de paciente solo en memoria; no introducir `localStorage`, refresh token ni recuperación remota sin una decisión posterior.

6. Mantener PI-3 y PI-4 no iniciados y prohibir aplicación canónica desde endpoints de paciente.

7. Preservar Releases 1 a 7 y el MVP aceptado sin debilitar tenant isolation, contratos ni boundaries cerrados.

8. Mantener payments, balances, receipts, cash management y fiscal/CFDI fuera del MVP aceptado hasta slices dedicados.

9. Mantener diferidas las `doctor-based views` y cualquier linkage cross-module no aceptado.

10. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS, roadmap, tenant model y ADRs en cada gate de PI-2.

## 12. Riesgos y temas a vigilar

**Tenant isolation** — [Hecho] Sigue siendo el riesgo estructural principal.

**Authorization model** — [Hecho] Permisos nuevos deben evolucionar junto con módulos reales, sin cambiar silenciosamente scopes o roles.

**Patient-facing identity e intake** — [Hecho] PI-1A a PI-1D establecen acceso separado. PI-2A agrega persistencia tenant-owned de draft, respuestas y revisiones. PI-2B expone create/get/save id-less para la cuenta vinculada y deriva Tenant/Patient/intake de la sesión validada, sin platform override ni aplicación canónica. PI-2C debe mantener separado el trust mode unlinked mediante `patient_intake`, token single-use y validación server-side de account/Tenant/intake/SessionVersion.

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

[Hecho] El orden completado del MVP es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Phase 2.1 Patient Intake and Portal Foundation está activa; PI-1, PI-2A y PI-2B están completados, PI-2C está activa y el portal amplio permanece en Phase 4.

[Hecho] Ningún release funcional ni Phase 2.1 se considera completado sin evidencia explícita en código, pruebas y documentación alineada.

[Hecho] Restricciones a preservar: modular monolith, ownership explícito, `TenantId` como boundary primario, `BranchId` subordinado, shared DB/schema, sin bypass oculto y sin autorización crítica solo en UI.

## 14. Nota tipo ADR resumida

**Estado:** Nota canónica actualizada con ADR 006 a ADR 017; Phase 2.1 abierta; PI-1, PI-2A y PI-2B completados; PI-2C activa.

**Contexto:** PI-2B expone por primera vez información médica declarada mediante una API patient-self. El cliente autorizó continuar exclusivamente con bootstrap de sala de espera, identidad intake-only y handoff staff mínimo.

**Decisión:** Aceptar PI-2B mediante PR #34 con contratos id-less/no-store y abrir PI-2C bajo ADR 017 como secuencia #36 → #37 → #38, usando credencial hash-only de 30 minutos, permiso TenantAdmin-only, scope `patient_intake` sin `patient_id` y QR generado localmente.

**Consecuencias:** Pacientes existentes ya pueden crear/leer/guardar su propio draft sin writes canónicos. Waiting-room bootstrap, UI staff de handoff y sesión intake-only siguen pendientes dentro de PI-2C; la captura Angular del cuestionario permanece PI-2D y submit/review/apply permanece PI-3.
