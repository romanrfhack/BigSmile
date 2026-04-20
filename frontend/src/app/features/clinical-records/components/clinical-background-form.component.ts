import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ClinicalRecord, SaveClinicalRecordSnapshotRequest } from '../models/clinical-record.models';
import { ClinicalAllergiesEditorComponent } from './clinical-allergies-editor.component';

@Component({
  selector: 'app-clinical-background-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ClinicalAllergiesEditorComponent],
  template: `
    <form class="clinical-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="section-head">
        <div>
          <p class="eyebrow">Clinical snapshot</p>
          <h3>{{ mode === 'edit' ? 'Update clinical snapshot' : 'Create clinical record' }}</h3>
          <p>Background, current medications, and current allergies only.</p>
        </div>
      </div>

      <section class="form-section">
        <div class="field-grid">
          <label class="field-wide">
            <span>Medical background summary</span>
            <textarea rows="4" formControlName="medicalBackgroundSummary"></textarea>
          </label>

          <label class="field-wide">
            <span>Current medications summary</span>
            <textarea rows="4" formControlName="currentMedicationsSummary"></textarea>
          </label>
        </div>
      </section>

      <app-clinical-allergies-editor
        [allergies]="allergies"
        (addRequested)="addAllergy()"
        (removeRequested)="removeAllergy($event)">
      </app-clinical-allergies-editor>

      <div *ngIf="error" class="error-banner">{{ error }}</div>

      <div class="form-actions">
        <button type="button" class="btn btn-secondary" (click)="cancel()" [disabled]="saving">
          {{ mode === 'edit' ? 'Reset form' : 'Cancel' }}
        </button>
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ saving ? 'Saving...' : mode === 'edit' ? 'Save clinical snapshot' : 'Create clinical record' }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .clinical-form {
      display: grid;
      gap: 1.25rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f4f8fb 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .section-head h3 {
      margin: 0;
      color: #16324f;
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    .section-head p {
      margin: 0.35rem 0 0;
      color: #5d6d82;
      max-width: 58ch;
    }

    .field-grid {
      display: grid;
      gap: 1rem;
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

    .field-wide {
      grid-column: 1 / -1;
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
  `]
})
export class ClinicalBackgroundFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() initialRecord: ClinicalRecord | null = null;
  @Input() mode: 'create' | 'edit' = 'edit';
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() saved = new EventEmitter<SaveClinicalRecordSnapshotRequest>();
  @Output() cancelled = new EventEmitter<void>();

  readonly form = this.formBuilder.group({
    medicalBackgroundSummary: ['', [Validators.maxLength(2000)]],
    currentMedicationsSummary: ['', [Validators.maxLength(2000)]],
    allergies: this.formBuilder.array([])
  });

  get allergies(): FormArray {
    return this.form.get('allergies') as FormArray;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialRecord']) {
      this.resetForm();
    }
  }

  addAllergy(): void {
    this.allergies.push(this.createAllergyGroup());
  }

  removeAllergy(index: number): void {
    this.allergies.removeAt(index);
  }

  cancel(): void {
    if (this.mode === 'create') {
      this.cancelled.emit();
      return;
    }

    this.resetForm();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const rawAllergies = (raw.allergies ?? []) as Array<{
      substance?: string | null;
      reactionSummary?: string | null;
      notes?: string | null;
    }>;

    const allergies = rawAllergies
      .map((allergy) => ({
        substance: this.normalizeRequired(allergy?.substance),
        reactionSummary: this.normalizeOptional(allergy?.reactionSummary),
        notes: this.normalizeOptional(allergy?.notes)
      }))
      .filter((allergy) => !!allergy.substance);

    this.saved.emit({
      medicalBackgroundSummary: this.normalizeOptional(raw.medicalBackgroundSummary),
      currentMedicationsSummary: this.normalizeOptional(raw.currentMedicationsSummary),
      allergies
    });
  }

  private resetForm(): void {
    this.form.reset({
      medicalBackgroundSummary: this.initialRecord?.medicalBackgroundSummary ?? '',
      currentMedicationsSummary: this.initialRecord?.currentMedicationsSummary ?? ''
    });

    this.allergies.clear();
    for (const allergy of this.initialRecord?.allergies ?? []) {
      this.allergies.push(this.createAllergyGroup(allergy));
    }
  }

  private createAllergyGroup(allergy?: ClinicalRecord['allergies'][number]): FormGroup {
    return this.formBuilder.group({
      substance: [allergy?.substance ?? '', [Validators.required, Validators.maxLength(150)]],
      reactionSummary: [allergy?.reactionSummary ?? '', [Validators.maxLength(500)]],
      notes: [allergy?.notes ?? '', [Validators.maxLength(500)]]
    });
  }

  private normalizeRequired(value: string | null | undefined): string {
    return `${value ?? ''}`.trim();
  }

  private normalizeOptional(value: string | null | undefined): string | null {
    const normalized = `${value ?? ''}`.trim();
    return normalized ? normalized : null;
  }
}
