import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { updateAccessToken, clearUser } from '../../stores/user.actions';
import { UserState } from '../../stores/user.reducer';

type CsrfResponse = { token: string };
type RefreshResponse = { accessToken: string };

@Injectable({ providedIn: 'root' })
export class AuthTokenService {
  accessToken: string | null = null;
  csrfToken: string | null = null;

  private csrfPromise: Promise<void> | null = null;

  constructor(
    private http: HttpClient,
    private store: Store<{ user: UserState }>,
  ) {
    this.store
      .select((state) => state.user.user?.Token)
      .subscribe((token) => {
        this.accessToken = token || null;
      });
  }

  async ensureCsrfToken(): Promise<void> {
    if (this.csrfToken) return;

    if (!this.csrfPromise) {
      this.csrfPromise = (async () => {
        const res = await firstValueFrom(
          this.http.get<CsrfResponse>(`${environment.backendUrl}/auth/csrf`, {
            withCredentials: true,
          }),
        );
        this.csrfToken = res?.token ?? null;
      })().finally(() => {
        this.csrfPromise = null;
      });
    }

    await this.csrfPromise;
  }

  async refreshAccessToken(): Promise<void> {
    await this.ensureCsrfToken();

    const headers = this.csrfToken
      ? new HttpHeaders({ 'X-CSRF-TOKEN': this.csrfToken })
      : undefined;

    const res = await firstValueFrom(
      this.http.post<RefreshResponse>(
        `${environment.backendUrl}/auth/refresh`,
        {},
        {
          withCredentials: true,
          headers,
        },
      ),
    );

    this.store.dispatch(updateAccessToken({ accessToken: res.accessToken }));
    this.accessToken = res.accessToken;
  }

  logoutLocal() {
    this.store.dispatch(clearUser());
    this.accessToken = null;
    this.csrfToken = null;
  }
}
