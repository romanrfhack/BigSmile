import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { SchedulingApiService } from './scheduling-api.service';

describe('SchedulingApiService patient intake request', () => {
  let api: SchedulingApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    api = TestBed.inject(SchedulingApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads the portal and one-time medical-history status for an appointment', () => {
    api.getPatientIntakeRequestStatus('appointment-1').subscribe();

    const request = httpTesting.expectOne(
      '/api/appointments/appointment-1/patient-intake-request');
    expect(request.request.method).toBe('GET');
    request.flush(status());
  });

  it('prepares access without accepting patient or tenant ownership identifiers', () => {
    api.preparePatientIntakeRequest('appointment-1').subscribe();

    const request = httpTesting.expectOne(
      '/api/appointments/appointment-1/patient-intake-request');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({});
    expect(request.request.body).not.toHaveProperty('patientId');
    expect(request.request.body).not.toHaveProperty('tenantId');
    request.flush({
      status: status(),
      accessMode: 'Activation',
      activationToken: 'one-time-token'
    });
  });
});

function status() {
  return {
    appointmentId: 'appointment-1',
    patientId: 'patient-1',
    patientFullName: 'Ana López',
    patientPrimaryPhone: '55 1234 5678',
    patientPortalRealm: 'tenant-a',
    portalAccessStatus: 'NotActivated',
    intakeStatus: 'NotStarted',
    recommendedAccess: 'Activation',
    canRequest: true,
    submittedAtUtc: null
  };
}
