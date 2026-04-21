import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { tap } from 'rxjs';
import { OdontogramsApiService } from '../data-access/odontograms-api.service';
import { Odontogram, UpdateToothStatusRequest } from '../models/odontogram.models';

@Injectable({
  providedIn: 'root'
})
export class OdontogramsFacade {
  private readonly odontogramsApi = inject(OdontogramsApiService);

  readonly currentOdontogram = signal<Odontogram | null>(null);
  readonly loadingOdontogram = signal(false);
  readonly odontogramMissing = signal(false);
  readonly odontogramError = signal<string | null>(null);

  loadOdontogram(patientId: string): void {
    this.loadingOdontogram.set(true);
    this.odontogramMissing.set(false);
    this.odontogramError.set(null);

    this.odontogramsApi.getOdontogram(patientId)
      .subscribe({
        next: (odontogram) => {
          this.currentOdontogram.set(odontogram);
          this.loadingOdontogram.set(false);
          this.odontogramMissing.set(false);
          this.odontogramError.set(null);
        },
        error: (error: unknown) => {
          this.currentOdontogram.set(null);
          this.loadingOdontogram.set(false);

          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.odontogramMissing.set(true);
            this.odontogramError.set(null);
            return;
          }

          this.odontogramMissing.set(false);
          this.odontogramError.set('The odontogram could not be loaded.');
        }
      });
  }

  clearOdontogram(): void {
    this.currentOdontogram.set(null);
    this.loadingOdontogram.set(false);
    this.odontogramMissing.set(false);
    this.odontogramError.set(null);
  }

  createOdontogram(patientId: string) {
    return this.odontogramsApi.createOdontogram(patientId).pipe(
      tap((odontogram) => this.setLoadedOdontogram(odontogram))
    );
  }

  updateToothStatus(patientId: string, toothCode: string, payload: UpdateToothStatusRequest) {
    return this.odontogramsApi.updateToothStatus(patientId, toothCode, payload).pipe(
      tap((odontogram) => this.setLoadedOdontogram(odontogram))
    );
  }

  private setLoadedOdontogram(odontogram: Odontogram): void {
    this.currentOdontogram.set(odontogram);
    this.loadingOdontogram.set(false);
    this.odontogramMissing.set(false);
    this.odontogramError.set(null);
  }
}
