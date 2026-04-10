import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PatientSummary } from '../models/patient.models';

@Component({
  selector: 'app-patient-search-results',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div *ngIf="error" class="state-card state-error">{{ error }}</div>
    <div *ngIf="loading" class="state-card">Loading patients...</div>

    <div *ngIf="!loading && !error && !patients.length" class="state-card">
      {{ emptyMessage }}
    </div>

    <div *ngIf="patients.length" class="results-grid">
      <article *ngFor="let patient of patients" class="result-card">
        <div class="card-head">
          <div>
            <h3>{{ patient.fullName }}</h3>
            <p>{{ patient.dateOfBirth | date: 'longDate' }}</p>
          </div>
          <div class="pill-stack">
            <span class="status-pill" [class.status-inactive]="!patient.isActive">
              {{ patient.isActive ? 'Active' : 'Inactive' }}
            </span>
            <span *ngIf="patient.hasClinicalAlerts" class="alert-pill">Alerts</span>
          </div>
        </div>

        <dl class="meta-grid">
          <div>
            <dt>Phone</dt>
            <dd>{{ patient.primaryPhone || 'Not provided' }}</dd>
          </div>
          <div>
            <dt>Email</dt>
            <dd>{{ patient.email || 'Not provided' }}</dd>
          </div>
        </dl>

        <div class="card-actions">
          <a [routerLink]="['/patients', patient.id]" class="action-link">View profile</a>
          <a [routerLink]="['/patients', patient.id, 'edit']" class="action-link action-secondary">Edit</a>
        </div>
      </article>
    </div>
  `,
  styles: [`
    .results-grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    }

    .result-card,
    .state-card {
      border-radius: 16px;
      border: 1px solid #d8e1ea;
      background: #ffffff;
      padding: 1rem 1.1rem;
      box-shadow: 0 18px 34px rgba(20, 49, 79, 0.08);
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
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
      color: #16324f;
    }

    .card-head p {
      margin: 0;
      color: #5b6d82;
    }

    .status-pill {
      border-radius: 999px;
      padding: 0.4rem 0.7rem;
      font-size: 0.85rem;
      font-weight: 700;
      background: #e8f4ec;
      color: #1d6a3a;
      white-space: nowrap;
    }

    .status-inactive {
      background: #fce8e8;
      color: #9b2d30;
    }

    .alert-pill {
      border-radius: 999px;
      padding: 0.4rem 0.7rem;
      font-size: 0.85rem;
      font-weight: 700;
      background: #fff1dd;
      color: #8f5a00;
      white-space: nowrap;
    }

    .meta-grid {
      margin: 1rem 0 0;
      display: grid;
      gap: 0.8rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #718298;
    }

    dd {
      margin: 0;
      color: #16324f;
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
      text-decoration: none;
      color: #0a5bb5;
      font-weight: 700;
    }

    .action-secondary {
      color: #4e6178;
    }

    @media (max-width: 640px) {
      .meta-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class PatientSearchResultsComponent {
  @Input() patients: PatientSummary[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() emptyMessage = 'No patients match the current search.';
}
