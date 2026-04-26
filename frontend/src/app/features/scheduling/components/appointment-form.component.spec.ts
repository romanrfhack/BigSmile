import { TestBed } from '@angular/core/testing';
import { AppointmentFormComponent } from './appointment-form.component';

describe('AppointmentFormComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentFormComponent]
    }).compileComponents();
  });

  it('emits a normalized create payload when the form is valid', () => {
    const fixture = TestBed.createComponent(AppointmentFormComponent);
    const component = fixture.componentInstance;
    let emittedPayload: unknown = null;

    component.patientOptions = [
      {
        id: 'patient-1',
        fullName: 'Ana Lopez',
        primaryPhone: '5551234567',
        email: 'ana@example.com',
        hasClinicalAlerts: true
      }
    ];
    component.draftDate = '2026-04-11';
    component.ngOnChanges({
      draftDate: {
        currentValue: '2026-04-11',
        previousValue: '',
        firstChange: true,
        isFirstChange: () => true
      }
    });

    component.saved.subscribe((payload) => {
      emittedPayload = payload;
    });

    component.selectPatient(component.patientOptions[0]);
    component.form.patchValue({
      startsAt: '2026-04-11T09:00',
      endsAt: '2026-04-11T09:30',
      notes: '  New patient consultation  '
    });

    component.submit();

    expect(emittedPayload).toEqual({
      patientId: 'patient-1',
      startsAt: '2026-04-11T09:00',
      endsAt: '2026-04-11T09:30',
      notes: 'New patient consultation'
    });
  });

  it('emits a reschedule payload without requiring patient search changes', () => {
    const fixture = TestBed.createComponent(AppointmentFormComponent);
    const component = fixture.componentInstance;
    let emittedPayload: unknown = null;

    component.mode = 'reschedule';
    component.initialAppointment = {
      id: 'appointment-1',
      branchId: 'branch-1',
      patientId: 'patient-1',
      patientFullName: 'Ana Lopez',
      startsAt: '2026-04-11T09:00:00',
      endsAt: '2026-04-11T09:30:00',
      status: 'Scheduled',
      confirmationStatus: 'Pending',
      confirmedAtUtc: null,
      confirmedByUserId: null,
      notes: 'Check-up',
      cancellationReason: null
    };
    component.ngOnChanges({
      mode: {
        currentValue: 'reschedule',
        previousValue: 'create',
        firstChange: true,
        isFirstChange: () => true
      },
      initialAppointment: {
        currentValue: component.initialAppointment,
        previousValue: null,
        firstChange: true,
        isFirstChange: () => true
      }
    });

    component.saved.subscribe((payload) => {
      emittedPayload = payload;
    });

    component.form.patchValue({
      startsAt: '2026-04-11T10:00',
      endsAt: '2026-04-11T10:30'
    });

    component.submit();

    expect(emittedPayload).toEqual({
      patientId: 'patient-1',
      startsAt: '2026-04-11T10:00',
      endsAt: '2026-04-11T10:30',
      notes: 'Check-up'
    });
  });
});
