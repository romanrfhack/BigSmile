import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { MEDICAL_QUESTIONNAIRE_KEYS } from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';
import { PatientIntakeAuthApi } from '../../patient-portal-auth/data-access/patient-intake-auth.api';
import { PatientPortalAuthApi } from '../../patient-portal-auth/data-access/patient-portal-auth.api';
import { PatientIntakeSessionStore } from '../../patient-portal-auth/services/patient-intake-session.store';
import { PatientPortalSessionBoundary } from '../../patient-portal-auth/services/patient-portal-session-boundary.service';
import { PatientPortalSessionStore } from '../../patient-portal-auth/services/patient-portal-session.store';
import { PatientIntakeApi } from '../data-access/patient-intake.api';
import { PatientIntakeDraft, PatientIntakeNonMedicalFormValue } from '../models/patient-intake.models';
import { PatientIntakeWorkspaceFacade } from './patient-intake-workspace.facade';

describe('PatientIntakeWorkspaceFacade lifecycle hardening', () => {
  let patientStore: PatientPortalSessionStore;
  let intakeStore: PatientIntakeSessionStore;
  let boundary: PatientPortalSessionBoundary;
  let intakeApi: PatientIntakeApi & {
    create: ReturnType<typeof vi.fn>;
    getCurrent: ReturnType<typeof vi.fn>;
    save: ReturnType<typeof vi.fn>;
  };
  let patientAuthApi: PatientPortalAuthApi & {
    getCurrent: ReturnType<typeof vi.fn>;
    logout: ReturnType<typeof vi.fn>;
  };
  let intakeAuthApi: PatientIntakeAuthApi & {
    getCurrent: ReturnType<typeof vi.fn>;
    logout: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    patientStore = new PatientPortalSessionStore();
    intakeStore = new PatientIntakeSessionStore();
    boundary = new PatientPortalSessionBoundary(patientStore, intakeStore);
    intakeApi = {
      create: vi.fn().mockReturnValue(of(replacementDraft())),
      getCurrent: vi.fn().mockReturnValue(of(draft())),
      save: vi.fn().mockReturnValue(of({ intake: savedDraft(), changed: true }))
    } as unknown as typeof intakeApi;
    patientAuthApi = {
      getCurrent: vi.fn().mockReturnValue(of(patientCurrent())),
      logout: vi.fn().mockReturnValue(of(void 0))
    } as unknown as typeof patientAuthApi;
    intakeAuthApi = {
      getCurrent: vi.fn().mockReturnValue(of(intakeCurrent())),
      logout: vi.fn().mockReturnValue(of(void 0))
    } as unknown as typeof intakeAuthApi;
  });

  it('retains the authoritative snapshot and blocks repeated stale writes after a conflict', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.save.mockReturnValue(throwError(() => problem(409, 'patient_intake.concurrency_conflict')));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveNonMedicalDraft(nonMedicalValue());
    facade.saveNonMedicalDraft(nonMedicalValue());

    expect(facade.blockingState()).toBe('conflict');
    expect(facade.saveBlocked()).toBe(true);
    expect(facade.intake()?.concurrencyToken).toBe('rv1.token');
    expect(intakeApi.save).toHaveBeenCalledTimes(1);
  });

  it('reloads the latest authoritative snapshot only after the explicit conflict action', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.save.mockReturnValue(throwError(() => problem(409, 'patient_intake.concurrency_conflict')));
    const facade = createFacade();
    facade.initialize('clinic-a');
    facade.saveNonMedicalDraft(nonMedicalValue());

    intakeApi.getCurrent.mockReturnValue(of({ ...draft(), concurrencyToken: 'rv1.latest-token' }));
    facade.reloadLatest();

    expect(intakeApi.getCurrent).toHaveBeenCalledTimes(2);
    expect(facade.blockingState()).toBeNull();
    expect(facade.intake()?.concurrencyToken).toBe('rv1.latest-token');
  });

  it('allows a linked patient to explicitly replace an expired draft', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.save.mockReturnValue(throwError(() => problem(409, 'patient_intake.expired')));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveNonMedicalDraft(nonMedicalValue());
    expect(facade.blockingState()).toBe('expired');
    expect(facade.canReplaceExpired()).toBe(true);

    facade.replaceExpiredDraft();

    expect(intakeApi.create).toHaveBeenCalledTimes(1);
    expect(facade.blockingState()).toBeNull();
    expect(facade.intake()?.concurrencyToken).toBe('rv1.replacement-token');
  });

  it('clears only the intake-only session and requires reception reissue after expiry', () => {
    boundary.setIntakeSession(intakeResponse());
    intakeApi.save.mockReturnValue(throwError(() => problem(409, 'patient_intake.expired')));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveNonMedicalDraft(nonMedicalValue());

    expect(facade.mode()).toBe('patient_intake');
    expect(facade.recoveryState()).toBe('waiting-room-reissue');
    expect(intakeStore.getAccessToken()).toBeNull();
    expect(patientStore.getAccessToken()).toBeNull();
  });

  it('fails closed to the correct login mode when a session refresh is rejected', () => {
    boundary.setPatientSession(patientResponse());
    patientAuthApi.getCurrent.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 401 })));
    const facade = createFacade();

    facade.initialize('clinic-a');

    expect(facade.status()).toBe('unauthorized');
    expect(facade.mode()).toBe('patient');
    expect(facade.recoveryState()).toBe('patient-login');
    expect(patientStore.getAccessToken()).toBeNull();
    expect(intakeApi.getCurrent).not.toHaveBeenCalled();
  });

  function createFacade(): PatientIntakeWorkspaceFacade {
    return new PatientIntakeWorkspaceFacade(
      intakeApi,
      patientAuthApi,
      intakeAuthApi,
      patientStore,
      intakeStore,
      boundary
    );
  }
});

function problem(status: number, code: string): HttpErrorResponse {
  return new HttpErrorResponse({
    status,
    error: {
      status,
      title: 'Patient intake draft conflict.',
      code
    }
  });
}

function patientResponse() {
  return {
    accessToken: 'patient-token',
    expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    current: patientCurrent()
  };
}

function intakeResponse() {
  return {
    accessToken: 'intake-token',
    expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    current: intakeCurrent()
  };
}

function patientCurrent() {
  return {
    accountId: 'patient-account-id',
    patientId: 'patient-id',
    tenantSubdomain: 'clinic-a',
    loginName: 'patient.login',
    sessionVersion: 1
  };
}

function intakeCurrent() {
  return {
    accountId: 'intake-account-id',
    intakeId: 'intake-id',
    tenantSubdomain: 'clinic-a',
    loginName: 'new.patient',
    sessionVersion: 1
  };
}

function nonMedicalValue(): PatientIntakeNonMedicalFormValue {
  return {
    firstName: 'Ana',
    lastName: 'López',
    dateOfBirth: '1990-05-10',
    sex: 'Female',
    occupation: '',
    maritalStatus: 'Single',
    referredBy: '',
    preferredPhone: '',
    mobilePhone: '+52 55 0000 0000',
    homePhone: '',
    workPhone: '',
    email: 'ana@example.test',
    responsiblePartyName: '',
    responsiblePartyRelationship: '',
    responsiblePartyPhone: '',
    reasonForVisit: 'Revisión general.'
  };
}

function draft(): PatientIntakeDraft {
  return {
    origin: 'ExistingPatientPortal',
    status: 'Draft',
    firstName: 'Ana',
    lastName: 'López',
    dateOfBirth: '1990-05-10',
    sex: 'Female',
    occupation: null,
    maritalStatus: 'Single',
    referredBy: null,
    preferredPhone: null,
    mobilePhone: '+52 55 0000 0000',
    homePhone: null,
    workPhone: null,
    email: 'ana@example.test',
    responsiblePartyName: null,
    responsiblePartyRelationship: null,
    responsiblePartyPhone: null,
    reasonForVisit: 'Revisión general.',
    medicalAnswers: MEDICAL_QUESTIONNAIRE_KEYS.map(questionKey => ({
      questionKey,
      answer: 'Unknown',
      details: null
    })),
    currentRevisionNumber: 0,
    concurrencyToken: 'rv1.token',
    createdAtUtc: '2026-07-29T10:00:00Z',
    lastUpdatedAtUtc: '2026-07-29T10:00:00Z',
    lastEffectiveSavedAtUtc: null,
    submittedAtUtc: null,
    expiresAtUtc: '2026-08-28T10:00:00Z'
  };
}

function savedDraft(): PatientIntakeDraft {
  return {
    ...draft(),
    currentRevisionNumber: 1,
    concurrencyToken: 'rv1.saved-token',
    lastEffectiveSavedAtUtc: '2026-07-29T11:00:00Z'
  };
}

function replacementDraft(): PatientIntakeDraft {
  return {
    ...draft(),
    firstName: null,
    lastName: null,
    medicalAnswers: MEDICAL_QUESTIONNAIRE_KEYS.map(questionKey => ({
      questionKey,
      answer: 'Unknown',
      details: null
    })),
    concurrencyToken: 'rv1.replacement-token'
  };
}
