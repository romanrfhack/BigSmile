import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { PatientIntakeAuthApi } from '../../patient-portal-auth/data-access/patient-intake-auth.api';
import { PatientPortalAuthApi } from '../../patient-portal-auth/data-access/patient-portal-auth.api';
import { PatientIntakeSessionStore } from '../../patient-portal-auth/services/patient-intake-session.store';
import { PatientPortalSessionBoundary } from '../../patient-portal-auth/services/patient-portal-session-boundary.service';
import { PatientPortalSessionStore } from '../../patient-portal-auth/services/patient-portal-session.store';
import { PatientIntakeApi } from '../data-access/patient-intake.api';
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
      save: vi.fn()
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

  function draft() {
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
      medicalAnswers: [],
      currentRevisionNumber: 0,
      concurrencyToken: 'rv1.token',
      createdAtUtc: '2026-07-27T10:00:00Z',
      lastUpdatedAtUtc: '2026-07-27T10:00:00Z',
      lastEffectiveSavedAtUtc: null,
      expiresAtUtc: '2026-08-26T10:00:00Z'
    };
  }
});
