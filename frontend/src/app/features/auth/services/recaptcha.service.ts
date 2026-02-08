import { Injectable } from '@angular/core';

declare global {
  interface Window {
    grecaptcha?: any;
  }
}

@Injectable({ providedIn: 'root' })
export class RecaptchaV3Service {
  private readyPromise: Promise<any> | null = null;

  private waitForGrecaptcha(): Promise<any> {
    if (this.readyPromise) return this.readyPromise;

    this.readyPromise = new Promise((resolve, reject) => {
      const start = Date.now();
      const timeoutMs = 10_000;

      const tick = () => {
        const g = window.grecaptcha;
        if (g && typeof g.ready === 'function' && typeof g.execute === 'function') {
          g.ready(() => resolve(g));
          return;
        }
        if (Date.now() - start > timeoutMs) {
          reject(new Error('reCAPTCHA v3 did not load in time.'));
          return;
        }
        setTimeout(tick, 50);
      };

      tick();
    });

    return this.readyPromise;
  }

  async execute(siteKey: string, action: string): Promise<string> {
    const g = await this.waitForGrecaptcha();
    const token = await g.execute(siteKey, { action });
    if (!token) throw new Error('Empty reCAPTCHA token.');
    return token;
  }
}
