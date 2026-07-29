import {
  MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS,
  MEDICAL_QUESTIONNAIRE_ANSWER_VALUES,
  MEDICAL_QUESTIONNAIRE_DETAILS_MAX_LENGTH,
  MEDICAL_QUESTIONNAIRE_GROUPS,
  MEDICAL_QUESTIONNAIRE_KEYS,
  MedicalQuestionnaireAnswerOption,
  MedicalQuestionnaireGroup,
  MedicalQuestionnaireQuestion
} from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';

export type ClinicalMedicalQuestionnaireQuestion = MedicalQuestionnaireQuestion;
export type ClinicalMedicalQuestionnaireGroup = MedicalQuestionnaireGroup;
export type ClinicalMedicalQuestionnaireAnswerOption = MedicalQuestionnaireAnswerOption;

export const CLINICAL_MEDICAL_ANSWER_VALUES = MEDICAL_QUESTIONNAIRE_ANSWER_VALUES;
export const CLINICAL_MEDICAL_ANSWER_OPTIONS = MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS;
export const CLINICAL_MEDICAL_ANSWER_DETAILS_MAX_LENGTH = MEDICAL_QUESTIONNAIRE_DETAILS_MAX_LENGTH;
export const CLINICAL_MEDICAL_QUESTIONNAIRE_GROUPS = MEDICAL_QUESTIONNAIRE_GROUPS;
export const CLINICAL_MEDICAL_QUESTION_KEYS = MEDICAL_QUESTIONNAIRE_KEYS;
