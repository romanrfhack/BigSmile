import { Injectable, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { PatientDocumentsApiService } from '../data-access/patient-documents-api.service';
import { PatientDocument } from '../models/patient-document.models';

@Injectable({
  providedIn: 'root'
})
export class PatientDocumentsFacade {
  private readonly patientDocumentsApi = inject(PatientDocumentsApiService);

  readonly documents = signal<PatientDocument[]>([]);
  readonly loadingDocuments = signal(false);
  readonly documentsError = signal<string | null>(null);

  loadDocuments(patientId: string): void {
    this.loadingDocuments.set(true);
    this.documentsError.set(null);

    this.patientDocumentsApi.getDocuments(patientId)
      .subscribe({
        next: (documents) => {
          this.documents.set(documents);
          this.loadingDocuments.set(false);
          this.documentsError.set(null);
        },
        error: () => {
          this.documents.set([]);
          this.loadingDocuments.set(false);
          this.documentsError.set('The patient documents could not be loaded.');
        }
      });
  }

  clearDocuments(): void {
    this.documents.set([]);
    this.loadingDocuments.set(false);
    this.documentsError.set(null);
  }

  uploadDocument(patientId: string, file: File) {
    return this.patientDocumentsApi.uploadDocument(patientId, file).pipe(
      tap((document) => {
        this.documents.update((currentDocuments) => [document, ...currentDocuments.filter((entry) => entry.documentId !== document.documentId)]);
        this.documentsError.set(null);
      })
    );
  }

  downloadDocument(patientId: string, documentId: string) {
    return this.patientDocumentsApi.downloadDocument(patientId, documentId);
  }

  retireDocument(patientId: string, documentId: string) {
    return this.patientDocumentsApi.retireDocument(patientId, documentId).pipe(
      tap(() => {
        this.documents.update((currentDocuments) => currentDocuments.filter((document) => document.documentId !== documentId));
        this.documentsError.set(null);
      })
    );
  }
}
