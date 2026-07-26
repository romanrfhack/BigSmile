import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { PatientIntakeLinkHandoffComponent } from '../components/patient-intake-link-handoff.component';
import { PatientIntakeLinkIssueFormComponent } from '../components/patient-intake-link-issue-form.component';
import { PatientIntakeLinkListComponent } from '../components/patient-intake-link-list.component';
import { PatientIntakeLinksFacade } from '../facades/patient-intake-links.facade';
import { PatientIntakeAccessLinkSummary } from '../models/patient-intake-link.models';

@Component({
  selector: 'app-patient-intake-links-page',
  standalone: true,
  imports: [
    PatientIntakeLinkIssueFormComponent,
    PatientIntakeLinkHandoffComponent,
    PatientIntakeLinkListComponent
  ],
  providers: [PatientIntakeLinksFacade],
  template: `
    <section class="waiting-room-page">
      <header class="waiting-room-page__header">
        <div>
          <p class="waiting-room-page__eyebrow">Recepción</p>
          <h1>Acceso de sala de espera</h1>
          <p>
            Genera credenciales de un solo uso para pacientes nuevos. La captura se mantiene como
            intake pendiente y no modifica expedientes canónicos.
          </p>
        </div>
        <button type="button" class="refresh-button" (click)="facade.loadLinks()" [disabled]="facade.loading()">
          {{ facade.loading() ? 'Actualizando…' : 'Actualizar' }}
        </button>
      </header>

      @if (facade.error()) {
        <div class="waiting-room-page__alert" role="alert" aria-live="polite">
          <span>{{ facade.error() }}</span>
          <button type="button" (click)="facade.clearError()" aria-label="Cerrar mensaje">×</button>
        </div>
      }

      <app-patient-intake-link-issue-form
        [branches]="facade.branches()"
        [issuing]="facade.issuing()"
        (issue)="facade.issue($event)" />

      @if (facade.handoff(); as handoff) {
        <app-patient-intake-link-handoff
          [handoff]="handoff"
          [copyState]="facade.copyState()"
          (copyRequested)="copyHandoff()"
          (printRequested)="facade.printHandoff()"
          (dismissRequested)="facade.clearHandoff()" />
      }

      <app-patient-intake-link-list
        [links]="facade.links()"
        [branches]="facade.branches()"
        [loading]="facade.loading()"
        [revokingId]="facade.revokingId()"
        (revoke)="revoke($event)" />
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .waiting-room-page {
      display: grid;
      gap: 1.25rem;
      width: min(100%, 1180px);
      margin-inline: auto;
    }

    .waiting-room-page__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
      padding: clamp(1rem, 3vw, 1.75rem);
      border-radius: var(--bsm-radius-lg);
      background: var(--bsm-gradient-brand);
      color: #ffffff;
      box-shadow: var(--bsm-shadow-md);
    }

    .waiting-room-page__eyebrow {
      margin: 0 0 0.25rem;
      font-size: 0.78rem;
      font-weight: 900;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      opacity: 0.88;
    }

    h1,
    p {
      margin: 0;
    }

    h1 {
      font-size: clamp(1.65rem, 4vw, 2.4rem);
    }

    .waiting-room-page__header p:not(.waiting-room-page__eyebrow) {
      max-width: 48rem;
      margin-top: 0.45rem;
      line-height: 1.55;
    }

    .refresh-button {
      flex: 0 0 auto;
      border: 1px solid rgba(255, 255, 255, 0.55);
      border-radius: var(--bsm-radius-pill);
      padding: 0.65rem 0.9rem;
      background: rgba(255, 255, 255, 0.14);
      color: #ffffff;
      font-weight: 800;
      cursor: pointer;
    }

    .refresh-button:disabled {
      opacity: 0.65;
    }

    .waiting-room-page__alert {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      border: 1px solid var(--bsm-color-danger-border);
      border-radius: var(--bsm-radius-sm);
      padding: 0.85rem 1rem;
      background: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger-text);
    }

    .waiting-room-page__alert button {
      border: 0;
      background: transparent;
      color: inherit;
      font-size: 1.25rem;
      cursor: pointer;
    }

    @media (max-width: 720px) {
      .waiting-room-page__header {
        flex-direction: column;
      }

      .refresh-button {
        width: 100%;
      }
    }
  `]
})
export class PatientIntakeLinksPageComponent implements OnInit, OnDestroy {
  readonly facade = inject(PatientIntakeLinksFacade);

  ngOnInit(): void {
    this.facade.initialize();
  }

  ngOnDestroy(): void {
    this.facade.clearHandoff();
  }

  copyHandoff(): void {
    void this.facade.copyHandoff();
  }

  revoke(link: PatientIntakeAccessLinkSummary): void {
    this.facade.revoke(link);
  }
}
