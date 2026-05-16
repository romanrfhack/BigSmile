import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { DashboardSummaryCardsComponent } from '../components/dashboard-summary-cards.component';
import { DashboardFacade } from '../facades/dashboard.facade';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  PageHeaderComponent,
  SectionCardComponent,
  StatusBadgeComponent
} from '../../../shared/ui';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    CommonModule,
    DashboardSummaryCardsComponent,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    LocalizedDatePipe,
    PageHeaderComponent,
    SectionCardComponent,
    StatusBadgeComponent,
    TranslatePipe
  ],
  template: `
    <section class="dashboard-page">
      <app-page-header
        [eyebrow]="'Dashboard' | t"
        [title]="'Operational dashboard' | t"
        [subtitle]="'Tenant-scoped summary of current clinic operations.' | t">
        <app-status-badge
          *ngIf="dashboardFacade.summary() as summary"
          page-header-actions
          tone="info"
          [label]="('Generated' | t) + ' ' + (summary.generatedAtUtc | bsDate: 'medium')">
        </app-status-badge>
      </app-page-header>

      <app-section-card [title]="'Dashboard summary' | t" variant="elevated">
        <div *ngIf="dashboardFacade.loading()" class="dashboard-loading">
          <app-loading-skeleton
            *ngFor="let card of loadingCards"
            variant="card"
            [ariaLabel]="'Loading dashboard summary...' | t">
          </app-loading-skeleton>
        </div>

        <div *ngIf="dashboardFacade.error()" class="dashboard-error" role="alert">
          {{ dashboardFacade.error() | t }}
        </div>

        <app-dashboard-summary-cards
          *ngIf="!dashboardFacade.loading() && !dashboardFacade.error() && dashboardFacade.summary() as summary"
          [summary]="summary">
        </app-dashboard-summary-cards>

        <app-empty-state
          *ngIf="!dashboardFacade.loading() && !dashboardFacade.error() && !dashboardFacade.summary()"
          icon="D"
          [title]="'No dashboard summary is loaded yet.' | t"
          [description]="'Tenant-scoped summary of current clinic operations.' | t">
        </app-empty-state>
      </app-section-card>
    </section>
  `,
  styles: [`
    .dashboard-page {
      display: grid;
      gap: 1.25rem;
    }

    .dashboard-loading {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
      gap: 1rem;
    }

    .dashboard-error {
      padding: 1rem;
      border: 1px solid var(--bsm-color-danger-soft);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }
  `]
})
export class DashboardPageComponent implements OnInit {
  readonly dashboardFacade = inject(DashboardFacade);
  readonly loadingCards = [0, 1, 2, 3, 4, 5, 6];

  ngOnInit(): void {
    this.dashboardFacade.loadSummary();
  }
}
