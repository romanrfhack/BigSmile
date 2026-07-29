import { PATIENT_INTAKE_LIFECYCLE_TRANSLATIONS } from './patient-intake-lifecycle-translations';

describe('patient intake lifecycle translations', () => {
  it('keeps Spanish-first conflict, expiry and unsaved-navigation copy', () => {
    const es = PATIENT_INTAKE_LIFECYCLE_TRANSLATIONS['es-MX'];

    expect(es['You have unsaved changes.']).toBe('Tienes cambios sin guardar.');
    expect(es['Reload latest version']).toBe('Recargar versión más reciente');
    expect(es['This intake draft has expired.']).toBe('Este borrador de captura venció.');
    expect(es['Start a new draft']).toBe('Iniciar nuevo borrador');
    expect(PATIENT_INTAKE_LIFECYCLE_TRANSLATIONS['en-US']).toEqual({});
  });
});
