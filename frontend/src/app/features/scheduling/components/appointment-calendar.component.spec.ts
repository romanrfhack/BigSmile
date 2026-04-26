import { TestBed } from '@angular/core/testing';
import { AppointmentCalendarComponent } from './appointment-calendar.component';

describe('AppointmentCalendarComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentCalendarComponent]
    }).compileComponents();
  });

  it('renders distinct status badges for attended and no-show appointments', () => {
    const fixture = TestBed.createComponent(AppointmentCalendarComponent);
    const component = fixture.componentInstance;

    component.calendar = {
      branchId: 'branch-1',
      startDate: '2026-04-14',
      days: 1,
      calendarDays: [
        {
          date: '2026-04-14',
          appointments: [
            {
              id: 'appointment-1',
              branchId: 'branch-1',
              patientId: 'patient-1',
              patientFullName: 'Ana Lopez',
              startsAt: '2026-04-14T09:00:00',
              endsAt: '2026-04-14T09:30:00',
              status: 'Attended',
              confirmationStatus: 'Confirmed',
              confirmedAtUtc: '2026-04-14T08:00:00Z',
              confirmedByUserId: 'user-1',
              notes: 'Follow-up',
              cancellationReason: null
            },
            {
              id: 'appointment-2',
              branchId: 'branch-1',
              patientId: 'patient-2',
              patientFullName: 'Bruno Garcia',
              startsAt: '2026-04-14T10:00:00',
              endsAt: '2026-04-14T10:30:00',
              status: 'NoShow',
              confirmationStatus: 'Pending',
              confirmedAtUtc: null,
              confirmedByUserId: null,
              notes: null,
              cancellationReason: null
            }
          ],
          blockedSlots: []
        }
      ]
    };

    fixture.detectChanges();

    const attendedCard = fixture.nativeElement.querySelector('.appointment-attended');
    const noShowCard = fixture.nativeElement.querySelector('.appointment-no-show');

    expect(attendedCard?.textContent).toContain('Attended');
    expect(attendedCard?.textContent).toContain('Confirmed');
    expect(noShowCard?.textContent).toContain('No-show');
    expect(noShowCard?.textContent).toContain('Pending confirmation');
  });
});
