import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { TreatmentPlansApiService } from '../data-access/treatment-plans-api.service';
import {
  AddTreatmentPlanItemRequest,
  ChangeTreatmentPlanStatusRequest,
  TreatmentPlan
} from '../models/treatment-plan.models';

@Injectable({
  providedIn: 'root'
})
export class TreatmentPlansFacade {
  private readonly treatmentPlansApi = inject(TreatmentPlansApiService);

  readonly currentTreatmentPlan = signal<TreatmentPlan | null>(null);
  readonly loadingTreatmentPlan = signal(false);
  readonly treatmentPlanMissing = signal(false);
  readonly treatmentPlanError = signal<string | null>(null);

  loadTreatmentPlan(patientId: string): void {
    this.loadingTreatmentPlan.set(true);
    this.treatmentPlanMissing.set(false);
    this.treatmentPlanError.set(null);

    this.treatmentPlansApi.getTreatmentPlan(patientId)
      .subscribe({
        next: (treatmentPlan) => {
          this.currentTreatmentPlan.set(treatmentPlan);
          this.loadingTreatmentPlan.set(false);
          this.treatmentPlanMissing.set(false);
          this.treatmentPlanError.set(null);
        },
        error: (error: unknown) => {
          this.currentTreatmentPlan.set(null);
          this.loadingTreatmentPlan.set(false);

          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.treatmentPlanMissing.set(true);
            this.treatmentPlanError.set(null);
            return;
          }

          this.treatmentPlanMissing.set(false);
          this.treatmentPlanError.set('The treatment plan could not be loaded.');
        }
      });
  }

  clearTreatmentPlan(): void {
    this.currentTreatmentPlan.set(null);
    this.loadingTreatmentPlan.set(false);
    this.treatmentPlanMissing.set(false);
    this.treatmentPlanError.set(null);
  }

  createTreatmentPlan(patientId: string) {
    return this.treatmentPlansApi.createTreatmentPlan(patientId).pipe(
      tap((treatmentPlan) => this.setLoadedTreatmentPlan(treatmentPlan))
    );
  }

  addItem(patientId: string, payload: AddTreatmentPlanItemRequest) {
    return this.treatmentPlansApi.addItem(patientId, payload).pipe(
      tap((treatmentPlan) => this.setLoadedTreatmentPlan(treatmentPlan))
    );
  }

  removeItem(patientId: string, itemId: string) {
    return this.treatmentPlansApi.removeItem(patientId, itemId).pipe(
      tap((treatmentPlan) => this.setLoadedTreatmentPlan(treatmentPlan))
    );
  }

  changeStatus(patientId: string, payload: ChangeTreatmentPlanStatusRequest) {
    return this.treatmentPlansApi.changeStatus(patientId, payload).pipe(
      tap((treatmentPlan) => this.setLoadedTreatmentPlan(treatmentPlan))
    );
  }

  private setLoadedTreatmentPlan(treatmentPlan: TreatmentPlan): void {
    this.currentTreatmentPlan.set(treatmentPlan);
    this.loadingTreatmentPlan.set(false);
    this.treatmentPlanMissing.set(false);
    this.treatmentPlanError.set(null);
  }
}
