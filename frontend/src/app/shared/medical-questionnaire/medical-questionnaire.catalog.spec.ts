import {
  MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS,
  MEDICAL_QUESTIONNAIRE_GROUPS,
  MEDICAL_QUESTIONNAIRE_KEYS
} from './medical-questionnaire.catalog';

describe('shared medical questionnaire catalog', () => {
  it('contains the accepted six groups and exactly 39 unique ordered keys', () => {
    expect(MEDICAL_QUESTIONNAIRE_GROUPS.map(group => group.id)).toEqual([
      'treatment-history',
      'allergies',
      'pressure-blood-coagulation',
      'systemic-diseases',
      'habits-dental-conditions',
      'pregnancy-anesthesia-special'
    ]);
    expect(MEDICAL_QUESTIONNAIRE_KEYS).toHaveLength(39);
    expect(new Set(MEDICAL_QUESTIONNAIRE_KEYS).size).toBe(39);
    expect(MEDICAL_QUESTIONNAIRE_KEYS[0]).toBe('currentMedicalTreatment');
    expect(MEDICAL_QUESTIONNAIRE_KEYS.at(-1)).toBe('anesthesiaComplications');
  });

  it('preserves the visible Yes, No, and Unknown answer metadata', () => {
    expect(MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS).toEqual([
      { value: 'Yes', labelKey: 'Yes' },
      { value: 'No', labelKey: 'No' },
      { value: 'Unknown', labelKey: 'No answer' }
    ]);
  });
});
