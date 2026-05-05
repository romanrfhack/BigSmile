import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { I18nService } from '../../../core/i18n';
import { TranslatePipe } from '../../../shared/i18n';
import { OdontogramToothState } from '../models/odontogram.models';
import { classifyOdontogramTooth } from './odontogram-tooth-classification';
import {
  ODONTOGRAM_TOOTH_LAYOUT,
  OdontogramArch,
  OdontogramToothLayout
} from './odontogram-tooth-layout';

interface OdontogramToothView {
  layout: OdontogramToothLayout;
  tooth: OdontogramToothState;
}

interface SelectedToothSummary {
  toothCode: string;
  toothType: string;
  arch: string;
  quadrant: string;
  status: string;
  modifiedSurfaceCount: number;
  findingCount: number;
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
          <span class="mouth-axis" aria-hidden="true"></span>

          <section class="arch-panel upper-panel" [attr.aria-label]="'Upper arch' | t">
            <p class="arch-label">{{ 'Upper arch' | t }}</p>

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
              <span class="tooth-signals" aria-hidden="true">
                <span *ngIf="hasSurfaceSignal(item.tooth)" class="signal surface-signal">S</span>
                <span *ngIf="hasFindings(item.tooth)" class="signal finding-signal">H</span>
              </span>
            </button>
          </section>

          <article *ngIf="selectedToothSummary as summary" class="selected-summary" aria-live="polite">
            <p class="summary-eyebrow">{{ 'Selected tooth' | t }}</p>
            <dl class="summary-grid">
              <div>
                <dt>{{ 'Tooth' | t }}</dt>
                <dd>{{ summary.toothCode }}</dd>
              </div>
              <div>
                <dt>{{ 'Type' | t }}</dt>
                <dd>{{ summary.toothType }}</dd>
              </div>
              <div>
                <dt>{{ 'Arch' | t }}</dt>
                <dd>{{ summary.arch }}</dd>
              </div>
              <div>
                <dt>{{ 'Quadrant' | t }}</dt>
                <dd>{{ summary.quadrant }}</dd>
              </div>
              <div>
                <dt>{{ 'Status' | t }}</dt>
                <dd>{{ summary.status }}</dd>
              </div>
              <div>
                <dt>{{ 'Modified surfaces' | t }}</dt>
                <dd>{{ summary.modifiedSurfaceCount }}</dd>
              </div>
              <div>
                <dt>{{ 'Findings' | t }}</dt>
                <dd>{{ summary.findingCount }}</dd>
              </div>
            </dl>
          </article>

          <section class="arch-panel lower-panel" [attr.aria-label]="'Lower arch' | t">
            <p class="arch-label">{{ 'Lower arch' | t }}</p>

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
      position: relative;
      min-width: 1040px;
      height: 660px;
      border-radius: 22px;
      border: 1px solid #cfdae6;
      background:
        radial-gradient(ellipse at 50% 50%, rgba(255, 235, 229, 0.74) 0 25%, transparent 26%),
        linear-gradient(180deg, #fbfdff 0%, #eef5f8 50%, #fbfdff 100%);
      overflow: hidden;
    }

    .mouth-axis {
      position: absolute;
      left: 50%;
      top: 4.5rem;
      bottom: 4.5rem;
      border-left: 1px dashed rgba(86, 112, 141, 0.32);
      z-index: 1;
    }

    .arch-panel {
      position: absolute;
      inset: 0;
      pointer-events: none;
    }

    .arch-panel::before {
      content: '';
      position: absolute;
      left: 6.5%;
      right: 6.5%;
      height: 40%;
      border: 2px solid rgba(84, 113, 142, 0.2);
      z-index: 1;
    }

    .upper-panel::before {
      top: 10%;
      border-bottom: 0;
      border-radius: 50% 50% 0 0;
    }

    .lower-panel::before {
      bottom: 10%;
      border-top: 0;
      border-radius: 0 0 50% 50%;
    }

    .arch-label {
      position: absolute;
      left: 1rem;
      margin: 0;
      color: #52677f;
      font-size: 0.8rem;
      font-weight: 800;
      text-transform: uppercase;
      z-index: 2;
    }

    .upper-panel .arch-label {
      top: 1rem;
    }

    .lower-panel .arch-label {
      bottom: 1rem;
    }

    .tooth-button {
      --tooth-rotation: 0deg;
      position: absolute;
      width: 2.65rem;
      min-height: 3.45rem;
      border: 1px solid #cbd8e5;
      border-radius: 0.82rem 0.82rem 1rem 1rem;
      background: #ffffff;
      padding: 0.35rem 0.22rem 0.3rem;
      display: grid;
      justify-items: center;
      align-content: start;
      gap: 0.14rem;
      transform: translate(-50%, -50%) rotate(var(--tooth-rotation));
      cursor: pointer;
      pointer-events: auto;
      transition: border-color 120ms ease, box-shadow 120ms ease, transform 120ms ease;
      z-index: 2;
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
      box-shadow: 0 0 0 3px rgba(10, 91, 181, 0.18), 0 10px 20px rgba(20, 48, 79, 0.1);
      z-index: 4;
    }

    .tooth-button.has-surface-signal {
      border-bottom-color: #0f766e;
    }

    .tooth-button.has-finding-signal {
      border-top-color: #b45309;
    }

    .tooth-number {
      color: #16324f;
      font-size: 0.86rem;
      font-weight: 800;
      line-height: 1;
    }

    .tooth-crown {
      width: 1.48rem;
      height: 1.08rem;
      border: 1px solid rgba(74, 99, 126, 0.28);
      border-radius: 0.62rem 0.62rem 0.78rem 0.78rem;
      background: linear-gradient(180deg, rgba(255, 255, 255, 0.95), rgba(238, 244, 250, 0.88));
      box-shadow: inset 0 -5px 8px rgba(102, 126, 152, 0.08);
    }

    .tooth-signals {
      display: flex;
      gap: 0.18rem;
      min-height: 0.7rem;
    }

    .signal {
      width: 0.62rem;
      height: 0.62rem;
      border-radius: 999px;
      color: #ffffff;
      display: inline-grid;
      place-items: center;
      font-size: 0.42rem;
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

    .selected-summary {
      position: absolute;
      left: 50%;
      top: 50%;
      width: min(22rem, 34%);
      transform: translate(-50%, -50%);
      border: 1px solid #cbd8e5;
      border-radius: 16px;
      background: rgba(255, 255, 255, 0.94);
      box-shadow: 0 16px 34px rgba(20, 48, 79, 0.12);
      padding: 0.82rem 0.9rem;
      z-index: 3;
    }

    .summary-eyebrow {
      margin: 0 0 0.55rem;
      color: #52677f;
      font-size: 0.76rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .summary-grid {
      margin: 0;
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 0.48rem 0.78rem;
    }

    .summary-grid div:nth-child(2),
    .summary-grid div:nth-child(4) {
      grid-column: span 2;
    }

    dt {
      margin: 0 0 0.14rem;
      color: #718298;
      font-size: 0.64rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    dd {
      margin: 0;
      color: #16324f;
      font-size: 0.84rem;
      font-weight: 700;
      line-height: 1.2;
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

  get selectedToothSummary(): SelectedToothSummary | null {
    const tooth = this.selectedTooth;
    if (!tooth) {
      return null;
    }

    const classification = classifyOdontogramTooth(tooth.toothCode);

    return {
      toothCode: tooth.toothCode,
      toothType: this.i18n.translate(classification.toothTypeLabelKey),
      arch: this.i18n.translate(classification.archLabelKey),
      quadrant: this.i18n.translate(classification.quadrantLabelKey),
      status: this.getStatusLabel(tooth.status),
      modifiedSurfaceCount: this.getModifiedSurfaceCount(tooth),
      findingCount: this.getFindingCount(tooth)
    };
  }

  getStatusLabel(status: OdontogramToothState['status']): string {
    return this.i18n.translate(status);
  }

  hasSurfaceSignal(tooth: OdontogramToothState): boolean {
    return this.getModifiedSurfaceCount(tooth) > 0;
  }

  hasFindings(tooth: OdontogramToothState): boolean {
    return this.getFindingCount(tooth) > 0;
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

  private get selectedTooth(): OdontogramToothState | null {
    if (!this.selectedToothCode) {
      return null;
    }

    return this.teeth.find((tooth) => tooth.toothCode === this.selectedToothCode) ?? null;
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

  private getModifiedSurfaceCount(tooth: OdontogramToothState): number {
    return tooth.surfaces.filter((surface) => surface.status !== 'Unknown').length;
  }

  private getFindingCount(tooth: OdontogramToothState): number {
    return tooth.surfaces.reduce((total, surface) => total + surface.findings.length, 0);
  }
}
