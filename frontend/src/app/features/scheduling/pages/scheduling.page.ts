import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { AppointmentBlockFormComponent } from '../components/appointment-block-form.component';
import { AppointmentCalendarComponent } from '../components/appointment-calendar.component';
import { AppointmentFormComponent } from '../components/appointment-form.component';
import { SchedulingFacade } from '../facades/scheduling.facade';
import {
  AppointmentBlockFormValue,
  AppointmentBlockSummary,
  AppointmentEditorMode,
  AppointmentFormValue,
  AppointmentSummary,
  CalendarViewMode
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
    AppointmentBlockFormComponent
  ],
  template: `
    <section class="scheduling-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">Release 2 / Scheduling</p>
          <h2>Scheduling foundation</h2>
          <p class="subtitle">
            Daily and weekly branch-aware scheduling for {{ tenantName }} on top of the completed Patients module.
          </p>
        </div>

        <div class="header-actions" *ngIf="canWrite">
          <button type="button" class="btn btn-primary" (click)="startCreate()">
            New appointment
          </button>
          <button type="button" class="btn btn-block" (click)="startCreateBlock()">
            Block time
          </button>
        </div>
      </header>

      <section class="toolbar">
        <label class="control">
          <span>Branch</span>
          <select
            [ngModel]="schedulingFacade.selectedBranchId()"
            (ngModelChange)="changeBranch($event)"
            [disabled]="schedulingFacade.loadingBranches()">
            <option [ngValue]="null">Select a branch</option>
            <option *ngFor="let branch of schedulingFacade.branches()" [ngValue]="branch.id">
              {{ branch.name }}
            </option>
          </select>
        </label>

        <label class="control">
          <span>Calendar date</span>
          <input
            type="date"
            [ngModel]="schedulingFacade.selectedDate()"
            (ngModelChange)="changeDate($event)" />
        </label>

        <label class="control">
          <span>View</span>
          <select
            [ngModel]="schedulingFacade.viewMode()"
            (ngModelChange)="changeViewMode($event)">
            <option value="day">Day</option>
            <option value="week">Week</option>
          </select>
        </label>
      </section>

      <div *ngIf="schedulingFacade.branchesError()" class="state-card state-error">
        {{ schedulingFacade.branchesError() }}
      </div>

      <div *ngIf="!schedulingFacade.loadingBranches() && !schedulingFacade.branches().length" class="state-card">
        No accessible branches are available for the current scheduling session.
      </div>

      <div *ngIf="selectedAppointment" class="selection-card">
        <div>
          <p class="selection-label">Selected appointment</p>
          <strong>{{ selectedAppointment.patientFullName }}</strong>
          <p>
            {{ selectedAppointment.startsAt | date: 'medium' }} to {{ selectedAppointment.endsAt | date: 'shortTime' }}
          </p>
          <p class="confirmation-note">
            Confirmation:
            <strong [class.confirmed-text]="selectedAppointment.confirmationStatus === 'Confirmed'">
              {{ selectedAppointment.confirmationStatus }}
            </strong>
            <span *ngIf="selectedAppointment.confirmedAtUtc">
              · {{ selectedAppointment.confirmedAtUtc | date: 'short' }}
            </span>
          </p>
          <p *ngIf="selectedAppointment.status === 'Cancelled'" class="cancelled-note">
            This appointment is cancelled and remains visible only for calendar traceability.
          </p>
          <p *ngIf="selectedAppointment.status === 'Attended'" class="attended-note">
            This appointment was marked as attended and is now read-only in the calendar.
          </p>
          <p *ngIf="selectedAppointment.status === 'NoShow'" class="no-show-note">
            This appointment was marked as no-show and remains visible for operational follow-up.
          </p>
        </div>

        <div class="selection-actions" *ngIf="canWrite && isScheduledAppointment(selectedAppointment)">
          <button
            *ngIf="selectedAppointment.confirmationStatus === 'Pending'"
            type="button"
            class="btn btn-success"
            (click)="confirmSelectedAppointment()">
            Confirm appointment
          </button>
          <button
            *ngIf="selectedAppointment.confirmationStatus === 'Confirmed'"
            type="button"
            class="btn btn-warning"
            (click)="markSelectedConfirmationPending()">
            Mark as pending
          </button>
          <button type="button" class="btn btn-secondary" (click)="startEdit(selectedAppointment)">Edit</button>
          <button type="button" class="btn btn-secondary" (click)="startReschedule(selectedAppointment)">Reschedule</button>
          <button type="button" class="btn btn-success" (click)="markSelectedAttended()">Mark attended</button>
          <button type="button" class="btn btn-warning" (click)="markSelectedNoShow()">Mark no-show</button>
          <button type="button" class="btn btn-danger" (click)="cancelSelected()">Cancel appointment</button>
        </div>
      </div>

      <div *ngIf="selectedBlockedSlot" class="selection-card selection-card-block">
        <div>
          <p class="selection-label">Selected blocked slot</p>
          <strong>{{ selectedBlockedSlot.label || 'Blocked slot' }}</strong>
          <p>
            {{ selectedBlockedSlot.startsAt | date: 'medium' }} to {{ selectedBlockedSlot.endsAt | date: 'shortTime' }}
          </p>
          <p class="blocked-note">
            Appointments are blocked for this branch during the selected range.
          </p>
        </div>

        <div class="selection-actions" *ngIf="canWrite">
          <button type="button" class="btn btn-danger" (click)="removeSelectedBlock()">Remove blocked slot</button>
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
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
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
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2 {
      margin: 0;
      color: #16324f;
      font-size: clamp(1.8rem, 2vw, 2.4rem);
    }

    .subtitle,
    .selection-card p {
      margin: 0.5rem 0 0;
      color: #5b6e84;
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
      color: #16324f;
      font-weight: 600;
    }

    .control select,
    .control input {
      border: 1px solid #c8d4df;
      border-radius: 14px;
      padding: 0.85rem 0.95rem;
      font: inherit;
      background: #ffffff;
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
      border-radius: 999px;
      padding: 0.85rem 1.1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: #0a5bb5;
      color: #ffffff;
    }

    .btn-secondary {
      background: #e5edf5;
      color: #17304d;
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

  tenantName = 'the current tenant';
  editorSurface: SchedulingEditorSurface = 'appointment';
  editorMode: AppointmentEditorMode = 'create';
  selectedAppointment: AppointmentSummary | null = null;
  selectedBlockedSlot: AppointmentBlockSummary | null = null;
  blockEditorRevision = 0;
  saving = false;
  submitError: string | null = null;

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
    this.tenantName = this.authService.getCurrentTenant()?.name ?? 'the current tenant';
    const preferredBranchId = this.authService.getCurrent()?.currentBranch?.id ?? null;
    this.schedulingFacade.loadInitialContext(preferredBranchId);
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
  }

  startReschedule(appointment: AppointmentSummary): void {
    this.editorSurface = 'appointment';
    this.selectedBlockedSlot = null;
    this.selectedAppointment = appointment;
    this.editorMode = 'reschedule';
    this.submitError = null;
  }

  selectAppointment(appointment: AppointmentSummary): void {
    this.editorSurface = 'appointment';
    this.selectedBlockedSlot = null;
    this.selectedAppointment = appointment;
    this.editorMode = this.isScheduledAppointment(appointment) ? 'edit' : 'create';
    this.submitError = null;
  }

  selectBlockedSlot(blockedSlot: AppointmentBlockSummary): void {
    this.editorSurface = 'block';
    this.selectedAppointment = null;
    this.selectedBlockedSlot = blockedSlot;
    this.editorMode = 'create';
    this.submitError = null;
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

    const confirmed = window.confirm(`Cancel the appointment for ${this.selectedAppointment.patientFullName}?`);
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

    const confirmed = window.confirm(`Mark the appointment for ${this.selectedAppointment.patientFullName} as attended?`);
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

    const confirmed = window.confirm(`Mark the appointment for ${this.selectedAppointment.patientFullName} as no-show?`);
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

    const confirmed = window.confirm(`Confirm the appointment for ${this.selectedAppointment.patientFullName}?`);
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

    const confirmed = window.confirm(`Mark the appointment for ${this.selectedAppointment.patientFullName} as pending confirmation?`);
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

    const label = this.selectedBlockedSlot.label || 'this blocked slot';
    const confirmed = window.confirm(`Remove ${label}?`);
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

  private clearSelection(): void {
    this.selectedAppointment = null;
    this.selectedBlockedSlot = null;
    this.submitError = null;
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
