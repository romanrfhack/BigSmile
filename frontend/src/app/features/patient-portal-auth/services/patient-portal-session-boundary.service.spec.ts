import { PatientIntakeSessionStore } from './patient-intake-session.store';
import { PatientPortalSessionBoundary } from './patient-portal-session-boundary.service';
import { PatientPortalSessionStore } from './patient-portal-session.store';

describe('PatientPortalSessionBoundary', () => {
  let patientStore: PatientPortalSessionStore;
  let intakeStore: PatientIntakeSessionStore;
  let boundary: PatientPortalSessionBoundary;

  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
    patientStore = new PatientPortalSessionStore();
    intakeStore = new PatientIntakeSessionStore();
    boundary = new PatientPortalSessionBoundary(patientStore, intakeStore);
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('sets a linked-patient session and clears any intake-only session', () => {
    boundary.setIntakeSession(intakeResponse());
    boundary.setPatientSession(patientResponse());

    expect(boundary.resolve()).toEqual({
      state: 'active',
      session: {
        mode: 'patient',
        accessToken: 'patient-token',
        tenantSubdomain: 'clinic-a'
      }
    });
    expect(intakeStore.current()).toBeNull();
  });

  it('sets an intake-only session and clears any linked-patient session', () => {
    boundary.setPatientSession(patientResponse());
    boundary.setIntakeSession(intakeResponse());

    expect(boundary.resolve()).toEqual({
      state: 'active',
      session: {
        mode: 'patient_intake',
        accessToken: 'intake-token',
        tenantSubdomain: 'clinic-a'
      }
    });
    expect(patientStore.current()).toBeNull();
  });

  it('fails closed and clears both stores when direct writes create ambiguity', () => {
    patientStore.setSession(patientResponse());
    intakeStore.setSession(intakeResponse());

    expect(boundary.resolve()).toEqual({ state: 'ambiguous' });
    expect(patientStore.current()).toBeNull();
    expect(intakeStore.current()).toBeNull();
  });

  it('keeps all tokens in memory only', () => {
    boundary.setPatientSession(patientResponse());

    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });

  function patientResponse() {
    return {
      accessToken: 'patient-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'patient-account-id',
        patientId: 'patient-id',
        tenantSubdomain: 'Clinic-A',
        loginName: 'patient.login',
        sessionVersion: 1
      }
    };
  }

  function intakeResponse() {
    return {
      accessToken: 'intake-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'intake-account-id',
        intakeId: 'intake-id',
        tenantSubdomain: 'Clinic-A',
        loginName: 'new.patient',
        sessionVersion: 1
      }
    };
  }
});
