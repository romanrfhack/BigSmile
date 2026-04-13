import { TestBed } from '@angular/core/testing';
import { AppointmentBlockFormComponent } from './appointment-block-form.component';

describe('AppointmentBlockFormComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentBlockFormComponent]
    }).compileComponents();
  });

  it('emits a normalized blocked-slot payload when the form is valid', () => {
    const fixture = TestBed.createComponent(AppointmentBlockFormComponent);
    const component = fixture.componentInstance;
    let emittedPayload: unknown = null;

    component.draftDate = '2026-04-16';
    component.ngOnChanges({
      draftDate: {
        currentValue: '2026-04-16',
        previousValue: '',
        firstChange: true,
        isFirstChange: () => true
      }
    });

    component.saved.subscribe((payload) => {
      emittedPayload = payload;
    });

    component.form.patchValue({
      startsAt: '2026-04-16T13:00',
      endsAt: '2026-04-16T14:30',
      label: '  Lunch break  '
    });

    component.submit();

    expect(emittedPayload).toEqual({
      startsAt: '2026-04-16T13:00',
      endsAt: '2026-04-16T14:30',
      label: 'Lunch break'
    });
  });
});
