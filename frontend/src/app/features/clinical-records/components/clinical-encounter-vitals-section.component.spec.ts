import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  AddClinicalEncounterRequest,
  ClinicalEncounter
} from '../models/clinical-record.models';
import { ClinicalEncounterVitalsSectionComponent } from './clinical-encounter-vitals-section.component';

describe('ClinicalEncounterVitalsSectionComponent', () => {
  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');

    await TestBed.configureTestingModule({
      imports: [ClinicalEncounterVitalsSectionComponent]
    }).compileComponents();
  });

  it('renders recent clinical encounters with captured vitals and linked note text', () => {
    const fixture = createComponent([buildEncounter()]);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;

    expect(text).toContain('Consultation and vital signs');
    expect(text).toContain('Tooth pain');
    expect(text).toContain('Urgency');
    expect(text).toContain('36.8');
    expect(text).toContain('120/80');
    expect(text).toContain('Patient reports pain at night.');
  });

  it('renders the empty state when there are no clinical encounters', () => {
    const fixture = createComponent([]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No encounters captured yet');
    expect(fixture.nativeElement.textContent).toContain('Recent consultation and vital sign records will appear here newest first.');
  });

  it('creates a normalized clinical encounter payload', () => {
    const fixture = createComponent([]);
    const savedPayloads: AddClinicalEncounterRequest[] = [];
    fixture.componentInstance.saved.subscribe((payload) => savedPayloads.push(payload));
    fixture.detectChanges();

    fixture.componentInstance.form.setValue({
      occurredAtLocal: '2026-05-12T10:30',
      chiefComplaint: '  Tooth sensitivity  ',
      consultationType: 'Treatment',
      temperatureC: 36.5,
      bloodPressureSystolic: 118,
      bloodPressureDiastolic: 76,
      weightKg: null,
      heightCm: 171.2,
      respiratoryRatePerMinute: null,
      heartRateBpm: 72,
      noteText: '   '
    });

    fixture.componentInstance.submit();

    expect(savedPayloads).toHaveLength(1);
    expect(savedPayloads[0]).toEqual({
      occurredAtUtc: new Date('2026-05-12T10:30').toISOString(),
      chiefComplaint: 'Tooth sensitivity',
      consultationType: 'Treatment',
      temperatureC: 36.5,
      bloodPressureSystolic: 118,
      bloodPressureDiastolic: 76,
      weightKg: null,
      heightCm: 171.2,
      respiratoryRatePerMinute: null,
      heartRateBpm: 72,
      noteText: null
    });
  });

  it('handles loading, load error, save error, and blood pressure validation states', () => {
    const fixture = TestBed.createComponent(ClinicalEncounterVitalsSectionComponent);
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Loading');

    fixture.componentRef.setInput('loading', false);
    fixture.componentRef.setInput('error', 'The clinical encounters could not be loaded.');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('The clinical encounters could not be loaded.');

    fixture.componentRef.setInput('error', null);
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('saveError', 'The clinical encounter could not be saved.');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('The clinical encounter could not be saved.');

    fixture.componentInstance.form.controls.bloodPressureSystolic.setValue(120);
    fixture.componentInstance.form.controls.bloodPressureSystolic.markAsTouched();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Blood pressure requires both systolic and diastolic values.');
  });
});

function createComponent(encounters: ClinicalEncounter[]): ComponentFixture<ClinicalEncounterVitalsSectionComponent> {
  const fixture = TestBed.createComponent(ClinicalEncounterVitalsSectionComponent);
  fixture.componentRef.setInput('encounters', encounters);
  fixture.componentRef.setInput('canWrite', true);

  return fixture;
}

function buildEncounter(): ClinicalEncounter {
  return {
    id: 'encounter-1',
    clinicalRecordId: 'record-1',
    patientId: 'patient-1',
    occurredAtUtc: '2026-05-12T18:00:00Z',
    chiefComplaint: 'Tooth pain',
    consultationType: 'Urgency',
    temperatureC: 36.8,
    bloodPressureSystolic: 120,
    bloodPressureDiastolic: 80,
    weightKg: 68.2,
    heightCm: 171,
    respiratoryRatePerMinute: 18,
    heartRateBpm: 78,
    clinicalNoteId: 'note-1',
    noteText: 'Patient reports pain at night.',
    createdAtUtc: '2026-05-12T18:05:00Z',
    createdByUserId: 'user-1'
  };
}
