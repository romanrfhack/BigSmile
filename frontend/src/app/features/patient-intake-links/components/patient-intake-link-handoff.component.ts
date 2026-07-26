import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import {
  WaitingRoomCopyState,
  WaitingRoomHandoff
} from '../models/patient-intake-link.models';
import { LocalQrCodeComponent } from './local-qr-code.component';

@Component({
  selector: 'app-patient-intake-link-handoff',
  standalone: true,
  imports: [DatePipe, LocalQrCodeComponent],
  template: `
    <section class="handoff bsm-print-surface" aria-labelledby="waiting-room-handoff-title">
      <header class="handoff__header">
        <div>
          <p class="handoff__eyebrow">Entrega inmediata</p>
          <h2 id="waiting-room-handoff-title">Enlace de sala de espera listo</h2>
          <p>
            Este enlace solo se muestra durante esta sesión. Al salir o recargar la página deberá
            generarse uno nuevo.
          </p>
        </div>
        <span class="handoff__badge">Un solo uso</span>
      </header>

      <div class="handoff__layout">
        <div class="handoff__details">
          <dl>
            <div>
              <dt>Clínica</dt>
              <dd>{{ handoff.clinicName }}</dd>
            </div>
            <div>
              <dt>Sucursal</dt>
              <dd>{{ handoff.branchName || 'Sin sucursal específica' }}</dd>
            </div>
            <div>
              <dt>Vence</dt>
              <dd>{{ handoff.expiresAtUtc | date:'medium' }}</dd>
            </div>
          </dl>

          <div class="handoff__url">
            <span>Enlace de activación</span>
            <code>{{ handoff.url }}</code>
          </div>

          <ol class="handoff__instructions">
            <li>Entrega esta hoja directamente al paciente.</li>
            <li>El paciente escanea el código o abre el enlace en su dispositivo.</li>
            <li>El enlace deja de funcionar al activarse, revocarse o vencer.</li>
          </ol>
        </div>

        <div class="handoff__qr">
          <app-local-qr-code [value]="handoff.url" />
          <p>Escanear para activar acceso privado</p>
        </div>
      </div>

      <footer class="handoff__actions bsm-no-print">
        <button type="button" class="button button--primary" (click)="copyRequested.emit()">
          {{ copyLabel }}
        </button>
        <button type="button" class="button button--secondary" (click)="printRequested.emit()">
          Imprimir hoja
        </button>
        <button type="button" class="button button--quiet" (click)="dismissRequested.emit()">
          Ocultar enlace
        </button>
      </footer>

      <p class="handoff__security-note">
        No contiene información clínica. El código QR representa exactamente el enlace one-time
        mostrado arriba y se genera de forma local.
      </p>
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .handoff {
      display: grid;
      gap: 1.25rem;
      padding: clamp(1.1rem, 3vw, 2rem);
      border: 2px solid var(--bsm-color-accent-accessible);
      border-radius: var(--bsm-radius-lg);
      background: var(--bsm-color-bg);
      box-shadow: var(--bsm-shadow-md);
    }

    .handoff__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
    }

    .handoff__eyebrow {
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

    .handoff__header p:not(.handoff__eyebrow),
    .handoff__security-note {
      color: var(--bsm-color-text-muted);
      line-height: 1.5;
    }

    .handoff__badge {
      flex: 0 0 auto;
      border-radius: var(--bsm-radius-pill);
      padding: 0.4rem 0.7rem;
      background: var(--bsm-color-accent-soft);
      color: var(--bsm-color-accent-dark);
      font-size: 0.78rem;
      font-weight: 900;
    }

    .handoff__layout {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(14rem, 18rem);
      gap: clamp(1rem, 4vw, 2.5rem);
      align-items: center;
    }

    .handoff__details {
      display: grid;
      gap: 1rem;
      min-width: 0;
    }

    dl {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 0.75rem;
      margin: 0;
    }

    dl div {
      min-width: 0;
      padding: 0.75rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
    }

    dt {
      margin-bottom: 0.25rem;
      color: var(--bsm-color-text-muted);
      font-size: 0.72rem;
      font-weight: 900;
      text-transform: uppercase;
    }

    dd {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-weight: 800;
      overflow-wrap: anywhere;
    }

    .handoff__url {
      display: grid;
      gap: 0.4rem;
    }

    .handoff__url span {
      color: var(--bsm-color-text-brand);
      font-weight: 900;
    }

    code {
      display: block;
      padding: 0.85rem;
      border: 1px dashed var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-neutral-soft);
      color: var(--bsm-color-text);
      line-height: 1.5;
      overflow-wrap: anywhere;
      word-break: break-all;
      user-select: all;
    }

    .handoff__instructions {
      margin: 0;
      padding-left: 1.25rem;
      color: var(--bsm-color-text);
      line-height: 1.6;
    }

    .handoff__qr {
      display: grid;
      justify-items: center;
      gap: 0.65rem;
    }

    .handoff__qr p {
      color: var(--bsm-color-text-muted);
      font-size: 0.85rem;
      font-weight: 800;
      text-align: center;
    }

    .handoff__actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .button {
      border: 0;
      border-radius: var(--bsm-radius-pill);
      padding: 0.7rem 1rem;
      font-weight: 800;
      cursor: pointer;
    }

    .button--primary {
      background: var(--bsm-color-primary);
      color: #ffffff;
    }

    .button--secondary {
      background: var(--bsm-color-primary-soft);
      color: var(--bsm-color-primary-dark);
    }

    .button--quiet {
      background: transparent;
      color: var(--bsm-color-text-muted);
      border: 1px solid var(--bsm-color-border);
    }

    .handoff__security-note {
      font-size: 0.82rem;
    }

    @media (max-width: 820px) {
      .handoff__layout,
      dl {
        grid-template-columns: 1fr;
      }

      .handoff__header {
        flex-direction: column;
      }
    }

    @media (max-width: 560px) {
      .handoff__actions,
      .button {
        width: 100%;
      }
    }

    @media print {
      .handoff {
        border: 0;
        box-shadow: none;
        padding: 0;
      }

      .handoff__layout {
        grid-template-columns: minmax(0, 1fr) 15rem;
      }

      .handoff__security-note {
        margin-top: 0.5rem;
      }
    }
  `]
})
export class PatientIntakeLinkHandoffComponent {
  @Input({ required: true }) handoff!: WaitingRoomHandoff;
  @Input() copyState: WaitingRoomCopyState = 'idle';
  @Output() readonly copyRequested = new EventEmitter<void>();
  @Output() readonly printRequested = new EventEmitter<void>();
  @Output() readonly dismissRequested = new EventEmitter<void>();

  get copyLabel(): string {
    switch (this.copyState) {
      case 'copying':
        return 'Copiando…';
      case 'copied':
        return 'Enlace copiado';
      case 'error':
        return 'No se pudo copiar';
      default:
        return 'Copiar enlace';
    }
  }
}
