import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export type SectionCardVariant = 'default' | 'elevated' | 'accent' | 'compact';

@Component({
  selector: 'app-section-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="bsm-section-card" [attr.data-variant]="variant">
      <header *ngIf="title || subtitle" class="bsm-section-card__header">
        <div class="bsm-section-card__copy">
          <h2 *ngIf="title" class="bsm-section-card__title">{{ title }}</h2>
          <p *ngIf="subtitle" class="bsm-section-card__subtitle">{{ subtitle }}</p>
        </div>

        <div class="bsm-section-card__actions">
          <ng-content select="[section-card-actions]"></ng-content>
        </div>
      </header>

      <div class="bsm-section-card__body">
        <ng-content></ng-content>
      </div>
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .bsm-section-card {
      display: grid;
      gap: 1rem;
      padding: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
      box-shadow: var(--bsm-shadow-sm);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard),
        transform var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .bsm-section-card:hover {
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-md);
      transform: translateY(-1px);
    }

    .bsm-section-card[data-variant="elevated"] {
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-md);
    }

    .bsm-section-card[data-variant="accent"] {
      border-color: var(--bsm-color-accent-accessible);
      background: var(--bsm-color-accent-soft);
    }

    .bsm-section-card[data-variant="compact"] {
      gap: 0.7rem;
      padding: 0.85rem;
      box-shadow: none;
    }

    .bsm-section-card__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
    }

    .bsm-section-card__copy {
      min-width: 0;
      display: grid;
      gap: 0.25rem;
    }

    .bsm-section-card__title,
    .bsm-section-card__subtitle {
      margin: 0;
    }

    .bsm-section-card__title {
      color: var(--bsm-color-text-brand);
      font-size: 1rem;
      line-height: 1.25;
    }

    .bsm-section-card__subtitle {
      color: var(--bsm-color-text-muted);
      font-size: 0.92rem;
      line-height: 1.4;
    }

    .bsm-section-card__actions {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 0.5rem;
      flex-wrap: wrap;
    }

    .bsm-section-card__actions:empty {
      display: none;
    }

    .bsm-section-card__body {
      min-width: 0;
    }

    @media (prefers-reduced-motion: reduce) {
      .bsm-section-card:hover {
        transform: none;
      }
    }

    @media (max-width: 640px) {
      .bsm-section-card__header {
        flex-direction: column;
      }

      .bsm-section-card__actions {
        justify-content: flex-start;
      }
    }
  `]
})
export class SectionCardComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() variant: SectionCardVariant = 'default';
}
