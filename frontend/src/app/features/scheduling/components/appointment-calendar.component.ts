import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AppointmentSummary, CalendarView } from '../models/scheduling.models';

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
            <p>{{ day.appointments.length }} appointment{{ day.appointments.length === 1 ? '' : 's' }}</p>
          </header>

          <div *ngIf="!day.appointments.length" class="empty-state">
            No appointments scheduled for this branch and date.
          </div>

          <button
            type="button"
            *ngFor="let appointment of day.appointments"
            class="appointment-card"
            [class.appointment-active]="appointment.id === activeAppointmentId"
            [class.appointment-cancelled]="appointment.status === 'Cancelled'"
            (click)="appointmentSelected.emit(appointment)">
            <span class="time-range">{{ appointment.startsAt | date: 'shortTime' }} - {{ appointment.endsAt | date: 'shortTime' }}</span>
            <strong>{{ appointment.patientFullName }}</strong>
            <p>{{ appointment.notes || 'No operational note.' }}</p>
            <span class="status-pill">{{ appointment.status }}</span>
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

    .appointment-cancelled {
      background: #fff3f3;
      border-color: #f2c4c4;
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

  @Output() appointmentSelected = new EventEmitter<AppointmentSummary>();
}
