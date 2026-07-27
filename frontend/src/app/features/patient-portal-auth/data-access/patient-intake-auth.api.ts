import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ActivatePatientIntakeAccountRequest,
  CurrentPatientIntakeSession,
  LoginPatientIntakeAccountRequest,
  PatientIntakeAuthenticationResponse
} from '../models/patient-intake-auth.models';

@Injectable({ providedIn: 'root' })
export class PatientIntakeAuthApi {
  private readonly baseUrl = `${environment.apiUrl}/api/patient-portal/intake-auth`;

  constructor(private readonly http: HttpClient) {}

  activate(request: ActivatePatientIntakeAccountRequest): Observable<PatientIntakeAuthenticationResponse> {
    return this.http.post<PatientIntakeAuthenticationResponse>(`${this.baseUrl}/activate`, request);
  }

  login(
    tenantSubdomain: string,
    request: LoginPatientIntakeAccountRequest
  ): Observable<PatientIntakeAuthenticationResponse> {
    return this.http.post<PatientIntakeAuthenticationResponse>(
      `${this.baseUrl}/realms/${encodeURIComponent(tenantSubdomain)}/login`,
      request
    );
  }

  getCurrent(): Observable<CurrentPatientIntakeSession> {
    return this.http.get<CurrentPatientIntakeSession>(`${this.baseUrl}/me`);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {});
  }
}
