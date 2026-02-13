import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthTokenService } from '../services/auth-token.service';
import { environment } from '../../../../environments/environment';

@Injectable()
export class CsrfInterceptor implements HttpInterceptor {
  constructor(private auth: AuthTokenService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const csrf = this.auth.csrfToken;

    const isBackendCall = req.url.startsWith(environment.backendUrl);

    const isUnsafeMethod = ['POST', 'PUT', 'PATCH', 'DELETE'].includes(req.method.toUpperCase());

    if (!csrf || !isBackendCall || !isUnsafeMethod) {
      return next.handle(req);
    }

    const needsCsrfEndpoint = req.url.includes('/auth/refresh') || req.url.includes('/auth/logout');
    if (!needsCsrfEndpoint) return next.handle(req);

    return next.handle(
      req.clone({
        setHeaders: { 'X-CSRF-TOKEN': csrf },
      }),
    );
  }
}
