import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { AppointmentBlockFormComponent } from '../components/appointment-block-form.component';
import { AppointmentCalendarComponent } from '../components/appointment-calendar.component';
import { AppointmentFormComponent } from '../components/appointment-form.component';
import { AppointmentManualReminderComponent } from '../components/appointment-manual-reminder.component';
import { AppointmentReminderLogComponent } from '../components/appointment-reminder-log.component';
import { AppointmentReminderWorklistComponent } from '../components/appointment-reminder-worklist.component';
import { ReminderTemplateManagerComponent } from '../components/reminder-template-manager.component';
import { SchedulingFacade } from '../facades/scheduling.facade';
import {
  AppointmentBlockFormValue,
  AppointmentBlockSummary,
  AppointmentEditorMode,
  AppointmentFormValue,
  AppointmentReminderFollowUpFormValue,
  AppointmentManualReminderFormValue,
  AppointmentReminderLogFormValue,
  AppointmentSummary,
  CalendarViewMode,
  ReminderTemplateFormValue
} from '../models/scheduling.models';

type SchedulingEditorSurface = 'appointment' | 'block';

@Component({
  selector: 'app-scheduling-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AppointmentCalendarComponent,
    AppointmentFormComponent,
    AppointmentBlockFormComponent,
    AppointmentManualReminderComponent,
    AppointmentReminderWorklistComponent,
    AppointmentReminderLogComponent,
    ReminderTemplateManagerComponent,
    LocalizedDatePipe,
    TranslatePipe
  ],
  template: `
    <section class="scheduling-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 2 / {{ 'Scheduling' | t }}</p>
          <h2>{{ 'Scheduling foundation' | t }}</h2>
          <p class="subtitle">
            {{ 'Daily and weekly branch-aware scheduling for {tenantName} on top of the completed Patients module.' | t:{ tenantName } }}
          </p>
        </div>

        <div class="header-actions" *ngIf="canWrite">
          <button type="button" class="btn btn-primary" (click)="startCreate()">
            {{ 'New appointment' | t }}
          </button>
          <button type="button" class="btn btn-block" (click)="startCreateBlock()">
            {{ 'Block time' | t }}
          </button>
        </div>
      </header>

      <section class="toolbar">
        <label class="control">
          <span>{{ 'Branch' | t }}</span>
          <select
            [ngModel]="schedulingFacade.selectedBranchId()"
            (ngModelChange)="changeBranch($event)"
            [disabled]="schedulingFacade.loadingBranches()">
            <option [ngValue]="null">{{ 'Select a branch' | t }}</option>
            <option *ngFor="let branch of schedulingFacade.branches()" [ngValue]="branch.id">
              {{ branch.name }}
            </option>
          </select>
        </label>

        <label class="control">
          <span>{{ 'Calendar date' | t }}</span>
          <input
            type="date"
            [ngModel]="schedulingFacade.selectedDate()"
            (ngModelChange)="changeDate($event)" />
        </label>

        <label class="control">
          <span>{{ 'View' | t }}</span>
          <select
            [ngModel]="schedulingFacade.viewMode()"
            (ngModelChange)="changeViewMode($event)">
            <option value="day">{{ 'Day' | t }}</option>
            <option value="week">{{ 'Week' | t }}</option>
          </select>
        </label>
      </section>

      <div *ngIf="schedulingFacade.branchesError()" class="state-card state-error">
        {{ schedulingFacade.branchesError() | t }}
      </div>

      <div *ngIf="!schedulingFacade.loadingBranches() && !schedulingFacade.branches().length" class="state-card">
        {{ 'No accessible branches are available for the current scheduling session.' | t }}
      </div>

      <app-reminder-template-manager
        [templates]="schedulingFacade.reminderTemplates()"
        [loading]="schedulingFacade.loadingReminderTemplates()"
        [error]="schedulingFacade.reminderTemplatesError()"
        [submitError]="reminderTemplateSubmitError"
        [canWrite]="canWrite"
        [saving]="savingReminderTemplate"
        [previewAppointmentId]="selectedAppointment?.id ?? null"
        [preview]="schedulingFacade.reminderTemplatePreview()"
        [previewing]="!!previewingReminderTemplateId"
        (saved)="saveReminderTemplate($event)"
        (deactivateRequested)="deactivateReminderTemplate($event)"
        (previewRequested)="previewReminderTemplate($event.templateId, $event.appointmentId)">
      </app-reminder-template-manager>

      <app-appointment-reminder-worklist
        *ngIf="schedulingFacade.selectedBranchId()"
        [items]="schedulingFacade.manualReminders()"
        [loading]="schedulingFacade.loadingManualReminders()"
        [error]="schedulingFacade.manualRemindersError()"
        [followUpError]="reminderFollowUpError"
        [canWrite]="canWrite"
        [savingAppointmentId]="savingReminderFollowUpAppointmentId"
        [reminderTemplates]="schedulingFacade.reminderTemplates()"
        [templatePreview]="schedulingFacade.reminderTemplatePreview()"
        [previewingTemplateId]="previewingReminderTemplateId"
        [templatePreviewError]="reminderTemplatePreviewError"
        (followUpSaved)="recordManualReminderFollowUp($event.appointmentId, $event.value)"
        (templatePreviewRequested)="previewReminderTemplate($event.templateId, $event.appointmentId)">
      </app-appointment-reminder-worklist>

      <div *ngIf="selectedAppointment" class="selection-card">
        <div>
          <p class="selection-label">{{ 'Selected appointment' | t }}</p>
          <strong>{{ selectedAppointment.patientFullName }}</strong>
          <p>
            {{ selectedAppointment.startsAt | bsDate: 'medium' }} {{ 'to' | t }} {{ selectedAppointment.endsAt | bsDate: 'shortTime' }}
          </p>
          <p class="confirmation-note">
            {{ 'Confirmation' | t }}:
            <strong [class.confirmed-text]="selectedAppointment.confirmationStatus === 'Confirmed'">
              {{ selectedAppointment.confirmationStatus | t }}
            </strong>
            <span *ngIf="selectedAppointment.confirmedAtUtc">
              / {{ selectedAppointment.confirmedAtUtc | bsDate: 'short' }}
            </span>
          </p>
          <p *ngIf="selectedAppointment.status === 'Cancelled'" class="cancelled-note">
            {{ 'This appointment is cancelled and remains visible only for calendar traceability.' | t }}
          </p>
          <p *ngIf="selectedAppointment.status === 'Attended'" class="attended-note">
            {{ 'This appointment was marked as attended and is now read-only in the calendar.' | t }}
          </p>
          <p *ngIf="selectedAppointment.status === 'NoShow'" class="no-show-note">
            {{ 'This appointment was marked as no-show and remains visible for operational follow-up.' | t }}
          </p>
        </div>

        <div class="selection-actions" *ngIf="canWrite && isScheduledAppointment(selectedAppointment)">
          <button
            *ngIf="selectedAppointment.confirmationStatus === 'Pending'"
            type="button"
            class="btn btn-success"
            (click)="confirmSelectedAppointment()">
            {{ 'Confirm appointment' | t }}
          </button>
          <button
            *ngIf="selectedAppointment.confirmationStatus === 'Confirmed'"
            type="button"
            class="btn btn-warning"
            (click)="markSelectedConfirmationPending()">
            {{ 'Mark as pending' | t }}
          </button>
          <button type="button" class="btn btn-secondary" (click)="startEdit(selectedAppointment)">{{ 'Edit' | t }}</button>
          <button type="button" class="btn btn-secondary" (click)="startReschedule(selectedAppointment)">{{ 'Reschedule' | t }}</button>
          <button type="button" class="btn btn-success" (click)="markSelectedAttended()">{{ 'Mark attended' | t }}</button>
          <button type="button" class="btn btn-warning" (click)="markSelectedNoShow()">{{ 'Mark no-show' | t }}</button>
          <button type="button" class="btn btn-danger" (click)="cancelSelected()">{{ 'Cancel appointment' | t }}</button>
        </div>
      </div>

      <app-appointment-manual-reminder
        *ngIf="selectedAppointment"
        [appointment]="selectedAppointment"
        [canWrite]="canWrite"
        [saving]="savingManualReminder"
        [error]="manualReminderError"
        (saved)="setManualReminder($event)"
        (cleared)="clearManualReminder()"
        (completed)="completeManualReminder()">
      </app-appointment-manual-reminder>

      <app-appointment-reminder-log
        *ngIf="selectedAppointment"
        [entries]="schedulingFacade.reminderLog()"
        [loading]="schedulingFacade.loadingReminderLog()"
        [error]="reminderLogSubmitError || schedulingFacade.reminderLogError()"
        [canWrite]="canWrite"
        [saving]="savingReminderLog"
        (saved)="addReminderLogEntry($event)">
      </app-appointment-reminder-log>

      <div *ngIf="selectedBlockedSlot" class="selection-card selection-card-block">
        <div>
          <p class="selection-label">{{ 'Selected blocked slot' | t }}</p>
          <strong>{{ selectedBlockedSlot.label || ('Blocked slot' | t) }}</strong>
          <p>
            {{ selectedBlockedSlot.startsAt | bsDate: 'medium' }} {{ 'to' | t }} {{ selectedBlockedSlot.endsAt | bsDate: 'shortTime' }}
          </p>
          <p class="blocked-note">
            {{ 'Appointments are blocked for this branch during the selected range.' | t }}
          </p>
        </div>

        <div class="selection-actions" *ngIf="canWrite">
          <button type="button" class="btn btn-danger" (click)="removeSelectedBlock()">{{ 'Remove blocked slot' | t }}</button>
        </div>
      </div>

      <div class="content-grid" *ngIf="schedulingFacade.branches().length">
        <section class="editor-panel" *ngIf="canWrite">
          <app-appointment-form
            *ngIf="editorSurface === 'appointment'"
            [mode]="editorMode"
            [selectedBranchName]="selectedBranchName"
            [draftDate]="schedulingFacade.selectedDate()"
            [initialAppointment]="editableAppointment"
            [patientOptions]="schedulingFacade.patientOptions()"
            [searchingPatients]="schedulingFacade.loadingPatients()"
            [saving]="saving"
            [error]="submitError"
            (patientSearchChanged)="schedulingFacade.searchPatients($event)"
            (saved)="saveAppointment($event)"
            (cancelled)="resetEditor()">
          </app-appointment-form>

          <app-appointment-block-form
            *ngIf="editorSurface === 'block'"
            [selectedBranchName]="selectedBranchName"
            [draftDate]="schedulingFacade.selectedDate()"
            [revision]="blockEditorRevision"
            [saving]="saving"
            [error]="submitError"
            (saved)="saveBlockedSlot($event)"
            (cancelled)="resetEditor()">
          </app-appointment-block-form>
        </section>

        <section class="calendar-panel" [class.calendar-panel-wide]="!canWrite">
          <app-appointment-calendar
            [calendar]="schedulingFacade.calendar()"
            [loading]="schedulingFacade.loadingCalendar()"
            [error]="schedulingFacade.calendarError()"
            [activeAppointmentId]="selectedAppointment?.id ?? null"
            [activeBlockId]="selectedBlockedSlot?.id ?? null"
            (appointmentSelected)="selectAppointment($event)"
            (blockedSlotSelected)="selectBlockedSlot($event)">
          </app-appointment-calendar>
        </section>
      </div>
    </section>
  `,
  styles: [`
    .scheduling-page {
      display: grid;
      gap: 1.25rem;
    }

    .page-head,
    .toolbar,
    .selection-card,
    .state-card,
    .editor-panel,
    .calendar-panel {
      border-radius: var(--bsm-radius-lg);
      border: 1px solid var(--bsm-color-border);
      background: var(--bsm-gradient-surface);
      padding: 1.4rem 1.5rem;
      box-shadow: var(--bsm-shadow-md);
    }

    .page-head,
    .selection-card {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .selection-card-block {
      border-color: #f0d5ad;
      background: linear-gradient(180deg, #fffaf2 0%, #fff5e7 100%);
    }

    .eyebrow,
    .selection-label {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2 {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: clamp(1.8rem, 2vw, 2.4rem);
    }

    .subtitle,
    .selection-card p {
      margin: 0.5rem 0 0;
      color: var(--bsm-color-text-muted);
      max-width: 60ch;
    }

    .header-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .toolbar {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    }

    .control {
      display: grid;
      gap: 0.45rem;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
    }

    .control select,
    .control input {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.85rem 0.95rem;
      font: inherit;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .control select:focus,
    .control input:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .content-grid {
      display: grid;
      gap: 1.25rem;
      grid-template-columns: minmax(300px, 360px) minmax(0, 1fr);
    }

    .calendar-panel-wide {
      grid-column: 1 / -1;
    }

    .selection-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .cancelled-note {
      color: #9b2d30;
      font-weight: 600;
    }

    .blocked-note {
      color: #8b4f0f;
      font-weight: 600;
    }

    .attended-note {
      color: #1d6a3a;
      font-weight: 600;
    }

    .no-show-note {
      color: #8b4f0f;
      font-weight: 600;
    }

    .confirmation-note strong {
      color: #8b4f0f;
    }

    .confirmation-note .confirmed-text {
      color: #1d6a3a;
    }

    .btn {
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: #ffffff;
    }

    .btn-secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .btn-danger {
      background: #fde3e3;
      color: #9b2d30;
    }

    .btn-success {
      background: #dff2e5;
      color: #1d6a3a;
    }

    .btn-warning {
      background: #fbe6bf;
      color: #8b4f0f;
    }

    .btn-block {
      background: #8b4f0f;
      color: #ffffff;
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }

    @media (max-width: 1080px) {
      .content-grid {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 768px) {
      .page-head,
      .selection-card {
        flex-direction: column;
      }

      .header-actions,
      .selection-actions {
        width: 100%;
      }

      .btn {
        width: 100%;
      }
    }
  `]
})
export class SchedulingPageComponent implements OnInit {
  readonly schedulingFacade = inject(SchedulingFacade);
  private readonly authService = inject(AuthService);
  private readonly i18n = inject(I18nService);

  tenantName = 'the current tenant';
  editorSurface: SchedulingEditorSurface = 'appointment';
  editorMode: AppointmentEditorMode = 'create';
  selectedAppointment: AppointmentSummary | null = null;
  selectedBlockedSlot: AppointmentBlockSummary | null = null;
  blockEditorRevision = 0;
  saving = false;
  savingReminderLog = false;
  savingManualReminder = false;
  savingReminderFollowUpAppointmentId: string | null = null;
  savingReminderTemplate = false;
  previewingReminderTemplateId: string | null = null;
  submitError: string | null = null;
  reminderLogSubmitError: string | null = null;
  manualReminderError: string | null = null;
  reminderFollowUpError: string | null = null;
  reminderTemplateSubmitError: string | null = null;
  reminderTemplatePreviewError: string | null = null;

  get canWrite(): boolean {
    return this.authService.hasPermissions(['scheduling.write']);
  }

  get selectedBranchName(): string {
    const branchId = this.schedulingFacade.selectedBranchId();
    return this.schedulingFacade.branches().find(branch => branch.id === branchId)?.name ?? '';
  }

  get editableAppointment(): AppointmentSummary | null {
    return this.editorMode === 'create'
      ? null
      : this.selectedAppointment;
  }

  ngOnInit(): void {
    this.tenantName = this.authService.getCurrentTenant()?.name ?? this.i18n.translate('the current tenant');
    const preferredBranchId = this.authService.getCurrent()?.currentBranch?.id ?? null;
    this.schedulingFacade.loadInitialContext(preferredBranchId);
    this.schedulingFacade.loadReminderTemplates();
  }

  changeBranch(branchId: string | null): void {
    this.clearSelection();
    this.editorMode = 'create';
    this.schedulingFacade.clearPatientOptions();
    this.schedulingFacade.selectBranch(branchId);
  }

  changeDate(date: string): void {
    this.clearSelection();
    this.editorMode = 'create';
    this.schedulingFacade.setDate(date);
  }

  changeViewMode(mode: CalendarViewMode): void {
    this.clearSelection();
    this.editorMode = 'create';
    this.schedulingFacade.setViewMode(mode);
  }

  startCreate(): void {
    this.editorSurface = 'appointment';
    this.clearSelection();
    this.editorMode = 'create';
    this.schedulingFacade.clearPatientOptions();
  }

  startCreateBlock(): void {
    this.editorSurface = 'block';
    this.blockEditorRevision += 1;
    this.clearSelection();
    this.editorMode = 'create';
    this.schedulingFacade.clearPatientOptions();
  }

  startEdit(appointment: AppointmentSummary): void {
    this.editorSurface = 'appointment';
    this.selectedBlockedSlot = null;
    this.selectedAppointment = appointment;
    this.editorMode = 'edit';
    this.submitError = null;
    this.reminderLogSubmitError = null;
    this.manualReminderError = null;
    this.reminderFollowUpError = null;
    this.reminderTemplatePreviewError = null;
    this.schedulingFacade.loadReminderLog(appointment.id);
  }

  startReschedule(appointment: AppointmentSummary): void {
    this.editorSurface = 'appointment';
    this.selectedBlockedSlot = null;
    this.selectedAppointment = appointment;
    this.editorMode = 'reschedule';
    this.submitError = null;
    this.reminderLogSubmitError = null;
    this.manualReminderError = null;
    this.reminderFollowUpError = null;
    this.reminderTemplatePreviewError = null;
    this.schedulingFacade.loadReminderLog(appointment.id);
  }

  selectAppointment(appointment: AppointmentSummary): void {
    this.editorSurface = 'appointment';
    this.selectedBlockedSlot = null;
    this.selectedAppointment = appointment;
    this.editorMode = this.isScheduledAppointment(appointment) ? 'edit' : 'create';
    this.submitError = null;
    this.reminderLogSubmitError = null;
    this.manualReminderError = null;
    this.reminderFollowUpError = null;
    this.reminderTemplatePreviewError = null;
    this.schedulingFacade.loadReminderLog(appointment.id);
  }

  selectBlockedSlot(blockedSlot: AppointmentBlockSummary): void {
    this.editorSurface = 'block';
    this.selectedAppointment = null;
    this.selectedBlockedSlot = blockedSlot;
    this.editorMode = 'create';
    this.submitError = null;
    this.reminderLogSubmitError = null;
    this.manualReminderError = null;
    this.reminderFollowUpError = null;
    this.reminderTemplatePreviewError = null;
    this.schedulingFacade.clearReminderLog();
    this.schedulingFacade.clearPatientOptions();
  }

  resetEditor(): void {
    if (this.editorSurface === 'block') {
      this.startCreateBlock();
      return;
    }

    this.startCreate();
  }

  saveAppointment(payload: AppointmentFormValue): void {
    const branchId = this.schedulingFacade.selectedBranchId();
    if (!branchId) {
      this.submitError = 'Select a branch before saving appointments.';
      return;
    }

    this.saving = true;
    this.submitError = null;

    const request$ = this.editorMode === 'create'
      ? this.schedulingFacade.createAppointment({
          branchId,
          patientId: payload.patientId ?? '',
          startsAt: payload.startsAt,
          endsAt: payload.endsAt,
          notes: payload.notes
        })
      : this.editorMode === 'reschedule' && this.selectedAppointment
        ? this.schedulingFacade.rescheduleAppointment(this.selectedAppointment.id, {
            startsAt: payload.startsAt,
            endsAt: payload.endsAt
          })
        : this.selectedAppointment
          ? this.schedulingFacade.updateAppointment(this.selectedAppointment.id, {
              patientId: payload.patientId ?? '',
              startsAt: payload.startsAt,
              endsAt: payload.endsAt,
              notes: payload.notes
            })
          : null;

    if (!request$) {
      this.saving = false;
      this.submitError = 'No appointment target is selected for this action.';
      return;
    }

    request$.subscribe({
      next: (appointment) => {
        this.saving = false;
        this.selectedAppointment = appointment.status === 'Scheduled' ? appointment : null;
        this.selectedBlockedSlot = null;
        this.editorMode = appointment.status === 'Scheduled' ? 'edit' : 'create';
        this.schedulingFacade.loadReminderLog(this.selectedAppointment?.id ?? null);
        this.schedulingFacade.clearPatientOptions();
      },
      error: (error) => {
        this.saving = false;
        this.submitError = this.getErrorMessage(error, 'AppointmentsController', 'The appointment could not be saved.');
      }
    });
  }

  saveBlockedSlot(payload: AppointmentBlockFormValue): void {
    const branchId = this.schedulingFacade.selectedBranchId();
    if (!branchId) {
      this.submitError = 'Select a branch before blocking time.';
      return;
    }

    this.saving = true;
    this.submitError = null;

    this.schedulingFacade.createAppointmentBlock({
      branchId,
      startsAt: payload.startsAt,
      endsAt: payload.endsAt,
      label: payload.label
    }).subscribe({
      next: (blockedSlot) => {
        this.saving = false;
        this.selectedAppointment = null;
        this.selectedBlockedSlot = blockedSlot;
        this.schedulingFacade.clearReminderLog();
      },
      error: (error) => {
        this.saving = false;
        this.submitError = this.getErrorMessage(error, 'AppointmentBlocksController', 'The blocked slot could not be saved.');
      }
    });
  }

  cancelSelected(): void {
    if (!this.selectedAppointment || !this.isScheduledAppointment(this.selectedAppointment) || !this.canWrite) {
      return;
    }

    const confirmed = window.confirm(this.i18n.translate('Cancel the appointment for {patientName}?', {
      patientName: this.selectedAppointment.patientFullName
    }));
    if (!confirmed) {
      return;
    }

    this.saving = true;
    this.submitError = null;

    this.schedulingFacade.cancelAppointment(this.selectedAppointment.id, { reason: null })
      .subscribe({
        next: () => {
          this.saving = false;
          this.startCreate();
        },
        error: (error) => {
          this.saving = false;
          this.submitError = this.getErrorMessage(error, 'AppointmentsController', 'The appointment could not be cancelled.');
        }
      });
  }

  markSelectedAttended(): void {
    if (!this.selectedAppointment || !this.isScheduledAppointment(this.selectedAppointment) || !this.canWrite) {
      return;
    }

    const confirmed = window.confirm(this.i18n.translate('Mark the appointment for {patientName} as attended?', {
      patientName: this.selectedAppointment.patientFullName
    }));
    if (!confirmed) {
      return;
    }

    this.saving = true;
    this.submitError = null;

    this.schedulingFacade.markAppointmentAttended(this.selectedAppointment.id)
      .subscribe({
        next: (appointment) => {
          this.saving = false;
          this.selectedAppointment = appointment;
          this.selectedBlockedSlot = null;
          this.editorMode = 'create';
          this.schedulingFacade.loadReminderLog(appointment.id);
          this.schedulingFacade.clearPatientOptions();
        },
        error: (error) => {
          this.saving = false;
          this.submitError = this.getErrorMessage(error, 'AppointmentsController', 'The appointment could not be marked as attended.');
        }
      });
  }

  markSelectedNoShow(): void {
    if (!this.selectedAppointment || !this.isScheduledAppointment(this.selectedAppointment) || !this.canWrite) {
      return;
    }

    const confirmed = window.confirm(this.i18n.translate('Mark the appointment for {patientName} as no-show?', {
      patientName: this.selectedAppointment.patientFullName
    }));
    if (!confirmed) {
      return;
    }

    this.saving = true;
    this.submitError = null;

    this.schedulingFacade.markAppointmentNoShow(this.selectedAppointment.id)
      .subscribe({
        next: (appointment) => {
          this.saving = false;
          this.selectedAppointment = appointment;
          this.selectedBlockedSlot = null;
          this.editorMode = 'create';
          this.schedulingFacade.loadReminderLog(appointment.id);
          this.schedulingFacade.clearPatientOptions();
        },
        error: (error) => {
          this.saving = false;
          this.submitError = this.getErrorMessage(error, 'AppointmentsController', 'The appointment could not be marked as no-show.');
        }
      });
  }

  confirmSelectedAppointment(): void {
    if (!this.selectedAppointment ||
        !this.isScheduledAppointment(this.selectedAppointment) ||
        this.selectedAppointment.confirmationStatus === 'Confirmed' ||
        !this.canWrite) {
      return;
    }

    const confirmed = window.confirm(this.i18n.translate('Confirm the appointment for {patientName}?', {
      patientName: this.selectedAppointment.patientFullName
    }));
    if (!confirmed) {
      return;
    }

    this.saving = true;
    this.submitError = null;

    this.schedulingFacade.confirmAppointment(this.selectedAppointment.id)
      .subscribe({
        next: (appointment) => {
          this.saving = false;
          this.selectedAppointment = appointment;
          this.selectedBlockedSlot = null;
          this.editorMode = 'edit';
          this.schedulingFacade.loadReminderLog(appointment.id);
          this.schedulingFacade.clearPatientOptions();
        },
        error: (error) => {
          this.saving = false;
          this.submitError = this.getErrorMessage(error, 'AppointmentsController', 'The appointment could not be confirmed.');
        }
      });
  }

  markSelectedConfirmationPending(): void {
    if (!this.selectedAppointment ||
        !this.isScheduledAppointment(this.selectedAppointment) ||
        this.selectedAppointment.confirmationStatus === 'Pending' ||
        !this.canWrite) {
      return;
    }

    const confirmed = window.confirm(this.i18n.translate('Mark the appointment for {patientName} as pending confirmation?', {
      patientName: this.selectedAppointment.patientFullName
    }));
    if (!confirmed) {
      return;
    }

    this.saving = true;
    this.submitError = null;

    this.schedulingFacade.markAppointmentConfirmationPending(this.selectedAppointment.id)
      .subscribe({
        next: (appointment) => {
          this.saving = false;
          this.selectedAppointment = appointment;
          this.selectedBlockedSlot = null;
          this.editorMode = 'edit';
          this.schedulingFacade.loadReminderLog(appointment.id);
          this.schedulingFacade.clearPatientOptions();
        },
        error: (error) => {
          this.saving = false;
          this.submitError = this.getErrorMessage(error, 'AppointmentsController', 'The appointment could not be marked pending.');
        }
      });
  }

  removeSelectedBlock(): void {
    if (!this.selectedBlockedSlot || !this.canWrite) {
      return;
    }

    const label = this.selectedBlockedSlot.label || this.i18n.translate('this blocked slot');
    const confirmed = window.confirm(this.i18n.translate('Remove {label}?', { label }));
    if (!confirmed) {
      return;
    }

    this.saving = true;
    this.submitError = null;

    this.schedulingFacade.deleteAppointmentBlock(this.selectedBlockedSlot.id)
      .subscribe({
        next: () => {
          this.saving = false;
          this.startCreateBlock();
        },
        error: (error) => {
          this.saving = false;
          this.submitError = this.getErrorMessage(error, 'AppointmentBlocksController', 'The blocked slot could not be removed.');
        }
      });
  }

  addReminderLogEntry(payload: AppointmentReminderLogFormValue): void {
    if (!this.selectedAppointment || !this.canWrite) {
      return;
    }

    this.savingReminderLog = true;
    this.reminderLogSubmitError = null;

    this.schedulingFacade.addReminderLogEntry(this.selectedAppointment.id, payload)
      .subscribe({
        next: () => {
          this.savingReminderLog = false;
        },
        error: (error) => {
          this.savingReminderLog = false;
          this.reminderLogSubmitError = this.getErrorMessage(
            error,
            'AppointmentsController',
            'The reminder log entry could not be saved.');
        }
      });
  }

  setManualReminder(payload: AppointmentManualReminderFormValue): void {
    if (!this.selectedAppointment || !this.canWrite) {
      return;
    }

    this.savingManualReminder = true;
    this.manualReminderError = null;

    this.schedulingFacade.configureManualReminder(this.selectedAppointment.id, {
      required: true,
      channel: payload.channel,
      dueAtUtc: payload.dueAtUtc
    }).subscribe({
      next: (appointment) => {
        this.savingManualReminder = false;
        this.selectedAppointment = appointment;
      },
      error: (error) => {
        this.savingManualReminder = false;
        this.manualReminderError = this.getErrorMessage(
          error,
          'AppointmentsController',
          'The manual reminder could not be saved.');
      }
    });
  }

  clearManualReminder(): void {
    if (!this.selectedAppointment || !this.canWrite) {
      return;
    }

    this.savingManualReminder = true;
    this.manualReminderError = null;

    this.schedulingFacade.configureManualReminder(this.selectedAppointment.id, {
      required: false,
      channel: null,
      dueAtUtc: null
    }).subscribe({
      next: (appointment) => {
        this.savingManualReminder = false;
        this.selectedAppointment = appointment;
      },
      error: (error) => {
        this.savingManualReminder = false;
        this.manualReminderError = this.getErrorMessage(
          error,
          'AppointmentsController',
          'The manual reminder could not be cleared.');
      }
    });
  }

  completeManualReminder(): void {
    if (!this.selectedAppointment || !this.canWrite) {
      return;
    }

    this.savingManualReminder = true;
    this.manualReminderError = null;

    this.schedulingFacade.completeManualReminder(this.selectedAppointment.id)
      .subscribe({
        next: (appointment) => {
          this.savingManualReminder = false;
          this.selectedAppointment = appointment;
        },
        error: (error) => {
          this.savingManualReminder = false;
          this.manualReminderError = this.getErrorMessage(
            error,
            'AppointmentsController',
            'The manual reminder could not be marked completed.');
        }
      });
  }

  recordManualReminderFollowUp(appointmentId: string, payload: AppointmentReminderFollowUpFormValue): void {
    if (!this.canWrite) {
      return;
    }

    this.savingReminderFollowUpAppointmentId = appointmentId;
    this.reminderFollowUpError = null;

    this.schedulingFacade.recordManualReminderFollowUp(appointmentId, payload)
      .subscribe({
        next: (result) => {
          this.savingReminderFollowUpAppointmentId = null;
          if (this.selectedAppointment?.id === result.appointment.id) {
            this.selectedAppointment = result.appointment;
            this.schedulingFacade.loadReminderLog(result.appointment.id);
          }
        },
        error: (error) => {
          this.savingReminderFollowUpAppointmentId = null;
          this.reminderFollowUpError = this.getErrorMessage(
            error,
            'AppointmentsController',
            'The reminder follow-up could not be saved.');
        }
      });
  }

  saveReminderTemplate(payload: ReminderTemplateFormValue): void {
    if (!this.canWrite) {
      return;
    }

    this.savingReminderTemplate = true;
    this.reminderTemplateSubmitError = null;

    const request$ = payload.id
      ? this.schedulingFacade.updateReminderTemplate(payload.id, {
          name: payload.name,
          body: payload.body
        })
      : this.schedulingFacade.createReminderTemplate({
          name: payload.name,
          body: payload.body
        });

    request$.subscribe({
      next: () => {
        this.savingReminderTemplate = false;
      },
      error: (error) => {
        this.savingReminderTemplate = false;
        this.reminderTemplateSubmitError = this.getErrorMessage(
          error,
          'ReminderTemplatesController',
          'The reminder template could not be saved.');
      }
    });
  }

  deactivateReminderTemplate(templateId: string): void {
    if (!this.canWrite) {
      return;
    }

    const confirmed = window.confirm(this.i18n.translate('Deactivate this reminder template?'));
    if (!confirmed) {
      return;
    }

    this.savingReminderTemplate = true;
    this.reminderTemplateSubmitError = null;

    this.schedulingFacade.deactivateReminderTemplate(templateId)
      .subscribe({
        next: () => {
          this.savingReminderTemplate = false;
        },
        error: (error) => {
          this.savingReminderTemplate = false;
          this.reminderTemplateSubmitError = this.getErrorMessage(
            error,
            'ReminderTemplatesController',
            'The reminder template could not be deactivated.');
        }
      });
  }

  previewReminderTemplate(templateId: string, appointmentId: string): void {
    this.previewingReminderTemplateId = templateId;
    this.reminderTemplatePreviewError = null;

    this.schedulingFacade.previewReminderTemplate(templateId, appointmentId)
      .subscribe({
        next: () => {
          this.previewingReminderTemplateId = null;
        },
        error: (error) => {
          this.previewingReminderTemplateId = null;
          this.reminderTemplatePreviewError = this.getErrorMessage(
            error,
            'ReminderTemplatesController',
            'The reminder template could not be previewed for this appointment.');
        }
      });
  }

  private clearSelection(): void {
    this.selectedAppointment = null;
    this.selectedBlockedSlot = null;
    this.submitError = null;
    this.reminderLogSubmitError = null;
    this.manualReminderError = null;
    this.reminderFollowUpError = null;
    this.reminderTemplatePreviewError = null;
    this.savingReminderFollowUpAppointmentId = null;
    this.schedulingFacade.clearReminderLog();
    this.schedulingFacade.clearReminderTemplatePreview();
  }

  isScheduledAppointment(appointment: AppointmentSummary | null): appointment is AppointmentSummary & { status: 'Scheduled' } {
    return appointment?.status === 'Scheduled';
  }

  private getErrorMessage(error: unknown, controllerName: string, fallback: string): string {
    const apiError = error as {
      error?: {
        errors?: Record<string, string[]>;
        title?: string;
      };
    };

    return apiError.error?.errors?.[controllerName]?.[0]
      ?? apiError.error?.title
      ?? fallback;
  }
}
