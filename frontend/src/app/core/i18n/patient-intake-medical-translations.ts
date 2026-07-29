import { SupportedLanguageCode } from './i18n.model';

export const PATIENT_INTAKE_MEDICAL_TRANSLATIONS: Record<SupportedLanguageCode, Record<string, string>> = {
  'es-MX': {
    'Personal information and medical answers remain in this private draft until the clinic reviews them.': 'Tu información personal y tus respuestas médicas permanecen en este borrador privado hasta que la clínica las revise.',
    'These answers remain part of your private draft until the clinic reviews them.': 'Estas respuestas permanecen en tu borrador privado hasta que la clínica las revise.',
    'Answered questions': 'Preguntas respondidas',
    'Optional details': 'Detalles opcionales',
    'Medical answers are saved together with the current personal information shown above.': 'Las respuestas médicas se guardan junto con la información personal actualmente registrada arriba.',
    'Saving medical answers...': 'Guardando antecedentes médicos...',
    'Save medical answers': 'Guardar antecedentes médicos'
  },
  'en-US': {}
};
