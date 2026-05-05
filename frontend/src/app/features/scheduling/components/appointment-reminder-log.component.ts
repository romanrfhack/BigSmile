import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  AppointmentReminderChannel,
  AppointmentReminderLogEntry,
  AppointmentReminderLogFormValue,
  AppointmentReminderOutcome
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-reminder-log',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="reminder-log-panel">
      <header class="reminder-log-head">
        <div>
          <p class="section-label">{{ 'Contact attempts' | t }}</p>
          <h3>{{ 'Reminder log' | t }}</h3>
          <p class="manual-note">{{ 'Manual log only. No WhatsApp, email, or SMS is sent from BigSmile.' | t }}</p>
        </div>
      </header>

      <div *ngIf="loading" class="state-card">
        {{ 'Loading contact attempts.' | t }}
      </div>

      <div *ngIf="!loading && error" class="state-card state-error">
        {{ error | t }}
      </div>

      <div *ngIf="!loading && !error && !entries.length" class="state-card">
        {{ 'No contact attempts have been logged for this appointment.' | t }}
      </div>

      <ol *ngIf="!loading && !error && entries.length" class="log-list">
        <li *ngFor="let entry of entries" class="log-entry">
          <div>
            <strong>{{ getChannelLabel(entry.channel) }}</strong>
            <span>{{ getOutcomeLabel(entry.outcome) }}</span>
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

        <div *ngIf="formError" class="form-error">{{ formError | t }}</div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          {{ 'Add log entry' | t }}
        </button>
      </form>
    </section>
  `,
  styles: [`
    .reminder-log-panel {
      border-radius: 20px;
      border: 1px solid var(--bsm-color-border);
      background: #ffffff;
      padding: 1.25rem;
      box-shadow: 0 18px 30px rgba(20, 48, 79, 0.08);
    }

    .reminder-log-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .section-label {
      margin: 0 0 0.35rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.78rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: 1.2rem;
    }

    .manual-note {
      margin: 0.45rem 0 0;
      color: var(--bsm-color-text-muted);
    }

    .state-card {
      margin-top: 1rem;
      border-radius: 14px;
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-muted);
      padding: 0.9rem 1rem;
    }

    .state-error {
      border: 1px solid #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
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
      border-radius: 14px;
      padding: 0.9rem 1rem;
      background: var(--bsm-color-surface);
    }

    .log-entry div {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
      align-items: baseline;
    }

    .log-entry strong {
      color: var(--bsm-color-text-brand);
    }

    .log-entry span {
      color: #1d6a3a;
      font-weight: 700;
    }

    .log-entry p {
      margin: 0.45rem 0 0;
      color: #42546a;
    }

    .log-entry small {
      display: block;
      margin-top: 0.45rem;
      color: #6a7f96;
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
      border-radius: 14px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      background: #ffffff;
      box-sizing: border-box;
    }

    textarea {
      resize: vertical;
      min-height: 84px;
    }

    .form-error {
      grid-column: 1 / -1;
      color: #8c2525;
      font-weight: 700;
    }

    .btn {
      border: none;
      border-radius: 999px;
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: #ffffff;
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.65;
    }

    @media (max-width: 720px) {
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
}
