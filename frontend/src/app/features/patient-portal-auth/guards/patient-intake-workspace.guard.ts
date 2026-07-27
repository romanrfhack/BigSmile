import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { normalizeTenantRealm } from './patient-portal-auth.guard';
import { PatientPortalSessionBoundary } from '../services/patient-portal-session-boundary.service';

export const patientIntakeWorkspaceGuard: CanActivateFn = route => {
  const sessionBoundary = inject(PatientPortalSessionBoundary);
  const router = inject(Router);
  const requestedRealm = normalizeTenantRealm(route.paramMap.get('tenantSubdomain'));
  const resolution = sessionBoundary.resolve();

  if (resolution.state !== 'active') {
    return requestedRealm
      ? router.createUrlTree(['/patient-portal', requestedRealm, 'intake-login'])
      : router.createUrlTree(['/patient-portal/intake-activate']);
  }

  const currentRealm = resolution.session.tenantSubdomain;
  if (!requestedRealm || requestedRealm !== currentRealm) {
    return router.createUrlTree(['/patient-portal', currentRealm, 'intake']);
  }

  return true;
};

export const patientIntakeAnonymousGuard: CanActivateFn = () => {
  const sessionBoundary = inject(PatientPortalSessionBoundary);
  const router = inject(Router);
  const resolution = sessionBoundary.resolve();

  if (resolution.state !== 'active') {
    return true;
  }

  return router.createUrlTree([
    '/patient-portal',
    resolution.session.tenantSubdomain,
    'intake'
  ]);
};
