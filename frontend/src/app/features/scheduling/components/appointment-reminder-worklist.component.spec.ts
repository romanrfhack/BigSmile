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
});
