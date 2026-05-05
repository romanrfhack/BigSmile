import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import { AppointmentBlockFormValue } from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-block-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="block-form" [formGroup]="form" (ngSubmit)="submit()">
      <header class="form-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 2 / {{ 'Scheduling' | t }}</p>
          <h3>{{ 'Block time slot' | t }}</h3>
          <p class="subtitle">
            {{ 'Reserve operational time in the selected branch without creating a patient appointment.' | t }}
          </p>
        </div>
        <button type="button" class="text-button" (click)="cancelled.emit()">{{ 'Reset' | t }}</button>
      </header>

      <div class="branch-banner">
        <span>{{ 'Branch' | t }}</span>
        <strong>{{ selectedBranchName || ('No branch selected' | t) }}</strong>
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

      <label class="field">
        <span>{{ 'Label' | t }}</span>
        <input
          type="text"
          formControlName="label"
          maxlength="200"
          [placeholder]="'Lunch break, maintenance, team meeting...' | t" />
      </label>

      <div *ngIf="error" class="form-error">{{ error | t }}</div>
      <div *ngIf="form.errors?.['timeRangeInvalid']" class="form-error">
        {{ 'End time must be after the start time.' | t }}
      </div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ (saving ? 'Saving...' : 'Create blocked slot') | t }}
        </button>
        <button type="button" class="btn btn-secondary" (click)="cancelled.emit()" [disabled]="saving">
          {{ 'Cancel' | t }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .block-form {
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

    .branch-banner {
      border-radius: var(--bsm-radius-md);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-surface);
      padding: 0.9rem 1rem;
    }

    .branch-banner span {
      display: block;
      font-size: 0.78rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-text-muted);
      margin-bottom: 0.25rem;
    }

    .branch-banner strong {
      color: var(--bsm-color-text-brand);
    }

    .field {
      display: grid;
      gap: 0.45rem;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
    }

    .field input {
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

    .field input:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .time-grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    }

    .form-error {
      border-radius: 14px;
      border: 1px solid #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
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
      background: var(--bsm-color-accent-dark);
      color: #ffffff;
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
export class AppointmentBlockFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() selectedBranchName = '';
  @Input() draftDate = '';
  @Input() revision = 0;
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() saved = new EventEmitter<AppointmentBlockFormValue>();
  @Output() cancelled = new EventEmitter<void>();

  readonly form = this.formBuilder.group({
    startsAt: ['', Validators.required],
    endsAt: ['', Validators.required],
    label: ['', Validators.maxLength(200)]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['draftDate'] || changes['revision']) {
      this.configureForm();
    }
  }

  submit(): void {
    this.form.setErrors(null);

    const startsAt = this.form.controls.startsAt.value ?? '';
    const endsAt = this.form.controls.endsAt.value ?? '';

    if (!startsAt || !endsAt || endsAt <= startsAt) {
      this.form.setErrors({ timeRangeInvalid: true });
      return;
    }

    this.saved.emit({
      startsAt,
      endsAt,
      label: normalizeOptional(this.form.controls.label.value)
    });
  }

  private configureForm(): void {
    const defaultStart = toLocalDateTime(this.draftDate ? new Date(`${this.draftDate}T13:00:00`) : new Date());
    const defaultEnd = toLocalDateTime(this.draftDate ? new Date(`${this.draftDate}T14:00:00`) : new Date(Date.now() + 60 * 60 * 1000));

    this.form.reset({
      startsAt: defaultStart,
      endsAt: defaultEnd,
      label: ''
    });
  }
}

function normalizeOptional(value: string | null | undefined): string | null {
  const normalized = value?.trim();
  return normalized ? normalized : null;
}

function toLocalDateTime(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  const hours = `${date.getHours()}`.padStart(2, '0');
  const minutes = `${date.getMinutes()}`.padStart(2, '0');
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}
