import { TestBed } from '@angular/core/testing';
import { DashboardSummaryCardsComponent } from './dashboard-summary-cards.component';

describe('DashboardSummaryCardsComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardSummaryCardsComponent]
    }).compileComponents();
  });

  it('renders the dashboard KPI cards with their values', () => {
    const fixture = TestBed.createComponent(DashboardSummaryCardsComponent);
    fixture.componentInstance.summary = {
      activePatientsCount: 12,
      todayAppointmentsCount: 6,
      todayPendingAppointmentsCount: 2,
      activeDocumentsCount: 18,
      activeTreatmentPlansCount: 4,
      acceptedQuotesCount: 3,
      issuedBillingDocumentsCount: 1,
      generatedAtUtc: '2026-04-24T15:00:00Z'
    };

    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Active patients');
    expect(text).toContain('12');
    expect(text).toContain('Today appointments');
    expect(text).toContain('6');
    expect(text).toContain('Today pending');
    expect(text).toContain('2');
    expect(text).toContain('Issued billing');
    expect(text).toContain('1');
  });
});
