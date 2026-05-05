import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { ClinicalTimelineEntry, ClinicalTimelineEventType } from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-timeline-list',
  standalone: true,
  imports: [CommonModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="timeline-list">
      <div *ngIf="!sortedTimeline.length" class="empty-copy">
        {{ 'No clinical timeline events are available yet.' | t }}
      </div>

      <article *ngFor="let entry of sortedTimeline" class="timeline-card" [attr.data-event-type]="entry.eventType">
        <header class="timeline-head">
          <div class="timeline-labels">
            <span
              class="event-badge"
              [class.event-note]="entry.eventType === 'ClinicalNoteCreated'"
              [class.event-diagnosis-created]="entry.eventType === 'ClinicalDiagnosisCreated'"
              [class.event-diagnosis-resolved]="entry.eventType === 'ClinicalDiagnosisResolved'">
              {{ getEventLabel(entry.eventType) }}
            </span>
            <strong>{{ entry.title }}</strong>
          </div>

          <div class="timeline-meta">
            <span>{{ entry.occurredAtUtc | bsDate: 'medium' }}</span>
            <span>{{ 'User' | t }} {{ entry.actorUserId }}</span>
          </div>
        </header>

        <p class="summary">{{ entry.summary }}</p>
      </article>
    </section>
  `,
  styles: [`
    .timeline-list {
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

    .timeline-card {
      display: grid;
      gap: 0.8rem;
      border-radius: 16px;
      border: 1px solid var(--bsm-color-border);
      background: #ffffff;
      padding: 1rem;
    }

    .timeline-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
      align-items: flex-start;
    }

    .timeline-labels {
      display: grid;
      gap: 0.45rem;
    }

    .timeline-meta {
      display: grid;
      gap: 0.2rem;
      justify-items: end;
      color: #607387;
      font-size: 0.95rem;
      text-align: right;
    }

    .event-badge {
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

    .event-note {
      background: #e8f2ff;
      color: #1f5e9e;
    }

    .event-diagnosis-created {
      background: #e8f4ea;
      color: #22663a;
    }

    .event-diagnosis-resolved {
      background: #eef2f6;
      color: #536677;
    }

    strong {
      color: var(--bsm-color-text-brand);
      font-size: 1rem;
    }

    .summary {
      margin: 0;
      color: #35506d;
      white-space: pre-wrap;
    }

    @media (max-width: 768px) {
      .timeline-meta {
        justify-items: start;
        text-align: left;
      }
    }
  `]
})
export class ClinicalTimelineListComponent {
  private readonly i18n = inject(I18nService);

  @Input() timeline: ClinicalTimelineEntry[] = [];

  get sortedTimeline(): ClinicalTimelineEntry[] {
    return [...this.timeline].sort((left, right) => {
      const occurredAtOrder = Date.parse(right.occurredAtUtc) - Date.parse(left.occurredAtUtc);
      if (occurredAtOrder !== 0) {
        return occurredAtOrder;
      }

      const referenceOrder = right.referenceId.localeCompare(left.referenceId);
      if (referenceOrder !== 0) {
        return referenceOrder;
      }

      return right.eventType.localeCompare(left.eventType);
    });
  }

  getEventLabel(eventType: ClinicalTimelineEventType): string {
    switch (eventType) {
      case 'ClinicalNoteCreated':
        return this.i18n.translate('Note created');
      case 'ClinicalDiagnosisCreated':
        return this.i18n.translate('Diagnosis created');
      case 'ClinicalDiagnosisResolved':
        return this.i18n.translate('Diagnosis resolved');
    }
  }
}
