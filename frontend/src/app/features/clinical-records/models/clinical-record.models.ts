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

export type ClinicalSnapshotHistoryEntryType =
  | 'SnapshotInitialized'
  | 'MedicalBackgroundUpdated'
  | 'CurrentMedicationsUpdated'
  | 'AllergiesUpdated';

export type ClinicalSnapshotHistorySection =
  | 'Initial'
  | 'MedicalBackground'
  | 'CurrentMedications'
  | 'Allergies';

export interface ClinicalSnapshotHistoryEntry {
  entryType: ClinicalSnapshotHistoryEntryType;
  changedAtUtc: string;
  changedByUserId: string;
  section: ClinicalSnapshotHistorySection;
  summary: string;
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
  snapshotHistory: ClinicalSnapshotHistoryEntry[];
  timeline: ClinicalTimelineEntry[];
  createdAtUtc: string;
  createdByUserId: string;
  lastUpdatedAtUtc: string;
  lastUpdatedByUserId: string;
}

export type ClinicalEncounterConsultationType = 'Treatment' | 'Urgency' | 'Other';

export interface ClinicalEncounter {
  id: string;
  clinicalRecordId: string;
  patientId: string;
  occurredAtUtc: string;
  chiefComplaint: string;
  consultationType: ClinicalEncounterConsultationType;
  temperatureC: number | null;
  bloodPressureSystolic: number | null;
  bloodPressureDiastolic: number | null;
  weightKg: number | null;
  heightCm: number | null;
  respiratoryRatePerMinute: number | null;
  heartRateBpm: number | null;
  clinicalNoteId: string | null;
  noteText: string | null;
  createdAtUtc: string;
  createdByUserId: string;
}

export interface AddClinicalEncounterRequest {
  occurredAtUtc: string;
  chiefComplaint: string;
  consultationType: ClinicalEncounterConsultationType;
  temperatureC: number | null;
  bloodPressureSystolic: number | null;
  bloodPressureDiastolic: number | null;
  weightKg: number | null;
  heightCm: number | null;
  respiratoryRatePerMinute: number | null;
  heartRateBpm: number | null;
  noteText: string | null;
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

export type ClinicalMedicalAnswerValue = 'Unknown' | 'Yes' | 'No';

export type ClinicalMedicalQuestionKey =
  | 'currentMedicalTreatment'
  | 'regularMedication'
  | 'priorSurgery'
  | 'bloodTransfusion'
  | 'drugUse'
  | 'allergicReactions'
  | 'allergyPenicillin'
  | 'allergyAnesthetics'
  | 'allergyAspirin'
  | 'allergySulfas'
  | 'allergyIodine'
  | 'allergyOther'
  | 'hypertension'
  | 'hypotension'
  | 'excessiveBleeding'
  | 'bloodOrCoagulationDisorder'
  | 'anemiaHemophiliaVitaminKDeficiency'
  | 'retroviralTreatment'
  | 'badDentalExperience'
  | 'covidHistory'
  | 'sexuallyTransmittedDisease'
  | 'congenitalOrCurrentHeartDisease'
  | 'hepatitis'
  | 'endocarditis'
  | 'seizures'
  | 'diabetes'
  | 'tuberculosis'
  | 'hyperthyroidism'
  | 'hypothyroidism'
  | 'heartAttackOrAngina'
  | 'openHeartSurgery'
  | 'recurrentHerpesOrAphthae'
  | 'bitesNailsOrLips'
  | 'smokes'
  | 'acidicFoodConsumption'
  | 'bruxismAtNight'
  | 'pregnantLactatingOrSuspected'
  | 'contraceptiveMedication'
  | 'anesthesiaComplications';

export interface ClinicalMedicalAnswer {
  id: string | null;
  questionKey: ClinicalMedicalQuestionKey;
  answer: ClinicalMedicalAnswerValue;
  details: string | null;
  updatedAtUtc: string | null;
  updatedByUserId: string | null;
}

export interface ClinicalMedicalQuestionnaire {
  clinicalRecordId: string;
  patientId: string;
  answers: ClinicalMedicalAnswer[];
}

export interface SaveClinicalMedicalAnswerRequest {
  questionKey: ClinicalMedicalQuestionKey;
  answer: ClinicalMedicalAnswerValue;
  details: string | null;
}

export interface SaveClinicalMedicalQuestionnaireRequest {
  answers: SaveClinicalMedicalAnswerRequest[];
}
