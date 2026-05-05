import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
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
  imports: [CommonModule, FormsModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="worklist-panel">
      <header class="worklist-head">
        <div>
          <p class="section-label">{{ 'Pending reminders' | t }}</p>
          <h3>{{ 'Manual reminder work' | t }}</h3>
          <p class="manual-note">{{ 'Manual preparation only. No providers, jobs, queues, external delivery templates, scheduler, or retry automation.' | t }}</p>
        </div>
      </header>

      <div *ngIf="loading" class="state-card">
        {{ 'Loading manual reminders.' | t }}
      </div>

      <div *ngIf="!loading && error" class="state-card state-error">
        {{ error | t }}
      </div>

      <div *ngIf="!loading && !error && !items.length" class="state-card">
        {{ 'No pending or due manual reminders for this branch.' | t }}
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
            {{ item.reminderDueAtUtc | bsDate: 'short' }}
            <span *ngIf="item.reminderChannel"> / {{ getChannelLabel(item.reminderChannel) }}</span>
          </p>
          <small>
            {{ 'Appointment' | t }} {{ item.startsAt | bsDate: 'short' }} / {{ item.appointmentStatus | t }} / {{ item.confirmationStatus | t }}
          </small>
          <button
            *ngIf="canWrite"
            type="button"
            class="btn btn-secondary"
            [disabled]="savingAppointmentId === item.appointmentId"
            (click)="startFollowUp(item)">
            {{ 'Record follow-up' | t }}
          </button>

          <form
            *ngIf="activeAppointmentId === item.appointmentId"
            class="follow-up-form"
            (ngSubmit)="submitFollowUp(item)">
            <p class="manual-record-note">{{ 'Manual record only. BigSmile does not send messages.' | t }}</p>

            <label class="control">
              <span>{{ 'Channel' | t }}</span>
              <select
                name="channel-{{ item.appointmentId }}"
                [(ngModel)]="channel"
                [disabled]="savingAppointmentId === item.appointmentId">
                <option *ngFor="let option of channelOptions" [ngValue]="option">
                  {{ getChannelLabel(option) }}
                </option>
              </select>
            </label>

            <label class="control">
              <span>{{ 'Outcome' | t }}</span>
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
              <span>{{ 'Notes' | t }}</span>
              <textarea
                name="notes-{{ item.appointmentId }}"
                rows="3"
                maxlength="500"
                [(ngModel)]="notes"
                [disabled]="savingAppointmentId === item.appointmentId"></textarea>
            </label>

            <div class="template-helper">
              <p>{{ 'Template preview is a manual draft helper only.' | t }}</p>
              <div *ngIf="!reminderTemplates.length" class="template-empty">
                {{ 'No active templates are available.' | t }}
              </div>
              <div *ngIf="reminderTemplates.length" class="template-controls">
                <label class="control">
                  <span>{{ 'Template' | t }}</span>
                  <select
                    name="template-{{ item.appointmentId }}"
                    [(ngModel)]="selectedTemplateId"
                    (ngModelChange)="onTemplateSelectionChanged()"
                    [disabled]="savingAppointmentId === item.appointmentId">
                    <option [ngValue]="null">{{ 'Select a template' | t }}</option>
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
                  {{ 'Preview template' | t }}
                </button>
              </div>

              <div
                *ngIf="templatePreview?.appointmentId === item.appointmentId && templatePreview?.templateId === selectedTemplateId"
                class="template-preview">
                <strong>{{ 'Rendered preview' | t }}</strong>
                <p>{{ templatePreview?.renderedBody }}</p>
                <small *ngIf="templatePreview?.unknownPlaceholders?.length">
                  {{ 'Unknown placeholders:' | t }} {{ templatePreview?.unknownPlaceholders?.join(', ') }}
                </small>
                <button type="button" class="btn btn-secondary" (click)="usePreviewAsNote()">
                  {{ 'Use as note' | t }}
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
              <span>{{ 'Mark reminder completed' | t }}</span>
            </label>

            <label class="checkbox-control">
              <input
                type="checkbox"
                name="confirm-{{ item.appointmentId }}"
                [(ngModel)]="confirmAppointment"
                [disabled]="savingAppointmentId === item.appointmentId" />
              <span>{{ 'Confirm appointment' | t }}</span>
            </label>

            <div *ngIf="formError" class="form-error">{{ formError | t }}</div>
            <div *ngIf="followUpError && activeAppointmentId === item.appointmentId" class="form-error">{{ followUpError | t }}</div>

            <div class="follow-up-actions">
              <button type="submit" class="btn btn-primary" [disabled]="savingAppointmentId === item.appointmentId">
                {{ 'Save follow-up' | t }}
              </button>
              <button
                type="button"
                class="btn btn-secondary"
                [disabled]="savingAppointmentId === item.appointmentId"
                (click)="cancelFollowUp()">
                {{ 'Cancel' | t }}
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
      border: 1px solid var(--bsm-color-border);
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

    .worklist {
      display: grid;
      gap: 0.75rem;
      margin: 1rem 0 0;
      padding: 0;
      list-style: none;
    }

    .work-item {
      border: 1px solid var(--bsm-color-border);
      border-radius: 14px;
      padding: 0.9rem 1rem;
      background: var(--bsm-color-surface);
    }

    .work-item div {
      display: flex;
      gap: 0.5rem;
      align-items: center;
      justify-content: space-between;
    }

    .work-item strong {
      color: var(--bsm-color-text-brand);
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
      background: var(--bsm-color-primary);
      color: #ffffff;
    }

    .btn-secondary {
      margin-top: 0.75rem;
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-text-brand);
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
      border-top: 1px solid var(--bsm-color-border);
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
      color: var(--bsm-color-text-muted);
      font-weight: 700;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: var(--bsm-color-text-brand);
      font-weight: 700;
    }

    select,
    textarea {
      width: 100%;
      border: 1px solid var(--bsm-color-border);
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
      color: var(--bsm-color-text-brand);
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
      border: 1px solid var(--bsm-color-border);
      background: #ffffff;
      padding: 0.85rem;
    }

    .template-helper p {
      margin: 0;
      color: var(--bsm-color-text-muted);
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
      color: var(--bsm-color-text-brand);
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
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-text-brand);
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
  private readonly i18n = inject(I18nService);

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
  selectedTemplateSourceId: string | null = null;
  formError: string | null = null;

  getStateLabel(state: AppointmentReminderState): string {
    return this.i18n.translate(state === 'Due' ? 'Due' : 'Pending');
  }

  startFollowUp(item: AppointmentReminderWorkItem): void {
    this.activeAppointmentId = item.appointmentId;
    this.channel = item.reminderChannel ?? 'Phone';
    this.outcome = 'Reached';
    this.notes = '';
    this.completeReminder = false;
    this.confirmAppointment = false;
    this.selectedTemplateId = null;
    this.selectedTemplateSourceId = null;
    this.formError = null;
  }

  cancelFollowUp(): void {
    this.activeAppointmentId = null;
    this.selectedTemplateId = null;
    this.selectedTemplateSourceId = null;
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
        confirmAppointment: this.confirmAppointment,
        reminderTemplateId: this.selectedTemplateSourceId
      }
    });
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

  getChannelLabel(channel: AppointmentReminderChannel): string {
    return this.i18n.translate(channel);
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

  onTemplateSelectionChanged(): void {
    this.selectedTemplateSourceId = null;
  }

  usePreviewAsNote(): void {
    if (!this.templatePreview || this.templatePreview.templateId !== this.selectedTemplateId) {
      return;
    }

    this.notes = this.templatePreview.renderedBody;
    this.selectedTemplateSourceId = this.templatePreview.templateId;
  }
}
