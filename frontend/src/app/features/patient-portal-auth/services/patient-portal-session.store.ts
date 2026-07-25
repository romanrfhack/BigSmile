import { Injectable, signal } from '@angular/core';
import {
  CurrentPatientPortalSession,
  PatientPortalAuthenticationResponse
} from '../models/patient-portal-auth.models';

@Injectable({ providedIn: 'root' })
export class PatientPortalSessionStore {
  private readonly accessTokenState = signal<string | null>(null);
  private readonly expiresAtUtcState = signal<string | null>(null);
  private readonly currentState = signal<CurrentPatientPortalSession | null>(null);

  readonly current = this.currentState.asReadonly();
  readonly expiresAtUtc = this.expiresAtUtcState.asReadonly();

  setSession(response: PatientPortalAuthenticationResponse): void {
    if (!response.accessToken?.trim()) {
      throw new Error('A patient portal access token is required.');
    }

    if (!response.current) {
      throw new Error('A patient portal current session is required.');
    }

    this.accessTokenState.set(response.accessToken);
    this.expiresAtUtcState.set(response.expiresAtUtc);
    this.currentState.set(response.current);
  }

  updateCurrent(current: CurrentPatientPortalSession): void {
    this.currentState.set(current);
  }

  isAuthenticated(): boolean {
    return this.hasUsableSession();
  }

  getAccessToken(): string | null {
    if (!this.hasUsableSession()) {
      this.clear();
      return null;
    }

    return this.accessTokenState();
  }

  clear(): void {
    this.accessTokenState.set(null);
    this.expiresAtUtcState.set(null);
    this.currentState.set(null);
  }

  private hasUsableSession(): boolean {
    const token = this.accessTokenState();
    const current = this.currentState();
    const expiresAtUtc = this.expiresAtUtcState();

    if (!token || !current || !expiresAtUtc) {
      return false;
    }

    const expiresAt = Date.parse(expiresAtUtc);
    return Number.isFinite(expiresAt) && expiresAt > Date.now();
  }
}
