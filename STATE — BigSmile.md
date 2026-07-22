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

## 3. Fases completadas

[Hecho] Foundation / Release 0 base — completada.

[Hecho] Pre-auth hardening — completada.

[Hecho] Identity + Persistence Foundation — completada.

[Hecho] Tenant-Aware Authorization Foundation — completada.

[Hecho] Release 1 — Patients — completada.

[Hecho] Release 2 — Scheduling — completada.

[Hecho] Release 3 — Clinical Records — completada.

[Hecho] Release 4 — Odontogram — completada como release fundacional mediante Release 4.1, 4.2, 4.3 y 4.4.

[Hecho] El cierre formal de Release 2 cubre calendario diario/semanal branch-aware, creación/edición/reprogramación/cancelación, appointment notes, blocked slots y estados `Attended` / `NoShow`. `doctor-based views` permanece diferido porque requiere provider/doctor assignment.

[Hecho] El cierre formal de Release 3 cubre creación explícita de expediente clínico, snapshot base, alergias actuales, notas append-only, diagnósticos básicos, timeline clínica acotada, snapshot history, cuestionario médico fijo, consulta/signos vitales, atribución por usuario y protección con `clinical.read` / `clinical.write`.

[Hecho] El cierre formal de Release 4 se apoya en auditoría de dominio, aplicación, API, persistencia, permisos, frontend y pruebas para odontograma explícito, 32 dientes FDI permanentes adultos, estados de diente, superficies `O/M/D/B/L`, hallazgos básicos y change history append-only de hallazgos.

## 4. Fase actual

[Hecho] La última fase funcional marcada como completada es Release 4 — Odontogram.

[Hecho] Release 4 queda cerrada y preservada como release odontológica fundacional. Los slices Release 4.1 a Release 4.4 permanecen como evidencia aceptada de cierre.

[Hecho] La siguiente fase funcional prevista por el roadmap es Release 5 — Treatments and Quotes.

[Hecho] Release 5 no queda abierta por el cierre de Release 4. Cualquier aceptación debe realizarse mediante auditoría/slice explícito, con alcance, pruebas y documentación propios.

[Hecho] Phase 2 Expansion — Modern Operations pertenece al roadmap posterior al MVP operativo inicial. No debe abrirse antes de completar y aceptar Release 5, Release 6 y Release 7, salvo repriorización futura explícita y documentada.

[Hecho] Phase 2.1 — Patient Intake and Portal Foundation permanece planificada dentro de Phase 2. Su arquitectura y plan general están aceptados, pero PI-1 a PI-4 no están implementados ni abiertos como fase activa.

## 4.1 Nota de reconciliación UX / código existente

[Hecho] El repositorio contiene código funcional en módulos posteriores al estado formal, incluyendo Treatments/Quotes, Billing, Documents, Dashboard y recordatorios/manual reminders.

[Hecho] La presencia de código, rutas, permisos, migrations o tests no implica por sí misma aceptación o cierre. Hasta una auditoría específica, esos módulos se clasifican como `implemented but not formally accepted/reconciled`.

[Hecho] Odontogram deja esa clasificación porque recibió auditoría específica y cierre formal mediante ADR 007 y `docs/release-4-odontogram-audit-and-closure.md`.

[Hecho] Los slices visuales pueden mejorar presentación, organización, copy, color, microinteracciones, modales/drawers/tabs/sticky action bars y deuda UX sin cambiar backend, APIs, permissions, auth, tenant context, branch context, migrations ni alcance funcional.

[Hecho] El ajuste UX del cuestionario médico solicitado por el cliente quedó integrado mediante PR #1: opciones visibles `Sí / No / Sin respuesta`, avance de captura y preservación de `Unknown` como estado seguro.

## 4.2 Plan futuro — Phase 2.1 Patient Intake and Portal Foundation

**Estado** — [Hecho] decisión aceptada y trabajo planificado; implementación no iniciada.

**Ubicación** — [Hecho] primer capability acotado de Phase 2 — Modern Operations después del MVP inicial; no desplaza Release 5 como siguiente fase funcional actual.

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

**Nombre exacto** — [Hecho] Release 4 — Odontogram.

**Estado operativo actual** — [Hecho] completada como release odontológica fundacional.

**Evidencia de cierre** — [Hecho]

- Release 4.1 — Odontogram Foundation.
- Release 4.2 — Odontogram Surface Foundation.
- Release 4.3 — Basic Dental Findings Foundation.
- Release 4.4 — Dental Findings Change History.
- Auditoría — `docs/release-4-odontogram-audit-and-closure.md`.
- Decisión — ADR 007 `docs/decisions/007-release-4-odontogram-closure.md`.

**Alcance cerrado** — [Hecho]

- `Odontogram` tenant-owned y patient-owned.
- Exactamente uno por `TenantId + PatientId`.
- Creación explícita; `GET` devuelve `404` cuando falta; sin autocreación.
- 32 dientes permanentes adultos mediante FDI `11-18`, `21-28`, `31-38`, `41-48`.
- Estado de diente `Unknown` / `Healthy` / `Missing` / `Restored` / `Caries`.
- Cinco superficies `O/M/D/B/L` por diente.
- Estado de superficie `Unknown` / `Healthy` / `Restored` / `Caries`.
- Hallazgos básicos `Caries` / `Restoration` / `MissingStructure` / `Sealant`.
- Add/remove explícito de hallazgos y rechazo de duplicados exactos.
- Finding history append-only para add/remove, newest-first y separada de current findings.
- Metadata UTC y actor en agregado, dientes, superficies, hallazgos e historial.
- `odontogram.read` / `odontogram.write`; `TenantUser` sin permisos de Odontogram.
- UI Angular en contexto de paciente, con HTTP en data-access y orquestación en facade.
- Pruebas de contratos, validaciones, no autocreación, cross-tenant y frontend.

**Tenant safety** — [Hecho] El agregado raíz tiene filtro tenant-aware y writes centralizados. Las tablas hijas se consumen mediante `Odontogram`; cualquier query directa futura debe usar join tenant-aware o modelar ownership explícito.

**Fuera del cierre** — [Hecho]

- Dentición infantil o mixta.
- Bulk editing.
- Catálogo complejo/configurable de hallazgos.
- Linkage con diagnósticos, tratamientos, documentos o imágenes.
- Timeline dental completa.
- Historial completo de estados de diente/superficie.
- Restore/revert y versionado completo.
- Ortodoncia, periodoncia, overlays de imagen o AI-assisted detection.
- Acceso del patient portal al odontograma.

**Deuda UX no bloqueante** — [Hecho]

- Reemplazar copy interno como `Release 4.4` por lenguaje clínico.
- Migrar colores hardcodeados residuales a tokens `--bsm-*`.
- Seguir dividiendo componentes grandes solo mediante slices visuales acotados.

## 7. Releases previos cerrados

**Release 1 — Patients** — [Hecho] registro, actualización, búsqueda tenant-scoped, perfil, responsible party, estatus, alertas clínicas básicas y permisos `patient.read` / `patient.write`.

**Release 2 — Scheduling** — [Hecho] citas tenant-owned/branch-aware, calendario day/week, create/edit/reschedule/cancel, notes, blocked slots, `Attended` / `NoShow` y permisos `scheduling.read` / `scheduling.write`.

## 8. Backlog inmediato

Lista priorizada:

1. Preservar Releases 1 a 4 ya cerrados sin debilitar la fundación tenant-aware.

2. Auditar el código existente de `Release 5 — Treatments and Quotes` contra el roadmap y abrir/aceptar únicamente slices con evidencia explícita.

3. No tratar Treatments/Quotes como aceptado solo porque existen código, endpoints, permissions, migrations o tests.

4. Mantener diferidas las `doctor-based views` hasta un slice dedicado de provider/doctor assignment.

5. Mantener fuera de los agregados cerrados cualquier linkage cross-module no aceptado.

6. Si se consultan directamente tablas hijas de Clinical u Odontogram, usar join tenant-aware o modelarlas como tenant-owned.

7. Mantener Phase 2.1 visible mediante ADR 006, su plan e issues #2/#4/#5/#6/#7, sin iniciar PI-1 antes del gate de MVP o repriorización explícita.

8. Mantener sincronizados STATE, README, PROJECT_MAP, AGENTS y product-roadmap cuando cambie el estado de Release 5 o Phase 2.1.

## 9. Riesgos y temas a vigilar

**Tenant isolation** — [Hecho] Sigue siendo el riesgo estructural principal.

**Authorization model** — [Hecho] Permisos nuevos deben evolucionar junto con módulos reales, sin cambiar silenciosamente scopes o roles.

**Patient-facing identity** — [Hecho] La futura frontera pública no debe reutilizar staff membership, aceptar `PatientId`/`TenantId` como autoridad, permitir platform override ni aplicar cambios canónicos sin revisión.

**Query filters y acceso a datos** — [Hecho] No degradar filtros globales y write enforcement con filtros manuales dispersos.

**Clinical/Odontogram child tables** — [Hecho] Los accesos directos futuros necesitan tenant-aware joins u ownership explícito.

**Privileged/platform paths** — [Hecho] Toda operación fuera de tenant scope normal debe ser explícita, mínima y auditable.

**Clinical provenance** — [Hecho] Las declaraciones de pacientes no equivalen a observaciones profesionales.

**UX operativa** — [Hecho] Seguridad y estructura no deben degradar velocidad ni claridad.

**Alineación documental futura** — [Hecho] Cada apertura/cierre debe actualizar código, pruebas y documentación en el mismo cambio.

## 10. Criterios para no perder el rumbo

[Hecho] BigSmile es un producto SaaS multi-tenant; cualquier atajo que debilite tenant isolation, mantenibilidad o reviewability rompe el rumbo.

[Hecho] El orden actual es Foundation → Patients → Scheduling → Clinical Records → Odontogram → Treatments and Quotes → Billing → Documents and Dashboard. Después del MVP, Phase 2.1 cubre el bounded Patient Intake and Portal Foundation; el portal amplio permanece en Phase 4.

[Hecho] Ningún release funcional ni Phase 2.1 se considera completado sin evidencia explícita en código, pruebas y documentación alineada.

[Hecho] Restricciones a preservar: modular monolith, ownership explícito, `TenantId` como boundary primario, `BranchId` subordinado, shared DB/schema, sin bypass oculto y sin autorización crítica solo en UI.

## 11. Nota tipo ADR resumida

**Estado:** Nota canónica actualizada con ADR 006 y ADR 007.

**Contexto:** El código de Odontogram estaba implementado pero clasificado correctamente como no reconciliado hasta una auditoría específica.

**Decisión:** Cerrar Release 4 mediante slices 4.1 a 4.4, preservar su alcance fundacional y mover la frontera del roadmap a Release 5. Mantener Patient Intake and Portal como Phase 2.1 posterior al MVP mediante ADR 006.

**Consecuencias:** Treatments and Quotes es el siguiente módulo a auditar; no se acepta automáticamente. El alcance avanzado de Odontogram permanece diferido y la deuda visual se atiende mediante slices UX separados.
