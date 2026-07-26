const PATIENT_PORTAL_API_MARKER = '/api/patient-portal/';
const PATIENT_PORTAL_ACTIVATION_MARKER = '/api/patient-portal/auth/activate';
const PATIENT_PORTAL_LOGIN_MARKER = '/api/patient-portal/auth/realms/';
const PATIENT_INTAKE_AUTH_MARKER = '/api/patient-portal/intake-auth/';
const PATIENT_INTAKE_ACTIVATION_MARKER = '/api/patient-portal/intake-auth/activate';
const PATIENT_INTAKE_LOGIN_MARKER = '/api/patient-portal/intake-auth/realms/';
const PATIENT_INTAKE_API_MARKER = '/api/patient-portal/intake';

export function isPatientPortalApiRequest(url: string): boolean {
  return normalizeUrl(url).includes(PATIENT_PORTAL_API_MARKER);
}

export function isPatientIntakeAuthApiRequest(url: string): boolean {
  return normalizeUrl(url).includes(PATIENT_INTAKE_AUTH_MARKER);
}

export function isPatientIntakeDraftApiRequest(url: string): boolean {
  const normalizedUrl = normalizeUrl(url).split(/[?#]/, 1)[0].replace(/\/+$/, '');
  return normalizedUrl.endsWith(PATIENT_INTAKE_API_MARKER);
}

export function isPatientPortalPublicAuthRequest(url: string): boolean {
  const normalizedUrl = normalizeUrl(url);
  if (
    normalizedUrl.includes(PATIENT_PORTAL_ACTIVATION_MARKER) ||
    normalizedUrl.includes(PATIENT_INTAKE_ACTIVATION_MARKER)
  ) {
    return true;
  }

  return isRealmLoginRequest(normalizedUrl, PATIENT_PORTAL_LOGIN_MARKER) ||
    isRealmLoginRequest(normalizedUrl, PATIENT_INTAKE_LOGIN_MARKER);
}

function isRealmLoginRequest(normalizedUrl: string, marker: string): boolean {
  const loginIndex = normalizedUrl.indexOf(marker);
  return loginIndex >= 0 && normalizedUrl.slice(loginIndex).includes('/login');
}

function normalizeUrl(url: string): string {
  return (url ?? '').trim().toLowerCase();
}
