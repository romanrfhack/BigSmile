import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { DashboardSummaryCardsComponent } from '../components/dashboard-summary-cards.component';
import { DashboardFacade } from '../facades/dashboard.facade';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, DashboardSummaryCardsComponent],
  template: `
    <section class="dashboard-page">
      <header class="dashboard-head">
        <div>
          <p class="eyebrow">Release 7.2 / Dashboard Foundation</p>
          <h2>Operational dashboard</h2>
          <p class="subtitle">
            Tenant-scoped summary of current clinic operations. This foundation uses simple read-model aggregation only.
          </p>
        </div>
        <p *ngIf="dashboardFacade.summary() as summary" class="generated-at">
          Generated {{ summary.generatedAtUtc | date: 'medium' }}
        </p>
      </header>

      <div *ngIf="dashboardFacade.loading()" class="state-card">Loading dashboard summary...</div>

      <div *ngIf="dashboardFacade.error()" class="state-card state-error">
        {{ dashboardFacade.error() }}
      </div>

      <app-dashboard-summary-cards
        *ngIf="!dashboardFacade.loading() && !dashboardFacade.error() && dashboardFacade.summary() as summary"
        [summary]="summary">
      </app-dashboard-summary-cards>

      <div
        *ngIf="!dashboardFacade.loading() && !dashboardFacade.error() && !dashboardFacade.summary()"
        class="state-card">
        No dashboard summary is loaded yet.
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
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
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
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2 {
      margin: 0;
      color: #16324f;
    }

    .subtitle {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 62ch;
    }

    .generated-at {
      margin: 0;
      color: #5b6e84;
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
