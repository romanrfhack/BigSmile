export type PatientSex = 'Unspecified' | 'Female' | 'Male' | 'Other';

export type PatientMaritalStatus = 'Unspecified' | 'Single' | 'Married' | 'Divorced' | 'Widowed' | 'Other';

export interface ResponsibleParty {
  name: string;
  relationship: string | null;
  phone: string | null;
}

export interface PatientSummary {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  sex: PatientSex;
  occupation: string | null;
  maritalStatus: PatientMaritalStatus;
  referredBy: string | null;
  primaryPhone: string | null;
  email: string | null;
  isActive: boolean;
  hasClinicalAlerts: boolean;
}

export interface PatientDetail extends PatientSummary {
  clinicalAlertsSummary: string | null;
  responsibleParty: ResponsibleParty | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface SavePatientRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  sex: PatientSex;
  occupation: string | null;
  maritalStatus: PatientMaritalStatus;
  referredBy: string | null;
  primaryPhone: string | null;
  email: string | null;
  isActive: boolean;
  hasClinicalAlerts: boolean;
  clinicalAlertsSummary: string | null;
  responsiblePartyName: string | null;
  responsiblePartyRelationship: string | null;
  responsiblePartyPhone: string | null;
}
