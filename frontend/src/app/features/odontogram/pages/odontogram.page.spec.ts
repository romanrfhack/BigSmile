import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { OdontogramsFacade } from '../facades/odontograms.facade';
import { OdontogramPageComponent } from './odontogram.page';

describe('OdontogramPageComponent', () => {
  let odontogramsFacade: any;
  let patientsFacade: any;
  let createCalls: string[];
  let updateCalls: Array<{ patientId: string; toothCode: string; status: string }>;

  beforeEach(async () => {
    createCalls = [];
    updateCalls = [];

    odontogramsFacade = {
      currentOdontogram: signal<any | null>(null),
      loadingOdontogram: signal(false),
      odontogramMissing: signal(true),
      odontogramError: signal<string | null>(null),
      loadOdontogram: () => undefined,
      clearOdontogram: () => undefined,
      createOdontogram: (patientId: string) => {
        createCalls.push(patientId);
        odontogramsFacade.currentOdontogram.set({
          odontogramId: 'odontogram-1',
          patientId,
          teeth: [
            { toothCode: '11', status: 'Unknown', updatedAtUtc: '2026-04-20T10:00:00Z', updatedByUserId: 'user-1' },
            { toothCode: '12', status: 'Unknown', updatedAtUtc: '2026-04-20T10:00:00Z', updatedByUserId: 'user-1' }
          ],
          createdAtUtc: '2026-04-20T10:00:00Z',
          createdByUserId: 'user-1',
          lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
          lastUpdatedByUserId: 'user-1'
        });
        odontogramsFacade.odontogramMissing.set(false);
        return of(odontogramsFacade.currentOdontogram());
      },
      updateToothStatus: (patientId: string, toothCode: string, payload: any) => {
        updateCalls.push({ patientId, toothCode, status: payload.status });
        odontogramsFacade.currentOdontogram.set({
          ...odontogramsFacade.currentOdontogram(),
          teeth: (odontogramsFacade.currentOdontogram()?.teeth ?? []).map((tooth: any) => tooth.toothCode === toothCode
            ? {
                ...tooth,
                status: payload.status,
                updatedAtUtc: '2026-04-20T11:00:00Z',
                updatedByUserId: 'user-2'
              }
            : tooth),
          lastUpdatedAtUtc: '2026-04-20T11:00:00Z',
          lastUpdatedByUserId: 'user-2'
        });
        return of(odontogramsFacade.currentOdontogram());
      }
    };

    patientsFacade = {
      currentPatient: signal({
        id: 'patient-1',
        fullName: 'Ana Lopez'
      }),
      loadingPatient: signal(false),
      detailError: signal<string | null>(null),
      clearCurrentPatient: () => undefined,
      loadPatient: () => undefined
    };

    await TestBed.configureTestingModule({
      imports: [OdontogramPageComponent],
      providers: [
        { provide: OdontogramsFacade, useValue: odontogramsFacade },
        { provide: PatientsFacade, useValue: patientsFacade },
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

  it('renders the explicit empty state when the odontogram does not exist', () => {
    const fixture = TestBed.createComponent(OdontogramPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No odontogram yet');
    expect(fixture.nativeElement.textContent).toContain('not auto-created');
  });

  it('creates the odontogram only through the explicit create flow', () => {
    const fixture = TestBed.createComponent(OdontogramPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.createOdontogram();

    expect(createCalls).toEqual(['patient-1']);
    expect(component.selectedToothCode).toBe('11');
  });

  it('renders the odontogram grid once the odontogram exists', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        { toothCode: '11', status: 'Healthy', updatedAtUtc: '2026-04-20T10:00:00Z', updatedByUserId: 'user-1' },
        { toothCode: '12', status: 'Restored', updatedAtUtc: '2026-04-20T10:05:00Z', updatedByUserId: 'user-1' }
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:05:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('32 permanent adult teeth');
    expect(fixture.nativeElement.textContent).toContain('11');
    expect(fixture.nativeElement.textContent).toContain('Healthy');
    expect(fixture.nativeElement.textContent).toContain('Restored');
  });

  it('updates the selected tooth state through the facade', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        { toothCode: '11', status: 'Unknown', updatedAtUtc: '2026-04-20T10:00:00Z', updatedByUserId: 'user-1' }
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.updateToothStatus('Caries');

    expect(updateCalls).toEqual([{ patientId: 'patient-1', toothCode: '11', status: 'Caries' }]);
    expect(odontogramsFacade.currentOdontogram()?.teeth[0]?.status).toBe('Caries');
  });
});
