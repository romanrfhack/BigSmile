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

[Hecho] La corrección de consistencia de persistencia quedó absorbida dentro de Identity + Persistence Foundation: tenant y branch ya no viven en un path in-memory separado, el seed es durable y el login real contra SQL Server ya fue validado.

[Hecho] No hay otra fase cerrada explícitamente en el roadmap funcional. Patients, Scheduling, Clinical Records, Odontogram, Treatments and Quotes, Billing y Documents/Dashboard no deben asumirse como implementados o cerrados salvo evidencia explícita en código y documentación alineada.

## 4. Fase actual

[Hecho] La última fase marcada como completada es Tenant-Aware Authorization Foundation.

[Hecho] Release 1 — Patients ya fue abierta con un primer vertical slice funcional y acotado.

[Hecho] README.md, PROJECT_MAP.md y AGENTS.md ya fueron reconciliados con este estado canónico y ubican al repositorio más allá del bootstrap / early foundation stage.

[Hecho + Inferencia operativa] El proyecto ya no está solo “listo para abrir” Patients; ahora tiene una primera base implementada del módulo sobre la autorización tenant-aware ya validada.

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

[Hecho] Este slice inicial mantiene el modelo de paciente tenant-owned y branch-neutral; no introduce aún clinical alerts, expediente clínico, scheduling ni otros módulos posteriores.

[Hecho] No existe evidencia canónica de cierre para los releases funcionales 1–7 del roadmap; por tanto, Patients, Scheduling, Clinical Records, Odontogram, Treatments and Quotes, Billing y Documents/Dashboard no deben asumirse como implementados/cerrados.

## 5. Siguiente fase prevista

**Nombre exacto** — [Hecho] Release 1 — Patients.

**Objetivo** — [Hecho + Inferencia alineada a docs] Continuar Release 1 — Patients sobre una base ya endurecida de aislamiento tenant-aware, authz por scope/membership/permiso y override de plataforma explícito.

**Estado operativo actual** — [Hecho] El release ya comenzó con un slice fundacional pequeño y coherente; todavía no debe tratarse como release cerrado.

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

1. Continuar Release 1 — Patients sin debilitar la fundación tenant-aware ya cerrada.

2. Extender el slice inicial de Patients sin romper el modelo branch-neutral del paciente ni la autorización tenant-aware ya cerrada.

3. Reutilizar el modelo de authorization por scope/membership/permiso al introducir más use cases de pacientes.

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

**Decisión:** Tratar como cerradas Foundation / Release 0 base, Pre-auth hardening, Identity + Persistence Foundation y Tenant-Aware Authorization Foundation; tratar README.md, PROJECT_MAP.md y AGENTS.md como reconciliados con STATE; no asumir implementados o cerrados los releases funcionales del MVP mientras no exista evidencia explícita en código y documentación alineada.

**Consecuencias:** La prioridad inmediata pasa a ser abrir Release 1 — Patients sobre una base ya validada de autorización tenant-aware, override explícito y enforcement centralizado, manteniendo sincronizados STATE y documentación base cada vez que cambie el estado del proyecto.
