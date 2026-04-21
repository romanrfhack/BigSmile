import { TestBed } from '@angular/core/testing';
import { SurfaceFindingHistoryListComponent } from './surface-finding-history-list.component';

describe('SurfaceFindingHistoryListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SurfaceFindingHistoryListComponent]
    }).compileComponents();
  });

  it('renders only the selected tooth surface history newest first with added and removed labels', () => {
    const fixture = TestBed.createComponent(SurfaceFindingHistoryListComponent);
    const component = fixture.componentInstance;

    component.toothCode = '11';
    component.surfaceCode = 'O';
    component.entries = [
      {
        entryType: 'FindingAdded',
        toothCode: '11',
        surfaceCode: 'O',
        findingType: 'Caries',
        changedAtUtc: '2026-04-20T10:00:00Z',
        changedByUserId: 'user-1',
        summary: 'Finding added',
        referenceFindingId: 'finding-1'
      },
      {
        entryType: 'FindingRemoved',
        toothCode: '11',
        surfaceCode: 'O',
        findingType: 'Caries',
        changedAtUtc: '2026-04-20T12:00:00Z',
        changedByUserId: 'user-2',
        summary: 'Finding removed',
        referenceFindingId: 'finding-1'
      },
      {
        entryType: 'FindingAdded',
        toothCode: '11',
        surfaceCode: 'M',
        findingType: 'Sealant',
        changedAtUtc: '2026-04-20T11:00:00Z',
        changedByUserId: 'user-3',
        summary: 'Finding added',
        referenceFindingId: 'finding-2'
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
        entryType: 'FindingRemoved',
        summary: 'Finding removed'
      },
      {
        entryType: 'FindingAdded',
        summary: 'Finding added'
      }
    ]);

    expect(fixture.nativeElement.textContent).toContain('Finding added');
    expect(fixture.nativeElement.textContent).toContain('Finding removed');
    expect(fixture.nativeElement.textContent).not.toContain('Sealant');
  });
});
