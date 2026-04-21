import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AddTreatmentPlanItemRequest,
  ChangeTreatmentPlanStatusRequest,
  TreatmentPlan
} from '../models/treatment-plan.models';

@Injectable({
  providedIn: 'root'
})
export class TreatmentPlansApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/patients`;

  getTreatmentPlan(patientId: string): Observable<TreatmentPlan> {
    return this.http.get<TreatmentPlan>(`${this.baseUrl}/${patientId}/treatment-plan`);
  }

  createTreatmentPlan(patientId: string): Observable<TreatmentPlan> {
    return this.http.post<TreatmentPlan>(`${this.baseUrl}/${patientId}/treatment-plan`, {});
  }

  addItem(patientId: string, payload: AddTreatmentPlanItemRequest): Observable<TreatmentPlan> {
    return this.http.post<TreatmentPlan>(`${this.baseUrl}/${patientId}/treatment-plan/items`, payload);
  }

  removeItem(patientId: string, itemId: string): Observable<TreatmentPlan> {
    return this.http.delete<TreatmentPlan>(`${this.baseUrl}/${patientId}/treatment-plan/items/${itemId}`);
  }

  changeStatus(patientId: string, payload: ChangeTreatmentPlanStatusRequest): Observable<TreatmentPlan> {
    return this.http.put<TreatmentPlan>(`${this.baseUrl}/${patientId}/treatment-plan/status`, payload);
  }
}
