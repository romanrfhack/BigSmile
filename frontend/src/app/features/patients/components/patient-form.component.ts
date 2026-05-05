import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientDetail, SavePatientRequest } from '../models/patient.models';

function getTodayIsoDate(): string {
  const today = new Date();
  const year = today.getFullYear();
  const month = `${today.getMonth() + 1}`.padStart(2, '0');
  const day = `${today.getDate()}`.padStart(2, '0');

  return `${year}-${month}-${day}`;
}

function dateOfBirthValidator(control: AbstractControl): ValidationErrors | null {
  const dateOfBirth = `${control.value ?? ''}`.trim();
  if (!dateOfBirth) {
    return null;
  }

  return dateOfBirth > getTodayIsoDate()
    ? { futureDateOfBirth: true }
    : null;
}

function responsiblePartyValidator(control: AbstractControl): ValidationErrors | null {
  const relationship = `${control.get('responsiblePartyRelationship')?.value ?? ''}`.trim();
  const phone = `${control.get('responsiblePartyPhone')?.value ?? ''}`.trim();
  const name = `${control.get('responsiblePartyName')?.value ?? ''}`.trim();

  if ((relationship || phone) && !name) {
    return { responsiblePartyNameRequired: true };
  }

  return null;
}

@Component({
  selector: 'app-patient-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="patient-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="section-head">
        <div>
          <h2>{{ (mode === 'edit' ? 'Update patient' : 'Register patient') | t }}</h2>
          <p>{{ 'Keep the workflow short: core identity first, then optional responsible-party data.' | t }}</p>
        </div>
        <span class="status-indicator">
          {{ (form.controls.isActive.value ? 'Active record' : 'Inactive record') | t }}
        </span>
      </div>

      <section class="form-section">
        <h3>{{ 'Patient identity' | t }}</h3>
        <div class="field-grid">
          <label>
            <span>{{ 'First name' | t }}</span>
            <input type="text" formControlName="firstName" />
            <small *ngIf="showError('firstName', 'required')">{{ 'First name is required.' | t }}</small>
          </label>

          <label>
            <span>{{ 'Last name' | t }}</span>
            <input type="text" formControlName="lastName" />
            <small *ngIf="showError('lastName', 'required')">{{ 'Last name is required.' | t }}</small>
          </label>

          <label>
            <span>{{ 'Date of birth' | t }}</span>
            <input type="date" formControlName="dateOfBirth" />
            <small *ngIf="showError('dateOfBirth', 'required')">{{ 'Date of birth is required.' | t }}</small>
            <small *ngIf="showError('dateOfBirth', 'futureDateOfBirth')">{{ 'Date of birth cannot be in the future.' | t }}</small>
          </label>

          <label class="checkbox-field">
            <input type="checkbox" formControlName="isActive" />
            <span>{{ 'Patient is active' | t }}</span>
          </label>
        </div>
      </section>

      <section class="form-section">
        <h3>{{ 'Contact' | t }}</h3>
        <div class="field-grid">
          <label>
            <span>{{ 'Primary phone' | t }}</span>
            <input type="text" formControlName="primaryPhone" />
          </label>

          <label>
            <span>{{ 'Email' | t }}</span>
            <input type="email" formControlName="email" />
            <small *ngIf="showError('email', 'email')">{{ 'Enter a valid email address.' | t }}</small>
          </label>
        </div>
      </section>

      <section class="form-section">
        <h3>{{ 'Clinical alerts' | t }}</h3>
        <div class="field-grid">
          <label class="checkbox-field">
            <input type="checkbox" formControlName="hasClinicalAlerts" />
            <span>{{ 'Patient has basic clinical alerts' | t }}</span>
          </label>

          <label class="field-wide">
            <span>{{ 'Alert summary' | t }}</span>
            <textarea
              rows="3"
              formControlName="clinicalAlertsSummary"
              [placeholder]="'Short alert for immediate patient handling' | t"
            ></textarea>
            <small *ngIf="showError('clinicalAlertsSummary', 'maxlength')">
              {{ 'Alert summary must be 500 characters or fewer.' | t }}
            </small>
          </label>
        </div>
      </section>

      <section class="form-section">
        <h3>{{ 'Responsible party' | t }}</h3>
        <div class="field-grid">
          <label>
            <span>{{ 'Name' | t }}</span>
            <input type="text" formControlName="responsiblePartyName" />
          </label>

          <label>
            <span>{{ 'Relationship' | t }}</span>
            <input type="text" formControlName="responsiblePartyRelationship" />
          </label>

          <label>
            <span>{{ 'Phone' | t }}</span>
            <input type="text" formControlName="responsiblePartyPhone" />
          </label>
        </div>
        <small *ngIf="form.errors?.['responsiblePartyNameRequired']">
          {{ 'Responsible party name is required when relationship or phone is provided.' | t }}
        </small>
      </section>

      <div *ngIf="error" class="error-banner">{{ error | t }}</div>

      <div class="form-actions">
        <button type="button" class="btn btn-secondary" (click)="cancelled.emit()" [disabled]="saving">{{ 'Cancel' | t }}</button>
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ (saving ? 'Saving...' : mode === 'edit' ? 'Save changes' : 'Create patient') | t }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .patient-form {
      display: grid;
      gap: 1.25rem;
      padding: 1.5rem;
      border-radius: var(--bsm-radius-lg);
      background: var(--bsm-gradient-surface);
      border: 1px solid var(--bsm-color-border);
      box-shadow: var(--bsm-shadow-md);
    }

    .section-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .section-head h2,
    .form-section h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    .section-head p {
      margin: 0.35rem 0 0;
      color: var(--bsm-color-text-muted);
    }

    .status-indicator {
      border-radius: var(--bsm-radius-pill);
      padding: 0.5rem 0.8rem;
      background: var(--bsm-color-accent-soft);
      color: var(--bsm-color-accent-dark);
      font-weight: 700;
      white-space: nowrap;
    }

    .form-section {
      display: grid;
      gap: 0.85rem;
    }

    .field-grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    }

    label {
      display: grid;
      gap: 0.45rem;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
    }

    input,
    textarea {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: var(--bsm-color-text);
      background: var(--bsm-color-bg);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    textarea {
      resize: vertical;
      min-height: 96px;
    }

    input:focus,
    textarea:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    textarea:disabled {
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-muted);
    }

    .checkbox-field {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      font-weight: 600;
      min-height: 100%;
    }

    .field-wide {
      grid-column: 1 / -1;
    }

    .checkbox-field input {
      width: 18px;
      height: 18px;
      padding: 0;
    }

    small {
      color: #9b2d30;
      font-weight: 600;
    }

    .error-banner {
      padding: 0.85rem 1rem;
      border-radius: 12px;
      border: 1px solid #f2c4c4;
      background: #fff4f4;
      color: #8d292d;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .btn {
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.8rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: #ffffff;
    }

    .btn-secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .btn:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .section-head {
        flex-direction: column;
      }

      .form-actions .btn {
        width: 100%;
      }
    }
  `]
})
export class PatientFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  @Input() initialPatient: PatientDetail | null = null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() saved = new EventEmitter<SavePatientRequest>();
  @Output() cancelled = new EventEmitter<void>();

  readonly form = this.formBuilder.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    dateOfBirth: ['', [Validators.required, dateOfBirthValidator]],
    primaryPhone: ['', [Validators.maxLength(40)]],
    email: ['', [Validators.email, Validators.maxLength(256)]],
    isActive: [true, [Validators.required]],
    hasClinicalAlerts: [false, [Validators.required]],
    clinicalAlertsSummary: [{ value: '', disabled: true }, [Validators.maxLength(500)]],
    responsiblePartyName: ['', [Validators.maxLength(100)]],
    responsiblePartyRelationship: ['', [Validators.maxLength(100)]],
    responsiblePartyPhone: ['', [Validators.maxLength(40)]]
  }, { validators: responsiblePartyValidator });

  constructor() {
    this.form.controls.hasClinicalAlerts.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((hasClinicalAlerts) => {
        const summaryControl = this.form.controls.clinicalAlertsSummary;
        if (hasClinicalAlerts) {
          summaryControl.enable({ emitEvent: false });
          return;
        }

        summaryControl.reset('', { emitEvent: false });
        summaryControl.disable({ emitEvent: false });
      });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['initialPatient']) {
      return;
    }

    if (!this.initialPatient) {
      this.form.reset({
        firstName: '',
        lastName: '',
        dateOfBirth: '',
        primaryPhone: '',
        email: '',
        isActive: true,
        hasClinicalAlerts: false,
        clinicalAlertsSummary: '',
        responsiblePartyName: '',
        responsiblePartyRelationship: '',
        responsiblePartyPhone: ''
      });
      this.form.controls.clinicalAlertsSummary.disable({ emitEvent: false });
      return;
    }

    this.form.reset({
      firstName: this.initialPatient.firstName,
      lastName: this.initialPatient.lastName,
      dateOfBirth: this.initialPatient.dateOfBirth,
      primaryPhone: this.initialPatient.primaryPhone ?? '',
      email: this.initialPatient.email ?? '',
      isActive: this.initialPatient.isActive,
      hasClinicalAlerts: this.initialPatient.hasClinicalAlerts,
      clinicalAlertsSummary: this.initialPatient.clinicalAlertsSummary ?? '',
      responsiblePartyName: this.initialPatient.responsibleParty?.name ?? '',
      responsiblePartyRelationship: this.initialPatient.responsibleParty?.relationship ?? '',
      responsiblePartyPhone: this.initialPatient.responsibleParty?.phone ?? ''
    });

    if (this.initialPatient.hasClinicalAlerts) {
      this.form.controls.clinicalAlertsSummary.enable({ emitEvent: false });
      return;
    }

    this.form.controls.clinicalAlertsSummary.disable({ emitEvent: false });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.saved.emit({
      firstName: this.normalizeRequired(raw.firstName),
      lastName: this.normalizeRequired(raw.lastName),
      dateOfBirth: raw.dateOfBirth ?? '',
      primaryPhone: this.normalizeOptional(raw.primaryPhone),
      email: this.normalizeOptional(raw.email),
      isActive: raw.isActive ?? true,
      hasClinicalAlerts: raw.hasClinicalAlerts ?? false,
      clinicalAlertsSummary: raw.hasClinicalAlerts
        ? this.normalizeOptional(raw.clinicalAlertsSummary)
        : null,
      responsiblePartyName: this.normalizeOptional(raw.responsiblePartyName),
      responsiblePartyRelationship: this.normalizeOptional(raw.responsiblePartyRelationship),
      responsiblePartyPhone: this.normalizeOptional(raw.responsiblePartyPhone)
    });
  }

  showError(controlName: string, errorName: string): boolean {
    const control = this.form.get(controlName);
    return !!control && control.hasError(errorName) && (control.dirty || control.touched);
  }

  private normalizeRequired(value: string | null | undefined): string {
    return `${value ?? ''}`.trim();
  }

  private normalizeOptional(value: string | null | undefined): string | null {
    const normalized = `${value ?? ''}`.trim();
    return normalized ? normalized : null;
  }
}
