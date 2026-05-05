import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { AppointmentReminderLogComponent } from './appointment-reminder-log.component';

describe('AppointmentReminderLogComponent', () => {
  let fixture: ComponentFixture<AppointmentReminderLogComponent>;
  let component: AppointmentReminderLogComponent;

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');

    await TestBed.configureTestingModule({
      imports: [AppointmentReminderLogComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentReminderLogComponent);
    component = fixture.componentInstance;
  });

  it('renders the empty manual reminder log state', () => {
    component.entries = [];
    component.canWrite = true;

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Reminder log');
    expect(fixture.nativeElement.textContent).toContain('Manual log only');
    expect(fixture.nativeElement.textContent).toContain('No contact attempts have been logged');
  });

  it('renders existing reminder log entries newest-first as provided', () => {
    component.entries = [
      {
        id: 'entry-2',
        appointmentId: 'appointment-1',
        channel: 'Phone',
        outcome: 'Reached',
        notes: 'Confirmed by phone.',
        createdAtUtc: '2026-04-25T08:00:00Z',
        createdByUserId: 'user-2',
        reminderTemplateId: 'template-1',
        reminderTemplateNameSnapshot: 'Confirmation reminder'
      },
      {
        id: 'entry-1',
        appointmentId: 'appointment-1',
        channel: 'Email',
        outcome: 'LeftMessage',
        notes: null,
        createdAtUtc: '2026-04-24T08:00:00Z',
        createdByUserId: 'user-1',
        reminderTemplateId: null,
        reminderTemplateNameSnapshot: null
      }
    ];

    fixture.detectChanges();

    const entries = fixture.debugElement.queryAll(By.css('.log-entry'));
    expect(entries.length).toBe(2);
    expect(entries[0].nativeElement.textContent).toContain('Phone');
    expect(entries[0].nativeElement.textContent).toContain('Reached');
    expect(entries[0].nativeElement.textContent).toContain('Confirmed by phone.');
    expect(entries[0].nativeElement.textContent).toContain('Template: Confirmation reminder');
    expect(entries[0].nativeElement.textContent).toContain('user-2');
    expect(entries[1].nativeElement.textContent).toContain('Email');
    expect(entries[1].nativeElement.textContent).toContain('Left message');
    expect(entries[1].nativeElement.textContent).not.toContain('Template:');
  });

  it('emits a manual reminder log form value and trims optional notes', () => {
    const emitted: unknown[] = [];
    component.canWrite = true;
    component.saved.subscribe(value => emitted.push(value));
    fixture.detectChanges();

    component.channel = 'WhatsApp';
    component.outcome = 'NoAnswer';
    component.notes = ' Tried manually outside BigSmile. ';
    component.submit();

    expect(emitted).toEqual([
      {
        channel: 'WhatsApp',
        outcome: 'NoAnswer',
        notes: 'Tried manually outside BigSmile.'
      }
    ]);
    expect(component.notes).toBe('');
  });

  it('blocks notes over the 500 character limit', () => {
    const emitted: unknown[] = [];
    component.canWrite = true;
    component.saved.subscribe(value => emitted.push(value));
    component.notes = 'a'.repeat(501);

    component.submit();

    expect(emitted).toEqual([]);
    expect(component.formError).toContain('500');
  });
});
