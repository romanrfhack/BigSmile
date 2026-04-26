import { Injectable, inject, signal } from '@angular/core';
import { finalize, tap } from 'rxjs';
import { SchedulingApiService } from '../data-access/scheduling-api.service';
import {
  CreateAppointmentBlockRequest,
  CalendarView,
  CalendarViewMode,
  CancelAppointmentRequest,
  CreateAppointmentRequest,
  RescheduleAppointmentRequest,
  SchedulingBranch,
  SchedulingPatientLookup,
  UpdateAppointmentRequest
} from '../models/scheduling.models';

@Injectable({
  providedIn: 'root'
})
export class SchedulingFacade {
  private readonly schedulingApi = inject(SchedulingApiService);

  readonly branches = signal<SchedulingBranch[]>([]);
  readonly selectedBranchId = signal<string | null>(null);
  readonly selectedDate = signal(getTodayIsoDate());
  readonly viewMode = signal<CalendarViewMode>('day');
  readonly calendar = signal<CalendarView | null>(null);
  readonly patientOptions = signal<SchedulingPatientLookup[]>([]);
  readonly loadingBranches = signal(false);
  readonly loadingCalendar = signal(false);
  readonly loadingPatients = signal(false);
  readonly branchesError = signal<string | null>(null);
  readonly calendarError = signal<string | null>(null);

  loadInitialContext(preferredBranchId?: string | null): void {
    this.loadingBranches.set(true);
    this.branchesError.set(null);

    this.schedulingApi.listAccessibleBranches()
      .pipe(finalize(() => this.loadingBranches.set(false)))
      .subscribe({
        next: (branches) => {
          this.branches.set(branches);

          const preferredBranch = preferredBranchId && branches.some(branch => branch.id === preferredBranchId)
            ? preferredBranchId
            : branches[0]?.id ?? null;

          this.selectedBranchId.set(preferredBranch);
          this.loadCalendar();
        },
        error: () => {
          this.branches.set([]);
          this.selectedBranchId.set(null);
          this.calendar.set(null);
          this.branchesError.set('Accessible branches could not be loaded.');
        }
      });
  }

  selectBranch(branchId: string | null): void {
    this.selectedBranchId.set(branchId || null);
    this.loadCalendar();
  }

  setDate(date: string): void {
    this.selectedDate.set(date || getTodayIsoDate());
    this.loadCalendar();
  }

  setViewMode(mode: CalendarViewMode): void {
    this.viewMode.set(mode);
    this.loadCalendar();
  }

  loadCalendar(): void {
    const branchId = this.selectedBranchId();
    if (!branchId) {
      this.calendar.set(null);
      this.calendarError.set(null);
      return;
    }

    this.loadingCalendar.set(true);
    this.calendarError.set(null);

    this.schedulingApi.getCalendar(
      branchId,
      this.selectedDate(),
      this.viewMode() === 'day' ? 1 : 7)
      .pipe(finalize(() => this.loadingCalendar.set(false)))
      .subscribe({
        next: (calendar) => this.calendar.set(calendar),
        error: () => {
          this.calendar.set(null);
          this.calendarError.set('The scheduling calendar could not be loaded.');
        }
      });
  }

  searchPatients(searchTerm: string): void {
    const normalizedSearch = searchTerm.trim();
    if (!normalizedSearch) {
      this.patientOptions.set([]);
      return;
    }

    this.loadingPatients.set(true);

    this.schedulingApi.searchPatients(normalizedSearch)
      .pipe(finalize(() => this.loadingPatients.set(false)))
      .subscribe({
        next: (patients) => this.patientOptions.set(patients),
        error: () => this.patientOptions.set([])
      });
  }

  clearPatientOptions(): void {
    this.patientOptions.set([]);
  }

  createAppointment(payload: CreateAppointmentRequest) {
    return this.schedulingApi.createAppointment(payload).pipe(
      tap(() => this.loadCalendar())
    );
  }

  updateAppointment(id: string, payload: UpdateAppointmentRequest) {
    return this.schedulingApi.updateAppointment(id, payload).pipe(
      tap(() => this.loadCalendar())
    );
  }

  rescheduleAppointment(id: string, payload: RescheduleAppointmentRequest) {
    return this.schedulingApi.rescheduleAppointment(id, payload).pipe(
      tap(() => this.loadCalendar())
    );
  }

  cancelAppointment(id: string, payload: CancelAppointmentRequest) {
    return this.schedulingApi.cancelAppointment(id, payload).pipe(
      tap(() => this.loadCalendar())
    );
  }

  markAppointmentAttended(id: string) {
    return this.schedulingApi.markAppointmentAttended(id).pipe(
      tap(() => this.loadCalendar())
    );
  }

  markAppointmentNoShow(id: string) {
    return this.schedulingApi.markAppointmentNoShow(id).pipe(
      tap(() => this.loadCalendar())
    );
  }

  confirmAppointment(id: string) {
    return this.schedulingApi.confirmAppointment(id).pipe(
      tap(() => this.loadCalendar())
    );
  }

  markAppointmentConfirmationPending(id: string) {
    return this.schedulingApi.markAppointmentConfirmationPending(id).pipe(
      tap(() => this.loadCalendar())
    );
  }

  createAppointmentBlock(payload: CreateAppointmentBlockRequest) {
    return this.schedulingApi.createAppointmentBlock(payload).pipe(
      tap(() => this.loadCalendar())
    );
  }

  deleteAppointmentBlock(id: string) {
    return this.schedulingApi.deleteAppointmentBlock(id).pipe(
      tap(() => this.loadCalendar())
    );
  }
}

function getTodayIsoDate(): string {
  return new Date().toISOString().slice(0, 10);
}
