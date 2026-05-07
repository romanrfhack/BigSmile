import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  StatusBadgeComponent
} from '../../../shared/ui';
import { PatientSummary } from '../models/patient.models';

@Component({
  selector: 'app-patient-search-results',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    LocalizedDatePipe,
    StatusBadgeComponent,
    TranslatePipe
  ],
  template: `
    <div *ngIf="error" class="patients-error" role="alert">{{ error | t }}</div>

    <div *ngIf="loading" class="patients-loading" [attr.aria-label]="'Loading patients...' | t">
      <app-loading-skeleton
        *ngFor="let item of loadingCards"
        variant="card"
        [ariaLabel]="'Loading patients...' | t">
      </app-loading-skeleton>
    </div>

    <app-empty-state
      *ngIf="!loading && !error && !patients.length"
      icon="P"
      [title]="emptyMessage"
      [description]="'Name, phone, or email' | t">
    </app-empty-state>

    <div *ngIf="patients.length" class="results-grid" [attr.aria-label]="'Patients' | t">
      <article
        *ngFor="let patient of patients"
        class="result-card"
        [attr.aria-labelledby]="'patient-result-' + patient.id">
        <div class="card-head">
          <div>
            <h3 [id]="'patient-result-' + patient.id">{{ patient.fullName }}</h3>
            <p>{{ patient.dateOfBirth | bsDate: 'longDate' }}</p>
          </div>
          <div class="pill-stack">
            <app-status-badge
              [tone]="patient.isActive ? 'success' : 'neutral'"
              [label]="(patient.isActive ? 'Active' : 'Inactive') | t">
            </app-status-badge>
            <app-status-badge
              *ngIf="patient.hasClinicalAlerts"
              tone="warning"
              [label]="'Alerts' | t">
            </app-status-badge>
          </div>
        </div>

        <dl class="meta-grid">
          <div>
            <dt>{{ 'Phone' | t }}</dt>
            <dd>{{ patient.primaryPhone || ('Not provided' | t) }}</dd>
          </div>
          <div>
            <dt>{{ 'Email' | t }}</dt>
            <dd>{{ patient.email || ('Not provided' | t) }}</dd>
          </div>
        </dl>

        <div class="card-actions">
          <a [routerLink]="['/patients', patient.id]" class="action-link">{{ 'View profile' | t }}</a>
          <a [routerLink]="['/patients', patient.id, 'edit']" class="action-link action-secondary">{{ 'Edit' | t }}</a>
        </div>
      </article>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    .patients-loading,
    .results-grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    }

    .result-card,
    .patients-error {
      border-radius: var(--bsm-radius-sm);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
      padding: 1rem 1.1rem;
      box-shadow: var(--bsm-shadow-sm);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .result-card:hover {
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .result-card:focus-within {
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .patients-error {
      border-color: var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .card-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .pill-stack {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      justify-content: flex-end;
    }

    h3 {
      margin: 0 0 0.25rem;
      color: var(--bsm-color-text-brand);
    }

    .card-head p {
      margin: 0;
      color: var(--bsm-color-text-muted);
    }

    .meta-grid {
      margin: 1rem 0 0;
      display: grid;
      gap: 0.8rem;
      grid-template-columns: minmax(7.5rem, 0.8fr) minmax(10rem, 1.2fr);
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0;
      color: var(--bsm-color-text-muted);
    }

    dd {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
      word-break: break-word;
    }

    .card-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
      margin-top: 1rem;
    }

    .action-link {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      text-decoration: none;
      border: 1px solid var(--bsm-color-accent-soft);
      border-radius: var(--bsm-radius-pill);
      padding: 0.55rem 0.8rem;
      background: var(--bsm-color-accent-soft);
      color: var(--bsm-color-accent-accessible);
      font-weight: 700;
      line-height: 1.2;
    }

    .action-secondary {
      border-color: var(--bsm-color-primary-soft);
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary);
    }

    .action-link:hover {
      box-shadow: var(--bsm-shadow-sm);
    }

    .action-link:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    @media (prefers-reduced-motion: reduce) {
      .result-card:hover {
        transform: none;
      }
    }

    @media (max-width: 640px) {
      .card-head {
        flex-direction: column;
      }

      .pill-stack {
        justify-content: flex-start;
      }

      .meta-grid {
        grid-template-columns: 1fr;
      }

      .card-actions,
      .action-link {
        width: 100%;
      }
    }
  `]
})
export class PatientSearchResultsComponent {
  @Input() patients: PatientSummary[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() emptyMessage = 'No patients match the current search.';

  readonly loadingCards = [0, 1, 2, 3];
}
