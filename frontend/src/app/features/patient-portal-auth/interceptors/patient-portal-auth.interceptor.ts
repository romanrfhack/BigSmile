import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import {
  isPatientPortalApiRequest,
  isPatientPortalPublicAuthRequest
} from '../../../core/auth/auth-endpoint-scope';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';

@Injectable()
export class PatientPortalAuthInterceptor implements HttpInterceptor {
  constructor(private readonly sessionStore: PatientPortalSessionStore) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!isPatientPortalApiRequest(req.url) || isPatientPortalPublicAuthRequest(req.url)) {
      return next.handle(req);
    }

    const token = this.sessionStore.getAccessToken();
    if (!token) {
      return next.handle(req);
    }

    return next.handle(req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    })).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.sessionStore.clear();
        }

        return throwError(() => error);
      })
    );
  }
}
