import {
  MEDICAL_QUESTIONNAIRE_DETAILS_MAX_LENGTH,
  MEDICAL_QUESTIONNAIRE_KEYS,
  MedicalQuestionnaireAnswerValue,
  MedicalQuestionnaireQuestionKey
} from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';

export type PatientIntakeAnswerValue = MedicalQuestionnaireAnswerValue;
export type PatientIntakeQuestionKey = MedicalQuestionnaireQuestionKey;
export type PatientIntakeSex = 'Unspecified' | 'Female' | 'Male' | 'Other';
export type PatientIntakeMaritalStatus =
  | 'Unspecified'
  | 'Single'
  | 'Married'
  | 'Divorced'
  | 'Widowed'
  | 'Other';
export type PatientIntakeSaveOutcome = 'saved' | 'unchanged' | null;

export const PATIENT_INTAKE_SEX_VALUES: readonly PatientIntakeSex[] = [
  'Unspecified',
  'Female',
  'Male',
  'Other'
];

export const PATIENT_INTAKE_MARITAL_STATUS_VALUES: readonly PatientIntakeMaritalStatus[] = [
  'Unspecified',
  'Single',
  'Married',
  'Divorced',
  'Widowed',
  'Other'
];

export const PATIENT_INTAKE_FIELD_LIMITS = {
  name: 100,
  demographic: 100,
  phone: 40,
  email: 256,
  reasonForVisit: 500,
  medicalDetails: MEDICAL_QUESTIONNAIRE_DETAILS_MAX_LENGTH
} as const;

export interface PatientIntakeMedicalAnswer {
  questionKey: PatientIntakeQuestionKey;
  answer: PatientIntakeAnswerValue;
  details: string | null;
}

export interface PatientIntakeDraft {
  origin: string;
  status: string;
  firstName: string | null;
  lastName: string | null;
  dateOfBirth: string | null;
  sex: PatientIntakeSex;
  occupation: string | null;
  maritalStatus: PatientIntakeMaritalStatus;
  referredBy: string | null;
  preferredPhone: string | null;
  mobilePhone: string | null;
  homePhone: string | null;
  workPhone: string | null;
  email: string | null;
  responsiblePartyName: string | null;
  responsiblePartyRelationship: string | null;
  responsiblePartyPhone: string | null;
  reasonForVisit: string | null;
  medicalAnswers: PatientIntakeMedicalAnswer[];
  currentRevisionNumber: number;
  concurrencyToken: string;
  createdAtUtc: string;
  lastUpdatedAtUtc: string;
  lastEffectiveSavedAtUtc: string | null;
  submittedAtUtc: string | null;
  expiresAtUtc: string;
}

export interface PatientIntakeNonMedicalFormValue {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  sex: PatientIntakeSex;
  occupation: string;
  maritalStatus: PatientIntakeMaritalStatus;
  referredBy: string;
  preferredPhone: string;
  mobilePhone: string;
  homePhone: string;
  workPhone: string;
  email: string;
  responsiblePartyName: string;
  responsiblePartyRelationship: string;
  responsiblePartyPhone: string;
  reasonForVisit: string;
}

export interface PatientIntakeMedicalAnswerFormValue {
  questionKey: PatientIntakeQuestionKey;
  answer: PatientIntakeAnswerValue;
  details: string;
}

export interface SavePatientIntakeMedicalAnswerRequest {
  questionKey: PatientIntakeQuestionKey;
  answer: PatientIntakeAnswerValue;
  details: string | null;
}

export interface SavePatientIntakeDraftRequest {
  firstName: string | null;
  lastName: string | null;
  dateOfBirth: string | null;
  sex: PatientIntakeSex;
  occupation: string | null;
  maritalStatus: PatientIntakeMaritalStatus;
  referredBy: string | null;
  preferredPhone: string | null;
  mobilePhone: string | null;
  homePhone: string | null;
  workPhone: string | null;
  email: string | null;
  responsiblePartyName: string | null;
  responsiblePartyRelationship: string | null;
  responsiblePartyPhone: string | null;
  reasonForVisit: string | null;
  medicalAnswers: SavePatientIntakeMedicalAnswerRequest[];
  concurrencyToken: string;
}

export interface SavePatientIntakeDraftResponse {
  intake: PatientIntakeDraft;
  changed: boolean;
}

export interface SubmitPatientIntakeRequest {
  concurrencyToken: string;
}

export interface SubmitPatientIntakeResponse {
  intake: PatientIntakeDraft;
  changed: boolean;
}

export function toPatientIntakeNonMedicalFormValue(
  intake: PatientIntakeDraft
): PatientIntakeNonMedicalFormValue {
  return {
    firstName: intake.firstName ?? '',
    lastName: intake.lastName ?? '',
    dateOfBirth: intake.dateOfBirth ?? '',
    sex: intake.sex,
    occupation: intake.occupation ?? '',
    maritalStatus: intake.maritalStatus,
    referredBy: intake.referredBy ?? '',
    preferredPhone: intake.preferredPhone ?? '',
    mobilePhone: intake.mobilePhone ?? '',
    homePhone: intake.homePhone ?? '',
    workPhone: intake.workPhone ?? '',
    email: intake.email ?? '',
    responsiblePartyName: intake.responsiblePartyName ?? '',
    responsiblePartyRelationship: intake.responsiblePartyRelationship ?? '',
    responsiblePartyPhone: intake.responsiblePartyPhone ?? '',
    reasonForVisit: intake.reasonForVisit ?? ''
  };
}

export function toPatientIntakeMedicalFormValue(
  intake: PatientIntakeDraft
): PatientIntakeMedicalAnswerFormValue[] {
  const answersByKey = new Map(intake.medicalAnswers.map(answer => [answer.questionKey, answer]));

  return MEDICAL_QUESTIONNAIRE_KEYS.map(questionKey => {
    const answer = answersByKey.get(questionKey);
    if (!answer) {
      throw new Error(`Missing patient intake medical answer for ${questionKey}.`);
    }

    return {
      questionKey,
      answer: answer.answer,
      details: answer.details ?? ''
    };
  });
}

export function buildSavePatientIntakeDraftRequest(
  intake: PatientIntakeDraft,
  value: PatientIntakeNonMedicalFormValue,
  medicalAnswers: readonly PatientIntakeMedicalAnswerFormValue[] = toPatientIntakeMedicalFormValue(intake)
): SavePatientIntakeDraftRequest {
  return {
    firstName: normalizeOptional(value.firstName),
    lastName: normalizeOptional(value.lastName),
    dateOfBirth: normalizeOptional(value.dateOfBirth),
    sex: value.sex,
    occupation: normalizeOptional(value.occupation),
    maritalStatus: value.maritalStatus,
    referredBy: normalizeOptional(value.referredBy),
    preferredPhone: normalizeOptional(value.preferredPhone),
    mobilePhone: normalizeOptional(value.mobilePhone),
    homePhone: normalizeOptional(value.homePhone),
    workPhone: normalizeOptional(value.workPhone),
    email: normalizeOptional(value.email),
    responsiblePartyName: normalizeOptional(value.responsiblePartyName),
    responsiblePartyRelationship: normalizeOptional(value.responsiblePartyRelationship),
    responsiblePartyPhone: normalizeOptional(value.responsiblePartyPhone),
    reasonForVisit: normalizeOptional(value.reasonForVisit),
    medicalAnswers: normalizeMedicalAnswers(medicalAnswers),
    concurrencyToken: intake.concurrencyToken
  };
}

export function isPatientIntakeReadyForSubmission(
  value: PatientIntakeNonMedicalFormValue,
  medicalAnswers: readonly PatientIntakeMedicalAnswerFormValue[]
): boolean {
  return value.firstName.trim().length > 0 &&
    value.lastName.trim().length > 0 &&
    value.dateOfBirth.trim().length > 0 &&
    medicalAnswers.length === MEDICAL_QUESTIONNAIRE_KEYS.length &&
    medicalAnswers.every(answer => answer.answer !== 'Unknown');
}

function normalizeMedicalAnswers(
  medicalAnswers: readonly PatientIntakeMedicalAnswerFormValue[]
): SavePatientIntakeMedicalAnswerRequest[] {
  const answersByKey = new Map<PatientIntakeQuestionKey, PatientIntakeMedicalAnswerFormValue>();
  for (const answer of medicalAnswers) {
    if (answersByKey.has(answer.questionKey)) {
      throw new Error(`Duplicate patient intake medical answer for ${answer.questionKey}.`);
    }
    answersByKey.set(answer.questionKey, answer);
  }

  if (answersByKey.size !== MEDICAL_QUESTIONNAIRE_KEYS.length) {
    throw new Error('Patient intake medical answers must contain the complete fixed questionnaire.');
  }

  return MEDICAL_QUESTIONNAIRE_KEYS.map(questionKey => {
    const answer = answersByKey.get(questionKey);
    if (!answer) {
      throw new Error(`Missing patient intake medical answer for ${questionKey}.`);
    }

    return {
      questionKey,
      answer: answer.answer,
      details: normalizeOptional(answer.details)
    };
  });
}

function normalizeOptional(value: string): string | null {
  const normalized = value.trim();
  return normalized.length > 0 ? normalized : null;
}
