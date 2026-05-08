import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { SectionCardComponent, StatusBadgeComponent } from '../../../shared/ui';
import type { StatusBadgeTone } from '../../../shared/ui';
import {
  AppointmentManualReminderFormValue,
  AppointmentReminderChannel,
  AppointmentReminderState,
  AppointmentSummary
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-manual-reminder',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LocalizedDatePipe,
    TranslatePipe,
    SectionCardComponent,
    StatusBadgeComponent
  ],
  template: `
    <app-section-card
      class="manual-reminder-panel"
      [title]="'Reminder preparation' | t"
      [subtitle]="'Manual scheduling only. No WhatsApp, email, or SMS is sent from BigSmile.' | t">
      <span section-card-actions class="section-label">{{ 'Manual reminder' | t }}</span>
      <app-status-badge
        section-card-actions
        [tone]="getStateTone(reminderState)"
        [label]="getStateLabel(reminderState)">
      </app-status-badge>

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

        <div *ngIf="formError || error" class="form-error" role="alert">{{ (formError || error) | t }}</div>

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

    .reminder-details,
    .reminder-form {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      margin: 1rem 0 0;
    }

    .reminder-details {
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
      padding: 0.9rem 1rem;
    }

    dt {
      color: var(--bsm-color-text-muted);
      font-size: 0.78rem;
      font-weight: 800;
      margin: 0;
      text-transform: uppercase;
    }

    dd {
      color: var(--bsm-color-text-brand);
      font-weight: 700;
      margin: 0.25rem 0 0;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: var(--bsm-color-text-brand);
      font-weight: 700;
    }

    select,
    input {
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
    input:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
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
      border-radius: var(--bsm-radius-sm);
      border: 1px solid var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      padding: 0.85rem 0.95rem;
      font-weight: 700;
    }

    .state-card {
      margin-top: 1rem;
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

    .btn-secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-text-brand);
    }

    .btn-success {
      background: var(--bsm-color-success-soft);
      color: var(--bsm-color-success);
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

  getStateTone(state: AppointmentReminderState): StatusBadgeTone {
    switch (state) {
      case 'Completed':
        return 'success';
      case 'Due':
        return 'warning';
      case 'NotRequired':
        return 'neutral';
      default:
        return 'primary';
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
