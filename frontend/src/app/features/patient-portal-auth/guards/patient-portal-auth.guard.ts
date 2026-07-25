import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';

export const patientPortalAuthGuard: CanActivateFn = route => {
  const sessionStore = inject(PatientPortalSessionStore);
  const router = inject(Router);
  const requestedRealm = normalizeTenantRealm(route.paramMap.get('tenantSubdomain'));
  const current = sessionStore.current();

  if (!sessionStore.isAuthenticated() || !current) {
    return requestedRealm
      ? router.createUrlTree(['/patient-portal', requestedRealm, 'login'])
      : router.createUrlTree(['/patient-portal/activate']);
  }

  const currentRealm = normalizeTenantRealm(current.tenantSubdomain);
  if (requestedRealm && requestedRealm !== currentRealm) {
    return router.createUrlTree(['/patient-portal', currentRealm, 'home']);
  }

  return true;
};

export const patientPortalAnonymousGuard: CanActivateFn = () => {
  const sessionStore = inject(PatientPortalSessionStore);
  const router = inject(Router);
  const current = sessionStore.current();

  if (!sessionStore.isAuthenticated() || !current) {
    return true;
  }

  return router.createUrlTree([
    '/patient-portal',
    normalizeTenantRealm(current.tenantSubdomain),
    'home'
  ]);
};

export function normalizeTenantRealm(value: string | null | undefined): string {
  return (value ?? '').trim().toLowerCase();
}
