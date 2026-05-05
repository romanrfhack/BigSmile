import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OdontogramGridComponent } from './odontogram-grid.component';
import {
  ODONTOGRAM_LOWER_ARCH_ORDER,
  ODONTOGRAM_UPPER_ARCH_ORDER
} from './odontogram-tooth-layout';
import {
  OdontogramSurfaceCode,
  OdontogramSurfaceFinding,
  OdontogramSurfaceState,
  OdontogramSurfaceStatus,
  OdontogramToothState,
  OdontogramToothStatus
} from '../models/odontogram.models';

describe('OdontogramGridComponent', () => {
  let fixture: ComponentFixture<OdontogramGridComponent>;
  let component: OdontogramGridComponent;

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'es-MX');

    await TestBed.configureTestingModule({
      imports: [OdontogramGridComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(OdontogramGridComponent);
    component = fixture.componentInstance;
  });

  it('renders all 32 permanent teeth', () => {
    component.teeth = buildAdultTeeth();
    fixture.detectChanges();

    const teeth = fixture.nativeElement.querySelectorAll('.tooth-button');

    expect(teeth.length).toBe(32);
  });

  it('renders upper and lower arches in adult FDI order', () => {
    component.teeth = buildAdultTeeth();
    fixture.detectChanges();

    const upperCodes = readCodes('.upper-panel .tooth-button');
    const lowerCodes = readCodes('.lower-panel .tooth-button');

    expect(upperCodes).toEqual([...ODONTOGRAM_UPPER_ARCH_ORDER]);
    expect(lowerCodes).toEqual([...ODONTOGRAM_LOWER_ARCH_ORDER]);
  });

  it('emits the selected tooth code when a tooth is clicked', () => {
    component.teeth = buildAdultTeeth();
    const selectedCodes: string[] = [];
    component.toothSelected.subscribe((toothCode) => selectedCodes.push(toothCode));
    fixture.detectChanges();

    findButton('24').click();

    expect(selectedCodes).toEqual(['24']);
  });

  it('visually marks the selected tooth', () => {
    component.teeth = buildAdultTeeth();
    component.selectedToothCode = '11';
    fixture.detectChanges();

    const selected = fixture.nativeElement.querySelector('.tooth-button.is-selected');

    expect(selected?.textContent).toContain('11');
    expect(selected?.getAttribute('aria-pressed')).toBe('true');
  });

  it('displays localized state labels and surface/finding markers', () => {
    component.teeth = buildAdultTeeth({
      '16': buildTooth('16', 'Restored', {
        O: buildSurface('O', 'Caries', [buildFinding('finding-1', 'Caries')])
      })
    });
    fixture.detectChanges();

    const tooth = findButton('16');

    expect(tooth.getAttribute('aria-label')).toContain('Restaurado');
    expect(tooth.getAttribute('title')).toContain('Restaurado');
    expect(tooth.querySelector('.surface-signal')?.textContent?.trim()).toBe('S');
    expect(tooth.querySelector('.finding-signal')?.textContent?.trim()).toBe('H');
    expect(tooth.getAttribute('aria-label')).toContain('Cambios de superficie');
    expect(tooth.getAttribute('aria-label')).toContain('Hallazgos');
  });

  it('renders the selected-tooth summary with clinical context', () => {
    component.teeth = buildAdultTeeth({
      '16': buildTooth('16', 'Restored', {
        O: buildSurface('O', 'Caries', [buildFinding('finding-1', 'Caries')]),
        M: buildSurface('M', 'Restored')
      })
    });
    component.selectedToothCode = '16';
    fixture.detectChanges();

    const summaryText = readSummaryText();

    expect(summaryText).toContain('Diente seleccionado');
    expect(summaryText).toContain('Diente');
    expect(summaryText).toContain('16');
    expect(summaryText).toContain('Tipo');
    expect(summaryText).toContain('Primer molar');
    expect(summaryText).toContain('Arcada');
    expect(summaryText).toContain('Superior');
    expect(summaryText).toContain('Cuadrante');
    expect(summaryText).toContain('Superior derecho');
    expect(summaryText).toContain('Estado');
    expect(summaryText).toContain('Restaurado');
    expect(summaryText).toContain('Superficies modificadas');
    expect(summaryText).toContain('2');
    expect(summaryText).toContain('Hallazgos');
    expect(summaryText).toContain('1');
  });

  it('places the selected-tooth summary in the side inspector outside the arch chart', () => {
    component.teeth = buildAdultTeeth();
    component.selectedToothCode = '16';
    fixture.detectChanges();

    const archContent = fixture.nativeElement.querySelector('.arch-content') as HTMLElement | null;
    const inspector = fixture.nativeElement.querySelector('.selected-tooth-inspector') as HTMLElement | null;
    const chartInspector = fixture.nativeElement.querySelector('.arch-chart .selected-tooth-inspector');

    expect(archContent).not.toBeNull();
    expect(inspector).not.toBeNull();
    expect(chartInspector).toBeNull();
    expect(archContent?.firstElementChild).toBe(inspector);
    expect(archContent?.classList.contains('has-inspector')).toBe(true);
  });

  it('updates the selected-tooth summary when another tooth is selected', () => {
    component.teeth = buildAdultTeeth();
    component.selectedToothCode = '11';
    component.toothSelected.subscribe((toothCode) => {
      component.selectedToothCode = toothCode;
    });
    fixture.detectChanges();

    findButton('38').click();
    fixture.detectChanges();

    const summaryText = readSummaryText();

    expect(summaryText).toContain('38');
    expect(summaryText).toContain('Tercer molar');
    expect(summaryText).toContain('Inferior');
    expect(summaryText).toContain('Inferior izquierdo');
  });

  it('derives tooth type labels from FDI second digit', () => {
    component.teeth = buildAdultTeeth();

    const cases: Array<[string, string]> = [
      ['11', 'Incisivo central'],
      ['13', 'Canino'],
      ['16', 'Primer molar'],
      ['24', 'Primer premolar'],
      ['38', 'Tercer molar']
    ];

    for (const [toothCode, expectedType] of cases) {
      expect(renderSummaryForTooth(toothCode)).toContain(expectedType);
    }
  });

  it('derives arch and quadrant labels from FDI quadrant', () => {
    component.teeth = buildAdultTeeth();

    const cases: Array<[string, string, string]> = [
      ['11', 'Superior', 'Superior derecho'],
      ['21', 'Superior', 'Superior izquierdo'],
      ['31', 'Inferior', 'Inferior izquierdo'],
      ['41', 'Inferior', 'Inferior derecho']
    ];

    for (const [toothCode, expectedArch, expectedQuadrant] of cases) {
      const summaryText = renderSummaryForTooth(toothCode);
      expect(summaryText).toContain(expectedArch);
      expect(summaryText).toContain(expectedQuadrant);
    }
  });

  function readCodes(selector: string): string[] {
    return (Array.from(fixture.nativeElement.querySelectorAll(selector)) as HTMLElement[])
      .map((button) => button.querySelector('.tooth-number')?.textContent?.trim() ?? '');
  }

  function readSummaryText(): string {
    const summary = fixture.nativeElement.querySelector('.selected-tooth-inspector') as HTMLElement | null;

    if (!summary) {
      throw new Error('Selected-tooth summary was not rendered.');
    }

    return summary.textContent ?? '';
  }

  function renderSummaryForTooth(toothCode: string): string {
    fixture.destroy();
    fixture = TestBed.createComponent(OdontogramGridComponent);
    component = fixture.componentInstance;
    component.teeth = buildAdultTeeth();
    component.selectedToothCode = toothCode;
    fixture.detectChanges();

    return readSummaryText();
  }

  function findButton(toothCode: string): HTMLButtonElement {
    const buttons = Array.from(fixture.nativeElement.querySelectorAll('.tooth-button')) as HTMLButtonElement[];
    const button = buttons.find((candidate) =>
      candidate.querySelector('.tooth-number')?.textContent?.trim() === toothCode);

    if (!button) {
      throw new Error(`Tooth ${toothCode} was not rendered.`);
    }

    return button;
  }
});

function buildAdultTeeth(overrides: Partial<Record<string, OdontogramToothState>> = {}): OdontogramToothState[] {
  return [...ODONTOGRAM_UPPER_ARCH_ORDER, ...ODONTOGRAM_LOWER_ARCH_ORDER]
    .map((toothCode) => overrides[toothCode] ?? buildTooth(toothCode));
}

function buildTooth(
  toothCode: string,
  status: OdontogramToothStatus = 'Unknown',
  surfaceOverrides: Partial<Record<OdontogramSurfaceCode, OdontogramSurfaceState>> = {}
): OdontogramToothState {
  return {
    toothCode,
    status,
    updatedAtUtc: '2026-04-20T10:00:00Z',
    updatedByUserId: 'user-1',
    surfaces: [
      surfaceOverrides.O ?? buildSurface('O'),
      surfaceOverrides.M ?? buildSurface('M'),
      surfaceOverrides.D ?? buildSurface('D'),
      surfaceOverrides.B ?? buildSurface('B'),
      surfaceOverrides.L ?? buildSurface('L')
    ]
  };
}

function buildSurface(
  surfaceCode: OdontogramSurfaceCode,
  status: OdontogramSurfaceStatus = 'Unknown',
  findings: OdontogramSurfaceFinding[] = []
): OdontogramSurfaceState {
  return {
    surfaceCode,
    status,
    findings,
    updatedAtUtc: '2026-04-20T10:00:00Z',
    updatedByUserId: 'user-1'
  };
}

function buildFinding(findingId: string, findingType: OdontogramSurfaceFinding['findingType']): OdontogramSurfaceFinding {
  return {
    findingId,
    findingType,
    createdAtUtc: '2026-04-20T10:15:00Z',
    createdByUserId: 'user-1'
  };
}
