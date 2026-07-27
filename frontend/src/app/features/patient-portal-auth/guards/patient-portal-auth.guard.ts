import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PatientPortalSessionBoundary } from '../services/patient-portal-session-boundary.service';

export const patientPortalAuthGuard: CanActivateFn = route => {
  const sessionBoundary = inject(PatientPortalSessionBoundary);
  const router = inject(Router);
  const requestedRealm = normalizeTenantRealm(route.paramMap.get('tenantSubdomain'));
  const resolution = sessionBoundary.resolve();

  if (resolution.state !== 'active') {
    return requestedRealm
      ? router.createUrlTree(['/patient-portal', requestedRealm, 'login'])
      : router.createUrlTree(['/patient-portal/activate']);
  }

  const currentRealm = resolution.session.tenantSubdomain;
  if (resolution.session.mode === 'patient_intake') {
    return router.createUrlTree(['/patient-portal', currentRealm, 'intake']);
  }

  if (requestedRealm && requestedRealm !== currentRealm) {
    return router.createUrlTree(['/patient-portal', currentRealm, 'home']);
  }

  return true;
};

export const patientPortalAnonymousGuard: CanActivateFn = () => {
  const sessionBoundary = inject(PatientPortalSessionBoundary);
  const router = inject(Router);
  const resolution = sessionBoundary.resolve();

  if (resolution.state !== 'active') {
    return true;
  }

  return resolution.session.mode === 'patient'
    ? router.createUrlTree([
      '/patient-portal',
      resolution.session.tenantSubdomain,
      'home'
    ])
    : router.createUrlTree([
      '/patient-portal',
      resolution.session.tenantSubdomain,
      'intake'
    ]);
};

export function normalizeTenantRealm(value: string | null | undefined): string {
  return (value ?? '').trim().toLowerCase();
}
