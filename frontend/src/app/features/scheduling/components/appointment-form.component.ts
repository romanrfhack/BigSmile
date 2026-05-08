import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import {
  AppointmentEditorMode,
  AppointmentFormValue,
  AppointmentSummary,
  SchedulingPatientLookup
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="appointment-form" [formGroup]="form" (ngSubmit)="submit()">
      <header class="form-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 2 / {{ 'Scheduling' | t }}</p>
          <h3>{{ title | t }}</h3>
          <p class="subtitle">
            {{ subtitle | t }}
          </p>
        </div>
        <button type="button" class="text-button" (click)="cancelled.emit()">{{ 'Reset' | t }}</button>
      </header>

      <div class="branch-banner">
        <span>{{ 'Branch' | t }}</span>
        <strong>{{ selectedBranchName || ('No branch selected' | t) }}</strong>
      </div>

      <label class="field" *ngIf="mode !== 'reschedule'">
        <span>{{ 'Find patient' | t }}</span>
        <input
          type="search"
          formControlName="patientSearch"
          [placeholder]="'Search existing patients' | t"
          (input)="patientSearchChanged.emit(form.controls.patientSearch.value ?? '')" />
      </label>

      <div *ngIf="mode !== 'reschedule' && searchingPatients" class="search-state">{{ 'Searching patients...' | t }}</div>

      <div *ngIf="mode !== 'reschedule' && patientOptions.length" class="patient-results">
        <button
          type="button"
          class="patient-option"
          *ngFor="let patient of patientOptions"
          (click)="selectPatient(patient)">
          <span>{{ patient.fullName }}</span>
          <small>
            {{ patient.primaryPhone || patient.email || ('No contact data' | t) }}
            <span *ngIf="patient.hasClinicalAlerts"> / {{ 'Clinical alerts' | t }}</span>
          </small>
        </button>
      </div>

      <div *ngIf="mode !== 'reschedule' && selectedPatientName" class="selected-patient">
        <span>{{ 'Selected patient' | t }}</span>
        <strong>{{ selectedPatientName }}</strong>
      </div>

      <div *ngIf="mode === 'reschedule' && initialAppointment" class="selected-patient">
        <span>{{ 'Rescheduling' | t }}</span>
        <strong>{{ initialAppointment.patientFullName }}</strong>
      </div>

      <div class="time-grid">
        <label class="field">
          <span>{{ 'Start' | t }}</span>
          <input type="datetime-local" formControlName="startsAt" />
        </label>

        <label class="field">
          <span>{{ 'End' | t }}</span>
          <input type="datetime-local" formControlName="endsAt" />
        </label>
      </div>

      <label class="field" *ngIf="mode !== 'reschedule'">
        <span>{{ 'Notes' | t }}</span>
        <textarea formControlName="notes" rows="4" [placeholder]="'Short operational note' | t"></textarea>
      </label>

      <div *ngIf="mode === 'reschedule' && initialAppointment?.notes" class="reschedule-note">
        <span>{{ 'Existing note' | t }}</span>
        <p>{{ initialAppointment?.notes }}</p>
      </div>

      <div *ngIf="error" class="form-error">{{ error | t }}</div>
      <div *ngIf="form.errors?.['timeRangeInvalid']" class="form-error">
        {{ 'End time must be after the start time.' | t }}
      </div>
      <div *ngIf="form.errors?.['patientRequired']" class="form-error">
        {{ 'Select an existing patient before saving the appointment.' | t }}
      </div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ (saving ? 'Saving...' : submitLabel) | t }}
        </button>
        <button type="button" class="btn btn-secondary" (click)="cancelled.emit()" [disabled]="saving">
          {{ 'Cancel' | t }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .appointment-form {
      display: grid;
      gap: 1rem;
    }

    .form-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .eyebrow {
      margin: 0 0 0.35rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.78rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    .subtitle {
      margin: 0.45rem 0 0;
      color: var(--bsm-color-text-muted);
      max-width: 44ch;
    }

    .text-button {
      border: none;
      background: transparent;
      color: var(--bsm-color-accent-accessible);
      cursor: pointer;
      font-weight: 700;
    }

    .branch-banner,
    .selected-patient,
    .search-state,
    .reschedule-note {
      border-radius: var(--bsm-radius-md);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-surface);
      padding: 0.9rem 1rem;
    }

    .branch-banner span,
    .selected-patient span,
    .reschedule-note span {
      display: block;
      font-size: 0.78rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-text-muted);
      margin-bottom: 0.25rem;
    }

    .branch-banner strong,
    .selected-patient strong {
      color: var(--bsm-color-text-brand);
    }

    .field {
      display: grid;
      gap: 0.45rem;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
    }

    .field input,
    .field textarea {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.85rem 0.95rem;
      font: inherit;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .field input:focus,
    .field textarea:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .time-grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    }

    .patient-results {
      display: grid;
      gap: 0.6rem;
    }

    .patient-option {
      display: grid;
      gap: 0.2rem;
      text-align: left;
      border-radius: var(--bsm-radius-md);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
      padding: 0.85rem 0.95rem;
      cursor: pointer;
      color: var(--bsm-color-text-brand);
      font: inherit;
    }

    .patient-option small {
      color: var(--bsm-color-text-muted);
    }

    .form-error {
      border-radius: var(--bsm-radius-sm);
      border: 1px solid var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      padding: 0.85rem 0.95rem;
    }

    .form-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .btn {
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .btn-secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    @media (max-width: 768px) {
      .form-head {
        flex-direction: column;
      }

      .form-actions .btn {
        width: 100%;
      }
    }
  `]
})
export class AppointmentFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() mode: AppointmentEditorMode = 'create';
  @Input() selectedBranchName = '';
  @Input() draftDate = '';
  @Input() initialAppointment: AppointmentSummary | null = null;
  @Input() patientOptions: SchedulingPatientLookup[] = [];
  @Input() searchingPatients = false;
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() patientSearchChanged = new EventEmitter<string>();
  @Output() saved = new EventEmitter<AppointmentFormValue>();
  @Output() cancelled = new EventEmitter<void>();

  readonly form = this.formBuilder.group({
    patientSearch: [''],
    patientId: ['', Validators.required],
    startsAt: ['', Validators.required],
    endsAt: ['', Validators.required],
    notes: ['', Validators.maxLength(1000)]
  });

  selectedPatientName = '';

  get title(): string {
    switch (this.mode) {
      case 'edit':
        return 'Edit appointment';
      case 'reschedule':
        return 'Reschedule appointment';
      default:
        return 'Create appointment';
    }
  }

  get subtitle(): string {
    switch (this.mode) {
      case 'edit':
        return 'Adjust patient, time, or notes without leaving the selected branch.';
      case 'reschedule':
        return 'Move the appointment without reopening the full editing flow.';
      default:
        return 'Capture the smallest operational appointment needed for the schedule.';
    }
  }

  get submitLabel(): string {
    switch (this.mode) {
      case 'edit':
        return 'Save changes';
      case 'reschedule':
        return 'Reschedule';
      default:
        return 'Create appointment';
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['mode'] || changes['initialAppointment'] || changes['draftDate']) {
      this.configureForm();
    }
  }

  selectPatient(patient: SchedulingPatientLookup): void {
    this.selectedPatientName = patient.fullName;
    this.form.patchValue({
      patientSearch: patient.fullName,
      patientId: patient.id
    });
    this.patientSearchChanged.emit(patient.fullName);
  }

  submit(): void {
    this.form.setErrors(null);

    const startsAt = this.form.controls.startsAt.value ?? '';
    const endsAt = this.form.controls.endsAt.value ?? '';
    const patientId = this.form.controls.patientId.value ?? '';

    if (!startsAt || !endsAt || endsAt <= startsAt) {
      this.form.setErrors({ timeRangeInvalid: true });
      return;
    }

    if (this.mode !== 'reschedule' && !patientId) {
      this.form.setErrors({ patientRequired: true });
      return;
    }

    this.saved.emit({
      patientId: patientId || null,
      startsAt,
      endsAt,
      notes: normalizeOptional(this.form.controls.notes.value)
    });
  }

  private configureForm(): void {
    if (this.initialAppointment) {
      this.selectedPatientName = this.initialAppointment.patientFullName;
      this.form.reset({
        patientSearch: this.initialAppointment.patientFullName,
        patientId: this.initialAppointment.patientId,
        startsAt: toLocalDateTimeValue(this.initialAppointment.startsAt),
        endsAt: toLocalDateTimeValue(this.initialAppointment.endsAt),
        notes: this.initialAppointment.notes ?? ''
      }, { emitEvent: false });
    } else {
      this.selectedPatientName = '';
      const { startsAt, endsAt } = buildDraftTimes(this.draftDate);
      this.form.reset({
        patientSearch: '',
        patientId: '',
        startsAt,
        endsAt,
        notes: ''
      }, { emitEvent: false });
    }

    if (this.mode === 'reschedule') {
      this.form.controls.patientSearch.disable({ emitEvent: false });
      this.form.controls.patientId.disable({ emitEvent: false });
      this.form.controls.notes.disable({ emitEvent: false });
      return;
    }

    this.form.controls.patientSearch.enable({ emitEvent: false });
    this.form.controls.patientId.enable({ emitEvent: false });
    this.form.controls.notes.enable({ emitEvent: false });
  }
}

function toLocalDateTimeValue(value: string): string {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return '';
  }

  const year = parsed.getFullYear();
  const month = `${parsed.getMonth() + 1}`.padStart(2, '0');
  const day = `${parsed.getDate()}`.padStart(2, '0');
  const hours = `${parsed.getHours()}`.padStart(2, '0');
  const minutes = `${parsed.getMinutes()}`.padStart(2, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function buildDraftTimes(draftDate: string): { startsAt: string; endsAt: string } {
  const normalizedDate = draftDate || new Date().toISOString().slice(0, 10);
  return {
    startsAt: `${normalizedDate}T09:00`,
    endsAt: `${normalizedDate}T09:30`
  };
}

function normalizeOptional(value: string | null | undefined): string | null {
  const normalized = value?.trim();
  return normalized ? normalized : null;
}
