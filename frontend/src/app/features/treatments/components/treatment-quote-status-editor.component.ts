import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import { TreatmentQuoteStatus } from '../models/treatment-quote.models';

@Component({
  selector: 'app-treatment-quote-status-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="status-shell" [formGroup]="form" (ngSubmit)="submit()">
      <div class="section-head">
        <div>
          <p class="eyebrow">{{ 'Quote status' | t }}</p>
          <h3>{{ 'Commercial lifecycle' | t }}</h3>
          <p class="copy">{{ 'Draft, Proposed, and Accepted only in Release 5.2.' | t }}</p>
        </div>
      </div>

      <label>
        <span>{{ 'Status' | t }}</span>
        <select formControlName="status">
          <option *ngFor="let status of availableStatuses" [value]="status">{{ status | t }}</option>
        </select>
      </label>

      <p *ngIf="currentStatus === 'Accepted'" class="muted">
        {{ 'Accepted quotes are read-only in this slice. Advanced approvals, billing, taxes, and discounts stay outside Release 5.2.' | t }}
      </p>

      <div class="actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving || !canWrite || !canChange">
          {{ (saving ? 'Saving...' : 'Update status') | t }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .status-shell {
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

    .copy,
    .muted {
      margin: 0;
      color: var(--bsm-color-text-muted);
    }

    label {
      display: grid;
      gap: 0.45rem;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
      max-width: 260px;
    }

    select {
      border: 1px solid var(--bsm-color-border);
      border-radius: 12px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: var(--bsm-color-text-brand);
      background: #ffffff;
    }

    .actions {
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
  `]
})
export class TreatmentQuoteStatusEditorComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() currentStatus: TreatmentQuoteStatus = 'Draft';
  @Input() availableStatuses: TreatmentQuoteStatus[] = ['Draft'];
  @Input() canWrite = false;
  @Input() saving = false;

  @Output() statusChanged = new EventEmitter<TreatmentQuoteStatus>();

  readonly form = this.formBuilder.group({
    status: ['Draft', Validators.required]
  });

  get canChange(): boolean {
    return this.form.controls.status.value !== this.currentStatus && this.availableStatuses.length > 0;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['currentStatus'] || changes['availableStatuses']) {
      this.form.patchValue({ status: this.currentStatus }, { emitEvent: false });
    }
  }

  submit(): void {
    if (this.form.invalid || !this.canWrite || !this.canChange) {
      return;
    }

    this.statusChanged.emit(this.form.controls.status.value as TreatmentQuoteStatus);
  }
}
