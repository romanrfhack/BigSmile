import { Location } from '@angular/common';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { PatientIntakeAuthFacade } from '../facades/patient-intake-auth.facade';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';
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
    const clearLinkedSession = vi.fn();
    const facade = {
      current: signal(null),
      loading: signal(false),
      error: signal<string | null>(null),
      activate,
      logout: vi.fn().mockReturnValue(of(void 0)),
      clearSession: vi.fn(),
      clearError: vi.fn()
    };

    TestBed.configureTestingModule({
      imports: [PatientIntakeActivationPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { fragment: 'token=one-time-token' } } },
        { provide: Router, useValue: { url: '/patient-portal/intake-activate#token=one-time-token' } },
        { provide: Location, useValue: { replaceState } },
        { provide: PatientIntakeAuthFacade, useValue: facade },
        { provide: PatientPortalSessionStore, useValue: { clear: clearLinkedSession } }
      ]
    });

    const fixture = TestBed.createComponent(PatientIntakeActivationPageComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    expect(component.hasActivationToken()).toBe(true);
    expect(replaceState).toHaveBeenCalledWith('/patient-portal/intake-activate');
    expect(clearLinkedSession).toHaveBeenCalledTimes(1);
    expect(facade.clearSession).toHaveBeenCalledTimes(1);

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

  it('does not call activation when the fragment is missing', () => {
    const activate = vi.fn().mockReturnValue(of(activationResponse));
    const facade = {
      current: signal(null),
      loading: signal(false),
      error: signal<string | null>(null),
      activate,
      logout: vi.fn().mockReturnValue(of(void 0)),
      clearSession: vi.fn(),
      clearError: vi.fn()
    };

    TestBed.configureTestingModule({
      imports: [PatientIntakeActivationPageComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { fragment: null } } },
        { provide: Router, useValue: { url: '/patient-portal/intake-activate' } },
        { provide: Location, useValue: { replaceState: vi.fn() } },
        { provide: PatientIntakeAuthFacade, useValue: facade },
        { provide: PatientPortalSessionStore, useValue: { clear: vi.fn() } }
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
});
