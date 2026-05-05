import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import { AddClinicalDiagnosisRequest } from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-diagnosis-create-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="diagnosis-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="form-head">
        <div>
          <p class="eyebrow">{{ 'Basic diagnoses' | t }}</p>
          <h3>{{ 'Add diagnosis' | t }}</h3>
        </div>
      </div>

      <label>
        <span>{{ 'Diagnosis' | t }}</span>
        <input type="text" formControlName="diagnosisText" />
        <small *ngIf="form.controls.diagnosisText.hasError('required') && form.controls.diagnosisText.touched">
          {{ 'Diagnosis text is required.' | t }}
        </small>
      </label>

      <label>
        <span>{{ 'Brief note' | t }}</span>
        <textarea rows="3" formControlName="notes"></textarea>
      </label>

      <div *ngIf="error" class="error-banner">{{ error | t }}</div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ (saving ? 'Saving...' : 'Add diagnosis') | t }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .diagnosis-form {
      display: grid;
      gap: 1rem;
      border-radius: 20px;
      border: 1px solid var(--bsm-color-border);
      background: linear-gradient(180deg, #ffffff 0%, var(--bsm-color-surface) 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
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
      border-radius: 12px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: var(--bsm-color-text-brand);
      background: #ffffff;
    }

    textarea {
      resize: vertical;
      min-height: 92px;
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
      background: var(--bsm-color-primary);
      color: #ffffff;
    }

    small {
      color: #9b2d30;
      font-weight: 600;
    }
  `]
})
export class ClinicalDiagnosisCreateFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() saving = false;
  @Input() error: string | null = null;
  @Input() revision = 0;

  @Output() saved = new EventEmitter<AddClinicalDiagnosisRequest>();

  readonly form = this.formBuilder.group({
    diagnosisText: ['', [Validators.required, Validators.maxLength(250)]],
    notes: ['', [Validators.maxLength(500)]]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['revision'] && !changes['revision'].firstChange) {
      this.form.reset({ diagnosisText: '', notes: '' });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const diagnosisText = `${this.form.controls.diagnosisText.value ?? ''}`.trim();
    if (!diagnosisText) {
      this.form.controls.diagnosisText.setErrors({ required: true });
      this.form.controls.diagnosisText.markAsTouched();
      return;
    }

    const notes = `${this.form.controls.notes.value ?? ''}`.trim();

    this.saved.emit({
      diagnosisText,
      notes: notes ? notes : null
    });
  }
}
