import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AppointmentSummary,
  CalendarView,
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
export class SchedulingApiService {
  private readonly http = inject(HttpClient);
  private readonly appointmentsBaseUrl = `${environment.apiUrl}/api/appointments`;
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
}
