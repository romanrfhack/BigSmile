import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/i18n';
import {
  EmptyStateComponent,
  LoadingSkeletonComponent,
  SectionCardComponent,
  StatusBadgeComponent
} from '../../../shared/ui';
import {
  ReminderTemplate,
  ReminderTemplateFormValue,
  ReminderTemplatePreview
} from '../models/scheduling.models';

@Component({
  selector: 'app-reminder-template-manager',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslatePipe,
    EmptyStateComponent,
    LoadingSkeletonComponent,
    SectionCardComponent,
    StatusBadgeComponent
  ],
  template: `
    <app-section-card
      class="template-panel"
      [title]="'Reminder templates' | t"
      [subtitle]="'Templates are internal drafts only. BigSmile does not send messages.' | t">
      <span section-card-actions class="section-label">{{ 'Reminder templates' | t }}</span>

      <div *ngIf="loading" class="loading-stack">
        <app-loading-skeleton
          variant="card"
          [ariaLabel]="'Loading reminder templates.' | t">
        </app-loading-skeleton>
        <app-loading-skeleton
          variant="card"
          [ariaLabel]="'Loading reminder templates.' | t">
        </app-loading-skeleton>
      </div>

      <div *ngIf="!loading && error" class="state-card state-error" role="alert">
        {{ error | t }}
      </div>

      <form *ngIf="canWrite" class="template-form" (ngSubmit)="submit()">
        <label class="control">
          <span>{{ 'Name' | t }}</span>
          <input
            name="template-name"
            maxlength="120"
            [(ngModel)]="name"
            [disabled]="saving" />
        </label>

        <label class="control control-wide">
          <span>{{ 'Body' | t }}</span>
          <textarea
            name="template-body"
            rows="4"
            maxlength="1000"
            [(ngModel)]="body"
            [disabled]="saving"></textarea>
        </label>

        <div *ngIf="formError" class="form-error" role="alert">{{ formError | t }}</div>
        <div *ngIf="submitError" class="form-error" role="alert">{{ submitError | t }}</div>

        <div class="template-actions">
          <button type="submit" class="btn btn-primary" [disabled]="saving">
            {{ (editingTemplateId ? 'Save template' : 'Create template') | t }}
          </button>
          <button
            *ngIf="editingTemplateId"
            type="button"
            class="btn btn-secondary"
            [disabled]="saving"
            (click)="resetForm()">
            {{ 'Cancel' | t }}
          </button>
        </div>
      </form>

      <app-empty-state
        *ngIf="!loading && !error && !templates.length"
        icon="T"
        [title]="'No active reminder templates have been created.' | t"
        [description]="'Templates are internal drafts only. BigSmile does not send messages.' | t">
      </app-empty-state>

      <ol *ngIf="!loading && !error && templates.length" class="template-list">
        <li *ngFor="let template of templates" class="template-item">
          <div class="template-item-head">
            <strong>{{ template.name }}</strong>
            <app-status-badge
              *ngIf="!template.isActive"
              size="sm"
              tone="warning"
              [label]="'Inactive' | t">
            </app-status-badge>
          </div>
          <p>{{ template.body }}</p>

          <div class="template-row-actions" *ngIf="canWrite || previewAppointmentId">
            <button *ngIf="canWrite" type="button" class="btn btn-secondary" (click)="startEdit(template)">
              {{ 'Edit' | t }}
            </button>
            <button
              type="button"
              class="btn btn-secondary"
              [disabled]="!previewAppointmentId || previewing"
              (click)="requestPreview(template.id)">
              {{ 'Preview' | t }}
            </button>
            <button *ngIf="canWrite" type="button" class="btn btn-danger" [disabled]="saving" (click)="deactivateRequested.emit(template.id)">
              {{ 'Deactivate' | t }}
            </button>
          </div>

          <div *ngIf="preview?.templateId === template.id && preview?.appointmentId === previewAppointmentId" class="preview-card">
            <strong>{{ 'Preview' | t }}</strong>
            <p>{{ preview?.renderedBody }}</p>
            <small *ngIf="preview?.unknownPlaceholders?.length">
              {{ 'Unknown placeholders:' | t }} {{ preview?.unknownPlaceholders?.join(', ') }}
            </small>
          </div>
        </li>
      </ol>
    </app-section-card>
  `,
  styles: [`
    .section-label {
      display: inline-flex;
      margin: 0;
      text-transform: uppercase;
      letter-spacing: 0;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.78rem;
      font-weight: 800;
    }

    .template-form {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      margin-top: 1rem;
      border-top: 1px solid var(--bsm-color-border);
      padding-top: 1rem;
    }

    app-empty-state {
      display: block;
      margin-top: 1rem;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: var(--bsm-color-text-brand);
      font-weight: 700;
    }

    .control-wide,
    .form-error,
    .template-actions {
      grid-column: 1 / -1;
    }

    input,
    textarea {
      width: 100%;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.8rem 0.9rem;
      font: inherit;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
      box-sizing: border-box;
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    input:focus,
    textarea:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    textarea {
      resize: vertical;
      min-height: 108px;
    }

    .template-actions,
    .template-row-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .template-row-actions {
      margin-top: 0.85rem;
    }

    .btn {
      border: none;
      border-radius: var(--bsm-radius-pill);
      padding: 0.75rem 1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
      transition:
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard),
        opacity var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .btn-primary {
      background: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .btn-secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-text-brand);
    }

    .btn-danger {
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.65;
      transform: none;
    }

    .btn:hover:not(:disabled) {
      box-shadow: var(--bsm-shadow-sm);
      transform: translateY(-1px);
    }

    .btn:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    .loading-stack,
    .state-card {
      margin-top: 1rem;
    }

    .loading-stack {
      display: grid;
      gap: 0.75rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .state-card {
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-muted);
      padding: 0.9rem 1rem;
    }

    .state-error {
      border: 1px solid var(--bsm-color-danger-soft);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .form-error {
      color: var(--bsm-color-danger);
      font-weight: 700;
    }

    .template-list {
      display: grid;
      gap: 0.75rem;
      margin: 1rem 0 0;
      padding: 0;
      list-style: none;
    }

    .template-item {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      padding: 0.9rem 1rem;
      background: var(--bsm-color-surface);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .template-item:hover {
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-sm);
    }

    .template-item-head {
      display: flex;
      justify-content: space-between;
      gap: 0.75rem;
      align-items: center;
    }

    .template-item strong {
      color: var(--bsm-color-text-brand);
    }

    .template-item p {
      margin: 0.5rem 0 0;
      color: var(--bsm-color-text);
      white-space: pre-wrap;
      word-break: break-word;
    }

    .preview-card {
      margin-top: 0.9rem;
      border-radius: var(--bsm-radius-sm);
      border: 1px solid var(--bsm-color-success-soft);
      background: var(--bsm-color-success-soft);
      padding: 0.85rem;
    }

    .preview-card small {
      display: block;
      margin-top: 0.5rem;
      color: var(--bsm-color-warning);
      font-weight: 700;
    }

    @media (prefers-reduced-motion: reduce) {
      .btn:hover:not(:disabled) {
        transform: none;
      }
    }

    @media (max-width: 720px) {
      .loading-stack,
      .template-form {
        grid-template-columns: 1fr;
      }

      .btn {
        width: 100%;
      }
    }
  `]
})
export class ReminderTemplateManagerComponent {
  @Input() templates: ReminderTemplate[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() submitError: string | null = null;
  @Input() canWrite = false;
  @Input() saving = false;
  @Input() previewAppointmentId: string | null = null;
  @Input() preview: ReminderTemplatePreview | null = null;
  @Input() previewing = false;

  @Output() saved = new EventEmitter<ReminderTemplateFormValue>();
  @Output() deactivateRequested = new EventEmitter<string>();
  @Output() previewRequested = new EventEmitter<{ templateId: string; appointmentId: string }>();

  editingTemplateId: string | null = null;
  name = '';
  body = '';
  formError: string | null = null;

  startEdit(template: ReminderTemplate): void {
    this.editingTemplateId = template.id;
    this.name = template.name;
    this.body = template.body;
    this.formError = null;
  }

  resetForm(): void {
    this.editingTemplateId = null;
    this.name = '';
    this.body = '';
    this.formError = null;
  }

  submit(): void {
    const normalizedName = this.name.trim();
    const normalizedBody = this.body.trim();

    if (!normalizedName) {
      this.formError = 'Name is required.';
      return;
    }

    if (normalizedName.length > 120) {
      this.formError = 'Name must be 120 characters or fewer.';
      return;
    }

    if (!normalizedBody) {
      this.formError = 'Body is required.';
      return;
    }

    if (normalizedBody.length > 1000) {
      this.formError = 'Body must be 1000 characters or fewer.';
      return;
    }

    this.formError = null;
    this.saved.emit({
      id: this.editingTemplateId,
      name: normalizedName,
      body: normalizedBody
    });

    if (!this.editingTemplateId) {
      this.resetForm();
    }
  }

  requestPreview(templateId: string): void {
    if (!this.previewAppointmentId) {
      return;
    }

    this.previewRequested.emit({
      templateId,
      appointmentId: this.previewAppointmentId
    });
  }
}
