import { TestBed } from '@angular/core/testing';
import { ClinicalTimelineListComponent } from './clinical-timeline-list.component';

describe('ClinicalTimelineListComponent', () => {
  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');

    await TestBed.configureTestingModule({
      imports: [ClinicalTimelineListComponent]
    }).compileComponents();
  });

  it('renders note and diagnosis timeline entries newest first', () => {
    const fixture = TestBed.createComponent(ClinicalTimelineListComponent);
    const component = fixture.componentInstance;

    component.timeline = [
      {
        eventType: 'ClinicalNoteCreated',
        occurredAtUtc: '2026-04-20T09:00:00Z',
        actorUserId: 'user-1',
        title: 'Clinical note added',
        summary: 'Older note summary.',
        referenceId: 'note-1'
      },
      {
        eventType: 'ClinicalDiagnosisCreated',
        occurredAtUtc: '2026-04-20T10:00:00Z',
        actorUserId: 'user-2',
        title: 'Diagnosis added',
        summary: 'Occlusal caries',
        referenceId: 'diagnosis-1'
      },
      {
        eventType: 'ClinicalDiagnosisResolved',
        occurredAtUtc: '2026-04-20T11:00:00Z',
        actorUserId: 'user-3',
        title: 'Diagnosis resolved',
        summary: 'Occlusal caries',
        referenceId: 'diagnosis-1'
      }
    ];

    fixture.detectChanges();

    const renderedEvents = Array.from(fixture.nativeElement.querySelectorAll('.timeline-card') as NodeListOf<HTMLElement>)
      .map((element) => ({
        eventType: element.getAttribute('data-event-type'),
        title: element.querySelector('strong')?.textContent?.trim(),
        summary: element.querySelector('.summary')?.textContent?.trim()
      }));

    expect(renderedEvents).toEqual([
      {
        eventType: 'ClinicalDiagnosisResolved',
        title: 'Diagnosis resolved',
        summary: 'Occlusal caries'
      },
      {
        eventType: 'ClinicalDiagnosisCreated',
        title: 'Diagnosis added',
        summary: 'Occlusal caries'
      },
      {
        eventType: 'ClinicalNoteCreated',
        title: 'Clinical note added',
        summary: 'Older note summary.'
      }
    ]);

    expect(fixture.nativeElement.textContent).toContain('Diagnosis created');
    expect(fixture.nativeElement.textContent).toContain('Diagnosis resolved');
    expect(fixture.nativeElement.textContent).toContain('Note created');
  });
});
