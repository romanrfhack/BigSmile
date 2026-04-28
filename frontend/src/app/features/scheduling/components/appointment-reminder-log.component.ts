import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  AppointmentReminderChannel,
  AppointmentReminderLogEntry,
  AppointmentReminderLogFormValue,
  AppointmentReminderOutcome
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-reminder-log',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="reminder-log-panel">
      <header class="reminder-log-head">
        <div>
          <p class="section-label">Contact attempts</p>
          <h3>Reminder log</h3>
          <p class="manual-note">Manual log only. No WhatsApp, email, or SMS is sent from BigSmile.</p>
        </div>
      </header>

      <div *ngIf="loading" class="state-card">
        Loading contact attempts.
      </div>

      <div *ngIf="!loading && error" class="state-card state-error">
        {{ error }}
      </div>

      <div *ngIf="!loading && !error && !entries.length" class="state-card">
        No contact attempts have been logged for this appointment.
      </div>

      <ol *ngIf="!loading && !error && entries.length" class="log-list">
        <li *ngFor="let entry of entries" class="log-entry">
          <div>
            <strong>{{ getChannelLabel(entry.channel) }}</strong>
            <span>{{ getOutcomeLabel(entry.outcome) }}</span>
          </div>
          <p *ngIf="entry.notes">{{ entry.notes }}</p>
          <small *ngIf="entry.reminderTemplateNameSnapshot">Template: {{ entry.reminderTemplateNameSnapshot }}</small>
          <small>{{ entry.createdAtUtc | date: 'short' }} - {{ entry.createdByUserId }}</small>
        </li>
      </ol>

      <form *ngIf="canWrite" class="log-form" (ngSubmit)="submit()">
        <label class="control">
          <span>Channel</span>
          <select name="channel" [(ngModel)]="channel" [disabled]="saving">
            <option *ngFor="let option of channelOptions" [ngValue]="option">
              {{ getChannelLabel(option) }}
            </option>
          </select>
        </label>

        <label class="control">
          <span>Outcome</span>
          <select name="outcome" [(ngModel)]="outcome" [disabled]="saving">
            <option *ngFor="let option of outcomeOptions" [ngValue]="option">
              {{ getOutcomeLabel(option) }}
            </option>
          </select>
        </label>

        <label class="control control-wide">
          <span>Notes</span>
          <textarea
            name="notes"
            rows="3"
            maxlength="500"
            [(ngModel)]="notes"
            [disabled]="saving"></textarea>
        </label>

        <div *ngIf="formError" class="form-error">{{ formError }}</div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">
          Add log entry
        </button>
      </form>
    </section>
  `,
  styles: [`
    .reminder-log-panel {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
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
      color: #56708d;
      font-size: 0.78rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: #16324f;
      font-size: 1.2rem;
    }

    .manual-note {
      margin: 0.45rem 0 0;
      color: #5b6e84;
    }

    .state-card {
      margin-top: 1rem;
      border-radius: 14px;
      background: #f5f9fc;
      color: #5b6e84;
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
      border: 1px solid #dce6ef;
      border-radius: 14px;
      padding: 0.9rem 1rem;
      background: #f8fbfd;
    }

    .log-entry div {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
      align-items: baseline;
    }

    .log-entry strong {
      color: #16324f;
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
      border-top: 1px solid #dce6ef;
      padding-top: 1rem;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: #16324f;
      font-weight: 700;
    }

    .control-wide {
      grid-column: 1 / -1;
    }

    select,
    textarea {
      width: 100%;
      border: 1px solid #c8d4df;
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
      background: #0a5bb5;
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
    return channel;
  }

  getOutcomeLabel(outcome: AppointmentReminderOutcome): string {
    switch (outcome) {
      case 'NoAnswer':
        return 'No answer';
      case 'LeftMessage':
        return 'Left message';
      default:
        return 'Reached';
    }
  }
}
