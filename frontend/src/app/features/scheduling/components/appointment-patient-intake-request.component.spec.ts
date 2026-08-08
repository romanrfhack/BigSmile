import {
  buildPatientAccessUrl,
  normalizeWhatsAppPhone
} from './appointment-patient-intake-request.component';
import { PreparedAppointmentPatientIntakeRequest } from '../models/scheduling.models';

describe('appointment patient intake request helpers', () => {
  it('builds an activation URL with the one-time token only in the fragment', () => {
    const prepared = request('Activation', 'raw token+/=');

    const url = buildPatientAccessUrl(prepared, 'https://bigsmile.example');

    expect(url).toBe(
      'https://bigsmile.example/patient-portal/activate#token=raw%20token%2B%2F%3D');
    expect(new URL(url).search).toBe('');
  });

  it('refuses to create an empty activation link', () => {
    expect(() => buildPatientAccessUrl(
      request('Activation', null),
      'https://bigsmile.example'
    )).toThrowError('Activation access requires a one-time token.');
  });

  it('builds a realm-scoped login URL for an active account', () => {
    expect(buildPatientAccessUrl(
      request('Login', null),
      'https://bigsmile.example'
    )).toBe('https://bigsmile.example/patient-portal/tenant-a/login');
  });

  it('normalizes Mexican WhatsApp numbers and rejects unusable values', () => {
    expect(normalizeWhatsAppPhone('55 1234 5678')).toBe('525512345678');
    expect(normalizeWhatsAppPhone('+52 55 1234 5678')).toBe('525512345678');
    expect(normalizeWhatsAppPhone('123')).toBeNull();
    expect(normalizeWhatsAppPhone(null)).toBeNull();
  });
});

function request(
  accessMode: 'Activation' | 'Login',
  activationToken: string | null
): PreparedAppointmentPatientIntakeRequest {
  return {
    accessMode,
    activationToken,
    status: {
      appointmentId: 'appointment-1',
      patientId: 'patient-1',
      patientFullName: 'Ana López',
      patientPrimaryPhone: '55 1234 5678',
      patientPortalRealm: 'tenant-a',
      portalAccessStatus: accessMode === 'Login' ? 'Active' : 'NotActivated',
      intakeStatus: 'NotStarted',
      recommendedAccess: accessMode,
      canRequest: true,
      submittedAtUtc: null
    }
  };
}
