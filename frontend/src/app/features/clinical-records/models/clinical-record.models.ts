export interface ClinicalAllergy {
  id: string;
  substance: string;
  reactionSummary: string | null;
  notes: string | null;
}

export interface ClinicalNote {
  id: string;
  noteText: string;
  createdAtUtc: string;
  createdByUserId: string;
}

export interface ClinicalRecord {
  clinicalRecordId: string;
  patientId: string;
  medicalBackgroundSummary: string | null;
  currentMedicationsSummary: string | null;
  allergies: ClinicalAllergy[];
  notes: ClinicalNote[];
  createdAtUtc: string;
  createdByUserId: string;
  lastUpdatedAtUtc: string;
  lastUpdatedByUserId: string;
}

export interface SaveClinicalRecordAllergyRequest {
  substance: string;
  reactionSummary: string | null;
  notes: string | null;
}

export interface SaveClinicalRecordSnapshotRequest {
  medicalBackgroundSummary: string | null;
  currentMedicationsSummary: string | null;
  allergies: SaveClinicalRecordAllergyRequest[];
}

export interface AddClinicalNoteRequest {
  noteText: string;
}
