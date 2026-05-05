import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-treatment-plan-empty-state',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="empty-state">
      <p class="eyebrow">{{ 'Release' | t }} 5.1 / {{ 'Treatment Plan Foundation' | t }}</p>
      <h3>{{ 'No treatment plan yet' | t }}</h3>
      <p>
        {{ 'This patient does not have a treatment plan yet. It is not auto-created in this slice and must be initialized explicitly before adding items.' | t }}
      </p>

      <button
        *ngIf="canWrite"
        type="button"
        class="btn btn-primary"
        [disabled]="creating"
        (click)="createRequested.emit()">
        {{ (creating ? 'Creating...' : 'Create treatment plan') | t }}
      </button>

      <p *ngIf="!canWrite" class="muted">
        {{ 'Your current session can read the patient context, but only roles with treatment plan write permission can initialize this slice.' | t }}
      </p>
    </section>
  `,
  styles: [`
    .empty-state {
      display: grid;
      gap: 0.85rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
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

    p {
      margin: 0;
      color: #5b6e84;
      max-width: 58ch;
    }

    .muted {
      color: #7a8ea3;
    }

    .btn {
      justify-self: start;
      border: none;
      border-radius: 999px;
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: #0a5bb5;
      color: #ffffff;
    }
  `]
})
export class TreatmentPlanEmptyStateComponent {
  @Input() canWrite = false;
  @Input() creating = false;

  @Output() createRequested = new EventEmitter<void>();
}
