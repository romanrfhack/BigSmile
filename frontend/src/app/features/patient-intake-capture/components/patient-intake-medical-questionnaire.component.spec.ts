import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  MEDICAL_QUESTIONNAIRE_GROUPS,
  MEDICAL_QUESTIONNAIRE_KEYS
} from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';
import {
  PatientIntakeDraft,
  PatientIntakeMedicalAnswerFormValue
} from '../models/patient-intake.models';
import { PatientIntakeMedicalQuestionnaireComponent } from './patient-intake-medical-questionnaire.component';

describe('PatientIntakeMedicalQuestionnaireComponent', () => {
  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    window.sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [PatientIntakeMedicalQuestionnaireComponent]
    }).compileComponents();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('renders six familiar groups, 39 questions, and visible Yes/No/Unknown options', () => {
    const fixture = createComponent(draft());
    fixture.detectChanges();

    expect(MEDICAL_QUESTIONNAIRE_GROUPS).toHaveLength(6);
    expect(fixture.nativeElement.querySelectorAll('.medical-group')).toHaveLength(6);
    expect(fixture.nativeElement.querySelectorAll('.medical-card')).toHaveLength(39);
    expect(fixture.nativeElement.querySelectorAll('input[type="radio"]')).toHaveLength(117);
    expect(fixture.componentInstance.capturedQuestionCount).toBe(0);
    expect(fixture.nativeElement.textContent).toContain('0 / 39');

    const unknown = fixture.nativeElement.querySelector(
      '#patient-intake-medical-answer-diabetes-unknown') as HTMLInputElement;
    expect(unknown.checked).toBe(true);
    expect(unknown.parentElement?.textContent).toContain('No answer');
  });

  it('counts only non-Unknown answers and preserves details when changing away from Yes', () => {
    const fixture = createComponent(draft());
    const answer = fixture.componentInstance.answerForm('diabetes');

    answer.controls.answer.setValue('Yes');
    answer.controls.details.setValue('  Controlled with diet.  ');
    fixture.detectChanges();

    expect(fixture.componentInstance.capturedQuestionCount).toBe(1);
    expect(fixture.nativeElement.querySelector('#patient-intake-medical-details-diabetes')).not.toBeNull();

    answer.controls.answer.setValue('No');
    fixture.detectChanges();

    expect(fixture.componentInstance.capturedQuestionCount).toBe(1);
    expect(answer.controls.details.value).toBe('  Controlled with diet.  ');
    expect(fixture.nativeElement.querySelector('#patient-intake-medical-details-diabetes')).not.toBeNull();
  });

  it('emits the complete ordered catalog only on explicit submit without browser persistence', () => {
    const fixture = createComponent(draft());
    const emitted: PatientIntakeMedicalAnswerFormValue[][] = [];
    fixture.componentInstance.saveRequested.subscribe(value => emitted.push(value));

    fixture.componentInstance.answerForm('diabetes').controls.answer.setValue('Yes');
    fixture.componentInstance.answerForm('diabetes').controls.details.setValue('Diet controlled.');
    fixture.detectChanges();

    expect(emitted).toHaveLength(0);
    fixture.componentInstance.submit();

    expect(emitted).toHaveLength(1);
    expect(emitted[0]).toHaveLength(39);
    expect(emitted[0].map(answer => answer.questionKey)).toEqual(MEDICAL_QUESTIONNAIRE_KEYS);
    expect(new Set(emitted[0].map(answer => answer.questionKey)).size).toBe(39);
    expect(emitted[0].find(answer => answer.questionKey === 'diabetes')).toEqual({
      questionKey: 'diabetes',
      answer: 'Yes',
      details: 'Diet controlled.'
    });
    expect(window.localStorage.length).toBe(1);
    expect(window.localStorage.getItem('bigsmile.ui.language')).toBe('en-US');
    expect(window.sessionStorage.length).toBe(0);
  });

  it('blocks submit when optional details exceed the accepted maximum', () => {
    const fixture = createComponent(draft());
    const emitted: PatientIntakeMedicalAnswerFormValue[][] = [];
    fixture.componentInstance.saveRequested.subscribe(value => emitted.push(value));
    const answer = fixture.componentInstance.answerForm('diabetes');

    answer.controls.answer.setValue('Yes');
    answer.controls.details.setValue('x'.repeat(501));
    answer.controls.details.markAsTouched();
    fixture.detectChanges();
    fixture.componentInstance.submit();

    expect(answer.controls.details.hasError('maxlength')).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Details must be 500 characters or fewer.');
    expect(emitted).toHaveLength(0);
  });
});

function createComponent(intake: PatientIntakeDraft): ComponentFixture<PatientIntakeMedicalQuestionnaireComponent> {
  const fixture = TestBed.createComponent(PatientIntakeMedicalQuestionnaireComponent);
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
    reasonForVisit: 'Revisión general.',
    medicalAnswers: MEDICAL_QUESTIONNAIRE_KEYS.map(questionKey => ({
      questionKey,
      answer: 'Unknown',
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
