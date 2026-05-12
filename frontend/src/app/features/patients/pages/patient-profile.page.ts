import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  LoadingSkeletonComponent,
  PageHeaderComponent,
  SectionCardComponent,
  StatusBadgeComponent
} from '../../../shared/ui';
import { PatientsFacade } from '../facades/patients.facade';

@Component({
  selector: 'app-patient-profile-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    LoadingSkeletonComponent,
    LocalizedDatePipe,
    PageHeaderComponent,
    SectionCardComponent,
    StatusBadgeComponent,
    TranslatePipe
  ],
  template: `
    <section class="profile-page">
      <ng-container *ngIf="patientsFacade.currentPatient() as patient; else patientProfileState">
        <app-page-header
          [eyebrow]="('Release' | t) + ' 1 / ' + ('Patients' | t)"
          [title]="patient.fullName"
          [subtitle]="'Tenant-scoped patient identity, status, basic clinical alerts, and responsible-party context.' | t">
          <a page-header-actions routerLink="/patients" class="profile-action profile-action--secondary">
            {{ 'Back to search' | t }}
          </a>
          <a page-header-actions [routerLink]="['/patients', patient.id, 'edit']" class="profile-action profile-action--primary">
            {{ 'Edit patient' | t }}
          </a>
        </app-page-header>

        <section class="summary-strip" [attr.aria-label]="'Patient profile' | t">
          <article class="summary-item">
            <span class="summary-label">{{ 'Date of birth' | t }}</span>
            <strong>{{ patient.dateOfBirth | bsDate: 'longDate' }}</strong>
          </article>
          <article class="summary-item">
            <span class="summary-label">{{ 'Current status' | t }}</span>
            <app-status-badge
              [tone]="patient.isActive ? 'success' : 'neutral'"
              [label]="(patient.isActive ? 'Active' : 'Inactive') | t">
            </app-status-badge>
          </article>
          <article class="summary-item">
            <span class="summary-label">{{ 'Clinical alerts' | t }}</span>
            <app-status-badge
              [tone]="patient.hasClinicalAlerts ? 'warning' : 'neutral'"
              [label]="(patient.hasClinicalAlerts ? 'Clinical alerts' : 'No') | t">
            </app-status-badge>
          </article>
          <article class="summary-item">
            <span class="summary-label">{{ 'Contact' | t }}</span>
            <strong>{{ patient.primaryPhone || patient.email || ('No contact data' | t) }}</strong>
          </article>
        </section>

        <nav
          *ngIf="canReadClinicalRecords || canReadOdontogram || canReadTreatmentPlans || canReadTreatmentQuotes || canReadBillingDocuments || canReadDocuments"
          class="context-nav"
          [attr.aria-label]="'Patient profile' | t">
          <a
            *ngIf="canReadClinicalRecords"
            [routerLink]="['/patients', patient.id, 'clinical-record']"
            class="context-link">
            {{ 'Clinical record' | t }}
          </a>
          <a
            *ngIf="canReadOdontogram"
            [routerLink]="['/patients', patient.id, 'odontogram']"
            class="context-link">
            {{ 'Odontogram' | t }}
          </a>
          <a
            *ngIf="canReadTreatmentPlans"
            [routerLink]="['/patients', patient.id, 'treatment-plan']"
            class="context-link">
            {{ 'Treatment plan' | t }}
          </a>
          <a
            *ngIf="canReadTreatmentQuotes"
            [routerLink]="['/patients', patient.id, 'treatment-plan', 'quote']"
            class="context-link">
            {{ 'Quote' | t }}
          </a>
          <a
            *ngIf="canReadBillingDocuments"
            [routerLink]="['/patients', patient.id, 'treatment-plan', 'quote', 'billing']"
            class="context-link">
            {{ 'Billing' | t }}
          </a>
          <a
            *ngIf="canReadDocuments"
            [routerLink]="['/patients', patient.id, 'documents']"
            class="context-link">
            {{ 'Documents' | t }}
          </a>
        </nav>

        <div class="profile-grid">
          <app-section-card [title]="'Contact' | t" variant="elevated">
            <dl class="detail-grid detail-grid--contact">
              <div>
                <dt>{{ 'Phone' | t }}</dt>
                <dd>{{ patient.primaryPhone || ('Not provided' | t) }}</dd>
              </div>
              <div>
                <dt>{{ 'Email' | t }}</dt>
                <dd>{{ patient.email || ('Not provided' | t) }}</dd>
              </div>
              <div>
                <dt>{{ 'Date of birth' | t }}</dt>
                <dd>{{ patient.dateOfBirth | bsDate: 'longDate' }}</dd>
              </div>
            </dl>
          </app-section-card>

          <app-section-card [title]="'Demographics' | t" variant="default">
            <dl class="detail-grid">
              <div>
                <dt>{{ 'Sex' | t }}</dt>
                <dd>{{ (patient.sex || 'Unspecified') | t }}</dd>
              </div>
              <div>
                <dt>{{ 'Occupation' | t }}</dt>
                <dd>{{ patient.occupation || ('Not provided' | t) }}</dd>
              </div>
              <div>
                <dt>{{ 'Marital status' | t }}</dt>
                <dd>{{ (patient.maritalStatus || 'Unspecified') | t }}</dd>
              </div>
              <div>
                <dt>{{ 'Referred by' | t }}</dt>
                <dd>{{ patient.referredBy || ('Not provided' | t) }}</dd>
              </div>
            </dl>
          </app-section-card>

          <app-section-card [title]="'Clinical alerts' | t" [variant]="patient.hasClinicalAlerts ? 'accent' : 'compact'">
            <div class="section-status">
              <app-status-badge
                [tone]="patient.hasClinicalAlerts ? 'warning' : 'neutral'"
                [label]="(patient.hasClinicalAlerts ? 'Clinical alerts' : 'No') | t">
              </app-status-badge>
            </div>
            <dl class="detail-grid detail-grid--single">
              <div>
                <dt>{{ 'Alerts on file' | t }}</dt>
                <dd>{{ (patient.hasClinicalAlerts ? 'Yes' : 'No') | t }}</dd>
              </div>
              <div>
                <dt>{{ 'Summary' | t }}</dt>
                <dd>
                  {{ patient.hasClinicalAlerts
                    ? (patient.clinicalAlertsSummary || ('No summary provided' | t))
                    : ('No clinical alerts registered for this patient.' | t) }}
                </dd>
              </div>
            </dl>
          </app-section-card>

          <app-section-card [title]="'Responsible party' | t" variant="default">
            <dl *ngIf="patient.responsibleParty; else noResponsibleParty" class="detail-grid">
              <div>
                <dt>{{ 'Name' | t }}</dt>
                <dd>{{ patient.responsibleParty.name }}</dd>
              </div>
              <div>
                <dt>{{ 'Relationship' | t }}</dt>
                <dd>{{ patient.responsibleParty.relationship || ('Not provided' | t) }}</dd>
              </div>
              <div>
                <dt>{{ 'Phone' | t }}</dt>
                <dd>{{ patient.responsibleParty.phone || ('Not provided' | t) }}</dd>
              </div>
            </dl>
            <ng-template #noResponsibleParty>
              <p class="muted">{{ 'No responsible-party information is registered for this patient.' | t }}</p>
            </ng-template>
          </app-section-card>

          <app-section-card [title]="'Registered' | t" variant="compact">
            <dl class="detail-grid">
              <div>
                <dt>{{ 'Created' | t }}</dt>
                <dd>{{ patient.createdAt | bsDate: 'medium' }}</dd>
              </div>
              <div>
                <dt>{{ 'Updated' | t }}</dt>
                <dd>{{ patient.updatedAt ? (patient.updatedAt | bsDate: 'medium') : ('No updates yet' | t) }}</dd>
              </div>
            </dl>
          </app-section-card>
        </div>
      </ng-container>

      <ng-template #patientProfileState>
        <app-page-header
          [eyebrow]="('Release' | t) + ' 1 / ' + ('Patients' | t)"
          [title]="'Patient profile' | t"
          [subtitle]="'Tenant-scoped patient identity, status, basic clinical alerts, and responsible-party context.' | t">
          <a page-header-actions routerLink="/patients" class="profile-action profile-action--secondary">
            {{ 'Back to search' | t }}
          </a>
        </app-page-header>

        <app-section-card
          *ngIf="patientsFacade.loadingPatient()"
          [title]="'Loading patient profile...' | t"
          variant="elevated">
          <div class="profile-loading">
            <app-loading-skeleton
              *ngFor="let item of loadingCards"
              variant="card"
              [ariaLabel]="'Loading patient profile...' | t">
            </app-loading-skeleton>
          </div>
        </app-section-card>

        <div *ngIf="patientsFacade.detailError()" class="profile-error" role="alert">
          {{ patientsFacade.detailError() | t }}
        </div>
      </ng-template>
    </section>
  `,
  styles: [`
    .profile-page {
      display: grid;
      gap: 1rem;
    }

    .summary-strip,
    .context-nav,
    .profile-loading {
      display: grid;
      gap: 0.75rem;
    }

    .summary-strip {
      grid-template-columns: repeat(4, minmax(0, 1fr));
    }

    .summary-item {
      display: grid;
      align-content: start;
      gap: 0.45rem;
      min-width: 0;
      padding: 0.9rem 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
      box-shadow: var(--bsm-shadow-sm);
    }

    .summary-label {
      color: var(--bsm-color-text-muted);
      font-size: 0.75rem;
      font-weight: 800;
      line-height: 1.2;
      text-transform: uppercase;
    }

    .summary-item strong {
      min-width: 0;
      color: var(--bsm-color-text-brand);
      font-size: 0.98rem;
      line-height: 1.25;
      overflow-wrap: anywhere;
    }

    .context-nav {
      grid-template-columns: repeat(auto-fit, minmax(9.5rem, 1fr));
      padding: 0.75rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-sm);
    }

    .profile-grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: minmax(0, 1.2fr) minmax(18rem, 0.8fr);
      align-items: start;
    }

    .profile-grid app-section-card:nth-child(3),
    .profile-grid app-section-card:nth-child(4) {
      grid-column: span 1;
    }

    .section-status {
      display: flex;
      justify-content: flex-start;
      margin-bottom: 0.85rem;
    }

    .detail-grid {
      display: grid;
      gap: 0.9rem;
      grid-template-columns: repeat(auto-fit, minmax(11rem, 1fr));
      margin: 0;
    }

    .detail-grid--single {
      grid-template-columns: 1fr;
    }

    .detail-grid--contact {
      grid-template-columns: minmax(7rem, 0.8fr) minmax(14rem, 1.35fr) minmax(9rem, 1fr);
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

    .muted {
      margin: 0;
      color: var(--bsm-color-text-muted);
      line-height: 1.45;
    }

    .profile-action,
    .context-link {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 0;
      border-radius: var(--bsm-radius-pill);
      text-decoration: none;
      font-weight: 800;
      line-height: 1.2;
      text-align: center;
      transition:
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        color var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .profile-action {
      padding: 0.72rem 1rem;
      border: 1px solid var(--bsm-color-primary-soft);
    }

    .profile-action--primary {
      border-color: var(--bsm-color-primary);
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .profile-action--secondary,
    .context-link {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .context-link {
      padding: 0.65rem 0.8rem;
      border: 1px solid var(--bsm-color-primary-soft);
    }

    .profile-action:not(:disabled):hover,
    .context-link:hover {
      box-shadow: var(--bsm-shadow-sm);
      transform: translateY(-1px);
    }

    .profile-action:focus-visible,
    .context-link:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    .profile-error {
      border: 1px solid var(--bsm-color-danger-soft);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      padding: 1rem;
      font-weight: 700;
      box-shadow: var(--bsm-shadow-sm);
    }

    @media (prefers-reduced-motion: reduce) {
      .profile-action:not(:disabled):hover,
      .context-link:hover {
        transform: none;
      }
    }

    @media (max-width: 768px) {
      .summary-strip,
      .profile-grid {
        grid-template-columns: 1fr;
      }

      .detail-grid--contact {
        grid-template-columns: 1fr;
      }

      .profile-action,
      .context-link {
        width: 100%;
      }
    }
  `]
})
export class PatientProfilePageComponent implements OnInit {
  private readonly authService = inject(AuthService);

  readonly patientsFacade = inject(PatientsFacade);
  readonly canReadClinicalRecords = this.authService.hasPermissions(['clinical.read']);
  readonly canReadOdontogram = this.authService.hasPermissions(['odontogram.read']);
  readonly canReadTreatmentPlans = this.authService.hasPermissions(['treatmentplan.read']);
  readonly canReadTreatmentQuotes = this.authService.hasPermissions(['treatmentquote.read']);
  readonly canReadBillingDocuments = this.authService.hasPermissions(['billing.read']);
  readonly canReadDocuments = this.authService.hasPermissions(['document.read']);
  readonly loadingCards = [0, 1, 2];
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    const patientId = this.route.snapshot.paramMap.get('id');
    if (!patientId) {
      this.patientsFacade.clearCurrentPatient();
      return;
    }

    this.patientsFacade.clearCurrentPatient();
    this.patientsFacade.loadPatient(patientId);
  }
}
