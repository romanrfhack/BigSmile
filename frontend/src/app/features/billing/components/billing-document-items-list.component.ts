import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { BillingDocumentItem } from '../models/billing-document.models';

@Component({
  selector: 'app-billing-document-items-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="items-shell">
      <div class="section-head">
        <div>
          <p class="eyebrow">Billing lines</p>
          <h3>Snapshot lines</h3>
          <p class="copy">Each line is copied explicitly from the accepted quote and remains read-only in this slice.</p>
        </div>
      </div>

      <ul class="items-list">
        <li *ngFor="let item of items" class="item-card">
          <div class="item-head">
            <div>
              <h4>{{ item.title }}</h4>
              <p class="meta">
                {{ item.category || 'Uncategorized' }} · Qty {{ item.quantity }}
              </p>
            </div>

            <div class="totals">
              <span class="amount">{{ item.lineTotal | number: '1.2-2' }} {{ currencyCode }}</span>
              <small>Line total</small>
            </div>
          </div>

          <p *ngIf="item.notes" class="notes">{{ item.notes }}</p>

          <dl class="location-grid">
            <div>
              <dt>Dental reference</dt>
              <dd>
                <ng-container *ngIf="item.toothCode; else noDentalReference">
                  Tooth {{ item.toothCode }}<span *ngIf="item.surfaceCode"> / Surface {{ item.surfaceCode }}</span>
                </ng-container>
                <ng-template #noDentalReference>General item</ng-template>
              </dd>
            </div>
            <div>
              <dt>Source quote item</dt>
              <dd>{{ item.sourceTreatmentQuoteItemId }}</dd>
            </div>
            <div>
              <dt>Unit price</dt>
              <dd>{{ item.unitPrice | number: '1.2-2' }} {{ currencyCode }}</dd>
            </div>
            <div>
              <dt>Created</dt>
              <dd>{{ item.createdAtUtc | date: 'medium' }}</dd>
            </div>
            <div>
              <dt>Created by</dt>
              <dd>{{ item.createdByUserId }}</dd>
            </div>
          </dl>
        </li>
      </ul>
    </section>
  `,
  styles: [`
    .items-shell {
      display: grid;
      gap: 1rem;
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
    }

    .eyebrow {
      margin: 0 0 0.4rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #56708d;
      font-size: 0.8rem;
      font-weight: 700;
    }

    h3,
    h4 {
      margin: 0;
      color: #16324f;
    }

    .copy,
    .meta,
    .notes {
      margin: 0.35rem 0 0;
      color: #5b6e84;
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
      color: #16324f;
      font-size: 1.1rem;
      font-weight: 800;
      white-space: nowrap;
    }

    small {
      color: #718298;
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
      color: #718298;
    }

    dd {
      margin: 0;
      color: #16324f;
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
export class BillingDocumentItemsListComponent {
  @Input() items: BillingDocumentItem[] = [];
  @Input() currencyCode = 'MXN';
}
