import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { ClinicalRecordsFacade } from '../facades/clinical-records.facade';
import { ClinicalRecordPageComponent } from './clinical-record.page';

describe('ClinicalRecordPageComponent', () => {
  let clinicalRecordsFacade: any;
  let patientsFacade: any;
  let createCalls: any[];
  let updateCalls: any[];
  let addNoteCalls: any[];
  let addDiagnosisCalls: any[];
  let resolveDiagnosisCalls: any[];

  beforeEach(async () => {
    createCalls = [];
    updateCalls = [];
    addNoteCalls = [];
    addDiagnosisCalls = [];
    resolveDiagnosisCalls = [];

    clinicalRecordsFacade = {
      currentRecord: signal<any | null>(null),
      loadingRecord: signal(false),
      recordMissing: signal(true),
      recordError: signal<string | null>(null),
      loadRecord: () => undefined,
      clearRecord: () => undefined,
      createRecord: (_patientId: string, payload: any) => {
        createCalls.push(payload);
        clinicalRecordsFacade.currentRecord.set({
          clinicalRecordId: 'record-1',
          patientId: 'patient-1',
          medicalBackgroundSummary: payload.medicalBackgroundSummary,
          currentMedicationsSummary: payload.currentMedicationsSummary,
          allergies: payload.allergies,
          notes: [],
          diagnoses: [],
          createdAtUtc: '2026-04-20T10:00:00Z',
          createdByUserId: 'user-1',
          lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
          lastUpdatedByUserId: 'user-1'
        });
        clinicalRecordsFacade.recordMissing.set(false);
        return of(clinicalRecordsFacade.currentRecord());
      },
      updateRecord: (_patientId: string, payload: any) => {
        updateCalls.push(payload);
        clinicalRecordsFacade.currentRecord.set({
          ...clinicalRecordsFacade.currentRecord(),
          medicalBackgroundSummary: payload.medicalBackgroundSummary,
          currentMedicationsSummary: payload.currentMedicationsSummary,
          allergies: payload.allergies,
          diagnoses: clinicalRecordsFacade.currentRecord()?.diagnoses ?? [],
          lastUpdatedAtUtc: '2026-04-20T11:00:00Z',
          lastUpdatedByUserId: 'user-2'
        });
        return of(clinicalRecordsFacade.currentRecord());
      },
      addNote: (_patientId: string, payload: any) => {
        addNoteCalls.push(payload);
        clinicalRecordsFacade.currentRecord.set({
          ...clinicalRecordsFacade.currentRecord(),
          notes: [
            {
              id: 'note-2',
              noteText: payload.noteText,
              createdAtUtc: '2026-04-20T12:00:00Z',
              createdByUserId: 'user-2'
            },
            ...(clinicalRecordsFacade.currentRecord()?.notes ?? [])
          ]
        });
        return of(clinicalRecordsFacade.currentRecord());
      },
      addDiagnosis: (_patientId: string, payload: any) => {
        addDiagnosisCalls.push(payload);
        clinicalRecordsFacade.currentRecord.set({
          ...clinicalRecordsFacade.currentRecord(),
          diagnoses: [
            {
              diagnosisId: `diagnosis-${addDiagnosisCalls.length}`,
              diagnosisText: payload.diagnosisText,
              notes: payload.notes,
              status: 'Active',
              createdAtUtc: '2026-04-20T12:30:00Z',
              createdByUserId: 'user-2',
              resolvedAtUtc: null,
              resolvedByUserId: null
            },
            ...(clinicalRecordsFacade.currentRecord()?.diagnoses ?? [])
          ]
        });
        return of(clinicalRecordsFacade.currentRecord());
      },
      resolveDiagnosis: (_patientId: string, diagnosisId: string) => {
        resolveDiagnosisCalls.push(diagnosisId);
        clinicalRecordsFacade.currentRecord.set({
          ...clinicalRecordsFacade.currentRecord(),
          diagnoses: (clinicalRecordsFacade.currentRecord()?.diagnoses ?? []).map((diagnosis: any) => diagnosis.diagnosisId === diagnosisId
            ? {
                ...diagnosis,
                status: 'Resolved',
                resolvedAtUtc: '2026-04-20T13:00:00Z',
                resolvedByUserId: 'user-3'
              }
            : diagnosis)
        });
        return of(clinicalRecordsFacade.currentRecord());
      }
    };

    patientsFacade = {
      currentPatient: signal({
        id: 'patient-1',
        fullName: 'Ana Lopez'
      }),
      loadingPatient: signal(false),
      detailError: signal<string | null>(null),
      clearCurrentPatient: () => undefined,
      loadPatient: () => undefined
    };

    await TestBed.configureTestingModule({
      imports: [ClinicalRecordPageComponent],
      providers: [
        { provide: ClinicalRecordsFacade, useValue: clinicalRecordsFacade },
        { provide: PatientsFacade, useValue: patientsFacade },
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

  it('renders the explicit empty state when the clinical record does not exist', () => {
    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No clinical record yet');
  });

  it('creates the clinical record only after the explicit create flow starts', () => {
    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.startCreate();
    component.saveSnapshot({
      medicalBackgroundSummary: 'Initial background',
      currentMedicationsSummary: 'Ibuprofen',
      allergies: []
    });

    expect(createCalls).toEqual([
      {
        medicalBackgroundSummary: 'Initial background',
        currentMedicationsSummary: 'Ibuprofen',
        allergies: []
      }
    ]);
    expect(component.creatingRecord).toBe(false);
  });

  it('updates the clinical snapshot when a record already exists', () => {
    clinicalRecordsFacade.recordMissing.set(false);
    clinicalRecordsFacade.currentRecord.set({
      clinicalRecordId: 'record-1',
      patientId: 'patient-1',
      medicalBackgroundSummary: 'Old background',
      currentMedicationsSummary: null,
      allergies: [],
      notes: [],
      diagnoses: [],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.saveSnapshot({
      medicalBackgroundSummary: 'Updated background',
      currentMedicationsSummary: 'Amoxicillin',
      allergies: [{ substance: 'Penicillin', reactionSummary: 'Hives', notes: null }]
    });

    expect(updateCalls[0]?.medicalBackgroundSummary).toBe('Updated background');
    expect(clinicalRecordsFacade.currentRecord()?.allergies[0]?.substance).toBe('Penicillin');
  });

  it('appends a clinical note through the facade', () => {
    clinicalRecordsFacade.recordMissing.set(false);
    clinicalRecordsFacade.currentRecord.set({
      clinicalRecordId: 'record-1',
      patientId: 'patient-1',
      medicalBackgroundSummary: 'Background',
      currentMedicationsSummary: null,
      allergies: [],
      diagnoses: [],
      notes: [
        {
          id: 'note-1',
          noteText: 'Older note',
          createdAtUtc: '2026-04-20T10:00:00Z',
          createdByUserId: 'user-1'
        }
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.addNote({ noteText: 'Newest note' });

    expect(addNoteCalls).toEqual([{ noteText: 'Newest note' }]);
    expect(clinicalRecordsFacade.currentRecord()?.notes[0]?.noteText).toBe('Newest note');
  });

  it('renders diagnoses with active and resolved states', () => {
    clinicalRecordsFacade.recordMissing.set(false);
    clinicalRecordsFacade.currentRecord.set({
      clinicalRecordId: 'record-1',
      patientId: 'patient-1',
      medicalBackgroundSummary: 'Background',
      currentMedicationsSummary: null,
      allergies: [],
      notes: [],
      diagnoses: [
        {
          diagnosisId: 'diagnosis-1',
          diagnosisText: 'Occlusal caries',
          notes: 'Watch upper molar.',
          status: 'Active',
          createdAtUtc: '2026-04-20T10:00:00Z',
          createdByUserId: 'user-1',
          resolvedAtUtc: null,
          resolvedByUserId: null
        },
        {
          diagnosisId: 'diagnosis-2',
          diagnosisText: 'Resolved gingivitis',
          notes: null,
          status: 'Resolved',
          createdAtUtc: '2026-04-20T09:00:00Z',
          createdByUserId: 'user-1',
          resolvedAtUtc: '2026-04-20T11:00:00Z',
          resolvedByUserId: 'user-2'
        }
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Diagnoses');
    expect(fixture.nativeElement.textContent).toContain('Occlusal caries');
    expect(fixture.nativeElement.textContent).toContain('Active');
    expect(fixture.nativeElement.textContent).toContain('Resolved');
  });

  it('adds a diagnosis through the facade', () => {
    clinicalRecordsFacade.recordMissing.set(false);
    clinicalRecordsFacade.currentRecord.set({
      clinicalRecordId: 'record-1',
      patientId: 'patient-1',
      medicalBackgroundSummary: 'Background',
      currentMedicationsSummary: null,
      allergies: [],
      notes: [],
      diagnoses: [],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.addDiagnosis({ diagnosisText: 'Occlusal caries', notes: 'Upper molar.' });

    expect(addDiagnosisCalls).toEqual([{ diagnosisText: 'Occlusal caries', notes: 'Upper molar.' }]);
    expect(clinicalRecordsFacade.currentRecord()?.diagnoses[0]?.diagnosisText).toBe('Occlusal caries');
  });

  it('marks a diagnosis as resolved through the facade', () => {
    clinicalRecordsFacade.recordMissing.set(false);
    clinicalRecordsFacade.currentRecord.set({
      clinicalRecordId: 'record-1',
      patientId: 'patient-1',
      medicalBackgroundSummary: 'Background',
      currentMedicationsSummary: null,
      allergies: [],
      notes: [],
      diagnoses: [
        {
          diagnosisId: 'diagnosis-1',
          diagnosisText: 'Occlusal caries',
          notes: null,
          status: 'Active',
          createdAtUtc: '2026-04-20T10:00:00Z',
          createdByUserId: 'user-1',
          resolvedAtUtc: null,
          resolvedByUserId: null
        }
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.resolveDiagnosis('diagnosis-1');

    expect(resolveDiagnosisCalls).toEqual(['diagnosis-1']);
    expect(clinicalRecordsFacade.currentRecord()?.diagnoses[0]?.status).toBe('Resolved');
  });
});
