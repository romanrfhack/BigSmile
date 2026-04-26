import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AppointmentBlockSummary,
  AppointmentReminderLogEntry,
  AppointmentSummary,
  CalendarView,
  AddAppointmentReminderLogEntryRequest,
  CancelAppointmentRequest,
  ChangeAppointmentConfirmationRequest,
  CreateAppointmentBlockRequest,
  CreateAppointmentRequest,
  RescheduleAppointmentRequest,
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

  createAppointmentBlock(payload: CreateAppointmentBlockRequest): Observable<AppointmentBlockSummary> {
    return this.http.post<AppointmentBlockSummary>(this.appointmentBlocksBaseUrl, payload);
  }

  deleteAppointmentBlock(id: string): Observable<void> {
    return this.http.delete<void>(`${this.appointmentBlocksBaseUrl}/${id}`);
  }
}
