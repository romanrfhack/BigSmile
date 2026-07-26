import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import {
  isPatientIntakeAuthApiRequest,
  isPatientIntakeDraftApiRequest,
  isPatientPortalApiRequest,
  isPatientPortalPublicAuthRequest
} from '../../../core/auth/auth-endpoint-scope';
import { PatientIntakeSessionStore } from '../services/patient-intake-session.store';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';

@Injectable()
export class PatientPortalAuthInterceptor implements HttpInterceptor {
  constructor(
    private readonly patientSessionStore: PatientPortalSessionStore,
    private readonly intakeSessionStore: PatientIntakeSessionStore
  ) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!isPatientPortalApiRequest(req.url) || isPatientPortalPublicAuthRequest(req.url)) {
      return next.handle(req);
    }

    const usesIntakeSession = isPatientIntakeAuthApiRequest(req.url) ||
      (isPatientIntakeDraftApiRequest(req.url) && this.intakeSessionStore.isAuthenticated());
    const selectedStore = usesIntakeSession
      ? this.intakeSessionStore
      : this.patientSessionStore;
    const token = selectedStore.getAccessToken();

    if (!token) {
      return next.handle(req);
    }

    return next.handle(req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    })).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          selectedStore.clear();
        }

        return throwError(() => error);
      })
    );
  }
}
