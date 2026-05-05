import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-patient-documents-empty-state',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="empty-state">
      <h3>{{ 'No active documents yet' | t }}</h3>
      <p>
        {{ 'Documents are never auto-created in this slice. Upload a PDF, JPG, or PNG explicitly to keep a private patient document record.' | t }}
      </p>
      <p class="footnote" *ngIf="!canWrite">
        {{ 'Your current session can read this patient context, but only roles with document write permission can upload or retire documents.' | t }}
      </p>
      <p class="footnote" *ngIf="canWrite">
        {{ 'Active documents only. OCR, rich preview, versioning, and dashboard flows remain outside Release 7.1.' | t }}
      </p>
    </section>
  `,
  styles: [`
    .empty-state {
      border-radius: 20px;
      border: 1px dashed #bfd0e3;
      background: #f7fbff;
      padding: 1.4rem 1.5rem;
    }

    h3 {
      margin: 0;
      color: #16324f;
    }

    p {
      margin: 0.75rem 0 0;
      color: #5b6e84;
      max-width: 60ch;
    }

    .footnote {
      color: #6f8297;
      font-size: 0.95rem;
    }
  `]
})
export class PatientDocumentsEmptyStateComponent {
  @Input() canWrite = false;
}
