import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { TreatmentPlanEmptyStateComponent } from '../components/treatment-plan-empty-state.component';
import { TreatmentPlanItemFormComponent } from '../components/treatment-plan-item-form.component';
import { TreatmentPlanItemsListComponent } from '../components/treatment-plan-items-list.component';
import { TreatmentPlanStatusEditorComponent } from '../components/treatment-plan-status-editor.component';
import { TreatmentPlansFacade } from '../facades/treatment-plans.facade';
import {
  AddTreatmentPlanItemRequest,
  ChangeTreatmentPlanStatusRequest,
  TreatmentPlanStatus
} from '../models/treatment-plan.models';

@Component({
  selector: 'app-treatment-plan-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    TreatmentPlanEmptyStateComponent,
    TreatmentPlanItemFormComponent,
    TreatmentPlanItemsListComponent,
    TreatmentPlanStatusEditorComponent
  ],
  template: `
    <section class="treatment-plan-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">Release 5.1 / Treatment Plan Foundation</p>
          <h2>Treatment plan</h2>
          <p class="subtitle">
            Minimal patient-scoped treatment planning for {{ patientDisplayName }} with explicit creation, basic items, optional tooth/surface references, and a bounded Draft / Proposed / Accepted lifecycle.
          </p>
        </div>

        <div class="head-actions">
          <a *ngIf="patientId" [routerLink]="['/patients', patientId]" class="action-link action-secondary">Back to patient</a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient() || treatmentPlansFacade.loadingTreatmentPlan()" class="state-card">
        Loading treatment plan...
      </div>

      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() }}
      </div>

      <div *ngIf="treatmentPlansFacade.treatmentPlanError()" class="state-card state-error">
        {{ treatmentPlansFacade.treatmentPlanError() }}
      </div>

      <div *ngIf="actionError" class="state-card state-error">
        {{ actionError }}
      </div>

      <app-treatment-plan-empty-state
        *ngIf="!treatmentPlansFacade.loadingTreatmentPlan() && treatmentPlansFacade.treatmentPlanMissing() && !treatmentPlansFacade.treatmentPlanError()"
        [canWrite]="canWrite"
        [creating]="creatingPlan"
        (createRequested)="createTreatmentPlan()">
      </app-treatment-plan-empty-state>

      <article *ngIf="treatmentPlansFacade.currentTreatmentPlan() as treatmentPlan" class="plan-shell">
        <section class="plan-meta">
          <div>
            <dt>Status</dt>
            <dd>{{ treatmentPlan.status }}</dd>
          </div>
          <div>
            <dt>Items</dt>
            <dd>{{ treatmentPlan.items.length }}</dd>
          </div>
          <div>
            <dt>Created</dt>
            <dd>{{ treatmentPlan.createdAtUtc | date: 'medium' }}</dd>
          </div>
          <div>
            <dt>Created by</dt>
            <dd>{{ treatmentPlan.createdByUserId }}</dd>
          </div>
          <div>
            <dt>Last updated</dt>
            <dd>{{ treatmentPlan.lastUpdatedAtUtc | date: 'medium' }}</dd>
          </div>
          <div>
            <dt>Updated by</dt>
            <dd>{{ treatmentPlan.lastUpdatedByUserId }}</dd>
          </div>
        </section>

        <app-treatment-plan-status-editor
          [currentStatus]="treatmentPlan.status"
          [availableStatuses]="availableStatuses"
          [canWrite]="canWrite"
          [saving]="savingStatus"
          (statusChanged)="changeStatus({ status: $event })">
        </app-treatment-plan-status-editor>

        <app-treatment-plan-item-form
          *ngIf="canWrite"
          [saving]="savingItem"
          [disabled]="!canEditPlan"
          [error]="itemError"
          [revision]="itemFormRevision"
          (saved)="addItem($event)">
        </app-treatment-plan-item-form>

        <app-treatment-plan-items-list
          [items]="treatmentPlan.items"
          [canRemove]="canEditPlan"
          [removingItemId]="removingItemId"
          (removeRequested)="removeItem($event)">
        </app-treatment-plan-items-list>
      </article>
    </section>
  `,
  styles: [`
    .treatment-plan-page {
      display: grid;
      gap: 1.25rem;
    }

    .page-head,
    .state-card,
    .plan-meta {
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

    .plan-shell {
      display: grid;
      gap: 1.25rem;
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2 {
      margin: 0;
      color: #16324f;
    }

    .subtitle {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 62ch;
    }

    .plan-meta {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
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

    @media (max-width: 768px) {
      .page-head {
        flex-direction: column;
      }
    }
  `]
})
export class TreatmentPlanPageComponent implements OnInit {
  readonly treatmentPlansFacade = inject(TreatmentPlansFacade);
  readonly patientsFacade = inject(PatientsFacade);
  readonly canWrite = inject(AuthService).hasPermissions(['treatmentplan.write']);

  private readonly route = inject(ActivatedRoute);

  patientId: string | null = null;
  creatingPlan = false;
  savingItem = false;
  savingStatus = false;
  removingItemId: string | null = null;
  itemError: string | null = null;
  actionError: string | null = null;
  itemFormRevision = 0;

  get patientDisplayName(): string {
    return this.patientsFacade.currentPatient()?.fullName ?? 'this patient';
  }

  get canEditPlan(): boolean {
    const treatmentPlan = this.treatmentPlansFacade.currentTreatmentPlan();
    return !!treatmentPlan && this.canWrite && treatmentPlan.status !== 'Accepted';
  }

  get availableStatuses(): TreatmentPlanStatus[] {
    const treatmentPlan = this.treatmentPlansFacade.currentTreatmentPlan();
    if (!treatmentPlan) {
      return ['Draft'];
    }

    switch (treatmentPlan.status) {
      case 'Draft':
        return ['Draft', 'Proposed'];
      case 'Proposed':
        return ['Draft', 'Proposed', 'Accepted'];
      case 'Accepted':
        return ['Accepted'];
    }
  }

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id');
    if (!this.patientId) {
      this.treatmentPlansFacade.clearTreatmentPlan();
      this.patientsFacade.clearCurrentPatient();
      return;
    }

    this.patientsFacade.clearCurrentPatient();
    this.patientsFacade.loadPatient(this.patientId);
    this.treatmentPlansFacade.clearTreatmentPlan();
    this.treatmentPlansFacade.loadTreatmentPlan(this.patientId);
  }

  createTreatmentPlan(): void {
    if (!this.patientId) {
      return;
    }

    this.creatingPlan = true;
    this.actionError = null;

    this.treatmentPlansFacade.createTreatmentPlan(this.patientId)
      .subscribe({
        next: () => {
          this.creatingPlan = false;
        },
        error: () => {
          this.creatingPlan = false;
          this.actionError = 'The treatment plan could not be created.';
        }
      });
  }

  addItem(payload: AddTreatmentPlanItemRequest): void {
    if (!this.patientId) {
      return;
    }

    this.savingItem = true;
    this.itemError = null;
    this.actionError = null;

    this.treatmentPlansFacade.addItem(this.patientId, payload)
      .subscribe({
        next: () => {
          this.savingItem = false;
          this.itemFormRevision += 1;
        },
        error: () => {
          this.savingItem = false;
          this.itemError = 'The treatment plan item could not be added.';
        }
      });
  }

  removeItem(itemId: string): void {
    if (!this.patientId) {
      return;
    }

    this.removingItemId = itemId;
    this.itemError = null;
    this.actionError = null;

    this.treatmentPlansFacade.removeItem(this.patientId, itemId)
      .subscribe({
        next: () => {
          this.removingItemId = null;
        },
        error: () => {
          this.removingItemId = null;
          this.actionError = 'The treatment plan item could not be removed.';
        }
      });
  }

  changeStatus(payload: ChangeTreatmentPlanStatusRequest): void {
    if (!this.patientId) {
      return;
    }

    this.savingStatus = true;
    this.itemError = null;
    this.actionError = null;

    this.treatmentPlansFacade.changeStatus(this.patientId, payload)
      .subscribe({
        next: () => {
          this.savingStatus = false;
        },
        error: () => {
          this.savingStatus = false;
          this.actionError = 'The treatment plan status could not be updated.';
        }
      });
  }
}
