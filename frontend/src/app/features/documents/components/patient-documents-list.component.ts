import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PatientDocument } from '../models/patient-document.models';

@Component({
  selector: 'app-patient-documents-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="documents-card">
      <div class="card-head">
        <div>
          <h3>Active documents</h3>
          <p>Operational patient attachments only. Retired documents are hidden from this list and cannot be downloaded through the normal flow.</p>
        </div>
        <span class="count-pill">{{ documents.length }} active</span>
      </div>

      <ul class="documents-list">
        <li *ngFor="let document of documents" class="document-row">
          <div class="document-main">
            <h4>{{ document.originalFileName }}</h4>
            <dl>
              <div>
                <dt>Type</dt>
                <dd>{{ document.contentType }}</dd>
              </div>
              <div>
                <dt>Size</dt>
                <dd>{{ formatSize(document.sizeBytes) }}</dd>
              </div>
              <div>
                <dt>Uploaded</dt>
                <dd>{{ document.uploadedAtUtc | date: 'medium' }}</dd>
              </div>
              <div>
                <dt>Uploaded by</dt>
                <dd>{{ document.uploadedByUserId }}</dd>
              </div>
            </dl>
          </div>

          <div class="actions">
            <button
              type="button"
              class="primary-action"
              (click)="downloadRequested.emit(document)"
              [disabled]="downloadingDocumentId === document.documentId">
              {{ downloadingDocumentId === document.documentId ? 'Downloading...' : 'Download' }}
            </button>
            <button
              *ngIf="canRetire"
              type="button"
              class="danger-action"
              (click)="retireRequested.emit(document.documentId)"
              [disabled]="retiringDocumentId === document.documentId">
              {{ retiringDocumentId === document.documentId ? 'Retiring...' : 'Retire' }}
            </button>
          </div>
        </li>
      </ul>
    </section>
  `,
  styles: [`
    .documents-card {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
      display: grid;
      gap: 1rem;
    }

    .card-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    h3,
    h4 {
      margin: 0;
      color: #16324f;
    }

    p {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 62ch;
    }

    .count-pill {
      border-radius: 999px;
      padding: 0.45rem 0.75rem;
      background: #edf4fb;
      color: #356088;
      font-weight: 700;
      white-space: nowrap;
    }

    .documents-list {
      list-style: none;
      margin: 0;
      padding: 0;
      display: grid;
      gap: 1rem;
    }

    .document-row {
      border: 1px solid #d7dfe8;
      border-radius: 18px;
      padding: 1rem;
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
      background: #ffffff;
    }

    dl {
      margin: 0.8rem 0 0;
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #718298;
    }

    dd {
      margin: 0;
      color: #16324f;
      font-weight: 600;
      word-break: break-word;
    }

    .actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    button {
      border: 0;
      border-radius: 999px;
      padding: 0.75rem 1rem;
      font-weight: 700;
      cursor: pointer;
    }

    .primary-action {
      background: #0a5bb5;
      color: #ffffff;
    }

    .danger-action {
      background: #fff1f1;
      color: #9b2d30;
    }

    button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .card-head,
      .document-row {
        flex-direction: column;
      }
    }
  `]
})
export class PatientDocumentsListComponent {
  @Input() documents: PatientDocument[] = [];
  @Input() canRetire = false;
  @Input() downloadingDocumentId: string | null = null;
  @Input() retiringDocumentId: string | null = null;

  @Output() downloadRequested = new EventEmitter<PatientDocument>();
  @Output() retireRequested = new EventEmitter<string>();

  formatSize(sizeBytes: number): string {
    if (sizeBytes >= 1024 * 1024) {
      return `${(sizeBytes / (1024 * 1024)).toFixed(1)} MB`;
    }

    return `${Math.max(1, Math.round(sizeBytes / 1024))} KB`;
  }
}
