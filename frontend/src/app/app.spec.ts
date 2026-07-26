import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { App } from './app';

@Component({
  standalone: true,
  template: ''
})
class RouteStubComponent {}

describe('App', () => {
  let grantedPermissions: string[];

  beforeEach(async () => {
    window.localStorage.clear();
    grantedPermissions = ['dashboard.read'];

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([
          { path: 'login', component: RouteStubComponent },
          { path: 'patient-portal/activate', component: RouteStubComponent },
          { path: 'patient-portal/intake-activate', component: RouteStubComponent },
          { path: 'patient-portal/:tenantSubdomain/login', component: RouteStubComponent },
          { path: 'patient-intake-links', component: RouteStubComponent },
          { path: 'patients', component: RouteStubComponent }
        ]),
        {
          provide: AuthService,
          useValue: {
            hasPermissions: (permissions?: string[]) =>
              !permissions?.length || permissions.every(permission => grantedPermissions.includes(permission))
          }
        }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    window.localStorage.clear();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Bigsmile');
  });

  it('renders dashboard navigation when dashboard.read is granted', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Panel');
    expect(compiled.textContent).toContain('Idioma');
  });

  it('does not render dashboard navigation when dashboard.read is missing', async () => {
    grantedPermissions = [];

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).not.toContain('Panel');
  });

  it('renders waiting-room navigation with patientportal.intake.manage', async () => {
    grantedPermissions = ['patientportal.intake.manage'];

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Sala de espera');
  });

  it('hides waiting-room navigation without patientportal.intake.manage', async () => {
    grantedPermissions = [];

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Sala de espera');
  });

  it('hides shell navigation and language selector on the login route', async () => {
    const compiled = await renderAppAt('/login');
    expect(compiled.querySelector('.app-header')).toBeNull();
    expect(compiled.textContent).not.toContain('Pacientes');
    expect(compiled.textContent).not.toContain('Agenda');
    expect(compiled.textContent).not.toContain('Contexto de acceso');
    expect(compiled.textContent).not.toContain('Idioma');
  });

  it('hides shell navigation and language selector on the login route with query params', async () => {
    const compiled = await renderAppAt('/login?returnUrl=%2Fpatients');

    expect(compiled.querySelector('.app-header')).toBeNull();
    expect(compiled.textContent).not.toContain('Pacientes');
    expect(compiled.textContent).not.toContain('Agenda');
    expect(compiled.textContent).not.toContain('Contexto de acceso');
    expect(compiled.textContent).not.toContain('Idioma');
  });

  it('hides shell navigation and language selector on the login route with a fragment', async () => {
    const compiled = await renderAppAt('/login#top');

    expect(compiled.querySelector('.app-header')).toBeNull();
    expect(compiled.textContent).not.toContain('Pacientes');
    expect(compiled.textContent).not.toContain('Agenda');
    expect(compiled.textContent).not.toContain('Contexto de acceso');
    expect(compiled.textContent).not.toContain('Idioma');
  });

  it('hides staff shell navigation on patient portal routes', async () => {
    for (const url of [
      '/patient-portal/activate#token=secret',
      '/patient-portal/intake-activate#token=secret'
    ]) {
      const compiled = await renderAppAt(url);

      expect(compiled.querySelector('.app-header')).toBeNull();
      expect(compiled.querySelector('.app-footer')).toBeNull();
      expect(compiled.textContent).not.toContain('Pacientes');
      expect(compiled.textContent).not.toContain('Agenda');
      expect(compiled.textContent).not.toContain('Contexto de acceso');
      expect(compiled.textContent).not.toContain('Sala de espera');
    }
  });

  it('keeps shell navigation and language selector on non-login routes', async () => {
    const compiled = await renderAppAt('/patients');

    expect(compiled.querySelector('.app-header')).not.toBeNull();
    expect(compiled.textContent).toContain('Pacientes');
    expect(compiled.textContent).toContain('Agenda');
    expect(compiled.textContent).toContain('Contexto de acceso');
    expect(compiled.textContent).toContain('Idioma');
  });

  async function renderAppAt(url: string): Promise<HTMLElement> {
    const router = TestBed.inject(Router);
    await router.navigateByUrl(url);

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    fixture.detectChanges();

    return fixture.nativeElement as HTMLElement;
  }
});
