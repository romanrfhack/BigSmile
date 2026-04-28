import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ReminderTemplateManagerComponent } from './reminder-template-manager.component';

describe('ReminderTemplateManagerComponent', () => {
  let fixture: ComponentFixture<ReminderTemplateManagerComponent>;
  let component: ReminderTemplateManagerComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReminderTemplateManagerComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ReminderTemplateManagerComponent);
    component = fixture.componentInstance;
  });

  it('renders the reminder template section as internal drafts with no send action', () => {
    component.templates = [];

    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Reminder templates');
    expect(text).toContain('Templates are internal drafts only. BigSmile does not send messages.');
    expect(text).toContain('No active reminder templates');
    const sendButtons = fixture.debugElement
      .queryAll(By.css('button'))
      .filter(button => button.nativeElement.textContent.trim() === 'Send');
    expect(sendButtons.length).toBe(0);
  });

  it('emits create edit deactivate and preview actions', () => {
    const saves: unknown[] = [];
    const deactivations: string[] = [];
    const previews: unknown[] = [];
    component.canWrite = true;
    component.previewAppointmentId = 'appointment-1';
    component.templates = [
      {
        id: 'template-1',
        name: 'Confirmacion',
        body: 'Hola {{patientName}}.',
        isActive: true,
        createdAtUtc: '2026-04-27T08:00:00Z',
        createdByUserId: 'user-1',
        updatedAtUtc: null,
        updatedByUserId: null,
        deactivatedAtUtc: null,
        deactivatedByUserId: null
      }
    ];
    component.saved.subscribe(value => saves.push(value));
    component.deactivateRequested.subscribe(value => deactivations.push(value));
    component.previewRequested.subscribe(value => previews.push(value));

    component.name = ' New template ';
    component.body = ' Hola {{patientName}}. ';
    component.submit();
    component.startEdit(component.templates[0]!);
    component.name = 'Updated';
    component.body = 'Updated body';
    component.submit();

    fixture.detectChanges();
    const buttons = fixture.debugElement.queryAll(By.css('button'));
    buttons.find(button => button.nativeElement.textContent.includes('Preview'))?.triggerEventHandler('click');
    buttons.find(button => button.nativeElement.textContent.includes('Deactivate'))?.triggerEventHandler('click');

    expect(saves).toEqual([
      {
        id: null,
        name: 'New template',
        body: 'Hola {{patientName}}.'
      },
      {
        id: 'template-1',
        name: 'Updated',
        body: 'Updated body'
      }
    ]);
    expect(previews).toEqual([{ templateId: 'template-1', appointmentId: 'appointment-1' }]);
    expect(deactivations).toEqual(['template-1']);
  });

  it('shows rendered preview and unknown placeholders', () => {
    component.templates = [
      {
        id: 'template-1',
        name: 'Confirmacion',
        body: 'Hola {{patientName}}.',
        isActive: true,
        createdAtUtc: '2026-04-27T08:00:00Z',
        createdByUserId: 'user-1',
        updatedAtUtc: null,
        updatedByUserId: null,
        deactivatedAtUtc: null,
        deactivatedByUserId: null
      }
    ];
    component.previewAppointmentId = 'appointment-1';
    component.preview = {
      templateId: 'template-1',
      appointmentId: 'appointment-1',
      renderedBody: 'Hola Ana Lopez.',
      unknownPlaceholders: ['doctorName']
    };

    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Preview');
    expect(text).toContain('Hola Ana Lopez.');
    expect(text).toContain('Unknown placeholders: doctorName');
  });
});
