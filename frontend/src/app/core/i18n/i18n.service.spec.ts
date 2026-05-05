import { TestBed } from '@angular/core/testing';
import { DEFAULT_LANGUAGE, UI_LANGUAGE_STORAGE_KEY } from './i18n.model';
import { I18nService } from './i18n.service';

describe('I18nService', () => {
  beforeEach(() => {
    window.localStorage.clear();
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    window.localStorage.clear();
    TestBed.resetTestingModule();
  });

  it('defaults to Spanish for Mexico when no preference exists', () => {
    const service = TestBed.inject(I18nService);

    expect(service.currentLanguage()).toBe(DEFAULT_LANGUAGE);
    expect(service.translate('Patients')).toBe('Pacientes');
  });

  it('persists only the selected language code', () => {
    const service = TestBed.inject(I18nService);

    service.setLanguage('en-US');

    expect(service.currentLanguage()).toBe('en-US');
    expect(window.localStorage.getItem(UI_LANGUAGE_STORAGE_KEY)).toBe('en-US');
    expect(service.translate('Patients')).toBe('Patients');
  });

  it('falls back safely to Spanish for unsupported stored values', () => {
    window.localStorage.setItem(UI_LANGUAGE_STORAGE_KEY, 'pt-BR');

    const service = TestBed.inject(I18nService);

    expect(service.currentLanguage()).toBe(DEFAULT_LANGUAGE);
    expect(service.translate('Dashboard')).toBe('Panel');
  });

  it('interpolates named translation parameters', () => {
    const service = TestBed.inject(I18nService);

    expect(service.translate('Current detail for tooth {toothCode}', { toothCode: '11' }))
      .toBe('Detalle actual del diente 11');
  });
});
