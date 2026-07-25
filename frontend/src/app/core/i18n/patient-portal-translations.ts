import { SupportedLanguageCode } from './i18n.model';

export const PATIENT_PORTAL_TRANSLATIONS: Record<SupportedLanguageCode, Record<string, string>> = {
  'es-MX': {
    'Patient portal': 'Portal del paciente',
    'Secure patient access': 'Acceso seguro para pacientes',
    'Your access is private and limited to your own patient workflow.': 'Tu acceso es privado y está limitado a tu propio flujo como paciente.',
    'Activate your patient access': 'Activa tu acceso como paciente',
    'Create the private credentials you will use to return and complete information requested by your clinic.': 'Crea las credenciales privadas que usarás para volver y completar la información solicitada por tu clínica.',
    'This activation link is not valid or is no longer available.': 'Este enlace de activación no es válido o ya no está disponible.',
    'Contact the clinic so authorized staff can issue a new invitation.': 'Contacta a la clínica para que personal autorizado emita una nueva invitación.',
    'Login name': 'Nombre de acceso',
    'Use an email address or a username you can remember. It is unique only inside this clinic.': 'Usa un correo o nombre de usuario que puedas recordar. Solo debe ser único dentro de esta clínica.',
    'Login name is required.': 'El nombre de acceso es obligatorio.',
    'Login name must contain at least 3 characters.': 'El nombre de acceso debe tener al menos 3 caracteres.',
    'Use at least 12 characters.': 'Usa al menos 12 caracteres.',
    'Password must contain at least 12 characters.': 'La contraseña debe tener al menos 12 caracteres.',
    'Confirm password': 'Confirmar contraseña',
    'Password confirmation is required.': 'La confirmación de contraseña es obligatoria.',
    'Passwords do not match.': 'Las contraseñas no coinciden.',
    'Activating access...': 'Activando acceso...',
    'Activate access': 'Activar acceso',
    'Patient portal activation could not be completed.': 'No se pudo completar la activación del portal del paciente.',
    'Sign in as a patient': 'Inicia sesión como paciente',
    'Enter the private credentials created when your clinic activated your access.': 'Ingresa las credenciales privadas creadas cuando tu clínica activó tu acceso.',
    'Clinic access': 'Acceso de clínica',
    'The supplied patient portal credential is not valid.': 'La credencial proporcionada para el portal del paciente no es válida.',
    'Your patient access is ready': 'Tu acceso como paciente está listo',
    'This secure session is limited to your own patient workflow.': 'Esta sesión segura está limitada a tu propio flujo como paciente.',
    'The medical information form will be available in the next implementation step. No clinical data can be changed from this screen.': 'El formulario de información médica estará disponible en el siguiente paso de implementación. Desde esta pantalla no se puede modificar información clínica.',
    'Ending session...': 'Cerrando sesión...',
    'End session': 'Cerrar sesión',
    'Refreshing patient session...': 'Actualizando sesión del paciente...',
    'The patient portal session is no longer available.': 'La sesión del portal del paciente ya no está disponible.',
    'The patient portal session ended locally. Server confirmation was unavailable.': 'La sesión del portal del paciente terminó localmente, pero no fue posible confirmar el cierre con el servidor.'
  },
  'en-US': {}
};
