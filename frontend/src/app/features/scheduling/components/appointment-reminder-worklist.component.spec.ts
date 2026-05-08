import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { AppointmentReminderWorklistComponent } from './appointment-reminder-worklist.component';

describe('AppointmentReminderWorklistComponent', () => {
  let fixture: ComponentFixture<AppointmentReminderWorklistComponent>;
  let component: AppointmentReminderWorklistComponent;

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');

    await TestBed.configureTestingModule({
      imports: [AppointmentReminderWorklistComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentReminderWorklistComponent);
    component = fixture.componentInstance;
  });

  it('renders the empty manual reminder worklist state', () => {
    component.items = [];

    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Pending reminders');
    expect(text).toContain('Manual reminder work');
    expect(text).toContain('No pending or due manual reminders');
    expect(text).not.toContain('Send');
  });

  it('renders pending and due reminder work items', () => {
    component.items = [
      {
        appointmentId: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-27T09:00:00Z',
        appointmentStatus: 'Scheduled',
        confirmationStatus: 'Pending',
        reminderChannel: 'Phone',
        reminderDueAtUtc: '2026-04-27T08:00:00Z',
        reminderState: 'Due',
        reminderCompletedAtUtc: null,
        reminderCompletedByUserId: null
      },
      {
        appointmentId: 'appointment-2',
        branchId: 'branch-1',
        patientId: 'patient-2',
        patientFullName: 'Bruno Garcia',
        startsAt: '2026-04-27T11:00:00Z',
        appointmentStatus: 'Scheduled',
        confirmationStatus: 'Confirmed',
        reminderChannel: 'Email',
        reminderDueAtUtc: '2026-04-27T10:00:00Z',
        reminderState: 'Pending',
        reminderCompletedAtUtc: null,
        reminderCompletedByUserId: null
      }
    ];

    fixture.detectChanges();

    const entries = fixture.debugElement.queryAll(By.css('.work-item'));
    expect(entries.length).toBe(2);
    expect(entries[0].nativeElement.textContent).toContain('Ana Lopez');
    expect(entries[0].nativeElement.textContent).toContain('Due');
    expect(entries[1].nativeElement.textContent).toContain('Bruno Garcia');
    expect(entries[1].nativeElement.textContent).toContain('Pending');
  });

  it('renders record follow-up action and emits explicit manual follow-up flags', () => {
    const emitted: unknown[] = [];
    component.canWrite = true;
    component.items = [
      {
        appointmentId: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-27T09:00:00Z',
        appointmentStatus: 'Scheduled',
        confirmationStatus: 'Pending',
        reminderChannel: 'Phone',
        reminderDueAtUtc: '2026-04-27T08:00:00Z',
        reminderState: 'Due',
        reminderCompletedAtUtc: null,
        reminderCompletedByUserId: null
      }
    ];
    component.followUpSaved.subscribe(value => emitted.push(value));

    fixture.detectChanges();
    const action = fixture.debugElement.query(By.css('button.btn-secondary'));
    expect(action.nativeElement.textContent).toContain('Record follow-up');
    action.triggerEventHandler('click');
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Manual record only. BigSmile does not send messages.');
    expect(text).toContain('Mark reminder completed');
    expect(text).toContain('Confirm appointment');
    const sendButtons = fixture.debugElement
      .queryAll(By.css('button'))
      .filter(button => button.nativeElement.textContent.trim() === 'Send');
    expect(sendButtons.length).toBe(0);

    component.outcome = 'Reached';
    component.notes = ' Confirmed by phone. ';
    component.completeReminder = true;
    component.confirmAppointment = true;
    component.submitFollowUp(component.items[0]!);

    expect(emitted).toEqual([
      {
        appointmentId: 'appointment-1',
        value: {
          channel: 'Phone',
          outcome: 'Reached',
          notes: 'Confirmed by phone.',
          completeReminder: true,
          confirmAppointment: true,
          reminderTemplateId: null
        }
      }
    ]);
  });

  it('previews a template and uses rendered body as manual follow-up notes only', () => {
    const previewRequests: unknown[] = [];
    const followUps: unknown[] = [];
    component.canWrite = true;
    component.reminderTemplates = [
      {
        id: 'template-1',
        name: 'Confirmacion',
        body: 'Hola {{patientName}}.',
        isActive: true,
        createdAtUtc: '2026-04-27T08:00:00Z',
        createdByUserId: 'user-1',
        updatedAtUtc: null,
        updatedByUserId: null,
        deactivatedAtUtc: null,
        deactivatedByUserId: null
      }
    ];
    component.items = [
      {
        appointmentId: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-27T09:00:00Z',
        appointmentStatus: 'Scheduled',
        confirmationStatus: 'Pending',
        reminderChannel: 'Phone',
        reminderDueAtUtc: '2026-04-27T08:00:00Z',
        reminderState: 'Due',
        reminderCompletedAtUtc: null,
        reminderCompletedByUserId: null
      }
    ];
    component.templatePreviewRequested.subscribe(value => previewRequests.push(value));
    component.followUpSaved.subscribe(value => followUps.push(value));

    component.startFollowUp(component.items[0]!);
    component.selectedTemplateId = 'template-1';
    component.requestTemplatePreview(component.items[0]!);

    expect(previewRequests).toEqual([
      {
        templateId: 'template-1',
        appointmentId: 'appointment-1'
      }
    ]);

    component.templatePreview = {
      templateId: 'template-1',
      appointmentId: 'appointment-1',
      renderedBody: 'Hola Ana Lopez, le recordamos su cita.',
      unknownPlaceholders: ['customPlaceholder']
    };
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Rendered preview');
    expect(text).toContain('Unknown placeholders: customPlaceholder');
    expect(text).not.toContain('Send');

    component.usePreviewAsNote();
    component.submitFollowUp(component.items[0]!);

    expect(followUps).toEqual([
      {
        appointmentId: 'appointment-1',
        value: {
          channel: 'Phone',
          outcome: 'Reached',
          notes: 'Hola Ana Lopez, le recordamos su cita.',
          completeReminder: false,
          confirmAppointment: false,
          reminderTemplateId: 'template-1'
        }
      }
    ]);
  });

  it('submits no template trace when notes are typed without using a template preview', () => {
    const followUps: unknown[] = [];
    component.canWrite = true;
    component.items = [
      {
        appointmentId: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-27T09:00:00Z',
        appointmentStatus: 'Scheduled',
        confirmationStatus: 'Pending',
        reminderChannel: 'Phone',
        reminderDueAtUtc: '2026-04-27T08:00:00Z',
        reminderState: 'Due',
        reminderCompletedAtUtc: null,
        reminderCompletedByUserId: null
      }
    ];
    component.followUpSaved.subscribe(value => followUps.push(value));

    component.startFollowUp(component.items[0]!);
    component.notes = 'Typed manually.';
    component.submitFollowUp(component.items[0]!);

    expect(followUps).toEqual([
      {
        appointmentId: 'appointment-1',
        value: {
          channel: 'Phone',
          outcome: 'Reached',
          notes: 'Typed manually.',
          completeReminder: false,
          confirmAppointment: false,
          reminderTemplateId: null
        }
      }
    ]);
  });

  it('clears selected template source when follow-up is cancelled', () => {
    component.items = [
      {
        appointmentId: 'appointment-1',
        branchId: 'branch-1',
        patientId: 'patient-1',
        patientFullName: 'Ana Lopez',
        startsAt: '2026-04-27T09:00:00Z',
        appointmentStatus: 'Scheduled',
        confirmationStatus: 'Pending',
        reminderChannel: 'Phone',
        reminderDueAtUtc: '2026-04-27T08:00:00Z',
        reminderState: 'Due',
        reminderCompletedAtUtc: null,
        reminderCompletedByUserId: null
      }
    ];

    component.startFollowUp(component.items[0]!);
    component.selectedTemplateId = 'template-1';
    component.templatePreview = {
      templateId: 'template-1',
      appointmentId: 'appointment-1',
      renderedBody: 'Hola Ana Lopez.',
      unknownPlaceholders: []
    };
    component.usePreviewAsNote();

    expect(component.selectedTemplateSourceId).toBe('template-1');

    component.cancelFollowUp();

    expect(component.activeAppointmentId).toBeNull();
    expect(component.selectedTemplateId).toBeNull();
    expect(component.selectedTemplateSourceId).toBeNull();
  });
});
