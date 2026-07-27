import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import {
  PATIENT_INTAKE_FIELD_LIMITS,
  PATIENT_INTAKE_MARITAL_STATUS_VALUES,
  PATIENT_INTAKE_SEX_VALUES,
  PatientIntakeDraft,
  PatientIntakeMaritalStatus,
  PatientIntakeNonMedicalFormValue,
  PatientIntakeSex,
  toPatientIntakeNonMedicalFormValue
} from '../models/patient-intake.models';

export type PatientIntakeSaveOutcome = 'saved' | 'unchanged' | null;

@Component({
  selector: 'app-patient-intake-demographics-form',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="intake-form" [formGroup]="form" (ngSubmit)="submit()" novalidate>
      <div class="patient-alert patient-alert--info">
        {{ 'The information you enter remains a proposal until the clinic reviews it.' | t }}
      </div>

      @if (saveError) {
        <div class="patient-alert patient-alert--error" role="alert" aria-live="assertive">
          {{ saveError | t }}
        </div>
      }

      @if (saveOutcome === 'saved') {
        <div class="intake-save-message intake-save-message--success" role="status" aria-live="polite">
          {{ 'Your draft was saved.' | t }}
        </div>
      } @else if (saveOutcome === 'unchanged') {
        <div class="intake-save-message" role="status" aria-live="polite">
          {{ 'No changes were detected. The existing revision was preserved.' | t }}
        </div>
      }

      <section class="intake-section" aria-labelledby="intake-identification-heading">
        <div class="intake-section__heading">
          <p>{{ 'Patient information' | t }}</p>
          <h2 id="intake-identification-heading">{{ 'Identification' | t }}</h2>
        </div>

        <div class="intake-grid intake-grid--two">
          <label class="intake-field" for="intake-first-name">
            <span>{{ 'First name' | t }}</span>
            <input
              id="intake-first-name"
              type="text"
              formControlName="firstName"
              autocomplete="given-name"
              [attr.maxlength]="limits.name" />
            @if (hasControlError('firstName', 'maxlength')) {
              <small class="intake-field__error">{{ 'First name is too long.' | t }}</small>
            }
          </label>

          <label class="intake-field" for="intake-last-name">
            <span>{{ 'Last name' | t }}</span>
            <input
              id="intake-last-name"
              type="text"
              formControlName="lastName"
              autocomplete="family-name"
              [attr.maxlength]="limits.name" />
            @if (hasControlError('lastName', 'maxlength')) {
              <small class="intake-field__error">{{ 'Last name is too long.' | t }}</small>
            }
          </label>

          <label class="intake-field" for="intake-date-of-birth">
            <span>{{ 'Date of birth' | t }}</span>
            <input
              id="intake-date-of-birth"
              type="date"
              formControlName="dateOfBirth"
              autocomplete="bday"
              [attr.max]="maximumDate" />
            @if (hasControlError('dateOfBirth', 'futureDate')) {
              <small class="intake-field__error">{{ 'Date of birth cannot be in the future.' | t }}</small>
            }
          </label>

          <div class="intake-field intake-field--readonly" aria-live="polite">
            <span>{{ 'Age' | t }}</span>
            <output>{{ derivedAge ?? ('Not available' | t) }}</output>
            <small>{{ 'Age is calculated and is not submitted.' | t }}</small>
          </div>
        </div>
      </section>

      <section class="intake-section" aria-labelledby="intake-demographics-heading">
        <div class="intake-section__heading">
          <p>{{ 'Patient information' | t }}</p>
          <h2 id="intake-demographics-heading">{{ 'Demographic information' | t }}</h2>
        </div>

        <div class="intake-grid intake-grid--two">
          <label class="intake-field" for="intake-sex">
            <span>{{ 'Sex' | t }}</span>
            <select id="intake-sex" formControlName="sex">
              @for (option of sexOptions; track option) {
                <option [value]="option">{{ optionLabel(option) | t }}</option>
              }
            </select>
          </label>

          <label class="intake-field" for="intake-marital-status">
            <span>{{ 'Marital status' | t }}</span>
            <select id="intake-marital-status" formControlName="maritalStatus">
              @for (option of maritalStatusOptions; track option) {
                <option [value]="option">{{ optionLabel(option) | t }}</option>
              }
            </select>
          </label>

          <label class="intake-field" for="intake-occupation">
            <span>{{ 'Occupation' | t }}</span>
            <input
              id="intake-occupation"
              type="text"
              formControlName="occupation"
              autocomplete="organization-title"
              [attr.maxlength]="limits.demographic" />
            @if (hasControlError('occupation', 'maxlength')) {
              <small class="intake-field__error">{{ 'Occupation is too long.' | t }}</small>
            }
          </label>

          <label class="intake-field" for="intake-referred-by">
            <span>{{ 'Referred by' | t }}</span>
            <input
              id="intake-referred-by"
              type="text"
              formControlName="referredBy"
              [attr.maxlength]="limits.demographic" />
            @if (hasControlError('referredBy', 'maxlength')) {
              <small class="intake-field__error">{{ 'Referred-by information is too long.' | t }}</small>
            }
          </label>
        </div>
      </section>

      <section class="intake-section" aria-labelledby="intake-contact-heading">
        <div class="intake-section__heading">
          <p>{{ 'Patient information' | t }}</p>
          <h2 id="intake-contact-heading">{{ 'Contact information' | t }}</h2>
        </div>

        <div class="intake-grid intake-grid--two">
          <label class="intake-field" for="intake-preferred-phone">
            <span>{{ 'Preferred phone' | t }}</span>
            <input
              id="intake-preferred-phone"
              type="tel"
              formControlName="preferredPhone"
              autocomplete="tel"
              [attr.maxlength]="limits.phone" />
          </label>

          <label class="intake-field" for="intake-mobile-phone">
            <span>{{ 'Mobile phone' | t }}</span>
            <input
              id="intake-mobile-phone"
              type="tel"
              formControlName="mobilePhone"
              autocomplete="tel"
              [attr.maxlength]="limits.phone" />
          </label>

          <label class="intake-field" for="intake-home-phone">
            <span>{{ 'Home phone' | t }}</span>
            <input
              id="intake-home-phone"
              type="tel"
              formControlName="homePhone"
              [attr.maxlength]="limits.phone" />
          </label>

          <label class="intake-field" for="intake-work-phone">
            <span>{{ 'Work phone' | t }}</span>
            <input
              id="intake-work-phone"
              type="tel"
              formControlName="workPhone"
              autocomplete="work tel"
              [attr.maxlength]="limits.phone" />
          </label>

          <label class="intake-field intake-field--wide" for="intake-email">
            <span>{{ 'Email' | t }}</span>
            <input
              id="intake-email"
              type="email"
              formControlName="email"
              autocomplete="email"
              [attr.maxlength]="limits.email" />
            @if (hasControlError('email', 'email')) {
              <small class="intake-field__error">{{ 'Enter a valid email address.' | t }}</small>
            }
            @if (hasControlError('email', 'maxlength')) {
              <small class="intake-field__error">{{ 'Email is too long.' | t }}</small>
            }
          </label>
        </div>
      </section>

      <section class="intake-section" aria-labelledby="intake-responsible-heading">
        <div class="intake-section__heading">
          <p>{{ 'Patient information' | t }}</p>
          <h2 id="intake-responsible-heading">{{ 'Responsible party' | t }}</h2>
        </div>

        <div class="intake-grid intake-grid--two">
          <label class="intake-field" for="intake-responsible-name">
            <span>{{ 'Responsible party name' | t }}</span>
            <input
              id="intake-responsible-name"
              type="text"
              formControlName="responsiblePartyName"
              [attr.maxlength]="limits.name" />
            @if (responsiblePartyNameRequired) {
              <small class="intake-field__error">
                {{ 'Responsible party name is required when relationship or phone is provided.' | t }}
              </small>
            }
          </label>

          <label class="intake-field" for="intake-responsible-relationship">
            <span>{{ 'Relationship' | t }}</span>
            <input
              id="intake-responsible-relationship"
              type="text"
              formControlName="responsiblePartyRelationship"
              [attr.maxlength]="limits.demographic" />
          </label>

          <label class="intake-field intake-field--wide" for="intake-responsible-phone">
            <span>{{ 'Responsible party phone' | t }}</span>
            <input
              id="intake-responsible-phone"
              type="tel"
              formControlName="responsiblePartyPhone"
              [attr.maxlength]="limits.phone" />
          </label>
        </div>
      </section>

      <section class="intake-section" aria-labelledby="intake-visit-heading">
        <div class="intake-section__heading">
          <p>{{ 'Patient information' | t }}</p>
          <h2 id="intake-visit-heading">{{ 'Visit context' | t }}</h2>
        </div>

        <label class="intake-field" for="intake-reason-for-visit">
          <span>{{ 'Reason for visit' | t }}</span>
          <textarea
            id="intake-reason-for-visit"
            rows="5"
            formControlName="reasonForVisit"
            [attr.maxlength]="limits.reasonForVisit"></textarea>
          <small class="intake-field__hint">
            {{ remainingReasonCharacters }} {{ 'characters remaining' | t }}
          </small>
          @if (hasControlError('reasonForVisit', 'maxlength')) {
            <small class="intake-field__error">{{ 'Reason for visit is too long.' | t }}</small>
          }
        </label>
      </section>

      <div class="intake-actions">
        <p class="intake-actions__note">
          {{ 'Saving updates this private draft only. The clinic must review it before applying changes.' | t }}
        </p>
        <button
          type="submit"
          class="patient-button patient-button--primary"
          [disabled]="saving || form.invalid">
          {{ (saving ? 'Saving draft...' : 'Save draft') | t }}
        </button>
      </div>
    </form>
  `,
  styleUrl: '../../patient-portal-auth/styles/patient-portal-auth-page.scss',
  styles: [`
    :host,
    .intake-form,
    .intake-section,
    .intake-field {
      display: grid;
    }

    .intake-form {
      gap: 1rem;
    }

    .intake-section {
      gap: 1rem;
      padding: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      background: var(--bsm-color-bg);
    }

    .intake-section__heading p,
    .intake-section__heading h2,
    .intake-actions__note {
      margin: 0;
    }

    .intake-section__heading p {
      color: var(--bsm-color-text-muted);
      font-size: 0.75rem;
      font-weight: 800;
      letter-spacing: 0.04em;
      text-transform: uppercase;
    }

    .intake-section__heading h2 {
      margin-top: 0.25rem;
      color: var(--bsm-color-text-brand);
      font-size: 1.1rem;
    }

    .intake-grid {
      display: grid;
      gap: 1rem;
    }

    .intake-grid--two {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .intake-field {
      gap: 0.45rem;
      align-content: start;
    }

    .intake-field--wide {
      grid-column: 1 / -1;
    }

    .intake-field > span {
      color: var(--bsm-color-text-brand);
      font-weight: 750;
    }

    .intake-field input,
    .intake-field select,
    .intake-field textarea {
      width: 100%;
      box-sizing: border-box;
      padding: 0.8rem 0.9rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
      font: inherit;
    }

    .intake-field textarea {
      resize: vertical;
      min-height: 7rem;
    }

    .intake-field input:focus,
    .intake-field select:focus,
    .intake-field textarea:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .intake-field--readonly {
      padding: 0.8rem 0.9rem;
      border: 1px dashed var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      background: var(--bsm-color-surface);
    }

    .intake-field--readonly output {
      color: var(--bsm-color-text-brand);
      font-size: 1.1rem;
      font-weight: 800;
    }

    .intake-field--readonly small,
    .intake-field__hint,
    .intake-actions__note {
      color: var(--bsm-color-text-muted);
      font-size: 0.85rem;
      line-height: 1.45;
    }

    .intake-field__error {
      color: var(--bsm-color-danger);
      font-size: 0.85rem;
      font-weight: 650;
    }

    .intake-save-message {
      padding: 0.8rem 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-brand);
    }

    .intake-save-message--success {
      border-color: var(--bsm-color-success-soft);
      background: var(--bsm-color-success-soft);
      color: var(--bsm-color-success-text);
    }

    .intake-actions {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto;
      gap: 1rem;
      align-items: center;
      padding: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      background: var(--bsm-color-surface);
    }

    @media (max-width: 720px) {
      .intake-grid--two,
      .intake-actions {
        grid-template-columns: 1fr;
      }

      .intake-field--wide {
        grid-column: auto;
      }

      .intake-actions .patient-button {
        width: 100%;
      }
    }
  `]
})
export class PatientIntakeDemographicsFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input({ required: true }) intake!: PatientIntakeDraft;
  @Input() saving = false;
  @Input() saveOutcome: PatientIntakeSaveOutcome = null;
  @Input() saveError: string | null = null;
  @Output() readonly saveRequested = new EventEmitter<PatientIntakeNonMedicalFormValue>();

  readonly limits = PATIENT_INTAKE_FIELD_LIMITS;
  readonly sexOptions = PATIENT_INTAKE_SEX_VALUES;
  readonly maritalStatusOptions = PATIENT_INTAKE_MARITAL_STATUS_VALUES;
  readonly maximumDate = utcTodayIsoDate();

  readonly form = this.formBuilder.group({
    firstName: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.name)]),
    lastName: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.name)]),
    dateOfBirth: this.formBuilder.nonNullable.control('', [notFutureDateValidator()]),
    sex: this.formBuilder.nonNullable.control<PatientIntakeSex>('Unspecified'),
    occupation: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.demographic)]),
    maritalStatus: this.formBuilder.nonNullable.control<PatientIntakeMaritalStatus>('Unspecified'),
    referredBy: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.demographic)]),
    preferredPhone: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.phone)]),
    mobilePhone: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.phone)]),
    homePhone: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.phone)]),
    workPhone: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.phone)]),
    email: this.formBuilder.nonNullable.control('', [
      Validators.email,
      Validators.maxLength(this.limits.email)
    ]),
    responsiblePartyName: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.name)]),
    responsiblePartyRelationship: this.formBuilder.nonNullable.control('', [
      Validators.maxLength(this.limits.demographic)
    ]),
    responsiblePartyPhone: this.formBuilder.nonNullable.control('', [Validators.maxLength(this.limits.phone)]),
    reasonForVisit: this.formBuilder.nonNullable.control('', [
      Validators.maxLength(this.limits.reasonForVisit)
    ])
  }, { validators: [responsiblePartyNameValidator()] });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['intake'] && this.intake) {
      this.form.reset(toPatientIntakeNonMedicalFormValue(this.intake));
    }
  }

  get derivedAge(): number | null {
    return calculateAge(this.form.controls.dateOfBirth.value);
  }

  get remainingReasonCharacters(): number {
    return Math.max(
      0,
      this.limits.reasonForVisit - this.form.controls.reasonForVisit.value.length
    );
  }

  get responsiblePartyNameRequired(): boolean {
    const relevantControlTouched =
      this.form.controls.responsiblePartyName.touched ||
      this.form.controls.responsiblePartyRelationship.touched ||
      this.form.controls.responsiblePartyPhone.touched;

    return relevantControlTouched && this.form.hasError('responsiblePartyNameRequired');
  }

  hasControlError(
    controlName: keyof PatientIntakeNonMedicalFormValue,
    errorCode: string
  ): boolean {
    const control = this.form.controls[controlName];
    return control.touched && control.hasError(errorCode);
  }

  optionLabel(value: PatientIntakeSex | PatientIntakeMaritalStatus): string {
    return value === 'Unspecified' ? 'Not specified' : value;
  }

  submit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.saving) {
      return;
    }

    this.saveRequested.emit(this.form.getRawValue());
  }
}

export function notFutureDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = typeof control.value === 'string' ? control.value.trim() : '';
    if (!value) {
      return null;
    }

    return value > utcTodayIsoDate() ? { futureDate: true } : null;
  };
}

export function responsiblePartyNameValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const name = normalizeControlValue(control.get('responsiblePartyName')?.value);
    const relationship = normalizeControlValue(control.get('responsiblePartyRelationship')?.value);
    const phone = normalizeControlValue(control.get('responsiblePartyPhone')?.value);

    return (relationship || phone) && !name
      ? { responsiblePartyNameRequired: true }
      : null;
  };
}

export function calculateAge(
  dateOfBirth: string,
  todayIsoDate: string = utcTodayIsoDate()
): number | null {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(dateOfBirth) || !/^\d{4}-\d{2}-\d{2}$/.test(todayIsoDate)) {
    return null;
  }

  const [birthYear, birthMonth, birthDay] = dateOfBirth.split('-').map(Number);
  const [todayYear, todayMonth, todayDay] = todayIsoDate.split('-').map(Number);
  if (!birthYear || !birthMonth || !birthDay || dateOfBirth > todayIsoDate) {
    return null;
  }

  let age = todayYear - birthYear;
  if (todayMonth < birthMonth || (todayMonth === birthMonth && todayDay < birthDay)) {
    age -= 1;
  }

  return age >= 0 ? age : null;
}

function normalizeControlValue(value: unknown): string {
  return typeof value === 'string' ? value.trim() : '';
}

function utcTodayIsoDate(): string {
  return new Date().toISOString().slice(0, 10);
}
