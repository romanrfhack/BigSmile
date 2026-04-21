import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-treatment-quote-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="empty-state">
      <p class="eyebrow">Release 5.2 / Quote Basics</p>
      <h3>No quote yet</h3>
      <p>
        This patient already has a treatment plan, but the commercial quote is not auto-created in this slice.
        Create it explicitly to snapshot the current treatment plan items into a fixed-currency quote.
      </p>

      <button
        *ngIf="canWrite"
        type="button"
        class="btn btn-primary"
        [disabled]="creating"
        (click)="createRequested.emit()">
        {{ creating ? 'Creating...' : 'Create quote from treatment plan' }}
      </button>

      <p *ngIf="!canWrite" class="muted">
        Your current session can read this patient context, but only roles with treatment quote write permission can initialize the quote.
      </p>
    </section>
  `,
  styles: [`
    .empty-state {
      display: grid;
      gap: 0.85rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .eyebrow {
      margin: 0;
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

    p {
      margin: 0;
      color: #5b6e84;
      max-width: 58ch;
    }

    .muted {
      color: #7a8ea3;
    }

    .btn {
      justify-self: start;
      border: none;
      border-radius: 999px;
      padding: 0.85rem 1.1rem;
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
export class TreatmentQuoteEmptyStateComponent {
  @Input() canWrite = false;
  @Input() creating = false;

  @Output() createRequested = new EventEmitter<void>();
}
