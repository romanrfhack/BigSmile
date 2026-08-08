import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { SavePatientIntakeDraftRequest } from '../models/patient-intake.models';
import { PatientIntakeApi } from './patient-intake.api';

describe('PatientIntakeApi', () => {
  let api: PatientIntakeApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    api = TestBed.inject(PatientIntakeApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('creates a linked-patient draft without ownership identifiers', () => {
    api.create().subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/intake');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({});
    expect(request.request.body).not.toHaveProperty('tenantId');
    expect(request.request.body).not.toHaveProperty('patientId');
    expect(request.request.body).not.toHaveProperty('intakeId');
    request.flush(createDraft());
  });

  it('loads the current self-owned draft without route identifiers', () => {
    api.getCurrent().subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/intake');
    expect(request.request.method).toBe('GET');
    request.flush(createDraft());
  });

  it('saves the exact full snapshot contract with its concurrency token', () => {
    const payload: SavePatientIntakeDraftRequest = {
      firstName: 'Ana',
      lastName: 'López',
      dateOfBirth: '1990-05-10',
      sex: 'Female',
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
      medicalAnswers: [{ questionKey: 'diabetes', answer: 'Unknown', details: null }],
      concurrencyToken: 'rv1.token'
    };

    api.save(payload).subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/intake');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(payload);
    expect(request.request.body).not.toHaveProperty('tenantId');
    expect(request.request.body).not.toHaveProperty('patientId');
    expect(request.request.body).not.toHaveProperty('accountId');
    expect(request.request.body).not.toHaveProperty('intakeId');
    request.flush({ intake: createDraft(), changed: true });
  });

  it('submits only the current self-owned intake concurrency token', () => {
    api.submit({ concurrencyToken: 'rv1.token' }).subscribe();

    const request = httpTesting.expectOne('/api/patient-portal/intake/submit');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ concurrencyToken: 'rv1.token' });
    expect(request.request.body).not.toHaveProperty('tenantId');
    expect(request.request.body).not.toHaveProperty('patientId');
    expect(request.request.body).not.toHaveProperty('intakeId');
    request.flush({
      intake: {
        ...createDraft(),
        status: 'Submitted',
        submittedAtUtc: '2026-08-06T13:30:00Z'
      },
      changed: true
    });
  });

  function createDraft() {
    return {
      origin: 'ExistingPatientPortal',
      status: 'Draft',
      firstName: 'Ana',
      lastName: 'López',
      dateOfBirth: '1990-05-10',
      sex: 'Female',
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
      submittedAtUtc: null,
      expiresAtUtc: '2026-08-26T10:00:00Z'
    };
  }
});
