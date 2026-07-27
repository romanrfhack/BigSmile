import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { PatientIntakeAuthFacade } from '../facades/patient-intake-auth.facade';
import { PatientIntakeLoginPageComponent } from './patient-intake-login.page';

describe('PatientIntakeLoginPageComponent', () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('logs in within the tenant realm and routes to the shared intake workspace', () => {
    const response = {
      accessToken: 'intake-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'account-id',
        intakeId: 'intake-id',
        tenantSubdomain: 'clinic-a',
        loginName: 'new.patient',
        sessionVersion: 1
      }
    };
    const login = vi.fn().mockReturnValue(of(response));
    const navigate = vi.fn().mockResolvedValue(true);
    const facade = {
      loading: signal(false),
      error: signal<string | null>(null),
      login,
      clearError: vi.fn()
    };

    TestBed.configureTestingModule({
      imports: [PatientIntakeLoginPageComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ tenantSubdomain: 'Clinic-A' }) } }
        },
        { provide: Router, useValue: { navigate } },
        { provide: PatientIntakeAuthFacade, useValue: facade }
      ]
    });

    const fixture = TestBed.createComponent(PatientIntakeLoginPageComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.form.setValue({
      loginName: 'new.patient',
      password: 'twelve-character-password'
    });

    component.submit();

    expect(login).toHaveBeenCalledWith('clinic-a', {
      loginName: 'new.patient',
      password: 'twelve-character-password'
    });
    expect(navigate).toHaveBeenCalledWith(
      ['/patient-portal', 'clinic-a', 'intake'],
      { replaceUrl: true }
    );
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });
});
