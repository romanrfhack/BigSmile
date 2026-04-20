import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { ClinicalBackgroundFormComponent } from '../components/clinical-background-form.component';
import { ClinicalNoteCreateFormComponent } from '../components/clinical-note-create-form.component';
import { ClinicalNotesListComponent } from '../components/clinical-notes-list.component';
import { ClinicalRecordEmptyStateComponent } from '../components/clinical-record-empty-state.component';
import { ClinicalRecordsFacade } from '../facades/clinical-records.facade';
import {
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
    ClinicalNoteCreateFormComponent,
    ClinicalNotesListComponent
  ],
  template: `
    <section class="clinical-record-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">Release 3.1 / Clinical Record Foundation</p>
          <h2>Clinical record</h2>
          <p class="subtitle">
            Minimal clinical foundation for {{ patientDisplayName }} with explicit creation, current allergies, and append-only notes.
          </p>
        </div>

        <div class="head-actions">
          <a *ngIf="patientId" [routerLink]="['/patients', patientId]" class="action-link action-secondary">Back to patient</a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient() || clinicalRecordsFacade.loadingRecord()" class="state-card">
        Loading clinical record...
      </div>

      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() }}
      </div>

      <div *ngIf="clinicalRecordsFacade.recordError()" class="state-card state-error">
        {{ clinicalRecordsFacade.recordError() }}
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
            <dt>Created</dt>
            <dd>{{ record.createdAtUtc | date: 'medium' }}</dd>
          </div>
          <div>
            <dt>Created by</dt>
            <dd>{{ record.createdByUserId }}</dd>
          </div>
          <div>
            <dt>Last updated</dt>
            <dd>{{ record.lastUpdatedAtUtc | date: 'medium' }}</dd>
          </div>
          <div>
            <dt>Updated by</dt>
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

        <section class="notes-shell">
          <div class="notes-head">
            <div>
              <p class="eyebrow">Clinical notes</p>
              <h3>Notes newest first</h3>
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

  readonly canWrite = inject(AuthService).hasPermissions(['clinical.write']);

  private readonly route = inject(ActivatedRoute);

  patientId: string | null = null;
  creatingRecord = false;
  savingSnapshot = false;
  savingNote = false;
  snapshotError: string | null = null;
  noteError: string | null = null;
  noteFormRevision = 0;

  get patientDisplayName(): string {
    return this.patientsFacade.currentPatient()?.fullName ?? 'this patient';
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
}
