import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  PatientIntakeDraft,
  SavePatientIntakeDraftRequest,
  SavePatientIntakeDraftResponse
} from '../models/patient-intake.models';

@Injectable({ providedIn: 'root' })
export class PatientIntakeApi {
  private readonly baseUrl = `${environment.apiUrl}/api/patient-portal/intake`;

  constructor(private readonly http: HttpClient) {}

  create(): Observable<PatientIntakeDraft> {
    return this.http.post<PatientIntakeDraft>(this.baseUrl, {});
  }

  getCurrent(): Observable<PatientIntakeDraft> {
    return this.http.get<PatientIntakeDraft>(this.baseUrl);
  }

  save(request: SavePatientIntakeDraftRequest): Observable<SavePatientIntakeDraftResponse> {
    return this.http.put<SavePatientIntakeDraftResponse>(this.baseUrl, request);
  }
}
