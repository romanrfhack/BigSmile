import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { ClinicalDiagnosis } from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-diagnoses-list',
  standalone: true,
  imports: [CommonModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="diagnoses-list">
      <div *ngIf="!sortedDiagnoses.length" class="empty-copy">
        {{ 'No diagnoses have been added yet.' | t }}
      </div>

      <article *ngFor="let diagnosis of sortedDiagnoses" class="diagnosis-card">
        <header class="diagnosis-head">
          <div>
            <strong>{{ diagnosis.diagnosisText }}</strong>
            <p>{{ 'Registered' | t }} {{ diagnosis.createdAtUtc | bsDate: 'medium' }} {{ 'by' | t }} {{ diagnosis.createdByUserId }}</p>
          </div>

          <div class="head-actions">
            <span class="status-badge" [class.status-resolved]="diagnosis.status === 'Resolved'">
              {{ getStatusLabel(diagnosis.status) }}
            </span>

            <button
              *ngIf="canWrite && diagnosis.status === 'Active'"
              type="button"
              class="resolve-btn"
              [disabled]="resolvingDiagnosisId === diagnosis.diagnosisId"
              (click)="resolveRequested.emit(diagnosis.diagnosisId)">
              {{ (resolvingDiagnosisId === diagnosis.diagnosisId ? 'Resolving...' : 'Mark resolved') | t }}
            </button>
          </div>
        </header>

        <p *ngIf="diagnosis.notes" class="notes-copy">{{ diagnosis.notes }}</p>

        <p *ngIf="diagnosis.status === 'Resolved'" class="resolved-copy">
          {{ 'Resolved' | t }} {{ diagnosis.resolvedAtUtc | bsDate: 'medium' }} {{ 'by' | t }} {{ diagnosis.resolvedByUserId }}
        </p>
      </article>
    </section>
  `,
  styles: [`
    .diagnoses-list {
      display: grid;
      gap: 0.9rem;
    }

    .empty-copy {
      border-radius: 16px;
      border: 1px dashed var(--bsm-color-border);
      background: var(--bsm-color-neutral-soft);
      padding: 1rem;
      color: var(--bsm-color-text-muted);
    }

    .diagnosis-card {
      display: grid;
      gap: 0.75rem;
      border-radius: 16px;
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
      padding: 1rem;
    }

    .diagnosis-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
    }

    strong {
      color: var(--bsm-color-text-brand);
      font-size: 1rem;
    }

    .diagnosis-head p,
    .resolved-copy {
      margin: 0.2rem 0 0;
      color: var(--bsm-color-text-muted);
    }

    .head-actions {
      display: flex;
      gap: 0.75rem;
      align-items: center;
      flex-wrap: wrap;
    }

    .status-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 88px;
      padding: 0.45rem 0.8rem;
      border-radius: 999px;
      background: var(--bsm-color-success-soft);
      color: var(--bsm-color-success-text);
      font-weight: 700;
    }

    .status-resolved {
      background: var(--bsm-color-neutral-soft);
      color: var(--bsm-color-text-muted);
    }

    .resolve-btn {
      border: none;
      border-radius: 999px;
      padding: 0.65rem 0.95rem;
      background: var(--bsm-color-text-brand);
      color: var(--bsm-color-bg);
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .notes-copy {
      margin: 0;
      color: var(--bsm-color-text);
      white-space: pre-wrap;
    }
  `]
})
export class ClinicalDiagnosesListComponent {
  private readonly i18n = inject(I18nService);

  @Input() diagnoses: ClinicalDiagnosis[] = [];
  @Input() canWrite = false;
  @Input() resolvingDiagnosisId: string | null = null;

  @Output() resolveRequested = new EventEmitter<string>();

  get sortedDiagnoses(): ClinicalDiagnosis[] {
    return [...this.diagnoses].sort((left, right) => {
      if (left.status !== right.status) {
        return left.status === 'Active' ? -1 : 1;
      }

      return Date.parse(right.createdAtUtc) - Date.parse(left.createdAtUtc);
    });
  }

  getStatusLabel(status: ClinicalDiagnosis['status']): string {
    return this.i18n.translate(status);
  }
}
