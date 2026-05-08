import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  SectionCardComponent,
  StatusBadgeComponent
} from '../../../shared/ui';
import type { StatusBadgeTone } from '../../../shared/ui';
import {
  AppointmentReminderChannel,
  AppointmentReminderLogEntry,
  AppointmentReminderLogFormValue,
  AppointmentReminderOutcome
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-reminder-log',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LocalizedDatePipe,
    TranslatePipe,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    SectionCardComponent,
    StatusBadgeComponent
  ],
  template: `
    <app-section-card
      class="reminder-log-panel"
      [title]="'Reminder log' | t"
      [subtitle]="'Manual log only. No WhatsApp, email, or SMS is sent from BigSmile.' | t">
      <span section-card-actions class="section-label">{{ 'Contact attempts' | t }}</span>

      <div *ngIf="loading" class="loading-stack">
        <app-loading-skeleton
          variant="card"
          [ariaLabel]="'Loading contact attempts.' | t">
        </app-loading-skeleton>
        <app-loading-skeleton
          variant="card"
          [ariaLabel]="'Loading contact attempts.' | t">
        </app-loading-skeleton>
      </div>

      <div *ngIf="!loading && error" class="state-card state-error" role="alert">
        {{ error | t }}
      </div>

      <app-empty-state
        *ngIf="!loading && !error && !entries.length"
        icon="L"
        [title]="'No contact attempts have been logged for this appointment.' | t"
        [description]="'Manual log only. No WhatsApp, email, or SMS is sent from BigSmile.' | t">
      </app-empty-state>

      <ol *ngIf="!loading && !error && entries.length" class="log-list">
        <li *ngFor="let entry of entries" class="log-entry">
          <div class="log-entry-head">
            <strong>{{ getChannelLabel(entry.channel) }}</strong>
            <app-status-badge
              size="sm"
              [tone]="getOutcomeTone(entry.outcome)"
              [label]="getOutcomeLabel(entry.outcome)">
            </app-status-badge>
          </div>
          <p *ngIf="entry.notes">{{ entry.notes }}</p>
          <small *ngIf="entry.reminderTemplateNameSnapshot">{{ 'Template:' | t }} {{ entry.reminderTemplateNameSnapshot }}</small>
          <small>{{ entry.createdAtUtc | bsDate: 'short' }} - {{ entry.createdByUserId }}</small>
        </li>
      </ol>

      <form *ngIf="canWrite" class="log-form" (ngSubmit)="submit()">
        <label class="control">
          <span>{{ 'Channel' | t }}</span>
          <select name="channel" [(ngModel)]="channel" [disabled]="saving">
            <option *ngFor="let option of channelOptions" [ngValue]="option">
              {{ getChannelLabel(option) }}
            </option>
          </select>
        </label>

        <label class="control">
          <span>{{ 'Outcome' | t }}</span>
          <select name="outcome" [(ngModel)]="outcome" [disabled]="saving">
            <option *ngFor="let option of outcomeOptions" [ngValue]="option">
              {{ getOutcomeLabel(option) }}
            </option>
          </select>
        </label>

        <label class="control control-wide">
          <span>{{ 'Notes' | t }}</span>
          <textarea
            name="notes"
            rows="3"
            maxlength="500"
            [(ngModel)]="notes"
            [disabled]="saving"></textarea>
        </label>

        <div *ngIf="formError" class="form-error" role="alert">{{ formError | t }}</div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ 'Add log entry' | t }}
        </button>
      </form>
    </app-section-card>
  `,
  styles: [`
    .section-label {
      display: inline-flex;
      margin: 0;
      text-transform: uppercase;
      letter-spacing: 0;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.78rem;
      font-weight: 800;
    }

    .loading-stack,
    .state-card {
      margin-top: 1rem;
    }

    .loading-stack {
      display: grid;
      gap: 0.75rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .state-card {
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-muted);
      padding: 0.9rem 1rem;
    }

    .state-error {
      border: 1px solid var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .log-list {
      display: grid;
      gap: 0.75rem;
      margin: 1rem 0 0;
      padding: 0;
      list-style: none;
    }

    .log-entry {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      padding: 0.9rem 1rem;
      background: var(--bsm-color-surface);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .log-entry:hover {
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-sm);
    }

    .log-entry-head {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
      align-items: center;
    }

    .log-entry strong {
      color: var(--bsm-color-text-brand);
    }

    .log-entry p {
      margin: 0.45rem 0 0;
      color: var(--bsm-color-text);
    }

    .log-entry small {
      display: block;
      margin-top: 0.45rem;
      color: var(--bsm-color-text-muted);
      word-break: break-word;
    }

    .log-form {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      margin-top: 1rem;
      border-top: 1px solid var(--bsm-color-border);
      padding-top: 1rem;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: var(--bsm-color-text-brand);
      font-weight: 700;
    }

    .control-wide {
      grid-column: 1 / -1;
    }

    select,
    textarea {
      width: 100%;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.8rem 0.9rem;
      font: inherit;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
      box-sizing: border-box;
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    select:focus,
    textarea:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    textarea {
      resize: vertical;
      min-height: 84px;
    }

    .form-error {
      grid-column: 1 / -1;
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .btn {
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
      transition:
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard),
        opacity var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.65;
      transform: none;
    }

    .btn:hover:not(:disabled) {
      box-shadow: var(--bsm-shadow-sm);
      transform: translateY(-1px);
    }

    .btn:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    @media (prefers-reduced-motion: reduce) {
      .btn:hover:not(:disabled) {
        transform: none;
      }
    }

    @media (max-width: 720px) {
      .loading-stack,
      .log-form {
        grid-template-columns: 1fr;
      }

      .btn {
        width: 100%;
      }
    }
  `]
})
export class AppointmentReminderLogComponent {
  private readonly i18n = inject(I18nService);

  @Input() entries: AppointmentReminderLogEntry[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() canWrite = false;
  @Input() saving = false;

  @Output() saved = new EventEmitter<AppointmentReminderLogFormValue>();

  readonly channelOptions: AppointmentReminderChannel[] = ['Phone', 'WhatsApp', 'Email', 'Other'];
  readonly outcomeOptions: AppointmentReminderOutcome[] = ['Reached', 'NoAnswer', 'LeftMessage'];
  channel: AppointmentReminderChannel = 'Phone';
  outcome: AppointmentReminderOutcome = 'Reached';
  notes = '';
  formError: string | null = null;

  submit(): void {
    const normalizedNotes = this.notes.trim();

    if (normalizedNotes.length > 500) {
      this.formError = 'Notes must be 500 characters or fewer.';
      return;
    }

    this.formError = null;
    this.saved.emit({
      channel: this.channel,
      outcome: this.outcome,
      notes: normalizedNotes || null
    });
    this.notes = '';
  }

  getChannelLabel(channel: AppointmentReminderChannel): string {
    return this.i18n.translate(channel);
  }

  getOutcomeLabel(outcome: AppointmentReminderOutcome): string {
    switch (outcome) {
      case 'NoAnswer':
        return this.i18n.translate('No answer');
      case 'LeftMessage':
        return this.i18n.translate('Left message');
      default:
        return this.i18n.translate('Reached');
    }
  }

  getOutcomeTone(outcome: AppointmentReminderOutcome): StatusBadgeTone {
    switch (outcome) {
      case 'Reached':
        return 'success';
      case 'NoAnswer':
        return 'warning';
      default:
        return 'info';
    }
  }
}
