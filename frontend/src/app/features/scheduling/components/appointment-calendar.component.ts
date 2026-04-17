import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AppointmentBlockSummary, AppointmentSummary, CalendarView } from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-calendar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="calendar-shell">
      <div *ngIf="loading" class="state-card">Loading schedule...</div>
      <div *ngIf="!loading && error" class="state-card state-error">{{ error }}</div>

      <div *ngIf="!loading && !error && calendar as currentCalendar" class="calendar-grid" [class.calendar-day]="currentCalendar.days === 1">
        <article *ngFor="let day of currentCalendar.calendarDays" class="day-column">
          <header class="day-head">
            <h3>{{ day.date | date: 'fullDate' }}</h3>
            <p>
              {{ day.appointments.length }} appointment{{ day.appointments.length === 1 ? '' : 's' }}
              ·
              {{ day.blockedSlots.length }} blocked slot{{ day.blockedSlots.length === 1 ? '' : 's' }}
            </p>
          </header>

          <div *ngIf="!day.appointments.length && !day.blockedSlots.length" class="empty-state">
            No scheduling activity for this branch and date.
          </div>

          <button
            type="button"
            *ngFor="let blockedSlot of day.blockedSlots"
            class="appointment-card block-card"
            [class.block-active]="blockedSlot.id === activeBlockId"
            (click)="blockedSlotSelected.emit(blockedSlot)">
            <span class="time-range">{{ blockedSlot.startsAt | date: 'shortTime' }} - {{ blockedSlot.endsAt | date: 'shortTime' }}</span>
            <strong>{{ blockedSlot.label || 'Blocked slot' }}</strong>
            <p>Appointments are not allowed in this branch range.</p>
            <span class="status-pill block-pill">Blocked</span>
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
            <span class="time-range">{{ appointment.startsAt | date: 'shortTime' }} - {{ appointment.endsAt | date: 'shortTime' }}</span>
            <strong>{{ appointment.patientFullName }}</strong>
            <p>{{ appointment.notes || 'No operational note.' }}</p>
            <span class="status-pill">{{ getStatusLabel(appointment.status) }}</span>
          </button>
        </article>
      </div>

      <div *ngIf="!loading && !error && !calendar" class="state-card">
        Select a branch to load the scheduling calendar.
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
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .day-head h3 {
      margin: 0;
      color: #16324f;
      font-size: 1rem;
    }

    .day-head p {
      margin: 0.25rem 0 0;
      color: #62758a;
      font-size: 0.9rem;
    }

    .empty-state {
      margin-top: 1rem;
      color: #62758a;
      font-size: 0.95rem;
    }

    .appointment-card {
      display: grid;
      gap: 0.35rem;
      width: 100%;
      margin-top: 0.9rem;
      text-align: left;
      border-radius: 16px;
      border: 1px solid #d6e4ef;
      background: #ffffff;
      padding: 0.95rem 1rem;
      cursor: pointer;
      color: #16324f;
      font: inherit;
    }

    .appointment-active {
      border-color: #0a5bb5;
      box-shadow: inset 0 0 0 1px #0a5bb5;
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
      color: #62758a;
      font-weight: 700;
    }

    .appointment-card p {
      margin: 0;
      color: #62758a;
      font-size: 0.92rem;
    }

    .status-pill {
      justify-self: start;
      margin-top: 0.2rem;
      padding: 0.35rem 0.65rem;
      border-radius: 999px;
      background: #e8f4ec;
      color: #1d6a3a;
      font-size: 0.82rem;
      font-weight: 700;
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
  @Input() calendar: CalendarView | null = null;
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() activeAppointmentId: string | null = null;
  @Input() activeBlockId: string | null = null;

  @Output() appointmentSelected = new EventEmitter<AppointmentSummary>();
  @Output() blockedSlotSelected = new EventEmitter<AppointmentBlockSummary>();

  getStatusLabel(status: AppointmentSummary['status']): string {
    return status === 'NoShow' ? 'No-show' : status;
  }
}
