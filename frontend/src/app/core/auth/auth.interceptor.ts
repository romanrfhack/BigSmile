import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { isPatientPortalApiRequest } from './auth-endpoint-scope';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private readonly authService: AuthService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();
    if (token && !isPatientPortalApiRequest(req.url)) {
      return next.handle(req.clone({
        headers: req.headers.set('Authorization', `Bearer ${token}`)
      }));
    }

    return next.handle(req);
  }
}
