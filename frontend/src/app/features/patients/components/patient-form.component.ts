import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import { SectionCardComponent, StatusBadgeComponent, StickyActionBarComponent } from '../../../shared/ui';
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
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SectionCardComponent,
    StatusBadgeComponent,
    StickyActionBarComponent,
    TranslatePipe
  ],
  template: `
    <form id="patient-form" class="patient-form" [formGroup]="form" [attr.aria-busy]="saving" (ngSubmit)="submit()">
      <div class="form-grid">
        <app-section-card
          class="form-card form-card--wide"
          [title]="'Patient identity' | t"
          [subtitle]="'Keep the workflow short: core identity first, then optional responsible-party data.' | t"
          variant="elevated">
          <app-status-badge
            section-card-actions
            [tone]="form.controls.isActive.value ? 'success' : 'neutral'"
            [label]="(form.controls.isActive.value ? 'Active record' : 'Inactive record') | t">
          </app-status-badge>

          <div class="field-grid">
            <label class="form-field" for="patient-first-name">
              <span>{{ 'First name' | t }}</span>
              <input
                id="patient-first-name"
                type="text"
                formControlName="firstName"
                [attr.aria-invalid]="showAnyError('firstName') ? 'true' : 'false'"
                [attr.aria-describedby]="showError('firstName', 'required') ? 'patient-first-name-error' : null"
              />
              <small
                *ngIf="showError('firstName', 'required')"
                id="patient-first-name-error"
                class="field-error"
                role="alert">
                {{ 'First name is required.' | t }}
              </small>
            </label>

            <label class="form-field" for="patient-last-name">
              <span>{{ 'Last name' | t }}</span>
              <input
                id="patient-last-name"
                type="text"
                formControlName="lastName"
                [attr.aria-invalid]="showAnyError('lastName') ? 'true' : 'false'"
                [attr.aria-describedby]="showError('lastName', 'required') ? 'patient-last-name-error' : null"
              />
              <small
                *ngIf="showError('lastName', 'required')"
                id="patient-last-name-error"
                class="field-error"
                role="alert">
                {{ 'Last name is required.' | t }}
              </small>
            </label>

            <label class="form-field" for="patient-date-of-birth">
              <span>{{ 'Date of birth' | t }}</span>
              <input
                id="patient-date-of-birth"
                type="date"
                formControlName="dateOfBirth"
                [attr.aria-invalid]="showAnyError('dateOfBirth') ? 'true' : 'false'"
                [attr.aria-describedby]="dateOfBirthDescribedBy"
              />
              <small
                *ngIf="showError('dateOfBirth', 'required')"
                id="patient-date-of-birth-required-error"
                class="field-error"
                role="alert">
                {{ 'Date of birth is required.' | t }}
              </small>
              <small
                *ngIf="showError('dateOfBirth', 'futureDateOfBirth')"
                id="patient-date-of-birth-future-error"
                class="field-error"
                role="alert">
                {{ 'Date of birth cannot be in the future.' | t }}
              </small>
            </label>

            <label class="checkbox-field">
              <input type="checkbox" formControlName="isActive" />
              <span>{{ 'Patient is active' | t }}</span>
            </label>
          </div>
        </app-section-card>

        <app-section-card class="form-card" [title]="'Contact' | t" variant="default">
          <div class="field-grid field-grid--single">
            <label class="form-field" for="patient-primary-phone">
              <span>{{ 'Primary phone' | t }}</span>
              <input id="patient-primary-phone" type="text" inputmode="tel" formControlName="primaryPhone" />
            </label>

            <label class="form-field" for="patient-email">
              <span>{{ 'Email' | t }}</span>
              <input
                id="patient-email"
                type="email"
                formControlName="email"
                [attr.aria-invalid]="showAnyError('email') ? 'true' : 'false'"
                [attr.aria-describedby]="showError('email', 'email') ? 'patient-email-error' : null"
              />
              <small *ngIf="showError('email', 'email')" id="patient-email-error" class="field-error" role="alert">
                {{ 'Enter a valid email address.' | t }}
              </small>
            </label>
          </div>
        </app-section-card>

        <app-section-card class="form-card" [title]="'Responsible party' | t" variant="default">
          <div class="field-grid field-grid--single">
            <label class="form-field" for="patient-responsible-party-name">
              <span>{{ 'Name' | t }}</span>
              <input
                id="patient-responsible-party-name"
                type="text"
                formControlName="responsiblePartyName"
                [attr.aria-invalid]="(showAnyError('responsiblePartyName') || form.errors?.['responsiblePartyNameRequired']) ? 'true' : 'false'"
                [attr.aria-describedby]="form.errors?.['responsiblePartyNameRequired'] ? 'patient-responsible-party-error' : null"
              />
            </label>

            <label class="form-field" for="patient-responsible-party-relationship">
              <span>{{ 'Relationship' | t }}</span>
              <input
                id="patient-responsible-party-relationship"
                type="text"
                formControlName="responsiblePartyRelationship"
              />
            </label>

            <label class="form-field" for="patient-responsible-party-phone">
              <span>{{ 'Phone' | t }}</span>
              <input
                id="patient-responsible-party-phone"
                type="text"
                inputmode="tel"
                formControlName="responsiblePartyPhone"
              />
            </label>
          </div>

          <p
            *ngIf="form.errors?.['responsiblePartyNameRequired']"
            id="patient-responsible-party-error"
            class="field-error field-error--group"
            role="alert">
            {{ 'Responsible party name is required when relationship or phone is provided.' | t }}
          </p>
        </app-section-card>

        <app-section-card
          class="form-card form-card--wide"
          [title]="'Clinical alerts' | t"
          [variant]="form.controls.hasClinicalAlerts.value ? 'accent' : 'compact'">
          <app-status-badge
            section-card-actions
            [tone]="form.controls.hasClinicalAlerts.value ? 'warning' : 'neutral'"
            [label]="(form.controls.hasClinicalAlerts.value ? 'Clinical alerts' : 'No') | t">
          </app-status-badge>

          <div class="field-grid">
            <label class="checkbox-field">
              <input type="checkbox" formControlName="hasClinicalAlerts" />
              <span>{{ 'Patient has basic clinical alerts' | t }}</span>
            </label>

            <label class="form-field field-wide" for="patient-clinical-alerts-summary">
              <span>{{ 'Alert summary' | t }}</span>
              <textarea
                id="patient-clinical-alerts-summary"
                rows="3"
                formControlName="clinicalAlertsSummary"
                [placeholder]="'Short alert for immediate patient handling' | t"
                [attr.aria-invalid]="showAnyError('clinicalAlertsSummary') ? 'true' : 'false'"
                [attr.aria-describedby]="showError('clinicalAlertsSummary', 'maxlength') ? 'patient-clinical-alerts-summary-error' : null"
              ></textarea>
              <small
                *ngIf="showError('clinicalAlertsSummary', 'maxlength')"
                id="patient-clinical-alerts-summary-error"
                class="field-error"
                role="alert">
                {{ 'Alert summary must be 500 characters or fewer.' | t }}
              </small>
            </label>
          </div>
        </app-section-card>
      </div>

      <div *ngIf="error" class="error-banner" role="alert" aria-live="assertive">{{ error | t }}</div>

      <app-sticky-action-bar [ariaLabel]="(mode === 'edit' ? 'Save changes' : 'Create patient') | t">
        <button type="button" class="form-action form-action--secondary" (click)="cancelled.emit()" [disabled]="saving">
          {{ 'Cancel' | t }}
        </button>
        <button type="submit" class="form-action form-action--primary" [disabled]="saving">
          {{ (saving ? 'Saving...' : mode === 'edit' ? 'Save changes' : 'Create patient') | t }}
        </button>
      </app-sticky-action-bar>
    </form>
  `,
  styles: [`
    :host {
      display: block;
    }

    .patient-form {
      display: grid;
      gap: 1rem;
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 1rem;
      align-items: start;
    }

    .form-card--wide {
      grid-column: 1 / -1;
    }

    .field-grid {
      display: grid;
      gap: 0.9rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .field-grid--single {
      grid-template-columns: 1fr;
    }

    .form-field {
      display: grid;
      gap: 0.45rem;
      min-width: 0;
      color: var(--bsm-color-text-brand);
      font-weight: 700;
    }

    .form-field span,
    .checkbox-field span {
      line-height: 1.3;
    }

    input,
    textarea {
      width: 100%;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.82rem 0.92rem;
      font: inherit;
      color: var(--bsm-color-text);
      background: var(--bsm-color-bg);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    textarea {
      min-height: 7rem;
      resize: vertical;
    }

    input:focus,
    textarea:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    input[aria-invalid='true'],
    textarea[aria-invalid='true'] {
      border-color: var(--bsm-color-danger);
    }

    textarea:disabled {
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-muted);
    }

    .checkbox-field {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      min-height: 100%;
      color: var(--bsm-color-text-brand);
      font-weight: 700;
    }

    .checkbox-field input {
      width: 1.05rem;
      height: 1.05rem;
      flex: 0 0 auto;
      padding: 0;
      accent-color: var(--bsm-color-primary);
    }

    .field-wide {
      grid-column: 1 / -1;
    }

    .field-error {
      color: var(--bsm-color-danger);
      font-weight: 700;
      line-height: 1.35;
    }

    .field-error--group {
      margin: 0.85rem 0 0;
    }

    .error-banner {
      padding: 0.85rem 1rem;
      border: 1px solid var(--bsm-color-danger-soft);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
      box-shadow: var(--bsm-shadow-sm);
    }

    .form-action {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border: 1px solid transparent;
      border-radius: var(--bsm-radius-pill);
      padding: 0.78rem 1.08rem;
      font: inherit;
      font-weight: 800;
      line-height: 1.2;
      cursor: pointer;
      text-decoration: none;
      transition:
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        color var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .form-action--primary {
      border-color: var(--bsm-color-primary);
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .form-action--secondary {
      border-color: var(--bsm-color-primary-soft);
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .form-action:not(:disabled):hover {
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .form-action:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    .form-action:disabled {
      opacity: 0.72;
      cursor: not-allowed;
    }

    @media (prefers-reduced-motion: reduce) {
      .form-action:not(:disabled):hover {
        transform: none;
      }
    }

    @media (max-width: 760px) {
      .form-grid,
      .field-grid {
        grid-template-columns: 1fr;
      }

      .form-card--wide,
      .field-wide {
        grid-column: auto;
      }

      .form-action {
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

  get dateOfBirthDescribedBy(): string | null {
    if (this.showError('dateOfBirth', 'required')) {
      return 'patient-date-of-birth-required-error';
    }

    if (this.showError('dateOfBirth', 'futureDateOfBirth')) {
      return 'patient-date-of-birth-future-error';
    }

    return null;
  }

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

  showAnyError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
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
