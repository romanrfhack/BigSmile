import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ODONTOGRAM_LOWER_ARCH_ORDER,
  ODONTOGRAM_UPPER_ARCH_ORDER,
  OdontogramGridComponent
} from './odontogram-grid.component';
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

    expect(tooth.textContent).toContain('Restaurado');
    expect(tooth.querySelector('.surface-signal')?.textContent?.trim()).toBe('S');
    expect(tooth.querySelector('.finding-signal')?.textContent?.trim()).toBe('H');
    expect(tooth.getAttribute('aria-label')).toContain('Cambios de superficie');
    expect(tooth.getAttribute('aria-label')).toContain('Hallazgos');
  });

  function readCodes(selector: string): string[] {
    return (Array.from(fixture.nativeElement.querySelectorAll(selector)) as HTMLElement[])
      .map((button) => button.querySelector('.tooth-number')?.textContent?.trim() ?? '');
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
