import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { BillingDocumentsApiService } from '../data-access/billing-documents-api.service';
import {
  BillingDocument,
  ChangeBillingDocumentStatusRequest
} from '../models/billing-document.models';

@Injectable({
  providedIn: 'root'
})
export class BillingDocumentsFacade {
  private readonly billingDocumentsApi = inject(BillingDocumentsApiService);

  readonly currentBillingDocument = signal<BillingDocument | null>(null);
  readonly loadingBillingDocument = signal(false);
  readonly billingDocumentMissing = signal(false);
  readonly billingDocumentError = signal<string | null>(null);

  loadBillingDocument(patientId: string): void {
    this.loadingBillingDocument.set(true);
    this.billingDocumentMissing.set(false);
    this.billingDocumentError.set(null);

    this.billingDocumentsApi.getBillingDocument(patientId)
      .subscribe({
        next: (billingDocument) => {
          this.currentBillingDocument.set(billingDocument);
          this.loadingBillingDocument.set(false);
          this.billingDocumentMissing.set(false);
          this.billingDocumentError.set(null);
        },
        error: (error: unknown) => {
          this.currentBillingDocument.set(null);
          this.loadingBillingDocument.set(false);

          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.billingDocumentMissing.set(true);
            this.billingDocumentError.set(null);
            return;
          }

          this.billingDocumentMissing.set(false);
          this.billingDocumentError.set('The billing document could not be loaded.');
        }
      });
  }

  clearBillingDocument(): void {
    this.currentBillingDocument.set(null);
    this.loadingBillingDocument.set(false);
    this.billingDocumentMissing.set(false);
    this.billingDocumentError.set(null);
  }

  createBillingDocument(patientId: string) {
    return this.billingDocumentsApi.createBillingDocument(patientId).pipe(
      tap((billingDocument) => this.setLoadedBillingDocument(billingDocument))
    );
  }

  changeStatus(patientId: string, payload: ChangeBillingDocumentStatusRequest) {
    return this.billingDocumentsApi.changeStatus(patientId, payload).pipe(
      tap((billingDocument) => this.setLoadedBillingDocument(billingDocument))
    );
  }

  private setLoadedBillingDocument(billingDocument: BillingDocument): void {
    this.currentBillingDocument.set(billingDocument);
    this.loadingBillingDocument.set(false);
    this.billingDocumentMissing.set(false);
    this.billingDocumentError.set(null);
  }
}
