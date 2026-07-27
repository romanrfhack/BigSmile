import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, of, tap, throwError } from 'rxjs';
import { PatientIntakeAuthApi } from '../data-access/patient-intake-auth.api';
import {
  ActivatePatientIntakeAccountRequest,
  CurrentPatientIntakeSession,
  LoginPatientIntakeAccountRequest,
  PatientIntakeAuthenticationResponse
} from '../models/patient-intake-auth.models';
import { PatientIntakeSessionStore } from '../services/patient-intake-session.store';
import { PatientPortalSessionBoundary } from '../services/patient-portal-session-boundary.service';

@Injectable({ providedIn: 'root' })
export class PatientIntakeAuthFacade {
  private readonly api = inject(PatientIntakeAuthApi);
  private readonly sessionStore = inject(PatientIntakeSessionStore);
  private readonly sessionBoundary = inject(PatientPortalSessionBoundary);
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly current = this.sessionStore.current;
  readonly expiresAtUtc = this.sessionStore.expiresAtUtc;

  activate(request: ActivatePatientIntakeAccountRequest): Observable<PatientIntakeAuthenticationResponse> {
    this.beginRequest();
    this.sessionBoundary.clearAll();

    return this.api.activate(request).pipe(
      tap(response => this.sessionBoundary.setIntakeSession(response)),
      catchError(error => {
        this.errorState.set('The access could not be activated. Ask reception for a new link.');
        return throwError(() => error);
      }),
      finalize(() => this.loadingState.set(false))
    );
  }

  login(
    tenantSubdomain: string,
    request: LoginPatientIntakeAccountRequest
  ): Observable<PatientIntakeAuthenticationResponse> {
    this.beginRequest();
    this.sessionBoundary.clearAll();

    return this.api.login(tenantSubdomain, request).pipe(
      tap(response => this.sessionBoundary.setIntakeSession(response)),
      catchError(error => {
        this.errorState.set('The supplied intake credential is not valid.');
        return throwError(() => error);
      }),
      finalize(() => this.loadingState.set(false))
    );
  }

  refreshCurrent(): Observable<CurrentPatientIntakeSession> {
    this.beginRequest();

    return this.api.getCurrent().pipe(
      tap(current => this.sessionStore.updateCurrent(current)),
      catchError(error => {
        this.sessionBoundary.clearIntakeSession();
        this.errorState.set('The intake session is no longer available.');
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
        this.sessionBoundary.clearIntakeSession();
        this.loadingState.set(false);
      })
    );
  }

  clearSession(): void {
    this.sessionBoundary.clearIntakeSession();
  }

  clearError(): void {
    this.errorState.set(null);
  }

  private beginRequest(): void {
    this.loadingState.set(true);
    this.errorState.set(null);
  }
}
