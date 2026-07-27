import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  IssuedPatientIntakeAccessLink,
  IssuePatientIntakeAccessLinkRequest,
  PatientIntakeAccessLinkSummary
} from '../models/patient-intake-link.models';

@Injectable({ providedIn: 'root' })
export class PatientIntakeLinksApi {
  private readonly baseUrl = `${environment.apiUrl}/api/patient-intake-links`;

  constructor(private readonly http: HttpClient) {}

  list(includeResolved = true, take = 50): Observable<PatientIntakeAccessLinkSummary[]> {
    const params = new HttpParams()
      .set('includeResolved', includeResolved)
      .set('take', take);

    return this.http.get<PatientIntakeAccessLinkSummary[]>(this.baseUrl, { params });
  }

  issue(branchId: string | null): Observable<IssuedPatientIntakeAccessLink> {
    const request: IssuePatientIntakeAccessLinkRequest = { branchId };
    return this.http.post<IssuedPatientIntakeAccessLink>(this.baseUrl, request);
  }

  revoke(linkId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${encodeURIComponent(linkId)}`);
  }
}
