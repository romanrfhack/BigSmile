import { Component, Input } from '@angular/core';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-patient-portal-card',
  standalone: true,
  imports: [TranslatePipe],
  template: `
    <article class="patient-card">
      <header class="patient-card__header">
        @if (eyebrow) {
          <p class="patient-card__eyebrow">{{ eyebrow | t }}</p>
        }
        <h2>{{ title | t }}</h2>
        @if (description) {
          <p class="patient-card__description">{{ description | t }}</p>
        }
      </header>

      <ng-content />
    </article>
  `,
  styles: [`
    :host {
      display: block;
      width: min(100%, 500px);
    }

    .patient-card {
      position: relative;
      overflow: hidden;
      padding: clamp(1.35rem, 4vw, 2.25rem);
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-lg);
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-md);
    }

    .patient-card::before {
      content: '';
      position: absolute;
      inset: 0 0 auto;
      height: 4px;
      background: var(--bsm-gradient-brand);
    }

    .patient-card__header {
      margin-bottom: 1.4rem;
    }

    .patient-card__eyebrow {
      margin: 0 0 0.35rem;
      color: var(--bsm-color-accent-dark);
      font-weight: 800;
      font-size: 0.78rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    h2 {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: clamp(1.55rem, 4vw, 2rem);
      line-height: 1.15;
    }

    .patient-card__description {
      margin: 0.75rem 0 0;
      color: var(--bsm-color-text-muted);
      line-height: 1.55;
    }
  `]
})
export class PatientPortalCardComponent {
  @Input() eyebrow = '';
  @Input({ required: true }) title = '';
  @Input() description = '';
}
