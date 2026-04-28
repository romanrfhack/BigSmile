import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  AppointmentReminderChannel,
  AppointmentReminderFollowUpFormValue,
  AppointmentReminderOutcome,
  AppointmentReminderState,
  AppointmentReminderWorkItem,
  ReminderTemplate,
  ReminderTemplatePreview
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-reminder-worklist',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="worklist-panel">
      <header class="worklist-head">
        <div>
          <p class="section-label">Pending reminders</p>
          <h3>Manual reminder work</h3>
          <p class="manual-note">Manual preparation only. No providers, jobs, queues, external delivery templates, scheduler, or retry automation.</p>
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
          <button
            *ngIf="canWrite"
            type="button"
            class="btn btn-secondary"
            [disabled]="savingAppointmentId === item.appointmentId"
            (click)="startFollowUp(item)">
            Record follow-up
          </button>

          <form
            *ngIf="activeAppointmentId === item.appointmentId"
            class="follow-up-form"
            (ngSubmit)="submitFollowUp(item)">
            <p class="manual-record-note">Manual record only. BigSmile does not send messages.</p>

            <label class="control">
              <span>Channel</span>
              <select
                name="channel-{{ item.appointmentId }}"
                [(ngModel)]="channel"
                [disabled]="savingAppointmentId === item.appointmentId">
                <option *ngFor="let option of channelOptions" [ngValue]="option">
                  {{ option }}
                </option>
              </select>
            </label>

            <label class="control">
              <span>Outcome</span>
              <select
                name="outcome-{{ item.appointmentId }}"
                [(ngModel)]="outcome"
                [disabled]="savingAppointmentId === item.appointmentId">
                <option *ngFor="let option of outcomeOptions" [ngValue]="option">
                  {{ getOutcomeLabel(option) }}
                </option>
              </select>
            </label>

            <label class="control control-wide">
              <span>Notes</span>
              <textarea
                name="notes-{{ item.appointmentId }}"
                rows="3"
                maxlength="500"
                [(ngModel)]="notes"
                [disabled]="savingAppointmentId === item.appointmentId"></textarea>
            </label>

            <div class="template-helper">
              <p>Template preview is a manual draft helper only.</p>
              <div *ngIf="!reminderTemplates.length" class="template-empty">
                No active templates are available.
              </div>
              <div *ngIf="reminderTemplates.length" class="template-controls">
                <label class="control">
                  <span>Template</span>
                  <select
                    name="template-{{ item.appointmentId }}"
                    [(ngModel)]="selectedTemplateId"
                    [disabled]="savingAppointmentId === item.appointmentId">
                    <option [ngValue]="null">Select a template</option>
                    <option *ngFor="let template of reminderTemplates" [ngValue]="template.id">
                      {{ template.name }}
                    </option>
                  </select>
                </label>
                <button
                  type="button"
                  class="btn btn-secondary"
                  [disabled]="!selectedTemplateId || previewingTemplateId === selectedTemplateId"
                  (click)="requestTemplatePreview(item)">
                  Preview template
                </button>
              </div>

              <div
                *ngIf="templatePreview?.appointmentId === item.appointmentId && templatePreview?.templateId === selectedTemplateId"
                class="template-preview">
                <strong>Rendered preview</strong>
                <p>{{ templatePreview?.renderedBody }}</p>
                <small *ngIf="templatePreview?.unknownPlaceholders?.length">
                  Unknown placeholders: {{ templatePreview?.unknownPlaceholders?.join(', ') }}
                </small>
                <button type="button" class="btn btn-secondary" (click)="usePreviewAsNote()">
                  Use as note
                </button>
              </div>

              <div *ngIf="templatePreviewError" class="form-error">{{ templatePreviewError }}</div>
            </div>

            <label class="checkbox-control">
              <input
                type="checkbox"
                name="complete-{{ item.appointmentId }}"
                [(ngModel)]="completeReminder"
                [disabled]="savingAppointmentId === item.appointmentId" />
              <span>Mark reminder completed</span>
            </label>

            <label class="checkbox-control">
              <input
                type="checkbox"
                name="confirm-{{ item.appointmentId }}"
                [(ngModel)]="confirmAppointment"
                [disabled]="savingAppointmentId === item.appointmentId" />
              <span>Confirm appointment</span>
            </label>

            <div *ngIf="formError" class="form-error">{{ formError }}</div>
            <div *ngIf="followUpError && activeAppointmentId === item.appointmentId" class="form-error">{{ followUpError }}</div>

            <div class="follow-up-actions">
              <button type="submit" class="btn btn-primary" [disabled]="savingAppointmentId === item.appointmentId">
                Save follow-up
              </button>
              <button
                type="button"
                class="btn btn-secondary"
                [disabled]="savingAppointmentId === item.appointmentId"
                (click)="cancelFollowUp()">
                Cancel
              </button>
            </div>
          </form>
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

    .btn {
      border: none;
      border-radius: 999px;
      padding: 0.75rem 1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: #0a5bb5;
      color: #ffffff;
    }

    .btn-secondary {
      margin-top: 0.75rem;
      background: #e5edf5;
      color: #17304d;
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.65;
    }

    .follow-up-form {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      margin-top: 0.85rem;
      border-top: 1px solid #dce6ef;
      padding-top: 0.85rem;
    }

    .manual-record-note,
    .control-wide,
    .template-helper,
    .form-error,
    .follow-up-actions {
      grid-column: 1 / -1;
    }

    .manual-record-note {
      margin: 0;
      color: #5b6e84;
      font-weight: 700;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: #16324f;
      font-weight: 700;
    }

    select,
    textarea {
      width: 100%;
      border: 1px solid #c8d4df;
      border-radius: 14px;
      padding: 0.75rem 0.85rem;
      font: inherit;
      background: #ffffff;
      box-sizing: border-box;
    }

    textarea {
      resize: vertical;
      min-height: 84px;
    }

    .checkbox-control {
      display: flex;
      gap: 0.55rem;
      align-items: center;
      color: #16324f;
      font-weight: 700;
    }

    .checkbox-control input {
      width: 1rem;
      height: 1rem;
    }

    .form-error {
      color: #8c2525;
      font-weight: 700;
    }

    .template-helper {
      border-radius: 14px;
      border: 1px solid #dce6ef;
      background: #ffffff;
      padding: 0.85rem;
    }

    .template-helper p {
      margin: 0;
      color: #5b6e84;
      font-weight: 700;
    }

    .template-empty {
      margin-top: 0.6rem;
      color: #6a7f96;
    }

    .template-controls {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto;
      gap: 0.75rem;
      align-items: end;
      margin-top: 0.75rem;
    }

    .template-controls .btn-secondary {
      margin-top: 0;
    }

    .template-preview {
      margin-top: 0.75rem;
      border-radius: 14px;
      border: 1px solid #c9dfd2;
      background: #f2fbf5;
      padding: 0.75rem;
    }

    .template-preview strong {
      color: #16324f;
    }

    .template-preview p {
      margin: 0.5rem 0 0;
      color: #42546a;
      white-space: pre-wrap;
      word-break: break-word;
    }

    .template-preview small {
      display: block;
      margin-top: 0.5rem;
      color: #8b4f0f;
      font-weight: 700;
    }

    .template-preview .btn-secondary {
      margin-top: 0.75rem;
    }

    .follow-up-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .follow-up-actions .btn-secondary {
      margin-top: 0;
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

    @media (max-width: 720px) {
      .follow-up-form {
        grid-template-columns: 1fr;
      }

      .template-controls {
        grid-template-columns: 1fr;
      }

      .btn {
        width: 100%;
      }
    }
  `]
})
export class AppointmentReminderWorklistComponent {
  @Input() items: AppointmentReminderWorkItem[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() followUpError: string | null = null;
  @Input() canWrite = false;
  @Input() savingAppointmentId: string | null = null;
  @Input() reminderTemplates: ReminderTemplate[] = [];
  @Input() templatePreview: ReminderTemplatePreview | null = null;
  @Input() previewingTemplateId: string | null = null;
  @Input() templatePreviewError: string | null = null;

  @Output() followUpSaved = new EventEmitter<{
    appointmentId: string;
    value: AppointmentReminderFollowUpFormValue;
  }>();
  @Output() templatePreviewRequested = new EventEmitter<{ templateId: string; appointmentId: string }>();

  readonly channelOptions: AppointmentReminderChannel[] = ['Phone', 'WhatsApp', 'Email', 'Other'];
  readonly outcomeOptions: AppointmentReminderOutcome[] = ['Reached', 'NoAnswer', 'LeftMessage'];
  activeAppointmentId: string | null = null;
  channel: AppointmentReminderChannel = 'Phone';
  outcome: AppointmentReminderOutcome = 'Reached';
  notes = '';
  completeReminder = false;
  confirmAppointment = false;
  selectedTemplateId: string | null = null;
  formError: string | null = null;

  getStateLabel(state: AppointmentReminderState): string {
    return state === 'Due' ? 'Due' : 'Pending';
  }

  startFollowUp(item: AppointmentReminderWorkItem): void {
    this.activeAppointmentId = item.appointmentId;
    this.channel = item.reminderChannel ?? 'Phone';
    this.outcome = 'Reached';
    this.notes = '';
    this.completeReminder = false;
    this.confirmAppointment = false;
    this.selectedTemplateId = null;
    this.formError = null;
  }

  cancelFollowUp(): void {
    this.activeAppointmentId = null;
    this.formError = null;
  }

  submitFollowUp(item: AppointmentReminderWorkItem): void {
    const normalizedNotes = this.notes.trim();

    if (normalizedNotes.length > 500) {
      this.formError = 'Notes must be 500 characters or fewer.';
      return;
    }

    this.formError = null;
    this.followUpSaved.emit({
      appointmentId: item.appointmentId,
      value: {
        channel: this.channel,
        outcome: this.outcome,
        notes: normalizedNotes || null,
        completeReminder: this.completeReminder,
        confirmAppointment: this.confirmAppointment
      }
    });
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

  requestTemplatePreview(item: AppointmentReminderWorkItem): void {
    if (!this.selectedTemplateId) {
      this.formError = 'Select a template before previewing.';
      return;
    }

    this.formError = null;
    this.templatePreviewRequested.emit({
      templateId: this.selectedTemplateId,
      appointmentId: item.appointmentId
    });
  }

  usePreviewAsNote(): void {
    if (!this.templatePreview) {
      return;
    }

    this.notes = this.templatePreview.renderedBody;
  }
}
