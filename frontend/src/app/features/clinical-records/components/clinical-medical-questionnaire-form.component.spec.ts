import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  CLINICAL_MEDICAL_QUESTION_KEYS
} from '../models/clinical-medical-questionnaire.catalog';
import {
  ClinicalMedicalAnswerValue,
  ClinicalMedicalQuestionKey,
  ClinicalMedicalQuestionnaire,
  SaveClinicalMedicalQuestionnaireRequest
} from '../models/clinical-record.models';
import { ClinicalMedicalQuestionnaireFormComponent } from './clinical-medical-questionnaire-form.component';

describe('ClinicalMedicalQuestionnaireFormComponent', () => {
  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');

    await TestBed.configureTestingModule({
      imports: [ClinicalMedicalQuestionnaireFormComponent]
    }).compileComponents();
  });

  it('renders the fixed medical questionnaire groups, questions, and completion progress', () => {
    const fixture = createComponent(buildQuestionnaire());
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Treatment and history');
    expect(text).toContain('Allergies');
    expect(text).toContain('Blood pressure, blood, and coagulation');
    expect(text).toContain('Systemic diseases');
    expect(text).toContain('Habits and dental conditions');
    expect(text).toContain('Pregnancy, anesthesia, and special conditions');
    expect(text).toContain('Diabetes');
    expect(text).toContain(`0 / ${CLINICAL_MEDICAL_QUESTION_KEYS.length}`);
    expect(text).toContain('No medical questionnaire answers have been captured yet.');
  });

  it('shows visible Yes, No, and No answer options and reveals details when answer is Yes', () => {
    const fixture = createComponent(buildQuestionnaire());
    fixture.detectChanges();

    const yesRadio = fixture.nativeElement.querySelector(
      '#medical-questionnaire-answer-diabetes-yes'
    ) as HTMLInputElement;
    const noRadio = fixture.nativeElement.querySelector(
      '#medical-questionnaire-answer-diabetes-no'
    ) as HTMLInputElement;
    const unknownRadio = fixture.nativeElement.querySelector(
      '#medical-questionnaire-answer-diabetes-unknown'
    ) as HTMLInputElement;

    expect(yesRadio).not.toBeNull();
    expect(noRadio).not.toBeNull();
    expect(unknownRadio).not.toBeNull();
    expect(unknownRadio.checked).toBeTrue();
    expect(unknownRadio.parentElement?.textContent).toContain('No answer');

    yesRadio.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.answerForm('diabetes').controls.answer.value).toBe('Yes');
    expect(fixture.nativeElement.querySelector('#medical-questionnaire-details-diabetes')).not.toBeNull();
    expect(fixture.componentInstance.capturedQuestionCount).toBe(1);

    noRadio.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.answerForm('diabetes').controls.answer.value).toBe('No');
    expect(fixture.nativeElement.querySelector('#medical-questionnaire-details-diabetes')).toBeNull();
    expect(fixture.componentInstance.capturedQuestionCount).toBe(1);
  });

  it('shows details when an existing answer already has details and counts answered questions', () => {
    const fixture = createComponent(buildQuestionnaire({
      diabetes: {
        answer: 'Unknown',
        details: 'Monitor glucose before long procedures.'
      },
      hypertension: {
        answer: 'Yes',
        details: null
      },
      smokes: {
        answer: 'No',
        details: null
      }
    }));
    fixture.detectChanges();

    const details = fixture.nativeElement.querySelector('#medical-questionnaire-details-diabetes') as HTMLTextAreaElement;

    expect(details).not.toBeNull();
    expect(details.value).toBe('Monitor glucose before long procedures.');
    expect(fixture.componentInstance.capturedQuestionCount).toBe(3);
  });

  it('saves a normalized payload for the fixed questionnaire catalog', () => {
    const fixture = createComponent(buildQuestionnaire());
    const savedPayloads: SaveClinicalMedicalQuestionnaireRequest[] = [];
    fixture.componentInstance.saved.subscribe((payload) => savedPayloads.push(payload));
    fixture.detectChanges();

    fixture.componentInstance.answerForm('currentMedicalTreatment').controls.answer.setValue('Yes');
    fixture.componentInstance.answerForm('currentMedicalTreatment').controls.details.setValue('  Orthodontic follow-up.  ');
    fixture.componentInstance.answerForm('allergyOther').controls.details.setValue('   ');
    fixture.componentInstance.submit();

    expect(savedPayloads).toHaveLength(1);
    expect(savedPayloads[0].answers).toHaveLength(CLINICAL_MEDICAL_QUESTION_KEYS.length);
    expect(savedPayloads[0].answers.find((answer) => answer.questionKey === 'currentMedicalTreatment')).toEqual({
      questionKey: 'currentMedicalTreatment',
      answer: 'Yes',
      details: 'Orthodontic follow-up.'
    });
    expect(savedPayloads[0].answers.find((answer) => answer.questionKey === 'allergyOther')?.details).toBeNull();
  });

  it('handles loading, load error, and save error states', () => {
    const fixture = TestBed.createComponent(ClinicalMedicalQuestionnaireFormComponent);
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading');

    fixture.componentRef.setInput('loading', false);
    fixture.componentRef.setInput('error', 'The medical questionnaire could not be loaded.');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('The medical questionnaire could not be loaded.');

    fixture.componentRef.setInput('error', null);
    fixture.componentRef.setInput('questionnaire', buildQuestionnaire());
    fixture.componentRef.setInput('saveError', 'The medical questionnaire could not be saved.');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('The medical questionnaire could not be saved.');
  });
});

function createComponent(questionnaire: ClinicalMedicalQuestionnaire): ComponentFixture<ClinicalMedicalQuestionnaireFormComponent> {
  const fixture = TestBed.createComponent(ClinicalMedicalQuestionnaireFormComponent);
  fixture.componentRef.setInput('questionnaire', questionnaire);
  fixture.componentRef.setInput('canWrite', true);

  return fixture;
}

function buildQuestionnaire(
  overrides: Partial<Record<ClinicalMedicalQuestionKey, { answer: ClinicalMedicalAnswerValue; details: string | null }>> = {}
): ClinicalMedicalQuestionnaire {
  return {
    clinicalRecordId: 'record-1',
    patientId: 'patient-1',
    answers: CLINICAL_MEDICAL_QUESTION_KEYS.map((questionKey) => ({
      id: null,
      questionKey,
      answer: overrides[questionKey]?.answer ?? 'Unknown',
      details: overrides[questionKey]?.details ?? null,
      updatedAtUtc: null,
      updatedByUserId: null
    }))
  };
}
