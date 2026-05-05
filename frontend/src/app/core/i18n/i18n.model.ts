export type SupportedLanguageCode = 'es-MX' | 'en-US';

export interface SupportedLanguage {
  code: SupportedLanguageCode;
  label: string;
}

export const DEFAULT_LANGUAGE: SupportedLanguageCode = 'es-MX';

export const UI_LANGUAGE_STORAGE_KEY = 'bigsmile.ui.language';

export const SUPPORTED_LANGUAGES: SupportedLanguage[] = [
  { code: 'es-MX', label: 'Español (México)' },
  { code: 'en-US', label: 'English' }
];

export function normalizeLanguageCode(value: string | null | undefined): SupportedLanguageCode {
  return SUPPORTED_LANGUAGES.some((language) => language.code === value)
    ? value as SupportedLanguageCode
    : DEFAULT_LANGUAGE;
}
