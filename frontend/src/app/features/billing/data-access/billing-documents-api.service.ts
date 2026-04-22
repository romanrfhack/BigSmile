import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  BillingDocument,
  ChangeBillingDocumentStatusRequest
} from '../models/billing-document.models';

@Injectable({
  providedIn: 'root'
})
export class BillingDocumentsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/patients`;

  getBillingDocument(patientId: string): Observable<BillingDocument> {
    return this.http.get<BillingDocument>(`${this.baseUrl}/${patientId}/treatment-plan/quote/billing`);
  }

  createBillingDocument(patientId: string): Observable<BillingDocument> {
    return this.http.post<BillingDocument>(`${this.baseUrl}/${patientId}/treatment-plan/quote/billing`, {});
  }

  changeStatus(
    patientId: string,
    payload: ChangeBillingDocumentStatusRequest
  ): Observable<BillingDocument> {
    return this.http.put<BillingDocument>(`${this.baseUrl}/${patientId}/treatment-plan/quote/billing/status`, payload);
  }
}
