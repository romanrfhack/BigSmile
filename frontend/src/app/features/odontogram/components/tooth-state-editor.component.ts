import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ODONTOGRAM_TOOTH_STATUSES,
  OdontogramToothState,
  OdontogramToothStatus
} from '../models/odontogram.models';

@Component({
  selector: 'app-tooth-state-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="editor-card">
      <ng-container *ngIf="tooth; else noSelection">
        <p class="eyebrow">Selected tooth</p>
        <h3>Tooth {{ tooth.toothCode }}</h3>
        <p class="copy">Minimal tooth-level state only. No surfaces, findings bundles, treatment linkage, or dental timeline in Release 4.1.</p>

        <div class="meta-grid">
          <div>
            <dt>Current status</dt>
            <dd>{{ tooth.status }}</dd>
          </div>
          <div>
            <dt>Last updated</dt>
            <dd>{{ tooth.updatedAtUtc | date: 'medium' }}</dd>
          </div>
          <div>
            <dt>Updated by</dt>
            <dd>{{ tooth.updatedByUserId }}</dd>
          </div>
        </div>

        <div *ngIf="canWrite" class="editor-actions">
          <label class="field">
            <span>New status</span>
            <select [(ngModel)]="selectedStatus">
              <option *ngFor="let status of statuses" [ngValue]="status">{{ status }}</option>
            </select>
          </label>

          <button
            type="button"
            class="primary-action"
            [disabled]="saving || selectedStatus === tooth.status"
            (click)="updateRequested.emit(selectedStatus)">
            {{ saving ? 'Saving...' : 'Update tooth state' }}
          </button>
        </div>

        <p *ngIf="error" class="error-copy">{{ error }}</p>
      </ng-container>

      <ng-template #noSelection>
        <p class="muted">Select a tooth to inspect or update its current state.</p>
      </ng-template>
    </section>
  `,
  styles: [`
    .editor-card {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      display: grid;
      gap: 1rem;
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

    .copy,
    .muted {
      margin: 0;
      color: #5b6e84;
    }

    .meta-grid {
      display: grid;
      gap: 0.9rem;
      grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #718298;
    }

    dd {
      margin: 0;
      color: #16324f;
      font-weight: 600;
      word-break: break-word;
    }

    .editor-actions {
      display: flex;
      gap: 0.9rem;
      align-items: end;
      flex-wrap: wrap;
    }

    .field {
      display: grid;
      gap: 0.35rem;
      min-width: 220px;
      color: #16324f;
      font-weight: 600;
    }

    select {
      border-radius: 12px;
      border: 1px solid #c7d3e0;
      padding: 0.75rem 0.85rem;
      font: inherit;
      background: #ffffff;
    }

    .primary-action {
      border: none;
      border-radius: 999px;
      padding: 0.85rem 1.2rem;
      background: #0a5bb5;
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    .primary-action:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .error-copy {
      margin: 0;
      color: #8c2525;
      font-weight: 600;
    }
  `]
})
export class ToothStateEditorComponent implements OnChanges {
  @Input() tooth: OdontogramToothState | null = null;
  @Input() canWrite = false;
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() readonly updateRequested = new EventEmitter<OdontogramToothStatus>();

  readonly statuses = ODONTOGRAM_TOOTH_STATUSES;
  selectedStatus: OdontogramToothStatus = 'Unknown';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['tooth'] && this.tooth) {
      this.selectedStatus = this.tooth.status;
    }
  }
}
