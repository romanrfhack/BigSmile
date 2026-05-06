import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export type SummaryCardTone = 'primary' | 'accent' | 'neutral' | 'success' | 'warning' | 'danger';

@Component({
  selector: 'app-summary-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <article
      class="bsm-summary-card"
      [attr.data-tone]="tone"
      [attr.aria-label]="accessibleLabel">
      <span class="bsm-summary-card__mark" aria-hidden="true">{{ visualMark }}</span>

      <div class="bsm-summary-card__content">
        <p class="bsm-summary-card__label">{{ label }}</p>
        <strong class="bsm-summary-card__value">{{ value }}</strong>
        <p *ngIf="helper" class="bsm-summary-card__helper">{{ helper }}</p>
      </div>
    </article>
  `,
  styles: [`
    :host {
      display: block;
    }

    .bsm-summary-card {
      --summary-bg: var(--bsm-color-bg);
      --summary-fg: var(--bsm-color-text-brand);
      --summary-soft: var(--bsm-color-neutral-soft);

      display: grid;
      grid-template-columns: auto 1fr;
      gap: 0.8rem;
      min-height: 7.25rem;
      padding: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--summary-bg);
      box-shadow: var(--bsm-shadow-sm);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .bsm-summary-card:hover {
      border-color: var(--summary-fg);
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .bsm-summary-card[data-tone="primary"] {
      --summary-bg: var(--bsm-color-primary-soft);
      --summary-fg: var(--bsm-color-primary-dark);
      --summary-soft: var(--bsm-color-bg);
    }

    .bsm-summary-card[data-tone="accent"] {
      --summary-bg: var(--bsm-color-accent-soft);
      --summary-fg: var(--bsm-color-accent-dark);
      --summary-soft: var(--bsm-color-bg);
    }

    .bsm-summary-card[data-tone="success"] {
      --summary-bg: var(--bsm-color-success-soft);
      --summary-fg: var(--bsm-color-success);
      --summary-soft: var(--bsm-color-bg);
    }

    .bsm-summary-card[data-tone="warning"] {
      --summary-bg: var(--bsm-color-warning-soft);
      --summary-fg: var(--bsm-color-warning);
      --summary-soft: var(--bsm-color-bg);
    }

    .bsm-summary-card[data-tone="danger"] {
      --summary-bg: var(--bsm-color-danger-soft);
      --summary-fg: var(--bsm-color-danger);
      --summary-soft: var(--bsm-color-bg);
    }

    .bsm-summary-card__mark {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 2.4rem;
      height: 2.4rem;
      border-radius: var(--bsm-radius-sm);
      background: var(--summary-soft);
      color: var(--summary-fg);
      font-size: 0.9rem;
      font-weight: 800;
    }

    .bsm-summary-card__content {
      min-width: 0;
      display: grid;
      gap: 0.35rem;
    }

    .bsm-summary-card__label,
    .bsm-summary-card__helper {
      margin: 0;
    }

    .bsm-summary-card__label {
      color: var(--summary-fg);
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    .bsm-summary-card__value {
      color: var(--bsm-color-text-brand);
      font-size: 1.85rem;
      line-height: 1.1;
    }

    .bsm-summary-card__helper {
      color: var(--bsm-color-text-muted);
      font-size: 0.9rem;
      line-height: 1.35;
    }

    @media (prefers-reduced-motion: reduce) {
      .bsm-summary-card:hover {
        transform: none;
      }
    }
  `]
})
export class SummaryCardComponent {
  @Input({ required: true }) label = '';
  @Input({ required: true }) value: string | number = '';
  @Input() helper = '';
  @Input() icon = '';
  @Input() initial = '';
  @Input() tone: SummaryCardTone = 'neutral';

  get visualMark(): string {
    return this.icon || this.initial || this.label.trim().slice(0, 1).toUpperCase() || 'i';
  }

  get accessibleLabel(): string {
    const base = `${this.label}: ${this.value}`;

    return this.helper ? `${base}. ${this.helper}` : base;
  }
}
