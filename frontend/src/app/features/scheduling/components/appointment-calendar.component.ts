import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { AppointmentBlockSummary, AppointmentSummary, CalendarView } from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-calendar',
  standalone: true,
  imports: [CommonModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="calendar-shell">
      <div *ngIf="loading" class="state-card">{{ 'Loading schedule...' | t }}</div>
      <div *ngIf="!loading && error" class="state-card state-error">{{ error | t }}</div>

      <div *ngIf="!loading && !error && calendar as currentCalendar" class="calendar-grid" [class.calendar-day]="currentCalendar.days === 1">
        <article *ngFor="let day of currentCalendar.calendarDays" class="day-column">
          <header class="day-head">
            <h3>{{ day.date | bsDate: 'fullDate' }}</h3>
            <p>
              {{ day.appointments.length }} {{ (day.appointments.length === 1 ? 'appointment' : 'appointments') | t }}
              /
              {{ day.blockedSlots.length }} {{ (day.blockedSlots.length === 1 ? 'blocked slot' : 'blocked slots') | t }}
            </p>
          </header>

          <div *ngIf="!day.appointments.length && !day.blockedSlots.length" class="empty-state">
            {{ 'No scheduling activity for this branch and date.' | t }}
          </div>

          <button
            type="button"
            *ngFor="let blockedSlot of day.blockedSlots"
            class="appointment-card block-card"
            [class.block-active]="blockedSlot.id === activeBlockId"
            (click)="blockedSlotSelected.emit(blockedSlot)">
            <span class="time-range">{{ blockedSlot.startsAt | bsDate: 'shortTime' }} - {{ blockedSlot.endsAt | bsDate: 'shortTime' }}</span>
            <strong>{{ blockedSlot.label || ('Blocked slot' | t) }}</strong>
            <p>{{ 'Appointments are not allowed in this branch range.' | t }}</p>
            <span class="status-pill block-pill">{{ 'Blocked' | t }}</span>
          </button>

          <button
            type="button"
            *ngFor="let appointment of day.appointments"
            class="appointment-card"
            [class.appointment-active]="appointment.id === activeAppointmentId"
            [class.appointment-cancelled]="appointment.status === 'Cancelled'"
            [class.appointment-attended]="appointment.status === 'Attended'"
            [class.appointment-no-show]="appointment.status === 'NoShow'"
            (click)="appointmentSelected.emit(appointment)">
            <span class="time-range">{{ appointment.startsAt | bsDate: 'shortTime' }} - {{ appointment.endsAt | bsDate: 'shortTime' }}</span>
            <strong>{{ appointment.patientFullName }}</strong>
            <p>{{ appointment.notes || ('No operational note.' | t) }}</p>
            <span class="badge-row">
              <span class="status-pill">{{ getStatusLabel(appointment.status) }}</span>
              <span
                class="confirmation-pill"
                [class.confirmation-confirmed]="appointment.confirmationStatus === 'Confirmed'">
                {{ getConfirmationLabel(appointment.confirmationStatus) }}
              </span>
            </span>
          </button>
        </article>
      </div>

      <div *ngIf="!loading && !error && !calendar" class="state-card">
        {{ 'Select a branch to load the scheduling calendar.' | t }}
      </div>
    </section>
  `,
  styles: [`
    .calendar-shell,
    .calendar-grid {
      display: grid;
      gap: 1rem;
    }

    .calendar-grid {
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
    }

    .calendar-day {
      grid-template-columns: minmax(0, 1fr);
    }

    .day-column,
    .state-card {
      border-radius: var(--bsm-radius-lg);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-gradient-surface);
      padding: 1rem;
      box-shadow: var(--bsm-shadow-sm);
    }

    .day-head h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: 1rem;
    }

    .day-head p {
      margin: 0.25rem 0 0;
      color: var(--bsm-color-text-muted);
      font-size: 0.9rem;
    }

    .empty-state {
      margin-top: 1rem;
      color: var(--bsm-color-text-muted);
      font-size: 0.95rem;
    }

    .appointment-card {
      display: grid;
      gap: 0.35rem;
      width: 100%;
      margin-top: 0.9rem;
      text-align: left;
      border-radius: var(--bsm-radius-lg);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
      padding: 0.95rem 1rem;
      cursor: pointer;
      color: var(--bsm-color-text-brand);
      font: inherit;
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .appointment-active {
      border-color: var(--bsm-color-primary);
      box-shadow: inset 0 0 0 1px var(--bsm-color-primary), var(--bsm-shadow-sm);
    }

    .block-card {
      background: #fff7eb;
      border-color: #f0d5ad;
    }

    .block-active {
      border-color: #8b4f0f;
      box-shadow: inset 0 0 0 1px #8b4f0f;
    }

    .appointment-cancelled {
      background: #fff3f3;
      border-color: #f2c4c4;
    }

    .appointment-attended {
      background: #eef9f2;
      border-color: #b9e1c7;
    }

    .appointment-no-show {
      background: #fff6e5;
      border-color: #f3d39f;
    }

    .time-range {
      font-size: 0.82rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-text-muted);
      font-weight: 700;
    }

    .appointment-card p {
      margin: 0;
      color: var(--bsm-color-text-muted);
      font-size: 0.92rem;
    }

    .status-pill {
      justify-self: start;
      margin-top: 0.2rem;
      padding: 0.35rem 0.65rem;
      border-radius: var(--bsm-radius-pill);
      background: #e8f4ec;
      color: #1d6a3a;
      font-size: 0.82rem;
      font-weight: 700;
    }

    .badge-row {
      display: flex;
      gap: 0.45rem;
      flex-wrap: wrap;
      align-items: center;
    }

    .badge-row .status-pill {
      margin-top: 0;
    }

    .confirmation-pill {
      padding: 0.35rem 0.65rem;
      border-radius: var(--bsm-radius-pill);
      background: #fbe6bf;
      color: #8b4f0f;
      font-size: 0.82rem;
      font-weight: 700;
    }

    .confirmation-confirmed {
      background: #dff2e5;
      color: #1d6a3a;
    }

    .appointment-cancelled .status-pill {
      background: #fde3e3;
      color: #9b2d30;
    }

    .appointment-attended .status-pill {
      background: #dff2e5;
      color: #1d6a3a;
    }

    .appointment-no-show .status-pill {
      background: #fbe6bf;
      color: #8b4f0f;
    }

    .block-pill {
      background: #f8e3c5;
      color: #8b4f0f;
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }
  `]
})
export class AppointmentCalendarComponent {
  private readonly i18n = inject(I18nService);

  @Input() calendar: CalendarView | null = null;
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() activeAppointmentId: string | null = null;
  @Input() activeBlockId: string | null = null;

  @Output() appointmentSelected = new EventEmitter<AppointmentSummary>();
  @Output() blockedSlotSelected = new EventEmitter<AppointmentBlockSummary>();

  getStatusLabel(status: AppointmentSummary['status']): string {
    return this.i18n.translate(status === 'NoShow' ? 'No-show' : status);
  }

  getConfirmationLabel(status: AppointmentSummary['confirmationStatus']): string {
    return this.i18n.translate(status === 'Confirmed' ? 'Confirmed' : 'Pending confirmation');
  }
}
