import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-microsoft-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './microsoft-button.component.html',
  styleUrls: ['./microsoft-button.component.css'],
})
export class MicrosoftButtonComponent {
  async loginWithMicrosoft() {
    const codeVerifier = crypto.randomUUID() + crypto.randomUUID();
    const codeChallenge = await this.generateCodeChallenge(codeVerifier);
    sessionStorage.setItem('ms_code_verifier', codeVerifier);

    const authUrl =
      'https://login.microsoftonline.com/common/oauth2/v2.0/authorize' +
      `?client_id=${environment.msalClientId}` +
      `&response_type=code` +
      `&redirect_uri=${encodeURIComponent(`${environment.frontendUrl}/auth/microsoft`)}` +
      `&response_mode=query` +
      `&scope=openid profile email offline_access` +
      `&code_challenge=${codeChallenge}` +
      `&code_challenge_method=S256`;

    window.location.href = authUrl;
  }

  private async generateCodeChallenge(verifier: string): Promise<string> {
    const encoder = new TextEncoder();
    const data = encoder.encode(verifier);
    const digest = await crypto.subtle.digest('SHA-256', data);
    return this.base64URLEncode(digest);
  }

  private base64URLEncode(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let str = '';
    for (const b of bytes) str += String.fromCharCode(b);
    return btoa(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
  }
}
