# BigSmile — Estado actual canónico

## 1. Resumen ejecutivo

[Hecho] BigSmile es una plataforma SaaS para la gestión de clínicas y consultorios dentales, concebida desde el inicio como producto comercial multi-tenant y no como sistema interno de una sola clínica.

[Hecho] El objetivo del producto es cubrir el backbone operativo de la clínica en un flujo continuo: cita → expediente/paciente → expediente clínico → odontograma → plan de tratamiento → cotización → pago → seguimiento, preservando seguridad, mantenibilidad y UX operativa rápida.

[Hecho] Stack principal: backend .NET 10 + ASP.NET Core Web API + EF Core + SQL Server; frontend Angular 21 + TypeScript; calidad/operación con GitHub Actions, pruebas automatizadas, logging estructurado, health checks y auditoría.

[Hecho] Modelo SaaS: Tenant = clínica/consultorio cliente; Branch = sucursal/ubicación interna del tenant. TenantId es la frontera primaria de seguridad y propiedad; BranchId es scope operativo subordinado, no el boundary principal.

## 2. Decisiones arquitectónicas cerradas

**Arquitectura general** — [Hecho] BigSmile arranca como modular monolith, con fronteras explícitas de backend (Api / Application / Domain / Infrastructure / SharedKernel) y frontend (core / shell / shared / features). No hay decisión de microservicios para la etapa inicial.

**Multitenancy** — [Hecho] La estrategia base es shared database + shared schema + TenantId como discriminador transversal, con TenantContext por request, enforcement centralizado, y bypass solo para operaciones de plataforma, explícito y auditable.

**Auth** — [Hecho] La base de identidad y autenticación ya está establecida sobre JWT y ahora la fundación tenant-aware de autorización quedó formalizada: claims de scope/permiso, `TenantContext` enriquecido, policies/handlers por scope, `/api/auth/me`, override de plataforma explícito y auditable, y soporte mínimo de frontend para consumir el contexto actual sin persistencia insegura en navegador.

**Persistencia** — [Hecho] La persistencia base es EF Core sobre SQL Server, con AppDbContext, migraciones, seed durable y validación de login real contra base SQL Server. Tenant, Branch e identidad ya comparten la misma ruta de persistencia durable. BranchId solo se usa cuando el dominio lo requiere, subordinado a TenantId.

**Frontend** — [Hecho] El frontend queda fijado como feature-based, con separación entre páginas, componentes, facades, data-access y modelos; se prohíbe dispersar llamadas HTTP en componentes/páginas y se prioriza UX operativa rápida para recepción y clínica.

**Principios no negociables** — [Hecho] Tenant isolation no negociable; Branch no reemplaza a Tenant; no usar filtrado manual disperso como estrategia principal; producto antes que app ad hoc; cambios pequeños, auditables y reversibles; arquitectura mantenible; UX operativa como feature; no introducir complejidad distribuida prematura.

## 3. Fases completadas

[Hecho] Foundation / Release 0 base — completada.

[Hecho] Pre-auth hardening — completada.

[Hecho] Identity + Persistence Foundation — completada.

[Hecho] Tenant-Aware Authorization Foundation — completada.

[Hecho] Release 1 — Patients — completada.

[Hecho] Release 2 — Scheduling — completada.

[Hecho] La corrección de consistencia de persistencia quedó absorbida dentro de Identity + Persistence Foundation: tenant y branch ya no viven en un path in-memory separado, el seed es durable y el login real contra SQL Server ya fue validado.

[Hecho] El cierre formal de Release 2 se apoya en evidencia de código, pruebas, revisión de release y documentación alineada para calendario diario/semanal branch-aware, creación/edición/reprogramación/cancelación de citas, appointment notes, blocked slots y estados `Attended` / `NoShow`.

[Hecho] `doctor-based views` quedó explícitamente diferido por decisión documentada a un slice futuro acotado; no forma parte del cierre efectivo de Release 2.

[Hecho] No existe evidencia canónica de cierre para los releases funcionales posteriores a Release 3.2 — Basic Diagnoses Foundation; por tanto, Release 3 — Clinical Records no debe asumirse como cerrada y Odontogram, Treatments and Quotes, Billing y Documents/Dashboard no deben asumirse como implementados o cerrados salvo evidencia explícita en código y documentación alineada.

## 4. Fase actual

[Hecho] La última fase marcada como completada sigue siendo Release 2 — Scheduling.

[Hecho] Release 2 — Scheduling ya quedó cerrada con el alcance roadmap reconciliado, confirmado en código, pruebas, revisión de release y documentación alineada.

[Hecho] README.md, PROJECT_MAP.md, AGENTS.md y docs/product-roadmap.md ya fueron reconciliados con este estado canónico y ubican al repositorio más allá del bootstrap / early foundation stage.

[Hecho] Release 3 — Clinical Records ya fue abierta y está en progreso.

[Hecho] Los slices aceptados de Release 3 son Release 3.1 — Clinical Record Foundation y Release 3.2 — Basic Diagnoses Foundation.

[Hecho] Release 3.1 cubre, de forma acotada:
- `ClinicalRecord` tenant-owned y patient-owned
- exactamente 1 expediente activo por Patient por Tenant
- creación explícita vía `POST /api/patients/{patientId}/clinical-record`
- `GET /api/patients/{patientId}/clinical-record` devuelve `404` si no existe
- no hay autocreación ni por GET ni por agregar nota
- snapshot base con medical background summary y current medications summary
- lista actual de alergias
- clinical notes append-only con orden newest-first en lectura/UI
- clinician attribution mínima y metadata básica de auditoría

[Hecho] Release 3.2 cubre, de forma acotada:
- diagnósticos básicos dentro de un `ClinicalRecord` existente
- alta explícita vía `POST /api/patients/{patientId}/clinical-record/diagnoses`
- resolución explícita vía `POST /api/patients/{patientId}/clinical-record/diagnoses/{diagnosisId}/resolve`
- enriquecimiento de `GET /api/patients/{patientId}/clinical-record` para incluir diagnósticos
- sin autocreación del expediente clínico
- diagnósticos básicos y no codificados
- estados mínimos `Active` / `Resolved`
- orden `active-first` y `newest-first` dentro de cada grupo en lectura/UI

[Hecho] En esta fase, `clinical.read` y `clinical.write` se conceden a `PlatformAdmin` y `TenantAdmin`; `TenantUser` no recibe permisos clínicos.

[Hecho] Release 3 completa no está cerrada. La timeline clínica completa, odontogram, treatments y documents siguen fuera de los slices aceptados actuales; los diagnósticos ya entraron solo en forma básica y no codificada dentro de Release 3.2.

[Hecho + Inferencia operativa] El proyecto ya no está solo “listo para abrir” Clinical Records; ahora está dentro de Release 3 con un primer slice aceptado sobre una base que ya incluye Patients y Scheduling cerrados.

[Hecho combinado] Lo ya establecido a nivel fundacional incluye, como mínimo:
- estructura de solución
- baseline multi-tenant
- tenant/branch context
- auth base
- persistencia EF Core + SQL Server
- migraciones y seed durable
- login real validado contra base SQL Server
- error handling
- auditing/logging base
- validación arquitectónica
- CI
- pruebas backend y frontend de baseline

[Hecho] El primer slice de Patients implementado hasta ahora cubre:
- registro de paciente
- actualización de paciente
- búsqueda tenant-scoped de pacientes
- consulta de perfil básico
- responsible party inline
- estatus activo/inactivo
- persistencia EF Core + migración inicial del módulo
- endpoints y permisos explícitos `patient.read` / `patient.write`

[Hecho] Sobre ese slice inicial ya quedó completado el siguiente sub-slice mínimo de Patients:
- basic clinical alerts
- `HasClinicalAlerts` en paciente
- `ClinicalAlertsSummary` como texto corto y acotado
- visibilidad de estado de alertas en búsqueda/listado
- soporte en alta/edición/perfil
- limpieza automática del summary cuando `HasClinicalAlerts = false`

[Hecho] La corrección mínima final de cierre para Release 1 quedó absorbida sin ampliar scope:
- validación frontend para impedir fecha de nacimiento futura en el formulario de pacientes
- prueba frontend enfocada para ese guardrail operativo
- validación backend existente preservada sin cambios

[Hecho] Patients sigue manteniendo el modelo branch-neutral y tenant-owned; este avance no abre aún expediente clínico, diagnósticos, historia clínica, timeline, adjuntos, scheduling ni otros módulos posteriores.

[Hecho] Con esa corrección mínima final, Release 1 — Patients ya tiene evidencia suficiente de cierre sin abrir sub-slices adicionales ni debilitar tenant isolation.

## 5. Release 2 — Scheduling

**Nombre exacto** — [Hecho] Release 2 — Scheduling.

**Objetivo** — [Hecho + Inferencia alineada a docs] Abrir Scheduling solo sobre una base ya endurecida de aislamiento tenant-aware, authz por scope/membership/permiso y un módulo Patients ya cerrado.

**Estado operativo actual** — [Hecho] Release 2 quedó formalmente cerrada tras absorber los slices mínimos aceptados y reconciliar explícitamente el alcance para dejar `doctor-based views` diferido a un slice futuro acotado.

**Primer slice implementado de Release 2** — [Hecho]
- agregado `Appointment` tenant-owned y branch-aware
- enlace obligatorio a `Patient` existente dentro del tenant
- estatus mínimo explícito `Scheduled` / `Cancelled`
- creación, edición, reprogramación y cancelación de citas
- vista mínima de calendario diario/semanal por sucursal
- filtrado branch-aware usando membership, rol y permisos existentes
- permisos explícitos `scheduling.read` / `scheduling.write`
- soporte frontend mínimo con página de scheduling, selector de sucursal, vista day/week y formulario de cita

**Segundo sub-slice implementado de Release 2** — [Hecho]
- agregado `AppointmentBlock` tenant-owned y branch-aware como entidad dedicada, sin sobrecargar `AppointmentStatus`
- rango horario mínimo `StartsAt` / `EndsAt` con validación explícita de rango válido
- `Label` opcional corto para motivo operativo del bloqueo
- persistencia EF Core + migración dedicada `AddAppointmentBlocks`
- visibilidad de blocked slots dentro del mismo read model del calendario existente, sin crear una segunda vista de acceso
- endpoints mínimos para crear y eliminar blocked slots dentro del bounded context Scheduling
- bloqueo de creación/reprogramación de citas cuando se solapan con blocked slots de la misma sucursal
- cobertura automática para creación/lectura tenant-scoped, acceso cross-tenant prohibido, restricciones branch-aware y colisiones cita-vs-block
- soporte frontend mínimo para crear, visualizar y eliminar blocked slots desde la página actual de Scheduling

**Tercer sub-slice implementado de Release 2** — [Hecho]
- extensión mínima del lifecycle de `Appointment` con estados explícitos `Attended` y `NoShow`
- transiciones de dominio acotadas `MarkAttended()` y `MarkNoShow()` solo desde `Scheduled`
- bloqueo explícito de transiciones incompatibles desde `Cancelled`, `Attended` y `NoShow`
- endpoints mínimos para marcar citas como attended o no-show sin abrir un segundo modelo de acceso
- read model de calendario existente preservado y enriquecido con los nuevos estados para distinción visual mínima
- soporte frontend mínimo para marcar attended/no-show desde la selección de cita y mostrar badges/estilos diferenciados en el calendario
- cobertura automática para transiciones permitidas, transiciones inválidas, acceso cross-tenant prohibido, restricciones branch-aware y representación de estados en calendario

**Estado de Release 2** — [Hecho] completado.

**Decisión de alcance** — [Hecho] `doctor-based views` se difiere explícitamente a un slice futuro porque no es un parche pequeño de UI: requiere un slice dedicado de provider/doctor assignment, cambios de modelo y read models específicos de calendario.

**Fase abierta actual** — [Hecho] Release 3 — Clinical Records, con Release 3.1 — Clinical Record Foundation y Release 3.2 — Basic Diagnoses Foundation aceptadas.

**Precondición ya resuelta** — [Hecho]
- policies y/o handlers backend para tenant user / tenant admin / platform admin o equivalentes
- autorización evaluada por scope, membership, rol y permiso; no solo por nombre de rol
- enforcement centralizado tenant-aware para reads/writes en datos tenant-owned actuales
- soporte explícito y auditable para platform override permitido
- pruebas automáticas para reads/writes cross-tenant prohibidos, restricciones branch-aware y escenarios permitidos de bypass de plataforma
- endpoint `/api/auth/me` y base frontend de guards/contexto actual

## 6. Riesgos y temas a vigilar

**Tenant isolation** — [Hecho] Sigue siendo el riesgo estructural principal: cualquier fuga cross-tenant por modelado incompleto, resolución insegura de tenant, filtros incompletos o bypass silencioso compromete la frontera primaria de seguridad.

**Authorization model** — [Hecho] La fundación ya quedó cerrada y probada por membership, scope y permisos; el catálogo inicial es pequeño a propósito y seguirá evolucionando por módulo sin reabrir la forma base del modelo.

**Query filters y acceso a datos** — [Hecho] Los filtros globales de tenant y el enforcement centralizado siguen siendo críticos; el riesgo es degradarlos con filtros manuales dispersos, accesos ad hoc o bypasses no controlados.

**Privileged/platform paths** — [Hecho] Toda operación fuera del tenant scope normal debe ser explícita, mínima, segregada y auditable; el riesgo es normalizar caminos privilegiados sin trazabilidad suficiente.

**Consistencia TenantId / BranchId** — [Hecho] BranchId sigue siendo scope operativo subordinado; el riesgo es usarlo como sustituto implícito de TenantId o permitir combinaciones tenant/branch inconsistentes.

**Claims y contrato de identidad** — [Hecho] El contrato base ya quedó formalizado con user id, role, scope, permission, tenant claim y branch claim cuando aplica; futuras fases solo deberían extenderlo de forma compatible.

**UX operativa** — [Hecho] La seguridad y la estructura técnica no deben degradar la velocidad ni la claridad de los flujos núcleo; la UX operativa sigue siendo restricción de producto.

**CI / tests** — [Hecho] Cambios en tenant handling, authz, pacientes, clínica, pagos, documentos y overrides de plataforma requieren cobertura explícita y evidencia de validación; “compila” no es criterio suficiente.

**Alineación documental futura** — [Hecho] README.md, PROJECT_MAP.md y AGENTS.md ya están reconciliados con el estado canónico; el riesgo ahora es reintroducir desalineación cuando avance una fase o cambie una decisión arquitectónica sin actualizar STATE y documentación base en el mismo cambio.

## 7. Backlog inmediato

Lista priorizada:

1. Preservar Releases 1 y 2 ya cerrados sin debilitar la fundación tenant-aware ya cerrada.

2. Continuar Release 3 — Clinical Records en slices acotados, auditables y compatibles con la fundación tenant-aware ya cerrada.

3. Mantener diferidas las `doctor-based views` hasta abrir un slice dedicado de provider/doctor assignment; no reintroducirlas como parche incidental de UI.

4. Mantener explícitos y auditables los privileged/platform paths a medida que aparezcan endpoints funcionales.

5. Extender el catálogo de permisos solo junto con módulos reales, evitando inflarlo antes de tiempo.

6. Mantener sincronizados STATE — BigSmile.md, README.md, PROJECT_MAP.md y AGENTS.md cuando avance el estado del proyecto.

## 8. Criterios para no perder el rumbo

[Hecho] No olvidar que BigSmile es producto SaaS multi-tenant para clínicas, no una solución one-off; cualquier atajo que debilite tenant isolation, mantenibilidad o reviewabilidad rompe el rumbo.

[Hecho] El orden del producto importa: primero foundation, luego Patients, Scheduling, Clinical Records, Odontogram, Treatments and Quotes, Billing, Documents and Dashboard; después vendrán recordatorios, online booking, facturación electrónica, patient portal, analytics y automatizaciones.

[Hecho] Capacidades importantes aunque todavía no estén implementadas o cerradas: reminders por WhatsApp/email, online booking, advanced multi-branch administration, tenant branding/feature flags, onboarding automation, patient portal, analytics avanzados y automatizaciones. No deben olvidarse en el diseño aunque no entren todavía.

[Hecho] Restricciones de diseño a preservar: modular monolith; capas y ownership explícitos; TenantId como boundary primario; BranchId subordinado; shared DB/schema inicial; sin microservicios ni DB por tenant en esta etapa; sin bypass oculto de plataforma; sin lógica crítica de negocio/autorización solo en UI.

[Hecho] La UX operativa rápida es parte del diseño: búsqueda/alta de paciente, agenda, clínica, tratamientos y pagos deben seguir siendo flujos cortos, claros y de baja fricción.

[Hecho] Ningún release funcional del roadmap debe tratarse como completado sin evidencia explícita en código y documentación alineada.

## 9. Nota tipo ADR resumida

**Estado:** Aceptada como nota canónica operativa; reconciliada con README.md, PROJECT_MAP.md y AGENTS.md.

**Contexto:** BigSmile es un SaaS multi-tenant para clínicas dentales, con arquitectura modular monolith, Tenant como frontera primaria de seguridad, Branch como scope operativo subordinado y una base fundacional ya establecida más allá de bootstrap.

**Decisión:** Tratar como cerradas Foundation / Release 0 base, Pre-auth hardening, Identity + Persistence Foundation, Tenant-Aware Authorization Foundation, Release 1 — Patients y Release 2 — Scheduling; tratar Release 3 — Clinical Records como fase abierta en progreso con Release 3.1 — Clinical Record Foundation y Release 3.2 — Basic Diagnoses Foundation aceptadas; tratar `doctor-based views` como diferido a un slice futuro acotado; tratar README.md, PROJECT_MAP.md, AGENTS.md y docs/product-roadmap.md como reconciliados con STATE; no asumir cerrada Release 3 ni implementados o cerrados los sub-slices posteriores del módulo Clinical o los releases funcionales posteriores del MVP mientras no exista evidencia explícita en código y documentación alineada.

**Consecuencias:** La prioridad inmediata pasa a ser preservar el cierre de Patients y Scheduling, preservar los slices aceptados Release 3.1 — Clinical Record Foundation y Release 3.2 — Basic Diagnoses Foundation, continuar Release 3 — Clinical Records en slices acotados y mantener sincronizados STATE y documentación base cada vez que cambie el estado del proyecto.
