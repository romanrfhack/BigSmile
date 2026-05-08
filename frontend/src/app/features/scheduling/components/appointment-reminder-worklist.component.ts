import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  SectionCardComponent,
  StatusBadgeComponent
} from '../../../shared/ui';
import type { StatusBadgeTone } from '../../../shared/ui';
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
  imports: [
    CommonModule,
    FormsModule,
    LocalizedDatePipe,
    TranslatePipe,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    SectionCardComponent,
    StatusBadgeComponent
  ],
  template: `
    <app-section-card
      class="worklist-panel"
      variant="elevated"
      [title]="'Manual reminder work' | t"
      [subtitle]="'Manual record only. BigSmile does not send messages.' | t">
      <span section-card-actions class="section-label">{{ 'Pending reminders' | t }}</span>

      <div *ngIf="loading" class="loading-stack">
        <app-loading-skeleton
          variant="card"
          [ariaLabel]="'Loading manual reminders.' | t">
        </app-loading-skeleton>
        <app-loading-skeleton
          variant="card"
          [ariaLabel]="'Loading manual reminders.' | t">
        </app-loading-skeleton>
      </div>

      <div *ngIf="!loading && error" class="state-card state-error" role="alert">
        {{ error | t }}
      </div>

      <app-empty-state
        *ngIf="!loading && !error && !items.length"
        icon="R"
        [title]="'No pending or due manual reminders for this branch.' | t"
        [description]="'Manual record only. BigSmile does not send messages.' | t">
      </app-empty-state>

      <ol *ngIf="!loading && !error && items.length" class="worklist">
        <li *ngFor="let item of items" class="work-item">
          <div class="work-item-head">
            <strong>{{ item.patientFullName }}</strong>
            <app-status-badge
              size="sm"
              [tone]="getStateTone(item.reminderState)"
              [label]="getStateLabel(item.reminderState)">
            </app-status-badge>
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

              <div *ngIf="templatePreviewError" class="form-error" role="alert">{{ templatePreviewError | t }}</div>
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

            <div *ngIf="formError" class="form-error" role="alert">{{ formError | t }}</div>
            <div *ngIf="followUpError && activeAppointmentId === item.appointmentId" class="form-error" role="alert">{{ followUpError | t }}</div>

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

    .loading-stack,
    .state-card {
      margin-top: 1rem;
    }

    .loading-stack {
      display: grid;
      gap: 0.75rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .state-card {
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-muted);
      padding: 0.9rem 1rem;
    }

    .state-error {
      border: 1px solid var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
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
      border-radius: var(--bsm-radius-sm);
      padding: 0.9rem 1rem;
      background: var(--bsm-color-surface);
    }

    .work-item-head {
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
      color: var(--bsm-color-text);
      font-weight: 700;
    }

    .work-item small {
      display: block;
      margin-top: 0.45rem;
      color: var(--bsm-color-text-muted);
      word-break: break-word;
    }

    .btn {
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.75rem 1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
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

    .btn:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
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
      border-radius: var(--bsm-radius-md);
      padding: 0.75rem 0.85rem;
      font: inherit;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
      box-sizing: border-box;
    }

    select:focus,
    textarea:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
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
      accent-color: var(--bsm-color-primary);
    }

    .form-error {
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .template-helper {
      border-radius: var(--bsm-radius-sm);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-color-bg);
      padding: 0.85rem;
    }

    .template-helper p {
      margin: 0;
      color: var(--bsm-color-text-muted);
      font-weight: 700;
    }

    .template-empty {
      margin-top: 0.6rem;
      color: var(--bsm-color-text-muted);
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
      border-radius: var(--bsm-radius-sm);
      border: 1px solid var(--bsm-color-success-soft);
      background: var(--bsm-color-success-soft);
      padding: 0.75rem;
    }

    .template-preview strong {
      color: var(--bsm-color-text-brand);
    }

    .template-preview p {
      margin: 0.5rem 0 0;
      color: var(--bsm-color-text);
      white-space: pre-wrap;
      word-break: break-word;
    }

    .template-preview small {
      display: block;
      margin-top: 0.5rem;
      color: var(--bsm-color-warning);
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

    @media (max-width: 720px) {
      .loading-stack,
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

  getStateTone(state: AppointmentReminderState): StatusBadgeTone {
    return state === 'Due' ? 'warning' : 'primary';
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
