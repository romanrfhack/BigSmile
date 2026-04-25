import { routes } from './app.routes';
import { anonymousOnlyGuard, authGuard } from './core/auth/auth.guard';

describe('app routes', () => {
  const protectedRoutes = [
    { path: 'app', requiredPermissions: ['auth.self.read'] },
    { path: 'dashboard', requiredPermissions: ['dashboard.read'] },
    { path: 'patients', requiredPermissions: ['patient.read'] },
    { path: 'patients/new', requiredPermissions: ['patient.write'] },
    { path: 'patients/:id/edit', requiredPermissions: ['patient.write'] },
    { path: 'patients/:id', requiredPermissions: ['patient.read'] },
    { path: 'patients/:id/clinical-record', requiredPermissions: ['clinical.read'] },
    { path: 'patients/:id/odontogram', requiredPermissions: ['odontogram.read'] },
    { path: 'patients/:id/documents', requiredPermissions: ['document.read'] },
    { path: 'patients/:id/treatment-plan/quote/billing', requiredPermissions: ['billing.read'] },
    { path: 'patients/:id/treatment-plan/quote', requiredPermissions: ['treatmentquote.read'] },
    { path: 'patients/:id/treatment-plan', requiredPermissions: ['treatmentplan.read'] },
    { path: 'scheduling', requiredPermissions: ['scheduling.read'] },
  ];

  function findRoute(path: string) {
    const route = routes.find((entry) => entry.path === path);

    expect(route).toBeDefined();
    return route!;
  }

  it('keeps the login route anonymous-only and lazy-loaded', () => {
    const route = findRoute('login');

    expect(route.canActivate).toContain(anonymousOnlyGuard);
    expect(route.loadComponent).toEqual(expect.any(Function));
    expect(route.component).toBeUndefined();
  });

  it('keeps protected feature routes guarded, permissioned, and lazy-loaded', () => {
    for (const expectedRoute of protectedRoutes) {
      const route = findRoute(expectedRoute.path);

      expect(route.canActivate).toContain(authGuard);
      expect(route.data?.['requiredPermissions']).toEqual(expectedRoute.requiredPermissions);
      expect(route.loadComponent).toEqual(expect.any(Function));
      expect(route.component).toBeUndefined();
    }
  });
});
