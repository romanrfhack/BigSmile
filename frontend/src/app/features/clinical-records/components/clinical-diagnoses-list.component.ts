import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ClinicalDiagnosis } from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-diagnoses-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="diagnoses-list">
      <div *ngIf="!sortedDiagnoses.length" class="empty-copy">
        No diagnoses have been added yet.
      </div>

      <article *ngFor="let diagnosis of sortedDiagnoses" class="diagnosis-card">
        <header class="diagnosis-head">
          <div>
            <strong>{{ diagnosis.diagnosisText }}</strong>
            <p>Registered {{ diagnosis.createdAtUtc | date: 'medium' }} by {{ diagnosis.createdByUserId }}</p>
          </div>

          <div class="head-actions">
            <span class="status-badge" [class.status-resolved]="diagnosis.status === 'Resolved'">
              {{ diagnosis.status }}
            </span>

            <button
              *ngIf="canWrite && diagnosis.status === 'Active'"
              type="button"
              class="resolve-btn"
              [disabled]="resolvingDiagnosisId === diagnosis.diagnosisId"
              (click)="resolveRequested.emit(diagnosis.diagnosisId)">
              {{ resolvingDiagnosisId === diagnosis.diagnosisId ? 'Resolving...' : 'Mark resolved' }}
            </button>
          </div>
        </header>

        <p *ngIf="diagnosis.notes" class="notes-copy">{{ diagnosis.notes }}</p>

        <p *ngIf="diagnosis.status === 'Resolved'" class="resolved-copy">
          Resolved {{ diagnosis.resolvedAtUtc | date: 'medium' }} by {{ diagnosis.resolvedByUserId }}
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
      border: 1px dashed #c7d4e0;
      background: #f7fafc;
      padding: 1rem;
      color: #607387;
    }

    .diagnosis-card {
      display: grid;
      gap: 0.75rem;
      border-radius: 16px;
      border: 1px solid #d7dfe8;
      background: #ffffff;
      padding: 1rem;
    }

    .diagnosis-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
    }

    strong {
      color: #16324f;
      font-size: 1rem;
    }

    .diagnosis-head p,
    .resolved-copy {
      margin: 0.2rem 0 0;
      color: #607387;
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
      background: #e7f4ea;
      color: #1d6a36;
      font-weight: 700;
    }

    .status-resolved {
      background: #eef2f6;
      color: #536677;
    }

    .resolve-btn {
      border: none;
      border-radius: 999px;
      padding: 0.65rem 0.95rem;
      background: #16324f;
      color: #ffffff;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .notes-copy {
      margin: 0;
      color: #35506d;
      white-space: pre-wrap;
    }
  `]
})
export class ClinicalDiagnosesListComponent {
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
}
