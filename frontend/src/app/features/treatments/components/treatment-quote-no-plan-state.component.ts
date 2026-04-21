import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-treatment-quote-no-plan-state',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="empty-state">
      <p class="eyebrow">Release 5.2 / Quote Basics</p>
      <h3>Treatment plan required first</h3>
      <p>
        This slice never auto-creates the quote or the treatment plan. Create the patient treatment plan first, then return here to snapshot it into a quote.
      </p>

      <a
        *ngIf="patientId"
        [routerLink]="['/patients', patientId, 'treatment-plan']"
        class="action-link">
        Open treatment plan
      </a>
    </section>
  `,
  styles: [`
    .empty-state {
      display: grid;
      gap: 0.85rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
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

    p {
      margin: 0;
      color: #5b6e84;
      max-width: 58ch;
    }

    .action-link {
      justify-self: start;
      text-decoration: none;
      color: #0a5bb5;
      font-weight: 700;
    }
  `]
})
export class TreatmentQuoteNoPlanStateComponent {
  @Input() patientId: string | null = null;
}
