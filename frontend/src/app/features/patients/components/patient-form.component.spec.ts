import { TestBed } from '@angular/core/testing';
import { PatientFormComponent } from './patient-form.component';

describe('PatientFormComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientFormComponent]
    }).compileComponents();
  });

  it('emits a normalized patient payload when the form is valid', () => {
    const fixture = TestBed.createComponent(PatientFormComponent);
    const component = fixture.componentInstance;
    let emittedPayload: unknown = null;

    component.saved.subscribe((payload) => {
      emittedPayload = payload;
    });
    component.form.setValue({
      firstName: ' Ana ',
      lastName: ' Lopez ',
      dateOfBirth: '1991-02-14',
      primaryPhone: '5551234567',
      email: 'ana@example.com',
      isActive: true,
      responsiblePartyName: ' Maria Lopez ',
      responsiblePartyRelationship: ' Mother ',
      responsiblePartyPhone: '5559990000'
    });

    component.submit();

    expect(emittedPayload).toEqual({
      firstName: 'Ana',
      lastName: 'Lopez',
      dateOfBirth: '1991-02-14',
      primaryPhone: '5551234567',
      email: 'ana@example.com',
      isActive: true,
      responsiblePartyName: 'Maria Lopez',
      responsiblePartyRelationship: 'Mother',
      responsiblePartyPhone: '5559990000'
    });
  });

  it('requires a responsible party name when related data is filled', () => {
    const fixture = TestBed.createComponent(PatientFormComponent);
    const component = fixture.componentInstance;

    component.form.patchValue({
      firstName: 'Ana',
      lastName: 'Lopez',
      dateOfBirth: '1991-02-14',
      responsiblePartyRelationship: 'Mother'
    });

    component.submit();

    expect(component.form.errors?.['responsiblePartyNameRequired']).toBe(true);
  });
});
