import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  LoadingSkeletonComponent,
  PageHeaderComponent,
  SectionCardComponent,
  StatusBadgeComponent
} from '../../../shared/ui';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { ClinicalBackgroundFormComponent } from '../components/clinical-background-form.component';
import { ClinicalDiagnosisCreateFormComponent } from '../components/clinical-diagnosis-create-form.component';
import { ClinicalDiagnosesListComponent } from '../components/clinical-diagnoses-list.component';
import { ClinicalMedicalQuestionnaireFormComponent } from '../components/clinical-medical-questionnaire-form.component';
import { ClinicalNoteCreateFormComponent } from '../components/clinical-note-create-form.component';
import { ClinicalNotesListComponent } from '../components/clinical-notes-list.component';
import { ClinicalRecordEmptyStateComponent } from '../components/clinical-record-empty-state.component';
import { ClinicalSnapshotHistoryListComponent } from '../components/clinical-snapshot-history-list.component';
import { ClinicalTimelineListComponent } from '../components/clinical-timeline-list.component';
import { ClinicalRecordsFacade } from '../facades/clinical-records.facade';
import {
  AddClinicalDiagnosisRequest,
  AddClinicalNoteRequest,
  SaveClinicalMedicalQuestionnaireRequest,
  SaveClinicalRecordSnapshotRequest
} from '../models/clinical-record.models';

@Component({
  selector: 'app-clinical-record-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ClinicalRecordEmptyStateComponent,
    ClinicalBackgroundFormComponent,
    ClinicalDiagnosisCreateFormComponent,
    ClinicalDiagnosesListComponent,
    ClinicalMedicalQuestionnaireFormComponent,
    ClinicalNoteCreateFormComponent,
    ClinicalNotesListComponent,
    ClinicalSnapshotHistoryListComponent,
    ClinicalTimelineListComponent,
    LoadingSkeletonComponent,
    LocalizedDatePipe,
    PageHeaderComponent,
    SectionCardComponent,
    StatusBadgeComponent,
    TranslatePipe
  ],
  template: `
    <section class="clinical-record-page">
      <app-page-header
        [eyebrow]="('Release' | t) + ' 3.5 / ' + ('Clinical record' | t)"
        [title]="'Clinical record' | t"
        [subtitle]="'Structured clinical record for {patientDisplayName} with explicit creation, base snapshot, fixed medical questionnaire, current allergies, diagnoses, append-only notes, and separated history sections.' | t:{ patientDisplayName }">
        <a *ngIf="patientId" page-header-actions [routerLink]="['/patients', patientId]" class="clinical-action clinical-action--secondary">
          {{ 'Back to patient' | t }}
        </a>
      </app-page-header>

      <app-section-card
        *ngIf="patientsFacade.loadingPatient() || clinicalRecordsFacade.loadingRecord()"
        [title]="'Loading clinical record...' | t"
        variant="elevated">
        <div class="loading-grid">
          <app-loading-skeleton
            *ngFor="let item of loadingCards"
            variant="card"
            [ariaLabel]="'Loading clinical record...' | t">
          </app-loading-skeleton>
        </div>
      </app-section-card>

      <div *ngIf="patientsFacade.detailError()" class="clinical-error" role="alert">
        {{ patientsFacade.detailError() | t }}
      </div>

      <div *ngIf="clinicalRecordsFacade.recordError()" class="clinical-error" role="alert">
        {{ clinicalRecordsFacade.recordError() | t }}
      </div>

      <section
        *ngIf="patientsFacade.currentPatient() as patient"
        class="patient-context-strip"
        [attr.aria-label]="'Patient demographics' | t">
        <article class="patient-context-item patient-context-item--wide">
          <span>{{ 'Name' | t }}</span>
          <strong>{{ patient.fullName }}</strong>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Date of birth' | t }}</span>
          <strong>{{ patient.dateOfBirth | bsDate: 'longDate' }}</strong>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Sex' | t }}</span>
          <strong>{{ (patient.sex || 'Unspecified') | t }}</strong>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Occupation' | t }}</span>
          <strong>{{ patient.occupation || ('Not provided' | t) }}</strong>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Marital status' | t }}</span>
          <strong>{{ (patient.maritalStatus || 'Unspecified') | t }}</strong>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Referred by' | t }}</span>
          <strong>{{ patient.referredBy || ('Not provided' | t) }}</strong>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Contact' | t }}</span>
          <strong>{{ patient.primaryPhone || patient.email || ('No contact data' | t) }}</strong>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Current status' | t }}</span>
          <app-status-badge
            [tone]="patient.isActive ? 'success' : 'neutral'"
            [label]="(patient.isActive ? 'Active' : 'Inactive') | t">
          </app-status-badge>
        </article>
        <article class="patient-context-item">
          <span>{{ 'Clinical alerts' | t }}</span>
          <app-status-badge
            [tone]="patient.hasClinicalAlerts ? 'warning' : 'neutral'"
            [label]="(patient.hasClinicalAlerts ? 'Clinical alerts' : 'No') | t">
          </app-status-badge>
        </article>
      </section>

      <app-clinical-record-empty-state
        *ngIf="!clinicalRecordsFacade.loadingRecord() && clinicalRecordsFacade.recordMissing() && !clinicalRecordsFacade.recordError()"
        [canWrite]="canWrite"
        (createRequested)="startCreate()">
      </app-clinical-record-empty-state>

      <app-clinical-background-form
        *ngIf="clinicalRecordsFacade.recordMissing() && creatingRecord"
        [mode]="'create'"
        [saving]="savingSnapshot"
        [error]="snapshotError"
        (saved)="saveSnapshot($event)"
        (cancelled)="cancelCreate()">
      </app-clinical-background-form>

      <article *ngIf="clinicalRecordsFacade.currentRecord() as record" class="record-shell">
        <app-section-card [title]="'Clinical record audit' | t" variant="compact">
          <dl class="record-meta">
            <div>
              <dt>{{ 'Created' | t }}</dt>
              <dd>{{ record.createdAtUtc | bsDate: 'medium' }}</dd>
            </div>
            <div>
              <dt>{{ 'Created by' | t }}</dt>
              <dd>{{ record.createdByUserId }}</dd>
            </div>
            <div>
              <dt>{{ 'Last updated' | t }}</dt>
              <dd>{{ record.lastUpdatedAtUtc | bsDate: 'medium' }}</dd>
            </div>
            <div>
              <dt>{{ 'Updated by' | t }}</dt>
              <dd>{{ record.lastUpdatedByUserId }}</dd>
            </div>
          </dl>
        </app-section-card>

        <app-clinical-medical-questionnaire-form
          [questionnaire]="clinicalRecordsFacade.currentQuestionnaire()"
          [loading]="clinicalRecordsFacade.loadingQuestionnaire()"
          [missing]="clinicalRecordsFacade.questionnaireMissing()"
          [error]="clinicalRecordsFacade.questionnaireError()"
          [saveError]="questionnaireSaveError"
          [saving]="savingQuestionnaire"
          [canWrite]="canWrite"
          (saved)="saveQuestionnaire($event)"
          (retryRequested)="reloadQuestionnaire()">
        </app-clinical-medical-questionnaire-form>

        <app-clinical-background-form
          [mode]="'edit'"
          [initialRecord]="record"
          [saving]="savingSnapshot"
          [error]="snapshotError"
          (saved)="saveSnapshot($event)">
        </app-clinical-background-form>

        <section class="snapshot-history-shell">
          <div class="notes-head">
            <div>
              <p class="eyebrow">{{ 'Snapshot history' | t }}</p>
              <h3>{{ 'Base snapshot changes only' | t }}</h3>
              <p class="section-copy">{{ 'Snapshot history and the Release 3.3 clinical timeline stay as separate sections in this slice.' | t }}</p>
            </div>
          </div>

          <app-clinical-snapshot-history-list [snapshotHistory]="record.snapshotHistory"></app-clinical-snapshot-history-list>
        </section>

        <section class="timeline-shell">
          <div class="notes-head">
            <div>
              <p class="eyebrow">{{ 'Clinical timeline' | t }}</p>
              <h3>{{ 'Newest clinical events first' | t }}</h3>
              <p class="section-copy">{{ 'Only note created, diagnosis created, and diagnosis resolved events are included in this slice.' | t }}</p>
            </div>
          </div>

          <app-clinical-timeline-list [timeline]="record.timeline"></app-clinical-timeline-list>
        </section>

        <section class="diagnoses-shell">
          <div class="notes-head">
            <div>
              <p class="eyebrow">{{ 'Diagnoses' | t }}</p>
              <h3>{{ 'Basic diagnoses only' | t }}</h3>
              <p class="section-copy">{{ 'No coded catalogs or treatment linkage in this slice.' | t }}</p>
            </div>
          </div>

          <app-clinical-diagnosis-create-form
            *ngIf="canWrite"
            [saving]="savingDiagnosis"
            [error]="diagnosisError"
            [revision]="diagnosisFormRevision"
            (saved)="addDiagnosis($event)">
          </app-clinical-diagnosis-create-form>

          <app-clinical-diagnoses-list
            [diagnoses]="record.diagnoses"
            [canWrite]="canWrite"
            [resolvingDiagnosisId]="resolvingDiagnosisId"
            (resolveRequested)="resolveDiagnosis($event)">
          </app-clinical-diagnoses-list>
        </section>

        <section class="notes-shell">
          <div class="notes-head">
            <div>
              <p class="eyebrow">{{ 'Clinical notes' | t }}</p>
              <h3>{{ 'Notes newest first' | t }}</h3>
            </div>
          </div>

          <app-clinical-note-create-form
            *ngIf="canWrite"
            [saving]="savingNote"
            [error]="noteError"
            [revision]="noteFormRevision"
            (saved)="addNote($event)">
          </app-clinical-note-create-form>

          <app-clinical-notes-list [notes]="record.notes"></app-clinical-notes-list>
        </section>
      </article>
    </section>
  `,
  styles: [`
    .clinical-record-page {
      display: grid;
      gap: 1.25rem;
    }

    .snapshot-history-shell,
    .timeline-shell,
    .diagnoses-shell,
    .notes-shell {
      display: grid;
      gap: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
      padding: 1rem;
      box-shadow: var(--bsm-shadow-sm);
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    .record-shell {
      display: grid;
      gap: 1.25rem;
    }

    .loading-grid {
      display: grid;
      gap: 0.75rem;
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }

    .patient-context-strip {
      display: grid;
      grid-template-columns: repeat(5, minmax(0, 1fr));
      gap: 0.75rem;
      padding: 0.85rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-sm);
    }

    .patient-context-item {
      display: grid;
      gap: 0.35rem;
      min-width: 0;
      padding: 0.75rem 0.85rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
    }

    .patient-context-item--wide {
      grid-column: span 2;
    }

    .patient-context-item span {
      color: var(--bsm-color-text-muted);
      font-size: 0.75rem;
      font-weight: 800;
      line-height: 1.2;
      text-transform: uppercase;
    }

    .patient-context-item strong {
      min-width: 0;
      color: var(--bsm-color-text-brand);
      font-size: 0.98rem;
      line-height: 1.25;
      overflow-wrap: anywhere;
    }

    .record-meta {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      margin: 0;
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0;
      color: var(--bsm-color-text-muted);
    }

    dd {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
      word-break: break-word;
    }

    .section-copy {
      margin: 0.35rem 0 0;
      color: var(--bsm-color-text-muted);
      max-width: 60ch;
    }

    .clinical-action {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 0;
      border: 1px solid var(--bsm-color-primary-soft);
      border-radius: var(--bsm-radius-pill);
      padding: 0.72rem 1rem;
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
      text-decoration: none;
      font-weight: 800;
      line-height: 1.2;
      text-align: center;
      transition:
        background-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        color var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .clinical-action:hover {
      box-shadow: var(--bsm-shadow-sm);
      transform: translateY(-1px);
    }

    .clinical-action:focus-visible {
      outline: none;
      box-shadow: var(--bsm-shadow-focus);
    }

    .clinical-error {
      border: 1px solid var(--bsm-color-danger-soft);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
      padding: 1rem;
      font-weight: 700;
      box-shadow: var(--bsm-shadow-sm);
    }

    @media (prefers-reduced-motion: reduce) {
      .clinical-action:hover {
        transform: none;
      }
    }

    @media (max-width: 768px) {
      .loading-grid,
      .patient-context-strip {
        grid-template-columns: 1fr;
      }

      .patient-context-item--wide {
        grid-column: span 1;
      }

      .clinical-action {
        width: 100%;
      }
    }
  `]
})
export class ClinicalRecordPageComponent implements OnInit {
  readonly clinicalRecordsFacade = inject(ClinicalRecordsFacade);
  readonly patientsFacade = inject(PatientsFacade);
  private readonly i18n = inject(I18nService);

  readonly canWrite = inject(AuthService).hasPermissions(['clinical.write']);
  readonly loadingCards = [0, 1, 2];

  private readonly route = inject(ActivatedRoute);

  patientId: string | null = null;
  creatingRecord = false;
  savingSnapshot = false;
  savingNote = false;
  savingDiagnosis = false;
  savingQuestionnaire = false;
  snapshotError: string | null = null;
  noteError: string | null = null;
  diagnosisError: string | null = null;
  questionnaireSaveError: string | null = null;
  noteFormRevision = 0;
  diagnosisFormRevision = 0;
  resolvingDiagnosisId: string | null = null;

  get patientDisplayName(): string {
    return this.patientsFacade.currentPatient()?.fullName ?? this.i18n.translate('this patient');
  }

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id');
    if (!this.patientId) {
      this.clinicalRecordsFacade.clearRecord();
      this.patientsFacade.clearCurrentPatient();
      return;
    }

    this.patientsFacade.clearCurrentPatient();
    this.patientsFacade.loadPatient(this.patientId);
    this.clinicalRecordsFacade.clearRecord();
    this.clinicalRecordsFacade.loadRecord(this.patientId);
    this.clinicalRecordsFacade.loadQuestionnaire(this.patientId);
  }

  startCreate(): void {
    this.creatingRecord = true;
    this.snapshotError = null;
  }

  cancelCreate(): void {
    this.creatingRecord = false;
    this.snapshotError = null;
  }

  saveSnapshot(payload: SaveClinicalRecordSnapshotRequest): void {
    if (!this.patientId) {
      return;
    }

    this.savingSnapshot = true;
    this.snapshotError = null;

    const request$ = this.clinicalRecordsFacade.currentRecord()
      ? this.clinicalRecordsFacade.updateRecord(this.patientId, payload)
      : this.clinicalRecordsFacade.createRecord(this.patientId, payload);

    request$.subscribe({
      next: () => {
        this.savingSnapshot = false;
        this.creatingRecord = false;
        this.reloadQuestionnaire();
      },
      error: () => {
        this.savingSnapshot = false;
        this.snapshotError = this.clinicalRecordsFacade.currentRecord()
          ? 'The clinical record snapshot could not be updated.'
          : 'The clinical record could not be created.';
      }
    });
  }

  reloadQuestionnaire(): void {
    if (!this.patientId) {
      return;
    }

    this.questionnaireSaveError = null;
    this.clinicalRecordsFacade.loadQuestionnaire(this.patientId);
  }

  saveQuestionnaire(payload: SaveClinicalMedicalQuestionnaireRequest): void {
    if (!this.patientId) {
      return;
    }

    this.savingQuestionnaire = true;
    this.questionnaireSaveError = null;

    this.clinicalRecordsFacade.updateQuestionnaire(this.patientId, payload)
      .subscribe({
        next: () => {
          this.savingQuestionnaire = false;
          this.questionnaireSaveError = null;
        },
        error: () => {
          this.savingQuestionnaire = false;
          this.questionnaireSaveError = 'The medical questionnaire could not be saved.';
        }
      });
  }

  addNote(payload: AddClinicalNoteRequest): void {
    if (!this.patientId) {
      return;
    }

    this.savingNote = true;
    this.noteError = null;

    this.clinicalRecordsFacade.addNote(this.patientId, payload)
      .subscribe({
        next: () => {
          this.savingNote = false;
          this.noteFormRevision += 1;
        },
        error: () => {
          this.savingNote = false;
          this.noteError = 'The clinical note could not be added.';
        }
      });
  }

  addDiagnosis(payload: AddClinicalDiagnosisRequest): void {
    if (!this.patientId) {
      return;
    }

    this.savingDiagnosis = true;
    this.diagnosisError = null;

    this.clinicalRecordsFacade.addDiagnosis(this.patientId, payload)
      .subscribe({
        next: () => {
          this.savingDiagnosis = false;
          this.diagnosisFormRevision += 1;
        },
        error: () => {
          this.savingDiagnosis = false;
          this.diagnosisError = 'The diagnosis could not be added.';
        }
      });
  }

  resolveDiagnosis(diagnosisId: string): void {
    if (!this.patientId) {
      return;
    }

    this.resolvingDiagnosisId = diagnosisId;
    this.diagnosisError = null;

    this.clinicalRecordsFacade.resolveDiagnosis(this.patientId, diagnosisId)
      .subscribe({
        next: () => {
          this.resolvingDiagnosisId = null;
        },
        error: () => {
          this.resolvingDiagnosisId = null;
          this.diagnosisError = 'The diagnosis could not be resolved.';
        }
      });
  }
}
