import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { ClinicalRecordsApiService } from '../data-access/clinical-records-api.service';
import {
  AddClinicalDiagnosisRequest,
  AddClinicalNoteRequest,
  ClinicalRecord,
  SaveClinicalRecordSnapshotRequest
} from '../models/clinical-record.models';

@Injectable({
  providedIn: 'root'
})
export class ClinicalRecordsFacade {
  private readonly clinicalRecordsApi = inject(ClinicalRecordsApiService);

  readonly currentRecord = signal<ClinicalRecord | null>(null);
  readonly loadingRecord = signal(false);
  readonly recordMissing = signal(false);
  readonly recordError = signal<string | null>(null);

  loadRecord(patientId: string): void {
    this.loadingRecord.set(true);
    this.recordMissing.set(false);
    this.recordError.set(null);

    this.clinicalRecordsApi.getClinicalRecord(patientId)
      .subscribe({
        next: (record) => {
          this.currentRecord.set(record);
          this.recordMissing.set(false);
          this.recordError.set(null);
          this.loadingRecord.set(false);
        },
        error: (error: unknown) => {
          this.currentRecord.set(null);
          this.loadingRecord.set(false);

          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.recordMissing.set(true);
            this.recordError.set(null);
            return;
          }

          this.recordMissing.set(false);
          this.recordError.set('The clinical record could not be loaded.');
        }
      });
  }

  clearRecord(): void {
    this.currentRecord.set(null);
    this.loadingRecord.set(false);
    this.recordMissing.set(false);
    this.recordError.set(null);
  }

  createRecord(patientId: string, payload: SaveClinicalRecordSnapshotRequest) {
    return this.clinicalRecordsApi.createClinicalRecord(patientId, payload).pipe(
      tap((record) => this.setLoadedRecord(record))
    );
  }

  updateRecord(patientId: string, payload: SaveClinicalRecordSnapshotRequest) {
    return this.clinicalRecordsApi.updateClinicalRecord(patientId, payload).pipe(
      tap((record) => this.setLoadedRecord(record))
    );
  }

  addNote(patientId: string, payload: AddClinicalNoteRequest) {
    return this.clinicalRecordsApi.addClinicalNote(patientId, payload).pipe(
      tap((record) => this.setLoadedRecord(record))
    );
  }

  addDiagnosis(patientId: string, payload: AddClinicalDiagnosisRequest) {
    return this.clinicalRecordsApi.addDiagnosis(patientId, payload).pipe(
      tap((record) => this.setLoadedRecord(record))
    );
  }

  resolveDiagnosis(patientId: string, diagnosisId: string) {
    return this.clinicalRecordsApi.resolveDiagnosis(patientId, diagnosisId).pipe(
      tap((record) => this.setLoadedRecord(record))
    );
  }

  private setLoadedRecord(record: ClinicalRecord): void {
    this.currentRecord.set(record);
    this.loadingRecord.set(false);
    this.recordMissing.set(false);
    this.recordError.set(null);
  }
}
