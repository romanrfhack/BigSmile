import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { PatientsApiService } from '../data-access/patients-api.service';
import { PatientsFacade } from './patients.facade';

describe('PatientsFacade', () => {
  let facade: PatientsFacade;
  let searchArgs: [string, boolean, number] | null;
  let requestedPatientId: string | null;
  let api: PatientsApiService;

  beforeEach(() => {
    searchArgs = null;
    requestedPatientId = null;
    api = {
      searchPatients: (search: string, includeInactive: boolean, take: number) => {
        searchArgs = [search, includeInactive, take];
        return of([
          {
            id: 'patient-1',
            firstName: 'Ana',
            lastName: 'Lopez',
            fullName: 'Ana Lopez',
            dateOfBirth: '1991-02-14',
            primaryPhone: '5551234567',
            email: 'ana@example.com',
            isActive: true,
            hasClinicalAlerts: true
          }
        ]);
      },
      getPatient: (id: string) => {
        requestedPatientId = id;
        return of({
          id: 'patient-1',
          firstName: 'Ana',
          lastName: 'Lopez',
          fullName: 'Ana Lopez',
          dateOfBirth: '1991-02-14',
          primaryPhone: '5551234567',
          email: 'ana@example.com',
          isActive: true,
          hasClinicalAlerts: true,
          clinicalAlertsSummary: 'Allergy to latex.',
          responsibleParty: {
            name: 'Maria Lopez',
            relationship: 'Mother',
            phone: '5559990000'
          },
          createdAt: '2026-04-08T12:00:00Z',
          updatedAt: null
        });
      },
      createPatient: () => of(),
      updatePatient: () => of()
    } as unknown as PatientsApiService;

    TestBed.configureTestingModule({
      providers: [
        PatientsFacade,
        { provide: PatientsApiService, useValue: api }
      ]
    });

    facade = TestBed.inject(PatientsFacade);
  });

  it('loads search results into facade state', () => {
    facade.search('Ana');

    expect(searchArgs).toEqual(['Ana', false, 25]);
    expect(facade.patients()[0]?.id).toBe('patient-1');
    expect(facade.patients()[0]?.fullName).toBe('Ana Lopez');
    expect(facade.patients()[0]?.hasClinicalAlerts).toBe(true);
    expect(facade.loadingPatients()).toBe(false);
  });

  it('loads a patient profile into facade state', () => {
    facade.loadPatient('patient-1');

    expect(requestedPatientId).toBe('patient-1');
    expect(facade.currentPatient()?.id).toBe('patient-1');
    expect(facade.currentPatient()?.clinicalAlertsSummary).toBe('Allergy to latex.');
    expect(facade.currentPatient()?.responsibleParty?.name).toBe('Maria Lopez');
    expect(facade.loadingPatient()).toBe(false);
  });
});
