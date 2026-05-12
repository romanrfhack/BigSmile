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
      border: 1px dashed var(--bsm-color-border);
      background: var(--bsm-color-neutral-soft);
      padding: 1rem;
      color: var(--bsm-color-text-muted);
    }

    .history-card {
      display: grid;
      gap: 0.8rem;
      border-radius: 16px;
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
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
      color: var(--bsm-color-text-muted);
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
      background: var(--bsm-color-neutral-soft);
      color: var(--bsm-color-text-muted);
    }

    .entry-background {
      background: var(--bsm-color-info-soft);
      color: var(--bsm-color-info-text);
    }

    .entry-medications {
      background: var(--bsm-color-warning-soft);
      color: var(--bsm-color-warning-text);
    }

    .entry-allergies {
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger-text);
    }

    strong {
      color: var(--bsm-color-text-brand);
      font-size: 1rem;
    }

    .section-copy {
      margin: 0;
      color: var(--bsm-color-text);
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
