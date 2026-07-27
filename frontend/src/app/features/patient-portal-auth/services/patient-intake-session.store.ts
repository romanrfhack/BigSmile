import { Injectable, signal } from '@angular/core';
import {
  CurrentPatientIntakeSession,
  PatientIntakeAuthenticationResponse
} from '../models/patient-intake-auth.models';

@Injectable({ providedIn: 'root' })
export class PatientIntakeSessionStore {
  private readonly accessTokenState = signal<string | null>(null);
  private readonly expiresAtUtcState = signal<string | null>(null);
  private readonly currentState = signal<CurrentPatientIntakeSession | null>(null);

  readonly current = this.currentState.asReadonly();
  readonly expiresAtUtc = this.expiresAtUtcState.asReadonly();

  setSession(response: PatientIntakeAuthenticationResponse): void {
    if (!response.accessToken?.trim()) {
      throw new Error('A patient intake access token is required.');
    }

    if (!response.current?.intakeId) {
      throw new Error('A patient intake current session is required.');
    }

    this.accessTokenState.set(response.accessToken);
    this.expiresAtUtcState.set(response.expiresAtUtc);
    this.currentState.set(response.current);
  }

  updateCurrent(current: CurrentPatientIntakeSession): void {
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
