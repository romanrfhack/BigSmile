# BigSmile UX Redesign Reconciliation and Plan

## 1. Objetivo

Este documento reconcilia el estado canonico documental, el roadmap, el estado real del codigo y el trabajo actual de rediseño visual solicitado por el cliente.

El objetivo de este slice es documental y de planeacion visual. No abre releases funcionales, no cierra releases por presencia de codigo, no cambia backend, no cambia frontend funcional, no agrega permisos, no crea migrations y no modifica contratos API.

La interpretacion operativa para el trabajo activo es: **client-driven UX redesign / visual organization pass**.

## 2. Estado Canonico Documental

### Releases completadas y preservadas

Segun `STATE — BigSmile.md`, `README.md`, `PROJECT_MAP.md` y `docs/product-roadmap.md`, las fases formalmente completadas son:

- Foundation / Release 0 base.
- Pre-auth hardening.
- Identity + Persistence Foundation.
- Tenant-Aware Authorization Foundation.
- Release 1 — Patients.
- Release 2 — Scheduling.
- Release 3 — Clinical Records, cerrada mediante Release 3.1 a Release 3.6.

Release 3 se preserva como release clinico fundacional. Sus slices aceptados cubren expediente clinico explicito, snapshot base, alergias actuales, notas append-only, diagnosticos basicos, timeline clinica acotada, snapshot history, cuestionario medico fijo y encounters/vitals acotados.

### Siguiente fase funcional

La siguiente fase funcional prevista es **Release 4 — Odontogram**.

Release 4 no esta abierta por la documentacion actual. Cualquier aceptacion o cierre de Odontogram debe ocurrir en un slice futuro explicito, con auditoria, pruebas y documentacion propios.

### Roadmap por modulo

- Patients: Release 1 esta completada y preservada. Cubre registro, actualizacion, busqueda, perfil, responsible party, estatus activo/inactivo, alertas clinicas basicas, validacion de fecha de nacimiento futura y permisos `patient.read` / `patient.write`.
- Scheduling: Release 2 esta completada y preservada. Cubre calendario diario/semanal branch-aware, citas, reprogramacion, cancelacion, attended/no-show, blocked slots, appointment notes y permisos `scheduling.read` / `scheduling.write`. Doctor-based views quedan diferidas.
- Clinical Records: Release 3 esta completada y preservada mediante slices 3.1 a 3.6. No incluye odontogram, treatments, documents, billing, doctor/provider assignment, timeline avanzada ni form builder.
- Odontogram: Release 4 es la siguiente fase funcional prevista, pero no abierta. El roadmap planea una fundacion acotada con creacion explicita, un odontograma por paciente/tenant y numeracion FDI adulta.
- Treatments and Quotes: Release 5 esta planeada despues de Release 4. No esta abierta por el cierre de Release 3.
- Billing: Release 6 esta planeada despues de Release 5. Payments, balances, receipts, taxes, discounts, CFDI/PAC y advanced billing siguen diferidos.
- Documents and Dashboard: Release 7 esta planeada despues de Billing. Documents y Dashboard iniciales pertenecen al cierre del MVP, no a Phase 2.
- Phase 2 Expansion — Modern Operations: posterior al MVP estable. No esta activa.

### ADRs relevantes

- ADR 001 — Initial Architecture Principles: modular monolith, tenant isolation, frontend feature-oriented, UX operativa como feature de producto y disciplina de roadmap.
- ADR 002 — Tenant-Aware Authorization Foundation: autorizacion por scope/membership/role/permission, `TenantContext`, platform override explicito y enforcement centralizado.
- ADR 003 — Release 2 Scheduling Closure and Doctor-Based View Deferral: Scheduling cierra sin doctor-based views; provider/doctor assignment requiere slice dedicado.
- ADR 004 — Manual VPS Deployment Foundation: afecta operacion/deployment, no UX funcional directa.
- ADR 005 — Frontend Operational UX Redesign: formaliza rediseño progresivo, tokens `--bsm-*`, acciones visibles, tabs/drawers/modals/sticky bars, accesibilidad y prohibicion de cambiar backend/API/auth/permissions/migrations en slices visuales.

## 3. Estado Real Del Codigo Por Modulo

Leyenda:

- Backend/frontend/endpoints/permisos/migrations/tests: `si`, `no`, `parcial`.
- Estado visual: `rediseñado`, `parcialmente rediseñado`, `legacy`, `no revisado`.
- Estado funcional recomendado: `Accepted / preserved`, `Implemented but not reconciled`, `Candidate for acceptance`, `Visual-only pending`, `Out of scope`.

| Modulo | Backend | Frontend | Endpoints | Permisos | Migrations | Tests | Estado visual | Estado funcional recomendado |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Login/Shell | si | parcial | si (`/api/auth/login`, `/api/auth/me`) | si (`auth.self.read`) | si, base identity | si | parcialmente rediseñado | Accepted / preserved + Visual-only pending |
| Dashboard | si | si | si (`GET /api/dashboard/summary`) | parcial (`dashboard.read`, visible en TenantAdmin) | no especificas de dashboard | si | parcialmente rediseñado | Implemented but not reconciled |
| Patients | si | si | si (`api/patients`) | si (`patient.read`, `patient.write`) | si | si | parcialmente rediseñado | Accepted / preserved + Visual-only pending |
| Scheduling | si | si | si (`api/appointments`, `api/appointmentblocks`, manual reminders, reminder templates) | si (`scheduling.read`, `scheduling.write`) | si | si | parcialmente rediseñado | Accepted / preserved; reminder/manual template scope implemented but not reconciled |
| Clinical Records | si | si | si (`clinical-record`, `questionnaire`, `encounters`, `notes`, `diagnoses`) | si (`clinical.read`, `clinical.write`) | si | si | parcialmente rediseñado | Accepted / preserved + Visual-only pending |
| Odontogram | si | si | si (`api/patients/{id}/odontogram`, teeth, surfaces, findings) | si (`odontogram.read`, `odontogram.write`) | si | si | parcialmente rediseñado | Implemented but not reconciled; Candidate for future acceptance after audit |
| Treatments | si | si | si (`treatment-plan`, items, status, quote) | si (`treatmentplan.*`, `treatmentquote.*`) | si | si | parcialmente rediseñado | Implemented but not reconciled |
| Billing | si | si | si (`treatment-plan/quote/billing`) | si (`billing.read`, `billing.write`) | si | si | parcialmente rediseñado | Implemented but not reconciled |
| Documents | si | si | si (`documents`, upload/download/retire) | si (`document.read`, `document.write`) | si | si | parcialmente rediseñado | Implemented but not reconciled |
| Reminders / otros | parcial, dentro de Scheduling | parcial, dentro de Scheduling | si (`manual-reminders`, `reminder-log`, `reminder-templates`) | si, reutiliza Scheduling | si | si | parcialmente rediseñado | Implemented but not reconciled; Phase 2 no activa |

### Evidencia principal inspeccionada

- Controllers: `PatientsController`, `AppointmentsController`, `AppointmentBlocksController`, `PatientClinicalRecordsController`, `PatientOdontogramsController`, `PatientTreatmentPlansController`, `PatientTreatmentQuotesController`, `PatientBillingDocumentsController`, `PatientDocumentsController`, `DashboardController`, `ReminderTemplatesController`.
- Application features: `Patients`, `Scheduling`, `ClinicalRecords`, `Odontograms`, `TreatmentPlans`, `TreatmentQuotes`, `BillingDocuments`, `PatientDocuments`, `Dashboard`.
- Domain entities: `Patient`, `Appointment`, `ClinicalRecord`, `ClinicalMedicalAnswer`, `ClinicalEncounter`, `Odontogram`, `TreatmentPlan`, `TreatmentQuote`, `BillingDocument`, `PatientDocument`, `ReminderTemplate`.
- Frontend routes: `patients`, `patients/:id`, `clinical-record`, `odontogram`, `documents`, `treatment-plan`, `quote`, `billing`, `scheduling`, `dashboard`.
- Data access/facades: presentes por modulo; no se detecto HTTP directo en pages/components durante esta auditoria.
- Migrations: existen migrations hasta Clinical 3.6 y tambien para Odontogram, Treatments, Billing, Documents, appointment reminders/templates y Patient Demographics.
- Tests: existen unit/integration tests por modulo para Patients, Scheduling, Clinical, Odontogram, TreatmentPlans, TreatmentQuotes, BillingDocuments, Documents, Dashboard y tenant enforcement.

## 4. Drift Detectado

### Codigo mas adelante que roadmap formal

El repositorio contiene implementacion real de Odontogram, Treatments/Quotes, Billing, Documents, Dashboard y reminder/manual reminder flows. La documentacion canonica no los marca como releases aceptadas/cerradas. Por tanto, deben tratarse como **implemented but not reconciled** hasta auditoria especifica.

### Documentos que describen como futuro algo que ya existe

`STATE — BigSmile.md`, `README.md` y `docs/product-roadmap.md` dicen correctamente que Release 4+ no estan formalmente abiertas/cerradas. Sin embargo, el codigo tiene endpoints, permisos, migrations, frontend y tests para esas areas. La documentacion necesita una nota de reconciliacion que distinga presencia de codigo contra aceptacion formal.

### Labels internos de release o terminos tecnicos en UI

Se detectaron textos visibles en frontend con `Release`, `Release 4.4`, `Release 5.1`, `Release 6.1`, `Release 7.1`, `slice`, `foundation`, `roadmap` o descripciones tecnicas. Ejemplos:

- `frontend/src/app/features/clinical-records/pages/clinical-record.page.ts`
- `frontend/src/app/features/odontogram/pages/odontogram.page.ts`
- `frontend/src/app/features/treatments/pages/treatment-plan.page.ts`
- `frontend/src/app/features/treatments/pages/treatment-quote.page.ts`
- `frontend/src/app/features/billing/pages/billing-document.page.ts`
- `frontend/src/app/features/documents/pages/patient-documents.page.ts`
- `frontend/src/app/features/dashboard/pages/dashboard.page.ts`
- `frontend/src/app/core/i18n/translations.ts`

Estos labels ayudan a desarrollo/review, pero no son lenguaje natural para usuarios de clinica. En slices visuales deben reemplazarse por copy operativo sin cambiar funcionalidad.

### Scroll, organizacion y responsabilidades

- `scheduling.page.ts` tiene alrededor de 1274 lineas y concentra mucha UI de agenda/reminders/templates.
- `clinical-record.page.ts` ronda 702 lineas y mantiene varias secciones clinicas en flujo vertical.
- `clinical-encounter-vitals-section.component.ts` ronda 745 lineas.
- `clinical-medical-questionnaire-form.component.ts` ronda 537 lineas.
- `patient-form.component.ts`, `odontogram-grid.component.ts`, `tooth-state-editor.component.ts` y varias pages de modulos posteriores tambien son grandes.

Esto no implica bug por si mismo, pero confirma deuda de organizacion visual y posible mezcla de responsabilidades en pantallas densas.

### Colores hardcodeados

Existen tokens `--bsm-*` en `frontend/src/styles/_brand-tokens.scss`, pero tambien se detectaron colores hardcodeados en Odontogram, Treatments, Billing, Documents y algunos estados. En futuros slices visuales, esos colores deben migrarse a tokens existentes o nuevos tokens deliberados.

### Acciones fuera de vista y patrones faltantes

Ya existe `app-sticky-action-bar` y se usa en Patient form, Medical Questionnaire y Encounter/Vitals. No se detectaron componentes shared de modal/drawer/tab/accordion. Scheduling, Clinical Records y otros modulos densos podrian beneficiarse de tabs, drawers o paneles contextuales para reducir scroll sin cambiar funcionalidad.

## 5. Interpretacion Correcta Del Momento Actual

- El trabajo activo debe tratarse como **client-driven UX redesign / visual organization pass**.
- No se debe avanzar a mas funcionalidad sin reconciliar primero el codigo existente.
- No se debe cerrar una release por presencia de codigo.
- No se debe reabrir Patients, Scheduling o Clinical Records salvo para visual-only, QA o hardening explicitamente acotado.
- No se debe declarar Phase 2 como activa.
- No se debe abrir doctor/provider assignment.
- Si Release 4 u otros modulos tienen codigo existente, deben clasificarse como `implemented but not formally accepted/reconciled` hasta auditoria especifica.
- Reminders manuales y templates existentes no significan WhatsApp/email/SMS real, jobs, queues, providers, scheduler automatico, retry automation ni Phase 2 activa.

## 6. Criterios De Rediseño Visual

Basado en `docs/frontend-ux-guidelines.md` y ADR 005:

- Acciones principales visibles above-the-fold.
- Menos scroll innecesario.
- Tabs cuando existan secciones hermanas del mismo contexto.
- Modal/drawer para tareas contextuales acotadas.
- Sticky action bars en formularios largos.
- Mas color mediante tokens y estados semanticos.
- Microanimaciones sobrias, cortas y con `prefers-reduced-motion`.
- Sin scroll-jacking.
- Sin animaciones infinitas decorativas.
- Responsive desktop/tablet/mobile.
- Foco visible.
- i18n para textos visibles.
- Sin colores hardcodeados fuera de tokens.
- Sin HTTP directo en pages/components.
- Sin presentar capacidades diferidas como si estuvieran implementadas.

## 7. Reglas Para Modales, Drawers Y Pantallas

- Modal: usar para crear/editar algo corto, confirmaciones y decisiones que deben bloquear temporalmente la pantalla. Debe tener nombre accesible, foco atrapado, restauracion de foco y cierre claro.
- Drawer lateral: usar para detalle contextual, filtros, preview, acciones de fila, appointment detail, document metadata o informacion auxiliar que no debe sacar al usuario del flujo principal.
- Fullscreen sheet mobile: usar como equivalente mobile de drawer/modal cuando el contenido requiere espacio o inputs tactiles comodos.
- Tabs: usar para secciones hermanas dentro del mismo contexto, como Resumen / Antecedentes / Consulta / Timeline. No deben esconder la accion principal del flujo activo.
- Accordion: usar para contenido secundario o infrecuente. No esconder informacion critica en un accordion cerrado por defecto.
- Inline expansion: usar para detalle corto de una fila, validacion o metadata simple que no justifica drawer.
- Sticky action bar: usar en formularios largos o flujos editables para mantener guardar/cancelar/estado dirty/error visibles.

Ejemplos:

- Crear/editar algo corto: modal o drawer.
- Formularios largos: pagina dedicada o drawer amplio con sticky action bar.
- Datos de referencia: cards compactas o tabs.
- Acciones de fila: menu contextual o drawer.
- Confirmaciones: modal.
- Informacion critica: visible por defecto, no escondida en accordion cerrado.

## 8. Plan Visual Por Modulos

### Slice 1 — Shell/Login final QA

- Objetivo: dejar login/session home con lenguaje operativo, color sobrio, foco visible y estados claros.
- Archivos probables: `features/auth/login`, `features/auth/session-home`, `app.routes.ts`, shared UI si solo se reutiliza.
- Riesgo: tocar auth/session o guards accidentalmente.
- Que NO tocar: backend auth, JWT, `/api/auth/me`, guards, storage/session strategy, permisos.
- Validacion requerida: `npm test -- --watch=false`, build frontend, revision visual 1366x768 / 1440x900 / 390x844 si se toca UI.
- Criterios: sin terminos internos, acciones visibles, errores claros, foco visible, i18n conservado.

### Slice 2 — Patients final QA

- Objetivo: optimizar busqueda, formulario y perfil con acciones primarias visibles y menos scroll.
- Archivos probables: `features/patients/pages`, `features/patients/components`, `features/patients/facades` solo si no cambia comportamiento.
- Riesgo: duplicar datos clinicos o alterar validaciones.
- Que NO tocar: APIs, `Patient` backend, permisos, migrations, Clinical Records.
- Validacion requerida: tests frontend de Patients, build, `git diff --check`, visual desktop/tablet/mobile.
- Criterios: busqueda rapida, registro claro, estado/alertas visibles, sticky action bar en formulario largo.

### Slice 3 — Scheduling final QA

- Objetivo: reducir densidad y scroll de agenda sin reabrir scope, con acciones contextuales visibles.
- Archivos probables: `features/scheduling/pages/scheduling.page.ts`, componentes de appointment/reminders/templates.
- Riesgo: reabrir doctor-based views o presentar reminders automaticos.
- Que NO tocar: provider/doctor assignment, backend Scheduling, permisos, reminder providers/jobs/queues/webhooks.
- Validacion requerida: tests Scheduling frontend, build, revision visual calendario y mobile.
- Criterios: branch context visible, crear/editar/reprogramar accesible, manual reminder copy claro, sin implicar envio automatico.

### Slice 4 — Clinical Records presentation/organization

- Objetivo: organizar expediente clinico segun formato fisico sin duplicar Patient ni mezclar Billing; reducir scroll con tabs/paneles/sticky bars.
- Archivos probables: `features/clinical-records/pages`, `components/clinical-medical-questionnaire-form`, `components/clinical-encounter-vitals-section`, shared UI si se agregan tabs/drawer visuales.
- Riesgo: mezclar timeline con snapshot history, introducir events nuevos, duplicar demografia o contaminar con Billing.
- Que NO tocar: backend Clinical, endpoints, contratos, permissions, allergies auto-sync, doctor/provider assignment, Odontogram/Treatments/Documents/Billing.
- Validacion requerida: tests frontend Clinical, build, grep HTTP directo, grep colores hardcodeados, visual desktop/tablet/mobile.
- Criterios: Patient context read-only visible, cuestionario medico agrupado, encounters/vitals claros, acciones de guardado visibles, timeline y snapshot history separados.

### Slice 5 — Odontogram visual inventory

- Objetivo: inventariar UX real de Odontogram y proponer reconciliacion visual sin aceptar Release 4 todavia.
- Archivos probables: `features/odontogram`, `PatientOdontogramsController`, tests solo para lectura de contexto si se audita.
- Riesgo: tratar codigo 4.x como release cerrada por presencia de UI.
- Que NO tocar: funcionalidad odontogram, permisos, migrations, acceptance state.
- Validacion requerida: inventario documental, screenshots si se revisa visualmente, sin npm/dotnet si solo hay docs.
- Criterios: clasificacion clara `implemented but not reconciled`; lista de deuda visual y copy visible.

### Slice 6 — Treatments/Billing/Documents inventory only

- Objetivo: inventariar codigo existente y deuda visual sin rediseñar funcionalmente.
- Archivos probables: `features/treatments`, `features/billing`, `features/documents`, controllers y tests asociados solo para auditoria.
- Riesgo: mezclar releases 5/6/7 o abrir Billing/Documents como aceptados sin reconciliacion.
- Que NO tocar: APIs, permisos, migrations, payment/tax/CFDI/OCR/sharing behavior.
- Validacion requerida: documento de inventario y `git diff --check`.
- Criterios: cada modulo separado, sin cierre de release, sin Phase 2.

## 9. Relacion Con El Formato Fisico De Historia Clinica

Confirmaciones desde `docs/release-3-clinical-records-form-mapping.md` y codigo:

- Patient data no se duplica en Clinical Records.
- Patient demographics viven en Patients: nombre, fecha de nacimiento, sexo, ocupacion, estado civil, referido por, telefono principal y estado.
- Edad se deriva desde `Patient.DateOfBirth`; no se persiste.
- Cuestionario medico vive en Clinical Records mediante `ClinicalMedicalAnswer` con catalogo fijo `QuestionKey`, `Unknown` / `Yes` / `No` y `Details`.
- Motivo de consulta y signos vitales viven en `ClinicalEncounter`; notas de consulta pueden crear `ClinicalNote` append-only vinculada.
- Facturacion queda fuera de Clinical Records.
- `Requiere factura` y datos fiscales pertenecen a Billing/fiscal profile futuro, no al expediente clinico.
- Telefonos separados casa/oficina/celular quedan como follow-up de Patient Contact Details si el cliente lo confirma.

## 10. Recomendacion Documental

No se recomienda modificar `STATE — BigSmile.md` en este slice sin autorizacion explicita. Si se autoriza un cambio documental posterior, el texto sugerido exacto es:

```md
## Nota de reconciliacion UX / codigo existente

[Hecho] El trabajo activo posterior al cierre de Release 3 debe tratarse como `client-driven UX redesign / visual organization pass`.

[Hecho] El repositorio contiene codigo funcional existente en modulos posteriores al roadmap formal, incluyendo Odontogram, Treatments/Quotes, Billing, Documents, Dashboard y recordatorios/manual reminders. La presencia de codigo, rutas, permisos, migrations o tests en esos modulos no implica por si misma que Release 4, Release 5, Release 6, Release 7 o Phase 2 esten abiertas, aceptadas o cerradas.

[Hecho] Hasta una auditoria y aceptacion especifica por modulo, esos modulos posteriores deben clasificarse como `implemented but not formally accepted/reconciled`.

[Hecho] Los slices visuales pueden mejorar presentacion, organizacion, copy, color, microinteracciones, modales/drawers/tabs/sticky action bars y deuda UX sin cambiar backend, APIs, permissions, auth, tenant context, branch context, migrations ni scope funcional.
```

Tambien se recomienda actualizar en un slice documental posterior:

- `README.md`: agregar una seccion corta que distinga estado canonico aceptado vs codigo existente no reconciliado.
- `PROJECT_MAP.md`: agregar una nota operacional para futuros agentes sobre modulos implementados no reconciliados.
- `docs/product-roadmap.md`: mantener Releases 4-7 como planeadas, pero agregar una nota de drift auditado.

No se recomienda cambiar ADRs salvo que un futuro slice altere convenciones de UX compartidas, module boundaries, frontend state strategy, tenant/auth/authorization o acceptance state.

## 11. Riesgos

- Cerrar releases por presencia de codigo.
- Mezclar rediseño visual con funcionalidad nueva.
- Duplicar datos de Patient en Clinical.
- Contaminar Clinical con Billing.
- Abrir doctor/provider scope.
- Introducir modales/drawers sin accesibilidad.
- Perder densidad operativa por hacer todo mas vistoso pero menos usable.
- Crear abstracciones shared prematuras.
- Mantener labels internos de release/slice visibles al usuario final.
- Migrar colores hardcodeados de forma inconsistente en lugar de usar tokens.
- Presentar reminders manuales como automatizacion Phase 2.

## 12. Validacion Recomendada

Para este documento:

- `git diff --check`.

Para futuros slices frontend:

- `npm test -- --watch=false`.
- `npm run build -- --configuration production`.
- `git diff --check`.
- Grep de colores hardcodeados.
- Grep de HTTP directo en pages/components.
- Revision visual 1366x768, 1440x900, 390x844.

Para futuros slices backend:

- `dotnet build`.
- `dotnet test`.
- Architecture validation si existe.
- Tenant/cross-tenant tests.

En este slice no se deben ejecutar `npm` ni `dotnet` si solo se crea este documento.
