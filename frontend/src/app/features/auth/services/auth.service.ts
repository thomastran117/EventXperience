import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
  remember: boolean;
}

export interface SignupRequest {
  email: string;
  password: string;
  role: 'customer' | 'owner';
}

export interface AuthResponse {
  username: string;
  token: string;
  avatar: string;
  role: string;
  id: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.backendUrl}/auth`;

  constructor(private http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, payload, {
      withCredentials: true,
    });
  }

  signup(payload: SignupRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/signup`, payload, {
      withCredentials: true,
    });
  }

  verifyEmail(token: string): Observable<{ message: string }> {
    return this.http.get<{ message: string }>(`${this.baseUrl}/verify/${token}`, {
      withCredentials: true,
    });
  }

  googleVerify(idToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.baseUrl}/google`,
      { id_token: idToken },
      { withCredentials: true },
    );
  }

  microsoftVerify(idToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.baseUrl}/microsoft`,
      { id_token: idToken },
      { withCredentials: true },
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {}, { withCredentials: true });
  }

  get googleOAuthUrl(): string {
    return `${this.baseUrl}/login/google`;
  }

  get microsoftOAuthUrl(): string {
    return `${this.baseUrl}/login/microsoft`;
  }
}
