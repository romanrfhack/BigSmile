import { routes } from '../../app.routes';
import {
  patientIntakeAnonymousGuard,
  patientIntakeWorkspaceGuard
} from './guards/patient-intake-workspace.guard';
import {
  patientPortalAnonymousGuard,
  patientPortalAuthGuard
} from './guards/patient-portal-auth.guard';

describe('patient portal route tree', () => {
  it('keeps linked and intake-only routes under the separate patient shell', () => {
    const patientPortal = routes.find(route => route.path === 'patient-portal');
    const children = patientPortal?.children ?? [];

    expect(patientPortal?.loadComponent).toBeDefined();
    expect(children.map(route => route.path)).toEqual([
      'activate',
      'intake-activate',
      ':tenantSubdomain/intake-login',
      ':tenantSubdomain/intake',
      ':tenantSubdomain/login',
      ':tenantSubdomain/home',
      ':tenantSubdomain'
    ]);
    expect(children.find(route => route.path === 'activate')?.canActivate).toBeUndefined();
    expect(children.find(route => route.path === 'intake-activate')?.canActivate).toBeUndefined();
    expect(children.find(route => route.path === ':tenantSubdomain/intake-login')?.canActivate)
      .toEqual([patientIntakeAnonymousGuard]);
    expect(children.find(route => route.path === ':tenantSubdomain/intake')?.canActivate)
      .toEqual([patientIntakeWorkspaceGuard]);
    expect(children.find(route => route.path === ':tenantSubdomain/login')?.canActivate)
      .toEqual([patientPortalAnonymousGuard]);
    expect(children.find(route => route.path === ':tenantSubdomain/home')?.canActivate)
      .toEqual([patientPortalAuthGuard]);
  });

  it('keeps static and bounded intake routes before the tenant fallback', () => {
    const paths = routes.find(route => route.path === 'patient-portal')?.children
      ?.map(route => route.path ?? '') ?? [];

    expect(paths.indexOf('intake-activate')).toBeLessThan(paths.indexOf(':tenantSubdomain'));
    expect(paths.indexOf(':tenantSubdomain/intake-login')).toBeLessThan(paths.indexOf(':tenantSubdomain'));
    expect(paths.indexOf(':tenantSubdomain/intake')).toBeLessThan(paths.indexOf(':tenantSubdomain'));
  });

  it('does not place any staff operational route inside the patient tree', () => {
    const patientChildren = routes.find(route => route.path === 'patient-portal')?.children ?? [];
    const paths = patientChildren.map(route => route.path ?? '');

    expect(paths.some(path => path.includes('patients/:id'))).toBe(false);
    expect(paths.some(path => path.includes('clinical'))).toBe(false);
    expect(paths.some(path => path.includes('billing'))).toBe(false);
    expect(paths.some(path => path.includes('scheduling'))).toBe(false);
  });
});
