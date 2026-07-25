import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ActivatePatientPortalAccountRequest,
  CurrentPatientPortalSession,
  LoginPatientPortalAccountRequest,
  PatientPortalAuthenticationResponse
} from '../models/patient-portal-auth.models';

@Injectable({ providedIn: 'root' })
export class PatientPortalAuthApi {
  private readonly baseUrl = `${environment.apiUrl}/api/patient-portal/auth`;

  constructor(private readonly http: HttpClient) {}

  activate(request: ActivatePatientPortalAccountRequest): Observable<PatientPortalAuthenticationResponse> {
    return this.http.post<PatientPortalAuthenticationResponse>(`${this.baseUrl}/activate`, request);
  }

  login(
    tenantSubdomain: string,
    request: LoginPatientPortalAccountRequest
  ): Observable<PatientPortalAuthenticationResponse> {
    return this.http.post<PatientPortalAuthenticationResponse>(
      `${this.baseUrl}/realms/${encodeURIComponent(tenantSubdomain)}/login`,
      request
    );
  }

  getCurrent(): Observable<CurrentPatientPortalSession> {
    return this.http.get<CurrentPatientPortalSession>(`${this.baseUrl}/me`);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {});
  }
}
