import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { TreatmentQuotesFacade } from '../../treatments/facades/treatment-quotes.facade';
import { BillingDocumentsFacade } from '../facades/billing-documents.facade';
import { BillingDocumentPageComponent } from './billing-document.page';

describe('BillingDocumentPageComponent', () => {
  let treatmentQuotesFacade: any;
  let billingDocumentsFacade: any;
  let createCalls: string[];
  let statusCalls: string[];

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    createCalls = [];
    statusCalls = [];

    treatmentQuotesFacade = {
      currentTreatmentQuote: signal<any | null>(null),
      loadingTreatmentQuote: signal(false),
      treatmentQuoteMissing: signal(true),
      treatmentQuoteError: signal<string | null>(null),
      clearTreatmentQuote: () => undefined,
      loadTreatmentQuote: () => undefined
    };

    billingDocumentsFacade = {
      currentBillingDocument: signal<any | null>(null),
      loadingBillingDocument: signal(false),
      billingDocumentMissing: signal(true),
      billingDocumentError: signal<string | null>(null),
      clearBillingDocument: () => undefined,
      loadBillingDocument: () => undefined,
      createBillingDocument: (patientId: string) => {
        createCalls.push(patientId);
        billingDocumentsFacade.currentBillingDocument.set(buildBillingDocument(patientId, 'Draft'));
        billingDocumentsFacade.billingDocumentMissing.set(false);
        return of(billingDocumentsFacade.currentBillingDocument());
      },
      changeStatus: (patientId: string, payload: { status: string }) => {
        statusCalls.push(`${patientId}:${payload.status}`);
        const currentBillingDocument = billingDocumentsFacade.currentBillingDocument();
        billingDocumentsFacade.currentBillingDocument.set({
          ...currentBillingDocument,
          status: payload.status,
          issuedAtUtc: '2026-04-22T08:45:00Z',
          issuedByUserId: 'user-1'
        });
        return of(billingDocumentsFacade.currentBillingDocument());
      }
    };

    await TestBed.configureTestingModule({
      imports: [BillingDocumentPageComponent],
      providers: [
        {
          provide: TreatmentQuotesFacade,
          useValue: treatmentQuotesFacade
        },
        {
          provide: BillingDocumentsFacade,
          useValue: billingDocumentsFacade
        },
        {
          provide: PatientsFacade,
          useValue: {
            currentPatient: signal({
              id: 'patient-1',
              fullName: 'Ana Lopez',
              dateOfBirth: '1991-02-14',
              primaryPhone: null,
              email: null,
              isActive: true,
              hasClinicalAlerts: false,
              clinicalAlertsSummary: null,
              responsibleParty: null,
              createdAt: '2026-04-10T10:00:00Z',
              updatedAt: null
            }),
            loadingPatient: signal(false),
            detailError: signal<string | null>(null),
            clearCurrentPatient: () => undefined,
            loadPatient: () => undefined
          }
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => 'patient-1'
              }
            }
          }
        },
        {
          provide: AuthService,
          useValue: {
            hasPermissions: () => true
          }
        }
      ]
    }).compileComponents();
  });

  it('renders the no-quote message when the patient does not have a quote yet', () => {
    const fixture = TestBed.createComponent(BillingDocumentPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Accepted quote required first');
    expect(fixture.nativeElement.textContent).toContain('never auto-creates billing');
  });

  it('renders the quote-not-accepted message when the quote exists but is not accepted', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Draft'));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);

    const fixture = TestBed.createComponent(BillingDocumentPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Quote must be accepted before billing');
    expect(fixture.nativeElement.textContent).toContain('Draft');
  });

  it('renders the explicit empty state when the accepted quote exists but billing does not', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Accepted'));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);

    const fixture = TestBed.createComponent(BillingDocumentPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No billing document yet');
    expect(fixture.nativeElement.textContent).toContain('never auto-created');
  });

  it('creates the billing document only through the explicit create flow', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Accepted'));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);

    const fixture = TestBed.createComponent(BillingDocumentPageComponent);
    fixture.detectChanges();

    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Create billing document from accepted quote')) as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(createCalls).toEqual(['patient-1']);
    expect(fixture.nativeElement.textContent).toContain('Billing lines');
  });

  it('renders billing lines and total for an existing billing document', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Accepted'));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);
    billingDocumentsFacade.currentBillingDocument.set(buildBillingDocument('patient-1', 'Draft'));
    billingDocumentsFacade.billingDocumentMissing.set(false);

    const fixture = TestBed.createComponent(BillingDocumentPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Composite restoration');
    expect(fixture.nativeElement.textContent).toContain('450.00 MXN');
  });

  it('issues the billing document through the explicit issue flow', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Accepted'));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);
    billingDocumentsFacade.currentBillingDocument.set(buildBillingDocument('patient-1', 'Draft'));
    billingDocumentsFacade.billingDocumentMissing.set(false);

    const fixture = TestBed.createComponent(BillingDocumentPageComponent);
    fixture.detectChanges();

    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Issue billing document')) as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(statusCalls).toEqual(['patient-1:Issued']);
    expect(fixture.nativeElement.textContent).toContain('Issued');
  });

  it('blocks issue actions once the billing document is issued', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Accepted'));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);
    billingDocumentsFacade.currentBillingDocument.set(buildBillingDocument('patient-1', 'Issued'));
    billingDocumentsFacade.billingDocumentMissing.set(false);

    const fixture = TestBed.createComponent(BillingDocumentPageComponent);
    fixture.detectChanges();

    const issueButton = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Issue billing document'));

    expect(issueButton).toBeUndefined();
    expect(fixture.nativeElement.textContent).toContain('Issued billing documents are read-only in this slice');
  });

  function buildQuote(patientId: string, status: string) {
    return {
      treatmentQuoteId: 'quote-1',
      patientId,
      treatmentPlanId: 'plan-1',
      status,
      currencyCode: 'MXN',
      total: 450,
      items: [
        {
          quoteItemId: 'quote-item-1',
          sourceTreatmentPlanItemId: 'plan-item-1',
          title: 'Composite restoration',
          category: 'Restorative',
          quantity: 1,
          notes: null,
          toothCode: '16',
          surfaceCode: 'O',
          unitPrice: 450,
          lineTotal: 450,
          createdAtUtc: '2026-04-22T08:00:00Z',
          createdByUserId: 'user-1'
        }
      ],
      createdAtUtc: '2026-04-22T08:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-22T08:00:00Z',
      lastUpdatedByUserId: 'user-1'
    };
  }

  function buildBillingDocument(patientId: string, status: string) {
    return {
      billingDocumentId: 'billing-1',
      patientId,
      treatmentQuoteId: 'quote-1',
      status,
      currencyCode: 'MXN',
      totalAmount: 450,
      items: [
        {
          billingItemId: 'billing-item-1',
          sourceTreatmentQuoteItemId: 'quote-item-1',
          title: 'Composite restoration',
          category: 'Restorative',
          quantity: 1,
          notes: null,
          toothCode: '16',
          surfaceCode: 'O',
          unitPrice: 450,
          lineTotal: 450,
          createdAtUtc: '2026-04-22T08:05:00Z',
          createdByUserId: 'user-1'
        }
      ],
      createdAtUtc: '2026-04-22T08:05:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-22T08:05:00Z',
      lastUpdatedByUserId: 'user-1',
      issuedAtUtc: status === 'Issued' ? '2026-04-22T08:45:00Z' : null,
      issuedByUserId: status === 'Issued' ? 'user-1' : null
    };
  }
});
