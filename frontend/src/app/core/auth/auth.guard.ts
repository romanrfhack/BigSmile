import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccessScope, AuthService } from './auth.service';

type AuthGuardData = {
  requiredPermissions?: string[];
  requiredScopes?: AccessScope[];
};

export const authGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const data = route.data as AuthGuardData | undefined;

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (!authService.hasPermissions(data?.requiredPermissions) || !authService.hasScopes(data?.requiredScopes)) {
    return router.createUrlTree(['/app']);
  }

  return true;
};

export const anonymousOnlyGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated()
    ? router.createUrlTree(['/app'])
    : true;
};
