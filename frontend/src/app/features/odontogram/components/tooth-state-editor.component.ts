import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { I18nService } from '../../../core/i18n';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import {
  ODONTOGRAM_SURFACE_CODES,
  ODONTOGRAM_SURFACE_FINDING_TYPES,
  ODONTOGRAM_SURFACE_STATUSES,
  OdontogramSurfaceCode,
  OdontogramSurfaceFindingHistoryEntry,
  OdontogramSurfaceFindingType,
  OdontogramSurfaceState,
  OdontogramSurfaceStatus,
  ODONTOGRAM_TOOTH_STATUSES,
  OdontogramToothState,
  OdontogramToothStatus
} from '../models/odontogram.models';
import { SurfaceFindingHistoryListComponent } from './surface-finding-history-list.component';

@Component({
  selector: 'app-tooth-state-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, SurfaceFindingHistoryListComponent, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="editor-card">
      <ng-container *ngIf="tooth; else noSelection">
        <p class="eyebrow">{{ 'Selected tooth' | t }}</p>
        <h3>{{ 'Tooth' | t }} {{ tooth.toothCode }}</h3>
        <p class="copy">{{ 'Minimal tooth state, surface state, and basic surface findings with bounded add/remove history only. No restore, full odontogram versioning, treatment linkage, or advanced charting in Release 4.4.' | t }}</p>

        <div class="meta-grid">
          <div>
            <dt>{{ 'Current status' | t }}</dt>
            <dd>{{ getStatusLabel(tooth.status) }}</dd>
          </div>
          <div>
            <dt>{{ 'Last updated' | t }}</dt>
            <dd>{{ tooth.updatedAtUtc | bsDate: 'medium' }}</dd>
          </div>
          <div>
            <dt>{{ 'Updated by' | t }}</dt>
            <dd>{{ tooth.updatedByUserId }}</dd>
          </div>
        </div>

        <div *ngIf="canWrite" class="editor-actions">
          <label class="field">
            <span>{{ 'New status' | t }}</span>
            <select [(ngModel)]="selectedStatus">
              <option *ngFor="let status of statuses" [ngValue]="status">{{ getStatusLabel(status) }}</option>
            </select>
          </label>

          <button
            type="button"
            class="primary-action"
            [disabled]="savingTooth || selectedStatus === tooth.status"
            (click)="updateRequested.emit(selectedStatus)">
            {{ (savingTooth ? 'Saving...' : 'Update tooth state') | t }}
          </button>
        </div>

        <section class="surface-section" *ngIf="tooth.surfaces.length > 0">
          <div class="surface-head">
            <div>
              <p class="eyebrow">{{ 'Surfaces' | t }}</p>
              <h4>{{ 'Current detail for tooth {toothCode}' | t:{ toothCode: tooth.toothCode } }}</h4>
            </div>
          </div>

          <div class="surface-grid">
            <button
              *ngFor="let surface of tooth.surfaces"
              type="button"
              class="surface-card"
              [class.is-selected]="surface.surfaceCode === selectedSurfaceCode"
              [class.status-healthy]="surface.status === 'Healthy'"
              [class.status-restored]="surface.status === 'Restored'"
              [class.status-caries]="surface.status === 'Caries'"
              (click)="selectSurface(surface.surfaceCode)">
              <span class="surface-code">{{ surface.surfaceCode }}</span>
              <span class="surface-status">{{ getStatusLabel(surface.status) }}</span>
            </button>
          </div>

          <ng-container *ngIf="selectedSurface as surface">
            <div class="meta-grid">
              <div>
                <dt>{{ 'Selected surface' | t }}</dt>
                <dd>{{ surface.surfaceCode }}</dd>
              </div>
              <div>
                <dt>{{ 'Current surface status' | t }}</dt>
                <dd>{{ getStatusLabel(surface.status) }}</dd>
              </div>
              <div>
                <dt>{{ 'Last updated' | t }}</dt>
                <dd>{{ surface.updatedAtUtc | bsDate: 'medium' }}</dd>
              </div>
              <div>
                <dt>{{ 'Updated by' | t }}</dt>
                <dd>{{ surface.updatedByUserId }}</dd>
              </div>
            </div>

            <div *ngIf="canWrite" class="editor-actions">
              <label class="field">
                  <span>{{ 'Surface' | t }}</span>
                <select [(ngModel)]="selectedSurfaceCode" (ngModelChange)="onSurfaceSelectionChange($event)">
                  <option *ngFor="let surfaceCode of surfaceCodes" [ngValue]="surfaceCode">{{ surfaceCode }}</option>
                </select>
              </label>

              <label class="field">
                  <span>{{ 'New surface status' | t }}</span>
                  <select [(ngModel)]="selectedSurfaceStatus">
                    <option *ngFor="let status of surfaceStatuses" [ngValue]="status">{{ getStatusLabel(status) }}</option>
                  </select>
              </label>

              <button
                type="button"
                class="primary-action"
                [disabled]="savingSurface || !selectedSurfaceCode || selectedSurfaceStatus === surface.status"
                (click)="emitSurfaceUpdate()">
                {{ (savingSurface ? 'Saving...' : 'Update surface state') | t }}
              </button>
            </div>

            <section class="finding-section">
              <div>
                <p class="eyebrow">{{ 'Basic findings' | t }}</p>
                <h4>{{ 'Current findings for surface {surfaceCode}' | t:{ surfaceCode: surface.surfaceCode } }}</h4>
                <p class="copy">{{ 'Findings coexist with the operational surface status. They do not auto-recalculate the surface or tooth state in this slice.' | t }}</p>
              </div>

              <div *ngIf="surface.findings.length > 0; else noFindings" class="finding-list">
                <article *ngFor="let finding of surface.findings" class="finding-card">
                  <div class="finding-meta">
                    <strong>{{ getFindingTypeLabel(finding.findingType) }}</strong>
                    <span>{{ finding.createdAtUtc | bsDate: 'medium' }}</span>
                    <span>{{ finding.createdByUserId }}</span>
                  </div>

                  <button
                    *ngIf="canWrite"
                    type="button"
                    class="secondary-action danger-action"
                    [disabled]="savingFinding"
                    (click)="emitFindingRemove(finding.findingId)">
                    {{ (removingFindingId === finding.findingId ? 'Removing...' : 'Remove finding') | t }}
                  </button>
                </article>
              </div>

              <ng-template #noFindings>
                <p class="muted">{{ 'No basic findings registered for this surface yet.' | t }}</p>
              </ng-template>

              <div *ngIf="canWrite" class="editor-actions">
                <label class="field">
                  <span>{{ 'New finding type' | t }}</span>
                  <select [(ngModel)]="selectedFindingType">
                    <option *ngFor="let findingType of findingTypes" [ngValue]="findingType">{{ getFindingTypeLabel(findingType) }}</option>
                  </select>
                </label>

                <button
                  type="button"
                  class="primary-action"
                  [disabled]="savingFinding || !selectedSurfaceCode || surfaceHasFinding(selectedFindingType)"
                  (click)="emitFindingAdd()">
                  {{ (savingFinding ? 'Saving...' : 'Add finding') | t }}
                </button>
              </div>
            </section>

            <section class="finding-section">
              <div>
                <p class="eyebrow">{{ 'Findings history' | t }}</p>
                <h4>{{ 'Current history for surface {surfaceCode}' | t:{ surfaceCode: surface.surfaceCode } }}</h4>
                <p class="copy">{{ 'This bounded history stays separate from current findings and from any future dental timeline. Release 4.4 tracks only add/remove events for the basic finding catalog.' | t }}</p>
              </div>

              <app-surface-finding-history-list
                [entries]="findingsHistory"
                [toothCode]="tooth.toothCode"
                [surfaceCode]="surface.surfaceCode">
              </app-surface-finding-history-list>
            </section>
          </ng-container>
        </section>

        <p *ngIf="error" class="error-copy">{{ error | t }}</p>
      </ng-container>

      <ng-template #noSelection>
        <p class="muted">{{ 'Select a tooth to inspect or update its current state.' | t }}</p>
      </ng-template>
    </section>
  `,
  styles: [`
    .editor-card {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      display: grid;
      gap: 1rem;
    }

    .eyebrow {
      margin: 0;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3 {
      margin: 0;
      color: #16324f;
    }

    .copy,
    .muted {
      margin: 0;
      color: #5b6e84;
    }

    .meta-grid {
      display: grid;
      gap: 0.9rem;
      grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #718298;
    }

    dd {
      margin: 0;
      color: #16324f;
      font-weight: 600;
      word-break: break-word;
    }

    .editor-actions {
      display: flex;
      gap: 0.9rem;
      align-items: end;
      flex-wrap: wrap;
    }

    .field {
      display: grid;
      gap: 0.35rem;
      min-width: 220px;
      color: #16324f;
      font-weight: 600;
    }

    select {
      border-radius: 12px;
      border: 1px solid #c7d3e0;
      padding: 0.75rem 0.85rem;
      font: inherit;
      background: #ffffff;
    }

    .primary-action {
      border: none;
      border-radius: 999px;
      padding: 0.85rem 1.2rem;
      background: #0a5bb5;
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    .primary-action:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .surface-section {
      display: grid;
      gap: 1rem;
      border-top: 1px solid #d7dfe8;
      padding-top: 1rem;
    }

    .surface-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: center;
    }

    h4 {
      margin: 0.25rem 0 0;
      color: #16324f;
    }

    .surface-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
      gap: 0.75rem;
    }

    .surface-card {
      border-radius: 16px;
      border: 1px solid #d6dfeb;
      background: #ffffff;
      padding: 0.8rem;
      display: grid;
      gap: 0.35rem;
      text-align: left;
      cursor: pointer;
    }

    .surface-card.is-selected {
      border-color: #0a5bb5;
      box-shadow: 0 0 0 2px rgba(10, 91, 181, 0.12);
    }

    .surface-code {
      color: #16324f;
      font-size: 0.95rem;
      font-weight: 800;
    }

    .surface-status {
      color: #5b6e84;
      font-size: 0.85rem;
      font-weight: 600;
    }

    .surface-card.status-healthy {
      background: #eef8f0;
    }

    .surface-card.status-restored {
      background: #eef4ff;
    }

    .surface-card.status-caries {
      background: #fff4e8;
    }

    .error-copy {
      margin: 0;
      color: #8c2525;
      font-weight: 600;
    }

    .finding-section {
      display: grid;
      gap: 0.9rem;
      border-top: 1px solid #e1e8ef;
      padding-top: 1rem;
    }

    .finding-list {
      display: grid;
      gap: 0.75rem;
    }

    .finding-card {
      border-radius: 16px;
      border: 1px solid #d6dfeb;
      background: #ffffff;
      padding: 0.85rem;
      display: flex;
      justify-content: space-between;
      gap: 0.75rem;
      align-items: center;
      flex-wrap: wrap;
    }

    .finding-meta {
      display: grid;
      gap: 0.2rem;
      color: #5b6e84;
    }

    .finding-meta strong {
      color: #16324f;
    }

    .secondary-action {
      border-radius: 999px;
      border: 1px solid #d6dfeb;
      background: #ffffff;
      color: #16324f;
      padding: 0.75rem 1rem;
      font-weight: 700;
      cursor: pointer;
    }

    .secondary-action:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .danger-action {
      border-color: #efc2c2;
      color: #8c2525;
    }
  `]
})
export class ToothStateEditorComponent implements OnChanges {
  private readonly i18n = inject(I18nService);

  @Input() tooth: OdontogramToothState | null = null;
  @Input() canWrite = false;
  @Input() savingTooth = false;
  @Input() savingSurface = false;
  @Input() savingFinding = false;
  @Input() removingFindingId: string | null = null;
  @Input() error: string | null = null;
  @Input() findingsHistory: OdontogramSurfaceFindingHistoryEntry[] = [];

  @Output() readonly updateRequested = new EventEmitter<OdontogramToothStatus>();
  @Output() readonly surfaceUpdateRequested = new EventEmitter<{ surfaceCode: string; status: OdontogramSurfaceStatus }>();
  @Output() readonly findingAddRequested = new EventEmitter<{ surfaceCode: string; findingType: OdontogramSurfaceFindingType }>();
  @Output() readonly findingRemoveRequested = new EventEmitter<{ surfaceCode: string; findingId: string }>();

  readonly statuses = ODONTOGRAM_TOOTH_STATUSES;
  readonly surfaceCodes = ODONTOGRAM_SURFACE_CODES;
  readonly surfaceStatuses = ODONTOGRAM_SURFACE_STATUSES;
  readonly findingTypes = ODONTOGRAM_SURFACE_FINDING_TYPES;
  selectedStatus: OdontogramToothStatus = 'Unknown';
  selectedSurfaceCode: OdontogramSurfaceCode | null = null;
  selectedSurfaceStatus: OdontogramSurfaceStatus = 'Unknown';
  selectedFindingType: OdontogramSurfaceFindingType = 'Caries';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['tooth'] && this.tooth) {
      this.selectedStatus = this.tooth.status;
      this.selectedSurfaceCode = this.tooth.surfaces[0]?.surfaceCode ?? null;
      this.selectedSurfaceStatus = this.selectedSurface?.status ?? 'Unknown';
      this.selectedFindingType = this.selectedSurface?.findings[0]?.findingType ?? 'Caries';
    }
  }

  get selectedSurface(): OdontogramSurfaceState | null {
    if (!this.tooth || !this.selectedSurfaceCode) {
      return null;
    }

    return this.tooth.surfaces.find((surface) => surface.surfaceCode === this.selectedSurfaceCode) ?? null;
  }

  selectSurface(surfaceCode: OdontogramSurfaceCode): void {
    this.selectedSurfaceCode = surfaceCode;
    this.selectedSurfaceStatus = this.selectedSurface?.status ?? 'Unknown';
  }

  onSurfaceSelectionChange(surfaceCode: OdontogramSurfaceCode): void {
    this.selectSurface(surfaceCode);
  }

  emitSurfaceUpdate(): void {
    if (!this.selectedSurfaceCode) {
      return;
    }

    this.surfaceUpdateRequested.emit({
      surfaceCode: this.selectedSurfaceCode,
      status: this.selectedSurfaceStatus
    });
  }

  surfaceHasFinding(findingType: OdontogramSurfaceFindingType): boolean {
    return this.selectedSurface?.findings.some((finding) => finding.findingType === findingType) ?? false;
  }

  emitFindingAdd(): void {
    if (!this.selectedSurfaceCode) {
      return;
    }

    this.findingAddRequested.emit({
      surfaceCode: this.selectedSurfaceCode,
      findingType: this.selectedFindingType
    });
  }

  emitFindingRemove(findingId: string): void {
    if (!this.selectedSurfaceCode) {
      return;
    }

    this.findingRemoveRequested.emit({
      surfaceCode: this.selectedSurfaceCode,
      findingId
    });
  }

  getStatusLabel(status: OdontogramToothStatus | OdontogramSurfaceStatus): string {
    return this.i18n.translate(status);
  }

  getFindingTypeLabel(findingType: OdontogramSurfaceFindingType): string {
    return this.i18n.translate(findingType);
  }
}
