import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { PatientIntakeLinksApi } from './patient-intake-links.api';

describe('PatientIntakeLinksApi', () => {
  let api: PatientIntakeLinksApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    api = TestBed.inject(PatientIntakeLinksApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('lists bounded metadata without reconstructing a token', () => {
    api.list(true, 25).subscribe(links => {
      expect(links[0]).not.toHaveProperty('accessToken');
      expect(links[0]).not.toHaveProperty('tokenHash');
    });

    const request = httpTesting.expectOne(
      candidate => candidate.url === '/api/patient-intake-links' &&
        candidate.params.get('includeResolved') === 'true' &&
        candidate.params.get('take') === '25'
    );
    expect(request.request.method).toBe('GET');
    request.flush([{
      id: 'link-id',
      branchId: null,
      purpose: 'NewPatientWaitingRoomRegistration',
      status: 'Active',
      createdAtUtc: '2026-07-26T10:00:00Z',
      expiresAtUtc: '2026-07-26T10:30:00Z',
      revokedAtUtc: null,
      consumedAtUtc: null
    }]);
  });

  it('issues a credential with only optional BranchId in the request body', () => {
    api.issue('branch-id').subscribe();

    const request = httpTesting.expectOne('/api/patient-intake-links');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ branchId: 'branch-id' });
    expect(request.request.body).not.toHaveProperty('tenantId');
    request.flush({
      id: 'link-id',
      branchId: 'branch-id',
      purpose: 'NewPatientWaitingRoomRegistration',
      accessToken: 'one-time-token',
      createdAtUtc: '2026-07-26T10:00:00Z',
      expiresAtUtc: '2026-07-26T10:30:00Z'
    });
  });

  it('revokes by server-issued link id', () => {
    api.revoke('link-id').subscribe();

    const request = httpTesting.expectOne('/api/patient-intake-links/link-id');
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
