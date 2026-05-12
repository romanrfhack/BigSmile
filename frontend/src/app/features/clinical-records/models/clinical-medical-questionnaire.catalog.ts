import { ClinicalMedicalAnswerValue, ClinicalMedicalQuestionKey } from './clinical-record.models';

export interface ClinicalMedicalQuestionnaireQuestion {
  questionKey: ClinicalMedicalQuestionKey;
  labelKey: string;
}

export interface ClinicalMedicalQuestionnaireGroup {
  id: string;
  titleKey: string;
  questions: readonly ClinicalMedicalQuestionnaireQuestion[];
}

export const CLINICAL_MEDICAL_ANSWER_VALUES: readonly ClinicalMedicalAnswerValue[] = ['Unknown', 'Yes', 'No'];

export const CLINICAL_MEDICAL_QUESTIONNAIRE_GROUPS: readonly ClinicalMedicalQuestionnaireGroup[] = [
  {
    id: 'treatment-history',
    titleKey: 'Treatment and history',
    questions: [
      { questionKey: 'currentMedicalTreatment', labelKey: 'Currently under medical treatment' },
      { questionKey: 'regularMedication', labelKey: 'Takes medication regularly' },
      { questionKey: 'priorSurgery', labelKey: 'Has had surgery' }
    ]
  },
  {
    id: 'allergies',
    titleKey: 'Allergies',
    questions: [
      { questionKey: 'allergicReactions', labelKey: 'Has had allergic reactions' },
      { questionKey: 'allergyPenicillin', labelKey: 'Penicillin' },
      { questionKey: 'allergyAnesthetics', labelKey: 'Anesthetics' },
      { questionKey: 'allergyAspirin', labelKey: 'Aspirin' },
      { questionKey: 'allergySulfas', labelKey: 'Sulfas' },
      { questionKey: 'allergyIodine', labelKey: 'Iodine' },
      { questionKey: 'allergyOther', labelKey: 'Other food, substance, or medication' }
    ]
  },
  {
    id: 'pressure-blood-coagulation',
    titleKey: 'Blood pressure, blood, and coagulation',
    questions: [
      { questionKey: 'bloodTransfusion', labelKey: 'Has received blood transfusions' },
      { questionKey: 'hypertension', labelKey: 'High blood pressure / hypertension' },
      { questionKey: 'hypotension', labelKey: 'Low blood pressure / hypotension' },
      { questionKey: 'excessiveBleeding', labelKey: 'Excessive bleeding from wounds' },
      { questionKey: 'bloodOrCoagulationDisorder', labelKey: 'Blood or coagulation disorder' },
      { questionKey: 'anemiaHemophiliaVitaminKDeficiency', labelKey: 'Anemia, hemophilia, or vitamin K deficiency' }
    ]
  },
  {
    id: 'systemic-diseases',
    titleKey: 'Systemic diseases',
    questions: [
      { questionKey: 'retroviralTreatment', labelKey: 'Takes retroviral medication' },
      { questionKey: 'covidHistory', labelKey: 'Had COVID' },
      { questionKey: 'sexuallyTransmittedDisease', labelKey: 'Sexually transmitted disease history' },
      { questionKey: 'congenitalOrCurrentHeartDisease', labelKey: 'Heart disease from birth or currently' },
      { questionKey: 'hepatitis', labelKey: 'Hepatitis A, B, C, or other' },
      { questionKey: 'endocarditis', labelKey: 'Endocarditis' },
      { questionKey: 'seizures', labelKey: 'Seizures or convulsions' },
      { questionKey: 'diabetes', labelKey: 'Diabetes' },
      { questionKey: 'tuberculosis', labelKey: 'Tuberculosis' },
      { questionKey: 'hyperthyroidism', labelKey: 'Hyperthyroidism' },
      { questionKey: 'hypothyroidism', labelKey: 'Hypothyroidism' },
      { questionKey: 'heartAttackOrAngina', labelKey: 'Heart attack or angina' },
      { questionKey: 'openHeartSurgery', labelKey: 'Open-heart surgery' }
    ]
  },
  {
    id: 'habits-dental-conditions',
    titleKey: 'Habits and dental conditions',
    questions: [
      { questionKey: 'drugUse', labelKey: 'Consumed or consumes drugs' },
      { questionKey: 'badDentalExperience', labelKey: 'Unfavorable dental experiences' },
      { questionKey: 'recurrentHerpesOrAphthae', labelKey: 'Recurrent herpes or aphthae' },
      { questionKey: 'bitesNailsOrLips', labelKey: 'Bites nails or lips' },
      { questionKey: 'smokes', labelKey: 'Smokes' },
      { questionKey: 'acidicFoodConsumption', labelKey: 'Consumes citrus or acidic foods' },
      { questionKey: 'bruxismAtNight', labelKey: 'Clenching or grinding teeth at night' }
    ]
  },
  {
    id: 'pregnancy-anesthesia-special',
    titleKey: 'Pregnancy, anesthesia, and special conditions',
    questions: [
      { questionKey: 'pregnantLactatingOrSuspected', labelKey: 'Lactating, pregnant, or suspected pregnancy' },
      { questionKey: 'contraceptiveMedication', labelKey: 'Takes contraceptive medication' },
      { questionKey: 'anesthesiaComplications', labelKey: 'Anesthesia-related complications' }
    ]
  }
];

export const CLINICAL_MEDICAL_QUESTION_KEYS: readonly ClinicalMedicalQuestionKey[] =
  CLINICAL_MEDICAL_QUESTIONNAIRE_GROUPS.flatMap((group) => group.questions.map((question) => question.questionKey));
