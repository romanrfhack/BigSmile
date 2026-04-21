import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TreatmentQuoteStatus } from '../models/treatment-quote.models';

@Component({
  selector: 'app-treatment-quote-status-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form class="status-shell" [formGroup]="form" (ngSubmit)="submit()">
      <div class="section-head">
        <div>
          <p class="eyebrow">Quote status</p>
          <h3>Commercial lifecycle</h3>
          <p class="copy">Draft, Proposed, and Accepted only in Release 5.2.</p>
        </div>
      </div>

      <label>
        <span>Status</span>
        <select formControlName="status">
          <option *ngFor="let status of availableStatuses" [value]="status">{{ status }}</option>
        </select>
      </label>

      <p *ngIf="currentStatus === 'Accepted'" class="muted">
        Accepted quotes are read-only in this slice. Advanced approvals, billing, taxes, and discounts stay outside Release 5.2.
      </p>

      <div class="actions">
        <button type="submit" class="btn btn-primary" [disabled]="saving || !canWrite || !canChange">
          {{ saving ? 'Saving...' : 'Update status' }}
        </button>
      </div>
    </form>
  `,
  styles: [`
    .status-shell {
      display: grid;
      gap: 1rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
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

    .copy,
    .muted {
      margin: 0;
      color: #5b6e84;
    }

    label {
      display: grid;
      gap: 0.45rem;
      color: #16324f;
      font-weight: 600;
      max-width: 260px;
    }

    select {
      border: 1px solid #c8d4df;
      border-radius: 12px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      color: #16324f;
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
      background: #0a5bb5;
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
