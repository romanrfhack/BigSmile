import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '../../../shared/i18n';
import { BillingDocumentStatus } from '../models/billing-document.models';

@Component({
  selector: 'app-billing-document-status-editor',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="status-shell">
      <div class="section-head">
        <div>
          <p class="eyebrow">{{ 'Billing status' | t }}</p>
          <h3>{{ 'Commercial issue step' | t }}</h3>
          <p class="copy">{{ 'Draft and Issued only in Release 6.1.' | t }}</p>
        </div>
      </div>

      <p class="status-copy">
        {{ 'Current status:' | t }} <strong>{{ currentStatus | t }}</strong>
      </p>

      <button
        *ngIf="currentStatus === 'Draft'"
        type="button"
        class="btn btn-primary"
        [disabled]="saving || !canWrite"
        (click)="issueRequested.emit()">
        {{ (saving ? 'Issuing...' : 'Issue billing document') | t }}
      </button>

      <p *ngIf="currentStatus === 'Draft' && !canWrite" class="muted">
        {{ 'Only roles with billing write permission can issue this billing document.' | t }}
      </p>

      <p *ngIf="currentStatus === 'Issued'" class="muted">
        {{ 'Issued billing documents are read-only in this slice. Payments, taxes, discounts, CFDI, and advanced billing workflows remain outside Release 6.1.' | t }}
      </p>
    </section>
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
    .status-copy,
    .muted {
      margin: 0;
      color: #5b6e84;
    }

    strong {
      color: #16324f;
    }

    .btn {
      justify-self: start;
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
export class BillingDocumentStatusEditorComponent {
  @Input() currentStatus: BillingDocumentStatus = 'Draft';
  @Input() canWrite = false;
  @Input() saving = false;

  @Output() issueRequested = new EventEmitter<void>();
}
