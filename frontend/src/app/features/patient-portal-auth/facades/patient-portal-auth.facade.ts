import { Injectable, signal } from '@angular/core';
import { Observable, catchError, finalize, of, tap, throwError } from 'rxjs';
import { PatientPortalAuthApi } from '../data-access/patient-portal-auth.api';
import {
  ActivatePatientPortalAccountRequest,
  CurrentPatientPortalSession,
  LoginPatientPortalAccountRequest,
  PatientPortalAuthenticationResponse
} from '../models/patient-portal-auth.models';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';

@Injectable({ providedIn: 'root' })
export class PatientPortalAuthFacade {
  private readonly loadingState = signal(false);
  private readonly errorState = signal<string | null>(null);

  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly current = this.sessionStore.current;

  constructor(
    private readonly api: PatientPortalAuthApi,
    private readonly sessionStore: PatientPortalSessionStore
  ) {}

  activate(request: ActivatePatientPortalAccountRequest): Observable<PatientPortalAuthenticationResponse> {
    this.beginRequest();

    return this.api.activate(request).pipe(
      tap(response => this.sessionStore.setSession(response)),
      catchError(error => {
        this.errorState.set('Patient portal activation could not be completed.');
        return throwError(() => error);
      }),
      finalize(() => this.loadingState.set(false))
    );
  }

  login(
    tenantSubdomain: string,
    request: LoginPatientPortalAccountRequest
  ): Observable<PatientPortalAuthenticationResponse> {
    this.beginRequest();

    return this.api.login(tenantSubdomain, request).pipe(
      tap(response => this.sessionStore.setSession(response)),
      catchError(error => {
        this.errorState.set('The supplied patient portal credential is not valid.');
        return throwError(() => error);
      }),
      finalize(() => this.loadingState.set(false))
    );
  }

  refreshCurrent(): Observable<CurrentPatientPortalSession> {
    this.beginRequest();

    return this.api.getCurrent().pipe(
      tap(current => this.sessionStore.updateCurrent(current)),
      catchError(error => {
        this.sessionStore.clear();
        this.errorState.set('The patient portal session is no longer available.');
        return throwError(() => error);
      }),
      finalize(() => this.loadingState.set(false))
    );
  }

  logout(): Observable<void> {
    this.beginRequest();

    return this.api.logout().pipe(
      catchError(() => {
        this.errorState.set('The patient portal session ended locally. Server confirmation was unavailable.');
        return of(void 0);
      }),
      finalize(() => {
        this.sessionStore.clear();
        this.loadingState.set(false);
      })
    );
  }

  clearError(): void {
    this.errorState.set(null);
  }

  private beginRequest(): void {
    this.loadingState.set(true);
    this.errorState.set(null);
  }
}
