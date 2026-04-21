import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AddTreatmentPlanItemRequest,
  TREATMENT_PLAN_SURFACE_CODES,
  TREATMENT_PLAN_TOOTH_CODES
} from '../models/treatment-plan.models';

@Component({
  selector: 'app-treatment-plan-item-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form class="item-form" [formGroup]="form" (ngSubmit)="submit()">
      <div class="form-head">
        <div>
          <p class="eyebrow">Treatment items</p>
          <h3>Add treatment item</h3>
          <p class="copy">Minimal treatment intent only. No pricing, quotes, billing, or scheduling linkage in Release 5.1.</p>
        </div>
      </div>

      <div class="grid">
        <label class="full-width">
          <span>Title</span>
          <input type="text" formControlName="title" />
          <small *ngIf="form.controls.title.hasError('required') && form.controls.title.touched">
            Treatment item title is required.
          </small>
        </label>

        <label>
          <span>Category</span>
          <input type="text" formControlName="category" />
        </label>

        <label>
          <span>Quantity</span>
          <input type="number" min="1" step="1" formControlName="quantity" />
          <small *ngIf="form.controls.quantity.invalid && form.controls.quantity.touched">
            Quantity must be greater than zero.
          </small>
        </label>

        <label>
          <span>Tooth code</span>
          <select formControlName="toothCode">
            <option value="">General / not linked</option>
            <option *ngFor="let toothCode of toothCodes" [value]="toothCode">{{ toothCode }}</option>
          </select>
        </label>

        <label>
          <span>Surface code</span>
          <select formControlName="surfaceCode">
            <option value="">No surface</option>
            <option *ngFor="let surfaceCode of surfaceCodes" [value]="surfaceCode">{{ surfaceCode }}</option>
          </select>
        </label>

        <label class="full-width">
          <span>Short note</span>
          <textarea rows="3" formControlName="notes"></textarea>
        </label>
      </div>

      <div *ngIf="error" class="error-banner">{{ error }}</div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving || disabled">
          {{ saving ? 'Saving...' : 'Add item' }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .item-form {
      display: grid;
      gap: 1rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f4f8fb 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .form-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
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

    .copy {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 62ch;
    }

    .grid {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    }

    .full-width {
      grid-column: 1 / -1;
    }

    label {
      display: grid;
      gap: 0.45rem;
      color: #16324f;
      font-weight: 600;
    }

    input,
    select,
    textarea {
      border: 1px solid #c8d4df;
      border-radius: 12px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: #16324f;
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
      background: #0a5bb5;
      color: #ffffff;
    }

    small {
      color: #9b2d30;
      font-weight: 600;
    }
  `]
})
export class TreatmentPlanItemFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() saving = false;
  @Input() disabled = false;
  @Input() error: string | null = null;
  @Input() revision = 0;

  @Output() saved = new EventEmitter<AddTreatmentPlanItemRequest>();

  readonly toothCodes = TREATMENT_PLAN_TOOTH_CODES;
  readonly surfaceCodes = TREATMENT_PLAN_SURFACE_CODES;

  readonly form = this.formBuilder.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    category: ['', [Validators.maxLength(100)]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    notes: ['', [Validators.maxLength(500)]],
    toothCode: [''],
    surfaceCode: ['']
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['revision'] && !changes['revision'].firstChange) {
      this.form.reset({
        title: '',
        category: '',
        quantity: 1,
        notes: '',
        toothCode: '',
        surfaceCode: ''
      });
    }
  }

  submit(): void {
    if (this.form.invalid || this.disabled) {
      this.form.markAllAsTouched();
      return;
    }

    const title = `${this.form.controls.title.value ?? ''}`.trim();
    if (!title) {
      this.form.controls.title.setErrors({ required: true });
      this.form.controls.title.markAsTouched();
      return;
    }

    const toothCode = `${this.form.controls.toothCode.value ?? ''}`.trim();
    const surfaceCode = `${this.form.controls.surfaceCode.value ?? ''}`.trim();

    if (surfaceCode && !toothCode) {
      this.form.controls.toothCode.setErrors({ required: true });
      this.form.controls.toothCode.markAsTouched();
      return;
    }

    const quantity = Number(this.form.controls.quantity.value ?? 0);
    if (!Number.isFinite(quantity) || quantity <= 0) {
      this.form.controls.quantity.setErrors({ min: true });
      this.form.controls.quantity.markAsTouched();
      return;
    }

    const category = `${this.form.controls.category.value ?? ''}`.trim();
    const notes = `${this.form.controls.notes.value ?? ''}`.trim();

    this.saved.emit({
      title,
      category: category ? category : null,
      quantity,
      notes: notes ? notes : null,
      toothCode: toothCode ? toothCode : null,
      surfaceCode: surfaceCode ? surfaceCode : null
    });
  }
}
