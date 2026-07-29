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
  PatientIntakeNonMedicalFormValue
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

  it('refreshes and loads the linked patient draft without creating it', () => {
    boundary.setPatientSession(patientResponse());
    const facade = createFacade();

    facade.initialize('clinic-a');

    expect(facade.mode()).toBe('patient');
    expect(facade.status()).toBe('ready');
    expect(facade.intake()?.concurrencyToken).toBe('rv1.token');
    expect(patientAuthApi.getCurrent).toHaveBeenCalledTimes(1);
    expect(intakeAuthApi.getCurrent).not.toHaveBeenCalled();
    expect(intakeApi.getCurrent).toHaveBeenCalledTimes(1);
    expect(intakeApi.create).not.toHaveBeenCalled();
  });

  it('refreshes and loads the exact intake-only draft', () => {
    boundary.setIntakeSession(intakeResponse());
    const facade = createFacade();

    facade.initialize('clinic-a');

    expect(facade.mode()).toBe('patient_intake');
    expect(facade.status()).toBe('ready');
    expect(intakeAuthApi.getCurrent).toHaveBeenCalledTimes(1);
    expect(patientAuthApi.getCurrent).not.toHaveBeenCalled();
  });

  it('shows explicit create only for a linked patient after a side-effect-free 404', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.getCurrent.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    const facade = createFacade();

    facade.initialize('clinic-a');

    expect(facade.status()).toBe('missing');
    expect(facade.canCreate()).toBe(true);
    expect(intakeApi.create).not.toHaveBeenCalled();

    facade.createDraft();

    expect(intakeApi.create).toHaveBeenCalledTimes(1);
    expect(facade.status()).toBe('ready');
  });

  it('never allows an intake-only session to create an arbitrary draft', () => {
    boundary.setIntakeSession(intakeResponse());
    intakeApi.getCurrent.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    const facade = createFacade();

    facade.initialize('clinic-a');
    facade.createDraft();

    expect(facade.status()).toBe('missing');
    expect(facade.canCreate()).toBe(false);
    expect(intakeApi.create).not.toHaveBeenCalled();
  });

  it('fails closed on tenant realm mismatch without calling the API', () => {
    boundary.setPatientSession(patientResponse());
    const facade = createFacade();

    facade.initialize('clinic-b');

    expect(facade.status()).toBe('unauthorized');
    expect(patientAuthApi.getCurrent).not.toHaveBeenCalled();
    expect(intakeApi.getCurrent).not.toHaveBeenCalled();
  });

  it('saves the complete non-medical snapshot while preserving all 39 medical answers', () => {
    boundary.setPatientSession(patientResponse());
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveNonMedicalDraft(nonMedicalValue());

    expect(intakeApi.save).toHaveBeenCalledTimes(1);
    const request = intakeApi.save.mock.calls[0][0];
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
    expect(request).not.toHaveProperty('age');
    expect(request).not.toHaveProperty('tenantId');
    expect(request).not.toHaveProperty('patientId');
    expect(request).not.toHaveProperty('intakeId');
    expect(facade.saveTarget()).toBe('demographics');
    expect(facade.saveOutcome()).toBe('saved');
    expect(facade.intake()?.currentRevisionNumber).toBe(1);
    expect(facade.intake()?.concurrencyToken).toBe('rv1.next-token');
  });

  it('saves all 39 medical answers while preserving the authoritative non-medical snapshot', () => {
    boundary.setIntakeSession(intakeResponse());
    const loaded = draft();
    loaded.firstName = 'Ana';
    loaded.lastName = 'López';
    loaded.reasonForVisit = 'Revisión general.';
    intakeApi.getCurrent.mockReturnValue(of(loaded));
    const facade = createFacade();
    facade.initialize('clinic-a');

    const answers = medicalValue();
    const diabetes = answers.find(answer => answer.questionKey === 'diabetes')!;
    diabetes.answer = 'Yes';
    diabetes.details = '  Diet controlled.  ';
    facade.saveMedicalDraft(answers);

    const request = intakeApi.save.mock.calls[0][0];
    expect(request.firstName).toBe('Ana');
    expect(request.lastName).toBe('López');
    expect(request.reasonForVisit).toBe('Revisión general.');
    expect(request.medicalAnswers.map(answer => answer.questionKey)).toEqual(MEDICAL_QUESTIONNAIRE_KEYS);
    expect(request.medicalAnswers.find(answer => answer.questionKey === 'diabetes')).toEqual({
      questionKey: 'diabetes',
      answer: 'Yes',
      details: 'Diet controlled.'
    });
    expect(facade.saveTarget()).toBe('medical');
    expect(facade.saveOutcome()).toBe('saved');
  });

  it('represents an unchanged save without inventing a revision increment', () => {
    boundary.setIntakeSession(intakeResponse());
    intakeApi.save.mockReturnValue(of({ intake: draft(), changed: false }));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveMedicalDraft(medicalValue());

    expect(facade.saveTarget()).toBe('medical');
    expect(facade.saveOutcome()).toBe('unchanged');
    expect(facade.intake()?.currentRevisionNumber).toBe(0);
    expect(facade.intake()?.concurrencyToken).toBe('rv1.token');
  });

  it('keeps the current local snapshot and exposes a bounded message on conflict', () => {
    boundary.setPatientSession(patientResponse());
    intakeApi.save.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));
    const facade = createFacade();
    facade.initialize('clinic-a');

    facade.saveNonMedicalDraft(nonMedicalValue());

    expect(facade.intake()?.concurrencyToken).toBe('rv1.token');
    expect(facade.saveTarget()).toBe('demographics');
    expect(facade.saveOutcome()).toBeNull();
    expect(facade.saveError()).toBe(
      'The intake draft changed or expired. Reload it before saving again.'
    );
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
});

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
    expiresAtUtc: '2026-08-26T10:00:00Z'
  };
}

function savedDraft(): PatientIntakeDraft {
  return {
    ...draft(),
    firstName: 'María',
    lastName: 'García',
    dateOfBirth: '1992-08-10',
    sex: 'Female',
    maritalStatus: 'Single',
    mobilePhone: '+52 55 0000 0000',
    email: 'maria@example.test',
    reasonForVisit: 'Dolor al masticar.',
    currentRevisionNumber: 1,
    concurrencyToken: 'rv1.next-token',
    lastUpdatedAtUtc: '2026-07-27T11:00:00Z',
    lastEffectiveSavedAtUtc: '2026-07-27T11:00:00Z',
    expiresAtUtc: '2026-08-26T11:00:00Z'
  };
}
