import { Component, Input } from '@angular/core';

export type StatusBadgeTone = 'neutral' | 'info' | 'success' | 'warning' | 'danger' | 'primary';
export type StatusBadgeSize = 'sm' | 'md';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `
    <span
      class="bsm-status-badge"
      [attr.data-tone]="tone"
      [attr.data-size]="size"
      [attr.aria-label]="label">
      <span class="bsm-status-badge__dot" aria-hidden="true"></span>
      <span class="bsm-status-badge__label">{{ label }}</span>
    </span>
  `,
  styles: [`
    :host {
      display: inline-flex;
      max-width: 100%;
      vertical-align: middle;
    }

    .bsm-status-badge {
      --badge-bg: var(--bsm-color-neutral-soft);
      --badge-fg: var(--bsm-color-text-muted);
      --badge-border: var(--bsm-color-border);

      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      max-width: 100%;
      border: 1px solid var(--badge-border);
      border-radius: var(--bsm-radius-pill);
      background: var(--badge-bg);
      color: var(--badge-fg);
      font-weight: 800;
      line-height: 1.2;
      white-space: nowrap;
    }

    .bsm-status-badge[data-size="sm"] {
      padding: 0.2rem 0.5rem;
      font-size: 0.74rem;
    }

    .bsm-status-badge[data-size="md"] {
      padding: 0.32rem 0.65rem;
      font-size: 0.82rem;
    }

    .bsm-status-badge[data-tone="primary"] {
      --badge-bg: var(--bsm-color-primary-soft);
      --badge-fg: var(--bsm-color-primary-dark);
      --badge-border: var(--bsm-color-primary-soft);
    }

    .bsm-status-badge[data-tone="info"] {
      --badge-bg: var(--bsm-color-accent-soft);
      --badge-fg: var(--bsm-color-accent-dark);
      --badge-border: var(--bsm-color-accent-soft);
    }

    .bsm-status-badge[data-tone="success"] {
      --badge-bg: var(--bsm-color-success-soft);
      --badge-fg: var(--bsm-color-success);
      --badge-border: var(--bsm-color-success-soft);
    }

    .bsm-status-badge[data-tone="warning"] {
      --badge-bg: var(--bsm-color-warning-soft);
      --badge-fg: var(--bsm-color-warning);
      --badge-border: var(--bsm-color-warning-soft);
    }

    .bsm-status-badge[data-tone="danger"] {
      --badge-bg: var(--bsm-color-danger-soft);
      --badge-fg: var(--bsm-color-danger);
      --badge-border: var(--bsm-color-danger-soft);
    }

    .bsm-status-badge__dot {
      width: 0.45rem;
      height: 0.45rem;
      flex: 0 0 auto;
      border-radius: var(--bsm-radius-pill);
      background: currentColor;
    }

    .bsm-status-badge__label {
      min-width: 0;
      overflow: hidden;
      text-overflow: ellipsis;
    }
  `]
})
export class StatusBadgeComponent {
  @Input({ required: true }) label = '';
  @Input() tone: StatusBadgeTone = 'neutral';
  @Input() size: StatusBadgeSize = 'md';
}
