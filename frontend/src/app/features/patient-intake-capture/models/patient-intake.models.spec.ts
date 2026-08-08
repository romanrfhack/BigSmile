import { MEDICAL_QUESTIONNAIRE_KEYS } from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';
import {
  PATIENT_INTAKE_MARITAL_STATUS_VALUES,
  PATIENT_INTAKE_SEX_VALUES,
  PatientIntakeDraft,
  PatientIntakeMedicalAnswerFormValue,
  PatientIntakeNonMedicalFormValue,
  buildSavePatientIntakeDraftRequest,
  isPatientIntakeReadyForSubmission,
  toPatientIntakeMedicalFormValue,
  toPatientIntakeNonMedicalFormValue
} from './patient-intake.models';

describe('patient intake form mapping', () => {
  it('uses only the backend-supported demographic enum values', () => {
    expect(PATIENT_INTAKE_SEX_VALUES).toEqual([
      'Unspecified',
      'Female',
      'Male',
      'Other'
    ]);
    expect(PATIENT_INTAKE_MARITAL_STATUS_VALUES).toEqual([
      'Unspecified',
      'Single',
      'Married',
      'Divorced',
      'Widowed',
      'Other'
    ]);
  });

  it('maps the authoritative draft into editable non-medical and medical values', () => {
    const intake = draft();
    intake.firstName = null;
    intake.mobilePhone = '+52 55 1234 5678';

    expect(toPatientIntakeNonMedicalFormValue(intake)).toMatchObject({
      firstName: '',
      lastName: 'López',
      mobilePhone: '+52 55 1234 5678',
      sex: 'Female',
      maritalStatus: 'Single'
    });
    expect(toPatientIntakeMedicalFormValue(intake).map(answer => answer.questionKey))
      .toEqual(MEDICAL_QUESTIONNAIRE_KEYS);
  });

  it('normalizes non-medical fields and preserves all 39 medical answers unchanged by default', () => {
    const intake = draft();
    const request = buildSavePatientIntakeDraftRequest(intake, nonMedicalValue());

    expect(request).toMatchObject({
      firstName: 'Ana',
      lastName: 'López',
      occupation: null,
      referredBy: 'Dra. Pérez',
      preferredPhone: '55 0000 0000',
      mobilePhone: null,
      email: 'ana@example.test',
      reasonForVisit: 'Revisión general.',
      concurrencyToken: 'rv1.token'
    });
    expect(request.medicalAnswers).toEqual(intake.medicalAnswers);
    expect(request.medicalAnswers).toHaveLength(39);
    expect(request).not.toHaveProperty('age');
    expect(request).not.toHaveProperty('tenantId');
    expect(request).not.toHaveProperty('patientId');
    expect(request).not.toHaveProperty('intakeId');
  });

  it('normalizes and reorders medical answers while preserving all non-medical fields', () => {
    const intake = draft();
    const medicalAnswers: PatientIntakeMedicalAnswerFormValue[] =
      toPatientIntakeMedicalFormValue(intake).reverse();
    const diabetes = medicalAnswers.find(answer => answer.questionKey === 'diabetes')!;
    diabetes.answer = 'Yes';
    diabetes.details = '  Diet controlled.  ';

    const request = buildSavePatientIntakeDraftRequest(intake, nonMedicalValue(), medicalAnswers);

    expect(request.medicalAnswers.map(answer => answer.questionKey)).toEqual(MEDICAL_QUESTIONNAIRE_KEYS);
    expect(request.medicalAnswers.find(answer => answer.questionKey === 'diabetes')).toEqual({
      questionKey: 'diabetes',
      answer: 'Yes',
      details: 'Diet controlled.'
    });
    expect(request.firstName).toBe('Ana');
    expect(request.lastName).toBe('López');
    expect(request.reasonForVisit).toBe('Revisión general.');
  });

  it('rejects incomplete or duplicated medical form snapshots before transport', () => {
    const intake = draft();
    const medicalAnswers = toPatientIntakeMedicalFormValue(intake);

    expect(() => buildSavePatientIntakeDraftRequest(
      intake,
      nonMedicalValue(),
      medicalAnswers.slice(1)
    )).toThrowError('Patient intake medical answers must contain the complete fixed questionnaire.');

    const duplicated = [...medicalAnswers];
    duplicated[1] = { ...duplicated[0] };
    expect(() => buildSavePatientIntakeDraftRequest(
      intake,
      nonMedicalValue(),
      duplicated
    )).toThrowError(/Duplicate patient intake medical answer/);
  });

  it('requires identity fields and an explicit answer to every question before submission', () => {
    const intake = draft();
    const nonMedical = toPatientIntakeNonMedicalFormValue(intake);
    const medical = toPatientIntakeMedicalFormValue(intake);

    expect(isPatientIntakeReadyForSubmission(nonMedical, medical)).toBe(false);

    const completedMedical = medical.map(answer => ({
      ...answer,
      answer: 'No' as const
    }));
    expect(isPatientIntakeReadyForSubmission(nonMedical, completedMedical)).toBe(true);

    expect(isPatientIntakeReadyForSubmission(
      { ...nonMedical, dateOfBirth: '' },
      completedMedical
    )).toBe(false);
  });
});

function nonMedicalValue(): PatientIntakeNonMedicalFormValue {
  return {
    firstName: '  Ana  ',
    lastName: '  López  ',
    dateOfBirth: '1990-05-10',
    sex: 'Female',
    occupation: '   ',
    maritalStatus: 'Single',
    referredBy: '  Dra. Pérez ',
    preferredPhone: ' 55 0000 0000 ',
    mobilePhone: '',
    homePhone: '',
    workPhone: '',
    email: ' ana@example.test ',
    responsiblePartyName: '',
    responsiblePartyRelationship: '',
    responsiblePartyPhone: '',
    reasonForVisit: '  Revisión general.  '
  };
}

function draft(): PatientIntakeDraft {
  return {
    origin: 'ExistingPatientPortal',
    status: 'Draft',
    firstName: 'Ana',
    lastName: 'López',
    dateOfBirth: '1990-05-10',
    sex: 'Female',
    occupation: null,
    maritalStatus: 'Single',
    referredBy: null,
    preferredPhone: null,
    mobilePhone: null,
    homePhone: null,
    workPhone: null,
    email: null,
    responsiblePartyName: null,
    responsiblePartyRelationship: null,
    responsiblePartyPhone: null,
    reasonForVisit: null,
    medicalAnswers: MEDICAL_QUESTIONNAIRE_KEYS.map((questionKey, index) => ({
      questionKey,
      answer: index % 3 === 0 ? 'Yes' : index % 3 === 1 ? 'No' : 'Unknown',
      details: index === 0 ? 'Preserve this detail.' : null
    })),
    currentRevisionNumber: 3,
    concurrencyToken: 'rv1.token',
    createdAtUtc: '2026-07-27T10:00:00Z',
    lastUpdatedAtUtc: '2026-07-27T10:00:00Z',
    lastEffectiveSavedAtUtc: '2026-07-27T10:00:00Z',
    submittedAtUtc: null,
    expiresAtUtc: '2026-08-26T10:00:00Z'
  };
}
