import { SupportedLanguageCode } from './i18n.model';

export const PATIENT_INTAKE_LIFECYCLE_TRANSLATIONS: Record<SupportedLanguageCode, Record<string, string>> = {
  'es-MX': {
    'Your intake session is not available. Sign in again to continue.': 'Tu sesión de captura ya no está disponible. Inicia sesión nuevamente para continuar.',
    'This waiting-room intake access is no longer available. Ask reception for a new credential.': 'Este acceso de sala de espera ya no está disponible. Solicita una nueva credencial en recepción.',
    'Intake workspace': 'Espacio de captura',
    'Saving your complete draft...': 'Guardando tu borrador completo...',
    'You have unsaved changes.': 'Tienes cambios sin guardar.',
    'All visible changes are saved.': 'Todos los cambios visibles están guardados.',
    'A newer version of this intake exists.': 'Existe una versión más reciente de esta captura.',
    'Your local edits remain on this screen. Reloading will discard them and load the latest saved version.': 'Tus cambios locales permanecen en esta pantalla. Al recargar se descartarán y se abrirá la versión guardada más reciente.',
    'Reload latest version': 'Recargar versión más reciente',
    'This intake draft has expired.': 'Este borrador de captura venció.',
    'Your local edits cannot be saved. Start a new draft to continue.': 'Tus cambios locales ya no se pueden guardar. Inicia un nuevo borrador para continuar.',
    'Your local edits cannot be saved. Ask reception for a new waiting-room credential.': 'Tus cambios locales ya no se pueden guardar. Solicita una nueva credencial de sala de espera en recepción.',
    'Start a new draft': 'Iniciar nuevo borrador',
    'You have unsaved changes. Leave this page and discard them?': 'Tienes cambios sin guardar. ¿Deseas salir y descartarlos?',
    'Reload the latest saved version and discard the local edits shown on this screen?': '¿Recargar la versión guardada más reciente y descartar los cambios locales mostrados en esta pantalla?',
    'Start a new intake draft and discard the expired local edits shown on this screen?': '¿Iniciar un nuevo borrador y descartar los cambios locales vencidos mostrados en esta pantalla?',
    'End the session and discard your unsaved changes?': '¿Cerrar la sesión y descartar tus cambios sin guardar?',
    'The current intake draft expired and can no longer be saved.': 'El borrador actual venció y ya no puede guardarse.',
    'A newer version of this intake exists. Reload it before saving again.': 'Existe una versión más reciente de esta captura. Recárgala antes de volver a guardar.'
  },
  'en-US': {}
};
