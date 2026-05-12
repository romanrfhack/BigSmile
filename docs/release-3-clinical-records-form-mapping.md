# Release 3 — Clinical Records Physical Form Mapping

## 1. Objetivo

Documentar una auditoria tecnica y un mapeo formal del formato fisico de historia clinica compartido por el cliente contra el estado real del repositorio BigSmile.

Este documento nacio como mapeo/auditoria. Actualmente tambien registra el cierre backend del slice `Release 3.5 — Medical Questionnaire Backend`, la integracion frontend acotada del cuestionario medico estructurado en Clinical Records, y el cierre backend del slice `Release 3.6 — Clinical Encounter / Vitals Backend`. Su objetivo sigue siendo separar ownership por modulo, evitar duplicar datos ya cubiertos por Patients, evitar contaminar Clinical Records con Billing o Scheduling, y proponer slices futuros pequenos y auditables para incorporar la captura clinica restante derivada del formato fisico.

## 2. Estado canonico y deriva detectada

Estado confirmado desde `STATE — BigSmile.md`, `README.md`, `PROJECT_MAP.md`, `docs/product-roadmap.md` y ADRs:

- Release 1 — Patients esta completada.
- Release 2 — Scheduling esta completada.
- `doctor-based views` siguen diferidas y no hay provider/doctor assignment en el codigo actual.
- Release 3 — Clinical Records no es una fase futura virgen: ya esta abierta y preservada por slices aceptados Release 3.1, 3.2, 3.3, 3.4, 3.5 y 3.6.
- La fase activa canonica actual es Phase 2 Expansion — Modern Operations, con slices aceptados hasta Phase 2.6.

Deriva frente al objetivo textual de este slice: la instruccion pide confirmar que Release 3 es la siguiente fase funcional. El estado canonico actual no confirma eso; indica que Release 3 ya existe en codigo y documentacion, y que debe preservarse. Por lo tanto, este documento trata el trabajo como mapeo/auditoria para una expansion futura acotada de Clinical Records, no como reapertura funcional de Release 3 desde cero.

## 3. Estado real encontrado en codigo

### Frontend Clinical Records

Existe implementacion real en `frontend/src/app/features/clinical-records`:

- pagina `clinical-record.page.ts` en ruta `/patients/:id/clinical-record`;
- `ClinicalRecordsApiService` con llamadas reales a `/api/patients/{patientId}/clinical-record`;
- `ClinicalRecordsApiService` con llamadas reales a `/api/patients/{patientId}/clinical-record/questionnaire`;
- no hay llamadas frontend todavia a `/api/patients/{patientId}/clinical-record/encounters`;
- facade con estado `currentRecord`, `loadingRecord`, `recordMissing`, `currentQuestionnaire`, `loadingQuestionnaire`, `questionnaireMissing` y errores;
- componentes para empty state, snapshot clinico, alergias actuales, notas, diagnosticos, timeline y snapshot history;
- componente de cuestionario medico estructurado con catalogo fijo, secciones, respuestas `Unknown` / `Yes` / `No` y `Details`;
- modelos TypeScript para `ClinicalRecord`, `ClinicalAllergy`, `ClinicalNote`, `ClinicalDiagnosis`, `ClinicalTimelineEntry`, `ClinicalSnapshotHistoryEntry`, `ClinicalMedicalQuestionnaire` y `ClinicalMedicalAnswer`.

Limitaciones actuales:

- no hay tabs formales en la pagina clinica actual; las secciones aparecen en flujo vertical;
- la UI del cuestionario medico es acotada al catalogo fijo de Release 3.5 y no introduce form builder, versionado ni sincronizacion automatica;
- no existe UI de signos vitales/encounters todavia;
- no existe `Encounter` como modelo frontend separado todavia;
- no existe header clinico read-only con summary strip segun la guia UX nueva;
- el paciente se consulta via `PatientsFacade`, lo cual es correcto para evitar duplicar identidad del paciente en Clinical.

### Backend Clinical Records

Existe implementacion real, no solo scaffold:

- controller `PatientClinicalRecordsController`;
- endpoint `GET /api/patients/{patientId}/clinical-record`;
- endpoint `POST /api/patients/{patientId}/clinical-record`;
- endpoint `PUT /api/patients/{patientId}/clinical-record`;
- endpoint `POST /api/patients/{patientId}/clinical-record/notes`;
- endpoint `POST /api/patients/{patientId}/clinical-record/diagnoses`;
- endpoint `POST /api/patients/{patientId}/clinical-record/diagnoses/{diagnosisId}/resolve`;
- endpoint `GET /api/patients/{patientId}/clinical-record/questionnaire`;
- endpoint `PUT /api/patients/{patientId}/clinical-record/questionnaire`;
- endpoint `GET /api/patients/{patientId}/clinical-record/encounters`;
- endpoint `POST /api/patients/{patientId}/clinical-record/encounters`;
- servicios de aplicacion `ClinicalRecordCommandService` y `ClinicalRecordQueryService`;
- repositorio `IClinicalRecordRepository` / `EfClinicalRecordRepository`;
- DTOs y mapping de read model.

Entidades existentes:

- `ClinicalRecord`: tenant-owned y patient-owned, con `TenantId`, `PatientId`, `MedicalBackgroundSummary`, `CurrentMedicationsSummary`, alergias, notas, diagnosticos, snapshot history y metadata minima;
- `ClinicalNote`: nota append-only con texto, fecha y usuario creador;
- `ClinicalDiagnosis`: diagnostico basico no codificado, estado `Active` / `Resolved`;
- `ClinicalAllergyEntry`: alergia actual con sustancia, resumen de reaccion y notas;
- `ClinicalSnapshotHistoryEntry`: historial acotado del snapshot base.
- `ClinicalMedicalAnswer`: respuesta tenant-owned, patient-owned y ligada a `ClinicalRecord`, con `QuestionKey`, `Answer`, `Details`, `UpdatedAtUtc` y `UpdatedByUserId`.
- `ClinicalEncounter`: consulta clinica tenant-owned, patient-owned y ligada a `ClinicalRecord`, con `OccurredAtUtc`, `ChiefComplaint`, `ConsultationType`, signos vitales opcionales, `ClinicalNoteId` opcional, `CreatedAtUtc` y `CreatedByUserId`.

No existen todavia:

- `ClinicalMedicalQuestionnaire`;
- `ClinicalVitals` como entidad separada; los signos vitales aceptados viven dentro de `ClinicalEncounter`.

### Permisos y autorizacion

Existen permisos `clinical.read` y `clinical.write` en `Permissions.cs`.

`RolePermissionCatalog` concede `clinical.read` y `clinical.write` a:

- `PlatformAdmin`;
- `TenantAdmin`.

`TenantUser` no recibe permisos clinicos en esta fase.

Las policies `AuthorizationPolicies.ClinicalRead` y `AuthorizationPolicies.ClinicalWrite` existen y protegen los endpoints clinicos.

### Patients

Patients existe y cubre:

- `FirstName`;
- `LastName`;
- `FullName` derivado;
- `DateOfBirth`;
- `Sex`;
- `Occupation`;
- `MaritalStatus`;
- `ReferredBy`;
- `PrimaryPhone`;
- `Email`;
- `IsActive`;
- `HasClinicalAlerts`;
- `ClinicalAlertsSummary`;
- responsible party inline.

No existen en Patient:

- telefono casa/oficina separado;
- telefono celular separado.

Nota de seguimiento: el mini-slice `Patient Demographics` agrego `Sex`, `Occupation`, `MaritalStatus` y `ReferredBy` a Patients. Clinical Records puede mostrar esos datos como contexto read-only desde Patients, pero no debe persistir copias en `ClinicalRecord`.

### Scheduling

Scheduling existe y es branch-aware:

- `Appointment` con `TenantId`, `BranchId`, `PatientId`, fechas, notas, estados, confirmacion, recordatorio manual y log manual;
- endpoints reales de calendario, cita, cancelacion, attended/no-show, confirmacion, reminder manual, follow-up, reminder log;
- frontend real en `features/scheduling`.

No hay provider/doctor assignment ni doctor-based views.

### Migrations y tenant enforcement

Existen migrations clinicas:

- `20260420165518_AddClinicalRecordFoundation`;
- `20260420212406_AddClinicalDiagnosesFoundation`;
- `20260421003354_AddClinicalSnapshotHistory`.
- `20260512125248_AddClinicalMedicalQuestionnaire`.
- `20260512180403_AddClinicalEncounterVitals`.

`AppDbContext` declara `DbSet<ClinicalRecord>`, `DbSet<ClinicalDiagnosis>`, `DbSet<ClinicalSnapshotHistoryEntry>`, `DbSet<ClinicalMedicalAnswer>` y `DbSet<ClinicalEncounter>`.

`AppDbContext` aplica query filter tenant-aware sobre `ClinicalRecord`, `ClinicalMedicalAnswer` y `ClinicalEncounter`, y valida writes tenant-owned en `SaveChanges` mediante `ITenantOwnedEntity`. Los hijos clinicos historicos se alcanzan a traves de `ClinicalRecord`; `ClinicalMedicalAnswer` y `ClinicalEncounter` tambien incluyen `TenantId` para enforcement centralizado.

### Tests clinicos existentes

Hay pruebas unitarias e integracion para:

- creacion y lectura tenant-scoped;
- `GET` devuelve null/404 cuando no existe expediente;
- bloqueo cross-tenant;
- no autocreacion al agregar nota o diagnostico;
- unicidad de expediente por paciente/tenant;
- orden de notas, diagnosticos, timeline y snapshot history;
- snapshot history solo en cambios efectivos;
- cuestionario medico backend tenant-scoped con lectura, upsert, catalogo fijo, validacion de `QuestionKey`, validacion de `Answer`, rechazo de duplicados, rechazo de `TenantId` en request y permisos `clinical.read` / `clinical.write`;
- encounters/vitals backend tenant-scoped con creacion, listado, validacion de vitals, validacion de `ConsultationType`, rechazo de `TenantId`/`CreatedByUserId` en request, paciente y expediente dentro del tenant actual, y permisos `clinical.read` / `clinical.write`;
- cuestionario medico frontend con pruebas de render, cambios `Unknown` / `Yes` / `No`, visibilidad de `Details`, payload normalizado y errores de carga/guardado;
- permisos clinicos ausentes para `TenantUser`;
- plataforma sin tenant context bloqueada para acceso clinico.

## 4. Tabla de mapeo campo por campo

Leyenda:

- Existe: `Si`, `Parcial`, `No`.
- Falta: `No`, `Parcial`, `Si`.
- El owner recomendado es el modulo o modelo que deberia ser fuente de verdad.

Actualizacion Release 3.5: las filas cuyo owner recomendado es `ClinicalMedicalQuestionnaire` ya cuentan con soporte backend fijo mediante `ClinicalMedicalAnswer`, catalogo permitido de `QuestionKey`, `Answer` `Unknown` / `Yes` / `No`, `Details` opcional acotado y endpoints `GET` / `PUT` de cuestionario. Tambien cuentan con UI frontend acotada dentro de la pantalla de Clinical Record, usando Patients como contexto read-only cuando esta disponible. No hay form builder, auto-sync de alergias, timeline enrichment, versionado del cuestionario ni apertura de Billing, Scheduling, Odontogram, Treatments, Documents o doctor/provider scope.

Actualizacion Release 3.6: las filas cuyo owner recomendado es `ClinicalNote / Encounter` para consulta/signos vitales ya cuentan con soporte backend mediante `ClinicalEncounter`, endpoints `GET` / `POST /api/patients/{patientId}/clinical-record/encounters`, `TenantId` y `CreatedByUserId` derivados de contexto, `ConsultationType` controlado, vitals acotados y `ClinicalNote` append-only opcional cuando se envia `noteText`. No hay frontend todavia, no hay PUT/DELETE de encounters, no hay nuevo modelo de eventos de timeline y no se abre Billing, Scheduling, Odontogram, Treatments, Documents ni doctor/provider scope.

| Campo del formato fisico | Owner recomendado | Existe en codigo | Falta | Accion recomendada | Riesgo |
| --- | --- | --- | --- | --- | --- |
| Fecha | Derived / Read-only / Encounter | Parcial | Parcial | Mostrar `ClinicalRecord.CreatedAtUtc` para fecha de creacion; para consulta clinica usar `ClinicalEncounter.OccurredAtUtc`. No guardar una fecha duplicada sin significado clinico claro. | Duplicar metadata o confundir fecha de expediente con fecha de consulta. |
| Nombre | Patient | Si | No | Leer desde `Patient.FullName`. No copiar a ClinicalRecord. | Duplicacion y desalineacion de identidad. |
| Edad | Derived / Read-only | Parcial | Parcial | Calcular desde `Patient.DateOfBirth` en UI/read model. No persistir edad. | Edad persistida queda obsoleta. |
| Fecha de nacimiento | Patient | Si | No | Leer desde `Patient.DateOfBirth`. | Duplicacion de datos personales. |
| Sexo | Patient | Si | No | Leer desde `Patient.Sex`; mostrar en Clinical solo como contexto read-only. | Duplicacion si se vuelve a capturar en Clinical. |
| Ocupacion | Patient | Si | No | Leer desde `Patient.Occupation`; no copiar a ClinicalRecord. | Duplicacion futura y busqueda inconsistente si se re-modela en Clinical. |
| Estado civil | Patient | Si | No | Leer desde `Patient.MaritalStatus`; no mezclar datos civiles en Clinical. | Mezclar datos civiles en Clinical. |
| Telefono casa/oficina | Patient | Parcial | Parcial | `PrimaryPhone` existe, pero no hay telefono por tipo. Decidir si Patients necesita phone slots separados antes de modelarlo. | Duplicar contacto o perder semantica del telefono. |
| Telefono celular | Patient | Parcial | Parcial | `PrimaryPhone` existe. Si el cliente necesita celular separado, abrir mini-slice Patient Contact Details. | Datos de contacto inconsistentes. |
| Referido por | Patient | Si | No | Leer desde `Patient.ReferredBy`; no abrir CRM/marketing avanzado. | Meter datos comerciales/marketing en Clinical. |
| Motivo de consulta | ClinicalEncounter | Si backend | Frontend | Implementado como `ClinicalEncounter.ChiefComplaint` en POST/listado de encounters. No copiar a Patient ni guardarlo como snapshot unico. | Perder historia por sobrescritura si se guarda como snapshot unico. |
| Requiere factura | Billing | No | Si | Fuera de Release 3 salvo decision explicita de Billing. | Contaminar Clinical con datos fiscales. |
| Datos de facturacion | Billing | No | Si | Fuera de Release 3. Debe vivir en Billing/fiscal profile futuro. | Datos fiscales sensibles en modulo incorrecto. |
| Tipo de consulta: Tratamiento / Urgencias | ClinicalEncounter | Si backend | Frontend | Implementado como `ConsultationType` controlado `Treatment` / `Urgency` / `Other`. No reabre Scheduling ni Treatments. | Abrir scope de scheduling/treatment sin modelo. |
| Temperatura | ClinicalEncounter | Si backend | Frontend | Implementado como `TemperatureC` opcional con rango acotado dentro de encounter, con fecha y autor. | Signos vitales sin contexto temporal si se extraen del encounter. |
| Tension arterial | ClinicalEncounter | Si backend | Frontend | Implementado como `BloodPressureSystolic` / `BloodPressureDiastolic` opcionales y validados como par dentro de encounter. | Valores clinicos sensibles sin estructura si se guardan como nota libre. |
| Peso | ClinicalEncounter | Si backend | Frontend | Implementado como `WeightKg` opcional y acotado dentro de encounter. | Dato clinico cambiante si se guarda como snapshot permanente. |
| Frecuencia respiratoria | ClinicalEncounter | Si backend | Frontend | Implementado como `RespiratoryRatePerMinute` opcional y acotado dentro de encounter. | Falta de contexto temporal/unidades si se separa del encounter. |
| Talla | ClinicalEncounter | Si backend | Frontend | Implementado como `HeightCm` opcional y acotado dentro de encounter. | Unidades ambiguas si no se mantiene el contrato en cm. |
| Frecuencia cardiaca | ClinicalEncounter | Si backend | Frontend | Implementado como `HeartRateBpm` opcional y acotado dentro de encounter. | Falta de contexto temporal/unidades si se separa del encounter. |
| Esta actualmente bajo tratamiento medico | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con respuesta Unknown/Yes/No y details opcional. | Texto libre imposible de explotar si se reabre fuera del catalogo fijo. |
| Toma habitualmente algun medicamento | ClinicalMedicalQuestionnaire | Si | No | `CurrentMedicationsSummary` sigue como snapshot libre; la respuesta estructurada no lo reemplaza ni lo sincroniza. | Duplicar medicamentos si se interpreta como fuente unica. |
| Le han practicado intervenciones quirurgicas | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Dato medico sensible sin audit trail si se mueve a texto libre. |
| Ha recibido transfusiones sanguineas | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico no visible si se oculta en notas. |
| Consumio o consume drogas | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details; tratar como dato sensible. | Privacidad y autorizacion clinica. |
| Ha presentado reacciones alergicas | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija; relacionar visualmente con alergias actuales, sin auto-sincronizar. | Duplicar con `ClinicalAllergyEntry`. |
| Penicilina | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija; details opcional no crea alergia actual. | Inconsistencia entre questionnaire y alergias actuales. |
| Anestesicos | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija por sustancia. | Riesgo clinico alto si se oculta. |
| Aspirina | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija por sustancia. | Duplicacion con alergias actuales. |
| Sulfas | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija por sustancia. | Duplicacion con alergias actuales. |
| Yodo | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija por sustancia. | Duplicacion con alergias actuales. |
| Otro alimento, sustancia o medicamento | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details opcional; no crea alergia actual. | Texto libre dificil de normalizar. |
| Presion arterial alta / Hipertension | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico si no aparece en summary. |
| Presion arterial baja / Hipotension | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico si no aparece en summary. |
| Sangrado excesivo ante heridas | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Alteraciones sanguineas o de coagulacion | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Anemia, hemofilia, deficiencia de vitamina K | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija ligada al catalogo estable. | Granularidad excesiva si se remodela sin catalogo claro. |
| Toma algun retroviral | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Privacidad alta. |
| Experiencias desfavorables con el dentista | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Dato cualitativo perdido si solo es boolean. |
| Tuvo COVID | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details opcional. | Puede volverse historico irrelevante sin fecha/details. |
| Enfermedades de transmision sexual | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details; sensible. | Privacidad y acceso clinico. |
| Enfermedades del corazon de nacimiento o actualmente | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Hepatitis A, B, C u otra | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details para tipo. | Riesgo clinico y privacidad. |
| Endocarditis | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Crisis o convulsiones | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Diabetes | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Tuberculosis | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico y privacidad. |
| Hipertiroidismo | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico medio. |
| Hipotiroidismo | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico medio. |
| Infarto o angina de pecho | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Cirugia a corazon abierto | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Herpes o aftas recurrentes | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico medio. |
| Se muerde las unas o labios | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details; no abre odontogram/treatment linkage. | Dato conductual puede requerir odontogram/treatment linkage futuro; no abrir ahora. |
| Fuma | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Dato de riesgo clinico no estructurado. |
| Consumo de alimentos citricos o acidos | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Puede contaminar odontogram si se liga prematuramente. |
| Apretamiento o rechinido de dientes por las noches | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details; no abre tratamiento/odontogram linkage. | Puede reabrir tratamientos por accidente. |
| Lactando, embarazada o sospecha de embarazo | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details; sensible. | Riesgo clinico alto y privacidad. |
| Toma medicamentos anticonceptivos | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Privacidad. |
| Complicaciones asociadas a la anestesia | ClinicalMedicalQuestionnaire | Si | No | Cubierto por pregunta fija con details. | Riesgo clinico alto. |
| Notas | ClinicalMedicalQuestionnaire / ClinicalNote / Encounter | Parcial | Parcial | Si son notas del cuestionario, guardarlas como details/general notes del cuestionario; si son notas de consulta, usar `ClinicalNote` o future Encounter. | Mezclar notas administrativas, intake y evolucion clinica. |

## 5. Campos que NO deben duplicarse

Estos campos deben leerse desde Patients o derivarse. Clinical Records puede mostrarlos como header read-only, pero no debe persistir copias:

- Nombre: existe como `Patient.FullName`.
- Fecha de nacimiento: existe como `Patient.DateOfBirth`.
- Edad: debe derivarse desde `DateOfBirth`; no debe persistirse.
- Telefonos: `Patient.PrimaryPhone` existe parcialmente. Si se requieren casa/oficina/celular separados, debe abrirse un mini-slice de Patient Contact Details.
- Estado del paciente: existe como `Patient.IsActive`.
- Alertas clinicas basicas existentes: existen como `Patient.HasClinicalAlerts` y `Patient.ClinicalAlertsSummary`.

## 6. Campos probablemente Patient Demographics

Los siguientes campos pertenecen mejor a Patient porque describen identidad/demografia o procedencia del paciente, no el expediente clinico:

| Campo | Existe en Patient | Recomendacion |
| --- | --- | --- |
| Sexo | Si | Mantener como campo controlado de Patient y mostrarlo en Clinical solo read-only. |
| Ocupacion | Si | Mantener como string corto de Patient. |
| Estado civil | Si | Mantener como campo controlado de Patient. |
| Referido por | Si | Mantener como string corto de Patient, sin abrir marketing/CRM avanzado. |

Recomendacion: este ownership ya quedo resuelto por el mini-slice `Patient Demographics`. Cualquier implementacion clinica futura debe consumir estos datos desde Patients y no duplicarlos en Clinical Records.

## 7. Campos Billing

Los siguientes campos deben quedar fuera de Release 3 salvo decision explicita:

- Requiere factura.
- Datos de facturacion.

Owner recomendado: Billing o futuro fiscal profile del paciente/tenant, no Clinical Records.

Razon: son datos fiscales/comerciales, no antecedentes clinicos. Meterlos en Clinical crearia acoplamiento incorrecto, riesgos de privacidad y posible duplicacion con billing documents o facturacion electronica futura.

## 8. Propuesta de modelo Clinical Records

Modelo recomendado para evolucionar sin romper los slices aceptados:

### ClinicalRecord

Mantener como aggregate tenant-owned y patient-owned:

- exactamente un expediente activo por Patient/Tenant;
- creacion explicita;
- snapshot base actual: background summary, medications summary, current allergies;
- audit metadata;
- read model que compone secciones clinicas sin duplicar Patient.

### ClinicalMedicalAnswer

Mantener la coleccion estructurada de respuestas clinicas por expediente:

- `TenantId`;
- `ClinicalRecordId`;
- `QuestionKey`;
- `Answer` (`Unknown`, `Yes`, `No`);
- `Details` nullable;
- `UpdatedAtUtc`;
- `UpdatedByUserId`.

Decision vigente de Release 3.5: la entidad vive como `ClinicalMedicalAnswer` ligada a `ClinicalRecord`, sin introducir un contenedor versionable `ClinicalMedicalQuestionnaire`. El frontend consume y guarda el catalogo fijo; no hay form builder avanzado.

### ClinicalNote / Encounter

`ClinicalNote` ya existe y debe seguir append-only para notas generales.

Decision vigente de Release 3.6: signos vitales, motivo de consulta y tipo de consulta viven en un `ClinicalEncounter` acotado, porque esos datos tienen fecha, autor y contexto de consulta:

- `ClinicalEncounter`;
- `ChiefComplaint`;
- `ConsultationType` (`Treatment`, `Urgency`, `Other`);
- vitals opcionales con unidades explicitas;
- `ClinicalNote` append-only opcional cuando `noteText` se envia al crear el encounter;
- `OccurredAtUtc`, `CreatedAtUtc` y `CreatedByUserId`.

No hay PUT/DELETE de encounters, no hay UI todavia y no se abre doctor/provider assignment en este modelo.

### ClinicalTimeline

El timeline actual es read model construido desde notas y diagnosticos, sin tabla propia. Mantener esa regla.

Cuando existan questionnaire o encounter events, agregar eventos al timeline solo mediante un slice explicito. No mezclar `snapshotHistory` con timeline y no crear timeline cross-module.

## 9. Catalogo fijo implementado de preguntas medicas

Respuesta recomendada para cada pregunta:

- `Unknown`: sin respuesta capturada o no preguntado todavia.
- `Yes`: respuesta afirmativa.
- `No`: respuesta negativa.
- `Details`: texto opcional para contexto, fecha, tipo, medicamento, reaccion o aclaracion.

QuestionKey debe ser estable y no depender del texto visible. El label vive en i18n del frontend. Las keys implementadas por backend/frontend son lower camelCase.

| QuestionKey | Label i18n sugerido | Answer | Details |
| --- | --- | --- | --- |
| `currentMedicalTreatment` | Esta actualmente bajo tratamiento medico | Unknown/Yes/No | Opcional |
| `regularMedication` | Toma habitualmente algun medicamento | Unknown/Yes/No | Opcional |
| `priorSurgery` | Le han practicado intervenciones quirurgicas | Unknown/Yes/No | Opcional |
| `bloodTransfusion` | Ha recibido transfusiones sanguineas | Unknown/Yes/No | Opcional |
| `drugUse` | Consumio o consume drogas | Unknown/Yes/No | Opcional |
| `allergicReactions` | Ha presentado reacciones alergicas | Unknown/Yes/No | Opcional |
| `allergyPenicillin` | Penicilina | Unknown/Yes/No | Reaccion opcional |
| `allergyAnesthetics` | Anestesicos | Unknown/Yes/No | Reaccion opcional |
| `allergyAspirin` | Aspirina | Unknown/Yes/No | Reaccion opcional |
| `allergySulfas` | Sulfas | Unknown/Yes/No | Reaccion opcional |
| `allergyIodine` | Yodo | Unknown/Yes/No | Reaccion opcional |
| `allergyOther` | Otro alimento, sustancia o medicamento | Unknown/Yes/No | Opcional |
| `hypertension` | Presion arterial alta / Hipertension | Unknown/Yes/No | Opcional |
| `hypotension` | Presion arterial baja / Hipotension | Unknown/Yes/No | Opcional |
| `excessiveBleeding` | Sangrado excesivo ante heridas | Unknown/Yes/No | Opcional |
| `bloodOrCoagulationDisorder` | Alteraciones sanguineas o de coagulacion | Unknown/Yes/No | Opcional |
| `anemiaHemophiliaVitaminKDeficiency` | Anemia, hemofilia o deficiencia de vitamina K | Unknown/Yes/No | Opcional |
| `retroviralTreatment` | Toma algun retroviral | Unknown/Yes/No | Opcional |
| `badDentalExperience` | Experiencias desfavorables con el dentista | Unknown/Yes/No | Opcional |
| `covidHistory` | Tuvo COVID | Unknown/Yes/No | Opcional |
| `sexuallyTransmittedDisease` | Enfermedades de transmision sexual | Unknown/Yes/No | Opcional |
| `congenitalOrCurrentHeartDisease` | Enfermedades del corazon de nacimiento o actualmente | Unknown/Yes/No | Opcional |
| `hepatitis` | Hepatitis A, B, C u otra | Unknown/Yes/No | Tipo/details |
| `endocarditis` | Endocarditis | Unknown/Yes/No | Opcional |
| `seizures` | Crisis o convulsiones | Unknown/Yes/No | Opcional |
| `diabetes` | Diabetes | Unknown/Yes/No | Opcional |
| `tuberculosis` | Tuberculosis | Unknown/Yes/No | Opcional |
| `hyperthyroidism` | Hipertiroidismo | Unknown/Yes/No | Opcional |
| `hypothyroidism` | Hipotiroidismo | Unknown/Yes/No | Opcional |
| `heartAttackOrAngina` | Infarto o angina de pecho | Unknown/Yes/No | Opcional |
| `openHeartSurgery` | Cirugia a corazon abierto | Unknown/Yes/No | Opcional |
| `recurrentHerpesOrAphthae` | Herpes o aftas recurrentes | Unknown/Yes/No | Opcional |
| `bitesNailsOrLips` | Se muerde las unas o labios | Unknown/Yes/No | Opcional |
| `smokes` | Fuma | Unknown/Yes/No | Opcional |
| `acidicFoodConsumption` | Consumo de alimentos citricos o acidos | Unknown/Yes/No | Opcional |
| `bruxismAtNight` | Apretamiento o rechinido de dientes por las noches | Unknown/Yes/No | Opcional |
| `pregnantLactatingOrSuspected` | Lactando, embarazada o sospecha de embarazo | Unknown/Yes/No | Opcional |
| `contraceptiveMedication` | Toma medicamentos anticonceptivos | Unknown/Yes/No | Opcional |
| `anesthesiaComplications` | Complicaciones asociadas a la anestesia | Unknown/Yes/No | Opcional |

Notas generales del cuestionario: no existe campo general independiente en el contrato actual; el frontend solo guarda `Details` por pregunta. Para notas de consulta, seguir usando `ClinicalNote` o el backend `ClinicalEncounter` aceptado en Release 3.6. No crear preguntas dinamicas ni form builder avanzado en este alcance.

## 10. UX implementada y evolucion futura

Seguir `docs/frontend-ux-guidelines.md` y ADR 005.

Implementado en el slice de UI acotado:

- Header de paciente read-only usando Patients: nombre, edad derivada, fecha de nacimiento, telefono principal, estado activo/inactivo, demografia disponible y alertas clinicas.
- Seccion `Antecedentes medicos` dentro del flujo vertical existente de Clinical Record; no se introdujo un sistema de tabs nuevo.
- Catalogo fijo por grupos, respuesta Unknown/Yes/No, details visible cuando la respuesta es `Yes` o ya existe contenido.
- Sticky action bar para guardar/cancelar el formulario largo.
- Loading, empty, error y saving states dentro del area clinica.
- Uso de shared UI foundation y tokens `--bsm-*`.
- Sin sincronizacion automatica hacia alergias actuales, snapshot history o timeline.

Pendiente para slices futuros, solo si se abre alcance:

- Summary strip: alergias actuales, medicamentos actuales, alertas, ultima actualizacion, diagnosticos activos.
- Tabs:
  - Resumen.
  - Antecedentes medicos.
  - Consulta / notas.
  - Timeline.
- `Consulta / notas`: motivo de consulta, signos vitales y nota usando el backend `ClinicalEncounter`; la UI queda pendiente para un slice futuro.
- Error states diferenciados: falta expediente, sin permiso, error de carga, error de validacion.

## 11. Propuesta de implementacion por slices

### Slice 1 — Backend clinical record foundation reconciliation

El foundation ya existe. Si se abre un nuevo slice, debe ser de reconciliacion:

- no cambiar permisos;
- no cambiar endpoints existentes salvo necesidad documentada;
- verificar que el nuevo mapeo no rompa Release 3.1-3.4;
- documentar cualquier contrato nuevo antes de implementarlo.

Validacion recomendada:

- unit tests de dominio clinico existentes;
- integration tests de tenant isolation clinico;
- controller tests de 404/no autocreacion;
- `dotnet test` solo si se toca codigo.

### Slice 2 — Medical questionnaire backend

Implementado como `Release 3.5 — Medical Questionnaire Backend` con catalogo fijo:

- entidad `ClinicalMedicalAnswer` tenant-owned, patient-owned y ligada a `ClinicalRecord`;
- `QuestionKey`, `Answer`, `Details`;
- endpoint de lectura y guardado acotado: `GET` / `PUT /api/patients/{patientId}/clinical-record/questionnaire`;
- sin form builder;
- sin permisos nuevos, reutiliza `clinical.read` / `clinical.write`;
- sin sincronizacion automatica hacia alergias actuales.

Validacion cubierta:

- cross-tenant read/write prohibido;
- no autocreacion de ClinicalRecord;
- unknown/yes/no validation;
- QuestionKey desconocido rechazado;
- details length validado;
- TenantUser sigue sin permisos clinicos.

### Slice 3 — Clinical encounter/vitals backend

Implementado como `Release 3.6 — Clinical Encounter / Vitals Backend`:

- `ClinicalEncounter` tenant-owned, patient-owned y ligado a un `ClinicalRecord` existente;
- `GET` / `POST /api/patients/{patientId}/clinical-record/encounters`;
- vitals con unidades/formatos claros y rangos basicos;
- nota append-only vinculada a `ClinicalNote` cuando `noteText` viene informado;
- sin PUT/DELETE;
- sin frontend;
- sin nuevo modelo de eventos de timeline;
- sin doctor/provider assignment.

Validacion cubierta:

- vitals con rangos/formato basico;
- tenant isolation y bloqueo cross-tenant;
- rechazo de `TenantId` y `CreatedByUserId` desde request;
- permisos `clinical.read` / `clinical.write`;
- no cross-module timeline;
- no cambios a Scheduling, Treatments ni Billing.

### Slice 4 — Frontend integration

Implementado de forma acotada en `features/clinical-records`:

- header de paciente read-only usando Patients;
- sin summary strip avanzado ni tabs nuevos;
- formulario de antecedentes medicos con catalogo fijo;
- estados loading/empty/error;
- sticky action bar;
- mantener HTTP en data-access y orquestacion en facade.

Validacion cubierta/recomendada:

- frontend unit/component tests para render, cambios de answer, details, payload normalizado y errores;
- component tests para estados y permisos;
- route guard sigue usando `clinical.read`;
- no mostrar Billing/Scheduling/doctor scope como implementado.

### Slice 5 — UX/QA closure docs

Cerrar y mantener el flujo con evidencia:

- revisar consistencia visual con `docs/frontend-ux-guidelines.md`;
- actualizar docs de Release 3 solo si el slice queda implementado y probado;
- mantener `STATE — BigSmile.md` alineado con el estado real validado.

Validacion recomendada:

- build frontend/backend si hubo codigo;
- pruebas clinicas relevantes;
- `git diff --check`;
- review packet con tenant/security implications.

## 12. Riesgos

- Cross-tenant clinical data leak: cualquier entidad nueva de antecedentes medicos debe tener ownership tenant-safe y pruebas.
- Permisos `clinical.*` ambiguos: no agregar permisos todavia; si se agregan, documentar matriz y pruebas.
- Duplicacion de patient data: nombre, fecha de nacimiento, edad, telefonos, estado y alertas deben ser read-only desde Patient.
- Contaminar Clinical con Billing: facturacion debe quedar fuera de Clinical Records.
- Abrir advanced forms intake: el cuestionario debe ser catalogo fijo, no form builder.
- Abrir doctor/provider scope por accidente: encounters y notas no deben requerir doctor assignment hasta un slice dedicado.
- Mezclar snapshot history y timeline: mantener secciones separadas.
- Sincronizacion automatica de alergias: no inferir alergias actuales desde respuestas yes/no sin accion explicita y reglas claras.

## 13. Validacion recomendada por slice

| Slice | Validacion minima |
| --- | --- |
| Documentacion / mapeo | `git diff --check`; revisar que no haya codigo, migrations ni permisos nuevos. |
| Backend questionnaire | Unit tests de dominio; integration tests cross-tenant; API/controller tests; authorization tests; `dotnet test`. |
| Backend encounter/vitals | Unit tests de rangos/formato; integration tests tenant-safe; timeline tests si se enriquece el read model; `dotnet test`. |
| Frontend integration | Unit/component tests; route guard permissions; estados loading/empty/error; `npm test` o suite relevante. |
| UX/QA closure | Frontend build; backend build si aplica; revision visual desktop/tablet/mobile; documentacion alineada si se completa un slice funcional. |

## 14. Decision recomendada inmediata

Estado inmediato: `Patient Demographics` se mantiene como fuente de verdad para sexo, ocupacion, estado civil y referido por; Clinical Records ya consume esos datos como contexto read-only cuando estan disponibles. La UI del cuestionario medico fijo de Release 3.5 ya esta integrada contra los endpoints backend existentes. El backend de encounters/vitals de Release 3.6 ya existe, pero su UI queda pendiente. Billing fields deben quedar fuera hasta un slice fiscal/billing explicito.
