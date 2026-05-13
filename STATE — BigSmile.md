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

[Hecho] Phase 2 Expansion — Modern Operations pertenece al roadmap posterior al MVP operativo inicial. No debe tratarse como siguiente paso inmediato después de cerrar Release 3, ni antes de completar Release 4, Release 5, Release 6 y Release 7 según `docs/product-roadmap.md`.

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

**Campos del formato físico fuera del cierre clínico** — [Hecho]

- Teléfonos separados casa/oficina/celular pertenecen a un futuro mini-slice Patient Contact Details, solo si el cliente lo confirma.
- `Requiere factura` y datos de facturación pertenecen a Billing/fiscal profile futuro, no a Clinical Records.

## 6. Releases previos cerrados

**Release 1 — Patients** — [Hecho] cubre registro, actualización, búsqueda tenant-scoped, perfil básico, responsible party inline, estatus activo/inactivo, basic clinical alerts, validación de fecha de nacimiento futura y permisos `patient.read` / `patient.write`.

**Release 2 — Scheduling** — [Hecho] cubre citas tenant-owned y branch-aware, calendario diario/semanal, creación/edición/reprogramación/cancelación, appointment notes, blocked slots, estados `Attended` / `NoShow`, permisos `scheduling.read` / `scheduling.write` y diferimiento explícito de doctor-based views.

## 7. Backlog inmediato

Lista priorizada:

1. Preservar Releases 1, 2 y 3 ya cerrados sin debilitar la fundación tenant-aware ya cerrada.

2. Preparar el siguiente slice funcional de `Release 4 — Odontogram` solo cuando se abra explícitamente, manteniendo tenant safety, permisos acotados y documentación alineada.

3. Mantener diferidas las `doctor-based views` hasta abrir un slice dedicado de provider/doctor assignment; no reintroducirlas como parche incidental de UI.

4. Mantener fuera de Clinical Records los follow-ups de Patient Contact Details, Billing/fiscal profile y cualquier scope de Odontogram/Treatments/Documents.

5. Considerar como hardening opcional futuro el rechazo de JSON con miembros no mapeados en DTOs clínicos antiguos, sin cambiar permisos ni contratos funcionales sin slice explícito.

6. Si en el futuro se consultan directamente tablas hijas clínicas, esas queries deben usar join tenant-aware o modelarse como tenant-owned.

7. Mantener sincronizados STATE — BigSmile.md, README.md, PROJECT_MAP.md, AGENTS.md y docs/product-roadmap.md cuando avance el estado del proyecto.

## 8. Riesgos y temas a vigilar

**Tenant isolation** — [Hecho] Sigue siendo el riesgo estructural principal: cualquier fuga cross-tenant por modelado incompleto, resolución insegura de tenant, filtros incompletos o bypass silencioso compromete la frontera primaria de seguridad.

**Authorization model** — [Hecho] La fundación ya quedó cerrada y probada por membership, scope y permisos; el catálogo inicial seguirá evolucionando por módulo sin reabrir la forma base del modelo.

**Query filters y acceso a datos** — [Hecho] Los filtros globales de tenant y el enforcement centralizado siguen siendo críticos; el riesgo es degradarlos con filtros manuales dispersos, accesos ad hoc o bypasses no controlados.

**Clinical child tables** — [Hecho] Los hijos clínicos históricos se consumen a través de `ClinicalRecord`; cualquier acceso directo futuro debe conservar tenant safety mediante join tenant-aware o modelado tenant-owned.

**Privileged/platform paths** — [Hecho] Toda operación fuera del tenant scope normal debe ser explícita, mínima, segregada y auditable.

**UX operativa** — [Hecho] La seguridad y la estructura técnica no deben degradar la velocidad ni la claridad de los flujos núcleo.

**Alineación documental futura** — [Hecho] El riesgo ahora es reintroducir desalineación cuando avance una fase o cambie una decisión sin actualizar STATE y documentación base en el mismo cambio.

## 9. Criterios para no perder el rumbo

[Hecho] No olvidar que BigSmile es producto SaaS multi-tenant para clínicas, no una solución one-off; cualquier atajo que debilite tenant isolation, mantenibilidad o reviewabilidad rompe el rumbo.

[Hecho] El orden del producto importa: primero foundation, luego Patients, Scheduling, Clinical Records, Odontogram, Treatments and Quotes, Billing, Documents and Dashboard; después vendrán recordatorios, online booking, facturación electrónica, patient portal, analytics y automatizaciones.

[Hecho] Phase 2 ocurre después del MVP operativo, no inmediatamente después de Release 3.

[Hecho] Restricciones de diseño a preservar: modular monolith; capas y ownership explícitos; TenantId como boundary primario; BranchId subordinado; shared DB/schema inicial; sin microservicios ni DB por tenant en esta etapa; sin bypass oculto de plataforma; sin lógica crítica de negocio/autorización solo en UI.

[Hecho] Ningún release funcional del roadmap debe tratarse como completado sin evidencia explícita en código y documentación alineada.

## 10. Nota tipo ADR resumida

**Estado:** Aceptada como nota canónica operativa; reconciliada con README.md, PROJECT_MAP.md, AGENTS.md y docs/product-roadmap.md.

**Contexto:** BigSmile es un SaaS multi-tenant para clínicas dentales, con arquitectura modular monolith, Tenant como frontera primaria de seguridad, Branch como scope operativo subordinado y una base fundacional ya establecida más allá de bootstrap.

**Decisión:** Tratar como cerradas Foundation / Release 0 base, Pre-auth hardening, Identity + Persistence Foundation, Tenant-Aware Authorization Foundation, Release 1 — Patients, Release 2 — Scheduling y Release 3 — Clinical Records. Tratar Release 3.1 a Release 3.6 como evidencia aceptada de cierre. Tratar Release 4 — Odontogram como siguiente fase funcional prevista, no abierta por este cierre. Tratar Phase 2 Expansion — Modern Operations como roadmap posterior al MVP, no como siguiente paso inmediato después de Release 3.

**Consecuencias:** La prioridad inmediata pasa a ser preservar el cierre de Patients, Scheduling y Clinical Records, preparar Release 4 solo mediante slices explícitos, mantener Clinical Records fuera de Billing/Scheduling/Odontogram/Treatments/Documents y mantener sincronizados STATE y documentación base cada vez que cambie el estado del proyecto.
