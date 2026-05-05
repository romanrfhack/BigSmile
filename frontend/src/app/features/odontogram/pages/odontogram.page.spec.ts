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
  let updateSurfaceCalls: Array<{ patientId: string; toothCode: string; surfaceCode: string; status: string }>;
  let addFindingCalls: Array<{ patientId: string; toothCode: string; surfaceCode: string; findingType: string }>;
  let removeFindingCalls: Array<{ patientId: string; toothCode: string; surfaceCode: string; findingId: string }>;

  function buildFinding(findingId: string, findingType = 'Caries') {
    return {
      findingId,
      findingType,
      createdAtUtc: '2026-04-20T10:15:00Z',
      createdByUserId: 'user-1'
    };
  }

  function buildHistoryEntry(
    entryType: 'FindingAdded' | 'FindingRemoved',
    toothCode: string,
    surfaceCode: string,
    findingType = 'Caries',
    changedAtUtc = '2026-04-20T12:00:00Z',
    changedByUserId = 'user-2',
    referenceFindingId: string | null = null
  ) {
    return {
      entryType,
      toothCode,
      surfaceCode,
      findingType,
      changedAtUtc,
      changedByUserId,
      summary: entryType === 'FindingAdded' ? 'Finding added' : 'Finding removed',
      referenceFindingId
    };
  }

  function buildSurface(surfaceCode: string, status = 'Unknown', findings: any[] = []) {
    return {
      surfaceCode,
      status,
      updatedAtUtc: '2026-04-20T10:00:00Z',
      updatedByUserId: 'user-1',
      findings
    };
  }

  function buildTooth(toothCode: string, status = 'Unknown', surfaceOverrides: Partial<Record<string, any>> = {}) {
    return {
      toothCode,
      status,
      updatedAtUtc: '2026-04-20T10:00:00Z',
      updatedByUserId: 'user-1',
      surfaces: [
        { ...buildSurface('O'), ...surfaceOverrides['O'] },
        { ...buildSurface('M'), ...surfaceOverrides['M'] },
        { ...buildSurface('D'), ...surfaceOverrides['D'] },
        { ...buildSurface('B'), ...surfaceOverrides['B'] },
        { ...buildSurface('L'), ...surfaceOverrides['L'] }
      ]
    };
  }

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    createCalls = [];
    updateCalls = [];
    updateSurfaceCalls = [];
    addFindingCalls = [];
    removeFindingCalls = [];

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
            buildTooth('11'),
            buildTooth('12')
          ],
          findingsHistory: [],
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
      },
      updateSurfaceStatus: (patientId: string, toothCode: string, surfaceCode: string, payload: any) => {
        updateSurfaceCalls.push({ patientId, toothCode, surfaceCode, status: payload.status });
        odontogramsFacade.currentOdontogram.set({
          ...odontogramsFacade.currentOdontogram(),
          teeth: (odontogramsFacade.currentOdontogram()?.teeth ?? []).map((tooth: any) => tooth.toothCode === toothCode
            ? {
                ...tooth,
                surfaces: (tooth.surfaces ?? []).map((surface: any) => surface.surfaceCode === surfaceCode
                  ? {
                      ...surface,
                      status: payload.status,
                      updatedAtUtc: '2026-04-20T11:30:00Z',
                      updatedByUserId: 'user-2'
                    }
                  : surface)
              }
            : tooth),
          lastUpdatedAtUtc: '2026-04-20T11:30:00Z',
          lastUpdatedByUserId: 'user-2'
        });
        return of(odontogramsFacade.currentOdontogram());
      },
      addSurfaceFinding: (patientId: string, toothCode: string, surfaceCode: string, payload: any) => {
        addFindingCalls.push({ patientId, toothCode, surfaceCode, findingType: payload.findingType });
        const createdFindingId = `finding-${payload.findingType}`;
        odontogramsFacade.currentOdontogram.set({
          ...odontogramsFacade.currentOdontogram(),
          teeth: (odontogramsFacade.currentOdontogram()?.teeth ?? []).map((tooth: any) => tooth.toothCode === toothCode
            ? {
                ...tooth,
                surfaces: (tooth.surfaces ?? []).map((surface: any) => surface.surfaceCode === surfaceCode
                  ? {
                      ...surface,
                      findings: [
                        {
                          findingId: createdFindingId,
                          findingType: payload.findingType,
                          createdAtUtc: '2026-04-20T12:00:00Z',
                          createdByUserId: 'user-2'
                        },
                        ...(surface.findings ?? [])
                      ]
                    }
                  : surface)
              }
            : tooth),
          findingsHistory: [
            buildHistoryEntry('FindingAdded', toothCode, surfaceCode, payload.findingType, '2026-04-20T12:00:00Z', 'user-2', createdFindingId),
            ...(odontogramsFacade.currentOdontogram()?.findingsHistory ?? [])
          ],
          lastUpdatedAtUtc: '2026-04-20T12:00:00Z',
          lastUpdatedByUserId: 'user-2'
        });
        return of(odontogramsFacade.currentOdontogram());
      },
      removeSurfaceFinding: (patientId: string, toothCode: string, surfaceCode: string, findingId: string) => {
        removeFindingCalls.push({ patientId, toothCode, surfaceCode, findingId });
        const findingType = odontogramsFacade.currentOdontogram()?.teeth
          ?.find((tooth: any) => tooth.toothCode === toothCode)
          ?.surfaces?.find((surface: any) => surface.surfaceCode === surfaceCode)
          ?.findings?.find((finding: any) => finding.findingId === findingId)
          ?.findingType ?? 'Caries';

        odontogramsFacade.currentOdontogram.set({
          ...odontogramsFacade.currentOdontogram(),
          teeth: (odontogramsFacade.currentOdontogram()?.teeth ?? []).map((tooth: any) => tooth.toothCode === toothCode
            ? {
                ...tooth,
                surfaces: (tooth.surfaces ?? []).map((surface: any) => surface.surfaceCode === surfaceCode
                  ? {
                      ...surface,
                      findings: (surface.findings ?? []).filter((finding: any) => finding.findingId !== findingId)
                    }
                  : surface)
              }
            : tooth),
          findingsHistory: [
            buildHistoryEntry('FindingRemoved', toothCode, surfaceCode, findingType, '2026-04-20T12:15:00Z', 'user-2', findingId),
            ...(odontogramsFacade.currentOdontogram()?.findingsHistory ?? [])
          ],
          lastUpdatedAtUtc: '2026-04-20T12:15:00Z',
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
        buildTooth('11', 'Healthy'),
        buildTooth('12', 'Restored')
      ],
      findingsHistory: [],
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
    expect(fixture.nativeElement.textContent).toContain('findings history stays separate from any future dental timeline');
  });

  it('updates the selected tooth state through the facade', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11')
      ],
      findingsHistory: [],
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

  it('renders tooth surface detail for the selected tooth', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11', 'Healthy')
      ],
      findingsHistory: [],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Surfaces');
    expect(fixture.nativeElement.textContent).toContain('Current detail for tooth 11');
    expect(fixture.nativeElement.textContent).toContain('O');
    expect(fixture.nativeElement.textContent).toContain('M');
  });

  it('renders findings for the selected surface', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11', 'Healthy', {
          O: { findings: [buildFinding('finding-caries', 'Caries')] }
        })
      ],
      findingsHistory: [],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Basic findings');
    expect(fixture.nativeElement.textContent).toContain('Current findings for surface O');
    expect(fixture.nativeElement.textContent).toContain('Caries');
  });

  it('renders finding history for the selected surface only', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11', 'Healthy', {
          O: { findings: [buildFinding('finding-caries', 'Caries')] }
        })
      ],
      findingsHistory: [
        buildHistoryEntry('FindingRemoved', '11', 'O', 'Caries', '2026-04-20T12:00:00Z', 'user-2', 'finding-caries'),
        buildHistoryEntry('FindingAdded', '11', 'O', 'Caries', '2026-04-20T11:00:00Z', 'user-1', 'finding-caries'),
        buildHistoryEntry('FindingAdded', '11', 'M', 'Sealant', '2026-04-20T10:00:00Z', 'user-3', 'finding-sealant')
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T12:00:00Z',
      lastUpdatedByUserId: 'user-2'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Findings history');
    expect(fixture.nativeElement.textContent).toContain('Current history for surface O');
    expect(fixture.nativeElement.textContent).toContain('Finding removed');
    expect(fixture.nativeElement.textContent).toContain('Finding added');
    expect(fixture.nativeElement.textContent).not.toContain('finding-sealant');
  });

  it('updates the selected surface state through the facade', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11')
      ],
      findingsHistory: [],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.updateSurfaceStatus({ surfaceCode: 'O', status: 'Caries' });

    expect(updateSurfaceCalls).toEqual([{ patientId: 'patient-1', toothCode: '11', surfaceCode: 'O', status: 'Caries' }]);
    expect(odontogramsFacade.currentOdontogram()?.teeth[0]?.surfaces[0]?.status).toBe('Caries');
  });

  it('adds a finding through the facade', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11')
      ],
      findingsHistory: [],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.addSurfaceFinding({ surfaceCode: 'O', findingType: 'Sealant' });
    fixture.detectChanges();

    expect(addFindingCalls).toEqual([{ patientId: 'patient-1', toothCode: '11', surfaceCode: 'O', findingType: 'Sealant' }]);
    expect(odontogramsFacade.currentOdontogram()?.teeth[0]?.surfaces[0]?.findings[0]?.findingType).toBe('Sealant');
    expect(odontogramsFacade.currentOdontogram()?.findingsHistory[0]?.entryType).toBe('FindingAdded');
    expect(fixture.nativeElement.textContent).toContain('Finding added');
  });

  it('removes a finding through the facade', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11', 'Unknown', {
          O: { findings: [buildFinding('finding-caries', 'Caries')] }
        })
      ],
      findingsHistory: [
        buildHistoryEntry('FindingAdded', '11', 'O', 'Caries', '2026-04-20T10:05:00Z', 'user-1', 'finding-caries')
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.removeSurfaceFinding({ surfaceCode: 'O', findingId: 'finding-caries' });
    fixture.detectChanges();

    expect(removeFindingCalls).toEqual([{ patientId: 'patient-1', toothCode: '11', surfaceCode: 'O', findingId: 'finding-caries' }]);
    expect(odontogramsFacade.currentOdontogram()?.teeth[0]?.surfaces[0]?.findings).toEqual([]);
    expect(odontogramsFacade.currentOdontogram()?.findingsHistory[0]?.entryType).toBe('FindingRemoved');
    expect(fixture.nativeElement.textContent).toContain('Finding removed');
  });
});
