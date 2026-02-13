import { environment } from '../../../environments/environment';
import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { setUser, clearUser } from '../stores/user.actions';
import { AuthResponse } from '../models/auth-response.model';
import { firstValueFrom } from 'rxjs';
import { AuthTokenService } from '../api/services/auth-token.service';

@Injectable({ providedIn: 'root' })
export class SessionManagerService {
  private http = inject(HttpClient);
  private store = inject(Store);
  private auth = inject(AuthTokenService);

  loading = signal(true);

  async restoreSession(): Promise<void> {
    try {
      await this.auth.ensureCsrfToken();

      const headers = this.auth.csrfToken
        ? new HttpHeaders({ 'X-CSRF-TOKEN': this.auth.csrfToken })
        : undefined;

      const res = await firstValueFrom(
        this.http.post<AuthResponse>(
          `${environment.backendUrl}/auth/refresh`,
          {},
          {
            withCredentials: true,
            headers,
          },
        ),
      );

      if (res?.Token) {
        this.store.dispatch(setUser({ user: res }));
      } else {
        this.store.dispatch(clearUser());
      }
    } catch (err) {
      console.warn('Session restore failed:', err);
      this.store.dispatch(clearUser());
    } finally {
      this.loading.set(false);
    }
  }
}
