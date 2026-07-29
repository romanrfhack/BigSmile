import {
  MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS,
  MEDICAL_QUESTIONNAIRE_GROUPS,
  MEDICAL_QUESTIONNAIRE_KEYS
} from '../../../shared/medical-questionnaire/medical-questionnaire.catalog';
import {
  CLINICAL_MEDICAL_ANSWER_OPTIONS,
  CLINICAL_MEDICAL_QUESTIONNAIRE_GROUPS,
  CLINICAL_MEDICAL_QUESTION_KEYS
} from './clinical-medical-questionnaire.catalog';

describe('clinical medical questionnaire catalog adapter', () => {
  it('reuses the exact shared groups, keys, and answer metadata', () => {
    expect(CLINICAL_MEDICAL_QUESTIONNAIRE_GROUPS).toBe(MEDICAL_QUESTIONNAIRE_GROUPS);
    expect(CLINICAL_MEDICAL_QUESTION_KEYS).toBe(MEDICAL_QUESTIONNAIRE_KEYS);
    expect(CLINICAL_MEDICAL_ANSWER_OPTIONS).toBe(MEDICAL_QUESTIONNAIRE_ANSWER_OPTIONS);
  });
});
