import { Injectable } from '@angular/core';
import { PatientIntakeAuthenticationResponse } from '../models/patient-intake-auth.models';
import { PatientPortalAuthenticationResponse } from '../models/patient-portal-auth.models';
import { PatientIntakeSessionStore } from './patient-intake-session.store';
import { PatientPortalSessionStore } from './patient-portal-session.store';

export type PatientPortalSessionMode = 'patient' | 'patient_intake';

export interface ActivePatientPortalSession {
  mode: PatientPortalSessionMode;
  accessToken: string;
  tenantSubdomain: string;
}

export type PatientPortalSessionResolution =
  | { state: 'active'; session: ActivePatientPortalSession }
  | { state: 'none' }
  | { state: 'ambiguous' };

@Injectable({ providedIn: 'root' })
export class PatientPortalSessionBoundary {
  constructor(
    private readonly patientSessionStore: PatientPortalSessionStore,
    private readonly intakeSessionStore: PatientIntakeSessionStore
  ) {}

  setPatientSession(response: PatientPortalAuthenticationResponse): void {
    this.intakeSessionStore.clear();
    this.patientSessionStore.setSession(response);
  }

  setIntakeSession(response: PatientIntakeAuthenticationResponse): void {
    this.patientSessionStore.clear();
    this.intakeSessionStore.setSession(response);
  }

  resolve(): PatientPortalSessionResolution {
    const patientToken = this.patientSessionStore.getAccessToken();
    const intakeToken = this.intakeSessionStore.getAccessToken();

    if (patientToken && intakeToken) {
      this.clearAll();
      return { state: 'ambiguous' };
    }

    if (patientToken) {
      const current = this.patientSessionStore.current();
      if (!current) {
        this.patientSessionStore.clear();
        return { state: 'none' };
      }

      return {
        state: 'active',
        session: {
          mode: 'patient',
          accessToken: patientToken,
          tenantSubdomain: normalizeRealm(current.tenantSubdomain)
        }
      };
    }

    if (intakeToken) {
      const current = this.intakeSessionStore.current();
      if (!current) {
        this.intakeSessionStore.clear();
        return { state: 'none' };
      }

      return {
        state: 'active',
        session: {
          mode: 'patient_intake',
          accessToken: intakeToken,
          tenantSubdomain: normalizeRealm(current.tenantSubdomain)
        }
      };
    }

    return { state: 'none' };
  }

  clearPatientSession(): void {
    this.patientSessionStore.clear();
  }

  clearIntakeSession(): void {
    this.intakeSessionStore.clear();
  }

  clearMode(mode: PatientPortalSessionMode): void {
    if (mode === 'patient') {
      this.clearPatientSession();
      return;
    }

    this.clearIntakeSession();
  }

  clearAll(): void {
    this.patientSessionStore.clear();
    this.intakeSessionStore.clear();
  }
}

function normalizeRealm(value: string | null | undefined): string {
  return (value ?? '').trim().toLowerCase();
}
