import { Location } from '@angular/common';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { PatientIntakeAuthFacade } from '../facades/patient-intake-auth.facade';
import { PatientPortalSessionBoundary } from '../services/patient-portal-session-boundary.service';
import { PatientIntakeActivationPageComponent } from './patient-intake-activation.page';

describe('PatientIntakeActivationPageComponent', () => {
  const activationResponse = {
    accessToken: 'intake-access-token',
    expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    current: {
      accountId: 'account-id',
      intakeId: 'intake-id',
      tenantSubdomain: 'clinic-a',
      loginName: 'new.patient',
      sessionVersion: 1
    }
  };

  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('reads the one-time fragment once, removes it immediately, and sends it only in the body command', () => {
    const activate = vi.fn().mockReturnValue(of(activationResponse));
    const replaceState = vi.fn();
    const clearAll = vi.fn();
    const facade = createFacade(activate);

    TestBed.configureTestingModule({
      imports: [PatientIntakeActivationPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { fragment: 'token=one-time-token' } } },
        { provide: Router, useValue: { url: '/patient-portal/intake-activate#token=one-time-token', navigate: vi.fn() } },
        { provide: Location, useValue: { replaceState } },
        { provide: PatientIntakeAuthFacade, useValue: facade },
        { provide: PatientPortalSessionBoundary, useValue: { clearAll } }
      ]
    });

    const fixture = TestBed.createComponent(PatientIntakeActivationPageComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect(component.hasActivationToken()).toBe(true);
    expect(replaceState).toHaveBeenCalledWith('/patient-portal/intake-activate');
    expect(clearAll).toHaveBeenCalledTimes(1);

    component.form.setValue({
      loginName: 'new.patient',
      password: 'twelve-character-password',
      confirmPassword: 'twelve-character-password'
    });
    component.submit();

    expect(activate).toHaveBeenCalledWith({
      accessToken: 'one-time-token',
      loginName: 'new.patient',
      password: 'twelve-character-password'
    });
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });

  it('navigates an activated intake-only session to the shared workspace', () => {
    const navigate = vi.fn().mockResolvedValue(true);
    const facade = createFacade(vi.fn().mockReturnValue(of(activationResponse)));

    TestBed.configureTestingModule({
      imports: [PatientIntakeActivationPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { fragment: null } } },
        { provide: Router, useValue: { url: '/patient-portal/intake-activate', navigate } },
        { provide: Location, useValue: { replaceState: vi.fn() } },
        { provide: PatientIntakeAuthFacade, useValue: facade },
        { provide: PatientPortalSessionBoundary, useValue: { clearAll: vi.fn() } }
      ]
    });

    const component = TestBed.createComponent(PatientIntakeActivationPageComponent).componentInstance;
    component.continueToIntake('clinic-a');

    expect(navigate).toHaveBeenCalledWith(
      ['/patient-portal', 'clinic-a', 'intake'],
      { replaceUrl: true }
    );
  });

  it('does not call activation when the fragment is missing', () => {
    const activate = vi.fn().mockReturnValue(of(activationResponse));
    const facade = createFacade(activate);

    TestBed.configureTestingModule({
      imports: [PatientIntakeActivationPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { fragment: null } } },
        { provide: Router, useValue: { url: '/patient-portal/intake-activate', navigate: vi.fn() } },
        { provide: Location, useValue: { replaceState: vi.fn() } },
        { provide: PatientIntakeAuthFacade, useValue: facade },
        { provide: PatientPortalSessionBoundary, useValue: { clearAll: vi.fn() } }
      ]
    });

    const fixture = TestBed.createComponent(PatientIntakeActivationPageComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.form.setValue({
      loginName: 'new.patient',
      password: 'twelve-character-password',
      confirmPassword: 'twelve-character-password'
    });
    component.submit();

    expect(component.hasActivationToken()).toBe(false);
    expect(activate).not.toHaveBeenCalled();
  });

  function createFacade(activate: ReturnType<typeof vi.fn>) {
    return {
      current: signal(null),
      loading: signal(false),
      error: signal<string | null>(null),
      activate,
      logout: vi.fn().mockReturnValue(of(void 0)),
      clearSession: vi.fn(),
      clearError: vi.fn()
    };
  }
});
