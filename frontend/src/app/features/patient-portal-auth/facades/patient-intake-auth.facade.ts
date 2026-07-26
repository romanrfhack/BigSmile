import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, of, tap, throwError } from 'rxjs';
import { PatientIntakeAuthApi } from '../data-access/patient-intake-auth.api';
import {
  ActivatePatientIntakeAccountRequest,
  PatientIntakeAuthenticationResponse
} from '../models/patient-intake-auth.models';
import { PatientIntakeSessionStore } from '../services/patient-intake-session.store';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';

@Injectable({ providedIn: 'root' })
export class PatientIntakeAuthFacade {
  private readonly api = inject(PatientIntakeAuthApi);
  private readonly sessionStore = inject(PatientIntakeSessionStore);
  private readonly linkedPatientSessionStore = inject(PatientPortalSessionStore);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly current = this.sessionStore.current;
  readonly expiresAtUtc = this.sessionStore.expiresAtUtc;

  activate(request: ActivatePatientIntakeAccountRequest): Observable<PatientIntakeAuthenticationResponse> {
    this.beginRequest();
    this.linkedPatientSessionStore.clear();
    this.sessionStore.clear();

    return this.api.activate(request).pipe(
      tap(response => this.sessionStore.setSession(response)),
      catchError(error => {
        this.errorState.set('The access could not be activated. Ask reception for a new link.');
        return throwError(() => error);
      }),
      finalize(() => this.loadingState.set(false))
    );
  }

  logout(): Observable<void> {
    this.beginRequest();

    return this.api.logout().pipe(
      catchError(() => {
        this.errorState.set('The session ended locally. Server confirmation was unavailable.');
        return of(void 0);
      }),
      finalize(() => {
        this.sessionStore.clear();
        this.loadingState.set(false);
      })
    );
  }

  clearSession(): void {
    this.sessionStore.clear();
  }

  clearError(): void {
    this.errorState.set(null);
  }

  private beginRequest(): void {
    this.loadingState.set(true);
    this.errorState.set(null);
  }
}
