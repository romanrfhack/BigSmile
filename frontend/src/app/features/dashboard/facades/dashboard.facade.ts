import { Injectable, inject, signal } from '@angular/core';
import { DashboardApiService } from '../data-access/dashboard-api.service';
import { DashboardSummary } from '../models/dashboard-summary.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardFacade {
  private readonly dashboardApi = inject(DashboardApiService);

  readonly summary = signal<DashboardSummary | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  loadSummary(): void {
    this.loading.set(true);
    this.error.set(null);

    this.dashboardApi.getSummary().subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.loading.set(false);
        this.error.set(null);
      },
      error: () => {
        this.summary.set(null);
        this.loading.set(false);
        this.error.set('The dashboard summary could not be loaded.');
      }
    });
  }
}
