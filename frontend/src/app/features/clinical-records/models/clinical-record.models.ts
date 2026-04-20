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

export type ClinicalDiagnosisStatus = 'Active' | 'Resolved';

export interface ClinicalDiagnosis {
  diagnosisId: string;
  diagnosisText: string;
  notes: string | null;
  status: ClinicalDiagnosisStatus;
  createdAtUtc: string;
  createdByUserId: string;
  resolvedAtUtc: string | null;
  resolvedByUserId: string | null;
}

export type ClinicalTimelineEventType =
  | 'ClinicalNoteCreated'
  | 'ClinicalDiagnosisCreated'
  | 'ClinicalDiagnosisResolved';

export interface ClinicalTimelineEntry {
  eventType: ClinicalTimelineEventType;
  occurredAtUtc: string;
  actorUserId: string;
  title: string;
  summary: string;
  referenceId: string;
}

export interface ClinicalRecord {
  clinicalRecordId: string;
  patientId: string;
  medicalBackgroundSummary: string | null;
  currentMedicationsSummary: string | null;
  allergies: ClinicalAllergy[];
  notes: ClinicalNote[];
  diagnoses: ClinicalDiagnosis[];
  timeline: ClinicalTimelineEntry[];
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

export interface AddClinicalDiagnosisRequest {
  diagnosisText: string;
  notes: string | null;
}
