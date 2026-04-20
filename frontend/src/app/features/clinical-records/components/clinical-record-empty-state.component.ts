import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-clinical-record-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="empty-state">
      <p class="eyebrow">Release 3.1 / Clinical Record Foundation</p>
      <h3>No clinical record yet</h3>
      <p>
        This patient does not have a clinical record yet. Creation is explicit in this slice and must be started manually.
      </p>

      <button
        *ngIf="canWrite"
        type="button"
        class="btn btn-primary"
        (click)="createRequested.emit()">
        Create clinical record
      </button>

      <p *ngIf="!canWrite" class="muted">
        Your current session can read clinical data but cannot create or update the record.
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
export class ClinicalRecordEmptyStateComponent {
  @Input() canWrite = false;

  @Output() createRequested = new EventEmitter<void>();
}
