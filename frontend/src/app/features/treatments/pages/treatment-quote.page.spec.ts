import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { TreatmentPlansFacade } from '../facades/treatment-plans.facade';
import { TreatmentQuotesFacade } from '../facades/treatment-quotes.facade';
import { TreatmentQuotePageComponent } from './treatment-quote.page';

describe('TreatmentQuotePageComponent', () => {
  let treatmentPlansFacade: any;
  let treatmentQuotesFacade: any;
  let createCalls: string[];
  let priceCalls: Array<{ patientId: string; quoteItemId: string; unitPrice: number }>;
  let statusCalls: string[];

  beforeEach(async () => {
    createCalls = [];
    priceCalls = [];
    statusCalls = [];

    treatmentPlansFacade = {
      currentTreatmentPlan: signal<any | null>(buildPlan('patient-1')),
      loadingTreatmentPlan: signal(false),
      treatmentPlanMissing: signal(false),
      treatmentPlanError: signal<string | null>(null),
      clearTreatmentPlan: () => undefined,
      loadTreatmentPlan: () => undefined
    };

    treatmentQuotesFacade = {
      currentTreatmentQuote: signal<any | null>(null),
      loadingTreatmentQuote: signal(false),
      treatmentQuoteMissing: signal(true),
      treatmentQuoteError: signal<string | null>(null),
      clearTreatmentQuote: () => undefined,
      loadTreatmentQuote: () => undefined,
      createTreatmentQuote: (patientId: string) => {
        createCalls.push(patientId);
        treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote(patientId, 'Draft', 0));
        treatmentQuotesFacade.treatmentQuoteMissing.set(false);
        return of(treatmentQuotesFacade.currentTreatmentQuote());
      },
      updateItemPrice: (patientId: string, quoteItemId: string, payload: { unitPrice: number }) => {
        priceCalls.push({ patientId, quoteItemId, unitPrice: payload.unitPrice });
        treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote(patientId, treatmentQuotesFacade.currentTreatmentQuote()?.status ?? 'Draft', payload.unitPrice));
        return of(treatmentQuotesFacade.currentTreatmentQuote());
      },
      changeStatus: (patientId: string, payload: { status: string }) => {
        statusCalls.push(`${patientId}:${payload.status}`);
        const currentQuote = treatmentQuotesFacade.currentTreatmentQuote();
        treatmentQuotesFacade.currentTreatmentQuote.set({
          ...currentQuote,
          status: payload.status
        });
        return of(treatmentQuotesFacade.currentTreatmentQuote());
      }
    };

    await TestBed.configureTestingModule({
      imports: [TreatmentQuotePageComponent],
      providers: [
        {
          provide: TreatmentPlansFacade,
          useValue: treatmentPlansFacade
        },
        {
          provide: TreatmentQuotesFacade,
          useValue: treatmentQuotesFacade
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

  it('renders the explicit empty state when the quote does not exist but the treatment plan does', () => {
    const fixture = TestBed.createComponent(TreatmentQuotePageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No quote yet');
    expect(fixture.nativeElement.textContent).toContain('not auto-created');
  });

  it('renders the no-plan message when the patient does not have a treatment plan yet', () => {
    treatmentPlansFacade.currentTreatmentPlan.set(null);
    treatmentPlansFacade.treatmentPlanMissing.set(true);
    treatmentQuotesFacade.currentTreatmentQuote.set(null);
    treatmentQuotesFacade.treatmentQuoteMissing.set(true);

    const fixture = TestBed.createComponent(TreatmentQuotePageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Treatment plan required first');
    expect(fixture.nativeElement.textContent).toContain('never auto-creates the quote or the treatment plan');
  });

  it('creates the quote only through the explicit create flow', () => {
    const fixture = TestBed.createComponent(TreatmentQuotePageComponent);
    fixture.detectChanges();

    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Create quote from treatment plan')) as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(createCalls).toEqual(['patient-1']);
    expect(fixture.nativeElement.textContent).toContain('Quote items');
  });

  it('updates a quote item unit price through the minimal price editor', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Draft', 0));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);

    const fixture = TestBed.createComponent(TreatmentQuotePageComponent);
    fixture.detectChanges();

    const priceInput = fixture.nativeElement.querySelector('input[formcontrolname="unitPrice"]') as HTMLInputElement;
    priceInput.value = '450';
    priceInput.dispatchEvent(new Event('input'));

    const form = fixture.nativeElement.querySelector('form.price-editor') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(priceCalls).toEqual([{ patientId: 'patient-1', quoteItemId: 'quote-item-1', unitPrice: 450 }]);
    expect(fixture.nativeElement.textContent).toContain('450.00 MXN');
  });

  it('changes the quote status through the minimal status editor', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Draft', 250));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);

    const fixture = TestBed.createComponent(TreatmentQuotePageComponent);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select[formcontrolname="status"]') as HTMLSelectElement;
    select.value = 'Proposed';
    select.dispatchEvent(new Event('change'));

    const statusForm = fixture.nativeElement.querySelector('form.status-shell') as HTMLFormElement;
    statusForm.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(statusCalls).toEqual(['patient-1:Proposed']);
    expect(fixture.nativeElement.textContent).toContain('Proposed');
  });

  it('blocks price editing when the quote is accepted', () => {
    treatmentQuotesFacade.currentTreatmentQuote.set(buildQuote('patient-1', 'Accepted', 300));
    treatmentQuotesFacade.treatmentQuoteMissing.set(false);

    const fixture = TestBed.createComponent(TreatmentQuotePageComponent);
    fixture.detectChanges();

    const priceButton = fixture.nativeElement.querySelector('form.price-editor button') as HTMLButtonElement;

    expect(priceButton.disabled).toBe(true);
    expect(fixture.nativeElement.textContent).toContain('Accepted quotes are read-only in this slice');
  });

  function buildPlan(patientId: string) {
    return {
      treatmentPlanId: 'plan-1',
      patientId,
      status: 'Draft',
      items: [
        {
          itemId: 'plan-item-1',
          title: 'Exam',
          category: 'Diagnostics',
          quantity: 1,
          notes: null,
          toothCode: null,
          surfaceCode: null,
          createdAtUtc: '2026-04-20T09:00:00Z',
          createdByUserId: 'user-1'
        }
      ],
      createdAtUtc: '2026-04-20T09:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T09:00:00Z',
      lastUpdatedByUserId: 'user-1'
    };
  }

  function buildQuote(patientId: string, status: string, unitPrice: number) {
    return {
      treatmentQuoteId: 'quote-1',
      patientId,
      treatmentPlanId: 'plan-1',
      status,
      currencyCode: 'MXN',
      total: unitPrice,
      items: [
        {
          quoteItemId: 'quote-item-1',
          sourceTreatmentPlanItemId: 'plan-item-1',
          title: 'Exam',
          category: 'Diagnostics',
          quantity: 1,
          notes: null,
          toothCode: null,
          surfaceCode: null,
          unitPrice,
          lineTotal: unitPrice,
          createdAtUtc: '2026-04-20T10:00:00Z',
          createdByUserId: 'user-1'
        }
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    };
  }
});
