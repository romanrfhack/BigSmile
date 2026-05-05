import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { DashboardSummaryCardsComponent } from '../components/dashboard-summary-cards.component';
import { DashboardFacade } from '../facades/dashboard.facade';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, DashboardSummaryCardsComponent, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="dashboard-page">
      <header class="dashboard-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 7.2 / {{ 'Dashboard Foundation' | t }}</p>
          <h2>{{ 'Operational dashboard' | t }}</h2>
          <p class="subtitle">
            {{ 'Tenant-scoped summary of current clinic operations. This foundation uses simple read-model aggregation only.' | t }}
          </p>
        </div>
        <p *ngIf="dashboardFacade.summary() as summary" class="generated-at">
          {{ 'Generated' | t }} {{ summary.generatedAtUtc | bsDate: 'medium' }}
        </p>
      </header>

      <div *ngIf="dashboardFacade.loading()" class="state-card">{{ 'Loading dashboard summary...' | t }}</div>

      <div *ngIf="dashboardFacade.error()" class="state-card state-error">
        {{ dashboardFacade.error() | t }}
      </div>

      <app-dashboard-summary-cards
        *ngIf="!dashboardFacade.loading() && !dashboardFacade.error() && dashboardFacade.summary() as summary"
        [summary]="summary">
      </app-dashboard-summary-cards>

      <div
        *ngIf="!dashboardFacade.loading() && !dashboardFacade.error() && !dashboardFacade.summary()"
        class="state-card">
        {{ 'No dashboard summary is loaded yet.' | t }}
      </div>
    </section>
  `,
  styles: [`
    .dashboard-page {
      display: grid;
      gap: 1.25rem;
    }

    .dashboard-head,
    .state-card {
      border-radius: var(--bsm-radius-lg);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-gradient-surface);
      padding: 1.4rem 1.5rem;
      box-shadow: var(--bsm-shadow-md);
    }

    .dashboard-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    .subtitle {
      margin: 0.45rem 0 0;
      color: var(--bsm-color-text-muted);
      max-width: 62ch;
    }

    .generated-at {
      margin: 0;
      color: var(--bsm-color-text-muted);
      font-weight: 700;
      white-space: nowrap;
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }

    @media (max-width: 768px) {
      .dashboard-head {
        flex-direction: column;
      }
    }
  `]
})
export class DashboardPageComponent implements OnInit {
  readonly dashboardFacade = inject(DashboardFacade);

  ngOnInit(): void {
    this.dashboardFacade.loadSummary();
  }
}
