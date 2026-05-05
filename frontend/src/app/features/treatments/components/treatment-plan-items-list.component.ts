import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LocalizedDatePipe, TranslatePipe } from '../../../shared/i18n';
import { TreatmentPlanItem } from '../models/treatment-plan.models';

@Component({
  selector: 'app-treatment-plan-items-list',
  standalone: true,
  imports: [CommonModule, LocalizedDatePipe, TranslatePipe],
  template: `
    <section class="items-shell">
      <div class="section-head">
        <div>
          <p class="eyebrow">{{ 'Current items' | t }}</p>
          <h3>{{ 'Treatment plan items' | t }}</h3>
        </div>
      </div>

      <div *ngIf="!items.length" class="empty-list">
        {{ 'No items have been added yet.' | t }}
      </div>

      <ul *ngIf="items.length" class="items-list">
        <li *ngFor="let item of items" class="item-card">
          <div class="item-head">
            <div>
              <h4>{{ item.title }}</h4>
              <p class="meta">
                {{ item.category || ('Uncategorized' | t) }} / {{ 'Qty' | t }} {{ item.quantity }}
              </p>
            </div>

            <button
              *ngIf="canRemove"
              type="button"
              class="remove-btn"
              [disabled]="removingItemId === item.itemId"
              (click)="removeRequested.emit(item.itemId)">
              {{ (removingItemId === item.itemId ? 'Removing...' : 'Remove') | t }}
            </button>
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
              <dt>{{ 'Created' | t }}</dt>
              <dd>{{ item.createdAtUtc | bsDate: 'medium' }}</dd>
            </div>
            <div>
              <dt>{{ 'Created by' | t }}</dt>
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

    .meta,
    .notes,
    .empty-list {
      margin: 0;
      color: #5b6e84;
    }

    .remove-btn {
      border: 1px solid #d8a8a8;
      border-radius: 999px;
      background: #fff4f4;
      color: #8d292d;
      padding: 0.7rem 1rem;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
      white-space: nowrap;
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
    }
  `]
})
export class TreatmentPlanItemsListComponent {
  @Input() items: TreatmentPlanItem[] = [];
  @Input() canRemove = false;
  @Input() removingItemId: string | null = null;

  @Output() removeRequested = new EventEmitter<string>();
}
