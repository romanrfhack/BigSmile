import {
  HttpEvent,
  HttpHandler,
  HttpRequest,
  HttpResponse
} from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { AuthInterceptor } from '../../../core/auth/auth.interceptor';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';
import { PatientPortalAuthInterceptor } from './patient-portal-auth.interceptor';

class RecordingHandler implements HttpHandler {
  request: HttpRequest<unknown> | null = null;

  handle(req: HttpRequest<unknown>): Observable<HttpEvent<unknown>> {
    this.request = req;
    return of(new HttpResponse({ status: 200 }));
  }
}

describe('patient and staff auth interceptor separation', () => {
  it('attaches the patient token only to protected patient portal endpoints', () => {
    const store = createPatientSession();
    const interceptor = new PatientPortalAuthInterceptor(store);
    const handler = new RecordingHandler();

    interceptor.intercept(new HttpRequest('GET', '/api/patient-portal/auth/me'), handler).subscribe();

    expect(handler.request?.headers.get('Authorization')).toBe('Bearer patient-token');
  });

  it('does not attach the patient token to activation, login, or staff endpoints', () => {
    const store = createPatientSession();
    const interceptor = new PatientPortalAuthInterceptor(store);

    for (const url of [
      '/api/patient-portal/auth/activate',
      '/api/patient-portal/auth/realms/clinic-a/login',
      '/api/patients'
    ]) {
      const handler = new RecordingHandler();
      interceptor.intercept(new HttpRequest('POST', url, {}), handler).subscribe();
      expect(handler.request?.headers.has('Authorization')).toBe(false);
    }
  });

  it('does not attach the staff token to patient portal endpoints', () => {
    const staffAuth = { getToken: () => 'staff-token' } as AuthService;
    const interceptor = new AuthInterceptor(staffAuth);
    const patientHandler = new RecordingHandler();
    const staffHandler = new RecordingHandler();

    interceptor.intercept(new HttpRequest('GET', '/api/patient-portal/auth/me'), patientHandler).subscribe();
    interceptor.intercept(new HttpRequest('GET', '/api/patients'), staffHandler).subscribe();

    expect(patientHandler.request?.headers.has('Authorization')).toBe(false);
    expect(staffHandler.request?.headers.get('Authorization')).toBe('Bearer staff-token');
  });

  function createPatientSession(): PatientPortalSessionStore {
    const store = new PatientPortalSessionStore();
    store.setSession({
      accessToken: 'patient-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'account-id',
        patientId: 'patient-id',
        tenantSubdomain: 'clinic-a',
        loginName: 'patient.login',
        sessionVersion: 1
      }
    });
    return store;
  }
});
