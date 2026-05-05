import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  ClinicalSnapshotHistoryEntry,
  ClinicalSnapshotHistoryEntryType,
  ClinicalSnapshotHistorySection
} from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-snapshot-history-list',
  standalone: true,
  imports: [CommonModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="snapshot-history-list">
      <div *ngIf="!sortedSnapshotHistory.length" class="empty-copy">
        {{ 'No snapshot history entries are available yet.' | t }}
      </div>

      <article *ngFor="let entry of sortedSnapshotHistory" class="history-card" [attr.data-entry-type]="entry.entryType">
        <header class="history-head">
          <div class="history-labels">
            <span
              class="entry-badge"
              [class.entry-initialized]="entry.entryType === 'SnapshotInitialized'"
              [class.entry-background]="entry.entryType === 'MedicalBackgroundUpdated'"
              [class.entry-medications]="entry.entryType === 'CurrentMedicationsUpdated'"
              [class.entry-allergies]="entry.entryType === 'AllergiesUpdated'">
              {{ getEntryLabel(entry.entryType) }}
            </span>
            <strong>{{ entry.summary }}</strong>
          </div>

          <div class="history-meta">
            <span>{{ entry.changedAtUtc | bsDate: 'medium' }}</span>
            <span>{{ 'User' | t }} {{ entry.changedByUserId }}</span>
          </div>
        </header>

        <p class="section-copy">{{ 'Section:' | t }} {{ getSectionLabel(entry.section) }}</p>
      </article>
    </section>
  `,
  styles: [`
    .snapshot-history-list {
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
      gap: 0.8rem;
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

    .history-meta {
      display: grid;
      gap: 0.2rem;
      justify-items: end;
      color: #607387;
      font-size: 0.95rem;
      text-align: right;
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

    .entry-initialized {
      background: #eef2f6;
      color: #536677;
    }

    .entry-background {
      background: #e8f2ff;
      color: #1f5e9e;
    }

    .entry-medications {
      background: #fff3e3;
      color: #8c4a07;
    }

    .entry-allergies {
      background: #fbeaf4;
      color: #8b2b63;
    }

    strong {
      color: #16324f;
      font-size: 1rem;
    }

    .section-copy {
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
export class ClinicalSnapshotHistoryListComponent {
  private readonly i18n = inject(I18nService);

  @Input() snapshotHistory: ClinicalSnapshotHistoryEntry[] = [];

  get sortedSnapshotHistory(): ClinicalSnapshotHistoryEntry[] {
    return [...this.snapshotHistory].sort((left, right) => {
      const changedAtOrder = Date.parse(right.changedAtUtc) - Date.parse(left.changedAtUtc);
      if (changedAtOrder !== 0) {
        return changedAtOrder;
      }

      return right.entryType.localeCompare(left.entryType);
    });
  }

  getEntryLabel(entryType: ClinicalSnapshotHistoryEntryType): string {
    switch (entryType) {
      case 'SnapshotInitialized':
        return this.i18n.translate('Snapshot initialized');
      case 'MedicalBackgroundUpdated':
        return this.i18n.translate('Medical background');
      case 'CurrentMedicationsUpdated':
        return this.i18n.translate('Current medications');
      case 'AllergiesUpdated':
        return this.i18n.translate('Current allergies');
    }
  }

  getSectionLabel(section: ClinicalSnapshotHistorySection): string {
    switch (section) {
      case 'Initial':
        return this.i18n.translate('Initial snapshot');
      case 'MedicalBackground':
        return this.i18n.translate('Medical background');
      case 'CurrentMedications':
        return this.i18n.translate('Current medications');
      case 'Allergies':
        return this.i18n.translate('Current allergies');
    }
  }
}
