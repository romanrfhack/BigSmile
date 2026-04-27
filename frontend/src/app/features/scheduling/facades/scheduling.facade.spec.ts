import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { SchedulingApiService } from '../data-access/scheduling-api.service';
import { SchedulingFacade } from './scheduling.facade';

describe('SchedulingFacade', () => {
  let facade: SchedulingFacade;
  let requestedCalendarArgs: [string, string, number] | null;
  let requestedPatientSearch: string | null;
  let requestedReminderLogAppointmentId: string | null;
  let requestedManualRemindersBranchId: string | null;
  let calendarLoadCount: number;
  let manualReminderLoadCount: number;
  let followUpRequests: unknown[];
  let api: SchedulingApiService;

  beforeEach(() => {
    requestedCalendarArgs = null;
    requestedPatientSearch = null;
    requestedReminderLogAppointmentId = null;
    requestedManualRemindersBranchId = null;
    calendarLoadCount = 0;
    manualReminderLoadCount = 0;
    followUpRequests = [];

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
      listManualReminders: (branchId: string) => {
        requestedManualRemindersBranchId = branchId;
        manualReminderLoadCount += 1;
        return of([
          {
            appointmentId: 'appointment-2',
            branchId,
            patientId: 'patient-2',
            patientFullName: 'Bruno Garcia',
            startsAt: '2026-04-16T11:00:00Z',
            appointmentStatus: 'Scheduled',
            confirmationStatus: 'Pending',
            reminderChannel: 'Email',
            reminderDueAtUtc: '2026-04-16T10:00:00Z',
            reminderState: 'Pending',
            reminderCompletedAtUtc: null,
            reminderCompletedByUserId: null
          },
          {
            appointmentId: 'appointment-1',
            branchId,
            patientId: 'patient-1',
            patientFullName: 'Ana Lopez',
            startsAt: '2026-04-16T09:00:00Z',
            appointmentStatus: 'Scheduled',
            confirmationStatus: 'Pending',
            reminderChannel: 'Phone',
            reminderDueAtUtc: '2026-04-16T08:00:00Z',
            reminderState: 'Due',
            reminderCompletedAtUtc: null,
            reminderCompletedByUserId: null
          }
        ]);
      },
      configureManualReminder: () => of({
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
        reminderRequired: true,
        reminderChannel: 'Phone',
        reminderDueAtUtc: '2026-04-16T08:00:00Z',
        reminderCompletedAtUtc: null,
        reminderCompletedByUserId: null,
        reminderUpdatedAtUtc: '2026-04-16T07:00:00Z',
        reminderUpdatedByUserId: 'user-1',
        notes: 'Check-up',
        cancellationReason: null
      }),
      completeManualReminder: () => of({
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
        reminderRequired: true,
        reminderChannel: 'Phone',
        reminderDueAtUtc: '2026-04-16T08:00:00Z',
        reminderCompletedAtUtc: '2026-04-16T08:30:00Z',
        reminderCompletedByUserId: 'user-1',
        reminderUpdatedAtUtc: '2026-04-16T08:30:00Z',
        reminderUpdatedByUserId: 'user-1',
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
      recordManualReminderFollowUp: (appointmentId: string, payload: unknown) => {
        followUpRequests.push({ appointmentId, payload });
        return of({
          appointment: {
            id: appointmentId,
            branchId: 'branch-1',
            patientId: 'patient-1',
            patientFullName: 'Ana Lopez',
            startsAt: '2026-04-16T09:00:00',
            endsAt: '2026-04-16T09:30:00',
            status: 'Scheduled',
            confirmationStatus: 'Confirmed',
            confirmedAtUtc: '2026-04-16T08:30:00Z',
            confirmedByUserId: 'user-1',
            reminderRequired: true,
            reminderChannel: 'Phone',
            reminderDueAtUtc: '2026-04-16T08:00:00Z',
            reminderCompletedAtUtc: '2026-04-16T08:30:00Z',
            reminderCompletedByUserId: 'user-1',
            reminderUpdatedAtUtc: '2026-04-16T08:30:00Z',
            reminderUpdatedByUserId: 'user-1',
            notes: 'Check-up',
            cancellationReason: null
          },
          reminderLogEntry: {
            id: 'entry-4',
            appointmentId,
            channel: 'Phone',
            outcome: 'Reached',
            notes: 'Confirmed by phone.',
            createdAtUtc: '2026-04-26T09:00:00Z',
            createdByUserId: 'user-1'
          }
        });
      },
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
    expect(requestedManualRemindersBranchId).toBe('branch-1');
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

  it('loads manual reminder work items ordered by due date', () => {
    facade.loadInitialContext('branch-1');

    expect(facade.manualReminders().map(item => item.appointmentId)).toEqual(['appointment-1', 'appointment-2']);
    expect(facade.manualReminders()[0]?.reminderState).toBe('Due');
  });

  it('reloads calendar and manual reminder list after configuring a manual reminder', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);
    expect(manualReminderLoadCount).toBe(1);

    facade.configureManualReminder('appointment-1', {
      required: true,
      channel: 'Phone',
      dueAtUtc: '2026-04-16T08:00:00Z'
    }).subscribe();

    expect(calendarLoadCount).toBe(2);
    expect(manualReminderLoadCount).toBe(2);
  });

  it('reloads calendar and manual reminder list after completing a manual reminder', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);
    expect(manualReminderLoadCount).toBe(1);

    facade.completeManualReminder('appointment-1').subscribe();

    expect(calendarLoadCount).toBe(2);
    expect(manualReminderLoadCount).toBe(2);
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

  it('reloads calendar and manual reminder list after recording a manual reminder follow-up', () => {
    facade.loadInitialContext('branch-1');
    expect(calendarLoadCount).toBe(1);
    expect(manualReminderLoadCount).toBe(1);

    facade.recordManualReminderFollowUp('appointment-1', {
      channel: 'Phone',
      outcome: 'Reached',
      notes: 'Confirmed by phone.',
      completeReminder: true,
      confirmAppointment: true
    }).subscribe();

    expect(followUpRequests).toEqual([
      {
        appointmentId: 'appointment-1',
        payload: {
          channel: 'Phone',
          outcome: 'Reached',
          notes: 'Confirmed by phone.',
          completeReminder: true,
          confirmAppointment: true
        }
      }
    ]);
    expect(calendarLoadCount).toBe(2);
    expect(manualReminderLoadCount).toBe(2);
  });
});
