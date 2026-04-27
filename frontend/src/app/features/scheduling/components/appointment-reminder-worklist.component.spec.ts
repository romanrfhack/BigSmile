import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { AppointmentReminderWorklistComponent } from './appointment-reminder-worklist.component';

describe('AppointmentReminderWorklistComponent', () => {
  let fixture: ComponentFixture<AppointmentReminderWorklistComponent>;
  let component: AppointmentReminderWorklistComponent;

  beforeEach(async () => {
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
          confirmAppointment: true
        }
      }
    ]);
  });
});
