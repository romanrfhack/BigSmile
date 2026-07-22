# BigSmile — Estado actual canónico

## 1. Resumen ejecutivo

[Hecho] BigSmile es una plataforma SaaS para la gestión de clínicas y consultorios dentales, concebida desde el inicio como producto comercial multi-tenant y no como sistema interno de una sola clínica.

[Hecho] El objetivo del producto es cubrir el backbone operativo de la clínica en un flujo continuo: cita → expediente/paciente → expediente clínico → odontograma → plan de tratamiento → cotización → pago → seguimiento, preservando seguridad, mantenibilidad y UX operativa rápida.

[Hecho] Stack principal: backend .NET 10 + ASP.NET Core Web API + EF Core + SQL Server; frontend Angular 21 + TypeScript; calidad/operación con GitHub Actions, pruebas automatizadas, logging estructurado, health checks y auditoría.

[Hecho] Modelo SaaS: Tenant = clínica/consultorio cliente; Branch = sucursal/ubicación interna del tenant. TenantId es la frontera primaria de seguridad y propiedad; BranchId es scope operativo subordinado, no el boundary principal.

## 2. Decisiones arquitectónicas cerradas

**Arquitectura general** — [Hecho] BigSmile arranca como modular monolith, con fronteras explícitas de backend (Api / Application / Domain / Infrastructure / SharedKernel) y frontend (core / shell / shared / features). No hay decisión de microservicios para la etapa inicial.

**Multitenancy** — [Hecho] La estrategia base es shared database + shared schema + TenantId como discriminador transversal, con TenantContext por request, enforcement centralizado, y bypass solo para operaciones de plataforma, explícito y auditable.

**Auth** — [Hecho] La base de identidad y autenticación ya está establecida sobre JWT y la fundación tenant-aware de autorización quedó formalizada: claims de scope/permiso, TenantContext enriquecido, policies/handlers por scope, `/api/auth/me`, override de plataforma explícito y auditable, y soporte mínimo de frontend para consumir el contexto actual sin persistencia insegura en navegador.

**Persistencia** — [Hecho] La persistencia base es EF Core sobre SQL Server, con AppDbContext, migraciones, seed durable y validación de login real contra base SQL Server. Tenant, Branch e identidad comparten la misma ruta de persistencia durable. BranchId solo se usa cuando el dominio lo requiere, subordinado a TenantId.

**Frontend** — [Hecho] El frontend queda fijado como feature-based, con separación entre páginas, componentes, facades, data-access y modelos; se prohíbe dispersar llamadas HTTP en componentes/páginas y se prioriza UX operativa rápida para recepción y clínica.

**Patient Intake and Portal Foundation** — [Hecho] ADR 004 queda aceptado como decisión futura: la identidad de paciente será separada del acceso interno de personal; los pacientes no reutilizarán `TenantUser`, `UserTenantMembership` ni permisos tenant-wide; pacientes existentes se activarán por invitación single-use y pacientes nuevos usarán enlace/QR tenant-scoped; la información seguirá `Draft -> Submitted -> Reviewed -> Applied/Rejected`; la aplicación canónica requerirá revisión de la clínica; y los cambios tendrán bitácora append-only. Esta decisión queda planificada como Phase 2.1 después del MVP inicial, no abre implementación actual.

## 3. Fases completadas

[Hecho] Foundation / Release 0 base — completada.

[Hecho] Pre-auth hardening — completada.

[Hecho] Identity + Persistence Foundation — completada.

[Hecho] Tenant-Aware Authorization Foundation — completada.

[Hecho] Release 1 — Patients — completada.

[Hecho] Release 2 — Scheduling — completada.

[Hecho] Release 3 — Clinical Records — completada.

[Hecho] El cierre formal de Release 2 se apoya en evidencia de código, pruebas, revisión de release y documentación alineada para calendario diario/semanal branch-aware, creación/edición/reprogramación/cancelación de citas, appointment notes, blocked slots y estados `Attended` / `NoShow`.

[Hecho] `doctor-based views` quedó explícitamente diferido por decisión documentada a un slice futuro acotado; no forma parte del cierre efectivo de Release 2.

[Hecho] El cierre formal de Release 3 se apoya en auditoría de código, pruebas y documentación del alcance aceptado Release 3.1 a Release 3.6: creación explícita de expediente clínico, snapshot base, alergias actuales, notas append-only, diagnósticos básicos, timeline clínica acotada, snapshot history, cuestionario médico fijo basado en el formato físico, consulta clínica/signos vitales, atribución por usuario y protección con `clinical.read` / `clinical.write` sin TenantId desde cliente.

## 4. Fase actual

[Hecho] La última fase funcional marcada como completada es Release 3 — Clinical Records.

[Hecho] Release 3 — Clinical Records queda cerrada y preservada como release clínico fundacional. Los slices Release 3.1 a Release 3.6 permanecen como evidencia aceptada de cierre.

[Hecho] La siguiente fase funcional prevista por el roadmap es Release 4 — Odontogram.

[Hecho] Release 4 — Odontogram no queda abierta por este cierre documental. Cualquier implementación o aceptación de Release 4 debe realizarse en un slice futuro explícito, con alcance, pruebas y documentación propios.

[Hecho] Phase 2 Expansion — Modern Operations pertenece al roadmap posterior al MVP operativo inicial. No debe tratarse como siguiente paso inmediato después de cerrar Release 3, ni antes de completar y aceptar Release 4, Release 5, Release 6 y Release 7 según `docs/product-roadmap.md`.

[Hecho] Phase 2.1 — Patient Intake and Portal Foundation queda planificada dentro de Phase 2, después del MVP inicial. Su arquitectura y plan general están aceptados, pero PI-1 a PI-4 no están implementados ni abiertos como fase activa.

## 4.1 Nota de reconciliación UX / código existente

[Hecho] El trabajo activo posterior al cierre de Release 3 debe tratarse como `client-driven UX redesign / visual organization pass` mientras no se abra explícitamente un nuevo slice funcional.

[Hecho] El repositorio contiene código funcional existente en módulos posteriores al roadmap formal, incluyendo Odontogram, Treatments/Quotes, Billing, Documents, Dashboard y recordatorios/manual reminders. La presencia de código, rutas, permisos, migrations o tests en esos módulos no implica por sí misma que Release 4, Release 5, Release 6, Release 7 o Phase 2 estén abiertas, aceptadas o cerradas.

[Hecho] Hasta una auditoría y aceptación específica por módulo, esos módulos posteriores deben clasificarse como `implemented but not formally accepted/reconciled`.

[Hecho] Los slices visuales pueden mejorar presentación, organización, copy, color, microinteracciones, modales/drawers/tabs/sticky action bars y deuda UX sin cambiar backend, APIs, permissions, auth, tenant context, branch context, migrations ni scope funcional.

[Hecho] El ajuste UX del cuestionario médico solicitado por el cliente quedó integrado mediante PR #1: conserva el catálogo y contratos existentes, reemplaza dropdowns por opciones visibles `Sí / No / Sin respuesta`, agrega avance de captura y mantiene `Unknown` como estado seguro. No abrió patient self-service ni cambió backend, permisos o tenant isolation.

## 4.2 Plan futuro — Phase 2.1 Patient Intake and Portal Foundation

**Estado** — [Hecho] decisión aceptada y trabajo planificado; implementación no iniciada.

**Ubicación** — [Hecho] primer capability acotado de Phase 2 — Modern Operations después del MVP inicial; no desplaza Release 4 como siguiente fase funcional actual.

**Tracking** — [Hecho]

- ADR 004 — `docs/decisions/004-patient-intake-and-portal-foundation.md`.
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

**Nombre exacto** — [Hecho] Release 3 — Clinical Records.

**Estado operativo actual** — [Hecho] completada como release clínico fundacional.

**Evidencia de cierre** — [Hecho] Release 3 queda cerrada mediante los slices aceptados:

- Release 3.1 — Clinical Record Foundation.
- Release 3.2 — Basic Diagnoses Foundation.
- Release 3.3 — Clinical Timeline Read Model.
- Release 3.4 — Clinical Snapshot Change History.
- Release 3.5 — Medical Questionnaire Backend.
- Release 3.6 — Clinical Encounter / Vitals Backend.

**Alcance cerrado** — [Hecho]

- `ClinicalRecord` tenant-owned y patient-owned.
- Exactamente un expediente clínico activo por Patient por Tenant.
- Creación explícita vía `POST /api/patients/{patientId}/clinical-record`.
- `GET /api/patients/{patientId}/clinical-record` devuelve `404` cuando no existe.
- Sin autocreación por lectura, notas, diagnósticos, cuestionario o encounters.
- Snapshot base con medical background summary, current medications summary y alergias actuales.
- Clinical notes append-only con orden newest-first.
- Diagnósticos básicos no codificados con alta y resolución explícitas.
- Timeline clínica acotada, construida solo desde notas y diagnósticos/resoluciones, sin endpoint nuevo y sin tabla nueva.
- Snapshot history acotado, separado de la timeline.
- Cuestionario médico estructurado con catálogo fijo `QuestionKey`, respuestas `Unknown` / `Yes` / `No` y `Details` opcional acotado.
- Consulta clínica/signos vitales mediante `ClinicalEncounter` sobre un expediente existente.
- `ChiefComplaint`, `ConsultationType` `Treatment` / `Urgency` / `Other`, signos vitales opcionales y `noteText` opcional como nota append-only vinculada.
- `TenantId` y `CreatedByUserId` derivados del contexto, no aceptados desde cliente en los contratos nuevos.
- Reutilización de `clinical.read` / `clinical.write`, sin permisos nuevos.
- UI acotada dentro de `features/clinical-records`, con HTTP en data-access y orquestación por facade.
- Contexto read-only de Patient para identidad/demografía, incluyendo edad derivada desde `Patient.DateOfBirth` sin persistir edad ni agregar `Age` al backend.

**Acceso clínico** — [Hecho] En esta fase, `clinical.read` y `clinical.write` se conceden a `PlatformAdmin` y `TenantAdmin`; `TenantUser` no recibe permisos clínicos.

**Fuera del cierre** — [Hecho]

- Full o advanced patient clinical timeline.
- Timeline cross-module.
- Restore/revert de snapshot history.
- Versionado completo del expediente clínico.
- Rich snapshot diff.
- Form builder configurable por tenant.
- Sincronización automática de alergias desde questionnaire.
- Eventos de questionnaire o encounter en timeline.
- Edición/borrado de encounters.
- Doctor/provider assignment.
- Scheduling, Billing, Odontogram, Treatments o Documents.
- Patient self-service, portal e intake/review workflow.

**Campos del formato físico fuera del cierre clínico** — [Hecho]

- Teléfonos separados casa/oficina/celular pertenecen a un futuro mini-slice Patient Contact Details o a campos propuestos del intake sujetos a revisión; no deben duplicarse sin decisión de ownership.
- `Requiere factura` y datos de facturación pertenecen a Billing/fiscal profile futuro, no a Clinical Records.

## 6. Releases previos cerrados

**Release 1 — Patients** — [Hecho] cubre registro, actualización, búsqueda tenant-scoped, perfil básico, responsible party inline, estatus activo/inactivo, basic clinical alerts, validación de fecha de nacimiento futura y permisos `patient.read` / `patient.write`.

**Release 2 — Scheduling** — [Hecho] cubre citas tenant-owned y branch-aware, calendario diario/semanal, creación/edición/reprogramación/cancelación, appointment notes, blocked slots, estados `Attended` / `NoShow`, permisos `scheduling.read` / `scheduling.write` y diferimiento explícito de doctor-based views.

## 7. Backlog inmediato

Lista priorizada:

1. Preservar Releases 1, 2 y 3 ya cerrados sin debilitar la fundación tenant-aware ya cerrada.

2. Preparar el siguiente slice funcional de `Release 4 — Odontogram` solo cuando se abra explícitamente, manteniendo tenant safety, permisos acotados y documentación alineada.

3. Mantener diferidas las `doctor-based views` hasta abrir un slice dedicado de provider/doctor assignment; no reintroducirlas como parche incidental de UI.

4. Mantener fuera de Clinical Records los follow-ups de Patient Contact Details, Billing/fiscal profile y cualquier scope de Odontogram/Treatments/Documents/Patient Portal.

5. Considerar como hardening opcional futuro el rechazo de JSON con miembros no mapeados en DTOs clínicos antiguos, sin cambiar permisos ni contratos funcionales sin slice explícito.

6. Si en el futuro se consultan directamente tablas hijas clínicas, esas queries deben usar join tenant-aware o modelarse como tenant-owned.

7. Mantener Phase 2.1 visible y trazable mediante ADR 004, `docs/patient-intake-and-portal-plan.md` e issues #2/#4/#5/#6/#7, sin iniciar PI-1 antes del gate de MVP o de una repriorización futura explícita.

8. Mantener sincronizados STATE — BigSmile.md, README.md, PROJECT_MAP.md, AGENTS.md y docs/product-roadmap.md cuando avance el estado del proyecto o se abra Phase 2.1.

## 8. Riesgos y temas a vigilar

**Tenant isolation** — [Hecho] Sigue siendo el riesgo estructural principal: cualquier fuga cross-tenant por modelado incompleto, resolución insegura de tenant, filtros incompletos o bypass silencioso compromete la frontera primaria de seguridad.

**Authorization model** — [Hecho] La fundación ya quedó cerrada y probada por membership, scope y permisos; el catálogo inicial seguirá evolucionando por módulo sin reabrir la forma base del modelo.

**Patient-facing identity** — [Hecho] La futura frontera pública es de alto riesgo: no debe reutilizar staff membership, aceptar PatientId/TenantId como autoridad desde cliente, permitir platform override, exponer búsqueda pública de expedientes ni aplicar cambios canónicos sin revisión.

**Query filters y acceso a datos** — [Hecho] Los filtros globales de tenant y el enforcement centralizado siguen siendo críticos; el riesgo es degradarlos con filtros manuales dispersos, accesos ad hoc o bypasses no controlados.

**Clinical child tables** — [Hecho] Los hijos clínicos históricos se consumen a través de `ClinicalRecord`; cualquier acceso directo futuro debe conservar tenant safety mediante join tenant-aware o modelado tenant-owned.

**Privileged/platform paths** — [Hecho] Toda operación fuera del tenant scope normal debe ser explícita, mínima, segregada y auditable.

**Clinical provenance** — [Hecho] Las declaraciones del paciente no equivalen a observaciones profesionales. Intake, revisión, aplicación y actor deben permanecer diferenciados y auditables.

**UX operativa** — [Hecho] La seguridad y la estructura técnica no deben degradar la velocidad ni la claridad de los flujos núcleo.

**Alineación documental futura** — [Hecho] El riesgo ahora es reintroducir desalineación cuando avance una fase o cambie una decisión sin actualizar STATE y documentación base en el mismo cambio.

## 9. Criterios para no perder el rumbo

[Hecho] No olvidar que BigSmile es producto SaaS multi-tenant para clínicas, no una solución one-off; cualquier atajo que debilite tenant isolation, mantenibilidad o reviewabilidad rompe el rumbo.

[Hecho] El orden del producto importa: primero foundation, luego Patients, Scheduling, Clinical Records, Odontogram, Treatments and Quotes, Billing, Documents and Dashboard. Después del MVP inicial, Phase 2.1 cubre el bounded Patient Intake and Portal Foundation; el patient portal amplio permanece en Phase 4 junto con capacidades avanzadas.

[Hecho] Phase 2 ocurre después del MVP operativo, no inmediatamente después de Release 3, salvo una repriorización futura explícita y documentada.

[Hecho] Restricciones de diseño a preservar: modular monolith; capas y ownership explícitos; TenantId como boundary primario; BranchId subordinado; shared DB/schema inicial; sin microservicios ni DB por tenant en esta etapa; sin bypass oculto de plataforma; sin lógica crítica de negocio/autorización solo en UI.

[Hecho] Ningún release funcional ni Phase 2.1 debe tratarse como completado sin evidencia explícita en código, pruebas y documentación alineada.

## 10. Nota tipo ADR resumida

**Estado:** Aceptada como nota canónica operativa; actualizada con ADR 004 y el plan futuro de Patient Intake and Portal.

**Contexto:** BigSmile es un SaaS multi-tenant para clínicas dentales, con arquitectura modular monolith, Tenant como frontera primaria de seguridad, Branch como scope operativo subordinado y una base fundacional ya establecida más allá de bootstrap. El cliente confirmó la necesidad futura de registro/intake por pacientes nuevos y actualización por pacientes existentes con revisión clínica y bitácora.

**Decisión:** Tratar como cerradas Foundation / Release 0 base, Pre-auth hardening, Identity + Persistence Foundation, Tenant-Aware Authorization Foundation, Release 1 — Patients, Release 2 — Scheduling y Release 3 — Clinical Records. Tratar Release 3.1 a Release 3.6 como evidencia aceptada de cierre. Tratar Release 4 — Odontogram como siguiente fase funcional prevista. Aceptar ADR 004 y ubicar Patient Intake and Portal Foundation en Phase 2.1 después del MVP inicial, sin abrir implementación actual ni el patient portal amplio.

**Consecuencias:** La prioridad inmediata continúa siendo preservar Releases 1 a 3 y preparar Release 4 solo mediante slices explícitos. Patient Intake and Portal queda completamente visible y descompuesto en PI-1 a PI-4, pero no se implementa antes de su gate. Toda implementación futura deberá mantener identidad separada, self-scope, revisión antes de aplicación canónica, tenant isolation y bitácora append-only.