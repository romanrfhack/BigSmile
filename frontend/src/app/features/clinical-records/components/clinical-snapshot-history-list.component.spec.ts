import { TestBed } from '@angular/core/testing';
import { ClinicalSnapshotHistoryListComponent } from './clinical-snapshot-history-list.component';

describe('ClinicalSnapshotHistoryListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClinicalSnapshotHistoryListComponent]
    }).compileComponents();
  });

  it('renders snapshot history entries newest first with all supported entry types', () => {
    const fixture = TestBed.createComponent(ClinicalSnapshotHistoryListComponent);
    const component = fixture.componentInstance;

    component.snapshotHistory = [
      {
        entryType: 'SnapshotInitialized',
        changedAtUtc: '2026-04-20T09:00:00Z',
        changedByUserId: 'user-1',
        section: 'Initial',
        summary: 'Clinical snapshot initialized'
      },
      {
        entryType: 'MedicalBackgroundUpdated',
        changedAtUtc: '2026-04-20T10:00:00Z',
        changedByUserId: 'user-2',
        section: 'MedicalBackground',
        summary: 'Medical background updated'
      },
      {
        entryType: 'CurrentMedicationsUpdated',
        changedAtUtc: '2026-04-20T11:00:00Z',
        changedByUserId: 'user-3',
        section: 'CurrentMedications',
        summary: 'Current medications updated'
      },
      {
        entryType: 'AllergiesUpdated',
        changedAtUtc: '2026-04-20T12:00:00Z',
        changedByUserId: 'user-4',
        section: 'Allergies',
        summary: 'Current allergies updated'
      }
    ];

    fixture.detectChanges();

    const renderedEntries = Array.from(fixture.nativeElement.querySelectorAll('.history-card') as NodeListOf<HTMLElement>)
      .map((element) => ({
        entryType: element.getAttribute('data-entry-type'),
        summary: element.querySelector('strong')?.textContent?.trim()
      }));

    expect(renderedEntries).toEqual([
      {
        entryType: 'AllergiesUpdated',
        summary: 'Current allergies updated'
      },
      {
        entryType: 'CurrentMedicationsUpdated',
        summary: 'Current medications updated'
      },
      {
        entryType: 'MedicalBackgroundUpdated',
        summary: 'Medical background updated'
      },
      {
        entryType: 'SnapshotInitialized',
        summary: 'Clinical snapshot initialized'
      }
    ]);

    expect(fixture.nativeElement.textContent).toContain('Snapshot initialized');
    expect(fixture.nativeElement.textContent).toContain('Medical background');
    expect(fixture.nativeElement.textContent).toContain('Current medications');
    expect(fixture.nativeElement.textContent).toContain('Current allergies');
  });
});
