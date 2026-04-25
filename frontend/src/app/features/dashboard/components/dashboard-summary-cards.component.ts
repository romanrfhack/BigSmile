import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { DashboardSummary } from '../models/dashboard-summary.models';

type DashboardCard = {
  label: string;
  value: number;
  hint: string;
};

@Component({
  selector: 'app-dashboard-summary-cards',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="summary-grid" aria-label="Dashboard summary">
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
      border: 1px solid #d7dfe8;
      border-radius: 20px;
      padding: 1.2rem;
      background: linear-gradient(180deg, #ffffff 0%, #f4f8fb 100%);
      box-shadow: 0 18px 34px rgba(20, 48, 79, 0.08);
    }

    .summary-card p {
      margin: 0;
      color: #58708a;
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .summary-card strong {
      display: block;
      margin: 0.55rem 0 0.35rem;
      color: #14324d;
      font-size: clamp(2rem, 5vw, 3.2rem);
      line-height: 1;
    }

    .summary-card span {
      color: #5b6e84;
      font-size: 0.92rem;
    }
  `]
})
export class DashboardSummaryCardsComponent {
  @Input({ required: true }) summary!: DashboardSummary;

  get cards(): DashboardCard[] {
    return [
      {
        label: 'Active patients',
        value: this.summary.activePatientsCount,
        hint: 'Patients currently active in this tenant.'
      },
      {
        label: 'Today appointments',
        value: this.summary.todayAppointmentsCount,
        hint: 'Appointments whose start time falls today.'
      },
      {
        label: 'Today pending',
        value: this.summary.todayPendingAppointmentsCount,
        hint: 'Today appointments still in Scheduled status.'
      },
      {
        label: 'Active documents',
        value: this.summary.activeDocumentsCount,
        hint: 'Patient documents not retired.'
      },
      {
        label: 'Treatment plans',
        value: this.summary.activeTreatmentPlansCount,
        hint: 'Existing patient treatment plans.'
      },
      {
        label: 'Accepted quotes',
        value: this.summary.acceptedQuotesCount,
        hint: 'Treatment quotes in Accepted status.'
      },
      {
        label: 'Issued billing',
        value: this.summary.issuedBillingDocumentsCount,
        hint: 'Billing documents in Issued status.'
      }
    ];
  }
}
