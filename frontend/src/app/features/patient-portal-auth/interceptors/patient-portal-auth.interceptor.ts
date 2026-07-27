import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import {
  isPatientIntakeAuthApiRequest,
  isPatientIntakeDraftApiRequest,
  isPatientPortalApiRequest,
  isPatientPortalPublicAuthRequest
} from '../../../core/auth/auth-endpoint-scope';
import {
  PatientPortalSessionBoundary,
  PatientPortalSessionMode
} from '../services/patient-portal-session-boundary.service';

@Injectable()
export class PatientPortalAuthInterceptor implements HttpInterceptor {
  constructor(private readonly sessionBoundary: PatientPortalSessionBoundary) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!isPatientPortalApiRequest(req.url) || isPatientPortalPublicAuthRequest(req.url)) {
      return next.handle(req);
    }

    const resolution = this.sessionBoundary.resolve();
    if (resolution.state !== 'active') {
      return next.handle(req);
    }

    const mode = resolution.session.mode;
    if (!this.isModeAllowedForRequest(req.url, mode)) {
      return next.handle(req);
    }

    return next.handle(req.clone({
      headers: req.headers.set('Authorization', `Bearer ${resolution.session.accessToken}`)
    })).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.sessionBoundary.clearMode(mode);
        }

        return throwError(() => error);
      })
    );
  }

  private isModeAllowedForRequest(url: string, mode: PatientPortalSessionMode): boolean {
    if (isPatientIntakeDraftApiRequest(url)) {
      return mode === 'patient' || mode === 'patient_intake';
    }

    if (isPatientIntakeAuthApiRequest(url)) {
      return mode === 'patient_intake';
    }

    return mode === 'patient';
  }
}
