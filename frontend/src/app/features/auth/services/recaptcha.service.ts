import { Injectable } from '@angular/core';
import { RecaptchaLoaderService } from './recaptcha-loader.service';

declare global {
  interface Window {
    grecaptcha?: any;
  }
}

@Injectable({ providedIn: 'root' })
export class RecaptchaV3Service {
  constructor(private loader: RecaptchaLoaderService) {}

  async execute(siteKey: string, action: string): Promise<string> {
    await this.loader.load(siteKey);

    const g = window.grecaptcha;
    if (!g) throw new Error('grecaptcha missing after script load.');

    await new Promise<void>((resolve) => g.ready(resolve));
    const token = await g.execute(siteKey, { action });

    if (!token) throw new Error('Empty reCAPTCHA token.');
    return token;
  }
}
