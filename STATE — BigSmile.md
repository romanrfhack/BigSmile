# BigSmile — Estado actual canónico

## 1. Resumen ejecutivo

[Hecho] BigSmile es una plataforma SaaS para la gestión de clínicas y consultorios dentales, concebida desde el inicio como producto comercial multi-tenant y no como sistema interno de una sola clínica.

[Hecho] El objetivo del producto es cubrir el backbone operativo de la clínica en un flujo continuo: cita → expediente/paciente → expediente clínico → odontograma → plan de tratamiento → cotización → pago → seguimiento, preservando seguridad, mantenibilidad y UX operativa rápida.

[Hecho] Stack principal: backend .NET 10 + ASP.NET Core Web API + EF Core + SQL Server; frontend Angular 21 + TypeScript; calidad/operación con GitHub Actions, pruebas automatizadas, logging estructurado, health checks y auditoría.

[Hecho] Modelo SaaS: Tenant = clínica/consultorio cliente; Branch = sucursal/ubicación interna del tenant. TenantId es la frontera primaria de seguridad y propiedad; BranchId es scope operativo subordinado, no el boundary principal.

## 2. Decisiones arquitectónicas cerradas

**Arquitectura general** — [Hecho] BigSmile arranca como modular monolith, con fronteras explícitas de backend (Api / Application / Domain / Infrastructure / SharedKernel) y frontend (core / shell / shared / features). No hay decisión de microservicios para la etapa inicial.

**Multitenancy** — [Hecho] La estrategia base es shared database + shared schema + TenantId como discriminador transversal, con TenantContext por request, enforcement centralizado, y bypass solo para operaciones de plataforma, explícito y auditable.

**Auth** — [Hecho] La base de identidad y autenticación ya está establecida sobre JWT como dirección principal, con tenant claim como autoridad primaria para requests tenant-scoped y sin confiar la autorización crítica al frontend ni al tenant scope enviado de forma insegura por el cliente. [Pendiente por validar] La capa completa de autorización tenant-aware por scope / membership / permiso todavía no está cerrada.

**Persistencia** — [Hecho] La persistencia base es EF Core sobre SQL Server, con AppDbContext, migraciones, seed durable y validación de login real contra base SQL Server. Tenant, Branch e identidad ya comparten la misma ruta de persistencia durable. BranchId solo se usa cuando el dominio lo requiere, subordinado a TenantId.

**Frontend** — [Hecho] El frontend queda fijado como feature-based, con separación entre páginas, componentes, facades, data-access y modelos; se prohíbe dispersar llamadas HTTP en componentes/páginas y se prioriza UX operativa rápida para recepción y clínica.

**Principios no negociables** — [Hecho] Tenant isolation no negociable; Branch no reemplaza a Tenant; no usar filtrado manual disperso como estrategia principal; producto antes que app ad hoc; cambios pequeños, auditables y reversibles; arquitectura mantenible; UX operativa como feature; no introducir complejidad distribuida prematura.

## 3. Fases completadas

[Hecho] Foundation / Release 0 base — completada.

[Hecho] Pre-auth hardening — completada.

[Hecho] Identity + Persistence Foundation — completada.

[Hecho] La corrección de consistencia de persistencia quedó absorbida dentro de Identity + Persistence Foundation: tenant y branch ya no viven en un path in-memory separado, el seed es durable y el login real contra SQL Server ya fue validado.

[Hecho] No hay otra fase cerrada explícitamente en el roadmap funcional. Patients, Scheduling, Clinical Records, Odontogram, Treatments and Quotes, Billing y Documents/Dashboard no deben asumirse como implementados o cerrados salvo evidencia explícita en código y documentación alineada.

## 4. Fase actual

[Hecho] La última fase marcada como completada es Identity + Persistence Foundation.

[Hecho] README.md, PROJECT_MAP.md y AGENTS.md ya fueron reconciliados con este estado canónico y ubican al repositorio más allá del bootstrap / early foundation stage.

[Inferencia operativa] El proyecto está listo para arrancar formalmente Tenant-Aware Authorization Foundation.

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

[Hecho] No existe evidencia canónica de cierre para los releases funcionales 1–7 del roadmap; por tanto, Patients, Scheduling, Clinical Records, Odontogram, Treatments and Quotes, Billing y Documents/Dashboard no deben asumirse como implementados/cerrados.

## 5. Siguiente fase prevista

**Nombre exacto** — [Hecho] Tenant-Aware Authorization Foundation.

**Objetivo** — [Hecho + Inferencia alineada a docs] Llevar la autorización desde una base auth existente hacia un modelo realmente scope-aware y tenant-aware, donde la decisión de acceso combine tenant membership, branch assignment cuando aplique, rol, permiso y contexto de plataforma / tenant / branch.

**Entregables esperados** — [Inferencia operativa]
- políticas y/o requirement handlers backend para tenant user / tenant admin / platform admin o equivalentes
- autorización evaluada por scope, membership, rol y permiso; no solo por nombre de rol
- enforcement centralizado de aislamiento tenant-aware en acceso a datos (por ejemplo, query filters globales o mecanismo equivalente)
- soporte de platform/support override solo donde esté explícitamente permitido y con auditoría
- pruebas automáticas para reads/writes cross-tenant prohibidos, restricciones branch-aware y escenarios permitidos de bypass de plataforma
- documentación/ADR específica para tenant resolution, auth/session y authorization model
- base frontend para authz: guards, contexto de usuario/tenant y, si aplica, endpoint `/auth/me`

## 6. Riesgos y temas a vigilar

**Tenant isolation** — [Hecho] Sigue siendo el riesgo estructural principal: cualquier fuga cross-tenant por modelado incompleto, resolución insegura de tenant, filtros incompletos o bypass silencioso compromete la frontera primaria de seguridad.

**Authorization model** — [Hecho + Pendiente] Los roles por sí solos no bastan; la autorización debe cerrarse y probarse por membership, scope y permisos. El catálogo final de permisos y la implementación exacta de auth/session siguen siendo áreas evolutivas.

**Query filters y acceso a datos** — [Hecho] Los filtros globales de tenant y el enforcement centralizado siguen siendo críticos; el riesgo es degradarlos con filtros manuales dispersos, accesos ad hoc o bypasses no controlados.

**Privileged/platform paths** — [Hecho] Toda operación fuera del tenant scope normal debe ser explícita, mínima, segregada y auditable; el riesgo es normalizar caminos privilegiados sin trazabilidad suficiente.

**Consistencia TenantId / BranchId** — [Hecho] BranchId sigue siendo scope operativo subordinado; el riesgo es usarlo como sustituto implícito de TenantId o permitir combinaciones tenant/branch inconsistentes.

**Claims y contrato de identidad** — [Hecho + Pendiente] La base JWT ya existe, pero el contrato de claims para tenant users, platform users, branch scope y override auditable todavía debe formalizarse mejor en la siguiente fase.

**UX operativa** — [Hecho] La seguridad y la estructura técnica no deben degradar la velocidad ni la claridad de los flujos núcleo; la UX operativa sigue siendo restricción de producto.

**CI / tests** — [Hecho] Cambios en tenant handling, authz, pacientes, clínica, pagos, documentos y overrides de plataforma requieren cobertura explícita y evidencia de validación; “compila” no es criterio suficiente.

**Alineación documental futura** — [Hecho] README.md, PROJECT_MAP.md y AGENTS.md ya están reconciliados con el estado canónico; el riesgo ahora es reintroducir desalineación cuando avance una fase o cambie una decisión arquitectónica sin actualizar STATE y documentación base en el mismo cambio.

## 7. Backlog inmediato

Lista priorizada:

1. Cerrar Tenant-Aware Authorization Foundation con enforcement backend tenant-aware y, cuando aplique, branch-aware, integrado con TenantContext y BranchContext.

2. Formalizar la decisión de acceso por scope (platform / tenant / branch), membership, rol y permiso, evitando depender solo del nombre del rol o de scope enviado de forma insegura por el cliente.

3. Introducir y/o endurecer el enforcement centralizado de acceso a datos tenant-owned (por ejemplo, query filters globales o equivalente) y revisar los caminos de bypass explícito.

4. Hacer explícitos, segregados y auditables los privileged/platform paths y cualquier platform override permitido.

5. Ampliar la cobertura automática para reads/writes cross-tenant prohibidos, restricciones branch-aware y escenarios permitidos de override de plataforma.

6. Incorporar el soporte mínimo de frontend para la fase de authz: guards, contexto de usuario/tenant y endpoint `/auth/me` o equivalente si todavía no existe.

7. Formalizar los ADRs siguientes ya anticipados por ADR 001: Tenant Resolution Strategy, Authentication and Session Strategy y Authorization Model.

8. Mantener sincronizados STATE — BigSmile.md, README.md, PROJECT_MAP.md y AGENTS.md cuando avance el estado del proyecto; no reintroducir lenguaje de bootstrap en documentación base ya reconciliada.

9. No abrir Release 1 — Patients hasta dejar verde, trazable y validada Tenant-Aware Authorization Foundation.

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

**Decisión:** Tratar como cerradas Foundation / Release 0 base, Pre-auth hardening e Identity + Persistence Foundation; tratar README.md, PROJECT_MAP.md y AGENTS.md como ya reconciliados con STATE; no asumir implementados o cerrados los releases funcionales del MVP mientras no exista evidencia explícita en código y documentación alineada.

**Consecuencias:** La prioridad inmediata es construir y estabilizar Tenant-Aware Authorization Foundation — autorización por scope, membership, rol y permiso; platform override explícito y auditable; enforcement centralizado y pruebas de aislamiento — y mantener sincronizados STATE y documentación base cada vez que cambie el estado del proyecto. Solo después corresponde abrir de forma ordenada Release 1 — Patients.
