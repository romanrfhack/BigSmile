import { PatientIntakeSessionStore } from './patient-intake-session.store';

describe('PatientIntakeSessionStore', () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('keeps the access token and current intake only in memory', () => {
    const store = new PatientIntakeSessionStore();
    store.setSession({
      accessToken: 'intake-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'account-id',
        intakeId: 'intake-id',
        tenantSubdomain: 'clinic-a',
        loginName: 'new.patient',
        sessionVersion: 1
      }
    });

    expect(store.getAccessToken()).toBe('intake-token');
    expect(store.current()?.intakeId).toBe('intake-id');
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });

  it('clears an expired session before returning a token', () => {
    const store = new PatientIntakeSessionStore();
    store.setSession({
      accessToken: 'expired-token',
      expiresAtUtc: new Date(Date.now() - 1_000).toISOString(),
      current: {
        accountId: 'account-id',
        intakeId: 'intake-id',
        tenantSubdomain: 'clinic-a',
        loginName: 'new.patient',
        sessionVersion: 1
      }
    });

    expect(store.getAccessToken()).toBeNull();
    expect(store.current()).toBeNull();
  });
});
