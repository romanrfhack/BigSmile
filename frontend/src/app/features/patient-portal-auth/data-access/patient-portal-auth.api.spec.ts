import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PatientPortalAuthApi } from './patient-portal-auth.api';

describe('PatientPortalAuthApi', () => {
  let api: PatientPortalAuthApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    api = TestBed.inject(PatientPortalAuthApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('sends the activation token only in the request body', () => {
    api.activate({
      activationToken: 'one-time-token',
      loginName: 'patient.login',
      password: 'twelve-character-password'
    }).subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/auth/activate');
    expect(request.request.method).toBe('POST');
    expect(request.request.urlWithParams).not.toContain('one-time-token');
    expect(request.request.body).toEqual({
      activationToken: 'one-time-token',
      loginName: 'patient.login',
      password: 'twelve-character-password'
    });
    request.flush(buildResponse());
  });

  it('uses an encoded tenant realm for recurrent login', () => {
    api.login('clinic-a', {
      loginName: 'patient.login',
      password: 'patient-password'
    }).subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/auth/realms/clinic-a/login');
    expect(request.request.method).toBe('POST');
    request.flush(buildResponse());
  });

  it('uses the bounded current-session and logout endpoints', () => {
    api.getCurrent().subscribe();
    const currentRequest = httpTesting.expectOne('/api/patient-portal/auth/me');
    expect(currentRequest.request.method).toBe('GET');
    currentRequest.flush(buildResponse().current);

    api.logout().subscribe();
    const logoutRequest = httpTesting.expectOne('/api/patient-portal/auth/logout');
    expect(logoutRequest.request.method).toBe('POST');
    logoutRequest.flush(null);
  });

  function buildResponse() {
    return {
      accessToken: 'patient-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'account-id',
        patientId: 'patient-id',
        tenantSubdomain: 'clinic-a',
        loginName: 'patient.login',
        sessionVersion: 1
      }
    };
  }
});
