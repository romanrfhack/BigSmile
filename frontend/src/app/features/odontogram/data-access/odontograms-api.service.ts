import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Odontogram, UpdateToothStatusRequest } from '../models/odontogram.models';

@Injectable({
  providedIn: 'root'
})
export class OdontogramsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/patients`;

  getOdontogram(patientId: string): Observable<Odontogram> {
    return this.http.get<Odontogram>(`${this.baseUrl}/${patientId}/odontogram`);
  }

  createOdontogram(patientId: string): Observable<Odontogram> {
    return this.http.post<Odontogram>(`${this.baseUrl}/${patientId}/odontogram`, {});
  }

  updateToothStatus(patientId: string, toothCode: string, payload: UpdateToothStatusRequest): Observable<Odontogram> {
    return this.http.put<Odontogram>(`${this.baseUrl}/${patientId}/odontogram/teeth/${toothCode}`, payload);
  }
}
