import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, finalize, of, switchMap, tap } from 'rxjs';
import { PatientIntakeAuthApi } from '../../patient-portal-auth/data-access/patient-intake-auth.api';
import { PatientPortalAuthApi } from '../../patient-portal-auth/data-access/patient-portal-auth.api';
import { PatientIntakeSessionStore } from '../../patient-portal-auth/services/patient-intake-session.store';
import {
  PatientPortalSessionBoundary,
  PatientPortalSessionMode
} from '../../patient-portal-auth/services/patient-portal-session-boundary.service';
import { PatientPortalSessionStore } from '../../patient-portal-auth/services/patient-portal-session.store';
import { PatientIntakeApi } from '../data-access/patient-intake.api';
import {
  PatientIntakeDraft,
  PatientIntakeMedicalAnswerFormValue,
  PatientIntakeNonMedicalFormValue,
  PatientIntakeSaveOutcome,
  buildSavePatientIntakeDraftRequest,
  toPatientIntakeMedicalFormValue,
  toPatientIntakeNonMedicalFormValue
} from '../models/patient-intake.models';

export type PatientIntakeWorkspaceStatus =
  | 'idle'
  | 'loading'
  | 'ready'
  | 'missing'
  | 'unauthorized'
  | 'error';

export type PatientIntakeSaveTarget = 'demographics' | 'medical' | null;
export type PatientIntakeBlockingState = 'conflict' | 'expired' | null;
export type PatientIntakeRecoveryState = 'patient-login' | 'waiting-room-reissue' | null;

@Injectable()
export class PatientIntakeWorkspaceFacade {
  private readonly statusState = signal<PatientIntakeWorkspaceStatus>('idle');
  private readonly modeState = signal<PatientPortalSessionMode | null>(null);
  private readonly intakeState = signal<PatientIntakeDraft | null>(null);
  private readonly errorState = signal<string | null>(null);
  private readonly creatingState = signal(false);
  private readonly savingState = signal(false);
  private readonly submittingState = signal(false);
  private readonly saveOutcomeState = signal<PatientIntakeSaveOutcome>(null);
  private readonly saveErrorState = signal<string | null>(null);
  private readonly saveTargetState = signal<PatientIntakeSaveTarget>(null);
  private readonly blockingStateValue = signal<PatientIntakeBlockingState>(null);
  private readonly recoveryStateValue = signal<PatientIntakeRecoveryState>(null);
  private readonly submitErrorState = signal<string | null>(null);
  private tenantRealm = '';

  readonly status = this.statusState.asReadonly();
  readonly mode = this.modeState.asReadonly();
  readonly intake = this.intakeState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly creating = this.creatingState.asReadonly();
  readonly saving = this.savingState.asReadonly();
  readonly submitting = this.submittingState.asReadonly();
  readonly saveOutcome = this.saveOutcomeState.asReadonly();
  readonly saveError = this.saveErrorState.asReadonly();
  readonly saveTarget = this.saveTargetState.asReadonly();
  readonly blockingState = this.blockingStateValue.asReadonly();
  readonly recoveryState = this.recoveryStateValue.asReadonly();
  readonly submitError = this.submitErrorState.asReadonly();
  readonly submitted = computed(() => this.intakeState()?.status === 'Submitted');
  readonly saveBlocked = computed(() => this.blockingStateValue() !== null);
  readonly canCreate = computed(() => this.modeState() === 'patient' && this.statusState() === 'missing');
  readonly canReplaceExpired = computed(() =>
    this.modeState() === 'patient' &&
    this.statusState() === 'ready' &&
    this.blockingStateValue() === 'expired');

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
    this.blockingStateValue.set(null);
    this.recoveryStateValue.set(null);
    this.submitErrorState.set(null);
    this.clearSaveFeedback();

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

  reloadLatest(): void {
    if (this.blockingStateValue() !== 'conflict') {
      return;
    }

    this.initialize(this.tenantRealm);
  }

  createDraft(): void {
    if (!this.canCreate() || this.creatingState()) {
      return;
    }

    this.createCurrentDraft();
  }

  replaceExpiredDraft(): void {
    if (!this.canReplaceExpired() || this.creatingState()) {
      return;
    }

    this.createCurrentDraft();
  }

  saveNonMedicalDraft(
    value: PatientIntakeNonMedicalFormValue,
    medicalAnswers?: readonly PatientIntakeMedicalAnswerFormValue[]
  ): void {
    const intake = this.intakeState();
    if (!intake) {
      return;
    }

    this.saveDraft(
      buildSavePatientIntakeDraftRequest(
        intake,
        value,
        medicalAnswers ?? toPatientIntakeMedicalFormValue(intake)
      ),
      'demographics'
    );
  }

  saveMedicalDraft(
    value: PatientIntakeMedicalAnswerFormValue[],
    nonMedicalValue?: PatientIntakeNonMedicalFormValue
  ): void {
    const intake = this.intakeState();
    if (!intake) {
      return;
    }

    this.saveDraft(
      buildSavePatientIntakeDraftRequest(
        intake,
        nonMedicalValue ?? toPatientIntakeNonMedicalFormValue(intake),
        value
      ),
      'medical'
    );
  }

  submitIntake(
    nonMedicalValue: PatientIntakeNonMedicalFormValue,
    medicalAnswers: readonly PatientIntakeMedicalAnswerFormValue[]
  ): void {
    const intake = this.intakeState();
    const mode = this.modeState();
    if (
      !intake ||
      !mode ||
      this.statusState() !== 'ready' ||
      this.savingState() ||
      this.submittingState() ||
      this.blockingStateValue() !== null ||
      intake.status === 'Submitted'
    ) {
      return;
    }

    this.submittingState.set(true);
    this.submitErrorState.set(null);
    this.clearSaveFeedback();

    const saveRequest = buildSavePatientIntakeDraftRequest(
      intake,
      nonMedicalValue,
      medicalAnswers);
    this.intakeApi.save(saveRequest).pipe(
      tap(response => this.intakeState.set(response.intake)),
      switchMap(response => this.intakeApi.submit({
        concurrencyToken: response.intake.concurrencyToken
      })),
      tap(response => {
        this.intakeState.set(response.intake);
        this.blockingStateValue.set(null);
        this.submitErrorState.set(null);
      }),
      catchError(error => {
        this.handleSubmitError(error, mode);
        return of(null);
      }),
      finalize(() => this.submittingState.set(false))
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
        this.blockingStateValue.set(null);
        this.recoveryStateValue.set(null);
        this.submitErrorState.set(null);
        this.clearSaveFeedback();
      })
    );
  }

  clearError(): void {
    this.errorState.set(null);
  }

  clearSaveFeedback(): void {
    this.saveOutcomeState.set(null);
    this.saveErrorState.set(null);
    this.saveTargetState.set(null);
  }

  clearSubmitError(): void {
    this.submitErrorState.set(null);
  }

  private createCurrentDraft(): void {
    this.creatingState.set(true);
    this.errorState.set(null);
    this.clearSaveFeedback();

    this.intakeApi.create().pipe(
      tap(intake => {
        this.intakeState.set(intake);
        this.blockingStateValue.set(null);
        this.recoveryStateValue.set(null);
        this.statusState.set('ready');
      }),
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.handleSessionFailure('patient');
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

  private saveDraft(
    request: ReturnType<typeof buildSavePatientIntakeDraftRequest>,
    target: Exclude<PatientIntakeSaveTarget, null>
  ): void {
    const mode = this.modeState();
    if (
      !mode ||
      this.statusState() !== 'ready' ||
      this.savingState() ||
      this.blockingStateValue() !== null
    ) {
      return;
    }

    this.savingState.set(true);
    this.saveTargetState.set(target);
    this.saveOutcomeState.set(null);
    this.saveErrorState.set(null);

    this.intakeApi.save(request).pipe(
      tap(response => {
        this.intakeState.set(response.intake);
        this.blockingStateValue.set(null);
        this.saveOutcomeState.set(response.changed ? 'saved' : 'unchanged');
      }),
      catchError(error => {
        this.handleSaveError(error, mode);
        return of(null);
      }),
      finalize(() => this.savingState.set(false))
    ).subscribe();
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
      this.handleSessionFailure(mode);
      return;
    }

    this.errorState.set('The intake workspace is temporarily unavailable.');
    this.statusState.set('error');
  }

  private handleSaveError(error: unknown, mode: PatientPortalSessionMode): void {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      this.handleSessionFailure(mode);
      this.saveErrorState.set('Your intake session is no longer available.');
      return;
    }

    if (error instanceof HttpErrorResponse && error.status === 409) {
      const code = readProblemCode(error);
      if (code === 'patient_intake.expired' || readProblemDetail(error).toLowerCase().includes('expired')) {
        this.blockingStateValue.set('expired');
        this.saveErrorState.set('The current intake draft expired and can no longer be saved.');
        if (mode === 'patient_intake') {
          this.sessionBoundary.clearIntakeSession();
          this.recoveryStateValue.set('waiting-room-reissue');
        }
      } else {
        this.blockingStateValue.set('conflict');
        this.saveErrorState.set('A newer version of this intake exists. Reload it before saving again.');
      }
      return;
    }

    if (error instanceof HttpErrorResponse && error.status === 400) {
      this.saveErrorState.set('Review the highlighted information before saving the draft.');
      return;
    }

    this.saveErrorState.set('The intake draft could not be saved. Try again.');
  }

  private handleSubmitError(error: unknown, mode: PatientPortalSessionMode): void {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      this.handleSessionFailure(mode);
      this.submitErrorState.set('Your intake session is no longer available.');
      return;
    }

    if (error instanceof HttpErrorResponse && error.status === 409) {
      const code = readProblemCode(error);
      if (code === 'patient_intake.incomplete') {
        this.submitErrorState.set('Complete your name, date of birth and every medical-history answer before submitting.');
      } else if (code === 'patient_intake.expired') {
        this.blockingStateValue.set('expired');
        this.submitErrorState.set('The current intake draft expired and can no longer be submitted.');
      } else {
        this.blockingStateValue.set('conflict');
        this.submitErrorState.set('A newer version of this intake exists. Reload it before submitting.');
      }
      return;
    }

    if (error instanceof HttpErrorResponse && error.status === 400) {
      this.submitErrorState.set('Review the information before submitting your medical history.');
      return;
    }

    this.submitErrorState.set('Your medical history could not be submitted. Try again.');
  }

  private handleSessionFailure(mode: PatientPortalSessionMode): void {
    this.sessionBoundary.clearMode(mode);
    this.modeState.set(mode);
    this.statusState.set('unauthorized');
    this.blockingStateValue.set(null);
    this.recoveryStateValue.set(mode === 'patient_intake' ? 'waiting-room-reissue' : 'patient-login');
  }
}

function normalizeRealm(value: string | null | undefined): string {
  return (value ?? '').trim().toLowerCase();
}

function readProblemCode(error: HttpErrorResponse): string {
  const body = error.error as { code?: unknown; extensions?: { code?: unknown } } | null;
  const value = body?.code ?? body?.extensions?.code;
  return typeof value === 'string' ? value : '';
}

function readProblemDetail(error: HttpErrorResponse): string {
  const body = error.error as { detail?: unknown } | null;
  return typeof body?.detail === 'string' ? body.detail : '';
}
