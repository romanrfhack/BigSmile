import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { MEDICAL_QUESTIONNAIRE_KEYS } from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';
import { PatientIntakeAuthApi } from '../../patient-portal-auth/data-access/patient-intake-auth.api';
import { PatientPortalAuthApi } from '../../patient-portal-auth/data-access/patient-portal-auth.api';
import { PatientIntakeSessionStore } from '../../patient-portal-auth/services/patient-intake-session.store';
import { PatientPortalSessionBoundary } from '../../patient-portal-auth/services/patient-portal-session-boundary.service';
import { PatientPortalSessionStore } from '../../patient-portal-auth/services/patient-portal-session.store';
import { PatientIntakeApi } from '../data-access/patient-intake.api';
import {
  PatientIntakeDraft,
  PatientIntakeMedicalAnswerFormValue,
  PatientIntakeNonMedicalFormValue,
  SavePatientIntakeDraftRequest
} from '../models/patient-intake.models';
import { PatientIntakeWorkspaceFacade } from './patient-intake-workspace.facade';

describe('PatientIntakeWorkspaceFacade', () => {
  let patientStore: PatientPortalSessionStore;
  let intakeStore: PatientIntakeSessionStore;
  let boundary: PatientPortalSessionBoundary;
  let intakeApi: PatientIntakeApi & {
    create: ReturnType<typeof vi.fn>;
    getCurrent: ReturnType<typeof vi.fn>;
    save: ReturnType<typeof vi.fn>;
    submit: ReturnType<typeof vi.fn>;
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
      create: vi.fn().mockReturnValue(of(draft())),
      getCurrent: vi.fn().mockReturnValue(of(draft())),
      save: vi.fn().mockReturnValue(of({ intake: savedDraft(), changed: true })),
      submit: vi.fn().mockReturnValue(of({ intake: submittedDraft(), changed: true }))
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

  it('loads the linked-patient draft without creating it', () => {
    boundary.setPatientSession(patientResponse());
    const facade = createFacade();

    facade.initialize('clinic-a');

    expect(facade.mode()).toBe('patient');
    expect(facade.status()).toBe('ready');
    expect(facade.intake()?.concurrencyToken).toBe('rv1.token');
    expect(patientAuthApi.getCurrent).toHaveBeenCalledTimes(1);
    expect(intakeApi.create).not.toHaveBeenCalled();
  });

  it('loads the exact intake-only draft through the same workspace', () => {
    boundary.setIntakeSession(intakeResponse());
    const facade = createFacade();

    facade.initialize('clinic-a');

    expect(facade.mode()).toBe('patient_intake');
    expect(facade.status()).toBe('ready');
    expect(intakeAuthApi.getCurrent).toHaveBeenCalledTimes(1);
    expect(patientAuthApi.getCurrent).not.toHaveBeenCalled();
  });

  it('offers explicit create only to a linked patient after side-effect-free 404', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.getCurrent.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    const facade = createFacade();

    facade.initialize('clinic-a');
    expect(facade.canCreate()).toBe(true);
    expect(intakeApi.create).not.toHaveBeenCalled();

    facade.createDraft();
    expect(intakeApi.create).toHaveBeenCalledTimes(1);
    expect(facade.status()).toBe('ready');
  });

  it('never allows intake-only scope to create an arbitrary draft', () => {
    boundary.setIntakeSession(intakeResponse());
    intakeApi.getCurrent.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    const facade = createFacade();

    facade.initialize('clinic-a');
    facade.createDraft();

    expect(facade.canCreate()).toBe(false);
    expect(intakeApi.create).not.toHaveBeenCalled();
  });

  it('fails closed on tenant realm mismatch without calling APIs', () => {
    boundary.setPatientSession(patientResponse());
    const facade = createFacade();

    facade.initialize('clinic-b');

    expect(facade.status()).toBe('unauthorized');
    expect(patientAuthApi.getCurrent).not.toHaveBeenCalled();
    expect(intakeApi.getCurrent).not.toHaveBeenCalled();
  });

  it('saves one complete snapshot with all 39 medical answers', () => {
    boundary.setPatientSession(patientResponse());
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveNonMedicalDraft(nonMedicalValue());

    const request = intakeApi.save.mock.calls[0][0] as SavePatientIntakeDraftRequest;
    expect(request).toMatchObject({
      firstName: 'María',
      lastName: 'García',
      occupation: null,
      mobilePhone: '+52 55 0000 0000',
      reasonForVisit: 'Dolor al masticar.',
      concurrencyToken: 'rv1.token'
    });
    expect(request.medicalAnswers).toHaveLength(39);
    expect(request.medicalAnswers).toEqual(draft().medicalAnswers);
    expect(request).not.toHaveProperty('tenantId');
    expect(request).not.toHaveProperty('patientId');
    expect(request).not.toHaveProperty('intakeId');
    expect(facade.saveOutcome()).toBe('saved');
    expect(facade.intake()?.concurrencyToken).toBe('rv1.next-token');
  });

  it('preserves current unsaved values from the sibling section', () => {
    boundary.setIntakeSession(intakeResponse());
    const facade = createFacade();
    facade.initialize('clinic-a');
    const answers = medicalValue();
    answers.find(answer => answer.questionKey === 'diabetes')!.answer = 'Yes';

    facade.saveMedicalDraft(answers, nonMedicalValue());

    const request = intakeApi.save.mock.calls[0][0] as SavePatientIntakeDraftRequest;
    expect(request.firstName).toBe('María');
    expect(request.medicalAnswers.map(answer => answer.questionKey)).toEqual(MEDICAL_QUESTIONNAIRE_KEYS);
    expect(request.medicalAnswers.find(answer => answer.questionKey === 'diabetes')?.answer).toBe('Yes');
  });

  it('represents no-op save without inventing a revision increment', () => {
    boundary.setIntakeSession(intakeResponse());
    intakeApi.save.mockReturnValue(of({ intake: draft(), changed: false }));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveMedicalDraft(medicalValue());

    expect(facade.saveOutcome()).toBe('unchanged');
    expect(facade.intake()?.currentRevisionNumber).toBe(0);
    expect(facade.intake()?.concurrencyToken).toBe('rv1.token');
  });

  it('retains the current snapshot and blocks repeated stale writes after conflict', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.save.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 409,
      error: { code: 'patient_intake.concurrency_conflict' }
    })));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveNonMedicalDraft(nonMedicalValue());
    facade.saveNonMedicalDraft(nonMedicalValue());

    expect(facade.intake()?.concurrencyToken).toBe('rv1.token');
    expect(facade.blockingState()).toBe('conflict');
    expect(facade.saveBlocked()).toBe(true);
    expect(facade.saveError()).toBe(
      'A newer version of this intake exists. Reload it before saving again.'
    );
    expect(intakeApi.save).toHaveBeenCalledTimes(1);
  });

  it('saves the complete snapshot and submits with the refreshed concurrency token', () => {
    boundary.setPatientSession(patientResponse());
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.submitIntake(nonMedicalValue(), completedMedicalValue());

    expect(intakeApi.save).toHaveBeenCalledTimes(1);
    expect(intakeApi.submit).toHaveBeenCalledWith({
      concurrencyToken: 'rv1.next-token'
    });
    expect(facade.submitted()).toBe(true);
    expect(facade.intake()?.status).toBe('Submitted');
    expect(facade.intake()?.submittedAtUtc).toBe('2026-08-06T13:30:00Z');
    expect(facade.submitError()).toBeNull();
  });

  it('does not call submit after a failed authoritative save', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.save.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 409,
      error: { code: 'patient_intake.concurrency_conflict' }
    })));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.submitIntake(nonMedicalValue(), completedMedicalValue());

    expect(intakeApi.submit).not.toHaveBeenCalled();
    expect(facade.blockingState()).toBe('conflict');
    expect(facade.submitted()).toBe(false);
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
    firstName: ' María ',
    lastName: ' García ',
    dateOfBirth: '1992-08-10',
    sex: 'Female',
    occupation: '   ',
    maritalStatus: 'Single',
    referredBy: '',
    preferredPhone: '',
    mobilePhone: '+52 55 0000 0000',
    homePhone: '',
    workPhone: '',
    email: 'maria@example.test',
    responsiblePartyName: '',
    responsiblePartyRelationship: '',
    responsiblePartyPhone: '',
    reasonForVisit: ' Dolor al masticar. '
  };
}

function medicalValue(): PatientIntakeMedicalAnswerFormValue[] {
  return MEDICAL_QUESTIONNAIRE_KEYS.map(questionKey => ({
    questionKey,
    answer: 'Unknown',
    details: ''
  }));
}

function completedMedicalValue(): PatientIntakeMedicalAnswerFormValue[] {
  return MEDICAL_QUESTIONNAIRE_KEYS.map(questionKey => ({
    questionKey,
    answer: 'No',
    details: ''
  }));
}

function draft(): PatientIntakeDraft {
  return {
    origin: 'ExistingPatientPortal',
    status: 'Draft',
    firstName: null,
    lastName: null,
    dateOfBirth: null,
    sex: 'Unspecified',
    occupation: null,
    maritalStatus: 'Unspecified',
    referredBy: null,
    preferredPhone: null,
    mobilePhone: null,
    homePhone: null,
    workPhone: null,
    email: null,
    responsiblePartyName: null,
    responsiblePartyRelationship: null,
    responsiblePartyPhone: null,
    reasonForVisit: null,
    medicalAnswers: MEDICAL_QUESTIONNAIRE_KEYS.map((questionKey, index) => ({
      questionKey,
      answer: index === 0 ? 'Yes' : 'Unknown',
      details: index === 0 ? 'Preserve this answer.' : null
    })),
    currentRevisionNumber: 0,
    concurrencyToken: 'rv1.token',
    createdAtUtc: '2026-07-27T10:00:00Z',
    lastUpdatedAtUtc: '2026-07-27T10:00:00Z',
    lastEffectiveSavedAtUtc: null,
    submittedAtUtc: null,
    expiresAtUtc: '2026-08-26T10:00:00Z'
  };
}

function savedDraft(): PatientIntakeDraft {
  return {
    ...draft(),
    firstName: 'María',
    lastName: 'García',
    currentRevisionNumber: 1,
    concurrencyToken: 'rv1.next-token',
    lastEffectiveSavedAtUtc: '2026-07-27T11:00:00Z'
  };
}

function submittedDraft(): PatientIntakeDraft {
  return {
    ...savedDraft(),
    status: 'Submitted',
    currentRevisionNumber: 2,
    concurrencyToken: 'rv1.submitted-token',
    submittedAtUtc: '2026-08-06T13:30:00Z'
  };
}
