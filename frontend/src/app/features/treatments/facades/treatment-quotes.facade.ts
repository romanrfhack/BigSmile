import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { TreatmentQuotesApiService } from '../data-access/treatment-quotes-api.service';
import {
  ChangeTreatmentQuoteStatusRequest,
  TreatmentQuote,
  UpdateTreatmentQuoteItemPriceRequest
} from '../models/treatment-quote.models';

@Injectable({
  providedIn: 'root'
})
export class TreatmentQuotesFacade {
  private readonly treatmentQuotesApi = inject(TreatmentQuotesApiService);

  readonly currentTreatmentQuote = signal<TreatmentQuote | null>(null);
  readonly loadingTreatmentQuote = signal(false);
  readonly treatmentQuoteMissing = signal(false);
  readonly treatmentQuoteError = signal<string | null>(null);

  loadTreatmentQuote(patientId: string): void {
    this.loadingTreatmentQuote.set(true);
    this.treatmentQuoteMissing.set(false);
    this.treatmentQuoteError.set(null);

    this.treatmentQuotesApi.getTreatmentQuote(patientId)
      .subscribe({
        next: (treatmentQuote) => {
          this.currentTreatmentQuote.set(treatmentQuote);
          this.loadingTreatmentQuote.set(false);
          this.treatmentQuoteMissing.set(false);
          this.treatmentQuoteError.set(null);
        },
        error: (error: unknown) => {
          this.currentTreatmentQuote.set(null);
          this.loadingTreatmentQuote.set(false);

          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.treatmentQuoteMissing.set(true);
            this.treatmentQuoteError.set(null);
            return;
          }

          this.treatmentQuoteMissing.set(false);
          this.treatmentQuoteError.set('The treatment quote could not be loaded.');
        }
      });
  }

  clearTreatmentQuote(): void {
    this.currentTreatmentQuote.set(null);
    this.loadingTreatmentQuote.set(false);
    this.treatmentQuoteMissing.set(false);
    this.treatmentQuoteError.set(null);
  }

  createTreatmentQuote(patientId: string) {
    return this.treatmentQuotesApi.createTreatmentQuote(patientId).pipe(
      tap((treatmentQuote) => this.setLoadedTreatmentQuote(treatmentQuote))
    );
  }

  updateItemPrice(patientId: string, quoteItemId: string, payload: UpdateTreatmentQuoteItemPriceRequest) {
    return this.treatmentQuotesApi.updateItemPrice(patientId, quoteItemId, payload).pipe(
      tap((treatmentQuote) => this.setLoadedTreatmentQuote(treatmentQuote))
    );
  }

  changeStatus(patientId: string, payload: ChangeTreatmentQuoteStatusRequest) {
    return this.treatmentQuotesApi.changeStatus(patientId, payload).pipe(
      tap((treatmentQuote) => this.setLoadedTreatmentQuote(treatmentQuote))
    );
  }

  private setLoadedTreatmentQuote(treatmentQuote: TreatmentQuote): void {
    this.currentTreatmentQuote.set(treatmentQuote);
    this.loadingTreatmentQuote.set(false);
    this.treatmentQuoteMissing.set(false);
    this.treatmentQuoteError.set(null);
  }
}
