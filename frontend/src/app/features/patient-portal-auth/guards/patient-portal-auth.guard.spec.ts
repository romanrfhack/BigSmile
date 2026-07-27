import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  convertToParamMap,
  provideRouter
} from '@angular/router';
import { PatientIntakeSessionStore } from '../services/patient-intake-session.store';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';
import {
  normalizeTenantRealm,
  patientPortalAnonymousGuard,
  patientPortalAuthGuard
} from './patient-portal-auth.guard';

describe('patient portal route guards', () => {
  let router: Router;
  let patientSessionStore: PatientPortalSessionStore;
  let intakeSessionStore: PatientIntakeSessionStore;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    router = TestBed.inject(Router);
    patientSessionStore = TestBed.inject(PatientPortalSessionStore);
    intakeSessionStore = TestBed.inject(PatientIntakeSessionStore);
  });

  it('redirects an unauthenticated patient to the requested tenant login', () => {
    const result = runAuthGuard('Clinic-A');

    expect(router.serializeUrl(result as UrlTree)).toBe('/patient-portal/clinic-a/login');
  });

  it('allows only the realm owned by the current linked patient session', () => {
    setPatientSession('clinic-a');

    expect(runAuthGuard('clinic-a')).toBe(true);
    expect(router.serializeUrl(runAuthGuard('clinic-b') as UrlTree))
      .toBe('/patient-portal/clinic-a/home');
  });

  it('keeps an intake-only session out of linked-patient pages', () => {
    setIntakeSession('clinic-a');

    expect(router.serializeUrl(runAuthGuard('clinic-a') as UrlTree))
      .toBe('/patient-portal/clinic-a/intake');
  });

  it('keeps login anonymous unless exactly one usable patient session already exists', () => {
    const anonymousResult = TestBed.runInInjectionContext(() =>
      patientPortalAnonymousGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    expect(anonymousResult).toBe(true);

    setPatientSession('clinic-a');
    const patientResult = TestBed.runInInjectionContext(() =>
      patientPortalAnonymousGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    expect(router.serializeUrl(patientResult as UrlTree)).toBe('/patient-portal/clinic-a/home');

    patientSessionStore.clear();
    setIntakeSession('clinic-a');
    const intakeResult = TestBed.runInInjectionContext(() =>
      patientPortalAnonymousGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    expect(router.serializeUrl(intakeResult as UrlTree)).toBe('/patient-portal/clinic-a/intake');
  });

  it('fails closed and clears both stores when session state is ambiguous', () => {
    setPatientSession('clinic-a');
    setIntakeSession('clinic-a');

    const result = runAuthGuard('clinic-a');

    expect(router.serializeUrl(result as UrlTree)).toBe('/patient-portal/clinic-a/login');
    expect(patientSessionStore.current()).toBeNull();
    expect(intakeSessionStore.current()).toBeNull();
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

  function setPatientSession(tenantSubdomain: string): void {
    patientSessionStore.setSession({
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

  function setIntakeSession(tenantSubdomain: string): void {
    intakeSessionStore.setSession({
      accessToken: 'intake-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'intake-account-id',
        intakeId: 'intake-id',
        tenantSubdomain,
        loginName: 'new.patient',
        sessionVersion: 1
      }
    });
  }
});
