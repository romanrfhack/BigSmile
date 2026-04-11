import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { SchedulingApiService } from '../data-access/scheduling-api.service';
import { SchedulingFacade } from './scheduling.facade';

describe('SchedulingFacade', () => {
  let facade: SchedulingFacade;
  let requestedCalendarArgs: [string, string, number] | null;
  let requestedPatientSearch: string | null;
  let api: SchedulingApiService;

  beforeEach(() => {
    requestedCalendarArgs = null;
    requestedPatientSearch = null;

    api = {
      listAccessibleBranches: () => of([
        {
          id: 'branch-1',
          tenantId: 'tenant-1',
          name: 'Main branch',
          address: null,
          isActive: true,
          createdAt: '2026-04-11T12:00:00Z',
          updatedAt: null
        }
      ]),
      getCalendar: (branchId: string, startDate: string, days: number) => {
        requestedCalendarArgs = [branchId, startDate, days];
        return of({
          branchId,
          startDate,
          days,
          calendarDays: [
            {
              date: startDate,
              appointments: []
            }
          ]
        });
      },
      searchPatients: (search: string) => {
        requestedPatientSearch = search;
        return of([
          {
            id: 'patient-1',
            fullName: 'Ana Lopez',
            primaryPhone: '5551234567',
            email: 'ana@example.com',
            hasClinicalAlerts: true
          }
        ]);
      },
      createAppointment: () => of(),
      updateAppointment: () => of(),
      rescheduleAppointment: () => of(),
      cancelAppointment: () => of()
    } as unknown as SchedulingApiService;

    TestBed.configureTestingModule({
      providers: [
        SchedulingFacade,
        { provide: SchedulingApiService, useValue: api }
      ]
    });

    facade = TestBed.inject(SchedulingFacade);
  });

  it('loads the preferred branch and its initial calendar window', () => {
    facade.loadInitialContext('branch-1');

    expect(facade.selectedBranchId()).toBe('branch-1');
    expect(requestedCalendarArgs?.[0]).toBe('branch-1');
    expect(requestedCalendarArgs?.[2]).toBe(1);
    expect(facade.calendar()?.branchId).toBe('branch-1');
  });

  it('loads patient lookup results for appointment creation', () => {
    facade.searchPatients('Ana');

    expect(requestedPatientSearch).toBe('Ana');
    expect(facade.patientOptions()[0]?.fullName).toBe('Ana Lopez');
    expect(facade.patientOptions()[0]?.hasClinicalAlerts).toBe(true);
  });
});
