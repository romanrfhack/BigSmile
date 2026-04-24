export interface PatientDocument {
  documentId: string;
  patientId: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAtUtc: string;
  uploadedByUserId: string;
}
