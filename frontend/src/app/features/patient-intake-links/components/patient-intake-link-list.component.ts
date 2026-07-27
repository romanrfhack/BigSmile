import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BranchSummary } from '../../../core/auth/auth.service';
import { TranslatePipe } from '../../../shared/i18n';
import {
  PatientIntakeAccessLinkSummary,
  isActiveWaitingRoomLink
} from '../models/patient-intake-link.models';

@Component({
  selector: 'app-patient-intake-link-list',
  standalone: true,
  imports: [DatePipe, TranslatePipe],
  template: `
    <section class="link-list" aria-labelledby="waiting-room-link-list-title">
      <header class="link-list__header">
        <div>
          <p class="link-list__eyebrow">{{ 'Operational control' | t }}</p>
          <h2 id="waiting-room-link-list-title">{{ 'Recent links' | t }}</h2>
          <p>{{ 'The list contains metadata only. It never reconstructs or displays the token again.' | t }}</p>
        </div>
        @if (loading) {
          <span class="link-list__loading" aria-live="polite">{{ 'Updating...' | t }}</span>
        }
      </header>

      @if (!loading && links.length === 0) {
        <p class="link-list__empty">{{ 'No waiting-room links have been registered yet.' | t }}</p>
      } @else {
        <div class="link-list__table-wrap">
          <table>
            <thead>
              <tr>
                <th>{{ 'Status' | t }}</th>
                <th>{{ 'Branch' | t }}</th>
                <th>{{ 'Created' | t }}</th>
                <th>{{ 'Expires' | t }}</th>
                <th><span class="sr-only">{{ 'Actions' | t }}</span></th>
              </tr>
            </thead>
            <tbody>
              @for (link of links; track link.id) {
                <tr>
                  <td>
                    <span class="status" [class]="statusClass(link)">{{ statusLabel(link) | t }}</span>
                  </td>
                  <td>{{ branchLabel(link.branchId) | t }}</td>
                  <td>{{ link.createdAtUtc | date:'short' }}</td>
                  <td>{{ link.expiresAtUtc | date:'short' }}</td>
                  <td class="link-list__actions">
                    @if (isActive(link)) {
                      <button
                        type="button"
                        class="revoke-button"
                        [disabled]="revokingId === link.id"
                        (click)="revoke.emit(link)">
                        {{ (revokingId === link.id ? 'Revoking...' : 'Revoke') | t }}
                      </button>
                    } @else {
                      <span class="resolved-label">{{ 'Resolved' | t }}</span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .link-list {
      display: grid;
      gap: 1rem;
      padding: clamp(1rem, 2vw, 1.5rem);
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-lg);
      background: var(--bsm-color-bg);
      box-shadow: var(--bsm-shadow-sm);
    }

    .link-list__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
    }

    .link-list__eyebrow {
      margin: 0 0 0.25rem;
      color: var(--bsm-color-accent-dark);
      font-size: 0.78rem;
      font-weight: 900;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    h2,
    p {
      margin: 0;
    }

    h2 {
      color: var(--bsm-color-text-brand);
    }

    .link-list__header p:not(.link-list__eyebrow),
    .link-list__empty,
    .link-list__loading,
    .resolved-label {
      color: var(--bsm-color-text-muted);
    }

    .link-list__table-wrap {
      overflow-x: auto;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      min-width: 44rem;
    }

    th,
    td {
      padding: 0.75rem;
      border-bottom: 1px solid var(--bsm-color-border);
      text-align: left;
      vertical-align: middle;
    }

    th {
      color: var(--bsm-color-text-muted);
      font-size: 0.75rem;
      letter-spacing: 0.04em;
      text-transform: uppercase;
    }

    td {
      color: var(--bsm-color-text);
    }

    .status {
      display: inline-flex;
      border-radius: var(--bsm-radius-pill);
      padding: 0.35rem 0.6rem;
      font-size: 0.78rem;
      font-weight: 900;
    }

    .status--active {
      background: var(--bsm-color-success-soft);
      color: var(--bsm-color-success-text);
    }

    .status--expired,
    .status--pending {
      background: var(--bsm-color-warning-soft);
      color: var(--bsm-color-warning-text);
    }

    .status--revoked,
    .status--consumed {
      background: var(--bsm-color-neutral-soft);
      color: var(--bsm-color-text-muted);
    }

    .link-list__actions {
      text-align: right;
    }

    .revoke-button {
      border: 1px solid var(--bsm-color-danger);
      border-radius: var(--bsm-radius-pill);
      padding: 0.45rem 0.75rem;
      background: transparent;
      color: var(--bsm-color-danger);
      font-weight: 800;
      cursor: pointer;
    }

    .revoke-button:disabled {
      opacity: 0.65;
    }

    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border: 0;
    }
  `]
})
export class PatientIntakeLinkListComponent {
  @Input() links: PatientIntakeAccessLinkSummary[] = [];
  @Input() branches: BranchSummary[] = [];
  @Input() loading = false;
  @Input() revokingId: string | null = null;
  @Output() readonly revoke = new EventEmitter<PatientIntakeAccessLinkSummary>();

  isActive(link: PatientIntakeAccessLinkSummary): boolean {
    return isActiveWaitingRoomLink(link);
  }

  branchLabel(branchId: string | null): string {
    if (!branchId) {
      return 'No specific branch';
    }

    return this.branches.find(branch => branch.id === branchId)?.name ?? 'Branch unavailable';
  }

  statusLabel(link: PatientIntakeAccessLinkSummary): string {
    switch (link.status.toLowerCase()) {
      case 'active':
        return 'Active';
      case 'expired':
        return 'Expired';
      case 'revoked':
        return 'Revoked';
      case 'consumed':
        return 'Consumed';
      default:
        return 'Pending';
    }
  }

  statusClass(link: PatientIntakeAccessLinkSummary): string {
    const normalized = link.status.toLowerCase();
    return `status status--${['active', 'expired', 'revoked', 'consumed'].includes(normalized) ? normalized : 'pending'}`;
  }
}
