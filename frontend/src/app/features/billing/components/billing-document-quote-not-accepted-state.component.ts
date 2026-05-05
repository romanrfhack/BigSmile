import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-billing-document-quote-not-accepted-state',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  template: `
    <section class="empty-state">
      <p class="eyebrow">{{ 'Release' | t }} 6.1 / {{ 'Billing Foundation' | t }}</p>
      <h3>{{ 'Quote must be accepted before billing' | t }}</h3>
      <p>
        {{ 'The current quote status is' | t }} <strong>{{ quoteStatus | t }}</strong>. {{ 'Billing can only be created from an explicitly accepted quote in this slice.' | t }}
      </p>

      <a
        *ngIf="patientId"
        [routerLink]="['/patients', patientId, 'treatment-plan', 'quote']"
        class="action-link">
        {{ 'Review quote' | t }}
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

    strong {
      color: var(--bsm-color-text-brand);
    }

    .action-link {
      justify-self: start;
      text-decoration: none;
      color: var(--bsm-color-primary);
      font-weight: 700;
    }
  `]
})
export class BillingDocumentQuoteNotAcceptedStateComponent {
  @Input() patientId: string | null = null;
  @Input() quoteStatus = 'Draft';
}
