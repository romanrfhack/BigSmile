export type PatientIntakeAnswerValue = 'Unknown' | 'Yes' | 'No';
export type PatientIntakeSex = 'Unspecified' | 'Female' | 'Male' | 'Other';
export type PatientIntakeMaritalStatus =
  | 'Unspecified'
  | 'Single'
  | 'Married'
  | 'Divorced'
  | 'Widowed'
  | 'Other';

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
  reasonForVisit: 500
} as const;

export interface PatientIntakeMedicalAnswer {
  questionKey: string;
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

export interface SavePatientIntakeMedicalAnswerRequest {
  questionKey: string;
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

export function buildSavePatientIntakeDraftRequest(
  intake: PatientIntakeDraft,
  value: PatientIntakeNonMedicalFormValue
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
    medicalAnswers: intake.medicalAnswers.map(answer => ({
      questionKey: answer.questionKey,
      answer: answer.answer,
      details: answer.details
    })),
    concurrencyToken: intake.concurrencyToken
  };
}

function normalizeOptional(value: string): string | null {
  const normalized = value.trim();
  return normalized.length > 0 ? normalized : null;
}
