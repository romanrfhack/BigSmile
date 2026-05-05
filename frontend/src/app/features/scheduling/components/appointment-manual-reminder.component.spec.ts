import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { AppointmentSummary } from '../models/scheduling.models';
import { AppointmentManualReminderComponent } from './appointment-manual-reminder.component';

describe('AppointmentManualReminderComponent', () => {
  let fixture: ComponentFixture<AppointmentManualReminderComponent>;
  let component: AppointmentManualReminderComponent;

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');

    await TestBed.configureTestingModule({
      imports: [AppointmentManualReminderComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentManualReminderComponent);
    component = fixture.componentInstance;
  });

  it('renders the manual reminder section without send or provider actions', () => {
    component.appointment = buildAppointment();
    component.canWrite = true;

    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Manual reminder');
    expect(text).toContain('Manual scheduling only');
    expect(text).toContain('Set reminder');
    expect(text).not.toContain('Send');
    expect(text).not.toContain('Provider');
  });

  it('emits a manual reminder value with a UTC due date', () => {
    const emitted: unknown[] = [];
    component.appointment = buildAppointment();
    component.canWrite = true;
    component.saved.subscribe(value => emitted.push(value));
    fixture.detectChanges();

    component.channel = 'WhatsApp';
    component.dueAt = '2026-04-27T14:00';
    component.submit();

    expect(emitted).toEqual([
      {
        channel: 'WhatsApp',
        dueAtUtc: new Date('2026-04-27T14:00').toISOString()
      }
    ]);
  });

  it('emits clear and complete actions for an active manual reminder', () => {
    const cleared: unknown[] = [];
    const completed: unknown[] = [];
    component.appointment = {
      ...buildAppointment(),
      reminderRequired: true,
      reminderChannel: 'Phone',
      reminderDueAtUtc: '2026-04-27T14:00:00Z'
    };
    component.canWrite = true;
    component.cleared.subscribe(value => cleared.push(value));
    component.completed.subscribe(value => completed.push(value));
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('button'));
    buttons.find(button => button.nativeElement.textContent.includes('Clear reminder'))?.nativeElement.click();
    buttons.find(button => button.nativeElement.textContent.includes('Mark reminder completed'))?.nativeElement.click();

    expect(cleared.length).toBe(1);
    expect(completed.length).toBe(1);
  });

  it('blocks missing due date before emitting', () => {
    const emitted: unknown[] = [];
    component.appointment = buildAppointment();
    component.canWrite = true;
    component.saved.subscribe(value => emitted.push(value));

    component.dueAt = '';
    component.submit();

    expect(emitted).toEqual([]);
    expect(component.formError).toContain('required');
  });
});

function buildAppointment(): AppointmentSummary {
  return {
    id: 'appointment-1',
    branchId: 'branch-1',
    patientId: 'patient-1',
    patientFullName: 'Ana Lopez',
    startsAt: '2026-04-27T09:00:00Z',
    endsAt: '2026-04-27T09:30:00Z',
    status: 'Scheduled',
    confirmationStatus: 'Pending',
    confirmedAtUtc: null,
    confirmedByUserId: null,
    reminderRequired: false,
    reminderChannel: null,
    reminderDueAtUtc: null,
    reminderCompletedAtUtc: null,
    reminderCompletedByUserId: null,
    reminderUpdatedAtUtc: null,
    reminderUpdatedByUserId: null,
    notes: null,
    cancellationReason: null
  };
}
