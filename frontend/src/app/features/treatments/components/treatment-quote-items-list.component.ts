import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { QuoteItemPriceEditorComponent } from './quote-item-price-editor.component';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { TreatmentQuoteItem } from '../models/treatment-quote.models';

@Component({
  selector: 'app-treatment-quote-items-list',
  standalone: true,
  imports: [CommonModule, QuoteItemPriceEditorComponent, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="items-shell">
      <div class="section-head">
        <div>
          <p class="eyebrow">{{ 'Quote items' | t }}</p>
          <h3>{{ 'Commercial snapshot' | t }}</h3>
          <p class="copy">{{ 'Each line is copied explicitly from the current treatment plan and priced independently in this quote.' | t }}</p>
        </div>
      </div>

      <ul class="items-list">
        <li *ngFor="let item of items" class="item-card">
          <div class="item-head">
            <div>
              <h4>{{ item.title }}</h4>
              <p class="meta">
                {{ item.category || ('Uncategorized' | t) }} / {{ 'Qty' | t }} {{ item.quantity }}
              </p>
            </div>

            <div class="totals">
              <span class="amount">{{ item.lineTotal | number: '1.2-2' }} {{ currencyCode }}</span>
              <small>{{ 'Line total' | t }}</small>
            </div>
          </div>

          <p *ngIf="item.notes" class="notes">{{ item.notes }}</p>

          <dl class="location-grid">
            <div>
              <dt>{{ 'Dental reference' | t }}</dt>
              <dd>
                <ng-container *ngIf="item.toothCode; else noDentalReference">
                  {{ 'Tooth' | t }} {{ item.toothCode }}<span *ngIf="item.surfaceCode"> / {{ 'Surface' | t }} {{ item.surfaceCode }}</span>
                </ng-container>
                <ng-template #noDentalReference>{{ 'General item' | t }}</ng-template>
              </dd>
            </div>
            <div>
              <dt>{{ 'Source plan item' | t }}</dt>
              <dd>{{ item.sourceTreatmentPlanItemId }}</dd>
            </div>
            <div>
              <dt>{{ 'Created' | t }}</dt>
              <dd>{{ item.createdAtUtc | bsDate: 'medium' }}</dd>
            </div>
            <div>
              <dt>{{ 'Created by' | t }}</dt>
              <dd>{{ item.createdByUserId }}</dd>
            </div>
          </dl>

          <app-quote-item-price-editor
            [unitPrice]="item.unitPrice"
            [currencyCode]="currencyCode"
            [canEdit]="canEdit"
            [saving]="savingItemId === item.quoteItemId"
            (priceSaved)="priceUpdateRequested.emit({ quoteItemId: item.quoteItemId, unitPrice: $event })">
          </app-quote-item-price-editor>
        </li>
      </ul>
    </section>
  `,
  styles: [`
    .items-shell {
      display: grid;
      gap: 1rem;
      border-radius: 20px;
      border: 1px solid var(--bsm-color-border);
      background: linear-gradient(180deg, #ffffff 0%, var(--bsm-color-surface) 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-accent-accessible);
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3,
    h4 {
      margin: 0;
      color: var(--bsm-color-text-brand);
    }

    .copy,
    .meta,
    .notes {
      margin: 0.35rem 0 0;
      color: var(--bsm-color-text-muted);
    }

    .items-list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: grid;
      gap: 1rem;
    }

    .item-card {
      border-radius: 16px;
      border: 1px solid #dde6ee;
      background: #ffffff;
      padding: 1rem 1.1rem;
      display: grid;
      gap: 0.85rem;
    }

    .item-head {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .totals {
      display: grid;
      justify-items: end;
      gap: 0.2rem;
    }

    .amount {
      color: var(--bsm-color-text-brand);
      font-size: 1.1rem;
      font-weight: 800;
      white-space: nowrap;
    }

    small {
      color: var(--bsm-color-text-muted);
      text-transform: uppercase;
      letter-spacing: 0.08em;
      font-size: 0.7rem;
    }

    .location-grid {
      display: grid;
      gap: 0.85rem;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      margin: 0;
    }

    dt {
      margin-bottom: 0.25rem;
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--bsm-color-text-muted);
    }

    dd {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-weight: 600;
      word-break: break-word;
    }

    @media (max-width: 768px) {
      .item-head {
        flex-direction: column;
      }

      .totals {
        justify-items: start;
      }
    }
  `]
})
export class TreatmentQuoteItemsListComponent {
  @Input() items: TreatmentQuoteItem[] = [];
  @Input() currencyCode = 'MXN';
  @Input() canEdit = false;
  @Input() savingItemId: string | null = null;

  @Output() priceUpdateRequested = new EventEmitter<{ quoteItemId: string; unitPrice: number }>();
}
