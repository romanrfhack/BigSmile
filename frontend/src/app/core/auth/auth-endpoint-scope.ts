const PATIENT_PORTAL_API_MARKER = '/api/patient-portal/';
const PATIENT_PORTAL_ACTIVATION_MARKER = '/api/patient-portal/auth/activate';
const PATIENT_PORTAL_LOGIN_MARKER = '/api/patient-portal/auth/realms/';

export function isPatientPortalApiRequest(url: string): boolean {
  return normalizeUrl(url).includes(PATIENT_PORTAL_API_MARKER);
}

export function isPatientPortalPublicAuthRequest(url: string): boolean {
  const normalizedUrl = normalizeUrl(url);
  if (normalizedUrl.includes(PATIENT_PORTAL_ACTIVATION_MARKER)) {
    return true;
  }

  const loginIndex = normalizedUrl.indexOf(PATIENT_PORTAL_LOGIN_MARKER);
  return loginIndex >= 0 && normalizedUrl.slice(loginIndex).includes('/login');
}

function normalizeUrl(url: string): string {
  return (url ?? '').trim().toLowerCase();
}
