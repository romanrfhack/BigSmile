import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientSearchResultsComponent } from '../components/patient-search-results.component';
import { PatientsFacade } from '../facades/patients.facade';

@Component({
  selector: 'app-patient-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PatientSearchResultsComponent],
  template: `
    <section class="patients-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">Release 1 / Patients</p>
          <h2>Patient search</h2>
          <p class="subtitle">
            Search and maintain patient records for {{ tenantName }} without leaving tenant scope.
          </p>
        </div>
        <a routerLink="/patients/new" class="btn btn-primary">Register patient</a>
      </header>

      <section class="search-panel">
        <form class="search-form" (ngSubmit)="applySearch()">
          <label class="search-field">
            <span>Search patients</span>
            <input
              type="search"
              name="searchTerm"
              [(ngModel)]="searchTerm"
              placeholder="Name, phone, or email"
            />
          </label>

          <label class="toggle-field">
            <input type="checkbox" name="includeInactive" [(ngModel)]="includeInactive" />
            <span>Include inactive patients</span>
          </label>

          <button type="submit" class="btn btn-secondary">Run search</button>
        </form>
      </section>

      <app-patient-search-results
        [patients]="patientsFacade.patients()"
        [loading]="patientsFacade.loadingPatients()"
        [error]="patientsFacade.listError()"
        [emptyMessage]="emptyMessage">
      </app-patient-search-results>
    </section>
  `,
  styles: [`
    .patients-page {
      display: grid;
      gap: 1.5rem;
    }

    .page-head,
    .search-panel {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .page-head {
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
      font-size: clamp(1.8rem, 2vw, 2.4rem);
    }

    .subtitle {
      margin: 0.5rem 0 0;
      color: #5b6e84;
      max-width: 60ch;
    }

    .search-form {
      display: grid;
      gap: 1rem;
      grid-template-columns: minmax(0, 1fr) auto auto;
      align-items: end;
    }

    .search-field,
    .toggle-field {
      display: grid;
      gap: 0.45rem;
      color: #16324f;
      font-weight: 600;
    }

    .toggle-field {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      align-self: center;
      margin-top: 1.55rem;
    }

    input[type='search'] {
      border: 1px solid #c8d4df;
      border-radius: 14px;
      padding: 0.9rem 1rem;
      font: inherit;
      background: #ffffff;
    }

    .btn {
      text-decoration: none;
      border: none;
      border-radius: 999px;
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: #0a5bb5;
      color: #ffffff;
    }

    .btn-secondary {
      background: #e5edf5;
      color: #17304d;
    }

    @media (max-width: 900px) {
      .page-head,
      .search-form {
        grid-template-columns: 1fr;
      }

      .page-head {
        flex-direction: column;
      }

      .btn {
        width: 100%;
        text-align: center;
      }

      .toggle-field {
        margin-top: 0;
      }
    }
  `]
})
export class PatientListPageComponent implements OnInit {
  readonly patientsFacade = inject(PatientsFacade);
  private readonly authService = inject(AuthService);

  searchTerm = '';
  includeInactive = false;
  tenantName = 'the current tenant';

  get emptyMessage(): string {
    return this.searchTerm.trim()
      ? 'No patients match the current search.'
      : 'No patients are registered for the current tenant yet.';
  }

  ngOnInit(): void {
    this.tenantName = this.authService.getCurrentTenant()?.name ?? 'the current tenant';
    this.patientsFacade.search();
  }

  applySearch(): void {
    this.patientsFacade.search(this.searchTerm, this.includeInactive);
  }
}
