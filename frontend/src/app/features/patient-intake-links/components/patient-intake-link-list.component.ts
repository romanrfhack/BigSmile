import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BranchSummary } from '../../../core/auth/auth.service';
import {
  PatientIntakeAccessLinkSummary,
  isActiveWaitingRoomLink
} from '../models/patient-intake-link.models';

@Component({
  selector: 'app-patient-intake-link-list',
  standalone: true,
  imports: [DatePipe],
  template: `
    <section class="link-list" aria-labelledby="waiting-room-link-list-title">
      <header class="link-list__header">
        <div>
          <p class="link-list__eyebrow">Control operativo</p>
          <h2 id="waiting-room-link-list-title">Enlaces recientes</h2>
          <p>El listado conserva solo metadata. Nunca reconstruye ni vuelve a mostrar el token.</p>
        </div>
        @if (loading) {
          <span class="link-list__loading" aria-live="polite">Actualizando…</span>
        }
      </header>

      @if (!loading && links.length === 0) {
        <p class="link-list__empty">Aún no hay enlaces de sala de espera registrados.</p>
      } @else {
        <div class="link-list__table-wrap">
          <table>
            <thead>
              <tr>
                <th>Estado</th>
                <th>Sucursal</th>
                <th>Creado</th>
                <th>Vence</th>
                <th><span class="sr-only">Acciones</span></th>
              </tr>
            </thead>
            <tbody>
              @for (link of links; track link.id) {
                <tr>
                  <td>
                    <span class="status" [class]="statusClass(link)">{{ statusLabel(link) }}</span>
                  </td>
                  <td>{{ branchLabel(link.branchId) }}</td>
                  <td>{{ link.createdAtUtc | date:'short' }}</td>
                  <td>{{ link.expiresAtUtc | date:'short' }}</td>
                  <td class="link-list__actions">
                    @if (isActive(link)) {
                      <button
                        type="button"
                        class="revoke-button"
                        [disabled]="revokingId === link.id"
                        (click)="revoke.emit(link)">
                        {{ revokingId === link.id ? 'Revocando…' : 'Revocar' }}
                      </button>
                    } @else {
                      <span class="resolved-label">Resuelto</span>
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
      return 'Sin sucursal específica';
    }

    return this.branches.find(branch => branch.id === branchId)?.name ?? 'Sucursal no disponible';
  }

  statusLabel(link: PatientIntakeAccessLinkSummary): string {
    switch (link.status.toLowerCase()) {
      case 'active':
        return 'Activo';
      case 'expired':
        return 'Vencido';
      case 'revoked':
        return 'Revocado';
      case 'consumed':
        return 'Consumido';
      default:
        return 'Pendiente';
    }
  }

  statusClass(link: PatientIntakeAccessLinkSummary): string {
    const normalized = link.status.toLowerCase();
    return `status status--${['active', 'expired', 'revoked', 'consumed'].includes(normalized) ? normalized : 'pending'}`;
  }
}
