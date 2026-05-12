# Release 3 — Clinical Records Physical Form Mapping

## 1. Objetivo

Documentar una auditoria tecnica y un mapeo formal del formato fisico de historia clinica compartido por el cliente contra el estado real del repositorio BigSmile.

Este documento nacio como mapeo/auditoria. Actualmente tambien registra el cierre backend del slice `Release 3.5 — Medical Questionnaire Backend`. Su objetivo sigue siendo separar ownership por modulo, evitar duplicar datos ya cubiertos por Patients, evitar contaminar Clinical Records con Billing o Scheduling, y proponer slices futuros pequenos y auditables para incorporar la captura clinica restante derivada del formato fisico.

## 2. Estado canonico y deriva detectada

Estado confirmado desde `STATE — BigSmile.md`, `README.md`, `PROJECT_MAP.md`, `docs/product-roadmap.md` y ADRs:

- Release 1 — Patients esta completada.
- Release 2 — Scheduling esta completada.
- `doctor-based views` siguen diferidas y no hay provider/doctor assignment en el codigo actual.
- Release 3 — Clinical Records no es una fase futura virgen: ya esta abierta y preservada por slices aceptados Release 3.1, 3.2, 3.3, 3.4 y 3.5.
- La fase activa canonica actual es Phase 2 Expansion — Modern Operations, con slices aceptados hasta Phase 2.6.

Deriva frente al objetivo textual de este slice: la instruccion pide confirmar que Release 3 es la siguiente fase funcional. El estado canonico actual no confirma eso; indica que Release 3 ya existe en codigo y documentacion, y que debe preservarse. Por lo tanto, este documento trata el trabajo como mapeo/auditoria para una expansion futura acotada de Clinical Records, no como reapertura funcional de Release 3 desde cero.

## 3. Estado real encontrado en codigo

### Frontend Clinical Records

Existe implementacion real en `frontend/src/app/features/clinical-records`:

- pagina `clinical-record.page.ts` en ruta `/patients/:id/clinical-record`;
- `ClinicalRecordsApiService` con llamadas reales a `/api/patients/{patientId}/clinical-record`;
- facade con estado `currentRecord`, `loadingRecord`, `recordMissing` y errores;
- componentes para empty state, snapshot clinico, alergias actuales, notas, diagnosticos, timeline y snapshot history;
- modelos TypeScript para `ClinicalRecord`, `ClinicalAllergy`, `ClinicalNote`, `ClinicalDiagnosis`, `ClinicalTimelineEntry` y `ClinicalSnapshotHistoryEntry`.

Limitaciones actuales:

- no hay tabs formales en la pagina clinica actual; las secciones aparecen en flujo vertical;
- no existe UI de cuestionario medico estructurado; el backend fijo ya existe en Release 3.5;
- no existen signos vitales;
- no existe `Encounter` como modelo separado;
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

No existen todavia:

- `ClinicalMedicalQuestionnaire`;
- `ClinicalEncounter`;
- `ClinicalVitals`;
- endpoint dedicado para encounter / vitals.

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

`AppDbContext` declara `DbSet<ClinicalRecord>`, `DbSet<ClinicalDiagnosis>`, `DbSet<ClinicalSnapshotHistoryEntry>` y `DbSet<ClinicalMedicalAnswer>`.

`AppDbContext` aplica query filter tenant-aware sobre `ClinicalRecord` y `ClinicalMedicalAnswer`, y valida writes tenant-owned en `SaveChanges` mediante `ITenantOwnedEntity`. Los hijos clinicos historicos se alcanzan a traves de `ClinicalRecord`; `ClinicalMedicalAnswer` tambien incluye `TenantId` para enforcement centralizado.

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
- permisos clinicos ausentes para `TenantUser`;
- plataforma sin tenant context bloqueada para acceso clinico.

## 4. Tabla de mapeo campo por campo

Leyenda:

- Existe: `Si`, `Parcial`, `No`.
- Falta: `No`, `Parcial`, `Si`.
- El owner recomendado es el modulo o modelo que deberia ser fuente de verdad.

Actualizacion Release 3.5: las filas cuyo owner recomendado es `ClinicalMedicalQuestionnaire` ya cuentan con soporte backend fijo mediante `ClinicalMedicalAnswer`, catalogo permitido de `QuestionKey`, `Answer` `Unknown` / `Yes` / `No`, `Details` opcional acotado y endpoints `GET` / `PUT` de cuestionario. Sigue faltando la UI del cuestionario y no hay form builder, auto-sync de alergias ni timeline enrichment.

| Campo del formato fisico | Owner recomendado | Existe en codigo | Falta | Accion recomendada | Riesgo |
| --- | --- | --- | --- | --- | --- |
| Fecha | Derived / Read-only | Parcial | Parcial | Mostrar `ClinicalRecord.CreatedAtUtc` para fecha de creacion; cuando exista encounter, usar fecha del encounter. No guardar una fecha duplicada sin significado clinico claro. | Duplicar metadata o confundir fecha de expediente con fecha de consulta. |
| Nombre | Patient | Si | No | Leer desde `Patient.FullName`. No copiar a ClinicalRecord. | Duplicacion y desalineacion de identidad. |
| Edad | Derived / Read-only | Parcial | Parcial | Calcular desde `Patient.DateOfBirth` en UI/read model. No persistir edad. | Edad persistida queda obsoleta. |
| Fecha de nacimiento | Patient | Si | No | Leer desde `Patient.DateOfBirth`. | Duplicacion de datos personales. |
| Sexo | Patient | Si | No | Leer desde `Patient.Sex`; mostrar en Clinical solo como contexto read-only. | Duplicacion si se vuelve a capturar en Clinical. |
| Ocupacion | Patient | Si | No | Leer desde `Patient.Occupation`; no copiar a ClinicalRecord. | Duplicacion futura y busqueda inconsistente si se re-modela en Clinical. |
| Estado civil | Patient | Si | No | Leer desde `Patient.MaritalStatus`; no mezclar datos civiles en Clinical. | Mezclar datos civiles en Clinical. |
| Telefono casa/oficina | Patient | Parcial | Parcial | `PrimaryPhone` existe, pero no hay telefono por tipo. Decidir si Patients necesita phone slots separados antes de modelarlo. | Duplicar contacto o perder semantica del telefono. |
| Telefono celular | Patient | Parcial | Parcial | `PrimaryPhone` existe. Si el cliente necesita celular separado, abrir mini-slice Patient Contact Details. | Datos de contacto inconsistentes. |
| Referido por | Patient | Si | No | Leer desde `Patient.ReferredBy`; no abrir CRM/marketing avanzado. | Meter datos comerciales/marketing en Clinical. |
| Motivo de consulta | ClinicalNote / Encounter | No | Si | Modelar como chief complaint del primer encounter o nota de consulta, no como campo demografico. | Perder historia por sobrescritura si se guarda como snapshot unico. |
| Requiere factura | Billing | No | Si | Fuera de Release 3 salvo decision explicita de Billing. | Contaminar Clinical con datos fiscales. |
| Datos de facturacion | Billing | No | Si | Fuera de Release 3. Debe vivir en Billing/fiscal profile futuro. | Datos fiscales sensibles en modulo incorrecto. |
| Tipo de consulta: Tratamiento / Urgencias | ClinicalNote / Encounter | No | Si | Capturarlo como tipo de encounter clinico si se abre esa entidad; no reabrir Scheduling ni Treatments por este campo. | Abrir scope de scheduling/treatment sin modelo. |
| Temperatura | ClinicalNote / Encounter | No | Si | Capturar como vitals dentro de encounter, con fecha y autor. | Signos vitales sin contexto temporal. |
| Tension arterial | ClinicalNote / Encounter | No | Si | Capturar como vitals dentro de encounter. | Valores clinicos sensibles sin estructura. |
| Peso | ClinicalNote / Encounter | No | Si | Capturar como vitals dentro de encounter. | Dato clinico cambiante si se guarda como snapshot permanente. |
| Frecuencia respiratoria | ClinicalNote / Encounter | No | Si | Capturar como vitals dentro de encounter. | Falta de contexto temporal/unidades. |
| Talla | ClinicalNote / Encounter | No | Si | Capturar como vitals dentro de encounter. | Unidades ambiguas. |
| Frecuencia cardiaca | ClinicalNote / Encounter | No | Si | Capturar como vitals dentro de encounter. | Falta de contexto temporal/unidades. |
| Esta actualmente bajo tratamiento medico | ClinicalMedicalQuestionnaire | No | Si | Agregar como pregunta fija con respuesta Unknown/Yes/No y details opcional. | Texto libre imposible de explotar. |
| Toma habitualmente algun medicamento | ClinicalMedicalQuestionnaire | Parcial | Parcial | `CurrentMedicationsSummary` existe como snapshot libre; agregar respuesta estructurada sin eliminar el summary. | Duplicar medicamentos si no hay regla clara. |
| Le han practicado intervenciones quirurgicas | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Dato medico sensible sin audit trail. |
| Ha recibido transfusiones sanguineas | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico no visible. |
| Consumio o consume drogas | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details; tratar como dato sensible. | Privacidad y autorizacion clinica. |
| Ha presentado reacciones alergicas | ClinicalMedicalQuestionnaire | Parcial | Parcial | Pregunta fija; relacionar visualmente con alergias actuales, sin auto-sincronizar en primer slice. | Duplicar con `ClinicalAllergyEntry`. |
| Penicilina | ClinicalMedicalQuestionnaire | Parcial | Parcial | Pregunta fija; si `Yes`, permitir detalle y posible accion explicita para crear alergia actual. | Inconsistencia entre questionnaire y alergias actuales. |
| Anestesicos | ClinicalMedicalQuestionnaire | Parcial | Parcial | Igual que otras alergias por sustancia. | Riesgo clinico alto si se oculta. |
| Aspirina | ClinicalMedicalQuestionnaire | Parcial | Parcial | Igual que otras alergias por sustancia. | Duplicacion con alergias actuales. |
| Sulfas | ClinicalMedicalQuestionnaire | Parcial | Parcial | Igual que otras alergias por sustancia. | Duplicacion con alergias actuales. |
| Yodo | ClinicalMedicalQuestionnaire | Parcial | Parcial | Igual que otras alergias por sustancia. | Duplicacion con alergias actuales. |
| Otro alimento, sustancia o medicamento | ClinicalMedicalQuestionnaire | Parcial | Parcial | Pregunta fija con details requerido cuando answer sea `Yes`; posible accion explicita hacia alergias actuales. | Texto libre dificil de normalizar. |
| Presion arterial alta / Hipertension | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico si no aparece en summary. |
| Presion arterial baja / Hipotension | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico si no aparece en summary. |
| Sangrado excesivo ante heridas | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Alteraciones sanguineas o de coagulacion | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Anemia, hemofilia, deficiencia de vitamina K | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija o subpregunta ligada a coagulacion; mantener QuestionKey estable. | Granularidad excesiva si se modela sin catalogo claro. |
| Toma algun retroviral | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Privacidad alta. |
| Experiencias desfavorables con el dentista | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details; util para UX clinica. | Dato cualitativo perdido si solo es boolean. |
| Tuvo COVID | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details opcional. | Puede volverse historico irrelevante sin fecha/details. |
| Enfermedades de transmision sexual | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details; sensible. | Privacidad y acceso clinico. |
| Enfermedades del corazon de nacimiento o actualmente | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Hepatitis A, B, C u otra | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details para tipo. | Riesgo clinico y privacidad. |
| Endocarditis | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Crisis o convulsiones | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Diabetes | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Tuberculosis | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico y privacidad. |
| Hipertiroidismo | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico medio. |
| Hipotiroidismo | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico medio. |
| Infarto o angina de pecho | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Cirugia a corazon abierto | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
| Herpes o aftas recurrentes | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico medio. |
| Se muerde las unas o labios | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Dato conductual puede requerir odontogram/treatment linkage futuro; no abrir ahora. |
| Fuma | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Dato de riesgo clinico no estructurado. |
| Consumo de alimentos citricos o acidos | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Puede contaminar odontogram si se liga prematuramente. |
| Apretamiento o rechinido de dientes por las noches | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details; no abrir tratamiento/odontogram linkage. | Puede reabrir tratamientos por accidente. |
| Lactando, embarazada o sospecha de embarazo | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details; sensible. | Riesgo clinico alto y privacidad. |
| Toma medicamentos anticonceptivos | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Privacidad. |
| Complicaciones asociadas a la anestesia | ClinicalMedicalQuestionnaire | No | Si | Pregunta fija con details. | Riesgo clinico alto. |
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

Agregar una coleccion estructurada de respuestas clinicas por expediente o por cuestionario de intake:

- `TenantId`;
- `ClinicalRecordId`;
- `QuestionKey`;
- `Answer` (`Unknown`, `Yes`, `No`);
- `Details` nullable;
- `UpdatedAtUtc`;
- `UpdatedByUserId`.

Decision pendiente para el slice de implementacion: si la entidad vive como `ClinicalMedicalAnswer` hija de `ClinicalRecord` o si se introduce `ClinicalMedicalQuestionnaire` como contenedor versionable. Para el primer slice, evitar form builder avanzado y usar catalogo fijo.

### ClinicalNote / Encounter

`ClinicalNote` ya existe y debe seguir append-only para notas generales.

Para signos vitales, motivo de consulta y tipo de consulta, conviene abrir un `ClinicalEncounter` acotado antes de meter esos campos en `ClinicalNote`, porque esos datos tienen fecha, autor y contexto de consulta:

- `ClinicalEncounter`;
- `ChiefComplaint`;
- `ConsultationType`;
- `Vitals`;
- nota clinica opcional;
- `OccurredAtUtc` / `CreatedByUserId`.

No abrir doctor/provider assignment en este modelo.

### ClinicalTimeline

El timeline actual es read model construido desde notas y diagnosticos, sin tabla propia. Mantener esa regla.

Cuando existan questionnaire o encounter events, agregar eventos al timeline solo mediante un slice explicito. No mezclar `snapshotHistory` con timeline y no crear timeline cross-module.

## 9. Catalogo fijo propuesto de preguntas medicas

Respuesta recomendada para cada pregunta:

- `Unknown`: sin respuesta capturada o no preguntado todavia.
- `Yes`: respuesta afirmativa.
- `No`: respuesta negativa.
- `Details`: texto opcional para contexto, fecha, tipo, medicamento, reaccion o aclaracion.

QuestionKey debe ser estable y no depender del texto visible. El label debe vivir en i18n del frontend.

| QuestionKey | Label i18n sugerido | Answer | Details |
| --- | --- | --- | --- |
| `UnderMedicalTreatment` | Esta actualmente bajo tratamiento medico | Unknown/Yes/No | Opcional |
| `RegularMedication` | Toma habitualmente algun medicamento | Unknown/Yes/No | Opcional |
| `SurgeryHistory` | Le han practicado intervenciones quirurgicas | Unknown/Yes/No | Opcional |
| `BloodTransfusionHistory` | Ha recibido transfusiones sanguineas | Unknown/Yes/No | Opcional |
| `DrugUseHistory` | Consumio o consume drogas | Unknown/Yes/No | Opcional |
| `AllergicReactionHistory` | Ha presentado reacciones alergicas | Unknown/Yes/No | Opcional |
| `AllergyPenicillin` | Penicilina | Unknown/Yes/No | Reaccion opcional |
| `AllergyAnesthetics` | Anestesicos | Unknown/Yes/No | Reaccion opcional |
| `AllergyAspirin` | Aspirina | Unknown/Yes/No | Reaccion opcional |
| `AllergySulfas` | Sulfas | Unknown/Yes/No | Reaccion opcional |
| `AllergyIodine` | Yodo | Unknown/Yes/No | Reaccion opcional |
| `AllergyOther` | Otro alimento, sustancia o medicamento | Unknown/Yes/No | Requerir details si `Yes` |
| `Hypertension` | Presion arterial alta / Hipertension | Unknown/Yes/No | Opcional |
| `Hypotension` | Presion arterial baja / Hipotension | Unknown/Yes/No | Opcional |
| `ExcessiveBleeding` | Sangrado excesivo ante heridas | Unknown/Yes/No | Opcional |
| `CoagulationDisorder` | Alteraciones sanguineas o de coagulacion | Unknown/Yes/No | Opcional |
| `AnemiaHemophiliaVitaminKDeficiency` | Anemia, hemofilia o deficiencia de vitamina K | Unknown/Yes/No | Opcional |
| `AntiretroviralMedication` | Toma algun retroviral | Unknown/Yes/No | Opcional |
| `UnfavorableDentalExperience` | Experiencias desfavorables con el dentista | Unknown/Yes/No | Opcional |
| `CovidHistory` | Tuvo COVID | Unknown/Yes/No | Opcional |
| `SexuallyTransmittedDiseaseHistory` | Enfermedades de transmision sexual | Unknown/Yes/No | Opcional |
| `HeartDiseaseHistory` | Enfermedades del corazon de nacimiento o actualmente | Unknown/Yes/No | Opcional |
| `HepatitisHistory` | Hepatitis A, B, C u otra | Unknown/Yes/No | Tipo/details |
| `Endocarditis` | Endocarditis | Unknown/Yes/No | Opcional |
| `SeizuresOrConvulsions` | Crisis o convulsiones | Unknown/Yes/No | Opcional |
| `Diabetes` | Diabetes | Unknown/Yes/No | Opcional |
| `Tuberculosis` | Tuberculosis | Unknown/Yes/No | Opcional |
| `Hyperthyroidism` | Hipertiroidismo | Unknown/Yes/No | Opcional |
| `Hypothyroidism` | Hipotiroidismo | Unknown/Yes/No | Opcional |
| `HeartAttackOrAngina` | Infarto o angina de pecho | Unknown/Yes/No | Opcional |
| `OpenHeartSurgery` | Cirugia a corazon abierto | Unknown/Yes/No | Opcional |
| `RecurrentHerpesOrAphthae` | Herpes o aftas recurrentes | Unknown/Yes/No | Opcional |
| `NailOrLipBiting` | Se muerde las unas o labios | Unknown/Yes/No | Opcional |
| `Smoking` | Fuma | Unknown/Yes/No | Opcional |
| `CitrusOrAcidFoodConsumption` | Consumo de alimentos citricos o acidos | Unknown/Yes/No | Opcional |
| `NightBruxism` | Apretamiento o rechinido de dientes por las noches | Unknown/Yes/No | Opcional |
| `PregnancyOrLactation` | Lactando, embarazada o sospecha de embarazo | Unknown/Yes/No | Opcional |
| `ContraceptiveMedication` | Toma medicamentos anticonceptivos | Unknown/Yes/No | Opcional |
| `AnesthesiaComplications` | Complicaciones asociadas a la anestesia | Unknown/Yes/No | Opcional |

Notas generales del cuestionario: usar un campo acotado del cuestionario o una nota clinica append-only segun el flujo UX final. No crear preguntas dinamicas ni form builder avanzado en este alcance.

## 10. Propuesta UX

Seguir `docs/frontend-ux-guidelines.md` y ADR 005.

Propuesta:

- Header de paciente read-only: nombre, edad derivada, fecha de nacimiento, telefono principal, estado activo/inactivo, alertas clinicas.
- Summary strip: alergias actuales, medicamentos actuales, alertas, ultima actualizacion, diagnosticos activos.
- Tabs:
  - Resumen.
  - Antecedentes medicos.
  - Consulta / notas.
  - Timeline.
- `Antecedentes medicos`: catalogo fijo por grupos, respuesta Unknown/Yes/No, details inline, sin form builder.
- `Consulta / notas`: motivo de consulta, signos vitales y nota solo cuando exista `ClinicalEncounter`; hasta entonces mantener `ClinicalNote` append-only.
- Sticky action bar para formularios largos de antecedentes medicos y encounter/vitals.
- Loading state dentro del area clinica, no full-page blocker si el paciente ya cargo.
- Empty state que preserve creacion explicita y no sugiera autocreacion.
- Error states diferenciados: falta expediente, sin permiso, error de carga, error de validacion.
- Usar shared UI foundation y tokens `--bsm-*`; evitar nuevos estilos aislados.

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

### Slice 3 — Clinical notes/vitals/timeline backend

Agregar `ClinicalEncounter` acotado si se decide capturar motivo de consulta, tipo de consulta y signos vitales:

- patient-owned y tenant-owned via ClinicalRecord;
- vitals con unidades/formatos claros;
- notas append-only o referencia a ClinicalNote;
- timeline read model enriquecido solo por decision explicita;
- sin doctor/provider assignment.

Validacion recomendada:

- vitals con rangos/formato basico;
- tenant isolation;
- no cross-module timeline;
- no cambios a Scheduling, Treatments ni Billing.

### Slice 4 — Frontend integration

Actualizar `features/clinical-records`:

- header de paciente read-only usando Patients;
- summary strip;
- tabs;
- formulario de antecedentes medicos con catalogo fijo;
- estados loading/empty/error;
- sticky action bar;
- mantener HTTP en data-access y orquestacion en facade.

Validacion recomendada:

- frontend unit tests para facade/data-access;
- component tests para estados y permisos;
- route guard sigue usando `clinical.read`;
- no mostrar Billing/Scheduling/doctor scope como implementado.

### Slice 5 — UX/QA closure docs

Cerrar el flujo con evidencia:

- revisar consistencia visual con `docs/frontend-ux-guidelines.md`;
- actualizar docs de Release 3 solo si el slice queda implementado y probado;
- no actualizar `STATE — BigSmile.md` hasta que el slice funcional este completo y validado.

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

Siguiente paso recomendado: mantener `Patient Demographics` como fuente de verdad para sexo, ocupacion, estado civil y referido por; despues, integrar frontend del cuestionario medico usando el backend fijo de Release 3.5. Billing fields deben quedar fuera hasta un slice fiscal/billing explicito.
