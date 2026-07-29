import { Injectable, signal } from '@angular/core';
import {
  DEFAULT_LANGUAGE,
  SUPPORTED_LANGUAGES,
  SupportedLanguage,
  SupportedLanguageCode,
  UI_LANGUAGE_STORAGE_KEY,
  normalizeLanguageCode
} from './i18n.model';
import { PATIENT_INTAKE_LIFECYCLE_TRANSLATIONS } from './patient-intake-lifecycle-translations';
import { PATIENT_INTAKE_MEDICAL_TRANSLATIONS } from './patient-intake-medical-translations';
import { PATIENT_PORTAL_TRANSLATIONS } from './patient-portal-translations';
import { TRANSLATIONS, TranslationParams } from './translations';

@Injectable({
  providedIn: 'root'
})
export class I18nService {
  readonly supportedLanguages: SupportedLanguage[] = SUPPORTED_LANGUAGES;

  private readonly currentLanguageSignal = signal<SupportedLanguageCode>(this.readStoredLanguage());

  readonly currentLanguage = this.currentLanguageSignal.asReadonly();

  setLanguage(languageCode: string): void {
    const normalizedLanguage = normalizeLanguageCode(languageCode);
    this.currentLanguageSignal.set(normalizedLanguage);
    this.persistLanguage(normalizedLanguage);
  }

  translate(key: string | null | undefined, params?: TranslationParams): string {
    if (!key) {
      return '';
    }

    const language = this.currentLanguageSignal();
    const featureTemplate =
      PATIENT_INTAKE_LIFECYCLE_TRANSLATIONS[language][key] ??
      PATIENT_INTAKE_MEDICAL_TRANSLATIONS[language][key] ??
      PATIENT_PORTAL_TRANSLATIONS[language][key];
    const sharedTemplate = language === DEFAULT_LANGUAGE
      ? TRANSLATIONS[DEFAULT_LANGUAGE][key]
      : TRANSLATIONS[language][key];
    const template = featureTemplate ?? sharedTemplate ?? key;

    return interpolate(template, params);
  }

  private readStoredLanguage(): SupportedLanguageCode {
    try {
      return normalizeLanguageCode(window.localStorage.getItem(UI_LANGUAGE_STORAGE_KEY));
    } catch {
      return DEFAULT_LANGUAGE;
    }
  }

  private persistLanguage(languageCode: SupportedLanguageCode): void {
    try {
      window.localStorage.setItem(UI_LANGUAGE_STORAGE_KEY, languageCode);
    } catch {
      // Language persistence is a non-sensitive UI preference; failing closed keeps the in-memory value.
    }
  }
}

function interpolate(template: string, params?: TranslationParams): string {
  if (!params) {
    return template;
  }

  return template.replace(/\{(\w+)\}/g, (match, paramName: string) => {
    const value = params[paramName];
    return value === null || value === undefined ? '' : `${value}`;
  });
}
