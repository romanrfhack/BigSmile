import { Component, Input, OnChanges, OnDestroy, SimpleChanges, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { I18nService } from '../../../core/i18n';
import { TranslatePipe } from '../../../shared/i18n';
import { SectionCardComponent, StatusBadgeComponent } from '../../../shared/ui';
import type { StatusBadgeTone } from '../../../shared/ui';
import { SchedulingFacade } from '../facades/scheduling.facade';
import {
  AppointmentPatientIntakeRequestStatus,
  AppointmentPatientIntakeStatus,
  PatientPortalAccessStatus,
  PreparedAppointmentPatientIntakeRequest
} from '../models/scheduling.models';

@Component({
  selector: 'app-appointment-patient-intake-request',
  standalone: true,
  imports: [TranslatePipe, SectionCardComponent, StatusBadgeComponent],
  template: `
    <app-section-card
      variant="accent"
      [title]="'Medical history request' | t"
      [subtitle]="'Prepare optional patient access from this appointment.' | t">

      @if (loading()) {
        <p class="request-state" role="status">{{ 'Checking patient access and medical history...' | t }}</p>
      } @else if (error()) {
        <div class="request-alert request-alert--error" role="alert">
          <span>{{ error() | t }}</span>
          <button type="button" class="btn btn-secondary" (click)="loadStatus()">
            {{ 'Retry' | t }}
          </button>
        </div>
      } @else if (status(); as current) {
        <div class="request-indicators">
          <article>
            <span>{{ 'Patient access' | t }}</span>
            <app-status-badge
              [tone]="portalTone(current.portalAccessStatus)"
              [label]="portalLabel(current.portalAccessStatus) | t">
            </app-status-badge>
          </article>
          <article>
            <span>{{ 'Medical history' | t }}</span>
            <app-status-badge
              [tone]="intakeTone(current.intakeStatus)"
              [label]="intakeLabel(current.intakeStatus) | t">
            </app-status-badge>
          </article>
        </div>

        @if (current.intakeStatus === 'Completed') {
          <div class="request-alert request-alert--success" role="status">
            {{ 'This patient already completed the medical history. Do not send another request.' | t }}
          </div>
        } @else if (current.portalAccessStatus === 'RecoveryRequired') {
          <div class="request-alert request-alert--warning" role="status">
            {{ 'The patient account needs assisted recovery before a new access link can be sent.' | t }}
          </div>
        } @else {
          <p class="request-help">
            {{ (current.intakeStatus === 'InProgress'
              ? 'The patient started the form but has not submitted it. You may remind them.'
              : 'The patient has not submitted a medical history yet. Sending access is optional.') | t }}
          </p>

          <button
            type="button"
            class="btn btn-primary"
            [disabled]="preparing() || !current.canRequest"
            (click)="prepareRequest()">
            {{ (preparing() ? 'Preparing secure access...' : 'Prepare secure access') | t }}
          </button>
        }

        @if (preparedUrl()) {
          <div class="prepared-access" role="status" aria-live="polite">
            <strong>{{ 'Secure access is ready.' | t }}</strong>
            <p>{{ 'Review the message in WhatsApp before sending it. BigSmile does not send it automatically.' | t }}</p>

            <label>
              <span>{{ 'Patient access link' | t }}</span>
              <input type="text" readonly [value]="preparedUrl()" />
            </label>

            <div class="prepared-actions">
              <button type="button" class="btn btn-secondary" (click)="copyLink()">
                {{ (copyState() === 'copied' ? 'Link copied' : 'Copy link') | t }}
              </button>
              <button
                type="button"
                class="btn btn-success"
                [disabled]="!whatsAppPhone()"
                (click)="openWhatsApp()">
                {{ 'Open WhatsApp' | t }}
              </button>
            </div>

            @if (!whatsAppPhone()) {
              <small>{{ 'No WhatsApp-compatible phone is available. Copy the link and deliver it through an approved channel.' | t }}</small>
            }
          </div>
        }
      }
    </app-section-card>
  `,
  styles: [`
    .request-state,
    .request-help,
    .prepared-access p {
      margin: 0;
      color: var(--bsm-color-text-muted);
    }

    .request-indicators {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 0.75rem;
    }

    .request-indicators article {
      display: grid;
      gap: 0.5rem;
      align-content: start;
      padding: 0.8rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
    }

    .request-indicators article > span,
    .prepared-access label > span {
      color: var(--bsm-color-text-muted);
      font-size: 0.78rem;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.03em;
    }

    .request-alert,
    .prepared-access {
      display: grid;
      gap: 0.75rem;
      padding: 0.85rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
    }

    .request-alert--success {
      border-color: var(--bsm-color-success-soft);
      color: var(--bsm-color-success);
    }

    .request-alert--warning {
      border-color: var(--bsm-color-warning-soft);
      color: var(--bsm-color-warning);
    }

    .request-alert--error {
      border-color: var(--bsm-color-danger-soft);
      color: var(--bsm-color-danger);
    }

    .prepared-access label {
      display: grid;
      gap: 0.4rem;
    }

    .prepared-access input {
      width: 100%;
      box-sizing: border-box;
      padding: 0.65rem 0.75rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text);
    }

    .prepared-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.65rem;
    }

    @media (max-width: 640px) {
      .request-indicators {
        grid-template-columns: 1fr;
      }

      .prepared-actions .btn {
        width: 100%;
      }
    }
  `]
})
export class AppointmentPatientIntakeRequestComponent implements OnChanges, OnDestroy {
  private readonly schedulingFacade = inject(SchedulingFacade);
  private readonly i18n = inject(I18nService);

  @Input({ required: true }) appointmentId = '';

  readonly status = signal<AppointmentPatientIntakeRequestStatus | null>(null);
  readonly loading = signal(false);
  readonly preparing = signal(false);
  readonly error = signal<string | null>(null);
  readonly preparedUrl = signal<string | null>(null);
  readonly whatsAppPhone = signal<string | null>(null);
  readonly copyState = signal<'idle' | 'copied'>('idle');

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['appointmentId']) {
      this.clearPreparedAccess();
      this.loadStatus();
    }
  }

  ngOnDestroy(): void {
    this.clearPreparedAccess();
  }

  loadStatus(): void {
    if (!this.appointmentId) {
      this.status.set(null);
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.schedulingFacade.getPatientIntakeRequestStatus(this.appointmentId).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: status => this.status.set(status),
      error: () => {
        this.status.set(null);
        this.error.set('Patient access and medical-history status could not be loaded.');
      }
    });
  }

  prepareRequest(): void {
    const current = this.status();
    if (!current?.canRequest || this.preparing()) {
      return;
    }

    this.preparing.set(true);
    this.error.set(null);
    this.clearPreparedAccess();
    this.schedulingFacade.preparePatientIntakeRequest(this.appointmentId).pipe(
      finalize(() => this.preparing.set(false))
    ).subscribe({
      next: prepared => this.handlePreparedRequest(prepared),
      error: () => {
        this.error.set('Secure patient access could not be prepared. Refresh the status and try again.');
        this.loadStatus();
      }
    });
  }

  async copyLink(): Promise<void> {
    const url = this.preparedUrl();
    if (!url) {
      return;
    }

    try {
      await navigator.clipboard.writeText(url);
      this.copyState.set('copied');
    } catch {
      this.copyState.set('idle');
      this.error.set('The link could not be copied automatically. Select and copy it manually.');
    }
  }

  openWhatsApp(): void {
    const current = this.status();
    const phone = this.whatsAppPhone();
    const url = this.preparedUrl();
    if (!current || !phone || !url) {
      return;
    }

    const message = this.i18n.translate(
      'Hello {patientName}, please complete your medical history before your appointment using this secure link: {accessUrl}',
      { patientName: current.patientFullName, accessUrl: url });
    const whatsAppUrl = `https://wa.me/${phone}?text=${encodeURIComponent(message)}`;
    window.open(whatsAppUrl, '_blank', 'noopener,noreferrer');
  }

  portalTone(status: PatientPortalAccessStatus): StatusBadgeTone {
    return status === 'Active' ? 'success' : status === 'RecoveryRequired' ? 'warning' : 'neutral';
  }

  portalLabel(status: PatientPortalAccessStatus): string {
    return status === 'Active'
      ? 'Active access'
      : status === 'RecoveryRequired'
        ? 'Recovery required'
        : 'No access yet';
  }

  intakeTone(status: AppointmentPatientIntakeStatus): StatusBadgeTone {
    return status === 'Completed' ? 'success' : status === 'InProgress' ? 'info' : 'warning';
  }

  intakeLabel(status: AppointmentPatientIntakeStatus): string {
    return status === 'Completed'
      ? 'Medical history completed'
      : status === 'InProgress'
        ? 'In progress'
        : 'Not started';
  }

  private handlePreparedRequest(prepared: PreparedAppointmentPatientIntakeRequest): void {
    this.status.set(prepared.status);
    const url = buildPatientAccessUrl(prepared, window.location.origin);
    this.preparedUrl.set(url);
    this.whatsAppPhone.set(normalizeWhatsAppPhone(prepared.status.patientPrimaryPhone));
    this.copyState.set('idle');
  }

  private clearPreparedAccess(): void {
    this.preparedUrl.set(null);
    this.whatsAppPhone.set(null);
    this.copyState.set('idle');
  }
}

export function buildPatientAccessUrl(
  prepared: PreparedAppointmentPatientIntakeRequest,
  origin: string
): string {
  if (prepared.accessMode === 'Activation' && !prepared.activationToken) {
    throw new Error('Activation access requires a one-time token.');
  }

  const path = prepared.accessMode === 'Activation'
    ? `/patient-portal/activate#token=${encodeURIComponent(prepared.activationToken!)}`
    : `/patient-portal/${encodeURIComponent(prepared.status.patientPortalRealm)}/login`;
  return new URL(path, origin).toString();
}

export function normalizeWhatsAppPhone(value: string | null | undefined): string | null {
  const raw = (value ?? '').trim();
  const digits = raw.replace(/\D/g, '');
  if (!digits) {
    return null;
  }

  const normalized = raw.startsWith('+')
    ? digits
    : digits.length === 10
      ? `52${digits}`
      : digits;
  return normalized.length >= 11 && normalized.length <= 15 ? normalized : null;
}
