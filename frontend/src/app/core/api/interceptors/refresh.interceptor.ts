import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError, from } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthTokenService } from '../services/auth-token.service';

@Injectable()
export class RefreshTokenInterceptor implements HttpInterceptor {
  private refreshing = false;
  private readonly excludedPattern = '/auth/';

  constructor(private auth: AuthTokenService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401 && !this.refreshing && !req.url.includes(this.excludedPattern)) {
          this.refreshing = true;

          return from(this.auth.refreshAccessToken()).pipe(
            switchMap(() => {
              this.refreshing = false;
              const newToken = this.auth.accessToken;

              const retryReq = req.clone({
                setHeaders: newToken ? { Authorization: `Bearer ${newToken}` } : {},
                withCredentials: true,
              });
              return next.handle(retryReq);
            }),
            catchError((innerErr) => {
              this.refreshing = false;
              this.auth.logout();
              return throwError(() => innerErr);
            }),
          );
        }

        return throwError(() => err);
      }),
    );
  }
}
