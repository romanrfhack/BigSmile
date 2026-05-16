import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { TranslatePipe } from '../../../shared/i18n';
import { PageHeaderComponent, SectionCardComponent } from '../../../shared/ui';
import { PatientSearchResultsComponent } from '../components/patient-search-results.component';
import { PatientsFacade } from '../facades/patients.facade';

@Component({
  selector: 'app-patient-list-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PageHeaderComponent,
    PatientSearchResultsComponent,
    RouterLink,
    SectionCardComponent,
    TranslatePipe
  ],
  template: `
    <section class="patients-page">
      <app-page-header
        [eyebrow]="'Patients' | t"
        [title]="'Patient search' | t"
        [subtitle]="'Search and maintain patient records for {tenantName} without leaving tenant scope.' | t:{ tenantName }">
        <a page-header-actions routerLink="/patients/new" class="patients-action patients-action--primary">
          {{ 'Register patient' | t }}
        </a>
      </app-page-header>

      <app-section-card
        [title]="'Search patients' | t"
        [subtitle]="'Name, phone, or email' | t"
        variant="elevated">
        <form class="search-form" role="search" (ngSubmit)="applySearch()">
          <label class="search-field">
            <span>{{ 'Search patients' | t }}</span>
            <input
              type="search"
              name="searchTerm"
              [(ngModel)]="searchTerm"
              [placeholder]="'Name, phone, or email' | t"
            />
          </label>

          <label class="toggle-field">
            <input type="checkbox" name="includeInactive" [(ngModel)]="includeInactive" />
            <span>{{ 'Include inactive patients' | t }}</span>
          </label>

          <button type="submit" class="patients-action patients-action--secondary">{{ 'Run search' | t }}</button>
        </form>
      </app-section-card>

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
      gap: 1rem;
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
      color: var(--bsm-color-text-brand);
      font-weight: 600;
    }

    .search-field span,
    .toggle-field span {
      line-height: 1.3;
    }

    .toggle-field {
      display: flex;
      align-items: center;
      gap: 0.65rem;
      align-self: center;
      margin-top: 1.55rem;
    }

    .toggle-field input {
      width: 1.05rem;
      height: 1.05rem;
      accent-color: var(--bsm-color-primary);
    }

    input[type='search'] {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.9rem 1rem;
      font: inherit;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    input[type='search']:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .patients-action {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      text-decoration: none;
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
      line-height: 1.2;
      transition:
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        color var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .patients-action--primary {
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .patients-action--secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .patients-action:not(:disabled):hover {
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .patients-action:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    @media (prefers-reduced-motion: reduce) {
      .patients-action:not(:disabled):hover {
        transform: none;
      }
    }

    @media (max-width: 900px) {
      .search-form {
        grid-template-columns: 1fr;
      }

      .patients-action {
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
  private readonly i18n = inject(I18nService);

  searchTerm = '';
  includeInactive = false;
  tenantName = 'the current tenant';

  get emptyMessage(): string {
    return this.searchTerm.trim()
      ? this.i18n.translate('No patients match the current search.')
      : this.i18n.translate('No patients are registered for the current tenant yet.');
  }

  ngOnInit(): void {
    this.tenantName = this.authService.getCurrentTenant()?.name ?? this.i18n.translate('the current tenant');
    this.patientsFacade.search();
  }

  applySearch(): void {
    this.patientsFacade.search(this.searchTerm, this.includeInactive);
  }
}
