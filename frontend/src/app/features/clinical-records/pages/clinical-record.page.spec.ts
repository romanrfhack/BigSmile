import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { ClinicalEncounterVitalsSectionComponent } from '../components/clinical-encounter-vitals-section.component';
import { ClinicalRecordsFacade } from '../facades/clinical-records.facade';
import { ClinicalRecordPageComponent } from './clinical-record.page';

describe('ClinicalRecordPageComponent', () => {
  let clinicalRecordsFacade: any;
  let patientsFacade: any;
  let createCalls: any[];
  let updateCalls: any[];
  let addNoteCalls: any[];
  let addDiagnosisCalls: any[];
  let addEncounterCalls: any[];
  let resolveDiagnosisCalls: any[];
  let loadQuestionnaireCalls: string[];
  let loadEncountersCalls: string[];
  let updateQuestionnaireCalls: any[];

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    createCalls = [];
    updateCalls = [];
    addNoteCalls = [];
    addDiagnosisCalls = [];
    addEncounterCalls = [];
    resolveDiagnosisCalls = [];
    loadQuestionnaireCalls = [];
    loadEncountersCalls = [];
    updateQuestionnaireCalls = [];

    clinicalRecordsFacade = {
      currentRecord: signal<any | null>(null),
      loadingRecord: signal(false),
      recordMissing: signal(true),
      recordError: signal<string | null>(null),
      currentQuestionnaire: signal<any | null>(null),
      loadingQuestionnaire: signal(false),
      questionnaireMissing: signal(false),
      questionnaireError: signal<string | null>(null),
      currentEncounters: signal<any[]>([]),
      loadingEncounters: signal(false),
      encountersMissing: signal(false),
      encountersError: signal<string | null>(null),
      loadRecord: () => undefined,
      loadQuestionnaire: (patientId: string) => {
        loadQuestionnaireCalls.push(patientId);
      },
      loadEncounters: (patientId: string) => {
        loadEncountersCalls.push(patientId);
      },
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
          snapshotHistory: [
            {
              entryType: 'SnapshotInitialized',
              changedAtUtc: '2026-04-20T10:00:00Z',
              changedByUserId: 'user-1',
              section: 'Initial',
              summary: 'Clinical snapshot initialized'
            }
          ],
          timeline: [],
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
          snapshotHistory: clinicalRecordsFacade.currentRecord()?.snapshotHistory ?? [],
          timeline: clinicalRecordsFacade.currentRecord()?.timeline ?? [],
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
          ],
          timeline: [
            {
              eventType: 'ClinicalNoteCreated',
              occurredAtUtc: '2026-04-20T12:00:00Z',
              actorUserId: 'user-2',
              title: 'Clinical note added',
              summary: payload.noteText,
              referenceId: 'note-2'
            },
            ...(clinicalRecordsFacade.currentRecord()?.timeline ?? [])
          ]
        });
        return of(clinicalRecordsFacade.currentRecord());
      },
      addEncounter: (_patientId: string, payload: any) => {
        addEncounterCalls.push(payload);
        const encounter = {
          id: `encounter-${addEncounterCalls.length}`,
          clinicalRecordId: 'record-1',
          patientId: 'patient-1',
          occurredAtUtc: payload.occurredAtUtc,
          chiefComplaint: payload.chiefComplaint,
          consultationType: payload.consultationType,
          temperatureC: payload.temperatureC,
          bloodPressureSystolic: payload.bloodPressureSystolic,
          bloodPressureDiastolic: payload.bloodPressureDiastolic,
          weightKg: payload.weightKg,
          heightCm: payload.heightCm,
          respiratoryRatePerMinute: payload.respiratoryRatePerMinute,
          heartRateBpm: payload.heartRateBpm,
          clinicalNoteId: payload.noteText ? 'note-encounter-1' : null,
          noteText: payload.noteText,
          createdAtUtc: '2026-05-12T18:00:00Z',
          createdByUserId: 'user-2'
        };
        clinicalRecordsFacade.currentEncounters.set([encounter, ...clinicalRecordsFacade.currentEncounters()]);
        return of(encounter);
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
          ],
          timeline: [
            {
              eventType: 'ClinicalDiagnosisCreated',
              occurredAtUtc: '2026-04-20T12:30:00Z',
              actorUserId: 'user-2',
              title: 'Diagnosis added',
              summary: payload.notes ? `${payload.diagnosisText}: ${payload.notes}` : payload.diagnosisText,
              referenceId: `diagnosis-${addDiagnosisCalls.length}`
            },
            ...(clinicalRecordsFacade.currentRecord()?.timeline ?? [])
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
            : diagnosis),
          timeline: [
            {
              eventType: 'ClinicalDiagnosisResolved',
              occurredAtUtc: '2026-04-20T13:00:00Z',
              actorUserId: 'user-3',
              title: 'Diagnosis resolved',
              summary: (clinicalRecordsFacade.currentRecord()?.diagnoses ?? [])
                .find((diagnosis: any) => diagnosis.diagnosisId === diagnosisId)?.diagnosisText ?? 'Resolved diagnosis',
              referenceId: diagnosisId
            },
            ...(clinicalRecordsFacade.currentRecord()?.timeline ?? [])
          ]
        });
        return of(clinicalRecordsFacade.currentRecord());
      },
      updateQuestionnaire: (_patientId: string, payload: any) => {
        updateQuestionnaireCalls.push(payload);
        clinicalRecordsFacade.currentQuestionnaire.set({
          clinicalRecordId: 'record-1',
          patientId: 'patient-1',
          answers: payload.answers.map((answer: any) => ({
            id: null,
            questionKey: answer.questionKey,
            answer: answer.answer,
            details: answer.details,
            updatedAtUtc: null,
            updatedByUserId: null
          }))
        });
        return of(clinicalRecordsFacade.currentQuestionnaire());
      }
    };

    patientsFacade = {
      currentPatient: signal({
        id: 'patient-1',
        fullName: 'Ana Lopez',
        dateOfBirth: '1991-02-14',
        sex: 'Female',
        occupation: 'Dentist',
        maritalStatus: 'Single',
        referredBy: 'Existing patient',
        primaryPhone: '555-0101',
        email: null,
        isActive: true,
        hasClinicalAlerts: false,
        clinicalAlertsSummary: null
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

  it('renders patient age derived from date of birth without duplicating patient data', () => {
    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    fixture.detectChanges();

    const expectedAge = fixture.componentInstance.calculateAge('1991-02-14');

    expect(fixture.nativeElement.textContent).toContain('Age');
    expect(fixture.nativeElement.textContent).toContain(`${expectedAge} years`);
  });

  it('calculates patient age from date of birth against a reference date', () => {
    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);

    expect(fixture.componentInstance.calculateAge('1991-02-14', new Date(2026, 1, 13))).toBe(34);
    expect(fixture.componentInstance.calculateAge('1991-02-14', new Date(2026, 1, 14))).toBe(35);
  });

  it('loads the medical questionnaire when the clinical record page opens', () => {
    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    fixture.detectChanges();

    expect(loadQuestionnaireCalls).toEqual(['patient-1']);
  });

  it('loads clinical encounters when the clinical record page opens', () => {
    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    fixture.detectChanges();

    expect(loadEncountersCalls).toEqual(['patient-1']);
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
      snapshotHistory: [],
      timeline: [],
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
      snapshotHistory: [],
      timeline: [],
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

  it('adds a clinical encounter through the facade', () => {
    clinicalRecordsFacade.recordMissing.set(false);
    clinicalRecordsFacade.currentRecord.set({
      clinicalRecordId: 'record-1',
      patientId: 'patient-1',
      medicalBackgroundSummary: 'Background',
      currentMedicationsSummary: null,
      allergies: [],
      diagnoses: [],
      snapshotHistory: [],
      timeline: [],
      notes: [],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.addEncounter({
      occurredAtUtc: '2026-05-12T18:00:00.000Z',
      chiefComplaint: 'Tooth pain',
      consultationType: 'Urgency',
      temperatureC: 36.8,
      bloodPressureSystolic: 120,
      bloodPressureDiastolic: 80,
      weightKg: null,
      heightCm: null,
      respiratoryRatePerMinute: null,
      heartRateBpm: 78,
      noteText: 'Patient reports pain at night.'
    });

    expect(addEncounterCalls).toEqual([
      {
        occurredAtUtc: '2026-05-12T18:00:00.000Z',
        chiefComplaint: 'Tooth pain',
        consultationType: 'Urgency',
        temperatureC: 36.8,
        bloodPressureSystolic: 120,
        bloodPressureDiastolic: 80,
        weightKg: null,
        heightCm: null,
        respiratoryRatePerMinute: null,
        heartRateBpm: 78,
        noteText: 'Patient reports pain at night.'
      }
    ]);
    expect(clinicalRecordsFacade.currentEncounters()[0]?.chiefComplaint).toBe('Tooth pain');
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
      snapshotHistory: [
        {
          entryType: 'MedicalBackgroundUpdated',
          changedAtUtc: '2026-04-20T12:30:00Z',
          changedByUserId: 'user-2',
          section: 'MedicalBackground',
          summary: 'Medical background updated'
        },
        {
          entryType: 'SnapshotInitialized',
          changedAtUtc: '2026-04-20T10:00:00Z',
          changedByUserId: 'user-1',
          section: 'Initial',
          summary: 'Clinical snapshot initialized'
        }
      ],
      timeline: [
        {
          eventType: 'ClinicalDiagnosisResolved',
          occurredAtUtc: '2026-04-20T11:00:00Z',
          actorUserId: 'user-2',
          title: 'Diagnosis resolved',
          summary: 'Resolved gingivitis',
          referenceId: 'diagnosis-2'
        },
        {
          eventType: 'ClinicalDiagnosisCreated',
          occurredAtUtc: '2026-04-20T10:00:00Z',
          actorUserId: 'user-1',
          title: 'Diagnosis added',
          summary: 'Occlusal caries: Watch upper molar.',
          referenceId: 'diagnosis-1'
        }
      ],
      createdAtUtc: '2026-04-20T10:00:00Z',
      createdByUserId: 'user-1',
      lastUpdatedAtUtc: '2026-04-20T10:00:00Z',
      lastUpdatedByUserId: 'user-1'
    });

    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('#clinical-tab-diagnoses')?.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Diagnoses');
    expect(fixture.nativeElement.textContent).toContain('Occlusal caries');
    expect(fixture.nativeElement.textContent).toContain('Active');
    expect(fixture.nativeElement.textContent).toContain('Resolved');

    fixture.nativeElement.querySelector('#clinical-tab-history')?.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Snapshot history');
    expect(fixture.nativeElement.textContent).toContain('Clinical snapshot initialized');
    expect(fixture.nativeElement.textContent).toContain('Medical background updated');
    expect(fixture.nativeElement.textContent).toContain('Clinical timeline');
    expect(fixture.nativeElement.textContent).toContain('Diagnosis added');
    expect(fixture.nativeElement.textContent).toContain('Diagnosis resolved');
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
      snapshotHistory: [],
      timeline: [],
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
      snapshotHistory: [],
      timeline: [],
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

  it('saves the medical questionnaire through the facade', () => {
    const fixture = TestBed.createComponent(ClinicalRecordPageComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.saveQuestionnaire({
      answers: [
        {
          questionKey: 'diabetes',
          answer: 'Yes',
          details: 'Type 2'
        }
      ]
    });

    expect(updateQuestionnaireCalls).toEqual([
      {
        answers: [
          {
            questionKey: 'diabetes',
            answer: 'Yes',
            details: 'Type 2'
          }
        ]
      }
    ]);
  });

  it('keeps clinical page and component orchestration free of direct HTTP clients', () => {
    expect(ClinicalRecordPageComponent.toString()).not.toContain('HttpClient');
    expect(ClinicalRecordPageComponent.toString()).not.toContain('fetch');
    expect(ClinicalEncounterVitalsSectionComponent.toString()).not.toContain('HttpClient');
    expect(ClinicalEncounterVitalsSectionComponent.toString()).not.toContain('fetch');
  });
});
