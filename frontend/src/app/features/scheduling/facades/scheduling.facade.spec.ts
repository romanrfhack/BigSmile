import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { SchedulingApiService } from '../data-access/scheduling-api.service';
import { SchedulingFacade } from './scheduling.facade';

describe('SchedulingFacade', () => {
  let facade: SchedulingFacade;
  let requestedCalendarArgs: [string, string, number] | null;
  let requestedPatientSearch: string | null;
  let requestedReminderLogAppointmentId: string | null;
  let calendarLoadCount: number;
  let api: SchedulingApiService;

  beforeEach(() => {
    requestedCalendarArgs = null;
    requestedPatientSearch = null;
    requestedReminderLogAppointmentId = null;
    calendarLoadCount = 0;

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
        calendarLoadCount += 1;
        requestedCalendarArgs = [branchId, startDate, days];
        return of({
          branchId,
          startDate,
          days,
          calendarDays: [
            {
              date: startDate,
              appointments: [],
              blockedSlots: []
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
      cancelAppointment: () => of(),
      markAppointmentAttended: () => of({
        id: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-16T09:00:00',
        endsAt: '2026-04-16T09:30:00',
        status: 'Attended',
        confirmationStatus: 'Pending',
        confirmedAtUtc: null,
        confirmedByUserId: null,
        notes: 'Check-up',
        cancellationReason: null
      }),
      markAppointmentNoShow: () => of({
        id: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-16T09:00:00',
        endsAt: '2026-04-16T09:30:00',
        status: 'NoShow',
        confirmationStatus: 'Pending',
        confirmedAtUtc: null,
        confirmedByUserId: null,
        notes: 'Check-up',
        cancellationReason: null
      }),
      confirmAppointment: () => of({
        id: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-16T09:00:00',
        endsAt: '2026-04-16T09:30:00',
        status: 'Scheduled',
        confirmationStatus: 'Confirmed',
        confirmedAtUtc: '2026-04-16T08:00:00Z',
        confirmedByUserId: 'user-1',
        notes: 'Check-up',
        cancellationReason: null
      }),
      markAppointmentConfirmationPending: () => of({
        id: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-16T09:00:00',
        endsAt: '2026-04-16T09:30:00',
        status: 'Scheduled',
        confirmationStatus: 'Pending',
        confirmedAtUtc: null,
        confirmedByUserId: null,
        notes: 'Check-up',
        cancellationReason: null
      }),
      getReminderLog: (appointmentId: string) => {
        requestedReminderLogAppointmentId = appointmentId;
        return of([
          {
            id: 'entry-1',
            appointmentId,
            channel: 'Email',
            outcome: 'LeftMessage',
            notes: null,
            createdAtUtc: '2026-04-24T08:00:00Z',
            createdByUserId: 'user-1'
          },
          {
            id: 'entry-2',
            appointmentId,
            channel: 'Phone',
            outcome: 'Reached',
            notes: 'Confirmed by phone.',
            createdAtUtc: '2026-04-25T08:00:00Z',
            createdByUserId: 'user-2'
          }
        ]);
      },
      addReminderLogEntry: (appointmentId: string) => of({
        id: 'entry-3',
        appointmentId,
        channel: 'WhatsApp',
        outcome: 'NoAnswer',
        notes: null,
        createdAtUtc: '2026-04-26T08:00:00Z',
        createdByUserId: 'user-3'
      }),
      createAppointmentBlock: () => of({
        id: 'block-1',
        branchId: 'branch-1',
        startsAt: '2026-04-16T13:00:00',
        endsAt: '2026-04-16T14:00:00',
        label: 'Lunch break'
      }),
      deleteAppointmentBlock: () => of(void 0)
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

  it('reloads the calendar after a blocked slot is created', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);

    facade.createAppointmentBlock({
      branchId: 'branch-1',
      startsAt: '2026-04-16T13:00',
      endsAt: '2026-04-16T14:00',
      label: 'Lunch break'
    }).subscribe();

    expect(calendarLoadCount).toBe(2);
  });

  it('reloads the calendar after an appointment is marked as attended', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);

    facade.markAppointmentAttended('appointment-1').subscribe();

    expect(calendarLoadCount).toBe(2);
  });

  it('reloads the calendar after an appointment is marked as no-show', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);

    facade.markAppointmentNoShow('appointment-1').subscribe();

    expect(calendarLoadCount).toBe(2);
  });

  it('reloads the calendar after an appointment is confirmed', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);

    facade.confirmAppointment('appointment-1').subscribe();

    expect(calendarLoadCount).toBe(2);
  });

  it('reloads the calendar after an appointment confirmation is marked pending', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);

    facade.markAppointmentConfirmationPending('appointment-1').subscribe();

    expect(calendarLoadCount).toBe(2);
  });

  it('loads reminder log entries for the selected appointment newest-first', () => {
    facade.loadReminderLog('appointment-1');

    expect(requestedReminderLogAppointmentId).toBe('appointment-1');
    expect(facade.reminderLog().map(entry => entry.id)).toEqual(['entry-2', 'entry-1']);
  });

  it('adds a reminder log entry without reloading the calendar', () => {
    facade.loadInitialContext('branch-1');
    facade.loadReminderLog('appointment-1');
    expect(calendarLoadCount).toBe(1);

    facade.addReminderLogEntry('appointment-1', {
      channel: 'WhatsApp',
      outcome: 'NoAnswer',
      notes: null
    }).subscribe();

    expect(facade.reminderLog().map(entry => entry.id)).toEqual(['entry-3', 'entry-2', 'entry-1']);
    expect(calendarLoadCount).toBe(1);
  });
});
