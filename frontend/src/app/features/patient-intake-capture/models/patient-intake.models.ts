export type PatientIntakeAnswerValue = 'Unknown' | 'Yes' | 'No';

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
  sex: string;
  occupation: string | null;
  maritalStatus: string;
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

export interface SavePatientIntakeMedicalAnswerRequest {
  questionKey: string;
  answer: PatientIntakeAnswerValue;
  details: string | null;
}

export interface SavePatientIntakeDraftRequest {
  firstName: string | null;
  lastName: string | null;
  dateOfBirth: string | null;
  sex: string;
  occupation: string | null;
  maritalStatus: string;
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
