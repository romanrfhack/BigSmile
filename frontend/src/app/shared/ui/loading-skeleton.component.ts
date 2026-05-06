import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export type LoadingSkeletonVariant = 'text' | 'card' | 'table-row';

@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section
      class="bsm-loading-skeleton"
      [attr.data-variant]="variant"
      role="status"
      aria-live="polite"
      aria-busy="true"
      [attr.aria-label]="ariaLabel">
      <span class="bsm-loading-skeleton__sr-only">{{ ariaLabel }}</span>

      <ng-container [ngSwitch]="variant">
        <ng-container *ngSwitchCase="'card'">
          <span class="bsm-loading-skeleton__block bsm-loading-skeleton__block--title"></span>
          <span
            *ngFor="let line of textLines; let isLast = last"
            class="bsm-loading-skeleton__block"
            [class.bsm-loading-skeleton__block--short]="isLast">
          </span>
        </ng-container>

        <ng-container *ngSwitchCase="'table-row'">
          <span
            *ngFor="let cell of tableCells"
            class="bsm-loading-skeleton__block bsm-loading-skeleton__block--cell">
          </span>
        </ng-container>

        <ng-container *ngSwitchDefault>
          <span
            *ngFor="let line of textLines; let isLast = last"
            class="bsm-loading-skeleton__block"
            [class.bsm-loading-skeleton__block--short]="isLast">
          </span>
        </ng-container>
      </ng-container>
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .bsm-loading-skeleton {
      display: grid;
      gap: 0.65rem;
      width: 100%;
    }

    .bsm-loading-skeleton[data-variant="card"] {
      padding: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
      box-shadow: var(--bsm-shadow-sm);
    }

    .bsm-loading-skeleton[data-variant="table-row"] {
      grid-template-columns: repeat(4, minmax(0, 1fr));
      align-items: center;
      padding: 0.75rem 0;
      border-bottom: 1px solid var(--bsm-color-border);
    }

    .bsm-loading-skeleton__block {
      min-height: 0.85rem;
      border-radius: var(--bsm-radius-sm);
      background:
        linear-gradient(
          90deg,
          var(--bsm-color-skeleton-base),
          var(--bsm-color-skeleton-highlight),
          var(--bsm-color-skeleton-base)
        );
      background-size: 220% 100%;
      animation: bsm-skeleton-shimmer var(--bsm-motion-skeleton) var(--bsm-ease-standard) infinite;
    }

    .bsm-loading-skeleton__block--title {
      width: 44%;
      min-height: 1.15rem;
    }

    .bsm-loading-skeleton__block--short {
      width: 62%;
    }

    .bsm-loading-skeleton__block--cell {
      min-height: 1rem;
    }

    .bsm-loading-skeleton__sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border: 0;
    }

    @keyframes bsm-skeleton-shimmer {
      from {
        background-position: 120% 0;
      }

      to {
        background-position: -120% 0;
      }
    }

    @media (prefers-reduced-motion: reduce) {
      .bsm-loading-skeleton__block {
        animation: none;
        background: var(--bsm-color-skeleton-base);
      }
    }

    @media (max-width: 640px) {
      .bsm-loading-skeleton[data-variant="table-row"] {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class LoadingSkeletonComponent {
  @Input() variant: LoadingSkeletonVariant = 'text';
  @Input() lines = 3;
  @Input() ariaLabel = 'Loading content';

  readonly tableCells = [0, 1, 2, 3];

  get textLines(): number[] {
    const count = Math.max(1, Math.min(this.lines, 8));

    return Array.from({ length: count }, (_, index) => index);
  }
}
