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

          <article *ngIf="selectedToothSummary as summary" class="selected-tooth-inspector" aria-live="polite">
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
      color: var(--bsm-color-text-muted);
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
      background: var(--bsm-color-accent-accessible);
    }

    .finding-dot,
    .finding-signal {
      background: var(--bsm-color-primary);
    }

    .chart-scroll {
      box-sizing: border-box;
      min-width: 0;
      overflow-x: hidden;
      padding: 0.25rem 0 0.65rem;
    }

    .arch-chart {
      box-sizing: border-box;
      position: relative;
      min-width: 0;
      width: 100%;
      max-width: 100%;
      height: 660px;
      border-radius: var(--bsm-radius-lg);
      border: 1px solid var(--bsm-color-border);
      background:
        radial-gradient(ellipse at 50% 50%, rgba(255, 235, 229, 0.74) 0 25%, transparent 26%),
        linear-gradient(180deg, #ffffff 0%, var(--bsm-color-accent-soft) 50%, #ffffff 100%);
      overflow: hidden;
    }

    .mouth-axis {
      position: absolute;
      left: 50%;
      top: 4.5rem;
      bottom: 4.5rem;
      border-left: 1px dashed rgba(100, 42, 143, 0.24);
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
      border: 2px solid rgba(100, 42, 143, 0.16);
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
      color: var(--bsm-color-text-muted);
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
      border: 1px solid var(--bsm-color-border);
      border-radius: 0.82rem 0.82rem 1rem 1rem;
      background: var(--bsm-color-bg);
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
      border-color: var(--bsm-color-accent-accessible);
      transform: translate(-50%, -50%) rotate(var(--tooth-rotation)) translateY(-2px);
      box-shadow: var(--bsm-shadow-sm);
      outline: none;
    }

    .tooth-button.is-selected {
      border-color: var(--bsm-color-primary);
      box-shadow: 0 0 0 3px rgba(100, 42, 143, 0.18), var(--bsm-shadow-sm);
      z-index: 4;
    }

    .tooth-button.has-surface-signal {
      border-bottom-color: var(--bsm-color-accent-accessible);
    }

    .tooth-button.has-finding-signal {
      border-top-color: var(--bsm-color-primary);
    }

    .tooth-number {
      color: var(--bsm-color-text-brand);
      font-size: 0.86rem;
      font-weight: 800;
      line-height: 1;
    }

    .tooth-crown {
      width: 1.48rem;
      height: 1.08rem;
      border: 1px solid rgba(100, 42, 143, 0.16);
      border-radius: 0.62rem 0.62rem 0.78rem 0.78rem;
      background: linear-gradient(180deg, rgba(255, 255, 255, 0.95), rgba(238, 244, 250, 0.88));
      box-shadow: inset 0 -5px 8px rgba(100, 42, 143, 0.07);
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

    .selected-tooth-inspector {
      position: absolute;
      left: 1.25rem;
      top: 9.5rem;
      width: 11.25rem;
      border-left: 3px solid rgba(100, 42, 143, 0.35);
      padding-inline-start: 0.75rem;
      background: transparent;
      color: var(--bsm-color-text-brand);
      pointer-events: none;
      z-index: 3;
    }

    .summary-eyebrow {
      margin: 0 0 0.55rem;
      color: var(--bsm-color-text-muted);
      font-size: 0.76rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .summary-grid {
      margin: 0;
      display: grid;
      gap: 0.48rem;
    }

    .summary-grid div {
      display: grid;
      gap: 0.1rem;
    }

    .summary-grid dt {
      margin: 0 0 0.14rem;
      color: var(--bsm-color-text-muted);
      font-size: 0.64rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .summary-grid dd {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: 0.84rem;
      font-weight: 700;
      line-height: 1.2;
    }

    @media (max-width: 860px) {
      .chart-scroll {
        overflow-x: auto;
      }

      .arch-chart {
        width: 860px;
        max-width: none;
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
