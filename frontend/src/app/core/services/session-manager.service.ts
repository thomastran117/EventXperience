import { environment } from '../../../environments/environment';
import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { setUser, clearUser } from '../stores/user.actions';
import { AuthResponse } from '../models/auth-response.model';

@Injectable({ providedIn: 'root' })
export class SessionManagerService {
  private http = inject(HttpClient);
  private store = inject(Store);

  loading = signal(true);

  async restoreSession(): Promise<void> {
    try {
      const res = await this.http
        .post<AuthResponse>(`${environment.backendUrl}/auth/refresh`, { withCredentials: true })
        .toPromise();

      if (res?.token) {
        localStorage.setItem('access_token', res.token);

        this.store.dispatch(
          setUser({
            user: {
              token: res.token,
              id: res.id,
              username: res.username,
              avatar: res.avatar,
              role: res.role,
            },
          }),
        );
      } else {
        this.store.dispatch(clearUser());
        localStorage.removeItem('access_token');
      }
    } catch (err) {
      console.warn('Session restore failed:', err);
      this.store.dispatch(clearUser());
      localStorage.removeItem('access_token');
    } finally {
      this.loading.set(false);
    }
  }
}
