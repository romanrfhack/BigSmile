import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  SectionCardComponent,
  StatusBadgeComponent,
  StatusBadgeTone,
  StickyActionBarComponent
} from '../../../shared/ui';
import {
  AddClinicalEncounterRequest,
  ClinicalEncounter,
  ClinicalEncounterConsultationType
} from '../models/clinical-record.models';

type EncounterVitalsForm = FormGroup<{
  occurredAtLocal: FormControl<string>;
  chiefComplaint: FormControl<string>;
  consultationType: FormControl<ClinicalEncounterConsultationType>;
  temperatureC: FormControl<number | null>;
  bloodPressureSystolic: FormControl<number | null>;
  bloodPressureDiastolic: FormControl<number | null>;
  weightKg: FormControl<number | null>;
  heightCm: FormControl<number | null>;
  respiratoryRatePerMinute: FormControl<number | null>;
  heartRateBpm: FormControl<number | null>;
  noteText: FormControl<string>;
}>;

type EncounterVitalsControlName = keyof EncounterVitalsForm['controls'];

@Component({
  selector: 'app-clinical-encounter-vitals-section',
  standalone: true,
  imports: [
    CommonModule,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    ReactiveFormsModule,
    SectionCardComponent,
    StatusBadgeComponent,
    StickyActionBarComponent,
    TranslatePipe
  ],
  template: `
    <app-section-card
      [title]="'Consultation and vital signs' | t"
      [subtitle]="'Register a clinical encounter with optional vital signs for this existing clinical record.' | t"
      variant="elevated">
      <div section-card-actions class="encounter-status">
        <app-status-badge *ngIf="loading" tone="info" [label]="'Loading' | t"></app-status-badge>
        <app-status-badge *ngIf="!loading && error" tone="danger" [label]="'Error' | t"></app-status-badge>
        <app-status-badge
          *ngIf="!loading && !error && encounters.length === 0"
          tone="neutral"
          [label]="'No encounters captured' | t">
        </app-status-badge>
        <app-status-badge
          *ngIf="!loading && !error && encounters.length > 0"
          tone="primary"
          [label]="'Recent encounters' | t">
        </app-status-badge>
      </div>

      <div class="encounter-shell">
        <ng-container *ngIf="loading">
          <app-loading-skeleton
            *ngFor="let item of loadingCards"
            variant="card"
            [ariaLabel]="'Loading clinical encounters...' | t">
          </app-loading-skeleton>
        </ng-container>

        <div *ngIf="!loading && error" class="state-note state-note--error" role="alert">
          <p>{{ error | t }}</p>
          <button type="button" class="btn btn-secondary" (click)="retryRequested.emit()">
            {{ 'Retry' | t }}
          </button>
        </div>

        <app-empty-state
          *ngIf="!loading && !error && missing"
          [title]="'Clinical encounters unavailable' | t"
          [description]="'Encounters are available only after the clinical record exists. They are not auto-created.' | t"
          icon="?"
          ariaLive="polite">
          <button empty-state-action type="button" class="btn btn-secondary" (click)="retryRequested.emit()">
            {{ 'Retry' | t }}
          </button>
        </app-empty-state>

        <form
          *ngIf="!loading && !error && !missing && canWrite"
          class="encounter-form"
          [formGroup]="form"
          (ngSubmit)="submit()">
          <p class="state-note">
            {{ 'The form keeps vitals as captured values only and does not classify them yet.' | t }}
          </p>

          <p *ngIf="saveError" class="state-note state-note--error" role="alert">
            {{ saveError | t }}
          </p>

          <div class="encounter-form-grid">
            <label class="field">
              <span>{{ 'Encounter date/time' | t }}</span>
              <input id="encounter-occurred-at" type="datetime-local" formControlName="occurredAtLocal">
              <small *ngIf="hasControlError('occurredAtLocal', 'required')">
                {{ 'Clinical encounter occurrence date/time is required.' | t }}
              </small>
            </label>

            <label class="field">
              <span>{{ 'Consultation type' | t }}</span>
              <select id="encounter-consultation-type" formControlName="consultationType">
                <option *ngFor="let type of consultationTypes" [value]="type">
                  {{ type | t }}
                </option>
              </select>
            </label>

            <label class="field field--wide">
              <span>{{ 'Chief complaint' | t }}</span>
              <input id="encounter-chief-complaint" type="text" formControlName="chiefComplaint">
              <small *ngIf="hasControlError('chiefComplaint', 'required')">
                {{ 'Chief complaint is required.' | t }}
              </small>
              <small *ngIf="hasControlError('chiefComplaint', 'maxlength')">
                {{ 'Chief complaint must be 500 characters or fewer.' | t }}
              </small>
            </label>
          </div>

          <section class="vitals-form" [attr.aria-label]="'Optional vital signs' | t">
            <h3>{{ 'Optional vital signs' | t }}</h3>

            <div class="vitals-grid">
              <label class="field">
                <span>{{ 'Temperature' | t }}</span>
                <input id="encounter-temperature" type="number" inputmode="decimal" step="0.1" formControlName="temperatureC">
                <small *ngIf="hasControlError('temperatureC', 'min') || hasControlError('temperatureC', 'max')">
                  {{ 'Temperature must be between 30.0 and 45.0 Celsius.' | t }}
                </small>
              </label>

              <label class="field">
                <span>{{ 'Blood pressure systolic' | t }}</span>
                <input id="encounter-bp-systolic" type="number" inputmode="numeric" formControlName="bloodPressureSystolic">
                <small *ngIf="hasControlError('bloodPressureSystolic', 'min') || hasControlError('bloodPressureSystolic', 'max')">
                  {{ 'Blood pressure systolic value must be between 50 and 260.' | t }}
                </small>
              </label>

              <label class="field">
                <span>{{ 'Blood pressure diastolic' | t }}</span>
                <input id="encounter-bp-diastolic" type="number" inputmode="numeric" formControlName="bloodPressureDiastolic">
                <small *ngIf="hasControlError('bloodPressureDiastolic', 'min') || hasControlError('bloodPressureDiastolic', 'max')">
                  {{ 'Blood pressure diastolic value must be between 30 and 180.' | t }}
                </small>
                <small *ngIf="hasBloodPressureError('bloodPressurePair')">
                  {{ 'Blood pressure requires both systolic and diastolic values.' | t }}
                </small>
                <small *ngIf="hasBloodPressureError('bloodPressureOrder')">
                  {{ 'Diastolic pressure must be lower than systolic pressure.' | t }}
                </small>
              </label>

              <label class="field">
                <span>{{ 'Weight' | t }}</span>
                <input id="encounter-weight" type="number" inputmode="decimal" step="0.1" formControlName="weightKg">
                <small *ngIf="hasControlError('weightKg', 'min') || hasControlError('weightKg', 'max')">
                  {{ 'Weight must be between 0.5 and 500.0 kilograms.' | t }}
                </small>
              </label>

              <label class="field">
                <span>{{ 'Height' | t }}</span>
                <input id="encounter-height" type="number" inputmode="decimal" step="0.1" formControlName="heightCm">
                <small *ngIf="hasControlError('heightCm', 'min') || hasControlError('heightCm', 'max')">
                  {{ 'Height must be between 30.0 and 250.0 centimeters.' | t }}
                </small>
              </label>

              <label class="field">
                <span>{{ 'Respiratory rate' | t }}</span>
                <input id="encounter-respiratory-rate" type="number" inputmode="numeric" formControlName="respiratoryRatePerMinute">
                <small *ngIf="hasControlError('respiratoryRatePerMinute', 'min') || hasControlError('respiratoryRatePerMinute', 'max')">
                  {{ 'Respiratory rate must be between 5 and 80 per minute.' | t }}
                </small>
              </label>

              <label class="field">
                <span>{{ 'Heart rate' | t }}</span>
                <input id="encounter-heart-rate" type="number" inputmode="numeric" formControlName="heartRateBpm">
                <small *ngIf="hasControlError('heartRateBpm', 'min') || hasControlError('heartRateBpm', 'max')">
                  {{ 'Heart rate must be between 20 and 240 BPM.' | t }}
                </small>
              </label>
            </div>
          </section>

          <label class="field">
            <span>{{ 'Optional encounter note' | t }}</span>
            <textarea id="encounter-note-text" rows="3" formControlName="noteText"></textarea>
            <small *ngIf="hasControlError('noteText', 'maxlength')">
              {{ 'Note must be 2000 characters or fewer.' | t }}
            </small>
          </label>

          <app-sticky-action-bar [ariaLabel]="'Encounter actions' | t">
            <span class="form-status">
              {{ (form.dirty ? 'Unsaved changes' : 'No unsaved changes') | t }}
            </span>
            <button type="button" class="btn btn-secondary" (click)="cancel()" [disabled]="saving">
              {{ 'Cancel' | t }}
            </button>
            <button type="submit" class="btn btn-primary" [disabled]="saving || form.invalid">
              {{ (saving ? 'Saving...' : 'Save encounter') | t }}
            </button>
          </app-sticky-action-bar>
        </form>

        <p *ngIf="!loading && !error && !missing && !canWrite" class="state-note state-note--readonly">
          {{ 'Your current session can read clinical data but cannot register encounters.' | t }}
        </p>

        <app-empty-state
          *ngIf="!loading && !error && !missing && encounters.length === 0"
          [title]="'No encounters captured yet' | t"
          [description]="'Recent consultation and vital sign records will appear here newest first.' | t"
          icon="+"
          ariaLive="polite">
        </app-empty-state>

        <section
          *ngIf="!loading && !error && !missing && encounters.length > 0"
          class="encounters-list"
          [attr.aria-label]="'Recent encounters' | t">
          <article *ngFor="let encounter of encounters" class="encounter-card">
            <header class="encounter-card__header">
              <div>
                <p class="eyebrow">{{ encounter.occurredAtUtc | date: 'medium' }}</p>
                <h3>{{ encounter.chiefComplaint }}</h3>
              </div>
              <app-status-badge
                [tone]="consultationTypeTone(encounter.consultationType)"
                [label]="encounter.consultationType | t">
              </app-status-badge>
            </header>

            <dl *ngIf="hasVitals(encounter); else noVitals" class="vitals-read-grid">
              <div *ngIf="encounter.temperatureC !== null">
                <dt>{{ 'Temperature' | t }}</dt>
                <dd>{{ encounter.temperatureC | number: '1.0-1' }} {{ 'Celsius short' | t }}</dd>
              </div>
              <div *ngIf="encounter.bloodPressureSystolic !== null && encounter.bloodPressureDiastolic !== null">
                <dt>{{ 'Blood pressure' | t }}</dt>
                <dd>{{ encounter.bloodPressureSystolic }}/{{ encounter.bloodPressureDiastolic }} {{ 'mmHg' | t }}</dd>
              </div>
              <div *ngIf="encounter.weightKg !== null">
                <dt>{{ 'Weight' | t }}</dt>
                <dd>{{ encounter.weightKg | number: '1.0-1' }} {{ 'kg' | t }}</dd>
              </div>
              <div *ngIf="encounter.heightCm !== null">
                <dt>{{ 'Height' | t }}</dt>
                <dd>{{ encounter.heightCm | number: '1.0-1' }} {{ 'cm' | t }}</dd>
              </div>
              <div *ngIf="encounter.respiratoryRatePerMinute !== null">
                <dt>{{ 'Respiratory rate' | t }}</dt>
                <dd>{{ encounter.respiratoryRatePerMinute }} {{ 'per min' | t }}</dd>
              </div>
              <div *ngIf="encounter.heartRateBpm !== null">
                <dt>{{ 'Heart rate' | t }}</dt>
                <dd>{{ encounter.heartRateBpm }} {{ 'bpm' | t }}</dd>
              </div>
            </dl>

            <ng-template #noVitals>
              <p class="state-note state-note--compact">{{ 'No vitals captured' | t }}</p>
            </ng-template>

            <p *ngIf="encounter.noteText" class="encounter-note">
              <strong>{{ 'Linked clinical note' | t }}:</strong>
              {{ encounter.noteText }}
            </p>

            <footer class="encounter-meta">
              <span>{{ 'Captured' | t }} {{ encounter.createdAtUtc | date: 'medium' }}</span>
              <span>{{ 'Captured by' | t }} {{ encounter.createdByUserId }}</span>
            </footer>
          </article>
        </section>
      </div>
    </app-section-card>
  `,
  styles: [`
    :host {
      display: block;
    }

    .encounter-shell,
    .encounter-form,
    .vitals-form,
    .encounters-list,
    .encounter-card {
      display: grid;
      gap: 1rem;
    }

    .encounter-status {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
      justify-content: flex-end;
    }

    .encounter-form-grid,
    .vitals-grid,
    .vitals-read-grid {
      display: grid;
      gap: 0.75rem;
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }

    .field,
    .vitals-form {
      display: grid;
      gap: 0.5rem;
    }

    .field--wide {
      grid-column: span 3;
    }

    .vitals-form h3,
    .encounter-card h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: 1rem;
      line-height: 1.25;
    }

    .field span,
    dt {
      color: var(--bsm-color-text-brand);
      font-weight: 800;
      line-height: 1.3;
    }

    input,
    select,
    textarea {
      width: 100%;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      padding: 0.72rem 0.8rem;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text-brand);
      font: inherit;
    }

    textarea {
      min-height: 5.25rem;
      resize: vertical;
    }

    input:focus-visible,
    select:focus-visible,
    textarea:focus-visible,
    .btn:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    small {
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .state-note {
      margin: 0;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      padding: 0.85rem 1rem;
      background: var(--bsm-color-neutral-soft);
      color: var(--bsm-color-text-muted);
      line-height: 1.45;
    }

    .state-note--compact {
      padding: 0.7rem 0.8rem;
    }

    .state-note--error {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
      border-color: var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .state-note--error p {
      margin: 0;
    }

    .state-note--readonly {
      border-color: var(--bsm-color-warning-soft);
      background: var(--bsm-color-warning-soft);
      color: var(--bsm-color-warning);
      font-weight: 700;
    }

    .encounter-card {
      padding: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
      box-shadow: var(--bsm-shadow-sm);
    }

    .encounter-card__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
    }

    .eyebrow {
      margin: 0 0 0.35rem;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 800;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    .vitals-read-grid {
      margin: 0;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    }

    .vitals-read-grid div {
      min-width: 0;
      padding: 0.72rem 0.8rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
    }

    dt {
      margin-bottom: 0.22rem;
      font-size: 0.76rem;
      text-transform: uppercase;
    }

    dd {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-weight: 800;
    }

    .encounter-note {
      margin: 0;
      color: var(--bsm-color-text);
      line-height: 1.45;
    }

    .encounter-meta {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
      color: var(--bsm-color-text-muted);
      font-size: 0.86rem;
      font-weight: 700;
    }

    .form-status {
      margin-inline-end: auto;
      color: var(--bsm-color-text-muted);
      font-weight: 800;
    }

    .btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 7.5rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-pill);
      padding: 0.75rem 1rem;
      font: inherit;
      font-weight: 800;
      line-height: 1.2;
      cursor: pointer;
    }

    .btn-primary {
      border-color: var(--bsm-color-primary);
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .btn-secondary {
      border-color: var(--bsm-color-primary-soft);
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .btn:not(:disabled):hover {
      box-shadow: var(--bsm-shadow-sm);
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.68;
    }

    @media (max-width: 920px) {
      .encounter-form-grid,
      .vitals-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }

      .field--wide {
        grid-column: span 2;
      }
    }

    @media (max-width: 640px) {
      .encounter-form-grid,
      .vitals-grid {
        grid-template-columns: 1fr;
      }

      .field--wide {
        grid-column: span 1;
      }

      .encounter-card__header,
      .state-note--error {
        align-items: stretch;
        flex-direction: column;
      }

      .form-status,
      .btn {
        width: 100%;
      }
    }
  `]
})
export class ClinicalEncounterVitalsSectionComponent implements OnChanges {
  @Input() encounters: ClinicalEncounter[] = [];
  @Input() loading = false;
  @Input() missing = false;
  @Input() error: string | null = null;
  @Input() saveError: string | null = null;
  @Input() saving = false;
  @Input() canWrite = false;
  @Input() revision = 0;

  @Output() saved = new EventEmitter<AddClinicalEncounterRequest>();
  @Output() retryRequested = new EventEmitter<void>();

  readonly consultationTypes: ClinicalEncounterConsultationType[] = ['Treatment', 'Urgency', 'Other'];
  readonly loadingCards = [0, 1];
  readonly form: EncounterVitalsForm = new FormGroup({
    occurredAtLocal: new FormControl(this.defaultLocalDateTime(), {
      nonNullable: true,
      validators: [Validators.required]
    }),
    chiefComplaint: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(500)]
    }),
    consultationType: new FormControl<ClinicalEncounterConsultationType>('Treatment', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    temperatureC: new FormControl<number | null>(null, [Validators.min(30), Validators.max(45)]),
    bloodPressureSystolic: new FormControl<number | null>(null, [Validators.min(50), Validators.max(260)]),
    bloodPressureDiastolic: new FormControl<number | null>(null, [Validators.min(30), Validators.max(180)]),
    weightKg: new FormControl<number | null>(null, [Validators.min(0.5), Validators.max(500)]),
    heightCm: new FormControl<number | null>(null, [Validators.min(30), Validators.max(250)]),
    respiratoryRatePerMinute: new FormControl<number | null>(null, [Validators.min(5), Validators.max(80)]),
    heartRateBpm: new FormControl<number | null>(null, [Validators.min(20), Validators.max(240)]),
    noteText: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(2000)]
    })
  }, { validators: [this.bloodPressureValidator] });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['revision'] && !changes['revision'].firstChange) {
      this.resetForm();
    }

    if (changes['canWrite'] || changes['saving'] || changes['revision']) {
      this.syncFormState();
    }
  }

  submit(): void {
    if (!this.canWrite || this.saving) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saved.emit(this.buildPayload());
  }

  cancel(): void {
    this.resetForm();
    this.syncFormState();
  }

  hasControlError(controlName: EncounterVitalsControlName, errorKey: string): boolean {
    const control = this.form.controls[controlName];

    return control.hasError(errorKey) && (control.dirty || control.touched);
  }

  hasBloodPressureError(errorKey: 'bloodPressurePair' | 'bloodPressureOrder'): boolean {
    const systolic = this.form.controls.bloodPressureSystolic;
    const diastolic = this.form.controls.bloodPressureDiastolic;

    return this.form.hasError(errorKey) && (systolic.dirty || systolic.touched || diastolic.dirty || diastolic.touched);
  }

  hasVitals(encounter: ClinicalEncounter): boolean {
    return encounter.temperatureC !== null ||
      encounter.bloodPressureSystolic !== null ||
      encounter.bloodPressureDiastolic !== null ||
      encounter.weightKg !== null ||
      encounter.heightCm !== null ||
      encounter.respiratoryRatePerMinute !== null ||
      encounter.heartRateBpm !== null;
  }

  consultationTypeTone(consultationType: ClinicalEncounterConsultationType): StatusBadgeTone {
    if (consultationType === 'Urgency') {
      return 'warning';
    }

    if (consultationType === 'Treatment') {
      return 'primary';
    }

    return 'neutral';
  }

  private buildPayload(): AddClinicalEncounterRequest {
    return {
      occurredAtUtc: new Date(this.form.controls.occurredAtLocal.value).toISOString(),
      chiefComplaint: this.form.controls.chiefComplaint.value.trim(),
      consultationType: this.form.controls.consultationType.value,
      temperatureC: this.normalizeNumber(this.form.controls.temperatureC.value),
      bloodPressureSystolic: this.normalizeNumber(this.form.controls.bloodPressureSystolic.value),
      bloodPressureDiastolic: this.normalizeNumber(this.form.controls.bloodPressureDiastolic.value),
      weightKg: this.normalizeNumber(this.form.controls.weightKg.value),
      heightCm: this.normalizeNumber(this.form.controls.heightCm.value),
      respiratoryRatePerMinute: this.normalizeNumber(this.form.controls.respiratoryRatePerMinute.value),
      heartRateBpm: this.normalizeNumber(this.form.controls.heartRateBpm.value),
      noteText: this.normalizeOptional(this.form.controls.noteText.value)
    };
  }

  private resetForm(): void {
    this.form.reset({
      occurredAtLocal: this.defaultLocalDateTime(),
      chiefComplaint: '',
      consultationType: 'Treatment',
      temperatureC: null,
      bloodPressureSystolic: null,
      bloodPressureDiastolic: null,
      weightKg: null,
      heightCm: null,
      respiratoryRatePerMinute: null,
      heartRateBpm: null,
      noteText: ''
    });
    this.form.markAsPristine();
    this.form.markAsUntouched();
  }

  private syncFormState(): void {
    if (!this.canWrite || this.saving) {
      this.form.disable({ emitEvent: false });
      return;
    }

    this.form.enable({ emitEvent: false });
  }

  private normalizeOptional(value: string): string | null {
    const normalized = value.trim();

    return normalized ? normalized : null;
  }

  private normalizeNumber<T extends number | null>(value: T): T {
    return value === null ? value : Number(value) as T;
  }

  private defaultLocalDateTime(): string {
    const now = new Date();
    const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60000);

    return localDate.toISOString().slice(0, 16);
  }

  private bloodPressureValidator(control: AbstractControl): ValidationErrors | null {
    const form = control as EncounterVitalsForm;
    const systolic = form.controls.bloodPressureSystolic.value;
    const diastolic = form.controls.bloodPressureDiastolic.value;

    if ((systolic === null) !== (diastolic === null)) {
      return { bloodPressurePair: true };
    }

    if (systolic !== null && diastolic !== null && diastolic >= systolic) {
      return { bloodPressureOrder: true };
    }

    return null;
  }
}
