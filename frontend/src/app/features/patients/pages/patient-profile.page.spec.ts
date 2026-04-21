import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../facades/patients.facade';
import { PatientProfilePageComponent } from './patient-profile.page';

describe('PatientProfilePageComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientProfilePageComponent],
      providers: [
        {
          provide: PatientsFacade,
          useValue: {
            currentPatient: signal({
              id: 'patient-1',
              fullName: 'Ana Lopez',
              dateOfBirth: '1991-02-14',
              primaryPhone: null,
              email: null,
              isActive: true,
              hasClinicalAlerts: false,
              clinicalAlertsSummary: null,
              responsibleParty: null,
              createdAt: '2026-04-10T10:00:00Z',
              updatedAt: null
            }),
            loadingPatient: signal(false),
            detailError: signal<string | null>(null),
            clearCurrentPatient: () => undefined,
            loadPatient: () => undefined
          }
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => 'patient-1'
              }
            }
          }
        },
        {
          provide: AuthService,
          useValue: {
            hasPermissions: () => true
          }
        }
      ]
    }).compileComponents();
  });

  it('renders navigation to the clinical record from patient profile', () => {
    const fixture = TestBed.createComponent(PatientProfilePageComponent);
    fixture.detectChanges();

    const links = Array.from(fixture.nativeElement.querySelectorAll('a') as NodeListOf<HTMLAnchorElement>);
    const clinicalRecordLink = links
      .find((link) => link.textContent?.includes('Clinical record'));
    const odontogramLink = links
      .find((link) => link.textContent?.includes('Odontogram'));
    const treatmentPlanLink = links
      .find((link) => link.textContent?.includes('Treatment plan'));
    const quoteLink = links
      .find((link) => link.textContent?.includes('Quote'));

    expect(clinicalRecordLink).toBeTruthy();
    expect(odontogramLink).toBeTruthy();
    expect(treatmentPlanLink).toBeTruthy();
    expect(quoteLink).toBeTruthy();
  });
});
