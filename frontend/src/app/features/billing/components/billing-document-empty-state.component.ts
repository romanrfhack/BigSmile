import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-billing-document-empty-state',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="empty-state">
      <p class="eyebrow">{{ 'Release' | t }} 6.1 / {{ 'Billing Foundation' | t }}</p>
      <h3>{{ 'No billing document yet' | t }}</h3>
      <p>
        {{ 'This patient already has an accepted quote, but the billing document is never auto-created in this slice. Create it explicitly to snapshot the accepted quote lines and total into a draft billing document.' | t }}
      </p>

      <button
        *ngIf="canWrite"
        type="button"
        class="btn btn-primary"
        [disabled]="creating"
        (click)="createRequested.emit()">
        {{ (creating ? 'Creating...' : 'Create billing document from accepted quote') | t }}
      </button>

      <p *ngIf="!canWrite" class="muted">
        {{ 'Your current session can read this patient context, but only roles with billing write permission can initialize the billing document.' | t }}
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
export class BillingDocumentEmptyStateComponent {
  @Input() canWrite = false;
  @Input() creating = false;

  @Output() createRequested = new EventEmitter<void>();
}
