import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PatientDocument } from '../models/patient-document.models';

@Injectable({
  providedIn: 'root'
})
export class PatientDocumentsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/patients`;

  getDocuments(patientId: string): Observable<PatientDocument[]> {
    return this.http.get<PatientDocument[]>(`${this.baseUrl}/${patientId}/documents`);
  }

  uploadDocument(patientId: string, file: File): Observable<PatientDocument> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<PatientDocument>(`${this.baseUrl}/${patientId}/documents`, formData);
  }

  downloadDocument(patientId: string, documentId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${patientId}/documents/${documentId}/download`, {
      responseType: 'blob'
    });
  }

  retireDocument(patientId: string, documentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${patientId}/documents/${documentId}`);
  }
}
