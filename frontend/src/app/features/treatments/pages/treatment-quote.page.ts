import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { TreatmentPlansFacade } from '../facades/treatment-plans.facade';
import { TreatmentQuoteEmptyStateComponent } from '../components/treatment-quote-empty-state.component';
import { TreatmentQuoteItemsListComponent } from '../components/treatment-quote-items-list.component';
import { TreatmentQuoteNoPlanStateComponent } from '../components/treatment-quote-no-plan-state.component';
import { TreatmentQuoteStatusEditorComponent } from '../components/treatment-quote-status-editor.component';
import { TreatmentQuotesFacade } from '../facades/treatment-quotes.facade';
import {
  ChangeTreatmentQuoteStatusRequest,
  TreatmentQuoteStatus
} from '../models/treatment-quote.models';

@Component({
  selector: 'app-treatment-quote-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    TreatmentQuoteEmptyStateComponent,
    TreatmentQuoteItemsListComponent,
    TreatmentQuoteNoPlanStateComponent,
    TreatmentQuoteStatusEditorComponent,
    LocalizedDatePipe,
    TranslatePipe
  ],
  template: `
    <section class="treatment-quote-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 5.2 / {{ 'Quote Basics' | t }}</p>
          <h2>{{ 'Treatment quote' | t }}</h2>
          <p class="subtitle">
            {{ 'Minimal patient-scoped commercial quote for {patientDisplayName} with explicit snapshot creation from the existing treatment plan, line-level pricing, subtotal/total calculation, and a bounded Draft / Proposed / Accepted lifecycle.' | t:{ patientDisplayName } }}
          </p>
        </div>

        <div class="head-actions">
          <a *ngIf="patientId" [routerLink]="['/patients', patientId]" class="action-link action-secondary">{{ 'Back to patient' | t }}</a>
          <a *ngIf="patientId" [routerLink]="['/patients', patientId, 'treatment-plan']" class="action-link action-secondary">{{ 'Treatment plan' | t }}</a>
          <a
            *ngIf="patientId && canReadBillingDocuments"
            [routerLink]="['/patients', patientId, 'treatment-plan', 'quote', 'billing']"
            class="action-link action-secondary">
            {{ 'Billing' | t }}
          </a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient() || treatmentPlansFacade.loadingTreatmentPlan() || treatmentQuotesFacade.loadingTreatmentQuote()" class="state-card">
        {{ 'Loading treatment quote...' | t }}
      </div>

      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() | t }}
      </div>

      <div *ngIf="treatmentPlansFacade.treatmentPlanError()" class="state-card state-error">
        {{ treatmentPlansFacade.treatmentPlanError() | t }}
      </div>

      <div *ngIf="treatmentQuotesFacade.treatmentQuoteError()" class="state-card state-error">
        {{ treatmentQuotesFacade.treatmentQuoteError() | t }}
      </div>

      <div *ngIf="actionError" class="state-card state-error">
        {{ actionError | t }}
      </div>

      <app-treatment-quote-no-plan-state
        *ngIf="!treatmentPlansFacade.loadingTreatmentPlan() && treatmentPlansFacade.treatmentPlanMissing() && !treatmentPlansFacade.treatmentPlanError()"
        [patientId]="patientId">
      </app-treatment-quote-no-plan-state>

      <app-treatment-quote-empty-state
        *ngIf="!treatmentPlansFacade.loadingTreatmentPlan()
          && !treatmentQuotesFacade.loadingTreatmentQuote()
          && !treatmentPlansFacade.treatmentPlanMissing()
          && !!treatmentPlansFacade.currentTreatmentPlan()
          && treatmentQuotesFacade.treatmentQuoteMissing()
          && !treatmentQuotesFacade.treatmentQuoteError()"
        [canWrite]="canWrite"
        [creating]="creatingQuote"
        (createRequested)="createTreatmentQuote()">
      </app-treatment-quote-empty-state>

      <article *ngIf="treatmentQuotesFacade.currentTreatmentQuote() as treatmentQuote" class="quote-shell">
        <section class="quote-meta">
          <div>
            <dt>{{ 'Status' | t }}</dt>
            <dd>{{ treatmentQuote.status | t }}</dd>
          </div>
          <div>
            <dt>{{ 'Items' | t }}</dt>
            <dd>{{ treatmentQuote.items.length }}</dd>
          </div>
          <div>
            <dt>{{ 'Currency' | t }}</dt>
            <dd>{{ treatmentQuote.currencyCode }}</dd>
          </div>
          <div>
            <dt>{{ 'Total' | t }}</dt>
            <dd>{{ treatmentQuote.total | number: '1.2-2' }} {{ treatmentQuote.currencyCode }}</dd>
          </div>
          <div>
            <dt>{{ 'Created' | t }}</dt>
            <dd>{{ treatmentQuote.createdAtUtc | bsDate: 'medium' }}</dd>
          </div>
          <div>
            <dt>{{ 'Created by' | t }}</dt>
            <dd>{{ treatmentQuote.createdByUserId }}</dd>
          </div>
          <div>
            <dt>{{ 'Last updated' | t }}</dt>
            <dd>{{ treatmentQuote.lastUpdatedAtUtc | bsDate: 'medium' }}</dd>
          </div>
          <div>
            <dt>{{ 'Updated by' | t }}</dt>
            <dd>{{ treatmentQuote.lastUpdatedByUserId }}</dd>
          </div>
        </section>

        <app-treatment-quote-status-editor
          [currentStatus]="treatmentQuote.status"
          [availableStatuses]="availableStatuses"
          [canWrite]="canWrite"
          [saving]="savingStatus"
          (statusChanged)="changeStatus({ status: $event })">
        </app-treatment-quote-status-editor>

        <app-treatment-quote-items-list
          [items]="treatmentQuote.items"
          [currencyCode]="treatmentQuote.currencyCode"
          [canEdit]="canEditQuote"
          [savingItemId]="savingItemId"
          (priceUpdateRequested)="updateItemPrice($event.quoteItemId, $event.unitPrice)">
        </app-treatment-quote-items-list>
      </article>
    </section>
  `,
  styles: [`
    .treatment-quote-page {
      display: grid;
      gap: 1.25rem;
    }

    .page-head,
    .state-card,
    .quote-meta {
      border-radius: 20px;
      border: 1px solid var(--bsm-color-border);
      background: linear-gradient(180deg, #ffffff 0%, var(--bsm-color-surface) 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .page-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .quote-shell {
      display: grid;
      gap: 1.25rem;
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 700;
    }

    h2 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    .subtitle {
      margin: 0.45rem 0 0;
      color: var(--bsm-color-text-muted);
      max-width: 62ch;
    }

    .quote-meta {
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
      color: var(--bsm-color-primary);
      font-weight: 700;
    }

    .action-secondary {
      color: var(--bsm-color-text-muted);
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
      color: var(--bsm-color-text-muted);
    }

    dd {
      margin: 0;
      color: var(--bsm-color-text-brand);
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
export class TreatmentQuotePageComponent implements OnInit {
  private readonly authService = inject(AuthService);

  readonly treatmentQuotesFacade = inject(TreatmentQuotesFacade);
  readonly treatmentPlansFacade = inject(TreatmentPlansFacade);
  readonly patientsFacade = inject(PatientsFacade);
  readonly canWrite = this.authService.hasPermissions(['treatmentquote.write']);
  readonly canReadBillingDocuments = this.authService.hasPermissions(['billing.read']);
  private readonly i18n = inject(I18nService);

  private readonly route = inject(ActivatedRoute);

  patientId: string | null = null;
  creatingQuote = false;
  savingItemId: string | null = null;
  savingStatus = false;
  actionError: string | null = null;

  get patientDisplayName(): string {
    return this.patientsFacade.currentPatient()?.fullName ?? this.i18n.translate('this patient');
  }

  get canEditQuote(): boolean {
    const treatmentQuote = this.treatmentQuotesFacade.currentTreatmentQuote();
    return !!treatmentQuote && this.canWrite && treatmentQuote.status !== 'Accepted';
  }

  get availableStatuses(): TreatmentQuoteStatus[] {
    const treatmentQuote = this.treatmentQuotesFacade.currentTreatmentQuote();
    if (!treatmentQuote) {
      return ['Draft'];
    }

    switch (treatmentQuote.status) {
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
      this.treatmentQuotesFacade.clearTreatmentQuote();
      this.patientsFacade.clearCurrentPatient();
      return;
    }

    this.patientsFacade.clearCurrentPatient();
    this.patientsFacade.loadPatient(this.patientId);
    this.treatmentPlansFacade.clearTreatmentPlan();
    this.treatmentPlansFacade.loadTreatmentPlan(this.patientId);
    this.treatmentQuotesFacade.clearTreatmentQuote();
    this.treatmentQuotesFacade.loadTreatmentQuote(this.patientId);
  }

  createTreatmentQuote(): void {
    if (!this.patientId) {
      return;
    }

    this.creatingQuote = true;
    this.actionError = null;

    this.treatmentQuotesFacade.createTreatmentQuote(this.patientId)
      .subscribe({
        next: () => {
          this.creatingQuote = false;
        },
        error: () => {
          this.creatingQuote = false;
          this.actionError = 'The treatment quote could not be created.';
        }
      });
  }

  updateItemPrice(quoteItemId: string, unitPrice: number): void {
    if (!this.patientId) {
      return;
    }

    this.savingItemId = quoteItemId;
    this.actionError = null;

    this.treatmentQuotesFacade.updateItemPrice(this.patientId, quoteItemId, { unitPrice })
      .subscribe({
        next: () => {
          this.savingItemId = null;
        },
        error: () => {
          this.savingItemId = null;
          this.actionError = 'The quote item price could not be updated.';
        }
      });
  }

  changeStatus(payload: ChangeTreatmentQuoteStatusRequest): void {
    if (!this.patientId) {
      return;
    }

    this.savingStatus = true;
    this.actionError = null;

    this.treatmentQuotesFacade.changeStatus(this.patientId, payload)
      .subscribe({
        next: () => {
          this.savingStatus = false;
        },
        error: () => {
          this.savingStatus = false;
          this.actionError = 'The treatment quote status could not be updated.';
        }
      });
  }
}
