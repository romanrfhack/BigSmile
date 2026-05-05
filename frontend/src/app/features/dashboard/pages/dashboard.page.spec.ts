import { WritableSignal, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { DashboardFacade } from '../facades/dashboard.facade';
import { DashboardSummary } from '../models/dashboard-summary.models';
import { DashboardPageComponent } from './dashboard.page';

describe('DashboardPageComponent', () => {
  let dashboardFacade: {
    summary: WritableSignal<DashboardSummary | null>;
    loading: WritableSignal<boolean>;
    error: WritableSignal<string | null>;
    loadSummary: () => void;
  };
  let loadCount: number;

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    loadCount = 0;
    dashboardFacade = {
      summary: signal<DashboardSummary | null>(null),
      loading: signal(false),
      error: signal<string | null>(null),
      loadSummary: () => {
        loadCount += 1;
      }
    };

    await TestBed.configureTestingModule({
      imports: [DashboardPageComponent],
      providers: [
        {
          provide: DashboardFacade,
          useValue: dashboardFacade
        }
      ]
    }).compileComponents();
  });

  it('loads the dashboard summary on init and renders loading state', () => {
    dashboardFacade.loading.set(true);

    const fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();

    expect(loadCount).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Loading dashboard summary...');
  });

  it('renders the error state', () => {
    dashboardFacade.error.set('The dashboard summary could not be loaded.');

    const fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('The dashboard summary could not be loaded.');
  });

  it('renders summary cards with operational values', () => {
    dashboardFacade.summary.set({
      activePatientsCount: 12,
      todayAppointmentsCount: 6,
      todayPendingAppointmentsCount: 2,
      activeDocumentsCount: 18,
      activeTreatmentPlansCount: 4,
      acceptedQuotesCount: 3,
      issuedBillingDocumentsCount: 1,
      generatedAtUtc: '2026-04-24T15:00:00Z'
    });

    const fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Operational dashboard');
    expect(text).toContain('Active patients');
    expect(text).toContain('12');
    expect(text).toContain('Issued billing');
    expect(text).toContain('1');
  });
});
