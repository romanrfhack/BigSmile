import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-sticky-action-bar',
  standalone: true,
  template: `
    <section class="bsm-sticky-action-bar" role="region" [attr.aria-label]="ariaLabel">
      <div class="bsm-sticky-action-bar__content">
        <ng-content></ng-content>
      </div>
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .bsm-sticky-action-bar {
      position: sticky;
      bottom: 0;
      z-index: 10;
      margin-top: 1rem;
      padding: 0.75rem 0 max(0.75rem, env(safe-area-inset-bottom));
      background: var(--bsm-color-bg);
      border-top: 1px solid var(--bsm-color-border);
      box-shadow: var(--bsm-shadow-sm);
    }

    .bsm-sticky-action-bar__content {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 0.65rem;
      flex-wrap: wrap;
      width: 100%;
    }

    @media (max-width: 640px) {
      .bsm-sticky-action-bar {
        margin-inline: -0.75rem;
        padding-inline: 0.75rem;
      }

      .bsm-sticky-action-bar__content {
        justify-content: stretch;
      }
    }
  `]
})
export class StickyActionBarComponent {
  @Input() ariaLabel = 'Form actions';
}
