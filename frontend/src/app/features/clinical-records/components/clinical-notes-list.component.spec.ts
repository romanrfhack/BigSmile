import { TestBed } from '@angular/core/testing';
import { ClinicalNotesListComponent } from './clinical-notes-list.component';

describe('ClinicalNotesListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClinicalNotesListComponent]
    }).compileComponents();
  });

  it('renders clinical notes newest first in the UI', () => {
    const fixture = TestBed.createComponent(ClinicalNotesListComponent);
    const component = fixture.componentInstance;

    component.notes = [
      {
        id: 'note-1',
        noteText: 'Older note',
        createdAtUtc: '2026-04-20T10:00:00Z',
        createdByUserId: 'user-1'
      },
      {
        id: 'note-2',
        noteText: 'Newest note',
        createdAtUtc: '2026-04-20T11:00:00Z',
        createdByUserId: 'user-2'
      }
    ];

    fixture.detectChanges();

    const renderedNotes = Array.from(fixture.nativeElement.querySelectorAll('.note-card p') as NodeListOf<HTMLParagraphElement>)
      .map((element) => element.textContent?.trim());

    expect(renderedNotes).toEqual(['Newest note', 'Older note']);
  });
});
