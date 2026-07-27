import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PatientIntakeDraft, PatientIntakeNonMedicalFormValue } from '../models/patient-intake.models';
import {
  PatientIntakeDemographicsFormComponent,
  calculateAge
} from './patient-intake-demographics-form.component';

describe('PatientIntakeDemographicsFormComponent', () => {
  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    window.sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [PatientIntakeDemographicsFormComponent]
    }).compileComponents();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('renders the supported sections and loads the authoritative draft values', () => {
    const fixture = createComponent(draft());
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Identification');
    expect(text).toContain('Demographic information');
    expect(text).toContain('Contact information');
    expect(text).toContain('Responsible party');
    expect(text).toContain('Visit context');
    expect(fixture.componentInstance.form.getRawValue()).toMatchObject({
      firstName: 'Ana',
      lastName: 'López',
      dateOfBirth: '1990-05-10',
      sex: 'Female',
      maritalStatus: 'Single'
    });
  });

  it('derives age without adding an age field to the form value', () => {
    expect(calculateAge('2000-07-28', '2026-07-27')).toBe(25);
    expect(calculateAge('2000-07-27', '2026-07-27')).toBe(26);
    expect(calculateAge('2027-01-01', '2026-07-27')).toBeNull();

    const fixture = createComponent(draft());
    expect(fixture.componentInstance.form.getRawValue()).not.toHaveProperty('age');
  });

  it('rejects a future date of birth', () => {
    const fixture = createComponent(draft());
    const tomorrow = new Date(Date.now() + 86_400_000).toISOString().slice(0, 10);

    fixture.componentInstance.form.controls.dateOfBirth.setValue(tomorrow);
    fixture.componentInstance.form.controls.dateOfBirth.markAsTouched();
    fixture.detectChanges();

    expect(fixture.componentInstance.form.controls.dateOfBirth.hasError('futureDate')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Date of birth cannot be in the future.');
  });

  it('requires the responsible-party name when relationship or phone is supplied', () => {
    const fixture = createComponent(draft());

    fixture.componentInstance.form.patchValue({
      responsiblePartyName: '',
      responsiblePartyRelationship: 'Mother',
      responsiblePartyPhone: '555-0000'
    });
    fixture.componentInstance.form.controls.responsiblePartyRelationship.markAsTouched();
    fixture.detectChanges();

    expect(fixture.componentInstance.form.hasError('responsiblePartyNameRequired')).toBe(true);
    expect(fixture.nativeElement.textContent)
      .toContain('Responsible party name is required when relationship or phone is provided.');
  });

  it('emits only on explicit submit and never writes form data to browser storage', () => {
    const fixture = createComponent(draft());
    const emitted: PatientIntakeNonMedicalFormValue[] = [];
    fixture.componentInstance.saveRequested.subscribe(value => emitted.push(value));

    fixture.componentInstance.form.patchValue({
      firstName: 'María',
      mobilePhone: '+52 55 0000 0000',
      reasonForVisit: 'Dolor al masticar.'
    });
    fixture.detectChanges();

    expect(emitted).toHaveLength(0);
    fixture.componentInstance.submit();

    expect(emitted).toHaveLength(1);
    expect(emitted[0]).toMatchObject({
      firstName: 'María',
      mobilePhone: '+52 55 0000 0000',
      reasonForVisit: 'Dolor al masticar.'
    });
    expect(window.localStorage.length).toBe(1);
    expect(window.localStorage.getItem('bigsmile.ui.language')).toBe('en-US');
    expect(window.sessionStorage.length).toBe(0);
  });

  it('shows saved and unchanged outcomes without inventing revision changes', () => {
    const fixture = createComponent(draft());
    fixture.componentRef.setInput('saveOutcome', 'saved');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Your draft was saved.');

    fixture.componentRef.setInput('saveOutcome', 'unchanged');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent)
      .toContain('No changes were detected. The existing revision was preserved.');
  });
});

function createComponent(
  intake: PatientIntakeDraft
): ComponentFixture<PatientIntakeDemographicsFormComponent> {
  const fixture = TestBed.createComponent(PatientIntakeDemographicsFormComponent);
  fixture.componentRef.setInput('intake', intake);
  fixture.detectChanges();
  return fixture;
}

function draft(): PatientIntakeDraft {
  return {
    origin: 'ExistingPatientPortal',
    status: 'Draft',
    firstName: 'Ana',
    lastName: 'López',
    dateOfBirth: '1990-05-10',
    sex: 'Female',
    occupation: 'Contadora',
    maritalStatus: 'Single',
    referredBy: null,
    preferredPhone: null,
    mobilePhone: null,
    homePhone: null,
    workPhone: null,
    email: 'ana@example.test',
    responsiblePartyName: null,
    responsiblePartyRelationship: null,
    responsiblePartyPhone: null,
    reasonForVisit: null,
    medicalAnswers: Array.from({ length: 39 }, (_, index) => ({
      questionKey: `question-${index + 1}`,
      answer: 'Unknown' as const,
      details: null
    })),
    currentRevisionNumber: 0,
    concurrencyToken: 'rv1.token',
    createdAtUtc: '2026-07-27T10:00:00Z',
    lastUpdatedAtUtc: '2026-07-27T10:00:00Z',
    lastEffectiveSavedAtUtc: null,
    expiresAtUtc: '2026-08-26T10:00:00Z'
  };
}
