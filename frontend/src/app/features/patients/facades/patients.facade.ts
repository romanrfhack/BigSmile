import { Injectable, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { PatientsApiService } from '../data-access/patients-api.service';
import { PatientDetail, PatientSummary, SavePatientRequest } from '../models/patient.models';

@Injectable({
  providedIn: 'root'
})
export class PatientsFacade {
  private readonly patientsApi = inject(PatientsApiService);

  readonly patients = signal<PatientSummary[]>([]);
  readonly currentPatient = signal<PatientDetail | null>(null);
  readonly loadingPatients = signal(false);
  readonly loadingPatient = signal(false);
  readonly listError = signal<string | null>(null);
  readonly detailError = signal<string | null>(null);

  search(searchTerm = '', includeInactive = false, take = 25): void {
    this.loadingPatients.set(true);
    this.listError.set(null);

    this.patientsApi.searchPatients(searchTerm, includeInactive, take)
      .pipe(finalize(() => this.loadingPatients.set(false)))
      .subscribe({
        next: (patients) => this.patients.set(patients),
        error: () => {
          this.patients.set([]);
          this.listError.set('Patient search could not be completed.');
        }
      });
  }

  loadPatient(id: string): void {
    this.loadingPatient.set(true);
    this.detailError.set(null);

    this.patientsApi.getPatient(id)
      .pipe(finalize(() => this.loadingPatient.set(false)))
      .subscribe({
        next: (patient) => this.currentPatient.set(patient),
        error: () => {
          this.currentPatient.set(null);
          this.detailError.set('The requested patient record could not be loaded.');
        }
      });
  }

  clearCurrentPatient(): void {
    this.currentPatient.set(null);
    this.detailError.set(null);
  }

  createPatient(payload: SavePatientRequest) {
    return this.patientsApi.createPatient(payload);
  }

  updatePatient(id: string, payload: SavePatientRequest) {
    return this.patientsApi.updatePatient(id, payload);
  }
}
