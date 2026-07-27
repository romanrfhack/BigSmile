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
  patientIntakeAnonymousGuard,
  patientIntakeWorkspaceGuard
} from './patient-intake-workspace.guard';

describe('patient intake workspace guards', () => {
  let router: Router;
  let patientSessionStore: PatientPortalSessionStore;
  let intakeSessionStore: PatientIntakeSessionStore;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    router = TestBed.inject(Router);
    patientSessionStore = TestBed.inject(PatientPortalSessionStore);
    intakeSessionStore = TestBed.inject(PatientIntakeSessionStore);
  });

  it('redirects a missing session to the tenant intake login', () => {
    const result = runWorkspaceGuard('Clinic-A');

    expect(router.serializeUrl(result as UrlTree))
      .toBe('/patient-portal/clinic-a/intake-login');
  });

  it('allows a linked patient session for its own realm', () => {
    setPatientSession('clinic-a');

    expect(runWorkspaceGuard('clinic-a')).toBe(true);
  });

  it('allows an intake-only session for its exact realm', () => {
    setIntakeSession('clinic-a');

    expect(runWorkspaceGuard('clinic-a')).toBe(true);
  });

  it('redirects realm mismatches to the realm carried by the active session', () => {
    setIntakeSession('clinic-a');

    expect(router.serializeUrl(runWorkspaceGuard('clinic-b') as UrlTree))
      .toBe('/patient-portal/clinic-a/intake');
  });

  it('redirects an already authenticated session away from intake login', () => {
    setPatientSession('clinic-a');

    const result = TestBed.runInInjectionContext(() =>
      patientIntakeAnonymousGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));

    expect(router.serializeUrl(result as UrlTree)).toBe('/patient-portal/clinic-a/intake');
  });

  it('fails closed when both session modes are present', () => {
    setPatientSession('clinic-a');
    setIntakeSession('clinic-a');

    const result = runWorkspaceGuard('clinic-a');

    expect(router.serializeUrl(result as UrlTree))
      .toBe('/patient-portal/clinic-a/intake-login');
    expect(patientSessionStore.current()).toBeNull();
    expect(intakeSessionStore.current()).toBeNull();
  });

  function runWorkspaceGuard(tenantSubdomain: string): ReturnType<typeof patientIntakeWorkspaceGuard> {
    const route = {
      paramMap: convertToParamMap({ tenantSubdomain })
    } as ActivatedRouteSnapshot;

    return TestBed.runInInjectionContext(() =>
      patientIntakeWorkspaceGuard(route, {} as RouterStateSnapshot));
  }

  function setPatientSession(tenantSubdomain: string): void {
    patientSessionStore.setSession({
      accessToken: 'patient-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'patient-account-id',
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
