import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-odontogram-empty-state',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="empty-card">
      <p class="eyebrow">{{ 'Release' | t }} 4.3 / {{ 'Basic Dental Findings Foundation' | t }}</p>
      <h3>{{ 'No odontogram yet' | t }}</h3>
      <p class="copy">
        {{ 'The odontogram is not auto-created. Start it explicitly to initialize the permanent adult FDI chart for this patient before registering any surface findings.' | t }}
      </p>
      <button
        *ngIf="canWrite"
        type="button"
        class="primary-action"
        [disabled]="creating"
        (click)="createRequested.emit()">
        {{ (creating ? 'Creating...' : 'Create odontogram') | t }}
      </button>
      <p *ngIf="!canWrite" class="muted">
        {{ 'Your current access can read the patient context, but only roles with odontogram write permission can initialize this slice.' | t }}
      </p>
    </section>
  `,
  styles: [`
    .empty-card {
      border-radius: 20px;
      border: 1px dashed #b4c4d4;
      background: linear-gradient(180deg, #fbfdff 0%, #eef5fb 100%);
      padding: 1.5rem;
      display: grid;
      gap: 0.8rem;
    }

    .eyebrow {
      margin: 0;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: #16324f;
    }

    .copy,
    .muted {
      margin: 0;
      color: #5b6e84;
      max-width: 60ch;
    }

    .primary-action {
      width: fit-content;
      border: none;
      border-radius: 999px;
      padding: 0.8rem 1.2rem;
      background: #0a5bb5;
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    .primary-action:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }
  `]
})
export class OdontogramEmptyStateComponent {
  @Input() canWrite = false;
  @Input() creating = false;
  @Output() readonly createRequested = new EventEmitter<void>();
}
