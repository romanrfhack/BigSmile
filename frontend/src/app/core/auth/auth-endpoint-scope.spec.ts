import {
  isPatientPortalApiRequest,
  isPatientPortalPublicAuthRequest
} from './auth-endpoint-scope';

describe('patient portal endpoint scope', () => {
  it('identifies patient portal API requests for relative and absolute URLs', () => {
    expect(isPatientPortalApiRequest('/api/patient-portal/auth/me')).toBe(true);
    expect(isPatientPortalApiRequest('https://example.test/api/patient-portal/auth/logout')).toBe(true);
    expect(isPatientPortalApiRequest('/api/patients')).toBe(false);
  });

  it('identifies activation and tenant-realm login as public patient auth requests', () => {
    expect(isPatientPortalPublicAuthRequest('/api/patient-portal/auth/activate')).toBe(true);
    expect(isPatientPortalPublicAuthRequest('/api/patient-portal/auth/realms/clinic-a/login')).toBe(true);
    expect(isPatientPortalPublicAuthRequest('/api/patient-portal/auth/me')).toBe(false);
    expect(isPatientPortalPublicAuthRequest('/api/patient-portal/auth/logout')).toBe(false);
  });
});
