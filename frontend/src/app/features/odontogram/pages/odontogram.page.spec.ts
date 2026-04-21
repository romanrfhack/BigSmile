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
                          findingId: `finding-${payload.findingType}`,
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
          lastUpdatedAtUtc: '2026-04-20T12:00:00Z',
          lastUpdatedByUserId: 'user-2'
        });
        return of(odontogramsFacade.currentOdontogram());
      },
      removeSurfaceFinding: (patientId: string, toothCode: string, surfaceCode: string, findingId: string) => {
        removeFindingCalls.push({ patientId, toothCode, surfaceCode, findingId });
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
    expect(fixture.nativeElement.textContent).toContain('Basic findings stay separate');
  });

  it('updates the selected tooth state through the facade', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11')
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

  it('renders tooth surface detail for the selected tooth', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11', 'Healthy')
      ],
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

  it('updates the selected surface state through the facade', () => {
    odontogramsFacade.odontogramMissing.set(false);
    odontogramsFacade.currentOdontogram.set({
      odontogramId: 'odontogram-1',
      patientId: 'patient-1',
      teeth: [
        buildTooth('11')
      ],
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
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.addSurfaceFinding({ surfaceCode: 'O', findingType: 'Sealant' });

    expect(addFindingCalls).toEqual([{ patientId: 'patient-1', toothCode: '11', surfaceCode: 'O', findingType: 'Sealant' }]);
    expect(odontogramsFacade.currentOdontogram()?.teeth[0]?.surfaces[0]?.findings[0]?.findingType).toBe('Sealant');
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
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(OdontogramPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.removeSurfaceFinding({ surfaceCode: 'O', findingId: 'finding-caries' });

    expect(removeFindingCalls).toEqual([{ patientId: 'patient-1', toothCode: '11', surfaceCode: 'O', findingId: 'finding-caries' }]);
    expect(odontogramsFacade.currentOdontogram()?.teeth[0]?.surfaces[0]?.findings).toEqual([]);
  });
});
