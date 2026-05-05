import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  AppointmentManualReminderFormValue,
  AppointmentReminderChannel,
  AppointmentReminderState,
  AppointmentSummary
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-manual-reminder',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="manual-reminder-panel">
      <header class="manual-reminder-head">
        <div>
          <p class="section-label">{{ 'Manual reminder' | t }}</p>
          <h3>{{ 'Reminder preparation' | t }}</h3>
          <p class="manual-note">{{ 'Manual scheduling only. No WhatsApp, email, or SMS is sent from BigSmile.' | t }}</p>
        </div>
        <span class="state-pill" [class.state-due]="reminderState === 'Due'" [class.state-completed]="reminderState === 'Completed'">
          {{ getStateLabel(reminderState) }}
        </span>
      </header>

      <dl class="reminder-details" *ngIf="appointment?.reminderRequired">
        <div>
          <dt>{{ 'Channel' | t }}</dt>
          <dd>{{ getChannelLabel(appointment?.reminderChannel) }}</dd>
        </div>
        <div>
          <dt>{{ 'Due' | t }}</dt>
          <dd>{{ appointment?.reminderDueAtUtc ? (appointment?.reminderDueAtUtc | bsDate: 'short') : ('Not set' | t) }}</dd>
        </div>
        <div *ngIf="appointment?.reminderCompletedAtUtc">
          <dt>{{ 'Completed' | t }}</dt>
          <dd>{{ appointment?.reminderCompletedAtUtc | bsDate: 'short' }}</dd>
        </div>
      </dl>

      <form *ngIf="canWrite && appointment?.status === 'Scheduled'" class="reminder-form" (ngSubmit)="submit()">
        <label class="control">
          <span>{{ 'Preferred channel' | t }}</span>
          <select name="channel" [(ngModel)]="channel" [disabled]="saving">
            <option *ngFor="let option of channelOptions" [ngValue]="option">
              {{ getChannelLabel(option) }}
            </option>
          </select>
        </label>

        <label class="control">
          <span>{{ 'Due date/time' | t }}</span>
          <input type="datetime-local" name="dueAt" [(ngModel)]="dueAt" [disabled]="saving" />
        </label>

        <div *ngIf="formError || error" class="form-error">{{ (formError || error) | t }}</div>

        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="saving">
            {{ 'Set reminder' | t }}
          </button>
          <button
            *ngIf="appointment?.reminderRequired"
            type="button"
            class="btn btn-secondary"
            [disabled]="saving"
            (click)="cleared.emit()">
            {{ 'Clear reminder' | t }}
          </button>
          <button
            *ngIf="appointment?.reminderRequired && !appointment?.reminderCompletedAtUtc"
            type="button"
            class="btn btn-success"
            [disabled]="saving"
            (click)="completed.emit()">
            {{ 'Mark reminder completed' | t }}
          </button>
        </div>
      </form>

      <div *ngIf="canWrite && appointment?.status !== 'Scheduled' && appointment?.reminderRequired" class="actions terminal-actions">
        <button type="button" class="btn btn-secondary" [disabled]="saving" (click)="cleared.emit()">
          {{ 'Clear reminder' | t }}
        </button>
        <button
          *ngIf="!appointment?.reminderCompletedAtUtc"
          type="button"
          class="btn btn-success"
          [disabled]="saving"
          (click)="completed.emit()">
          {{ 'Mark reminder completed' | t }}
        </button>
      </div>

      <div *ngIf="canWrite && appointment?.status !== 'Scheduled' && !appointment?.reminderRequired" class="state-card">
        {{ 'Terminal appointments cannot receive new manual reminder work.' | t }}
      </div>
    </section>
  `,
  styles: [`
    .manual-reminder-panel {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: #ffffff;
      padding: 1.25rem;
      box-shadow: 0 18px 30px rgba(20, 48, 79, 0.08);
    }

    .manual-reminder-head {
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

    .state-pill {
      border-radius: 999px;
      background: #e5edf5;
      color: #17304d;
      font-weight: 800;
      padding: 0.4rem 0.7rem;
      white-space: nowrap;
    }

    .state-due {
      background: #fbe6bf;
      color: #8b4f0f;
    }

    .state-completed {
      background: #dff2e5;
      color: #1d6a3a;
    }

    .reminder-details,
    .reminder-form {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      margin: 1rem 0 0;
    }

    .reminder-details {
      border-radius: 14px;
      background: #f8fbfd;
      padding: 0.9rem 1rem;
    }

    dt {
      color: #718298;
      font-size: 0.78rem;
      font-weight: 800;
      margin: 0;
      text-transform: uppercase;
    }

    dd {
      color: #16324f;
      font-weight: 700;
      margin: 0.25rem 0 0;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: #16324f;
      font-weight: 700;
    }

    select,
    input {
      width: 100%;
      border: 1px solid #c8d4df;
      border-radius: 14px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      background: #ffffff;
      box-sizing: border-box;
    }

    .actions {
      grid-column: 1 / -1;
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .terminal-actions {
      margin-top: 1rem;
    }

    .form-error,
    .state-card {
      grid-column: 1 / -1;
      border-radius: 14px;
      border: 1px solid #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
      padding: 0.85rem 0.95rem;
      font-weight: 700;
    }

    .state-card {
      margin-top: 1rem;
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

    .btn-secondary {
      background: #e5edf5;
      color: #17304d;
    }

    .btn-success {
      background: #dff2e5;
      color: #1d6a3a;
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.65;
    }

    @media (max-width: 720px) {
      .manual-reminder-head {
        flex-direction: column;
      }

      .actions .btn {
        width: 100%;
      }
    }
  `]
})
export class AppointmentManualReminderComponent implements OnChanges {
  private readonly i18n = inject(I18nService);

  @Input() appointment: AppointmentSummary | null = null;
  @Input() canWrite = false;
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() saved = new EventEmitter<AppointmentManualReminderFormValue>();
  @Output() cleared = new EventEmitter<void>();
  @Output() completed = new EventEmitter<void>();

  readonly channelOptions: AppointmentReminderChannel[] = ['Phone', 'WhatsApp', 'Email', 'Other'];
  channel: AppointmentReminderChannel = 'Phone';
  dueAt = '';
  formError: string | null = null;

  get reminderState(): AppointmentReminderState {
    if (!this.appointment?.reminderRequired) {
      return 'NotRequired';
    }

    if (this.appointment.reminderCompletedAtUtc) {
      return 'Completed';
    }

    const dueAtUtc = this.appointment.reminderDueAtUtc
      ? new Date(this.appointment.reminderDueAtUtc)
      : null;

    return dueAtUtc && dueAtUtc.getTime() <= Date.now() ? 'Due' : 'Pending';
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['appointment']) {
      this.channel = this.appointment?.reminderChannel ?? 'Phone';
      this.dueAt = this.appointment?.reminderDueAtUtc
        ? toLocalDateTimeValue(this.appointment.reminderDueAtUtc)
        : '';
      this.formError = null;
    }
  }

  submit(): void {
    if (!this.dueAt) {
      this.formError = 'Due date/time is required.';
      return;
    }

    const parsedDueAt = new Date(this.dueAt);
    if (Number.isNaN(parsedDueAt.getTime())) {
      this.formError = 'Due date/time is invalid.';
      return;
    }

    this.formError = null;
    this.saved.emit({
      channel: this.channel,
      dueAtUtc: parsedDueAt.toISOString()
    });
  }

  getStateLabel(state: AppointmentReminderState): string {
    switch (state) {
      case 'NotRequired':
        return this.i18n.translate('Not required');
      default:
        return this.i18n.translate(state);
    }
  }

  getChannelLabel(channel: AppointmentReminderChannel | null | undefined): string {
    return channel ? this.i18n.translate(channel) : this.i18n.translate('Not set');
  }
}

function toLocalDateTimeValue(value: string): string {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return '';
  }

  const year = parsed.getFullYear();
  const month = `${parsed.getMonth() + 1}`.padStart(2, '0');
  const day = `${parsed.getDate()}`.padStart(2, '0');
  const hours = `${parsed.getHours()}`.padStart(2, '0');
  const minutes = `${parsed.getMinutes()}`.padStart(2, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}`;
}
