import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  StatusBadgeComponent
} from '../../../shared/ui';
import type { StatusBadgeTone } from '../../../shared/ui';
import { AppointmentBlockSummary, AppointmentSummary, CalendarView } from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-calendar',
  standalone: true,
  imports: [
    CommonModule,
    LocalizedDatePipe,
    TranslatePipe,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent
  ],
  template: `
    <section class="calendar-shell">
      <div *ngIf="loading" class="calendar-loading" [attr.aria-label]="'Loading schedule...' | t">
        <app-loading-skeleton
          *ngFor="let item of loadingCards"
          variant="card"
          [ariaLabel]="'Loading schedule...' | t">
        </app-loading-skeleton>
      </div>

      <div *ngIf="!loading && error" class="state-card state-error" role="alert">{{ error | t }}</div>

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
            <app-status-badge tone="warning" size="sm" [label]="'Blocked' | t"></app-status-badge>
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
              <app-status-badge
                size="sm"
                [tone]="getStatusTone(appointment.status)"
                [label]="getStatusLabel(appointment.status)">
              </app-status-badge>
              <app-status-badge
                size="sm"
                [tone]="getConfirmationTone(appointment.confirmationStatus)"
                [label]="getConfirmationLabel(appointment.confirmationStatus)">
              </app-status-badge>
            </span>
          </button>
        </article>
      </div>

      <app-empty-state
        *ngIf="!loading && !error && !calendar"
        icon="S"
        [title]="'Select a branch to load the scheduling calendar.' | t"
        [description]="'Branch context is required before loading appointments.' | t">
      </app-empty-state>
    </section>
  `,
  styles: [`
    .calendar-shell,
    .calendar-grid,
    .calendar-loading {
      display: grid;
      gap: 1rem;
    }

    .calendar-grid,
    .calendar-loading {
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
    }

    .calendar-day {
      grid-template-columns: minmax(0, 1fr);
    }

    .day-column,
    .state-card {
      border-radius: var(--bsm-radius-sm);
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

    .appointment-card:hover {
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .appointment-card:focus-visible {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .appointment-active {
      border-color: var(--bsm-color-primary);
      box-shadow: inset 0 0 0 1px var(--bsm-color-primary), var(--bsm-shadow-sm);
    }

    .block-card {
      background: var(--bsm-color-warning-soft);
      border-color: var(--bsm-color-warning-soft);
    }

    .block-active {
      border-color: var(--bsm-color-warning);
      box-shadow: inset 0 0 0 1px var(--bsm-color-warning);
    }

    .appointment-cancelled {
      background: var(--bsm-color-danger-soft);
      border-color: var(--bsm-color-danger-soft);
    }

    .appointment-attended {
      background: var(--bsm-color-success-soft);
      border-color: var(--bsm-color-success-soft);
    }

    .appointment-no-show {
      background: var(--bsm-color-warning-soft);
      border-color: var(--bsm-color-warning-soft);
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

    .badge-row {
      display: flex;
      gap: 0.45rem;
      flex-wrap: wrap;
      align-items: center;
    }

    .state-error {
      border-color: var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    @media (prefers-reduced-motion: reduce) {
      .appointment-card:hover {
        transform: none;
      }
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

  readonly loadingCards = [0, 1, 2];

  getStatusLabel(status: AppointmentSummary['status']): string {
    return this.i18n.translate(status === 'NoShow' ? 'No-show' : status);
  }

  getConfirmationLabel(status: AppointmentSummary['confirmationStatus']): string {
    return this.i18n.translate(status === 'Confirmed' ? 'Confirmed' : 'Pending confirmation');
  }

  getStatusTone(status: AppointmentSummary['status']): StatusBadgeTone {
    switch (status) {
      case 'Attended':
        return 'success';
      case 'Cancelled':
        return 'danger';
      case 'NoShow':
        return 'warning';
      default:
        return 'primary';
    }
  }

  getConfirmationTone(status: AppointmentSummary['confirmationStatus']): StatusBadgeTone {
    return status === 'Confirmed' ? 'success' : 'warning';
  }
}
