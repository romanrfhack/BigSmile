import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { PatientDocumentUploadFormComponent } from '../components/patient-document-upload-form.component';
import { PatientDocumentsEmptyStateComponent } from '../components/patient-documents-empty-state.component';
import { PatientDocumentsListComponent } from '../components/patient-documents-list.component';
import { PatientDocumentsFacade } from '../facades/patient-documents.facade';
import { PatientDocument } from '../models/patient-document.models';

@Component({
  selector: 'app-patient-documents-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    PatientDocumentUploadFormComponent,
    PatientDocumentsEmptyStateComponent,
    PatientDocumentsListComponent,
    TranslatePipe
  ],
  template: `
    <section class="patient-documents-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 7.1 / {{ 'Documents Foundation' | t }}</p>
          <h2>{{ 'Documents' | t }}</h2>
          <p class="subtitle">
            {{ 'Minimal patient-scoped document registry for {patientDisplayName} with explicit upload, active list, authorized download, logical retire, and private storage. No OCR, rich preview, versioning, or dashboard behavior in this slice.' | t:{ patientDisplayName } }}
          </p>
        </div>

        <div class="head-actions">
          <a *ngIf="patientId" [routerLink]="['/patients', patientId]" class="action-link action-secondary">{{ 'Back to patient' | t }}</a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient() || patientDocumentsFacade.loadingDocuments()" class="state-card">
        {{ 'Loading patient documents...' | t }}
      </div>

      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() | t }}
      </div>

      <div *ngIf="patientDocumentsFacade.documentsError()" class="state-card state-error">
        {{ patientDocumentsFacade.documentsError() | t }}
      </div>

      <div *ngIf="actionError" class="state-card state-error">
        {{ actionError | t }}
      </div>

      <app-patient-document-upload-form
        *ngIf="canWrite"
        [saving]="uploadingDocument"
        [error]="uploadError"
        [revision]="uploadRevision"
        (uploadRequested)="uploadDocument($event)">
      </app-patient-document-upload-form>

      <app-patient-documents-empty-state
        *ngIf="!patientDocumentsFacade.loadingDocuments()
          && !patientDocumentsFacade.documentsError()
          && patientDocumentsFacade.documents().length === 0"
        [canWrite]="canWrite">
      </app-patient-documents-empty-state>

      <app-patient-documents-list
        *ngIf="patientDocumentsFacade.documents().length > 0"
        [documents]="patientDocumentsFacade.documents()"
        [canRetire]="canWrite"
        [downloadingDocumentId]="downloadingDocumentId"
        [retiringDocumentId]="retiringDocumentId"
        (downloadRequested)="downloadDocument($event)"
        (retireRequested)="retireDocument($event)">
      </app-patient-documents-list>
    </section>
  `,
  styles: [`
    .patient-documents-page {
      display: grid;
      gap: 1.25rem;
    }

    .page-head,
    .state-card {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .page-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2 {
      margin: 0;
      color: #16324f;
    }

    .subtitle {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 66ch;
    }

    .head-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .action-link {
      text-decoration: none;
      color: #0a5bb5;
      font-weight: 700;
    }

    .action-secondary {
      color: #4d6278;
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }

    @media (max-width: 768px) {
      .page-head {
        flex-direction: column;
      }
    }
  `]
})
export class PatientDocumentsPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly patientsFacade = inject(PatientsFacade);
  readonly patientDocumentsFacade = inject(PatientDocumentsFacade);
  readonly canWrite = this.authService.hasPermissions(['document.write']);
  private readonly i18n = inject(I18nService);

  patientId: string | null = null;
  uploadingDocument = false;
  downloadingDocumentId: string | null = null;
  retiringDocumentId: string | null = null;
  uploadRevision = 0;
  uploadError: string | null = null;
  actionError: string | null = null;

  get patientDisplayName(): string {
    return this.patientsFacade.currentPatient()?.fullName ?? this.i18n.translate('this patient');
  }

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id');
    if (!this.patientId) {
      this.patientsFacade.clearCurrentPatient();
      this.patientDocumentsFacade.clearDocuments();
      return;
    }

    this.patientsFacade.clearCurrentPatient();
    this.patientsFacade.loadPatient(this.patientId);
    this.patientDocumentsFacade.clearDocuments();
    this.patientDocumentsFacade.loadDocuments(this.patientId);
  }

  uploadDocument(file: File): void {
    if (!this.patientId) {
      return;
    }

    this.uploadingDocument = true;
    this.uploadError = null;
    this.actionError = null;

    this.patientDocumentsFacade.uploadDocument(this.patientId, file)
      .subscribe({
        next: () => {
          this.uploadingDocument = false;
          this.uploadRevision += 1;
        },
        error: () => {
          this.uploadingDocument = false;
          this.uploadError = 'The document could not be uploaded.';
        }
      });
  }

  downloadDocument(document: PatientDocument): void {
    if (!this.patientId) {
      return;
    }

    this.downloadingDocumentId = document.documentId;
    this.actionError = null;

    this.patientDocumentsFacade.downloadDocument(this.patientId, document.documentId)
      .subscribe({
        next: (blob) => {
          this.downloadingDocumentId = null;
          this.triggerBrowserDownload(blob, document.originalFileName);
        },
        error: () => {
          this.downloadingDocumentId = null;
          this.actionError = 'The document could not be downloaded.';
        }
      });
  }

  retireDocument(documentId: string): void {
    if (!this.patientId) {
      return;
    }

    this.retiringDocumentId = documentId;
    this.actionError = null;

    this.patientDocumentsFacade.retireDocument(this.patientId, documentId)
      .subscribe({
        next: () => {
          this.retiringDocumentId = null;
        },
        error: () => {
          this.retiringDocumentId = null;
          this.actionError = 'The document could not be retired.';
        }
      });
  }

  private triggerBrowserDownload(blob: Blob, fileName: string): void {
    const objectUrl = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(objectUrl);
  }
}
