import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../facades/patients.facade';

@Component({
  selector: 'app-patient-profile-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="profile-page">
      <header class="profile-head">
        <div>
          <p class="eyebrow">Release 1 / Patients</p>
          <h2>Patient profile</h2>
          <p class="subtitle">Tenant-scoped patient identity, status, basic clinical alerts, and responsible-party context.</p>
        </div>
        <div class="head-actions">
          <a routerLink="/patients" class="action-link action-secondary">Back to search</a>
          <a
            *ngIf="canReadClinicalRecords && patientsFacade.currentPatient() as patient"
            [routerLink]="['/patients', patient.id, 'clinical-record']"
            class="action-link action-secondary">
            Clinical record
          </a>
          <a
            *ngIf="canReadOdontogram && patientsFacade.currentPatient() as patient"
            [routerLink]="['/patients', patient.id, 'odontogram']"
            class="action-link action-secondary">
            Odontogram
          </a>
          <a
            *ngIf="canReadTreatmentPlans && patientsFacade.currentPatient() as patient"
            [routerLink]="['/patients', patient.id, 'treatment-plan']"
            class="action-link action-secondary">
            Treatment plan
          </a>
          <a
            *ngIf="canReadTreatmentQuotes && patientsFacade.currentPatient() as patient"
            [routerLink]="['/patients', patient.id, 'treatment-plan', 'quote']"
            class="action-link action-secondary">
            Quote
          </a>
          <a
            *ngIf="canReadBillingDocuments && patientsFacade.currentPatient() as patient"
            [routerLink]="['/patients', patient.id, 'treatment-plan', 'quote', 'billing']"
            class="action-link action-secondary">
            Billing
          </a>
          <a *ngIf="patientsFacade.currentPatient() as patient" [routerLink]="['/patients', patient.id, 'edit']" class="action-link">
            Edit patient
          </a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient()" class="state-card">Loading patient profile...</div>
      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">{{ patientsFacade.detailError() }}</div>

      <article *ngIf="patientsFacade.currentPatient() as patient" class="profile-card">
        <div class="identity-bar">
          <div>
            <h3>{{ patient.fullName }}</h3>
            <p>{{ patient.dateOfBirth | date: 'longDate' }}</p>
          </div>
          <div class="pill-stack">
            <span class="status-pill" [class.status-inactive]="!patient.isActive">
              {{ patient.isActive ? 'Active' : 'Inactive' }}
            </span>
            <span *ngIf="patient.hasClinicalAlerts" class="alert-pill">Clinical alerts</span>
          </div>
        </div>

        <section class="detail-grid">
          <div>
            <dt>Phone</dt>
            <dd>{{ patient.primaryPhone || 'Not provided' }}</dd>
          </div>
          <div>
            <dt>Email</dt>
            <dd>{{ patient.email || 'Not provided' }}</dd>
          </div>
          <div>
            <dt>Created</dt>
            <dd>{{ patient.createdAt | date: 'medium' }}</dd>
          </div>
          <div>
            <dt>Updated</dt>
            <dd>{{ patient.updatedAt ? (patient.updatedAt | date: 'medium') : 'No updates yet' }}</dd>
          </div>
        </section>

        <section class="clinical-alerts">
          <h4>Clinical alerts</h4>
          <div class="detail-grid">
            <div>
              <dt>Alerts on file</dt>
              <dd>{{ patient.hasClinicalAlerts ? 'Yes' : 'No' }}</dd>
            </div>
            <div>
              <dt>Summary</dt>
              <dd>
                {{ patient.hasClinicalAlerts
                  ? (patient.clinicalAlertsSummary || 'No summary provided')
                  : 'No clinical alerts registered for this patient.' }}
              </dd>
            </div>
          </div>
        </section>

        <section class="responsible-party">
          <h4>Responsible party</h4>
          <div *ngIf="patient.responsibleParty; else noResponsibleParty" class="detail-grid">
            <div>
              <dt>Name</dt>
              <dd>{{ patient.responsibleParty.name }}</dd>
            </div>
            <div>
              <dt>Relationship</dt>
              <dd>{{ patient.responsibleParty.relationship || 'Not provided' }}</dd>
            </div>
            <div>
              <dt>Phone</dt>
              <dd>{{ patient.responsibleParty.phone || 'Not provided' }}</dd>
            </div>
          </div>
          <ng-template #noResponsibleParty>
            <p class="muted">No responsible-party information is registered for this patient.</p>
          </ng-template>
        </section>
      </article>
    </section>
  `,
  styles: [`
    .profile-page {
      display: grid;
      gap: 1.25rem;
    }

    .profile-head,
    .profile-card,
    .state-card {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .profile-head {
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

    h2,
    h3,
    h4 {
      margin: 0;
      color: #16324f;
    }

    .subtitle,
    .identity-bar p,
    .muted {
      margin: 0.45rem 0 0;
      color: #5b6e84;
    }

    .head-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .pill-stack {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .action-link {
      text-decoration: none;
      color: #0a5bb5;
      font-weight: 700;
    }

    .action-secondary {
      color: #4d6278;
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }

    .identity-bar {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .status-pill {
      border-radius: 999px;
      padding: 0.5rem 0.75rem;
      background: #e8f4ec;
      color: #1d6a3a;
      font-weight: 700;
      white-space: nowrap;
    }

    .status-inactive {
      background: #fce8e8;
      color: #9b2d30;
    }

    .alert-pill {
      border-radius: 999px;
      padding: 0.5rem 0.75rem;
      background: #fff1dd;
      color: #8f5a00;
      font-weight: 700;
      white-space: nowrap;
    }

    .detail-grid {
      margin-top: 1.2rem;
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
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

    .clinical-alerts,
    .responsible-party {
      margin-top: 1.4rem;
    }

    @media (max-width: 768px) {
      .profile-head,
      .identity-bar {
        flex-direction: column;
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
