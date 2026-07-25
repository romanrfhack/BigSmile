import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientPortalShellComponent } from './patient-portal-shell.component';

describe('PatientPortalShellComponent', () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('clears any in-memory staff session when the patient surface opens', async () => {
    const staffAuth = { logout: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [PatientPortalShellComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: staffAuth }
      ]
    }).compileComponents();

    const fixture = TestBed.createComponent(PatientPortalShellComponent);
    fixture.detectChanges();

    expect(staffAuth.logout).toHaveBeenCalledTimes(1);
    expect(fixture.nativeElement.textContent).toContain('Portal del paciente');
    expect(fixture.nativeElement.textContent).not.toContain('Pacientes');
  });
});
