# Revisión UX — formato de antecedentes médicos del cliente

- **Estado:** Aceptada como ajuste visual acotado
- **Tipo de cambio:** Frontend UX / organización de captura
- **Alcance:** Clinical Records — cuestionario médico fijo
- **Sin cambios en:** backend, contratos API, permisos, tenant context, branch context, migrations o seeds

## 1. Contexto

El cliente compartió un formato físico de historia clínica con dos bloques principales:

1. Datos generales, motivo/tipo de consulta y signos vitales.
2. Antecedentes médicos mediante respuestas `Sí` / `No`, con espacio para observaciones.

La revisión se realizó contra el estado real del repositorio y contra el mapeo existente en `docs/release-3-clinical-records-form-mapping.md`.

## 2. Cobertura confirmada

### Datos generales del paciente

Los siguientes datos ya pertenecen a `Patients` y se muestran en Clinical Records como contexto read-only, sin duplicarlos:

- nombre;
- fecha de nacimiento;
- edad derivada;
- sexo;
- ocupación;
- estado civil;
- referido por;
- contacto principal;
- estado y alertas clínicas básicas.

Pendiente fuera de este ajuste: teléfonos separados casa/oficina/celular. El modelo actual conserva un contacto principal; una separación por tipo requeriría un mini-slice explícito de Patient Contact Details.

### Consulta y signos vitales

Ya están cubiertos mediante `ClinicalEncounter`:

- motivo de consulta;
- tipo de consulta `Treatment` / `Urgency` / `Other`;
- temperatura;
- presión arterial sistólica/diastólica;
- peso;
- talla;
- frecuencia respiratoria;
- frecuencia cardiaca;
- nota clínica opcional ligada a la consulta.

### Antecedentes médicos

El catálogo fijo actual cubre las 39 preguntas del bloque de antecedentes del formato físico, con:

- respuesta `Unknown` / `Yes` / `No`;
- detalle opcional por pregunta;
- persistencia tenant-owned y patient-owned;
- lectura y escritura protegidas con `clinical.read` / `clinical.write`;
- `TenantId` y usuario derivados del contexto del servidor.

### Campos fuera de Clinical Records

No se incorporan en este ajuste:

- `Requiere factura`;
- datos de facturación.

Estos datos pertenecen a Billing o a un futuro perfil fiscal, no al expediente clínico.

## 3. Problema UX detectado

El cuestionario ya tenía la cobertura funcional correcta, pero cada respuesta se seleccionaba mediante un dropdown. Para usuarios acostumbrados al formato físico, esa interacción ocultaba las opciones `Sí` / `No`, agregaba un clic por pregunta y hacía menos evidente el avance de captura.

## 4. Decisión

Mantener sin cambios el catálogo, los modelos y los contratos existentes, y aplicar una reorganización exclusivamente visual:

- conservar las secciones temáticas digitales actuales;
- mostrar `Sí`, `No` y `Sin respuesta` como opciones segmentadas visibles;
- mantener `Sin respuesta` como estado seguro por defecto, sin inferir `No`;
- mostrar el detalle opcional cuando la respuesta es `Sí` o cuando ya existe texto capturado;
- mostrar un contador compacto de preguntas capturadas sobre el total;
- conservar la barra sticky de guardar/cancelar;
- preservar responsive, navegación por teclado, foco visible y reduced motion.

Esta presentación conserva la familiaridad del formato en papel —pregunta con selección visible Sí/No—, pero aprovecha la web con agrupación temática, feedback inmediato y menor fricción de captura.

## 5. Fuera de alcance

- Form builder configurable por tenant.
- Portal o enlace para que el paciente responda directamente fuera de la sesión clínica.
- Firma o consentimiento electrónico.
- Sincronización automática con alergias actuales o alertas del paciente.
- Nota general exclusiva del cuestionario.
- Cambios en permisos o acceso clínico.
- Cambios en backend, API, base de datos o multi-tenancy.

## 6. Riesgos y mitigaciones

- **Riesgo:** interpretar una pregunta no respondida como negativa.  
  **Mitigación:** se conserva `Unknown` y se presenta como `Sin respuesta`.

- **Riesgo:** divergencia entre cuestionario y alergias actuales.  
  **Mitigación:** se mantiene el aviso existente y no se agrega sincronización implícita.

- **Riesgo:** mayor densidad visual en móvil.  
  **Mitigación:** una sola columna para las preguntas y opciones táctiles compactas.

- **Riesgo:** alterar contratos clínicos cerrados de Release 3.5.  
  **Mitigación:** el ajuste es frontend-only y conserva el payload actual.

## 7. Validación esperada

- El catálogo completo sigue renderizándose.
- Cada pregunta muestra `Sí`, `No` y `Sin respuesta` sin abrir un selector.
- El detalle aparece para `Sí` y se conserva cuando ya existe contenido.
- El contador refleja respuestas o detalles capturados.
- El payload conserva todas las `QuestionKey` y normaliza detalles vacíos a `null`.
- Los estados loading, error, read-only y saving permanecen funcionales.

## 8. Nota de decisión resumida

**Contexto:** el backend y el catálogo ya cubren el formato físico; la brecha estaba en la familiaridad y velocidad de captura web.

**Decisión:** mejorar únicamente la presentación del cuestionario fijo con opciones visibles `Sí` / `No` / `Sin respuesta`, contador de avance y detalles condicionales.

**Consecuencia:** aumenta la familiaridad para el cliente y reduce interacción por pregunta sin cambiar alcance clínico, contratos, seguridad multi-tenant ni estado canónico del proyecto.
