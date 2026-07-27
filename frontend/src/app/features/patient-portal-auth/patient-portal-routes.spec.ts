import { routes } from '../../app.routes';
import {
  patientPortalAnonymousGuard,
  patientPortalAuthGuard
} from './guards/patient-portal-auth.guard';

describe('patient portal route tree', () => {
  it('keeps patient authentication routes under a separate shell', () => {
    const patientPortal = routes.find(route => route.path === 'patient-portal');
    const children = patientPortal?.children ?? [];

    expect(patientPortal?.loadComponent).toBeDefined();
    expect(children.map(route => route.path)).toEqual([
      'activate',
      'intake-activate',
      ':tenantSubdomain/login',
      ':tenantSubdomain/home',
      ':tenantSubdomain'
    ]);
    expect(children.find(route => route.path === 'activate')?.canActivate).toBeUndefined();
    expect(children.find(route => route.path === 'intake-activate')?.canActivate).toBeUndefined();
    expect(children.find(route => route.path === ':tenantSubdomain/login')?.canActivate)
      .toEqual([patientPortalAnonymousGuard]);
    expect(children.find(route => route.path === ':tenantSubdomain/home')?.canActivate)
      .toEqual([patientPortalAuthGuard]);
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
