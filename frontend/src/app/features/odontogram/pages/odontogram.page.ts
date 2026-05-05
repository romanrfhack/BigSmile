import { CommonModule } from '@angular/common';
import { Component, OnInit, effect, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { OdontogramEmptyStateComponent } from '../components/odontogram-empty-state.component';
import { OdontogramGridComponent } from '../components/odontogram-grid.component';
import { ToothStateEditorComponent } from '../components/tooth-state-editor.component';
import { OdontogramsFacade } from '../facades/odontograms.facade';
import {
  OdontogramSurfaceFindingType,
  OdontogramSurfaceStatus,
  OdontogramToothState,
  OdontogramToothStatus
} from '../models/odontogram.models';

@Component({
  selector: 'app-odontogram-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    OdontogramEmptyStateComponent,
    OdontogramGridComponent,
    ToothStateEditorComponent,
    LocalizedDatePipe,
    TranslatePipe
  ],
  template: `
    <section class="odontogram-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 4.4 / {{ 'Dental Findings Change History' | t }}</p>
          <h2>{{ 'Odontogram' | t }}</h2>
          <p class="subtitle">
            {{ 'Patient-scoped odontogram for {patientDisplayName} using FDI adult permanent tooth numbering, the minimal O/M/D/B/L surface set, the basic finding catalog, and bounded surface finding add/remove history.' | t:{ patientDisplayName } }}
          </p>
        </div>

        <div class="head-actions">
          <a *ngIf="patientId" [routerLink]="['/patients', patientId]" class="action-link action-secondary">{{ 'Back to patient' | t }}</a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient() || odontogramsFacade.loadingOdontogram()" class="state-card">
        {{ 'Loading odontogram...' | t }}
      </div>

      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() | t }}
      </div>

      <div *ngIf="odontogramsFacade.odontogramError()" class="state-card state-error">
        {{ odontogramsFacade.odontogramError() | t }}
      </div>

      <div *ngIf="actionError && odontogramsFacade.odontogramMissing()" class="state-card state-error">
        {{ actionError | t }}
      </div>

      <app-odontogram-empty-state
        *ngIf="!odontogramsFacade.loadingOdontogram() && odontogramsFacade.odontogramMissing() && !odontogramsFacade.odontogramError()"
        [canWrite]="canWrite"
        [creating]="savingCreate"
        (createRequested)="createOdontogram()">
      </app-odontogram-empty-state>

      <article *ngIf="odontogramsFacade.currentOdontogram() as odontogram" class="odontogram-shell">
        <section class="meta-card">
          <div>
            <dt>{{ 'Created' | t }}</dt>
            <dd>{{ odontogram.createdAtUtc | bsDate: 'medium' }}</dd>
          </div>
          <div>
            <dt>{{ 'Created by' | t }}</dt>
            <dd>{{ odontogram.createdByUserId }}</dd>
          </div>
          <div>
            <dt>{{ 'Last updated' | t }}</dt>
            <dd>{{ odontogram.lastUpdatedAtUtc | bsDate: 'medium' }}</dd>
          </div>
          <div>
            <dt>{{ 'Updated by' | t }}</dt>
            <dd>{{ odontogram.lastUpdatedByUserId }}</dd>
          </div>
        </section>

        <section class="grid-card">
          <div class="section-head">
            <div>
              <p class="eyebrow">{{ 'Tooth and surface states' | t }}</p>
              <h3>{{ '32 permanent adult teeth' | t }}</h3>
              <p class="section-copy">{{ 'Surface detail uses the minimal O/M/D/B/L set. Basic findings stay separate from tooth and surface status, and findings history stays separate from any future dental timeline. No restore, full odontogram versioning, treatment linkage, or document linkage in Release 4.4.' | t }}</p>
            </div>
          </div>

          <app-odontogram-grid
            [teeth]="odontogram.teeth"
            [selectedToothCode]="selectedToothCode"
            (toothSelected)="selectTooth($event)">
          </app-odontogram-grid>
        </section>

        <app-tooth-state-editor
          [tooth]="selectedTooth"
          [canWrite]="canWrite"
          [savingTooth]="savingTooth"
          [savingSurface]="savingSurface"
          [savingFinding]="savingFinding"
          [removingFindingId]="removingFindingId"
          [error]="actionError"
          [findingsHistory]="odontogram.findingsHistory"
          (updateRequested)="updateToothStatus($event)"
          (surfaceUpdateRequested)="updateSurfaceStatus($event)"
          (findingAddRequested)="addSurfaceFinding($event)"
          (findingRemoveRequested)="removeSurfaceFinding($event)">
        </app-tooth-state-editor>
      </article>
    </section>
  `,
  styles: [`
    .odontogram-page {
      display: grid;
      gap: 1.25rem;
    }

    .page-head,
    .state-card,
    .meta-card,
    .grid-card {
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

    .subtitle,
    .section-copy {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 62ch;
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

    .odontogram-shell {
      display: grid;
      gap: 1.25rem;
    }

    .meta-card {
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

    .grid-card {
      display: grid;
      gap: 1rem;
    }

    @media (max-width: 768px) {
      .page-head {
        flex-direction: column;
      }
    }
  `]
})
export class OdontogramPageComponent implements OnInit {
  readonly odontogramsFacade = inject(OdontogramsFacade);
  readonly patientsFacade = inject(PatientsFacade);
  readonly canWrite = inject(AuthService).hasPermissions(['odontogram.write']);
  private readonly i18n = inject(I18nService);

  private readonly route = inject(ActivatedRoute);

  patientId: string | null = null;
  savingCreate = false;
  savingTooth = false;
  savingSurface = false;
  savingFinding = false;
  removingFindingId: string | null = null;
  actionError: string | null = null;
  selectedToothCode: string | null = null;

  constructor() {
    effect(() => {
      const odontogram = this.odontogramsFacade.currentOdontogram();
      if (!odontogram) {
        this.selectedToothCode = null;
        return;
      }

      if (!this.selectedToothCode || !odontogram.teeth.some((tooth) => tooth.toothCode === this.selectedToothCode)) {
        this.selectedToothCode = odontogram.teeth[0]?.toothCode ?? null;
      }
    });
  }

  get patientDisplayName(): string {
    return this.patientsFacade.currentPatient()?.fullName ?? this.i18n.translate('this patient');
  }

  get selectedTooth(): OdontogramToothState | null {
    const odontogram = this.odontogramsFacade.currentOdontogram();
    if (!odontogram || !this.selectedToothCode) {
      return null;
    }

    return odontogram.teeth.find((tooth) => tooth.toothCode === this.selectedToothCode) ?? null;
  }

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id');
    if (!this.patientId) {
      this.odontogramsFacade.clearOdontogram();
      this.patientsFacade.clearCurrentPatient();
      return;
    }

    this.patientsFacade.clearCurrentPatient();
    this.patientsFacade.loadPatient(this.patientId);
    this.odontogramsFacade.clearOdontogram();
    this.odontogramsFacade.loadOdontogram(this.patientId);
  }

  createOdontogram(): void {
    if (!this.patientId) {
      return;
    }

    this.savingCreate = true;
    this.actionError = null;

    this.odontogramsFacade.createOdontogram(this.patientId)
      .subscribe({
        next: (odontogram) => {
          this.savingCreate = false;
          this.selectedToothCode = odontogram.teeth[0]?.toothCode ?? null;
        },
        error: () => {
          this.savingCreate = false;
          this.actionError = 'The odontogram could not be created.';
        }
      });
  }

  selectTooth(toothCode: string): void {
    this.selectedToothCode = toothCode;
    this.actionError = null;
  }

  updateToothStatus(status: OdontogramToothStatus): void {
    if (!this.patientId || !this.selectedToothCode) {
      return;
    }

    this.savingTooth = true;
    this.actionError = null;

    this.odontogramsFacade.updateToothStatus(this.patientId, this.selectedToothCode, { status })
      .subscribe({
        next: () => {
          this.savingTooth = false;
        },
        error: () => {
          this.savingTooth = false;
          this.actionError = 'The tooth state could not be updated.';
        }
      });
  }

  updateSurfaceStatus(event: { surfaceCode: string; status: OdontogramSurfaceStatus }): void {
    if (!this.patientId || !this.selectedToothCode) {
      return;
    }

    this.savingSurface = true;
    this.actionError = null;

    this.odontogramsFacade.updateSurfaceStatus(this.patientId, this.selectedToothCode, event.surfaceCode, { status: event.status })
      .subscribe({
        next: () => {
          this.savingSurface = false;
        },
        error: () => {
          this.savingSurface = false;
          this.actionError = 'The surface state could not be updated.';
        }
      });
  }

  addSurfaceFinding(event: { surfaceCode: string; findingType: OdontogramSurfaceFindingType }): void {
    if (!this.patientId || !this.selectedToothCode) {
      return;
    }

    this.savingFinding = true;
    this.removingFindingId = null;
    this.actionError = null;

    this.odontogramsFacade.addSurfaceFinding(this.patientId, this.selectedToothCode, event.surfaceCode, {
      findingType: event.findingType
    }).subscribe({
      next: () => {
        this.savingFinding = false;
      },
      error: () => {
        this.savingFinding = false;
        this.actionError = 'The surface finding could not be added.';
      }
    });
  }

  removeSurfaceFinding(event: { surfaceCode: string; findingId: string }): void {
    if (!this.patientId || !this.selectedToothCode) {
      return;
    }

    this.savingFinding = true;
    this.removingFindingId = event.findingId;
    this.actionError = null;

    this.odontogramsFacade.removeSurfaceFinding(this.patientId, this.selectedToothCode, event.surfaceCode, event.findingId)
      .subscribe({
        next: () => {
          this.savingFinding = false;
          this.removingFindingId = null;
        },
        error: () => {
          this.savingFinding = false;
          this.removingFindingId = null;
          this.actionError = 'The surface finding could not be removed.';
        }
      });
  }
}
