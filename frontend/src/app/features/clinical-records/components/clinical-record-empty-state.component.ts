import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '../../../shared/i18n';
import { EmptyStateComponent } from '../../../shared/ui';

@Component({
  selector: 'app-clinical-record-empty-state',
  standalone: true,
  imports: [CommonModule, EmptyStateComponent, TranslatePipe],
  template: `
    <app-empty-state
      [title]="'No clinical record yet' | t"
      [description]="'Start the clinical record when the patient is ready for clinical intake. It is created only when you confirm it.' | t"
      [icon]="'+'">
      <button
        *ngIf="canWrite"
        type="button"
        empty-state-action
        class="btn btn-primary"
        (click)="createRequested.emit()">
        {{ 'Create clinical record' | t }}
      </button>

      <p *ngIf="!canWrite" empty-state-action class="muted">
        {{ 'Your current session can read clinical data but cannot create or update the record.' | t }}
      </p>
    </app-empty-state>
  `,
  styles: [`
    p {
      margin: 0;
      color: var(--bsm-color-text-muted);
    }

    .muted {
      color: var(--bsm-color-text-muted);
    }

    .btn {
      justify-self: start;
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }
  `]
})
export class ClinicalRecordEmptyStateComponent {
  @Input() canWrite = false;

  @Output() createRequested = new EventEmitter<void>();
}
