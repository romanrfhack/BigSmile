import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import {
  MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS,
  MEDICAL_QUESTIONNAIRE_GROUPS,
  MEDICAL_QUESTIONNAIRE_KEYS,
  MedicalQuestionnaireAnswerValue,
  MedicalQuestionnaireQuestionKey
} from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';
import {
  PATIENT_INTAKE_FIELD_LIMITS,
  PatientIntakeDraft,
  PatientIntakeMedicalAnswerFormValue,
  PatientIntakeSaveOutcome,
  toPatientIntakeMedicalFormValue
} from '../models/patient-intake.models';

type PatientIntakeMedicalAnswerForm = FormGroup<{
  questionKey: FormControl<MedicalQuestionnaireQuestionKey>;
  answer: FormControl<MedicalQuestionnaireAnswerValue>;
  details: FormControl<string>;
}>;

@Component({
  selector: 'app-patient-intake-medical-questionnaire',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe],
  template: `
    <form class="medical-form" [formGroup]="form" (ngSubmit)="submit()" novalidate>
      <section class="medical-intro" aria-labelledby="patient-medical-history-heading">
        <div>
          <p>{{ 'Patient information' | t }}</p>
          <h2 id="patient-medical-history-heading">{{ 'Medical history' | t }}</h2>
          <span>{{ 'These answers remain part of your private draft until the clinic reviews them.' | t }}</span>
        </div>
        <div class="medical-progress" role="status" aria-live="polite">
          <strong>{{ capturedQuestionCount }} / {{ totalQuestionCount }}</strong>
          <span>{{ 'Answered questions' | t }}</span>
        </div>
      </section>

      @if (saveError) {
        <div class="patient-alert patient-alert--error" role="alert" aria-live="assertive">
          {{ saveError | t }}
        </div>
      }

      @if (saveOutcome === 'saved') {
        <div class="medical-save-message medical-save-message--success" role="status" aria-live="polite">
          {{ 'Your draft was saved.' | t }}
        </div>
      } @else if (saveOutcome === 'unchanged') {
        <div class="medical-save-message" role="status" aria-live="polite">
          {{ 'No changes were detected. The existing revision was preserved.' | t }}
        </div>
      }

      @for (group of groups; track group.id) {
        <section class="medical-group" [attr.aria-labelledby]="'patient-medical-group-' + group.id">
          <header>
            <h3 [id]="'patient-medical-group-' + group.id">{{ group.titleKey | t }}</h3>
          </header>

          <div class="medical-grid">
            @for (question of group.questions; track question.questionKey) {
              <article class="medical-card" [formGroup]="answerForm(question.questionKey)">
                <p [id]="questionLabelId(question.questionKey)">{{ question.labelKey | t }}</p>

                <div
                  class="medical-options"
                  role="radiogroup"
                  [attr.aria-labelledby]="questionLabelId(question.questionKey)">
                  @for (option of answerOptions; track option.value) {
                    <label class="medical-option" [for]="answerOptionId(question.questionKey, option.value)">
                      <input
                        type="radio"
                        [id]="answerOptionId(question.questionKey, option.value)"
                        [name]="answerControlName(question.questionKey)"
                        [formControl]="answerForm(question.questionKey).controls.answer"
                        [value]="option.value" />
                      <span>{{ option.labelKey | t }}</span>
                    </label>
                  }
                </div>

                @if (shouldShowDetails(question.questionKey)) {
                  <label class="medical-details" [for]="detailsControlId(question.questionKey)">
                    <span>{{ 'Details' | t }}</span>
                    <textarea
                      [id]="detailsControlId(question.questionKey)"
                      rows="3"
                      formControlName="details"
                      [attr.maxlength]="limits.medicalDetails"
                      [placeholder]="'Optional details' | t"></textarea>
                    @if (hasDetailsLengthError(question.questionKey)) {
                      <small>{{ 'Details must be 500 characters or fewer.' | t }}</small>
                    }
                  </label>
                }
              </article>
            }
          </div>
        </section>
      }

      <div class="medical-actions">
        <p>{{ 'Medical answers are saved together with the current personal information shown above.' | t }}</p>
        <button
          type="submit"
          class="patient-button patient-button--primary"
          [disabled]="saving || form.invalid">
          {{ (saving ? 'Saving medical answers...' : 'Save medical answers') | t }}
        </button>
      </div>
    </form>
  `,
  styleUrl: './patient-intake-medical-questionnaire.component.scss'
})
export class PatientIntakeMedicalQuestionnaireComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input({ required: true }) intake!: PatientIntakeDraft;
  @Input() saving = false;
  @Input() saveOutcome: PatientIntakeSaveOutcome = null;
  @Input() saveError: string | null = null;
  @Output() readonly saveRequested = new EventEmitter<PatientIntakeMedicalAnswerFormValue[]>();

  readonly groups = MEDICAL_QUESTIONNAIRE_GROUPS;
  readonly answerOptions = MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS;
  readonly limits = PATIENT_INTAKE_FIELD_LIMITS;
  readonly totalQuestionCount = MEDICAL_QUESTIONNAIRE_KEYS.length;
  readonly answers = this.formBuilder.array<PatientIntakeMedicalAnswerForm>([]);
  readonly form = this.formBuilder.group({ answers: this.answers });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['intake'] && this.intake) {
      this.resetForm();
    }
  }

  get capturedQuestionCount(): number {
    return this.answers.controls.filter(control => control.controls.answer.value !== 'Unknown').length;
  }

  currentValue(): PatientIntakeMedicalAnswerFormValue[] {
    return this.answers.controls.map(control => ({
      questionKey: control.controls.questionKey.value,
      answer: control.controls.answer.value,
      details: control.controls.details.value
    }));
  }

  answerForm(questionKey: MedicalQuestionnaireQuestionKey): PatientIntakeMedicalAnswerForm {
    const control = this.answers.controls.find(
      answer => answer.controls.questionKey.value === questionKey);
    if (!control) {
      throw new Error(`Missing patient intake medical control for ${questionKey}.`);
    }
    return control;
  }

  answerControlName(questionKey: MedicalQuestionnaireQuestionKey): string {
    return `patient-intake-medical-answer-${questionKey}`;
  }

  answerOptionId(
    questionKey: MedicalQuestionnaireQuestionKey,
    value: MedicalQuestionnaireAnswerValue
  ): string {
    return `${this.answerControlName(questionKey)}-${value.toLowerCase()}`;
  }

  questionLabelId(questionKey: MedicalQuestionnaireQuestionKey): string {
    return `patient-intake-medical-label-${questionKey}`;
  }

  detailsControlId(questionKey: MedicalQuestionnaireQuestionKey): string {
    return `patient-intake-medical-details-${questionKey}`;
  }

  shouldShowDetails(questionKey: MedicalQuestionnaireQuestionKey): boolean {
    const answer = this.answerForm(questionKey);
    return answer.controls.answer.value === 'Yes' || answer.controls.details.value.trim().length > 0;
  }

  hasDetailsLengthError(questionKey: MedicalQuestionnaireQuestionKey): boolean {
    const control = this.answerForm(questionKey).controls.details;
    return control.hasError('maxlength') && (control.dirty || control.touched);
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.saving || this.form.invalid || this.answers.length !== MEDICAL_QUESTIONNAIRE_KEYS.length) {
      return;
    }
    this.saveRequested.emit(this.currentValue());
  }

  private resetForm(): void {
    this.answers.clear();
    for (const answer of toPatientIntakeMedicalFormValue(this.intake)) {
      this.answers.push(this.formBuilder.nonNullable.group({
        questionKey: this.formBuilder.nonNullable.control(answer.questionKey),
        answer: this.formBuilder.nonNullable.control(answer.answer),
        details: this.formBuilder.nonNullable.control(answer.details, [
          Validators.maxLength(this.limits.medicalDetails)
        ])
      }));
    }
    this.form.markAsPristine();
    this.form.markAsUntouched();
  }
}
