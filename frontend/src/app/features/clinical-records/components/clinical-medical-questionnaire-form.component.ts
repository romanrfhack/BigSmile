import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  SectionCardComponent,
  StatusBadgeComponent,
  StickyActionBarComponent
} from '../../../shared/ui';
import {
  CLINICAL_MEDICAL_QUESTIONNAIRE_GROUPS,
  CLINICAL_MEDICAL_QUESTION_KEYS
} from '../models/clinical-medical-questionnaire.catalog';
import {
  ClinicalMedicalAnswer,
  ClinicalMedicalAnswerValue,
  ClinicalMedicalQuestionKey,
  ClinicalMedicalQuestionnaire,
  SaveClinicalMedicalQuestionnaireRequest
} from '../models/clinical-record.models';

type MedicalQuestionnaireAnswerForm = FormGroup<{
  questionKey: FormControl<ClinicalMedicalQuestionKey>;
  answer: FormControl<ClinicalMedicalAnswerValue>;
  details: FormControl<string>;
}>;

@Component({
  selector: 'app-clinical-medical-questionnaire-form',
  standalone: true,
  imports: [
    CommonModule,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    ReactiveFormsModule,
    SectionCardComponent,
    StatusBadgeComponent,
    StickyActionBarComponent,
    TranslatePipe
  ],
  template: `
    <app-section-card
      [title]="'Medical history' | t"
      [subtitle]="'Fixed structured medical questionnaire for this existing clinical record.' | t"
      variant="elevated">
      <div section-card-actions class="questionnaire-status">
        <span
          *ngIf="!loading && !error && questionnaire"
          class="questionnaire-progress"
          role="status"
          aria-live="polite">
          <span class="visually-hidden">{{ 'Structured answers' | t }}:</span>
          {{ capturedQuestionCount }} / {{ totalQuestionCount }}
        </span>
        <app-status-badge
          *ngIf="loading"
          tone="info"
          [label]="'Loading' | t">
        </app-status-badge>
        <app-status-badge
          *ngIf="!loading && error"
          tone="danger"
          [label]="'Error' | t">
        </app-status-badge>
        <app-status-badge
          *ngIf="!loading && !error && questionnaire && isQuestionnaireEmpty()"
          tone="neutral"
          [label]="'No answers captured' | t">
        </app-status-badge>
        <app-status-badge
          *ngIf="!loading && !error && questionnaire && !isQuestionnaireEmpty()"
          tone="primary"
          [label]="'Structured answers' | t">
        </app-status-badge>
      </div>

      <div class="questionnaire-shell">
        <ng-container *ngIf="loading">
          <app-loading-skeleton
            *ngFor="let item of loadingCards"
            variant="card"
            [ariaLabel]="'Loading medical questionnaire...' | t">
          </app-loading-skeleton>
        </ng-container>

        <div *ngIf="!loading && error" class="questionnaire-error" role="alert">
          <p>{{ error | t }}</p>
          <button type="button" class="btn btn-secondary" (click)="retryRequested.emit()">
            {{ 'Retry' | t }}
          </button>
        </div>

        <app-empty-state
          *ngIf="!loading && !error && (missing || !questionnaire)"
          [title]="'Medical questionnaire unavailable' | t"
          [description]="'The questionnaire is only available after the clinical record exists. It is not auto-created.' | t"
          icon="?"
          ariaLive="polite">
          <button empty-state-action type="button" class="btn btn-secondary" (click)="retryRequested.emit()">
            {{ 'Retry' | t }}
          </button>
        </app-empty-state>

        <form
          *ngIf="!loading && !error && questionnaire"
          class="questionnaire-form"
          [formGroup]="form"
          (ngSubmit)="submit()">
          <p class="scope-note">
            {{ 'Questionnaire answers do not automatically update current allergies, patient alerts, timeline, or snapshot history.' | t }}
          </p>

          <p *ngIf="isQuestionnaireEmpty()" class="empty-note">
            {{ 'No medical questionnaire answers have been captured yet.' | t }}
          </p>

          <p *ngIf="saveError" class="save-error" role="alert">
            {{ saveError | t }}
          </p>

          <section
            *ngFor="let group of groups"
            class="question-group"
            [attr.aria-labelledby]="'question-group-' + group.id">
            <h3 [id]="'question-group-' + group.id">{{ group.titleKey | t }}</h3>

            <div class="question-grid">
              <article
                *ngFor="let question of group.questions"
                class="question-card"
                [formGroup]="answerForm(question.questionKey)">
                <div class="question-main">
                  <p [id]="questionLabelId(question.questionKey)">
                    {{ question.labelKey | t }}
                  </p>

                  <div
                    class="answer-options"
                    role="radiogroup"
                    [attr.aria-labelledby]="questionLabelId(question.questionKey)">
                    <label
                      *ngFor="let answerValue of answerValues"
                      class="answer-option"
                      [for]="answerOptionId(question.questionKey, answerValue)">
                      <input
                        type="radio"
                        [id]="answerOptionId(question.questionKey, answerValue)"
                        [name]="answerControlName(question.questionKey)"
                        [formControl]="answerForm(question.questionKey).controls.answer"
                        [value]="answerValue">
                      <span>{{ answerLabelKey(answerValue) | t }}</span>
                    </label>
                  </div>
                </div>

                <label
                  *ngIf="shouldShowDetails(question.questionKey)"
                  class="details-field"
                  [for]="detailsControlId(question.questionKey)">
                  <span>{{ 'Details' | t }}</span>
                  <textarea
                    [id]="detailsControlId(question.questionKey)"
                    rows="3"
                    formControlName="details"
                    [placeholder]="'Optional clinical context' | t">
                  </textarea>
                  <small *ngIf="hasDetailsLengthError(question.questionKey)">
                    {{ 'Details must be 500 characters or fewer.' | t }}
                  </small>
                </label>
              </article>
            </div>
          </section>

          <p *ngIf="!canWrite" class="read-only-note">
            {{ 'Your current session can read clinical data but cannot update the questionnaire.' | t }}
          </p>

          <app-sticky-action-bar *ngIf="canWrite" [ariaLabel]="'Medical questionnaire actions' | t">
            <span class="form-status">
              {{ (form.dirty ? 'Unsaved changes' : 'No unsaved changes') | t }}
            </span>
            <button type="button" class="btn btn-secondary" (click)="cancel()" [disabled]="saving">
              {{ 'Cancel' | t }}
            </button>
            <button type="submit" class="btn btn-primary" [disabled]="saving || form.invalid">
              {{ (saving ? 'Saving...' : 'Save medical history') | t }}
            </button>
          </app-sticky-action-bar>
        </form>
      </div>
    </app-section-card>
  `,
  styles: [`
    :host {
      display: block;
    }

    .questionnaire-shell,
    .questionnaire-form,
    .question-group,
    .question-card {
      display: grid;
      gap: 1rem;
    }

    .questionnaire-status {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
      justify-content: flex-end;
    }

    .visually-hidden {
      position: absolute;
      inline-size: 1px;
      block-size: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border: 0;
    }

    .questionnaire-progress {
      display: inline-flex;
      align-items: center;
      min-height: 1.75rem;
      border: 1px solid var(--bsm-color-primary-soft);
      border-radius: var(--bsm-radius-pill);
      padding: 0.25rem 0.65rem;
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
      font-size: 0.8rem;
      font-weight: 800;
    }

    .questionnaire-error,
    .scope-note,
    .empty-note,
    .save-error,
    .read-only-note {
      margin: 0;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      padding: 0.85rem 1rem;
      background: var(--bsm-color-neutral-soft);
      color: var(--bsm-color-text-muted);
      line-height: 1.45;
    }

    .questionnaire-error {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
      border-color: var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .questionnaire-error p {
      margin: 0;
    }

    .save-error {
      border-color: var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .empty-note {
      border-color: var(--bsm-color-accent-soft);
      background: var(--bsm-color-accent-soft);
      color: var(--bsm-color-accent-dark);
      font-weight: 700;
    }

    .read-only-note {
      border-color: var(--bsm-color-warning-soft);
      background: var(--bsm-color-warning-soft);
      color: var(--bsm-color-warning);
      font-weight: 700;
    }

    .question-group {
      padding-top: 0.25rem;
    }

    .question-group h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: 1rem;
      line-height: 1.25;
    }

    .question-grid {
      display: grid;
      gap: 0.75rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .question-card {
      align-content: start;
      min-width: 0;
      padding: 0.9rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
      box-shadow: var(--bsm-shadow-sm);
    }

    .question-main,
    .details-field {
      display: grid;
      gap: 0.5rem;
    }

    .question-main p,
    .details-field span {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-weight: 800;
      line-height: 1.3;
    }

    .answer-options {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 0.35rem;
    }

    .answer-option {
      position: relative;
      min-width: 0;
    }

    .answer-option input {
      position: absolute;
      inline-size: 1px;
      block-size: 1px;
      margin: 0;
      opacity: 0;
      pointer-events: none;
    }

    .answer-option span {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 2.35rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-pill);
      padding: 0.5rem 0.6rem;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text-muted);
      font-size: 0.82rem;
      font-weight: 800;
      line-height: 1.1;
      text-align: center;
      cursor: pointer;
      transition:
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        color var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .answer-option input:checked + span {
      border-color: var(--bsm-color-primary);
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .answer-option input:focus-visible + span,
    textarea:focus-visible,
    .btn:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    .answer-option input:not(:disabled) + span:hover {
      border-color: var(--bsm-color-primary-soft);
      transform: translateY(-1px);
    }

    .answer-option input:disabled + span {
      cursor: not-allowed;
      opacity: 0.68;
      transform: none;
    }

    textarea {
      width: 100%;
      min-height: 5.5rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      padding: 0.72rem 0.8rem;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text-brand);
      font: inherit;
      resize: vertical;
    }

    small {
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .form-status {
      margin-inline-end: auto;
      color: var(--bsm-color-text-muted);
      font-weight: 800;
    }

    .btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 7.5rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-pill);
      padding: 0.75rem 1rem;
      font: inherit;
      font-weight: 800;
      line-height: 1.2;
      cursor: pointer;
      transition:
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        color var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .btn-primary {
      border-color: var(--bsm-color-primary);
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .btn-secondary {
      border-color: var(--bsm-color-primary-soft);
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .btn:not(:disabled):hover {
      box-shadow: var(--bsm-shadow-sm);
      transform: translateY(-1px);
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.68;
    }

    @media (prefers-reduced-motion: reduce) {
      .answer-option span,
      .btn {
        transition: none;
      }

      .answer-option input:not(:disabled) + span:hover,
      .btn:not(:disabled):hover {
        transform: none;
      }
    }

    @media (max-width: 860px) {
      .question-grid {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 640px) {
      .questionnaire-error {
        align-items: stretch;
        flex-direction: column;
      }

      .form-status,
      .btn {
        width: 100%;
      }
    }
  `]
})
export class ClinicalMedicalQuestionnaireFormComponent implements OnChanges {
  private readonly formBuilder = inject(FormBuilder);

  @Input() questionnaire: ClinicalMedicalQuestionnaire | null = null;
  @Input() loading = false;
  @Input() missing = false;
  @Input() error: string | null = null;
  @Input() saveError: string | null = null;
  @Input() saving = false;
  @Input() canWrite = false;

  @Output() saved = new EventEmitter<SaveClinicalMedicalQuestionnaireRequest>();
  @Output() retryRequested = new EventEmitter<void>();

  readonly groups = CLINICAL_MEDICAL_QUESTIONNAIRE_GROUPS;
  readonly answerValues: readonly ClinicalMedicalAnswerValue[] = ['Yes', 'No', 'Unknown'];
  readonly loadingCards = [0, 1, 2];
  readonly totalQuestionCount = CLINICAL_MEDICAL_QUESTION_KEYS.length;
  readonly answers = this.formBuilder.array<MedicalQuestionnaireAnswerForm>([]);
  readonly form = this.formBuilder.group({ answers: this.answers });

  get capturedQuestionCount(): number {
    return this.answers.controls.filter((answerForm) =>
      answerForm.controls.answer.value !== 'Unknown' ||
      answerForm.controls.details.value.trim().length > 0).length;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['questionnaire']) {
      this.resetForm();
    }

    if (changes['canWrite'] || changes['saving'] || changes['questionnaire']) {
      this.syncFormState();
    }
  }

  answerForm(questionKey: ClinicalMedicalQuestionKey): MedicalQuestionnaireAnswerForm {
    const answer = this.findAnswerForm(questionKey);
    if (!answer) {
      throw new Error(`Missing medical questionnaire control for ${questionKey}.`);
    }

    return answer;
  }

  answerControlName(questionKey: ClinicalMedicalQuestionKey): string {
    return `medical-questionnaire-answer-${questionKey}`;
  }

  answerOptionId(questionKey: ClinicalMedicalQuestionKey, answerValue: ClinicalMedicalAnswerValue): string {
    return `${this.answerControlName(questionKey)}-${answerValue.toLowerCase()}`;
  }

  answerLabelKey(answerValue: ClinicalMedicalAnswerValue): string {
    return answerValue === 'Unknown' ? 'No answer' : answerValue;
  }

  questionLabelId(questionKey: ClinicalMedicalQuestionKey): string {
    return `medical-questionnaire-label-${questionKey}`;
  }

  detailsControlId(questionKey: ClinicalMedicalQuestionKey): string {
    return `medical-questionnaire-details-${questionKey}`;
  }

  shouldShowDetails(questionKey: ClinicalMedicalQuestionKey): boolean {
    const answer = this.answerForm(questionKey).controls.answer.value;
    const details = this.answerForm(questionKey).controls.details.value.trim();

    return answer === 'Yes' || details.length > 0;
  }

  hasDetailsLengthError(questionKey: ClinicalMedicalQuestionKey): boolean {
    const control = this.answerForm(questionKey).controls.details;

    return control.hasError('maxlength') && (control.dirty || control.touched);
  }

  isQuestionnaireEmpty(): boolean {
    return this.answers.controls.every((answerForm) =>
      answerForm.controls.answer.value === 'Unknown' &&
      answerForm.controls.details.value.trim().length === 0);
  }

  cancel(): void {
    this.resetForm();
    this.syncFormState();
  }

  submit(): void {
    if (!this.canWrite || this.saving) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saved.emit(this.buildPayload());
  }

  private resetForm(): void {
    this.answers.clear();

    const answersByQuestionKey = new Map<ClinicalMedicalQuestionKey, ClinicalMedicalAnswer>();
    for (const answer of this.questionnaire?.answers ?? []) {
      answersByQuestionKey.set(answer.questionKey, answer);
    }

    for (const questionKey of CLINICAL_MEDICAL_QUESTION_KEYS) {
      const answer = answersByQuestionKey.get(questionKey);
      this.answers.push(this.createAnswerGroup(
        questionKey,
        answer?.answer ?? 'Unknown',
        answer?.details ?? ''
      ));
    }

    this.form.markAsPristine();
    this.form.markAsUntouched();
  }

  private createAnswerGroup(
    questionKey: ClinicalMedicalQuestionKey,
    answer: ClinicalMedicalAnswerValue,
    details: string
  ): MedicalQuestionnaireAnswerForm {
    return this.formBuilder.nonNullable.group({
      questionKey: this.formBuilder.nonNullable.control(questionKey),
      answer: this.formBuilder.nonNullable.control(answer),
      details: this.formBuilder.nonNullable.control(details, [Validators.maxLength(500)])
    });
  }

  private buildPayload(): SaveClinicalMedicalQuestionnaireRequest {
    return {
      answers: this.answers.controls.map((answerForm) => ({
        questionKey: answerForm.controls.questionKey.value,
        answer: answerForm.controls.answer.value,
        details: this.normalizeOptional(answerForm.controls.details.value)
      }))
    };
  }

  private findAnswerForm(questionKey: ClinicalMedicalQuestionKey): MedicalQuestionnaireAnswerForm | undefined {
    return this.answers.controls.find((answerForm) => answerForm.controls.questionKey.value === questionKey);
  }

  private normalizeOptional(value: string): string | null {
    const normalized = value.trim();

    return normalized ? normalized : null;
  }

  private syncFormState(): void {
    if (!this.canWrite || this.saving) {
      this.form.disable({ emitEvent: false });
      return;
    }

    this.form.enable({ emitEvent: false });
  }
}
