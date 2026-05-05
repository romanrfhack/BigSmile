import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { TranslatePipe } from '../../../shared/i18n';
import { DashboardSummary } from '../models/dashboard-summary.models';

type DashboardCard = {
  label: string;
  value: number;
  hint: string;
};

@Component({
  selector: 'app-dashboard-summary-cards',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="summary-grid" [attr.aria-label]="'Dashboard summary' | t">
      <article *ngFor="let card of cards" class="summary-card">
        <p>{{ card.label }}</p>
        <strong>{{ card.value }}</strong>
        <span>{{ card.hint }}</span>
      </article>
    </section>
  `,
  styles: [`
    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
      gap: 1rem;
    }

    .summary-card {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-lg);
      padding: 1.2rem;
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-sm);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .summary-card:hover {
      border-color: rgba(0, 126, 163, 0.28);
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .summary-card p {
      margin: 0;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .summary-card strong {
      display: block;
      margin: 0.55rem 0 0.35rem;
      color: var(--bsm-color-primary);
      font-size: clamp(2rem, 5vw, 3.2rem);
      line-height: 1;
    }

    .summary-card span {
      color: var(--bsm-color-text-muted);
      font-size: 0.92rem;
    }
  `]
})
export class DashboardSummaryCardsComponent {
  private readonly i18n = inject(I18nService);

  @Input({ required: true }) summary!: DashboardSummary;

  get cards(): DashboardCard[] {
    return [
      {
        label: this.i18n.translate('Active patients'),
        value: this.summary.activePatientsCount,
        hint: this.i18n.translate('Patients currently active in this tenant.')
      },
      {
        label: this.i18n.translate('Today appointments'),
        value: this.summary.todayAppointmentsCount,
        hint: this.i18n.translate('Appointments whose start time falls today.')
      },
      {
        label: this.i18n.translate('Today pending'),
        value: this.summary.todayPendingAppointmentsCount,
        hint: this.i18n.translate('Today appointments still in Scheduled status.')
      },
      {
        label: this.i18n.translate('Active documents'),
        value: this.summary.activeDocumentsCount,
        hint: this.i18n.translate('Patient documents not retired.')
      },
      {
        label: this.i18n.translate('Treatment plans'),
        value: this.summary.activeTreatmentPlansCount,
        hint: this.i18n.translate('Existing patient treatment plans.')
      },
      {
        label: this.i18n.translate('Accepted quotes'),
        value: this.summary.acceptedQuotesCount,
        hint: this.i18n.translate('Treatment quotes in Accepted status.')
      },
      {
        label: this.i18n.translate('Issued billing'),
        value: this.summary.issuedBillingDocumentsCount,
        hint: this.i18n.translate('Billing documents in Issued status.')
      }
    ];
  }
}
