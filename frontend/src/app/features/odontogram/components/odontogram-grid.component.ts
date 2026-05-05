import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { TranslatePipe } from '../../../shared/i18n';
import { OdontogramToothState } from '../models/odontogram.models';

export type OdontogramArch = 'upper' | 'lower';

export interface OdontogramToothLayout {
  toothCode: string;
  arch: OdontogramArch;
  x: number;
  y: number;
  rotation: number;
}

export const ODONTOGRAM_UPPER_ARCH_ORDER = [
  '18', '17', '16', '15', '14', '13', '12', '11',
  '21', '22', '23', '24', '25', '26', '27', '28'
] as const;

export const ODONTOGRAM_LOWER_ARCH_ORDER = [
  '48', '47', '46', '45', '44', '43', '42', '41',
  '31', '32', '33', '34', '35', '36', '37', '38'
] as const;

export const ODONTOGRAM_TOOTH_LAYOUT: OdontogramToothLayout[] = [
  { toothCode: '18', arch: 'upper', x: 5, y: 20, rotation: -22 },
  { toothCode: '17', arch: 'upper', x: 10, y: 36, rotation: -20 },
  { toothCode: '16', arch: 'upper', x: 16, y: 52, rotation: -17 },
  { toothCode: '15', arch: 'upper', x: 23, y: 68, rotation: -13 },
  { toothCode: '14', arch: 'upper', x: 31, y: 82, rotation: -9 },
  { toothCode: '13', arch: 'upper', x: 39, y: 92, rotation: -5 },
  { toothCode: '12', arch: 'upper', x: 46, y: 98, rotation: -2 },
  { toothCode: '11', arch: 'upper', x: 50, y: 101, rotation: 0 },
  { toothCode: '21', arch: 'upper', x: 54, y: 98, rotation: 2 },
  { toothCode: '22', arch: 'upper', x: 61, y: 92, rotation: 5 },
  { toothCode: '23', arch: 'upper', x: 69, y: 82, rotation: 9 },
  { toothCode: '24', arch: 'upper', x: 77, y: 68, rotation: 13 },
  { toothCode: '25', arch: 'upper', x: 84, y: 52, rotation: 17 },
  { toothCode: '26', arch: 'upper', x: 90, y: 36, rotation: 20 },
  { toothCode: '27', arch: 'upper', x: 95, y: 20, rotation: 22 },
  { toothCode: '28', arch: 'upper', x: 96, y: 9, rotation: 24 },
  { toothCode: '48', arch: 'lower', x: 5, y: 80, rotation: 22 },
  { toothCode: '47', arch: 'lower', x: 10, y: 64, rotation: 20 },
  { toothCode: '46', arch: 'lower', x: 16, y: 48, rotation: 17 },
  { toothCode: '45', arch: 'lower', x: 23, y: 32, rotation: 13 },
  { toothCode: '44', arch: 'lower', x: 31, y: 18, rotation: 9 },
  { toothCode: '43', arch: 'lower', x: 39, y: 8, rotation: 5 },
  { toothCode: '42', arch: 'lower', x: 46, y: 2, rotation: 2 },
  { toothCode: '41', arch: 'lower', x: 50, y: 0, rotation: 0 },
  { toothCode: '31', arch: 'lower', x: 54, y: 2, rotation: -2 },
  { toothCode: '32', arch: 'lower', x: 61, y: 8, rotation: -5 },
  { toothCode: '33', arch: 'lower', x: 69, y: 18, rotation: -9 },
  { toothCode: '34', arch: 'lower', x: 77, y: 32, rotation: -13 },
  { toothCode: '35', arch: 'lower', x: 84, y: 48, rotation: -17 },
  { toothCode: '36', arch: 'lower', x: 90, y: 64, rotation: -20 },
  { toothCode: '37', arch: 'lower', x: 95, y: 80, rotation: -22 },
  { toothCode: '38', arch: 'lower', x: 96, y: 91, rotation: -24 }
];

interface OdontogramToothView {
  layout: OdontogramToothLayout;
  tooth: OdontogramToothState;
}

@Component({
  selector: 'app-odontogram-grid',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="arch-shell" [attr.aria-label]="'Odontogram dental arch' | t">
      <div class="legend-row">
        <span class="legend-item">
          <span class="legend-dot surface-dot"></span>
          {{ 'Surface changes' | t }}
        </span>
        <span class="legend-item">
          <span class="legend-dot finding-dot"></span>
          {{ 'Findings' | t }}
        </span>
      </div>

      <div class="chart-scroll">
        <div class="arch-chart">
          <section class="arch-panel upper-panel" [attr.aria-label]="'Upper arch' | t">
            <p class="arch-label">{{ 'Upper arch' | t }}</p>
            <span class="midline" aria-hidden="true"></span>

            <button
              *ngFor="let item of upperTeeth; trackBy: trackByToothCode"
              type="button"
              class="tooth-button"
              [class.is-selected]="item.tooth.toothCode === selectedToothCode"
              [class.status-healthy]="item.tooth.status === 'Healthy'"
              [class.status-missing]="item.tooth.status === 'Missing'"
              [class.status-restored]="item.tooth.status === 'Restored'"
              [class.status-caries]="item.tooth.status === 'Caries'"
              [class.has-surface-signal]="hasSurfaceSignal(item.tooth)"
              [class.has-finding-signal]="hasFindings(item.tooth)"
              [style.left.%]="item.layout.x"
              [style.top.%]="item.layout.y"
              [style.--tooth-rotation]="item.layout.rotation + 'deg'"
              [attr.aria-pressed]="item.tooth.toothCode === selectedToothCode"
              [attr.aria-label]="getToothAriaLabel(item.tooth)"
              [attr.title]="getToothAriaLabel(item.tooth)"
              (click)="toothSelected.emit(item.tooth.toothCode)">
              <span class="tooth-number">{{ item.tooth.toothCode }}</span>
              <span class="tooth-crown" aria-hidden="true"></span>
              <span class="tooth-status">{{ getStatusLabel(item.tooth.status) }}</span>
              <span class="tooth-signals" aria-hidden="true">
                <span *ngIf="hasSurfaceSignal(item.tooth)" class="signal surface-signal">S</span>
                <span *ngIf="hasFindings(item.tooth)" class="signal finding-signal">H</span>
              </span>
            </button>
          </section>

          <section class="arch-panel lower-panel" [attr.aria-label]="'Lower arch' | t">
            <p class="arch-label">{{ 'Lower arch' | t }}</p>
            <span class="midline" aria-hidden="true"></span>

            <button
              *ngFor="let item of lowerTeeth; trackBy: trackByToothCode"
              type="button"
              class="tooth-button"
              [class.is-selected]="item.tooth.toothCode === selectedToothCode"
              [class.status-healthy]="item.tooth.status === 'Healthy'"
              [class.status-missing]="item.tooth.status === 'Missing'"
              [class.status-restored]="item.tooth.status === 'Restored'"
              [class.status-caries]="item.tooth.status === 'Caries'"
              [class.has-surface-signal]="hasSurfaceSignal(item.tooth)"
              [class.has-finding-signal]="hasFindings(item.tooth)"
              [style.left.%]="item.layout.x"
              [style.top.%]="item.layout.y"
              [style.--tooth-rotation]="item.layout.rotation + 'deg'"
              [attr.aria-pressed]="item.tooth.toothCode === selectedToothCode"
              [attr.aria-label]="getToothAriaLabel(item.tooth)"
              [attr.title]="getToothAriaLabel(item.tooth)"
              (click)="toothSelected.emit(item.tooth.toothCode)">
              <span class="tooth-number">{{ item.tooth.toothCode }}</span>
              <span class="tooth-crown" aria-hidden="true"></span>
              <span class="tooth-status">{{ getStatusLabel(item.tooth.status) }}</span>
              <span class="tooth-signals" aria-hidden="true">
                <span *ngIf="hasSurfaceSignal(item.tooth)" class="signal surface-signal">S</span>
                <span *ngIf="hasFindings(item.tooth)" class="signal finding-signal">H</span>
              </span>
            </button>
          </section>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .arch-shell {
      display: grid;
      gap: 0.8rem;
    }

    .legend-row {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
      color: #52677f;
      font-size: 0.85rem;
      font-weight: 700;
    }

    .legend-item {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
    }

    .legend-dot {
      width: 0.75rem;
      height: 0.75rem;
      border-radius: 999px;
      display: inline-block;
    }

    .surface-dot,
    .surface-signal {
      background: #0f766e;
    }

    .finding-dot,
    .finding-signal {
      background: #b45309;
    }

    .chart-scroll {
      overflow-x: auto;
      padding: 0.25rem 0.1rem 0.65rem;
    }

    .arch-chart {
      min-width: 860px;
      display: grid;
      gap: 0.6rem;
      border-radius: 22px;
      border: 1px solid #cfdae6;
      background:
        radial-gradient(circle at 50% 50%, rgba(248, 213, 202, 0.46) 0 19%, transparent 20%),
        linear-gradient(180deg, #fbfdff 0%, #eef5f8 50%, #fbfdff 100%);
      padding: 1rem 1.1rem;
    }

    .arch-panel {
      position: relative;
      min-height: 190px;
      border-radius: 18px;
      background: rgba(255, 255, 255, 0.74);
      overflow: hidden;
    }

    .upper-panel {
      border-bottom: 1px solid rgba(96, 120, 146, 0.2);
    }

    .lower-panel {
      border-top: 1px solid rgba(96, 120, 146, 0.2);
    }

    .arch-label {
      position: absolute;
      left: 1rem;
      top: 0.8rem;
      margin: 0;
      color: #52677f;
      font-size: 0.8rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .midline {
      position: absolute;
      left: 50%;
      top: 0.85rem;
      bottom: 0.85rem;
      border-left: 1px dashed rgba(86, 112, 141, 0.34);
    }

    .tooth-button {
      --tooth-rotation: 0deg;
      position: absolute;
      width: 3.85rem;
      min-height: 5.35rem;
      border: 1px solid #cbd8e5;
      border-radius: 1rem 1rem 1.35rem 1.35rem;
      background: #ffffff;
      padding: 0.45rem 0.32rem 0.4rem;
      display: grid;
      justify-items: center;
      align-content: start;
      gap: 0.2rem;
      transform: translate(-50%, -50%) rotate(var(--tooth-rotation));
      cursor: pointer;
      transition: border-color 120ms ease, box-shadow 120ms ease, transform 120ms ease;
    }

    .tooth-button:hover,
    .tooth-button:focus-visible {
      border-color: #7aa8d9;
      transform: translate(-50%, -50%) rotate(var(--tooth-rotation)) translateY(-2px);
      box-shadow: 0 10px 20px rgba(20, 48, 79, 0.08);
      outline: none;
    }

    .tooth-button.is-selected {
      border-color: #0a5bb5;
      box-shadow: 0 0 0 3px rgba(10, 91, 181, 0.16);
    }

    .tooth-button.has-surface-signal {
      border-bottom-color: #0f766e;
    }

    .tooth-button.has-finding-signal {
      border-top-color: #b45309;
    }

    .tooth-number {
      color: #16324f;
      font-size: 0.95rem;
      font-weight: 800;
      line-height: 1;
    }

    .tooth-crown {
      width: 2.1rem;
      height: 1.65rem;
      border: 1px solid rgba(74, 99, 126, 0.28);
      border-radius: 0.8rem 0.8rem 1.05rem 1.05rem;
      background: linear-gradient(180deg, rgba(255, 255, 255, 0.95), rgba(238, 244, 250, 0.88));
      box-shadow: inset 0 -5px 8px rgba(102, 126, 152, 0.08);
    }

    .tooth-status {
      color: #5b6e84;
      font-size: 0.67rem;
      font-weight: 600;
      line-height: 1.1;
      max-width: 3.1rem;
      min-height: 1.55rem;
      text-align: center;
      overflow-wrap: anywhere;
    }

    .tooth-signals {
      display: flex;
      gap: 0.18rem;
      min-height: 0.8rem;
    }

    .signal {
      width: 0.78rem;
      height: 0.78rem;
      border-radius: 999px;
      color: #ffffff;
      display: inline-grid;
      place-items: center;
      font-size: 0.5rem;
      font-weight: 800;
      line-height: 1;
    }

    .status-healthy {
      background: #eef8f0;
    }

    .status-missing {
      background: #fff2f2;
    }

    .status-restored {
      background: #eef4ff;
    }

    .status-caries {
      background: #fff4e8;
    }

    @media (max-width: 980px) {
      .chart-scroll {
        margin-inline: -0.2rem;
      }
    }
  `]
})
export class OdontogramGridComponent {
  private readonly i18n = inject(I18nService);

  @Input() teeth: OdontogramToothState[] = [];
  @Input() selectedToothCode: string | null = null;

  @Output() readonly toothSelected = new EventEmitter<string>();

  get upperTeeth(): OdontogramToothView[] {
    return this.getArchTeeth('upper');
  }

  get lowerTeeth(): OdontogramToothView[] {
    return this.getArchTeeth('lower');
  }

  getStatusLabel(status: OdontogramToothState['status']): string {
    return this.i18n.translate(status);
  }

  hasSurfaceSignal(tooth: OdontogramToothState): boolean {
    return tooth.surfaces.some((surface) => surface.status !== 'Unknown');
  }

  hasFindings(tooth: OdontogramToothState): boolean {
    return tooth.surfaces.some((surface) => surface.findings.length > 0);
  }

  getToothAriaLabel(tooth: OdontogramToothState): string {
    const markers = [
      this.hasSurfaceSignal(tooth) ? this.i18n.translate('Surface changes') : null,
      this.hasFindings(tooth) ? this.i18n.translate('Findings') : null
    ].filter(Boolean);

    const markerText = markers.length > 0 ? `, ${markers.join(', ')}` : '';
    return `${this.i18n.translate('Tooth')} ${tooth.toothCode}, ${this.getStatusLabel(tooth.status)}${markerText}`;
  }

  trackByToothCode(_index: number, item: OdontogramToothView): string {
    return item.tooth.toothCode;
  }

  private getArchTeeth(arch: OdontogramArch): OdontogramToothView[] {
    const teethByCode = new Map(this.teeth.map((tooth) => [tooth.toothCode, tooth]));

    return ODONTOGRAM_TOOTH_LAYOUT
      .filter((layout) => layout.arch === arch)
      .map((layout) => {
        const tooth = teethByCode.get(layout.toothCode);
        return tooth ? { layout, tooth } : null;
      })
      .filter((item): item is OdontogramToothView => item !== null);
  }
}
