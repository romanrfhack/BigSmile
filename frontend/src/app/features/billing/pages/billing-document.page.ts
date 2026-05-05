import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { TreatmentQuotesFacade } from '../../treatments/facades/treatment-quotes.facade';
import { BillingDocumentEmptyStateComponent } from '../components/billing-document-empty-state.component';
import { BillingDocumentItemsListComponent } from '../components/billing-document-items-list.component';
import { BillingDocumentNoQuoteStateComponent } from '../components/billing-document-no-quote-state.component';
import { BillingDocumentQuoteNotAcceptedStateComponent } from '../components/billing-document-quote-not-accepted-state.component';
import { BillingDocumentStatusEditorComponent } from '../components/billing-document-status-editor.component';
import { BillingDocumentsFacade } from '../facades/billing-documents.facade';

@Component({
  selector: 'app-billing-document-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    BillingDocumentEmptyStateComponent,
    BillingDocumentItemsListComponent,
    BillingDocumentNoQuoteStateComponent,
    BillingDocumentQuoteNotAcceptedStateComponent,
    BillingDocumentStatusEditorComponent,
    LocalizedDatePipe,
    TranslatePipe
  ],
  template: `
    <section class="billing-document-page">
      <header class="page-head">
        <div>
          <p class="eyebrow">{{ 'Release' | t }} 6.1 / {{ 'Billing Foundation' | t }}</p>
          <h2>{{ 'Billing' | t }}</h2>
          <p class="subtitle">
            {{ 'Minimal patient-scoped billing document for {patientDisplayName} with explicit snapshot creation from an accepted quote, basic line totals, explicit issue, and read-only behavior once issued.' | t:{ patientDisplayName } }}
          </p>
        </div>

        <div class="head-actions">
          <a *ngIf="patientId" [routerLink]="['/patients', patientId]" class="action-link action-secondary">{{ 'Back to patient' | t }}</a>
          <a *ngIf="patientId" [routerLink]="['/patients', patientId, 'treatment-plan', 'quote']" class="action-link action-secondary">{{ 'Quote' | t }}</a>
        </div>
      </header>

      <div *ngIf="patientsFacade.loadingPatient() || treatmentQuotesFacade.loadingTreatmentQuote() || billingDocumentsFacade.loadingBillingDocument()" class="state-card">
        {{ 'Loading billing document...' | t }}
      </div>

      <div *ngIf="patientsFacade.detailError()" class="state-card state-error">
        {{ patientsFacade.detailError() | t }}
      </div>

      <div *ngIf="treatmentQuotesFacade.treatmentQuoteError()" class="state-card state-error">
        {{ treatmentQuotesFacade.treatmentQuoteError() | t }}
      </div>

      <div *ngIf="billingDocumentsFacade.billingDocumentError()" class="state-card state-error">
        {{ billingDocumentsFacade.billingDocumentError() | t }}
      </div>

      <div *ngIf="actionError" class="state-card state-error">
        {{ actionError | t }}
      </div>

      <app-billing-document-no-quote-state
        *ngIf="!treatmentQuotesFacade.loadingTreatmentQuote()
          && treatmentQuotesFacade.treatmentQuoteMissing()
          && !treatmentQuotesFacade.treatmentQuoteError()"
        [patientId]="patientId">
      </app-billing-document-no-quote-state>

      <app-billing-document-quote-not-accepted-state
        *ngIf="!treatmentQuotesFacade.loadingTreatmentQuote()
          && !billingDocumentsFacade.loadingBillingDocument()
          && !!treatmentQuotesFacade.currentTreatmentQuote()
          && treatmentQuotesFacade.currentTreatmentQuote()?.status !== 'Accepted'
          && billingDocumentsFacade.billingDocumentMissing()
          && !billingDocumentsFacade.billingDocumentError()"
        [patientId]="patientId"
        [quoteStatus]="treatmentQuotesFacade.currentTreatmentQuote()?.status ?? 'Draft'">
      </app-billing-document-quote-not-accepted-state>

      <app-billing-document-empty-state
        *ngIf="!treatmentQuotesFacade.loadingTreatmentQuote()
          && !billingDocumentsFacade.loadingBillingDocument()
          && !!treatmentQuotesFacade.currentTreatmentQuote()
          && treatmentQuotesFacade.currentTreatmentQuote()?.status === 'Accepted'
          && billingDocumentsFacade.billingDocumentMissing()
          && !billingDocumentsFacade.billingDocumentError()"
        [canWrite]="canWrite"
        [creating]="creatingBillingDocument"
        (createRequested)="createBillingDocument()">
      </app-billing-document-empty-state>

      <article *ngIf="billingDocumentsFacade.currentBillingDocument() as billingDocument" class="billing-shell">
        <section class="billing-meta">
          <div>
            <dt>{{ 'Status' | t }}</dt>
            <dd>{{ billingDocument.status | t }}</dd>
          </div>
          <div>
            <dt>{{ 'Items' | t }}</dt>
            <dd>{{ billingDocument.items.length }}</dd>
          </div>
          <div>
            <dt>{{ 'Currency' | t }}</dt>
            <dd>{{ billingDocument.currencyCode }}</dd>
          </div>
          <div>
            <dt>{{ 'Total' | t }}</dt>
            <dd>{{ billingDocument.totalAmount | number: '1.2-2' }} {{ billingDocument.currencyCode }}</dd>
          </div>
          <div>
            <dt>{{ 'Created' | t }}</dt>
            <dd>{{ billingDocument.createdAtUtc | bsDate: 'medium' }}</dd>
          </div>
          <div>
            <dt>{{ 'Created by' | t }}</dt>
            <dd>{{ billingDocument.createdByUserId }}</dd>
          </div>
          <div>
            <dt>{{ 'Last updated' | t }}</dt>
            <dd>{{ billingDocument.lastUpdatedAtUtc | bsDate: 'medium' }}</dd>
          </div>
          <div>
            <dt>{{ 'Updated by' | t }}</dt>
            <dd>{{ billingDocument.lastUpdatedByUserId }}</dd>
          </div>
          <div>
            <dt>{{ 'Issued' | t }}</dt>
            <dd>{{ billingDocument.issuedAtUtc ? (billingDocument.issuedAtUtc | bsDate: 'medium') : ('Not issued yet' | t) }}</dd>
          </div>
          <div>
            <dt>{{ 'Issued by' | t }}</dt>
            <dd>{{ billingDocument.issuedByUserId || ('Not issued yet' | t) }}</dd>
          </div>
        </section>

        <app-billing-document-status-editor
          [currentStatus]="billingDocument.status"
          [canWrite]="canWrite"
          [saving]="savingStatus"
          (issueRequested)="issueBillingDocument()">
        </app-billing-document-status-editor>

        <app-billing-document-items-list
          [items]="billingDocument.items"
          [currencyCode]="billingDocument.currencyCode">
        </app-billing-document-items-list>
      </article>
    </section>
  `,
  styles: [`
    .billing-document-page {
      display: grid;
      gap: 1.25rem;
    }

    .page-head,
    .state-card,
    .billing-meta {
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

    .billing-shell {
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

    .billing-meta {
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
export class BillingDocumentPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly billingDocumentsFacade = inject(BillingDocumentsFacade);
  readonly treatmentQuotesFacade = inject(TreatmentQuotesFacade);
  readonly patientsFacade = inject(PatientsFacade);
  readonly canWrite = this.authService.hasPermissions(['billing.write']);
  private readonly i18n = inject(I18nService);

  patientId: string | null = null;
  creatingBillingDocument = false;
  savingStatus = false;
  actionError: string | null = null;

  get patientDisplayName(): string {
    return this.patientsFacade.currentPatient()?.fullName ?? this.i18n.translate('this patient');
  }

  ngOnInit(): void {
    this.patientId = this.route.snapshot.paramMap.get('id');
    if (!this.patientId) {
      this.billingDocumentsFacade.clearBillingDocument();
      this.treatmentQuotesFacade.clearTreatmentQuote();
      this.patientsFacade.clearCurrentPatient();
      return;
    }

    this.patientsFacade.clearCurrentPatient();
    this.patientsFacade.loadPatient(this.patientId);
    this.treatmentQuotesFacade.clearTreatmentQuote();
    this.treatmentQuotesFacade.loadTreatmentQuote(this.patientId);
    this.billingDocumentsFacade.clearBillingDocument();
    this.billingDocumentsFacade.loadBillingDocument(this.patientId);
  }

  createBillingDocument(): void {
    if (!this.patientId) {
      return;
    }

    this.creatingBillingDocument = true;
    this.actionError = null;

    this.billingDocumentsFacade.createBillingDocument(this.patientId)
      .subscribe({
        next: () => {
          this.creatingBillingDocument = false;
        },
        error: () => {
          this.creatingBillingDocument = false;
          this.actionError = 'The billing document could not be created.';
        }
      });
  }

  issueBillingDocument(): void {
    if (!this.patientId) {
      return;
    }

    this.savingStatus = true;
    this.actionError = null;

    this.billingDocumentsFacade.changeStatus(this.patientId, { status: 'Issued' })
      .subscribe({
        next: () => {
          this.savingStatus = false;
        },
        error: () => {
          this.savingStatus = false;
          this.actionError = 'The billing document could not be issued.';
        }
      });
  }
}
