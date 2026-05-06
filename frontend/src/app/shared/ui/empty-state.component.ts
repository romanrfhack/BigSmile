import { Component, Input } from '@angular/core';

export type EmptyStateAriaLive = 'off' | 'polite' | 'assertive';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <section
      class="bsm-empty-state"
      [attr.aria-labelledby]="titleId"
      [attr.aria-live]="ariaLive">
      <span class="bsm-empty-state__icon" aria-hidden="true">{{ icon || 'i' }}</span>

      <div class="bsm-empty-state__copy">
        <h2 class="bsm-empty-state__title" [id]="titleId">{{ title }}</h2>
        <p class="bsm-empty-state__description">{{ description }}</p>
      </div>

      <div class="bsm-empty-state__action">
        <ng-content select="[empty-state-action]"></ng-content>
      </div>
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .bsm-empty-state {
      display: grid;
      justify-items: start;
      gap: 0.9rem;
      padding: 1.25rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-sm);
    }

    .bsm-empty-state__icon {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 2.6rem;
      height: 2.6rem;
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-accent-soft);
      color: var(--bsm-color-accent-dark);
      font-weight: 900;
    }

    .bsm-empty-state__copy {
      display: grid;
      gap: 0.35rem;
    }

    .bsm-empty-state__title,
    .bsm-empty-state__description {
      margin: 0;
    }

    .bsm-empty-state__title {
      color: var(--bsm-color-text-brand);
      font-size: 1.1rem;
      line-height: 1.25;
    }

    .bsm-empty-state__description {
      max-width: 66ch;
      color: var(--bsm-color-text-muted);
      font-size: 0.95rem;
      line-height: 1.45;
    }

    .bsm-empty-state__action {
      display: flex;
      flex-wrap: wrap;
      gap: 0.55rem;
    }

    .bsm-empty-state__action:empty {
      display: none;
    }
  `]
})
export class EmptyStateComponent {
  private static nextId = 0;

  @Input({ required: true }) title = '';
  @Input({ required: true }) description = '';
  @Input() icon = '';
  @Input() ariaLive: EmptyStateAriaLive = 'polite';

  readonly titleId = `bsm-empty-state-title-${EmptyStateComponent.nextId++}`;
}
