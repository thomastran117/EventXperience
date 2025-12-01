import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { updateAccessToken, clearUser } from '../../stores/user.actions';
import { UserState } from '../../stores/user.reducer';

@Injectable({ providedIn: 'root' })
export class AuthTokenService {
  accessToken: string | null = null;

  constructor(
    private http: HttpClient,
    private store: Store<{ user: UserState }>,
  ) {
    this.store
      .select((state) => state.user.user?.token)
      .subscribe((token) => {
        this.accessToken = token || null;
      });
  }

  async refreshAccessToken(): Promise<void> {
    const res = await firstValueFrom(
      this.http.post<{ accessToken: string }>(
        `${environment.backendUrl}/auth/refresh`,
        {},
        { withCredentials: true },
      ),
    );
    this.store.dispatch(updateAccessToken({ accessToken: res.accessToken }));
    this.accessToken = res.accessToken;
  }

  logout() {
    this.store.dispatch(clearUser());
    this.accessToken = null;
  }
}
