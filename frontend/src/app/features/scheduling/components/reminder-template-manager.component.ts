import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ReminderTemplate,
  ReminderTemplateFormValue,
  ReminderTemplatePreview
} from '../models/scheduling.models';

@Component({
  selector: 'app-reminder-template-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="template-panel">
      <header class="template-head">
        <div>
          <p class="section-label">Reminder templates</p>
          <h3>Reminder templates</h3>
          <p class="manual-note">Templates are internal drafts only. BigSmile does not send messages.</p>
        </div>
      </header>

      <div *ngIf="loading" class="state-card">
        Loading reminder templates.
      </div>

      <div *ngIf="!loading && error" class="state-card state-error">
        {{ error }}
      </div>

      <form *ngIf="canWrite" class="template-form" (ngSubmit)="submit()">
        <label class="control">
          <span>Name</span>
          <input
            name="template-name"
            maxlength="120"
            [(ngModel)]="name"
            [disabled]="saving" />
        </label>

        <label class="control control-wide">
          <span>Body</span>
          <textarea
            name="template-body"
            rows="4"
            maxlength="1000"
            [(ngModel)]="body"
            [disabled]="saving"></textarea>
        </label>

        <div *ngIf="formError" class="form-error">{{ formError }}</div>
        <div *ngIf="submitError" class="form-error">{{ submitError }}</div>

        <div class="template-actions">
          <button type="submit" class="btn btn-primary" [disabled]="saving">
            {{ editingTemplateId ? 'Save template' : 'Create template' }}
          </button>
          <button
            *ngIf="editingTemplateId"
            type="button"
            class="btn btn-secondary"
            [disabled]="saving"
            (click)="resetForm()">
            Cancel
          </button>
        </div>
      </form>

      <div *ngIf="!loading && !error && !templates.length" class="state-card">
        No active reminder templates have been created.
      </div>

      <ol *ngIf="!loading && !error && templates.length" class="template-list">
        <li *ngFor="let template of templates" class="template-item">
          <div>
            <strong>{{ template.name }}</strong>
            <span *ngIf="!template.isActive">Inactive</span>
          </div>
          <p>{{ template.body }}</p>

          <div class="template-row-actions" *ngIf="canWrite || previewAppointmentId">
            <button *ngIf="canWrite" type="button" class="btn btn-secondary" (click)="startEdit(template)">
              Edit
            </button>
            <button
              type="button"
              class="btn btn-secondary"
              [disabled]="!previewAppointmentId || previewing"
              (click)="requestPreview(template.id)">
              Preview
            </button>
            <button *ngIf="canWrite" type="button" class="btn btn-danger" [disabled]="saving" (click)="deactivateRequested.emit(template.id)">
              Deactivate
            </button>
          </div>

          <div *ngIf="preview?.templateId === template.id && preview?.appointmentId === previewAppointmentId" class="preview-card">
            <strong>Preview</strong>
            <p>{{ preview?.renderedBody }}</p>
            <small *ngIf="preview?.unknownPlaceholders?.length">
              Unknown placeholders: {{ preview?.unknownPlaceholders?.join(', ') }}
            </small>
          </div>
        </li>
      </ol>
    </section>
  `,
  styles: [`
    .template-panel {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: #ffffff;
      padding: 1.25rem;
      box-shadow: 0 18px 30px rgba(20, 48, 79, 0.08);
    }

    .template-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .section-label {
      margin: 0 0 0.35rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.78rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: #16324f;
      font-size: 1.2rem;
    }

    .manual-note {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      font-weight: 700;
    }

    .template-form {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      margin-top: 1rem;
      border-top: 1px solid #dce6ef;
      padding-top: 1rem;
    }

    .control {
      display: grid;
      gap: 0.4rem;
      color: #16324f;
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
      border: 1px solid #c8d4df;
      border-radius: 14px;
      padding: 0.8rem 0.9rem;
      font: inherit;
      background: #ffffff;
      box-sizing: border-box;
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
      border-radius: 999px;
      padding: 0.75rem 1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: #0a5bb5;
      color: #ffffff;
    }

    .btn-secondary {
      background: #e5edf5;
      color: #17304d;
    }

    .btn-danger {
      background: #fde3e3;
      color: #9b2d30;
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.65;
    }

    .state-card {
      margin-top: 1rem;
      border-radius: 14px;
      background: #f5f9fc;
      color: #5b6e84;
      padding: 0.9rem 1rem;
    }

    .state-error {
      border: 1px solid #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }

    .form-error {
      color: #8c2525;
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
      border: 1px solid #dce6ef;
      border-radius: 14px;
      padding: 0.9rem 1rem;
      background: #f8fbfd;
    }

    .template-item div:first-child {
      display: flex;
      justify-content: space-between;
      gap: 0.75rem;
      align-items: center;
    }

    .template-item strong {
      color: #16324f;
    }

    .template-item span {
      color: #8b4f0f;
      font-weight: 800;
    }

    .template-item p {
      margin: 0.5rem 0 0;
      color: #42546a;
      white-space: pre-wrap;
      word-break: break-word;
    }

    .preview-card {
      margin-top: 0.9rem;
      border-radius: 14px;
      border: 1px solid #c9dfd2;
      background: #f2fbf5;
      padding: 0.85rem;
    }

    .preview-card small {
      display: block;
      margin-top: 0.5rem;
      color: #8b4f0f;
      font-weight: 700;
    }

    @media (max-width: 720px) {
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
