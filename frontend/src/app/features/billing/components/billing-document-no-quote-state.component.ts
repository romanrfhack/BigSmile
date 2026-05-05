import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-billing-document-no-quote-state',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  template: `
    <section class="empty-state">
      <p class="eyebrow">{{ 'Release' | t }} 6.1 / {{ 'Billing Foundation' | t }}</p>
      <h3>{{ 'Accepted quote required first' | t }}</h3>
      <p>
        {{ 'This slice never auto-creates billing, the quote, or the treatment plan. Create and accept the patient quote first, then return here to snapshot it into billing.' | t }}
      </p>

      <a
        *ngIf="patientId"
        [routerLink]="['/patients', patientId, 'treatment-plan', 'quote']"
        class="action-link">
        {{ 'Open quote' | t }}
      </a>
    </section>
  `,
  styles: [`
    .empty-state {
      display: grid;
      gap: 0.85rem;
      border-radius: 20px;
      border: 1px solid var(--bsm-color-border);
      background: linear-gradient(180deg, #ffffff 0%, var(--bsm-color-surface) 100%);
      padding: 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .eyebrow {
      margin: 0;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    p {
      margin: 0;
      color: var(--bsm-color-text-muted);
      max-width: 58ch;
    }

    .action-link {
      justify-self: start;
      text-decoration: none;
      color: var(--bsm-color-primary);
      font-weight: 700;
    }
  `]
})
export class BillingDocumentNoQuoteStateComponent {
  @Input() patientId: string | null = null;
}
