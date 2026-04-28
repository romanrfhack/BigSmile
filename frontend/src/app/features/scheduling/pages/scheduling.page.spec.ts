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
  let noShowCalls: string[];
  let confirmationCalls: string[];
  let pendingCalls: string[];
  let reminderLogLoads: (string | null)[];
  let reminderLogAdds: unknown[];
  let manualReminderConfigs: unknown[];
  let manualReminderCompletions: string[];
  let reminderFollowUps: unknown[];
  let reminderTemplateCreates: unknown[];
  let reminderTemplateUpdates: unknown[];
  let reminderTemplateDeactivations: string[];
  let reminderTemplatePreviews: unknown[];
  let reminderTemplateLoads: number;

  beforeEach(async () => {
    attendedCalls = [];
    noShowCalls = [];
    confirmationCalls = [];
    pendingCalls = [];
    reminderLogLoads = [];
    reminderLogAdds = [];
    manualReminderConfigs = [];
    manualReminderCompletions = [];
    reminderFollowUps = [];
    reminderTemplateCreates = [];
    reminderTemplateUpdates = [];
    reminderTemplateDeactivations = [];
    reminderTemplatePreviews = [];
    reminderTemplateLoads = 0;

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
      reminderLog: signal([]),
      manualReminders: signal([]),
      reminderTemplates: signal([
        {
          id: 'template-1',
          name: 'Confirmacion',
          body: 'Hola {{patientName}}.',
          isActive: true,
          createdAtUtc: '2026-04-14T08:00:00Z',
          createdByUserId: 'user-1',
          updatedAtUtc: null,
          updatedByUserId: null,
          deactivatedAtUtc: null,
          deactivatedByUserId: null
        }
      ]),
      reminderTemplatePreview: signal(null),
      patientOptions: signal([]),
      loadingBranches: signal(false),
      loadingCalendar: signal(false),
      loadingReminderLog: signal(false),
      loadingManualReminders: signal(false),
      loadingReminderTemplates: signal(false),
      loadingReminderTemplatePreview: signal(false),
      loadingPatients: signal(false),
      branchesError: signal<string | null>(null),
      calendarError: signal<string | null>(null),
      reminderLogError: signal<string | null>(null),
      manualRemindersError: signal<string | null>(null),
      reminderTemplatesError: signal<string | null>(null),
      reminderTemplatePreviewError: signal<string | null>(null),
      loadInitialContext: () => undefined,
      loadManualReminders: () => undefined,
      loadReminderTemplates: () => {
        reminderTemplateLoads += 1;
      },
      clearPatientOptions: () => undefined,
      loadReminderLog: (appointmentId: string | null) => {
        reminderLogLoads.push(appointmentId);
      },
      clearReminderLog: () => {
        reminderLogLoads.push(null);
      },
      clearReminderTemplatePreview: () => {
        facade.reminderTemplatePreview.set(null);
      },
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
          confirmationStatus: 'Pending',
          confirmedAtUtc: null,
          confirmedByUserId: null,
          notes: 'Follow-up',
          cancellationReason: null
        });
      },
      markAppointmentNoShow: (id: string) => {
        noShowCalls.push(id);
        return of({
          id: 'appointment-1',
          branchId: 'branch-1',
          patientId: 'patient-1',
          patientFullName: 'Ana Lopez',
          startsAt: '2026-04-14T09:00:00',
          endsAt: '2026-04-14T09:30:00',
          status: 'NoShow',
          confirmationStatus: 'Pending',
          confirmedAtUtc: null,
          confirmedByUserId: null,
          notes: 'Follow-up',
          cancellationReason: null
        });
      },
      confirmAppointment: (id: string) => {
        confirmationCalls.push(id);
        return of({
          id: 'appointment-1',
          branchId: 'branch-1',
          patientId: 'patient-1',
          patientFullName: 'Ana Lopez',
          startsAt: '2026-04-14T09:00:00',
          endsAt: '2026-04-14T09:30:00',
          status: 'Scheduled',
          confirmationStatus: 'Confirmed',
          confirmedAtUtc: '2026-04-14T08:00:00Z',
          confirmedByUserId: 'user-1',
          notes: 'Follow-up',
          cancellationReason: null
        });
      },
      markAppointmentConfirmationPending: (id: string) => {
        pendingCalls.push(id);
        return of({
          id: 'appointment-1',
          branchId: 'branch-1',
          patientId: 'patient-1',
          patientFullName: 'Ana Lopez',
          startsAt: '2026-04-14T09:00:00',
          endsAt: '2026-04-14T09:30:00',
          status: 'Scheduled',
          confirmationStatus: 'Pending',
          confirmedAtUtc: null,
          confirmedByUserId: null,
          notes: 'Follow-up',
          cancellationReason: null
        });
      },
      addReminderLogEntry: (appointmentId: string, payload: unknown) => {
        reminderLogAdds.push({ appointmentId, payload });
        return of({
          id: 'entry-1',
          appointmentId,
          channel: 'Phone',
          outcome: 'Reached',
          notes: 'Confirmed by phone.',
          createdAtUtc: '2026-04-14T08:00:00Z',
          createdByUserId: 'user-1'
        });
      },
      configureManualReminder: (appointmentId: string, payload: unknown) => {
        manualReminderConfigs.push({ appointmentId, payload });
        return of({
          id: 'appointment-1',
          branchId: 'branch-1',
          patientId: 'patient-1',
          patientFullName: 'Ana Lopez',
          startsAt: '2026-04-14T09:00:00',
          endsAt: '2026-04-14T09:30:00',
          status: 'Scheduled',
          confirmationStatus: 'Pending',
          confirmedAtUtc: null,
          confirmedByUserId: null,
          reminderRequired: (payload as any).required,
          reminderChannel: (payload as any).channel,
          reminderDueAtUtc: (payload as any).dueAtUtc,
          reminderCompletedAtUtc: null,
          reminderCompletedByUserId: null,
          reminderUpdatedAtUtc: '2026-04-14T08:00:00Z',
          reminderUpdatedByUserId: 'user-1',
          notes: 'Follow-up',
          cancellationReason: null
        });
      },
      completeManualReminder: (appointmentId: string) => {
        manualReminderCompletions.push(appointmentId);
        return of({
          id: 'appointment-1',
          branchId: 'branch-1',
          patientId: 'patient-1',
          patientFullName: 'Ana Lopez',
          startsAt: '2026-04-14T09:00:00',
          endsAt: '2026-04-14T09:30:00',
          status: 'Scheduled',
          confirmationStatus: 'Pending',
          confirmedAtUtc: null,
          confirmedByUserId: null,
          reminderRequired: true,
          reminderChannel: 'Phone',
          reminderDueAtUtc: '2026-04-14T08:00:00Z',
          reminderCompletedAtUtc: '2026-04-14T08:30:00Z',
          reminderCompletedByUserId: 'user-1',
          reminderUpdatedAtUtc: '2026-04-14T08:30:00Z',
          reminderUpdatedByUserId: 'user-1',
          notes: 'Follow-up',
          cancellationReason: null
        });
      },
      recordManualReminderFollowUp: (appointmentId: string, payload: unknown) => {
        reminderFollowUps.push({ appointmentId, payload });
        return of({
          appointment: {
            id: appointmentId,
            branchId: 'branch-1',
            patientId: 'patient-1',
            patientFullName: 'Ana Lopez',
            startsAt: '2026-04-14T09:00:00',
            endsAt: '2026-04-14T09:30:00',
            status: 'Scheduled',
            confirmationStatus: 'Confirmed',
            confirmedAtUtc: '2026-04-14T08:30:00Z',
            confirmedByUserId: 'user-1',
            reminderRequired: true,
            reminderChannel: 'Phone',
            reminderDueAtUtc: '2026-04-14T08:00:00Z',
            reminderCompletedAtUtc: '2026-04-14T08:30:00Z',
            reminderCompletedByUserId: 'user-1',
            reminderUpdatedAtUtc: '2026-04-14T08:30:00Z',
            reminderUpdatedByUserId: 'user-1',
            notes: 'Follow-up',
            cancellationReason: null
          },
          reminderLogEntry: {
            id: 'entry-4',
            appointmentId,
            channel: 'Phone',
            outcome: 'Reached',
            notes: 'Confirmed by phone.',
            createdAtUtc: '2026-04-14T08:30:00Z',
            createdByUserId: 'user-1'
          }
        });
      },
      createReminderTemplate: (payload: unknown) => {
        reminderTemplateCreates.push(payload);
        return of({
          id: 'template-2',
          name: (payload as any).name,
          body: (payload as any).body,
          isActive: true,
          createdAtUtc: '2026-04-14T08:00:00Z',
          createdByUserId: 'user-1',
          updatedAtUtc: null,
          updatedByUserId: null,
          deactivatedAtUtc: null,
          deactivatedByUserId: null
        });
      },
      updateReminderTemplate: (id: string, payload: unknown) => {
        reminderTemplateUpdates.push({ id, payload });
        return of({
          id,
          name: (payload as any).name,
          body: (payload as any).body,
          isActive: true,
          createdAtUtc: '2026-04-14T08:00:00Z',
          createdByUserId: 'user-1',
          updatedAtUtc: '2026-04-14T09:00:00Z',
          updatedByUserId: 'user-1',
          deactivatedAtUtc: null,
          deactivatedByUserId: null
        });
      },
      deactivateReminderTemplate: (id: string) => {
        reminderTemplateDeactivations.push(id);
        return of(void 0);
      },
      previewReminderTemplate: (templateId: string, appointmentId: string) => {
        reminderTemplatePreviews.push({ templateId, appointmentId });
        const preview = {
          templateId,
          appointmentId,
          renderedBody: 'Hola Ana Lopez.',
          unknownPlaceholders: ['doctorName']
        };
        facade.reminderTemplatePreview.set(preview);
        return of(preview);
      },
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
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
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
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      notes: 'Follow-up',
      cancellationReason: null
    });

    expect(component.editorMode).toBe('create');
  });

  it('loads reminder log entries when an appointment is selected', () => {
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
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      notes: 'Follow-up',
      cancellationReason: null
    });

    expect(reminderLogLoads).toContain('appointment-1');
  });

  it('loads reminder templates on init', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);

    fixture.componentInstance.ngOnInit();

    expect(reminderTemplateLoads).toBe(1);
  });

  it('adds a manual reminder log entry through the scheduling facade', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;
    component.selectedAppointment = {
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'NoShow',
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      notes: 'Follow-up',
      cancellationReason: null
    };

    component.addReminderLogEntry({
      channel: 'Phone',
      outcome: 'Reached',
      notes: 'Confirmed by phone.'
    });

    expect(reminderLogAdds).toEqual([
      {
        appointmentId: 'appointment-1',
        payload: {
          channel: 'Phone',
          outcome: 'Reached',
          notes: 'Confirmed by phone.'
        }
      }
    ]);
    expect(component.savingReminderLog).toBe(false);
  });

  it('configures a manual reminder through the scheduling facade', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;
    component.selectedAppointment = {
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'Scheduled',
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      notes: 'Follow-up',
      cancellationReason: null
    };

    component.setManualReminder({
      channel: 'Phone',
      dueAtUtc: '2026-04-14T08:00:00Z'
    });

    expect(manualReminderConfigs).toEqual([
      {
        appointmentId: 'appointment-1',
        payload: {
          required: true,
          channel: 'Phone',
          dueAtUtc: '2026-04-14T08:00:00Z'
        }
      }
    ]);
    expect(component.selectedAppointment?.reminderRequired).toBe(true);
    expect(component.savingManualReminder).toBe(false);
  });

  it('clears a manual reminder through the scheduling facade', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;
    component.selectedAppointment = {
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'Scheduled',
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      reminderRequired: true,
      reminderChannel: 'Phone',
      reminderDueAtUtc: '2026-04-14T08:00:00Z',
      notes: 'Follow-up',
      cancellationReason: null
    };

    component.clearManualReminder();

    expect(manualReminderConfigs).toEqual([
      {
        appointmentId: 'appointment-1',
        payload: {
          required: false,
          channel: null,
          dueAtUtc: null
        }
      }
    ]);
    expect(component.selectedAppointment?.reminderRequired).toBe(false);
    expect(component.savingManualReminder).toBe(false);
  });

  it('completes a manual reminder through the scheduling facade', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;
    component.selectedAppointment = {
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'Scheduled',
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      reminderRequired: true,
      reminderChannel: 'Phone',
      reminderDueAtUtc: '2026-04-14T08:00:00Z',
      notes: 'Follow-up',
      cancellationReason: null
    };

    component.completeManualReminder();

    expect(manualReminderCompletions).toEqual(['appointment-1']);
    expect(component.selectedAppointment?.reminderCompletedAtUtc).toBe('2026-04-14T08:30:00Z');
    expect(component.savingManualReminder).toBe(false);
  });

  it('records a manual reminder follow-up from the worklist through the scheduling facade', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;
    component.selectedAppointment = {
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-14T09:00:00',
      endsAt: '2026-04-14T09:30:00',
      status: 'Scheduled',
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      reminderRequired: true,
      reminderChannel: 'Phone',
      reminderDueAtUtc: '2026-04-14T08:00:00Z',
      notes: 'Follow-up',
      cancellationReason: null
    };

    component.recordManualReminderFollowUp('appointment-1', {
      channel: 'Phone',
      outcome: 'Reached',
      notes: 'Confirmed by phone.',
      completeReminder: true,
      confirmAppointment: true
    });

    expect(reminderFollowUps).toEqual([
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
    expect(component.selectedAppointment?.confirmationStatus).toBe('Confirmed');
    expect(component.selectedAppointment?.reminderCompletedAtUtc).toBe('2026-04-14T08:30:00Z');
    expect(reminderLogLoads).toContain('appointment-1');
    expect(component.savingReminderFollowUpAppointmentId).toBeNull();
  });

  it('creates and updates reminder templates through the scheduling facade', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;

    component.saveReminderTemplate({
      id: null,
      name: 'Confirmacion',
      body: 'Hola {{patientName}}.'
    });
    component.saveReminderTemplate({
      id: 'template-1',
      name: 'Updated',
      body: 'Updated body'
    });

    expect(reminderTemplateCreates).toEqual([{ name: 'Confirmacion', body: 'Hola {{patientName}}.' }]);
    expect(reminderTemplateUpdates).toEqual([{ id: 'template-1', payload: { name: 'Updated', body: 'Updated body' } }]);
    expect(component.savingReminderTemplate).toBe(false);
  });

  it('deactivates reminder templates only after confirmation', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;
    const originalConfirm = window.confirm;
    window.confirm = () => true;

    try {
      component.deactivateReminderTemplate('template-1');

      expect(reminderTemplateDeactivations).toEqual(['template-1']);
      expect(component.savingReminderTemplate).toBe(false);
    } finally {
      window.confirm = originalConfirm;
    }
  });

  it('previews a reminder template for an appointment without saving follow-up notes', () => {
    const fixture = TestBed.createComponent(SchedulingPageComponent);
    const component = fixture.componentInstance;

    component.previewReminderTemplate('template-1', 'appointment-1');

    expect(reminderTemplatePreviews).toEqual([
      {
        templateId: 'template-1',
        appointmentId: 'appointment-1'
      }
    ]);
    expect(reminderFollowUps).toEqual([]);
    expect(facade.reminderTemplatePreview()?.renderedBody).toBe('Hola Ana Lopez.');
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
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
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

  it('marks the selected appointment as no-show through the scheduling facade', () => {
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
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      notes: 'Follow-up',
      cancellationReason: null
    };

    try {
      component.markSelectedNoShow();

      expect(noShowCalls).toEqual(['appointment-1']);
      expect(component.selectedAppointment?.status).toBe('NoShow');
      expect(component.editorMode).toBe('create');
    } finally {
      window.confirm = originalConfirm;
    }
  });

  it('confirms the selected appointment through the scheduling facade', () => {
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
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      notes: 'Follow-up',
      cancellationReason: null
    };

    try {
      component.confirmSelectedAppointment();

      expect(confirmationCalls).toEqual(['appointment-1']);
      expect(component.selectedAppointment?.confirmationStatus).toBe('Confirmed');
      expect(component.selectedAppointment?.confirmedByUserId).toBe('user-1');
      expect(component.editorMode).toBe('edit');
    } finally {
      window.confirm = originalConfirm;
    }
  });

  it('marks the selected appointment confirmation as pending through the scheduling facade', () => {
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
      confirmationStatus: 'Confirmed',
      confirmedAtUtc: '2026-04-14T08:00:00Z',
      confirmedByUserId: 'user-1',
      notes: 'Follow-up',
      cancellationReason: null
    };

    try {
      component.markSelectedConfirmationPending();

      expect(pendingCalls).toEqual(['appointment-1']);
      expect(component.selectedAppointment?.confirmationStatus).toBe('Pending');
      expect(component.selectedAppointment?.confirmedAtUtc).toBeNull();
      expect(component.selectedAppointment?.confirmedByUserId).toBeNull();
      expect(component.editorMode).toBe('edit');
    } finally {
      window.confirm = originalConfirm;
    }
  });
});
