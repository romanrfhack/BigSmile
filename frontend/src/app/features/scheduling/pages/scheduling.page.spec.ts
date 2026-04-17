import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { SchedulingFacade } from '../facades/scheduling.facade';
import { SchedulingPageComponent } from './scheduling.page';

describe('SchedulingPageComponent', () => {
  let facade: any;
  let authService: any;
  let attendedCalls: string[];

  beforeEach(async () => {
    attendedCalls = [];

    facade = {
      branches: signal([
        {
          id: 'branch-1',
          tenantId: 'tenant-1',
          name: 'Main branch',
          address: null,
          isActive: true,
          createdAt: '2026-04-14T00:00:00Z',
          updatedAt: null
        }
      ]),
      selectedBranchId: signal<string | null>('branch-1'),
      selectedDate: signal('2026-04-14'),
      viewMode: signal<'day' | 'week'>('day'),
      calendar: signal(null),
      patientOptions: signal([]),
      loadingBranches: signal(false),
      loadingCalendar: signal(false),
      loadingPatients: signal(false),
      branchesError: signal<string | null>(null),
      calendarError: signal<string | null>(null),
      loadInitialContext: () => undefined,
      clearPatientOptions: () => undefined,
      searchPatients: () => undefined,
      selectBranch: () => undefined,
      setDate: () => undefined,
      setViewMode: () => undefined,
      createAppointment: () => of(null),
      updateAppointment: () => of(null),
      rescheduleAppointment: () => of(null),
      cancelAppointment: () => of(null),
      markAppointmentAttended: (id: string) => {
        attendedCalls.push(id);
        return of({
          id: 'appointment-1',
          branchId: 'branch-1',
          patientId: 'patient-1',
          patientFullName: 'Ana Lopez',
          startsAt: '2026-04-14T09:00:00',
          endsAt: '2026-04-14T09:30:00',
          status: 'Attended',
          notes: 'Follow-up',
          cancellationReason: null
        });
      },
      markAppointmentNoShow: () => of({
        id: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-14T09:00:00',
        endsAt: '2026-04-14T09:30:00',
        status: 'NoShow',
        notes: 'Follow-up',
        cancellationReason: null
      }),
      createAppointmentBlock: () => of(null),
      deleteAppointmentBlock: () => of(null)
    };

    authService = {
      hasPermissions: () => true,
      getCurrentTenant: () => ({ id: 'tenant-1', name: 'Tenant A' }),
      getCurrent: () => ({
        currentBranch: { id: 'branch-1', name: 'Main branch' }
      })
    };

    await TestBed.configureTestingModule({
      imports: [SchedulingPageComponent],
      providers: [
        { provide: SchedulingFacade, useValue: facade },
        { provide: AuthService, useValue: authService }
      ]
    }).compileComponents();
  });

  it('keeps completed appointments out of edit mode when selected', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;

    component.selectAppointment({
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'Scheduled',
      notes: 'Follow-up',
      cancellationReason: null
    });

    expect(component.editorMode).toBe('edit');

    component.selectAppointment({
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'Attended',
      notes: 'Follow-up',
      cancellationReason: null
    });

    expect(component.editorMode).toBe('create');
  });

  it('marks the selected appointment as attended through the scheduling facade', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;
    const originalConfirm = window.confirm;
    window.confirm = () => true;

    component.selectedAppointment = {
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'Scheduled',
      notes: 'Follow-up',
      cancellationReason: null
    };

    try {
      component.markSelectedAttended();

      expect(attendedCalls).toEqual(['appointment-1']);
      expect(component.selectedAppointment?.status).toBe('Attended');
      expect(component.editorMode).toBe('create');
    } finally {
      window.confirm = originalConfirm;
    }
  });
});
