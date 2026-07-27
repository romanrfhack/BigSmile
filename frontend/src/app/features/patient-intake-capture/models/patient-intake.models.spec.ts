import {
  PATIENT_INTAKE_MARITAL_STATUS_VALUES,
  PATIENT_INTAKE_SEX_VALUES,
  PatientIntakeDraft,
  PatientIntakeNonMedicalFormValue,
  buildSavePatientIntakeDraftRequest,
  toPatientIntakeNonMedicalFormValue
} from './patient-intake.models';

describe('patient intake non-medical form mapping', () => {
  it('uses only the backend-supported enum values', () => {
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

  it('maps the authoritative draft into editable non-medical values', () => {
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
  });

  it('normalizes the non-medical fields and preserves all 39 medical answers unchanged', () => {
    const intake = draft();
    const formValue: PatientIntakeNonMedicalFormValue = {
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

    const request = buildSavePatientIntakeDraftRequest(intake, formValue);

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
});

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
    medicalAnswers: Array.from({ length: 39 }, (_, index) => ({
      questionKey: `question-${index + 1}`,
      answer: index % 3 === 0 ? 'Yes' : index % 3 === 1 ? 'No' : 'Unknown',
      details: index === 0 ? 'Preserve this detail.' : null
    })),
    currentRevisionNumber: 3,
    concurrencyToken: 'rv1.token',
    createdAtUtc: '2026-07-27T10:00:00Z',
    lastUpdatedAtUtc: '2026-07-27T10:00:00Z',
    lastEffectiveSavedAtUtc: '2026-07-27T10:00:00Z',
    expiresAtUtc: '2026-08-26T10:00:00Z'
  };
}
