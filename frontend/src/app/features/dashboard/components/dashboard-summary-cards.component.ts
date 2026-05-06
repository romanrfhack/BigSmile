import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { TranslatePipe } from '../../../shared/i18n';
import { SummaryCardComponent, type SummaryCardTone } from '../../../shared/ui';
import { DashboardSummary } from '../models/dashboard-summary.models';

type DashboardCard = {
  label: string;
  value: number;
  hint: string;
  initial: string;
  tone: SummaryCardTone;
};

@Component({
  selector: 'app-dashboard-summary-cards',
  standalone: true,
  imports: [CommonModule, TranslatePipe, SummaryCardComponent],
  template: `
    <section class="summary-grid" [attr.aria-label]="'Dashboard summary' | t">
      <app-summary-card
        *ngFor="let card of cards"
        [label]="card.label"
        [value]="card.value"
        [helper]="card.hint"
        [initial]="card.initial"
        [tone]="card.tone">
      </app-summary-card>
    </section>
  `,
  styles: [`
    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
      gap: 1rem;
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
        hint: this.i18n.translate('Patients currently active in this tenant.'),
        initial: 'P',
        tone: 'primary'
      },
      {
        label: this.i18n.translate('Today appointments'),
        value: this.summary.todayAppointmentsCount,
        hint: this.i18n.translate('Appointments whose start time falls today.'),
        initial: 'A',
        tone: 'accent'
      },
      {
        label: this.i18n.translate('Today pending'),
        value: this.summary.todayPendingAppointmentsCount,
        hint: this.i18n.translate('Today appointments still in Scheduled status.'),
        initial: 'P',
        tone: 'warning'
      },
      {
        label: this.i18n.translate('Active documents'),
        value: this.summary.activeDocumentsCount,
        hint: this.i18n.translate('Patient documents not retired.'),
        initial: 'D',
        tone: 'neutral'
      },
      {
        label: this.i18n.translate('Treatment plans'),
        value: this.summary.activeTreatmentPlansCount,
        hint: this.i18n.translate('Existing patient treatment plans.'),
        initial: 'T',
        tone: 'success'
      },
      {
        label: this.i18n.translate('Accepted quotes'),
        value: this.summary.acceptedQuotesCount,
        hint: this.i18n.translate('Treatment quotes in Accepted status.'),
        initial: 'Q',
        tone: 'success'
      },
      {
        label: this.i18n.translate('Issued billing'),
        value: this.summary.issuedBillingDocumentsCount,
        hint: this.i18n.translate('Billing documents in Issued status.'),
        initial: 'B',
        tone: 'primary'
      }
    ];
  }
}
