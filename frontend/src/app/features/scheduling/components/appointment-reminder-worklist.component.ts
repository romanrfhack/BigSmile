import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { AppointmentReminderState, AppointmentReminderWorkItem } from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-reminder-worklist',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="worklist-panel">
      <header class="worklist-head">
        <div>
          <p class="section-label">Pending reminders</p>
          <h3>Manual reminder work</h3>
          <p class="manual-note">Manual preparation only. No providers, jobs, queues, templates, scheduler, or retry automation.</p>
        </div>
      </header>

      <div *ngIf="loading" class="state-card">
        Loading manual reminders.
      </div>

      <div *ngIf="!loading && error" class="state-card state-error">
        {{ error }}
      </div>

      <div *ngIf="!loading && !error && !items.length" class="state-card">
        No pending or due manual reminders for this branch.
      </div>

      <ol *ngIf="!loading && !error && items.length" class="worklist">
        <li *ngFor="let item of items" class="work-item">
          <div>
            <strong>{{ item.patientFullName }}</strong>
            <span class="state-pill" [class.state-due]="item.reminderState === 'Due'">
              {{ getStateLabel(item.reminderState) }}
            </span>
          </div>
          <p>
            {{ item.reminderDueAtUtc | date: 'short' }}
            <span *ngIf="item.reminderChannel"> · {{ item.reminderChannel }}</span>
          </p>
          <small>
            Appointment {{ item.startsAt | date: 'short' }} · {{ item.appointmentStatus }} · {{ item.confirmationStatus }}
          </small>
        </li>
      </ol>
    </section>
  `,
  styles: [`
    .worklist-panel {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: #ffffff;
      padding: 1.25rem;
      box-shadow: 0 18px 30px rgba(20, 48, 79, 0.08);
    }

    .worklist-head {
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

    .worklist {
      display: grid;
      gap: 0.75rem;
      margin: 1rem 0 0;
      padding: 0;
      list-style: none;
    }

    .work-item {
      border: 1px solid #dce6ef;
      border-radius: 14px;
      padding: 0.9rem 1rem;
      background: #f8fbfd;
    }

    .work-item div {
      display: flex;
      gap: 0.5rem;
      align-items: center;
      justify-content: space-between;
    }

    .work-item strong {
      color: #16324f;
    }

    .work-item p {
      margin: 0.45rem 0 0;
      color: #42546a;
      font-weight: 700;
    }

    .work-item small {
      display: block;
      margin-top: 0.45rem;
      color: #6a7f96;
      word-break: break-word;
    }

    .state-pill {
      border-radius: 999px;
      background: #e5edf5;
      color: #17304d;
      font-weight: 800;
      padding: 0.35rem 0.6rem;
      white-space: nowrap;
    }

    .state-due {
      background: #fbe6bf;
      color: #8b4f0f;
    }
  `]
})
export class AppointmentReminderWorklistComponent {
  @Input() items: AppointmentReminderWorkItem[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;

  getStateLabel(state: AppointmentReminderState): string {
    return state === 'Due' ? 'Due' : 'Pending';
  }
}
