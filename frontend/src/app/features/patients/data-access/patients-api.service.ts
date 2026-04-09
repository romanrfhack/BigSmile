import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PatientDetail, PatientSummary, SavePatientRequest } from '../models/patient.models';

@Injectable({
  providedIn: 'root'
})
export class PatientsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/patients`;

  searchPatients(search = '', includeInactive = false, take = 25): Observable<PatientSummary[]> {
    let params = new HttpParams().set('includeInactive', includeInactive).set('take', take);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PatientSummary[]>(this.baseUrl, { params });
  }

  getPatient(id: string): Observable<PatientDetail> {
    return this.http.get<PatientDetail>(`${this.baseUrl}/${id}`);
  }

  createPatient(payload: SavePatientRequest): Observable<PatientDetail> {
    return this.http.post<PatientDetail>(this.baseUrl, payload);
  }

  updatePatient(id: string, payload: SavePatientRequest): Observable<PatientDetail> {
    return this.http.put<PatientDetail>(`${this.baseUrl}/${id}`, payload);
  }
}
