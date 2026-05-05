import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { TreatmentPlansFacade } from '../facades/treatment-plans.facade';
import { TreatmentPlanPageComponent } from './treatment-plan.page';

describe('TreatmentPlanPageComponent', () => {
  let treatmentPlansFacade: any;
  let createCalls: string[];
  let addItemCalls: any[];
  let removeItemCalls: string[];
  let statusCalls: string[];

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    createCalls = [];
    addItemCalls = [];
    removeItemCalls = [];
    statusCalls = [];

    treatmentPlansFacade = {
      currentTreatmentPlan: signal<any | null>(null),
      loadingTreatmentPlan: signal(false),
      treatmentPlanMissing: signal(true),
      treatmentPlanError: signal<string | null>(null),
      clearTreatmentPlan: () => undefined,
      loadTreatmentPlan: () => undefined,
      createTreatmentPlan: (patientId: string) => {
        createCalls.push(patientId);
        treatmentPlansFacade.currentTreatmentPlan.set(buildPlan(patientId));
        treatmentPlansFacade.treatmentPlanMissing.set(false);
        return of(treatmentPlansFacade.currentTreatmentPlan());
      },
      addItem: (patientId: string, payload: any) => {
        addItemCalls.push({ patientId, payload });
        treatmentPlansFacade.currentTreatmentPlan.set({
          ...treatmentPlansFacade.currentTreatmentPlan(),
          items: [
            ...(treatmentPlansFacade.currentTreatmentPlan()?.items ?? []),
            {
              itemId: 'item-1',
              title: payload.title,
              category: payload.category,
              quantity: payload.quantity,
              notes: payload.notes,
              toothCode: payload.toothCode,
              surfaceCode: payload.surfaceCode,
              createdAtUtc: '2026-04-20T10:00:00Z',
              createdByUserId: 'user-1'
            }
          ]
        });
        return of(treatmentPlansFacade.currentTreatmentPlan());
      },
      removeItem: (patientId: string, itemId: string) => {
        removeItemCalls.push(`${patientId}:${itemId}`);
        treatmentPlansFacade.currentTreatmentPlan.set({
          ...treatmentPlansFacade.currentTreatmentPlan(),
          items: (treatmentPlansFacade.currentTreatmentPlan()?.items ?? []).filter((item: any) => item.itemId !== itemId)
        });
        return of(treatmentPlansFacade.currentTreatmentPlan());
      },
      changeStatus: (patientId: string, payload: any) => {
        statusCalls.push(`${patientId}:${payload.status}`);
        treatmentPlansFacade.currentTreatmentPlan.set({
          ...treatmentPlansFacade.currentTreatmentPlan(),
          status: payload.status
        });
        return of(treatmentPlansFacade.currentTreatmentPlan());
      }
    };

    await TestBed.configureTestingModule({
      imports: [TreatmentPlanPageComponent],
      providers: [
        {
          provide: TreatmentPlansFacade,
          useValue: treatmentPlansFacade
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

  it('renders the explicit empty state when the treatment plan does not exist', () => {
    const fixture = TestBed.createComponent(TreatmentPlanPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No treatment plan yet');
    expect(fixture.nativeElement.textContent).toContain('It is not auto-created');
  });

  it('creates the treatment plan only through the explicit create flow', () => {
    const fixture = TestBed.createComponent(TreatmentPlanPageComponent);
    fixture.detectChanges();

    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Create treatment plan')) as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(createCalls).toEqual(['patient-1']);
    expect(fixture.nativeElement.textContent).toContain('Treatment plan items');
  });

  it('adds an item through the minimal item form', () => {
    treatmentPlansFacade.currentTreatmentPlan.set(buildPlan('patient-1'));
    treatmentPlansFacade.treatmentPlanMissing.set(false);

    const fixture = TestBed.createComponent(TreatmentPlanPageComponent);
    fixture.detectChanges();

    const titleInput = fixture.nativeElement.querySelector('input[formcontrolname="title"]') as HTMLInputElement;
    const quantityInput = fixture.nativeElement.querySelector('input[formcontrolname="quantity"]') as HTMLInputElement;
    titleInput.value = 'Composite restoration';
    titleInput.dispatchEvent(new Event('input'));
    quantityInput.value = '2';
    quantityInput.dispatchEvent(new Event('input'));

    const form = fixture.nativeElement.querySelector('form.item-form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(addItemCalls).toHaveLength(1);
    expect(addItemCalls[0].patientId).toBe('patient-1');
    expect(addItemCalls[0].payload.title).toBe('Composite restoration');
    expect(fixture.nativeElement.textContent).toContain('Composite restoration');
  });

  it('removes an item from the plan', () => {
    treatmentPlansFacade.currentTreatmentPlan.set(buildPlan('patient-1', [
      {
        itemId: 'item-1',
        title: 'Composite restoration',
        category: 'Restorative',
        quantity: 1,
        notes: null,
        toothCode: '11',
        surfaceCode: 'O',
        createdAtUtc: '2026-04-20T10:00:00Z',
        createdByUserId: 'user-1'
      }
    ]));
    treatmentPlansFacade.treatmentPlanMissing.set(false);

    const fixture = TestBed.createComponent(TreatmentPlanPageComponent);
    fixture.detectChanges();

    const button = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Remove')) as HTMLButtonElement;
    button.click();
    fixture.detectChanges();

    expect(removeItemCalls).toEqual(['patient-1:item-1']);
    expect(fixture.nativeElement.textContent).toContain('No items have been added yet');
  });

  it('changes the plan status through the minimal status editor', () => {
    treatmentPlansFacade.currentTreatmentPlan.set(buildPlan('patient-1'));
    treatmentPlansFacade.treatmentPlanMissing.set(false);

    const fixture = TestBed.createComponent(TreatmentPlanPageComponent);
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

  function buildPlan(patientId: string, items: any[] = []) {
    return {
      treatmentPlanId: 'plan-1',
      patientId,
      status: 'Draft',
      items,
      createdAtUtc: '2026-04-20T09:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T09:00:00Z',
      lastUpdatedByUserId: 'user-1'
    };
  }
});
