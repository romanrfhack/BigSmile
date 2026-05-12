import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AddClinicalDiagnosisRequest,
  AddClinicalNoteRequest,
  ClinicalMedicalQuestionnaire,
  ClinicalRecord,
  SaveClinicalMedicalQuestionnaireRequest,
  SaveClinicalRecordSnapshotRequest
} from '../models/clinical-record.models';

@Injectable({
  providedIn: 'root'
})
export class ClinicalRecordsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/patients`;

  getClinicalRecord(patientId: string): Observable<ClinicalRecord> {
    return this.http.get<ClinicalRecord>(`${this.baseUrl}/${patientId}/clinical-record`);
  }

  getClinicalMedicalQuestionnaire(patientId: string): Observable<ClinicalMedicalQuestionnaire> {
    return this.http.get<ClinicalMedicalQuestionnaire>(`${this.baseUrl}/${patientId}/clinical-record/questionnaire`);
  }

  createClinicalRecord(patientId: string, payload: SaveClinicalRecordSnapshotRequest): Observable<ClinicalRecord> {
    return this.http.post<ClinicalRecord>(`${this.baseUrl}/${patientId}/clinical-record`, payload);
  }

  updateClinicalRecord(patientId: string, payload: SaveClinicalRecordSnapshotRequest): Observable<ClinicalRecord> {
    return this.http.put<ClinicalRecord>(`${this.baseUrl}/${patientId}/clinical-record`, payload);
  }

  updateClinicalMedicalQuestionnaire(
    patientId: string,
    payload: SaveClinicalMedicalQuestionnaireRequest
  ): Observable<ClinicalMedicalQuestionnaire> {
    return this.http.put<ClinicalMedicalQuestionnaire>(`${this.baseUrl}/${patientId}/clinical-record/questionnaire`, payload);
  }

  addClinicalNote(patientId: string, payload: AddClinicalNoteRequest): Observable<ClinicalRecord> {
    return this.http.post<ClinicalRecord>(`${this.baseUrl}/${patientId}/clinical-record/notes`, payload);
  }

  addDiagnosis(patientId: string, payload: AddClinicalDiagnosisRequest): Observable<ClinicalRecord> {
    return this.http.post<ClinicalRecord>(`${this.baseUrl}/${patientId}/clinical-record/diagnoses`, payload);
  }

  resolveDiagnosis(patientId: string, diagnosisId: string): Observable<ClinicalRecord> {
    return this.http.post<ClinicalRecord>(`${this.baseUrl}/${patientId}/clinical-record/diagnoses/${diagnosisId}/resolve`, {});
  }
}
