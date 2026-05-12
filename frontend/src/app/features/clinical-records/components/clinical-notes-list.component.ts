import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { ClinicalNote } from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-notes-list',
  standalone: true,
  imports: [CommonModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="notes-list">
      <div *ngIf="!sortedNotes.length" class="empty-copy">
        {{ 'No clinical notes have been added yet.' | t }}
      </div>

      <article *ngFor="let note of sortedNotes" class="note-card">
        <header>
          <strong>{{ note.createdAtUtc | bsDate: 'medium' }}</strong>
          <span>{{ 'User' | t }} {{ note.createdByUserId }}</span>
        </header>
        <p>{{ note.noteText }}</p>
      </article>
    </section>
  `,
  styles: [`
    .notes-list {
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

    .note-card {
      display: grid;
      gap: 0.55rem;
      border-radius: 16px;
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
      padding: 1rem;
    }

    header {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
      color: var(--bsm-color-text-brand);
    }

    header span {
      color: var(--bsm-color-text-muted);
      font-size: 0.95rem;
    }

    p {
      margin: 0;
      color: var(--bsm-color-text);
      white-space: pre-wrap;
    }
  `]
})
export class ClinicalNotesListComponent {
  @Input() notes: ClinicalNote[] = [];

  get sortedNotes(): ClinicalNote[] {
    return [...this.notes].sort((left, right) => Date.parse(right.createdAtUtc) - Date.parse(left.createdAtUtc));
  }
}
