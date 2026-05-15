import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AbstractControl, FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-clinical-allergies-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  template: `
    <section class="allergies-section">
      <div class="section-head">
        <div>
          <h3>{{ 'Current allergies' | t }}</h3>
          <p>{{ 'Keep only current, clinically relevant allergies here.' | t }}</p>
        </div>
        <button type="button" class="btn btn-secondary" (click)="addRequested.emit()">{{ 'Add allergy' | t }}</button>
      </div>

      <div *ngIf="!allergies.length" class="empty-copy">
        {{ 'No allergies are currently recorded.' | t }}
      </div>

      <div class="allergy-list">
        <div *ngFor="let allergy of allergies.controls; let index = index" class="allergy-card" [formGroup]="asFormGroup(allergy)">
          <label>
            <span>{{ 'Substance' | t }}</span>
            <input type="text" formControlName="substance" />
            <small *ngIf="allergy.get('substance')?.hasError('required') && allergy.get('substance')?.touched">
              {{ 'Substance is required.' | t }}
            </small>
          </label>

          <label>
            <span>{{ 'Reaction summary' | t }}</span>
            <input type="text" formControlName="reactionSummary" />
          </label>

          <label class="field-wide">
            <span>{{ 'Notes' | t }}</span>
            <textarea rows="2" formControlName="notes"></textarea>
          </label>

          <button type="button" class="remove-link" (click)="removeRequested.emit(index)">{{ 'Remove' | t }}</button>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .allergies-section {
      display: grid;
      gap: 0.9rem;
    }

    .section-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .section-head h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    .section-head p,
    .empty-copy {
      margin: 0.35rem 0 0;
      color: var(--bsm-color-text-muted);
    }

    .allergy-list {
      display: grid;
      gap: 0.9rem;
    }

    .allergy-card {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      border-radius: var(--bsm-radius-sm);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
      padding: 1rem;
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
      background: var(--bsm-color-bg);
    }

    textarea {
      resize: vertical;
      min-height: 84px;
    }

    .field-wide {
      grid-column: 1 / -1;
    }

    .remove-link {
      justify-self: start;
      border: none;
      padding: 0;
      background: none;
      color: var(--bsm-color-danger-text);
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn {
      border: none;
      border-radius: 999px;
      padding: 0.8rem 1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-text-brand);
    }

    small {
      color: var(--bsm-color-danger-text);
      font-weight: 600;
    }

    @media (max-width: 768px) {
      .section-head {
        flex-direction: column;
      }
    }
  `]
})
export class ClinicalAllergiesEditorComponent {
  @Input({ required: true }) allergies!: FormArray;

  @Output() addRequested = new EventEmitter<void>();
  @Output() removeRequested = new EventEmitter<number>();

  asFormGroup(control: AbstractControl): FormGroup {
    return control as FormGroup;
  }
}
