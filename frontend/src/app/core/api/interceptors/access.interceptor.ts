import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthTokenService } from '../services/auth-token.service';

@Injectable()
export class AccessTokenInterceptor implements HttpInterceptor {
  constructor(private auth: AuthTokenService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.auth.accessToken;
    const authReq = token
      ? req.clone({
          setHeaders: { Authorization: `Bearer ${token}` },
          withCredentials: true,
        })
      : req.clone({ withCredentials: true });

    return next.handle(authReq);
  }
}
