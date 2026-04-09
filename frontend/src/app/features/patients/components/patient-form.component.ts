import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { PatientDetail, SavePatientRequest } from '../models/patient.models';

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
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form class="patient-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="section-head">
        <div>
          <h2>{{ mode === 'edit' ? 'Update patient' : 'Register patient' }}</h2>
          <p>Keep the workflow short: core identity first, then optional responsible-party data.</p>
        </div>
        <span class="status-indicator">
          {{ form.controls.isActive.value ? 'Active record' : 'Inactive record' }}
        </span>
      </div>

      <section class="form-section">
        <h3>Patient identity</h3>
        <div class="field-grid">
          <label>
            <span>First name</span>
            <input type="text" formControlName="firstName" />
            <small *ngIf="showError('firstName', 'required')">First name is required.</small>
          </label>

          <label>
            <span>Last name</span>
            <input type="text" formControlName="lastName" />
            <small *ngIf="showError('lastName', 'required')">Last name is required.</small>
          </label>

          <label>
            <span>Date of birth</span>
            <input type="date" formControlName="dateOfBirth" />
            <small *ngIf="showError('dateOfBirth', 'required')">Date of birth is required.</small>
          </label>

          <label class="checkbox-field">
            <input type="checkbox" formControlName="isActive" />
            <span>Patient is active</span>
          </label>
        </div>
      </section>

      <section class="form-section">
        <h3>Contact</h3>
        <div class="field-grid">
          <label>
            <span>Primary phone</span>
            <input type="text" formControlName="primaryPhone" />
          </label>

          <label>
            <span>Email</span>
            <input type="email" formControlName="email" />
            <small *ngIf="showError('email', 'email')">Enter a valid email address.</small>
          </label>
        </div>
      </section>

      <section class="form-section">
        <h3>Responsible party</h3>
        <div class="field-grid">
          <label>
            <span>Name</span>
            <input type="text" formControlName="responsiblePartyName" />
          </label>

          <label>
            <span>Relationship</span>
            <input type="text" formControlName="responsiblePartyRelationship" />
          </label>

          <label>
            <span>Phone</span>
            <input type="text" formControlName="responsiblePartyPhone" />
          </label>
        </div>
        <small *ngIf="form.errors?.['responsiblePartyNameRequired']">
          Responsible party name is required when relationship or phone is provided.
        </small>
      </section>

      <div *ngIf="error" class="error-banner">{{ error }}</div>

      <div class="form-actions">
        <button type="button" class="btn btn-secondary" (click)="cancelled.emit()" [disabled]="saving">Cancel</button>
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ saving ? 'Saving...' : mode === 'edit' ? 'Save changes' : 'Create patient' }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .patient-form {
      display: grid;
      gap: 1.25rem;
      padding: 1.5rem;
      border-radius: 20px;
      background: linear-gradient(180deg, #ffffff 0%, #f4f8fb 100%);
      border: 1px solid #d6dfe8;
      box-shadow: 0 22px 38px rgba(19, 44, 69, 0.08);
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
      color: #16324f;
    }

    .section-head p {
      margin: 0.35rem 0 0;
      color: #5d6d82;
    }

    .status-indicator {
      border-radius: 999px;
      padding: 0.5rem 0.8rem;
      background: #e7f2fb;
      color: #1d4f87;
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
      color: #16324f;
      font-weight: 600;
    }

    input {
      border: 1px solid #c8d4df;
      border-radius: 12px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: #16324f;
      background: #ffffff;
    }

    input:focus {
      outline: 2px solid #b8d5f4;
      border-color: #7aa8da;
    }

    .checkbox-field {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      font-weight: 600;
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
      border-radius: 999px;
      padding: 0.8rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: #0a5bb5;
      color: #ffffff;
    }

    .btn-secondary {
      background: #e8eef4;
      color: #17304d;
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

  @Input() initialPatient: PatientDetail | null = null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() saved = new EventEmitter<SavePatientRequest>();
  @Output() cancelled = new EventEmitter<void>();

  readonly form = this.formBuilder.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    dateOfBirth: ['', [Validators.required]],
    primaryPhone: ['', [Validators.maxLength(40)]],
    email: ['', [Validators.email, Validators.maxLength(256)]],
    isActive: [true, [Validators.required]],
    responsiblePartyName: ['', [Validators.maxLength(100)]],
    responsiblePartyRelationship: ['', [Validators.maxLength(100)]],
    responsiblePartyPhone: ['', [Validators.maxLength(40)]]
  }, { validators: responsiblePartyValidator });

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
        responsiblePartyName: '',
        responsiblePartyRelationship: '',
        responsiblePartyPhone: ''
      });
      return;
    }

    this.form.reset({
      firstName: this.initialPatient.firstName,
      lastName: this.initialPatient.lastName,
      dateOfBirth: this.initialPatient.dateOfBirth,
      primaryPhone: this.initialPatient.primaryPhone ?? '',
      email: this.initialPatient.email ?? '',
      isActive: this.initialPatient.isActive,
      responsiblePartyName: this.initialPatient.responsibleParty?.name ?? '',
      responsiblePartyRelationship: this.initialPatient.responsibleParty?.relationship ?? '',
      responsiblePartyPhone: this.initialPatient.responsibleParty?.phone ?? ''
    });
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
