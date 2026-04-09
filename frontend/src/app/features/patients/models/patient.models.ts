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
  primaryPhone: string | null;
  email: string | null;
  isActive: boolean;
}

export interface PatientDetail extends PatientSummary {
  responsibleParty: ResponsibleParty | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface SavePatientRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  primaryPhone: string | null;
  email: string | null;
  isActive: boolean;
  responsiblePartyName: string | null;
  responsiblePartyRelationship: string | null;
  responsiblePartyPhone: string | null;
}
