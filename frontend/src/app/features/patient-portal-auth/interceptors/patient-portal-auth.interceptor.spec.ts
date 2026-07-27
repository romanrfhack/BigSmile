import {
  HttpEvent,
  HttpHandler,
  HttpRequest,
  HttpResponse
} from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { AuthInterceptor } from '../../../core/auth/auth.interceptor';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientIntakeSessionStore } from '../services/patient-intake-session.store';
import { PatientPortalSessionBoundary } from '../services/patient-portal-session-boundary.service';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';
import { PatientPortalAuthInterceptor } from './patient-portal-auth.interceptor';

class RecordingHandler implements HttpHandler {
  request: HttpRequest<unknown> | null = null;

  handle(req: HttpRequest<unknown>): Observable<HttpEvent<unknown>> {
    this.request = req;
    return of(new HttpResponse({ status: 200 }));
  }
}

describe('patient, intake, and staff auth interceptor separation', () => {
  it('attaches the linked-patient token to linked auth and the shared intake endpoint', () => {
    const patientStore = createPatientSession();
    const intakeStore = new PatientIntakeSessionStore();
    const interceptor = createInterceptor(patientStore, intakeStore);

    for (const url of [
      '/api/patient-portal/auth/me',
      '/api/patient-portal/intake'
    ]) {
      const handler = new RecordingHandler();
      interceptor.intercept(new HttpRequest('GET', url), handler).subscribe();
      expect(handler.request?.headers.get('Authorization')).toBe('Bearer patient-token');
    }
  });

  it('attaches the intake token to intake auth and the shared self-only draft endpoint', () => {
    const patientStore = new PatientPortalSessionStore();
    const intakeStore = createIntakeSession();
    const interceptor = createInterceptor(patientStore, intakeStore);

    for (const url of [
      '/api/patient-portal/intake-auth/me',
      '/api/patient-portal/intake'
    ]) {
      const handler = new RecordingHandler();
      interceptor.intercept(new HttpRequest('GET', url), handler).subscribe();
      expect(handler.request?.headers.get('Authorization')).toBe('Bearer intake-token');
    }
  });

  it('never sends a linked-patient token to intake-only auth endpoints', () => {
    const interceptor = createInterceptor(
      createPatientSession(),
      new PatientIntakeSessionStore()
    );
    const handler = new RecordingHandler();

    interceptor.intercept(
      new HttpRequest('GET', '/api/patient-portal/intake-auth/me'),
      handler
    ).subscribe();

    expect(handler.request?.headers.has('Authorization')).toBe(false);
  });

  it('never sends an intake token to linked-patient auth endpoints', () => {
    const interceptor = createInterceptor(
      new PatientPortalSessionStore(),
      createIntakeSession()
    );
    const handler = new RecordingHandler();

    interceptor.intercept(
      new HttpRequest('GET', '/api/patient-portal/auth/me'),
      handler
    ).subscribe();

    expect(handler.request?.headers.has('Authorization')).toBe(false);
  });

  it('fails closed and clears both stores when the shared endpoint sees ambiguous sessions', () => {
    const patientStore = createPatientSession();
    const intakeStore = createIntakeSession();
    const interceptor = createInterceptor(patientStore, intakeStore);
    const handler = new RecordingHandler();

    interceptor.intercept(new HttpRequest('GET', '/api/patient-portal/intake'), handler).subscribe();

    expect(handler.request?.headers.has('Authorization')).toBe(false);
    expect(patientStore.current()).toBeNull();
    expect(intakeStore.current()).toBeNull();
  });

  it('does not attach patient or intake tokens to public auth or staff endpoints', () => {
    const interceptor = createInterceptor(
      createPatientSession(),
      createIntakeSession()
    );

    for (const url of [
      '/api/patient-portal/auth/activate',
      '/api/patient-portal/auth/realms/clinic-a/login',
      '/api/patient-portal/intake-auth/activate',
      '/api/patient-portal/intake-auth/realms/clinic-a/login',
      '/api/patient-intake-links',
      '/api/patients'
    ]) {
      const handler = new RecordingHandler();
      interceptor.intercept(new HttpRequest('POST', url, {}), handler).subscribe();
      expect(handler.request?.headers.has('Authorization')).toBe(false);
    }
  });

  it('does not attach the staff token to any patient portal endpoint', () => {
    const staffAuth = { getToken: () => 'staff-token' } as AuthService;
    const interceptor = new AuthInterceptor(staffAuth);

    for (const url of [
      '/api/patient-portal/auth/me',
      '/api/patient-portal/intake-auth/me',
      '/api/patient-portal/intake'
    ]) {
      const handler = new RecordingHandler();
      interceptor.intercept(new HttpRequest('GET', url), handler).subscribe();
      expect(handler.request?.headers.has('Authorization')).toBe(false);
    }

    const staffHandler = new RecordingHandler();
    interceptor.intercept(new HttpRequest('GET', '/api/patient-intake-links'), staffHandler).subscribe();
    expect(staffHandler.request?.headers.get('Authorization')).toBe('Bearer staff-token');
  });

  function createInterceptor(
    patientStore: PatientPortalSessionStore,
    intakeStore: PatientIntakeSessionStore
  ): PatientPortalAuthInterceptor {
    return new PatientPortalAuthInterceptor(
      new PatientPortalSessionBoundary(patientStore, intakeStore)
    );
  }

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

  function createIntakeSession(): PatientIntakeSessionStore {
    const store = new PatientIntakeSessionStore();
    store.setSession({
      accessToken: 'intake-token',
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
      current: {
        accountId: 'intake-account-id',
        intakeId: 'intake-id',
        tenantSubdomain: 'clinic-a',
        loginName: 'new.patient',
        sessionVersion: 1
      }
    });
    return store;
  }
});
