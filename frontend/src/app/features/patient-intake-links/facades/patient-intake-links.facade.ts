import { Injectable, signal } from '@angular/core';
import { EMPTY, catchError, finalize } from 'rxjs';
import { AuthService, BranchSummary } from '../../../core/auth/auth.service';
import { PatientIntakeLinksApi } from '../data-access/patient-intake-links.api';
import {
  PatientIntakeAccessLinkSummary,
  WaitingRoomCopyState,
  WaitingRoomHandoff
} from '../models/patient-intake-link.models';
import { PatientIntakeLinkBrowserActions } from '../services/patient-intake-link-browser-actions.service';
import { WaitingRoomHandoffUrlBuilder } from '../services/waiting-room-handoff-url.builder';

@Injectable()
export class PatientIntakeLinksFacade {
  private readonly linksState = signal<PatientIntakeAccessLinkSummary[]>([]);
  private readonly branchesState = signal<BranchSummary[]>([]);
  private readonly loadingState = signal(false);
  private readonly issuingState = signal(false);
  private readonly revokingIdState = signal<string | null>(null);
  private readonly errorState = signal<string | null>(null);
  private readonly handoffState = signal<WaitingRoomHandoff | null>(null);
  private readonly copyStateValue = signal<WaitingRoomCopyState>('idle');

  readonly links = this.linksState.asReadonly();
  readonly branches = this.branchesState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly issuing = this.issuingState.asReadonly();
  readonly revokingId = this.revokingIdState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly handoff = this.handoffState.asReadonly();
  readonly copyState = this.copyStateValue.asReadonly();

  constructor(
    private readonly api: PatientIntakeLinksApi,
    private readonly authService: AuthService,
    private readonly handoffUrlBuilder: WaitingRoomHandoffUrlBuilder,
    private readonly browserActions: PatientIntakeLinkBrowserActions
  ) {}

  initialize(): void {
    this.branchesState.set([...(this.authService.getCurrent()?.branches ?? [])]);
    this.loadLinks();
  }

  loadLinks(): void {
    if (this.loadingState()) {
      return;
    }

    this.loadingState.set(true);
    this.errorState.set(null);

    this.api.list(true, 50).pipe(
      catchError(() => {
        this.errorState.set('No se pudieron cargar los enlaces de sala de espera.');
        return EMPTY;
      }),
      finalize(() => this.loadingState.set(false))
    ).subscribe(links => this.linksState.set(links));
  }

  issue(branchId: string | null): void {
    if (this.issuingState()) {
      return;
    }

    this.issuingState.set(true);
    this.errorState.set(null);
    this.clearHandoff();

    this.api.issue(branchId).pipe(
      catchError(() => {
        this.errorState.set('No se pudo generar el enlace de sala de espera. Intenta nuevamente.');
        return EMPTY;
      }),
      finalize(() => this.issuingState.set(false))
    ).subscribe(issued => {
      const current = this.authService.getCurrent();
      const branchName = issued.branchId
        ? current?.branches.find(branch => branch.id === issued.branchId)?.name ?? null
        : null;
      const handoffUrl = this.handoffUrlBuilder.build(issued.accessToken);

      this.handoffState.set({
        clinicName: current?.tenant?.name ?? 'BigSmile',
        branchName,
        url: handoffUrl,
        createdAtUtc: issued.createdAtUtc,
        expiresAtUtc: issued.expiresAtUtc
      });
      this.copyStateValue.set('idle');
      this.linksState.update(links => [
        {
          id: issued.id,
          branchId: issued.branchId,
          purpose: issued.purpose,
          status: 'Active',
          createdAtUtc: issued.createdAtUtc,
          expiresAtUtc: issued.expiresAtUtc,
          revokedAtUtc: null,
          consumedAtUtc: null
        },
        ...links.filter(link => link.id !== issued.id)
      ]);
    });
  }

  async copyHandoff(): Promise<void> {
    const handoff = this.handoffState();
    if (!handoff || this.copyStateValue() === 'copying') {
      return;
    }

    this.copyStateValue.set('copying');
    try {
      await this.browserActions.copyText(handoff.url);
      this.copyStateValue.set('copied');
    } catch {
      this.copyStateValue.set('error');
    }
  }

  printHandoff(): void {
    if (!this.handoffState()) {
      return;
    }

    try {
      this.browserActions.printCurrentHandoff();
    } catch {
      this.errorState.set('No se pudo abrir la vista de impresión en este navegador.');
    }
  }

  revoke(link: PatientIntakeAccessLinkSummary): void {
    if (this.revokingIdState() || link.status.toLowerCase() !== 'active') {
      return;
    }

    if (!this.browserActions.confirmRevoke()) {
      return;
    }

    this.revokingIdState.set(link.id);
    this.errorState.set(null);

    this.api.revoke(link.id).pipe(
      catchError(() => {
        this.errorState.set('No se pudo revocar el enlace. Actualiza el listado e intenta nuevamente.');
        return EMPTY;
      }),
      finalize(() => this.revokingIdState.set(null))
    ).subscribe(() => {
      this.linksState.update(links => links.map(current =>
        current.id === link.id
          ? { ...current, status: 'Revoked' }
          : current
      ));
    });
  }

  clearHandoff(): void {
    this.handoffState.set(null);
    this.copyStateValue.set('idle');
  }

  clearError(): void {
    this.errorState.set(null);
  }
}
