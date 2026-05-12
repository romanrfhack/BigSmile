import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import { AddClinicalNoteRequest } from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-note-create-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="note-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="form-head">
        <div>
          <p class="eyebrow">{{ 'Append-only notes' | t }}</p>
          <h3>{{ 'Add clinical note' | t }}</h3>
        </div>
      </div>

      <label>
        <span>{{ 'Clinical note' | t }}</span>
        <textarea rows="4" formControlName="noteText"></textarea>
        <small *ngIf="form.controls.noteText.hasError('required') && form.controls.noteText.touched">
          {{ 'Clinical note text is required.' | t }}
        </small>
      </label>

      <div *ngIf="error" class="error-banner">{{ error | t }}</div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ (saving ? 'Saving...' : 'Add note') | t }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .note-form {
      display: grid;
      gap: 1rem;
      border-radius: 20px;
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-gradient-surface);
      padding: 1.4rem 1.5rem;
      box-shadow: var(--bsm-shadow-sm);
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

    textarea {
      border: 1px solid var(--bsm-color-border);
      border-radius: 12px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: var(--bsm-color-text-brand);
      background: var(--bsm-color-bg);
      resize: vertical;
      min-height: 110px;
    }

    .error-banner {
      padding: 0.85rem 1rem;
      border-radius: 12px;
      border: 1px solid var(--bsm-color-danger-border);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger-text);
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
      color: var(--bsm-color-bg);
    }

    small {
      color: var(--bsm-color-danger-text);
      font-weight: 600;
    }
  `]
})
export class ClinicalNoteCreateFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() saving = false;
  @Input() error: string | null = null;
  @Input() revision = 0;

  @Output() saved = new EventEmitter<AddClinicalNoteRequest>();

  readonly form = this.formBuilder.group({
    noteText: ['', [Validators.required, Validators.maxLength(2000)]]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['revision'] && !changes['revision'].firstChange) {
      this.form.reset({ noteText: '' });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const noteText = `${this.form.controls.noteText.value ?? ''}`.trim();
    if (!noteText) {
      this.form.controls.noteText.setErrors({ required: true });
      this.form.controls.noteText.markAsTouched();
      return;
    }

    this.saved.emit({ noteText });
  }
}
