import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AppointmentBlockSummary,
  AppointmentReminderLogEntry,
  AppointmentReminderWorkItem,
  AppointmentSummary,
  CalendarView,
  AddAppointmentReminderLogEntryRequest,
  CancelAppointmentRequest,
  ChangeAppointmentConfirmationRequest,
  ConfigureAppointmentManualReminderRequest,
  CreateAppointmentBlockRequest,
  CreateAppointmentRequest,
  ManualReminderFollowUpRequest,
  ManualReminderFollowUpResult,
  PreviewReminderTemplateRequest,
  ReminderTemplate,
  ReminderTemplatePreview,
  RescheduleAppointmentRequest,
  SaveReminderTemplateRequest,
  SchedulingBranch,
  SchedulingPatientLookup,
  UpdateAppointmentRequest
} from '../models/scheduling.models';

@Injectable({
  providedIn: 'root'
})
export class SchedulingApiService {
  private readonly http = inject(HttpClient);
  private readonly appointmentsBaseUrl = `${environment.apiUrl}/api/appointments`;
  private readonly appointmentBlocksBaseUrl = `${environment.apiUrl}/api/appointmentblocks`;
  private readonly reminderTemplatesBaseUrl = `${environment.apiUrl}/api/reminder-templates`;
  private readonly branchesBaseUrl = `${environment.apiUrl}/api/branches`;
  private readonly patientsBaseUrl = `${environment.apiUrl}/api/patients`;

  listAccessibleBranches(): Observable<SchedulingBranch[]> {
    return this.http.get<SchedulingBranch[]>(this.branchesBaseUrl);
  }

  getCalendar(branchId: string, startDate: string, days: number): Observable<CalendarView> {
    const params = new HttpParams()
      .set('branchId', branchId)
      .set('startDate', startDate)
      .set('days', days);

    return this.http.get<CalendarView>(`${this.appointmentsBaseUrl}/calendar`, { params });
  }

  searchPatients(search = '', take = 8): Observable<SchedulingPatientLookup[]> {
    let params = new HttpParams().set('take', take);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<SchedulingPatientLookup[]>(this.patientsBaseUrl, { params });
  }

  createAppointment(payload: CreateAppointmentRequest): Observable<AppointmentSummary> {
    return this.http.post<AppointmentSummary>(this.appointmentsBaseUrl, payload);
  }

  updateAppointment(id: string, payload: UpdateAppointmentRequest): Observable<AppointmentSummary> {
    return this.http.put<AppointmentSummary>(`${this.appointmentsBaseUrl}/${id}`, payload);
  }

  rescheduleAppointment(id: string, payload: RescheduleAppointmentRequest): Observable<AppointmentSummary> {
    return this.http.post<AppointmentSummary>(`${this.appointmentsBaseUrl}/${id}/reschedule`, payload);
  }

  cancelAppointment(id: string, payload: CancelAppointmentRequest): Observable<AppointmentSummary> {
    return this.http.post<AppointmentSummary>(`${this.appointmentsBaseUrl}/${id}/cancel`, payload);
  }

  markAppointmentAttended(id: string): Observable<AppointmentSummary> {
    return this.http.post<AppointmentSummary>(`${this.appointmentsBaseUrl}/${id}/attended`, {});
  }

  markAppointmentNoShow(id: string): Observable<AppointmentSummary> {
    return this.http.post<AppointmentSummary>(`${this.appointmentsBaseUrl}/${id}/no-show`, {});
  }

  confirmAppointment(id: string): Observable<AppointmentSummary> {
    const payload: ChangeAppointmentConfirmationRequest = { status: 'Confirmed' };
    return this.http.put<AppointmentSummary>(`${this.appointmentsBaseUrl}/${id}/confirmation`, payload);
  }

  markAppointmentConfirmationPending(id: string): Observable<AppointmentSummary> {
    const payload: ChangeAppointmentConfirmationRequest = { status: 'Pending' };
    return this.http.put<AppointmentSummary>(`${this.appointmentsBaseUrl}/${id}/confirmation`, payload);
  }

  listManualReminders(branchId: string, includeCompleted = false): Observable<AppointmentReminderWorkItem[]> {
    const params = new HttpParams()
      .set('branchId', branchId)
      .set('includeCompleted', includeCompleted);

    return this.http.get<AppointmentReminderWorkItem[]>(`${this.appointmentsBaseUrl}/manual-reminders`, { params });
  }

  configureManualReminder(
    appointmentId: string,
    payload: ConfigureAppointmentManualReminderRequest
  ): Observable<AppointmentSummary> {
    return this.http.put<AppointmentSummary>(
      `${this.appointmentsBaseUrl}/${appointmentId}/manual-reminder`,
      payload);
  }

  completeManualReminder(appointmentId: string): Observable<AppointmentSummary> {
    return this.http.put<AppointmentSummary>(
      `${this.appointmentsBaseUrl}/${appointmentId}/manual-reminder/complete`,
      {});
  }

  getReminderLog(appointmentId: string): Observable<AppointmentReminderLogEntry[]> {
    return this.http.get<AppointmentReminderLogEntry[]>(`${this.appointmentsBaseUrl}/${appointmentId}/reminder-log`);
  }

  addReminderLogEntry(
    appointmentId: string,
    payload: AddAppointmentReminderLogEntryRequest
  ): Observable<AppointmentReminderLogEntry> {
    return this.http.post<AppointmentReminderLogEntry>(
      `${this.appointmentsBaseUrl}/${appointmentId}/reminder-log`,
      payload);
  }

  recordManualReminderFollowUp(
    appointmentId: string,
    payload: ManualReminderFollowUpRequest
  ): Observable<ManualReminderFollowUpResult> {
    return this.http.post<ManualReminderFollowUpResult>(
      `${this.appointmentsBaseUrl}/${appointmentId}/manual-reminder/follow-up`,
      payload);
  }

  listReminderTemplates(includeInactive = false): Observable<ReminderTemplate[]> {
    const params = new HttpParams().set('includeInactive', includeInactive);

    return this.http.get<ReminderTemplate[]>(this.reminderTemplatesBaseUrl, { params });
  }

  createReminderTemplate(payload: SaveReminderTemplateRequest): Observable<ReminderTemplate> {
    return this.http.post<ReminderTemplate>(this.reminderTemplatesBaseUrl, payload);
  }

  updateReminderTemplate(id: string, payload: SaveReminderTemplateRequest): Observable<ReminderTemplate> {
    return this.http.put<ReminderTemplate>(`${this.reminderTemplatesBaseUrl}/${id}`, payload);
  }

  deactivateReminderTemplate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.reminderTemplatesBaseUrl}/${id}`);
  }

  previewReminderTemplate(
    templateId: string,
    payload: PreviewReminderTemplateRequest
  ): Observable<ReminderTemplatePreview> {
    return this.http.post<ReminderTemplatePreview>(
      `${this.reminderTemplatesBaseUrl}/${templateId}/preview`,
      payload);
  }

  createAppointmentBlock(payload: CreateAppointmentBlockRequest): Observable<AppointmentBlockSummary> {
    return this.http.post<AppointmentBlockSummary>(this.appointmentBlocksBaseUrl, payload);
  }

  deleteAppointmentBlock(id: string): Observable<void> {
    return this.http.delete<void>(`${this.appointmentBlocksBaseUrl}/${id}`);
  }
}
