import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  OdontogramSurfaceCode,
  OdontogramSurfaceFindingHistoryEntry,
  OdontogramSurfaceFindingHistoryEntryType
} from '../models/odontogram.models';

@Component({
  selector: 'app-surface-finding-history-list',
  standalone: true,
  imports: [CommonModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="history-list">
      <div *ngIf="!filteredEntries.length" class="empty-copy">
        {{ 'No finding history entries are available for this surface yet. Findings created before Release 4.4 may not have a previous added entry because this slice does not backfill history.' | t }}
      </div>

      <article *ngFor="let entry of filteredEntries" class="history-card" [attr.data-entry-type]="entry.entryType">
        <header class="history-head">
          <div class="history-labels">
            <span
              class="entry-badge"
              [class.entry-added]="entry.entryType === 'FindingAdded'"
              [class.entry-removed]="entry.entryType === 'FindingRemoved'">
              {{ getEntryLabel(entry.entryType) }}
            </span>
            <strong>{{ entry.summary }}</strong>
          </div>

          <div class="history-meta">
            <span>{{ entry.changedAtUtc | bsDate: 'medium' }}</span>
            <span>{{ 'User' | t }} {{ entry.changedByUserId }}</span>
          </div>
        </header>

        <p class="history-copy">
          {{ 'Tooth' | t }} {{ entry.toothCode }} / {{ 'Surface' | t }} {{ entry.surfaceCode }} / {{ getFindingTypeLabel(entry.findingType) }}
        </p>
      </article>
    </section>
  `,
  styles: [`
    .history-list {
      display: grid;
      gap: 0.9rem;
    }

    .empty-copy {
      border-radius: 16px;
      border: 1px dashed #c7d4e0;
      background: #f7fafc;
      padding: 1rem;
      color: #607387;
    }

    .history-card {
      display: grid;
      gap: 0.75rem;
      border-radius: 16px;
      border: 1px solid #d7dfe8;
      background: #ffffff;
      padding: 1rem;
    }

    .history-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
      align-items: flex-start;
    }

    .history-labels {
      display: grid;
      gap: 0.45rem;
    }

    .entry-badge {
      display: inline-flex;
      width: fit-content;
      align-items: center;
      justify-content: center;
      border-radius: 999px;
      padding: 0.35rem 0.7rem;
      font-size: 0.78rem;
      font-weight: 700;
      letter-spacing: 0.04em;
      text-transform: uppercase;
    }

    .entry-added {
      background: #e8f4ea;
      color: #1d6a2e;
    }

    .entry-removed {
      background: #fff0ef;
      color: #9b2c23;
    }

    strong {
      color: #16324f;
      font-size: 1rem;
    }

    .history-meta {
      display: grid;
      gap: 0.2rem;
      justify-items: end;
      color: #607387;
      font-size: 0.95rem;
      text-align: right;
    }

    .history-copy {
      margin: 0;
      color: #35506d;
    }

    @media (max-width: 768px) {
      .history-meta {
        justify-items: start;
        text-align: left;
      }
    }
  `]
})
export class SurfaceFindingHistoryListComponent {
  private readonly i18n = inject(I18nService);
  @Input() entries: OdontogramSurfaceFindingHistoryEntry[] = [];
  @Input() toothCode: string | null = null;
  @Input() surfaceCode: OdontogramSurfaceCode | null = null;

  get filteredEntries(): OdontogramSurfaceFindingHistoryEntry[] {
    if (!this.toothCode || !this.surfaceCode) {
      return [];
    }

    return [...this.entries]
      .filter((entry) => entry.toothCode === this.toothCode && entry.surfaceCode === this.surfaceCode)
      .sort((left, right) => {
        const changedAtOrder = Date.parse(right.changedAtUtc) - Date.parse(left.changedAtUtc);
        if (changedAtOrder !== 0) {
          return changedAtOrder;
        }

        const referenceOrder = (right.referenceFindingId ?? '').localeCompare(left.referenceFindingId ?? '');
        if (referenceOrder !== 0) {
          return referenceOrder;
        }

        return right.entryType.localeCompare(left.entryType);
      });
  }

  getEntryLabel(entryType: OdontogramSurfaceFindingHistoryEntryType): string {
    switch (entryType) {
      case 'FindingAdded':
        return this.i18n.translate('Finding added');
      case 'FindingRemoved':
        return this.i18n.translate('Finding removed');
    }
  }

  getFindingTypeLabel(findingType: OdontogramSurfaceFindingHistoryEntry['findingType']): string {
    return this.i18n.translate(findingType);
  }
}
