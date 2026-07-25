import { PatientPortalAuthenticationResponse } from '../models/patient-portal-auth.models';
import { PatientPortalSessionStore } from './patient-portal-session.store';

describe('PatientPortalSessionStore', () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('keeps the patient token and session in memory only', () => {
    const store = new PatientPortalSessionStore();
    const response = buildResponse();

    store.setSession(response);

    expect(store.isAuthenticated()).toBe(true);
    expect(store.getAccessToken()).toBe('patient-token');
    expect(store.current()).toEqual(response.current);
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });

  it('clears all patient session state explicitly', () => {
    const store = new PatientPortalSessionStore();
    store.setSession(buildResponse());

    store.clear();

    expect(store.isAuthenticated()).toBe(false);
    expect(store.getAccessToken()).toBeNull();
    expect(store.current()).toBeNull();
    expect(store.expiresAtUtc()).toBeNull();
  });

  it('rejects an expired in-memory token', () => {
    const store = new PatientPortalSessionStore();
    store.setSession(buildResponse(new Date(Date.now() - 1_000).toISOString()));

    expect(store.isAuthenticated()).toBe(false);
    expect(store.getAccessToken()).toBeNull();
    expect(store.current()).toBeNull();
  });

  function buildResponse(expiresAtUtc = new Date(Date.now() + 60_000).toISOString()): PatientPortalAuthenticationResponse {
    return {
      accessToken: 'patient-token',
      expiresAtUtc,
      current: {
        accountId: 'account-id',
        patientId: 'patient-id',
        tenantSubdomain: 'tenant-a',
        loginName: 'patient.login',
        sessionVersion: 1
      }
    };
  }
});
