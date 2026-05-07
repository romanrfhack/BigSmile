import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PatientFormComponent } from '../components/patient-form.component';
import { PatientsFacade } from '../facades/patients.facade';
import { SavePatientRequest } from '../models/patient.models';
import { TranslatePipe } from '../../../shared/i18n';
import { LoadingSkeletonComponent, PageHeaderComponent, SectionCardComponent } from '../../../shared/ui';

@Component({
  selector: 'app-patient-form-page',
  standalone: true,
  imports: [
    CommonModule,
    LoadingSkeletonComponent,
    PageHeaderComponent,
    PatientFormComponent,
    RouterLink,
    SectionCardComponent,
    TranslatePipe
  ],
  template: `
    <section class="page-shell">
      <app-page-header
        [eyebrow]="('Release' | t) + ' 1 / ' + ('Patients' | t)"
        [title]="(isEditMode ? 'Edit patient' : 'Register patient') | t"
        [subtitle]="(isEditMode
          ? 'Tenant-scoped patient identity, status, basic clinical alerts, and responsible-party context.'
          : 'Capture the minimum patient record needed for daily clinic operations.') | t">
        <a page-header-actions routerLink="/patients" class="patient-form-action patient-form-action--secondary">
          {{ 'Back to search' | t }}
        </a>
        <button
          page-header-actions
          type="button"
          class="patient-form-action patient-form-action--secondary"
          [disabled]="saving"
          (click)="cancel()">
          {{ 'Cancel' | t }}
        </button>
        <button
          page-header-actions
          type="submit"
          form="patient-form"
          class="patient-form-action patient-form-action--primary"
          [disabled]="saving || (isEditMode && !patientsFacade.currentPatient())">
          {{ (saving ? 'Saving...' : isEditMode ? 'Save changes' : 'Create patient') | t }}
        </button>
      </app-page-header>

      <app-section-card
        *ngIf="isEditMode && patientsFacade.loadingPatient()"
        [title]="'Loading patient record...' | t"
        variant="elevated">
        <app-loading-skeleton variant="card" [ariaLabel]="'Loading patient record...' | t"></app-loading-skeleton>
      </app-section-card>

      <div *ngIf="isEditMode && patientsFacade.detailError()" class="state-error" role="alert">
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
      gap: 1rem;
    }

    .state-error {
      padding: 1rem;
      border: 1px solid var(--bsm-color-danger-soft);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
      box-shadow: var(--bsm-shadow-sm);
    }

    .patient-form-action {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 0;
      border: 1px solid transparent;
      border-radius: var(--bsm-radius-pill);
      padding: 0.72rem 1rem;
      font: inherit;
      font-weight: 800;
      line-height: 1.2;
      text-align: center;
      text-decoration: none;
      cursor: pointer;
      transition:
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        color var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .patient-form-action--primary {
      border-color: var(--bsm-color-primary);
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .patient-form-action--secondary {
      border-color: var(--bsm-color-primary-soft);
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .patient-form-action:not(:disabled):hover {
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .patient-form-action:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    .patient-form-action:disabled {
      opacity: 0.72;
      cursor: not-allowed;
    }

    @media (prefers-reduced-motion: reduce) {
      .patient-form-action:not(:disabled):hover {
        transform: none;
      }
    }

    @media (max-width: 768px) {
      .patient-form-action {
        width: 100%;
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
