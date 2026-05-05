import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PatientFormComponent } from '../components/patient-form.component';
import { PatientsFacade } from '../facades/patients.facade';
import { SavePatientRequest } from '../models/patient.models';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-patient-form-page',
  standalone: true,
  imports: [CommonModule, RouterLink, PatientFormComponent, TranslatePipe],
  template: `
    <section class="page-shell">
      <header class="page-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 1 / {{ 'Patients' | t }}</p>
          <h2>{{ (isEditMode ? 'Edit patient' : 'Register patient') | t }}</h2>
          <p class="subtitle">
            {{ (isEditMode
              ? 'Update the patient profile without changing tenant ownership.'
              : 'Capture the minimum patient record needed for daily clinic operations.') | t }}
          </p>
        </div>
        <a routerLink="/patients" class="back-link">{{ 'Back to patient search' | t }}</a>
      </header>

      <div *ngIf="isEditMode && patientsFacade.loadingPatient()" class="state-card">
        {{ 'Loading patient record...' | t }}
      </div>

      <div *ngIf="isEditMode && patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() | t }}
      </div>

      <app-patient-form
        *ngIf="!isEditMode || patientsFacade.currentPatient()"
        [initialPatient]="patientsFacade.currentPatient()"
        [mode]="isEditMode ? 'edit' : 'create'"
        [saving]="saving"
        [error]="submitError"
        (saved)="save($event)"
        (cancelled)="cancel()">
      </app-patient-form>
    </section>
  `,
  styles: [`
    .page-shell {
      display: grid;
      gap: 1.25rem;
    }

    .page-head,
    .state-card {
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
    }

    .subtitle {
      margin: 0.5rem 0 0;
      color: #5b6e84;
      max-width: 54ch;
    }

    .back-link {
      color: #0a5bb5;
      text-decoration: none;
      font-weight: 700;
      white-space: nowrap;
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }

    @media (max-width: 768px) {
      .page-head {
        flex-direction: column;
      }
    }
  `]
})
export class PatientFormPageComponent implements OnInit {
  readonly patientsFacade = inject(PatientsFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  isEditMode = false;
  patientId: string | null = null;
  saving = false;
  submitError: string | null = null;

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.patientId;
    this.submitError = null;

    if (this.patientId) {
      this.patientsFacade.clearCurrentPatient();
      this.patientsFacade.loadPatient(this.patientId);
      return;
    }

    this.patientsFacade.clearCurrentPatient();
  }

  save(payload: SavePatientRequest): void {
    this.saving = true;
    this.submitError = null;

    const request$ = this.patientId
      ? this.patientsFacade.updatePatient(this.patientId, payload)
      : this.patientsFacade.createPatient(payload);

    request$.subscribe({
      next: (patient) => {
        this.saving = false;
        this.router.navigate(['/patients', patient.id]);
      },
      error: (error) => {
        this.saving = false;
        this.submitError = error.error?.errors?.SavePatientRequest?.[0]
          ?? error.error?.title
          ?? 'Patient could not be saved.';
      }
    });
  }

  cancel(): void {
    if (this.patientId) {
      this.router.navigate(['/patients', this.patientId]);
      return;
    }

    this.router.navigate(['/patients']);
  }
}
