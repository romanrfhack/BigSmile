import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, finalize, of, switchMap, tap, throwError } from 'rxjs';
import { PatientIntakeAuthApi } from '../../patient-portal-auth/data-access/patient-intake-auth.api';
import { PatientPortalAuthApi } from '../../patient-portal-auth/data-access/patient-portal-auth.api';
import { PatientIntakeSessionStore } from '../../patient-portal-auth/services/patient-intake-session.store';
import {
  PatientPortalSessionBoundary,
  PatientPortalSessionMode
} from '../../patient-portal-auth/services/patient-portal-session-boundary.service';
import { PatientPortalSessionStore } from '../../patient-portal-auth/services/patient-portal-session.store';
import { PatientIntakeApi } from '../data-access/patient-intake.api';
import { PatientIntakeDraft } from '../models/patient-intake.models';

export type PatientIntakeWorkspaceStatus =
  | 'idle'
  | 'loading'
  | 'ready'
  | 'missing'
  | 'unauthorized'
  | 'error';

@Injectable()
export class PatientIntakeWorkspaceFacade {
  private readonly statusState = signal<PatientIntakeWorkspaceStatus>('idle');
  private readonly modeState = signal<PatientPortalSessionMode | null>(null);
  private readonly intakeState = signal<PatientIntakeDraft | null>(null);
  private readonly errorState = signal<string | null>(null);
  private readonly creatingState = signal(false);
  private tenantRealm = '';

  readonly status = this.statusState.asReadonly();
  readonly mode = this.modeState.asReadonly();
  readonly intake = this.intakeState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly creating = this.creatingState.asReadonly();
  readonly canCreate = computed(() => this.modeState() === 'patient' && this.statusState() === 'missing');

  constructor(
    private readonly intakeApi: PatientIntakeApi,
    private readonly patientAuthApi: PatientPortalAuthApi,
    private readonly intakeAuthApi: PatientIntakeAuthApi,
    private readonly patientSessionStore: PatientPortalSessionStore,
    private readonly intakeSessionStore: PatientIntakeSessionStore,
    private readonly sessionBoundary: PatientPortalSessionBoundary
  ) {}

  initialize(tenantSubdomain: string): void {
    this.tenantRealm = normalizeRealm(tenantSubdomain);
    this.errorState.set(null);
    this.intakeState.set(null);

    const resolution = this.sessionBoundary.resolve();
    if (
      resolution.state !== 'active' ||
      !this.tenantRealm ||
      resolution.session.tenantSubdomain !== this.tenantRealm
    ) {
      this.modeState.set(null);
      this.statusState.set('unauthorized');
      return;
    }

    const mode = resolution.session.mode;
    this.modeState.set(mode);
    this.statusState.set('loading');

    this.refreshCurrent(mode).pipe(
      switchMap(() => this.intakeApi.getCurrent()),
      tap(intake => {
        this.intakeState.set(intake);
        this.statusState.set('ready');
      }),
      catchError(error => {
        this.handleLoadError(error, mode);
        return of(null);
      })
    ).subscribe();
  }

  reload(): void {
    this.initialize(this.tenantRealm);
  }

  createDraft(): void {
    if (!this.canCreate() || this.creatingState()) {
      return;
    }

    this.creatingState.set(true);
    this.errorState.set(null);

    this.intakeApi.create().pipe(
      tap(intake => {
        this.intakeState.set(intake);
        this.statusState.set('ready');
      }),
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.sessionBoundary.clearPatientSession();
          this.statusState.set('unauthorized');
        } else if (error instanceof HttpErrorResponse && error.status === 409) {
          this.errorState.set('A current intake draft already exists. Reload the workspace.');
        } else {
          this.errorState.set('The intake draft could not be created.');
        }

        return of(null);
      }),
      finalize(() => this.creatingState.set(false))
    ).subscribe();
  }

  logout(): Observable<void> {
    const mode = this.modeState();
    if (!mode) {
      this.sessionBoundary.clearAll();
      return of(void 0);
    }

    const request = mode === 'patient'
      ? this.patientAuthApi.logout()
      : this.intakeAuthApi.logout();

    return request.pipe(
      catchError(() => of(void 0)),
      finalize(() => {
        this.sessionBoundary.clearMode(mode);
        this.intakeState.set(null);
        this.modeState.set(null);
        this.statusState.set('unauthorized');
      })
    );
  }

  clearError(): void {
    this.errorState.set(null);
  }

  private refreshCurrent(mode: PatientPortalSessionMode): Observable<unknown> {
    if (mode === 'patient') {
      return this.patientAuthApi.getCurrent().pipe(
        tap(current => this.patientSessionStore.updateCurrent(current))
      );
    }

    return this.intakeAuthApi.getCurrent().pipe(
      tap(current => this.intakeSessionStore.updateCurrent(current))
    );
  }

  private handleLoadError(error: unknown, mode: PatientPortalSessionMode): void {
    this.intakeState.set(null);

    if (error instanceof HttpErrorResponse && error.status === 404) {
      this.statusState.set('missing');
      return;
    }

    if (error instanceof HttpErrorResponse && error.status === 401) {
      this.sessionBoundary.clearMode(mode);
      this.modeState.set(null);
      this.statusState.set('unauthorized');
      return;
    }

    this.errorState.set('The intake workspace is temporarily unavailable.');
    this.statusState.set('error');
  }
}

function normalizeRealm(value: string | null | undefined): string {
  return (value ?? '').trim().toLowerCase();
}
