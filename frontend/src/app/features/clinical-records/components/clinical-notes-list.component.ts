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
      border: 1px dashed #c7d4e0;
      background: #f7fafc;
      padding: 1rem;
      color: #607387;
    }

    .note-card {
      display: grid;
      gap: 0.55rem;
      border-radius: 16px;
      border: 1px solid #d7dfe8;
      background: #ffffff;
      padding: 1rem;
    }

    header {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
      color: #16324f;
    }

    header span {
      color: #607387;
      font-size: 0.95rem;
    }

    p {
      margin: 0;
      color: #35506d;
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
