import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ChangeTreatmentQuoteStatusRequest,
  TreatmentQuote,
  UpdateTreatmentQuoteItemPriceRequest
} from '../models/treatment-quote.models';

@Injectable({
  providedIn: 'root'
})
export class TreatmentQuotesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/patients`;

  getTreatmentQuote(patientId: string): Observable<TreatmentQuote> {
    return this.http.get<TreatmentQuote>(`${this.baseUrl}/${patientId}/treatment-plan/quote`);
  }

  createTreatmentQuote(patientId: string): Observable<TreatmentQuote> {
    return this.http.post<TreatmentQuote>(`${this.baseUrl}/${patientId}/treatment-plan/quote`, {});
  }

  updateItemPrice(
    patientId: string,
    quoteItemId: string,
    payload: UpdateTreatmentQuoteItemPriceRequest
  ): Observable<TreatmentQuote> {
    return this.http.put<TreatmentQuote>(
      `${this.baseUrl}/${patientId}/treatment-plan/quote/items/${quoteItemId}/price`,
      payload
    );
  }

  changeStatus(
    patientId: string,
    payload: ChangeTreatmentQuoteStatusRequest
  ): Observable<TreatmentQuote> {
    return this.http.put<TreatmentQuote>(`${this.baseUrl}/${patientId}/treatment-plan/quote/status`, payload);
  }
}
