import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  convertToParamMap,
  provideRouter
} from '@angular/router';
import {
  normalizeTenantRealm,
  patientPortalAnonymousGuard,
  patientPortalAuthGuard
} from './patient-portal-auth.guard';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';

describe('patient portal route guards', () => {
  let router: Router;
  let sessionStore: PatientPortalSessionStore;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    router = TestBed.inject(Router);
    sessionStore = TestBed.inject(PatientPortalSessionStore);
  });

  it('redirects an unauthenticated patient to the requested tenant login', () => {
    const result = runAuthGuard('Clinic-A');

    expect(router.serializeUrl(result as UrlTree)).toBe('/patient-portal/clinic-a/login');
  });

  it('allows only the realm owned by the current patient session', () => {
    setAuthenticatedSession('clinic-a');

    expect(runAuthGuard('clinic-a')).toBe(true);
    expect(router.serializeUrl(runAuthGuard('clinic-b') as UrlTree))
      .toBe('/patient-portal/clinic-a/home');
  });

  it('keeps login anonymous unless a usable patient session already exists', () => {
    const anonymousResult = TestBed.runInInjectionContext(() =>
      patientPortalAnonymousGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    expect(anonymousResult).toBe(true);

    setAuthenticatedSession('clinic-a');
    const authenticatedResult = TestBed.runInInjectionContext(() =>
      patientPortalAnonymousGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    expect(router.serializeUrl(authenticatedResult as UrlTree)).toBe('/patient-portal/clinic-a/home');
  });

  it('normalizes the public tenant realm consistently', () => {
    expect(normalizeTenantRealm('  Clinic-A  ')).toBe('clinic-a');
    expect(normalizeTenantRealm(null)).toBe('');
  });

  function runAuthGuard(tenantSubdomain: string): ReturnType<typeof patientPortalAuthGuard> {
    const route = {
      paramMap: convertToParamMap({ tenantSubdomain })
    } as ActivatedRouteSnapshot;

    return TestBed.runInInjectionContext(() =>
      patientPortalAuthGuard(route, {} as RouterStateSnapshot));
  }

  function setAuthenticatedSession(tenantSubdomain: string): void {
    sessionStore.setSession({
      accessToken: 'patient-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'account-id',
        patientId: 'patient-id',
        tenantSubdomain,
        loginName: 'patient.login',
        sessionVersion: 1
      }
    });
  }
});
