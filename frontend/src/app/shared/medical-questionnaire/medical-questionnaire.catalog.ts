import { ALLERGY_QUESTIONS } from './catalog/allergies.questions';
import { HABITS_DENTAL_CONDITION_QUESTIONS } from './catalog/habits-dental-conditions.questions';
import { PREGNANCY_ANESTHESIA_SPECIAL_QUESTIONS } from './catalog/pregnancy-anesthesia-special.questions';
import { PRESSURE_BLOOD_COAGULATION_QUESTIONS } from './catalog/pressure-blood-coagulation.questions';
import { SYSTEMIC_DISEASE_QUESTIONS } from './catalog/systemic-diseases.questions';
import { TREATMENT_HISTORY_QUESTIONS } from './catalog/treatment-history.questions';

export type MedicalQuestionnaireAnswerValue = 'Unknown' | 'Yes' | 'No';

export interface MedicalQuestionnaireAnswerOption {
  value: MedicalQuestionnaireAnswerValue;
  labelKey: string;
}

export const MEDICAL_QUESTIONNAIRE_DETAILS_MAX_LENGTH = 500;

export const MEDICAL_QUESTIONNAIRE_ANSWER_VALUES: readonly MedicalQuestionnaireAnswerValue[] = [
  'Unknown',
  'Yes',
  'No'
];

export const MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS: readonly MedicalQuestionnaireAnswerOption[] = [
  { value: 'Yes', labelKey: 'Yes' },
  { value: 'No', labelKey: 'No' },
  { value: 'Unknown', labelKey: 'No answer' }
];

export const MEDICAL_QUESTIONNAIRE_GROUPS = [
  {
    id: 'treatment-history',
    titleKey: 'Treatment and history',
    questions: TREATMENT_HISTORY_QUESTIONS
  },
  {
    id: 'allergies',
    titleKey: 'Allergies',
    questions: ALLERGY_QUESTIONS
  },
  {
    id: 'pressure-blood-coagulation',
    titleKey: 'Blood pressure, blood, and coagulation',
    questions: PRESSURE_BLOOD_COAGULATION_QUESTIONS
  },
  {
    id: 'systemic-diseases',
    titleKey: 'Systemic diseases',
    questions: SYSTEMIC_DISEASE_QUESTIONS
  },
  {
    id: 'habits-dental-conditions',
    titleKey: 'Habits and dental conditions',
    questions: HABITS_DENTAL_CONDITION_QUESTIONS
  },
  {
    id: 'pregnancy-anesthesia-special',
    titleKey: 'Pregnancy, anesthesia, and special conditions',
    questions: PREGNANCY_ANESTHESIA_SPECIAL_QUESTIONS
  }
] as const;

export type MedicalQuestionnaireGroup = (typeof MEDICAL_QUESTIONNAIRE_GROUPS)[number];
export type MedicalQuestionnaireQuestion = MedicalQuestionnaireGroup['questions'][number];
export type MedicalQuestionnaireQuestionKey = MedicalQuestionnaireQuestion['questionKey'];

export const MEDICAL_QUESTIONNAIRE_KEYS: readonly MedicalQuestionnaireQuestionKey[] =
  MEDICAL_QUESTIONNAIRE_GROUPS.flatMap((group) =>
    group.questions.map((question) => question.questionKey));
