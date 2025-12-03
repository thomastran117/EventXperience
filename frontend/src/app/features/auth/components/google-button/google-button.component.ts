import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../../environments/environment';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-google-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './google-button.component.html',
  styleUrls: ['./google-button.component.css'],
})
export class GoogleButtonComponent {
  loading = false;

  constructor(private auth: AuthService) {}

  loginWithGoogle(): void {
    try {
      this.loading = true;

      const scope = 'openid email profile';
      const params = new URLSearchParams({
        client_id: environment.googleClientId,
        redirect_uri: `${environment.frontendUrl}/auth/google`,
        response_type: 'id_token',
        scope,
        nonce: crypto.randomUUID(),
        prompt: 'select_account',
      });

      const googleAuthUrl = `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`;
      window.location.href = googleAuthUrl;
    } catch (error) {
      console.error('Failed to start Google OAuth flow:', error);
    } finally {
      this.loading = false;
    }
  }
}
