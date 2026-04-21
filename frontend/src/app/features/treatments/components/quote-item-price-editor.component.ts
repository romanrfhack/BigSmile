import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-quote-item-price-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form class="price-editor" [formGroup]="form" (ngSubmit)="submit()">
      <label>
        <span>Unit price ({{ currencyCode }})</span>
        <input type="number" min="0" step="0.01" formControlName="unitPrice" [readOnly]="!canEdit" />
      </label>

      <button type="submit" class="btn btn-primary" [disabled]="saving || !canEdit || !canSave">
        {{ saving ? 'Saving...' : 'Update price' }}
      </button>
    </form>
  `,
  styles: [`
    .price-editor {
      display: grid;
      gap: 0.75rem;
      align-items: end;
      grid-template-columns: minmax(0, 220px) auto;
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

    .btn {
      border: none;
      border-radius: 999px;
      padding: 0.8rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
      justify-self: start;
    }

    .btn-primary {
      background: #0a5bb5;
      color: #ffffff;
    }

    @media (max-width: 768px) {
      .price-editor {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class QuoteItemPriceEditorComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() unitPrice = 0;
  @Input() currencyCode = 'MXN';
  @Input() canEdit = false;
  @Input() saving = false;

  @Output() priceSaved = new EventEmitter<number>();

  readonly form = this.formBuilder.group({
    unitPrice: [0, [Validators.required, Validators.min(0)]]
  });

  get canSave(): boolean {
    const unitPrice = Number(this.form.controls.unitPrice.value ?? NaN);
    return Number.isFinite(unitPrice) && unitPrice >= 0 && unitPrice !== this.unitPrice;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['unitPrice']) {
      this.form.patchValue({ unitPrice: this.unitPrice }, { emitEvent: false });
    }
  }

  submit(): void {
    if (this.form.invalid || !this.canEdit || !this.canSave) {
      this.form.markAllAsTouched();
      return;
    }

    const unitPrice = Number(this.form.controls.unitPrice.value ?? NaN);
    if (!Number.isFinite(unitPrice) || unitPrice < 0) {
      this.form.controls.unitPrice.setErrors({ min: true });
      this.form.controls.unitPrice.markAsTouched();
      return;
    }

    this.priceSaved.emit(unitPrice);
  }
}
