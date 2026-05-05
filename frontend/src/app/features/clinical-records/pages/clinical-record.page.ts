import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { ClinicalBackgroundFormComponent } from '../components/clinical-background-form.component';
import { ClinicalDiagnosisCreateFormComponent } from '../components/clinical-diagnosis-create-form.component';
import { ClinicalDiagnosesListComponent } from '../components/clinical-diagnoses-list.component';
import { ClinicalNoteCreateFormComponent } from '../components/clinical-note-create-form.component';
import { ClinicalNotesListComponent } from '../components/clinical-notes-list.component';
import { ClinicalRecordEmptyStateComponent } from '../components/clinical-record-empty-state.component';
import { ClinicalSnapshotHistoryListComponent } from '../components/clinical-snapshot-history-list.component';
import { ClinicalTimelineListComponent } from '../components/clinical-timeline-list.component';
import { ClinicalRecordsFacade } from '../facades/clinical-records.facade';
import {
  AddClinicalDiagnosisRequest,
  AddClinicalNoteRequest,
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
    ClinicalNoteCreateFormComponent,
    ClinicalNotesListComponent,
    ClinicalSnapshotHistoryListComponent,
    ClinicalTimelineListComponent,
    LocalizedDatePipe,
    TranslatePipe
  ],
  template: `
    <section class="clinical-record-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 3.4 / {{ 'Clinical Snapshot Change History' | t }}</p>
          <h2>{{ 'Clinical record' | t }}</h2>
          <p class="subtitle">
            {{ 'Minimal clinical foundation for {patientDisplayName} with explicit creation, base snapshot history, current allergies, basic diagnoses, append-only notes, and a separate clinical timeline.' | t:{ patientDisplayName } }}
          </p>
        </div>

        <div class="head-actions">
          <a *ngIf="patientId" [routerLink]="['/patients', patientId]" class="action-link action-secondary">{{ 'Back to patient' | t }}</a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient() || clinicalRecordsFacade.loadingRecord()" class="state-card">
        {{ 'Loading clinical record...' | t }}
      </div>

      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() | t }}
      </div>

      <div *ngIf="clinicalRecordsFacade.recordError()" class="state-card state-error">
        {{ clinicalRecordsFacade.recordError() | t }}
      </div>

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
        <section class="record-meta">
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
        </section>

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

    .page-head,
    .state-card,
    .record-meta,
    .snapshot-history-shell,
    .timeline-shell,
    .diagnoses-shell,
    .notes-shell {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .page-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2,
    h3 {
      margin: 0;
      color: #16324f;
    }

    .subtitle {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 62ch;
    }

    .record-shell {
      display: grid;
      gap: 1.25rem;
    }

    .record-meta {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #718298;
    }

    dd {
      margin: 0;
      color: #16324f;
      font-weight: 600;
      word-break: break-word;
    }

    .notes-shell {
      display: grid;
      gap: 1rem;
    }

    .timeline-shell {
      display: grid;
      gap: 1rem;
    }

    .snapshot-history-shell {
      display: grid;
      gap: 1rem;
    }

    .diagnoses-shell {
      display: grid;
      gap: 1rem;
    }

    .section-copy {
      margin: 0.35rem 0 0;
      color: #5b6e84;
      max-width: 60ch;
    }

    .head-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .action-link {
      text-decoration: none;
      color: #0a5bb5;
      font-weight: 700;
    }

    .action-secondary {
      color: #4d6278;
    }

    .state-error {
      border-color: #f2c4c4;
      background: #fff3f3;
      color: #8c2525;
    }

    @media (max-width: 768px) {
      .page-head {
        flex-direction: column;
      }
    }
  `]
})
export class ClinicalRecordPageComponent implements OnInit {
  readonly clinicalRecordsFacade = inject(ClinicalRecordsFacade);
  readonly patientsFacade = inject(PatientsFacade);
  private readonly i18n = inject(I18nService);

  readonly canWrite = inject(AuthService).hasPermissions(['clinical.write']);

  private readonly route = inject(ActivatedRoute);

  patientId: string | null = null;
  creatingRecord = false;
  savingSnapshot = false;
  savingNote = false;
  savingDiagnosis = false;
  snapshotError: string | null = null;
  noteError: string | null = null;
  diagnosisError: string | null = null;
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
      },
      error: () => {
        this.savingSnapshot = false;
        this.snapshotError = this.clinicalRecordsFacade.currentRecord()
          ? 'The clinical record snapshot could not be updated.'
          : 'The clinical record could not be created.';
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
