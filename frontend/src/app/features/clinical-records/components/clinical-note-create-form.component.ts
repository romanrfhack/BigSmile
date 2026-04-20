import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AddClinicalNoteRequest } from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-note-create-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form class="note-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="form-head">
        <div>
          <p class="eyebrow">Append-only notes</p>
          <h3>Add clinical note</h3>
        </div>
      </div>

      <label>
        <span>Clinical note</span>
        <textarea rows="4" formControlName="noteText"></textarea>
        <small *ngIf="form.controls.noteText.hasError('required') && form.controls.noteText.touched">
          Clinical note text is required.
        </small>
      </label>

      <div *ngIf="error" class="error-banner">{{ error }}</div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ saving ? 'Saving...' : 'Add note' }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .note-form {
      display: grid;
      gap: 1rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f4f8fb 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: #16324f;
    }

    label {
      display: grid;
      gap: 0.45rem;
      color: #16324f;
      font-weight: 600;
    }

    textarea {
      border: 1px solid #c8d4df;
      border-radius: 12px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: #16324f;
      background: #ffffff;
      resize: vertical;
      min-height: 110px;
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
      background: #0a5bb5;
      color: #ffffff;
    }

    small {
      color: #9b2d30;
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
