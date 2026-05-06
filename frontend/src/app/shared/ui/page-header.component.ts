import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="bsm-page-header" [attr.aria-labelledby]="titleId">
      <div class="bsm-page-header__copy">
        <p *ngIf="eyebrow" class="bsm-page-header__eyebrow">{{ eyebrow }}</p>
        <h1 class="bsm-page-header__title" [id]="titleId">{{ title }}</h1>
        <p *ngIf="subtitle" class="bsm-page-header__subtitle">{{ subtitle }}</p>
      </div>

      <div class="bsm-page-header__actions">
        <ng-content select="[page-header-actions]"></ng-content>
      </div>
    </header>
  `,
  styles: [`
    :host {
      display: block;
    }

    .bsm-page-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
      padding: 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-sm);
    }

    .bsm-page-header__copy {
      min-width: 0;
      display: grid;
      gap: 0.35rem;
    }

    .bsm-page-header__eyebrow,
    .bsm-page-header__subtitle,
    .bsm-page-header__title {
      margin: 0;
    }

    .bsm-page-header__eyebrow {
      color: var(--bsm-color-accent-accessible);
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    .bsm-page-header__title {
      color: var(--bsm-color-text-brand);
      font-size: 1.55rem;
      line-height: 1.16;
    }

    .bsm-page-header__subtitle {
      max-width: 72ch;
      color: var(--bsm-color-text-muted);
      font-size: 0.98rem;
      line-height: 1.45;
    }

    .bsm-page-header__actions {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 0.65rem;
      flex-wrap: wrap;
    }

    .bsm-page-header__actions:empty {
      display: none;
    }

    @media (max-width: 720px) {
      .bsm-page-header {
        flex-direction: column;
        align-items: stretch;
      }

      .bsm-page-header__actions {
        justify-content: flex-start;
      }
    }
  `]
})
export class PageHeaderComponent {
  private static nextId = 0;

  @Input({ required: true }) title = '';
  @Input() subtitle = '';
  @Input() eyebrow = '';

  readonly titleId = `bsm-page-header-title-${PageHeaderComponent.nextId++}`;
}
