import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PatientIntakeAuthApi } from './patient-intake-auth.api';

describe('PatientIntakeAuthApi', () => {
  let api: PatientIntakeAuthApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    api = TestBed.inject(PatientIntakeAuthApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('sends the waiting-room token only in the activation body', () => {
    api.activate({
      accessToken: 'one-time-token',
      loginName: 'new.patient',
      password: 'twelve-character-password'
    }).subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/intake-auth/activate');
    expect(request.request.method).toBe('POST');
    expect(request.request.urlWithParams).not.toContain('one-time-token');
    expect(request.request.body).toEqual({
      accessToken: 'one-time-token',
      loginName: 'new.patient',
      password: 'twelve-character-password'
    });
    request.flush(buildResponse());
  });

  it('uses the tenant realm only for recurrent intake login', () => {
    api.login('clinic-a', {
      loginName: 'new.patient',
      password: 'twelve-character-password'
    }).subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/intake-auth/realms/clinic-a/login');
    expect(request.request.method).toBe('POST');
    request.flush(buildResponse());
  });

  it('uses the bounded current-session and logout endpoints', () => {
    api.getCurrent().subscribe();
    const currentRequest = httpTesting.expectOne('/api/patient-portal/intake-auth/me');
    expect(currentRequest.request.method).toBe('GET');
    currentRequest.flush(buildResponse().current);

    api.logout().subscribe();
    const logoutRequest = httpTesting.expectOne('/api/patient-portal/intake-auth/logout');
    expect(logoutRequest.request.method).toBe('POST');
    logoutRequest.flush(null);
  });

  function buildResponse() {
    return {
      accessToken: 'intake-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'account-id',
        intakeId: 'intake-id',
        tenantSubdomain: 'clinic-a',
        loginName: 'new.patient',
        sessionVersion: 1
      }
    };
  }
});
