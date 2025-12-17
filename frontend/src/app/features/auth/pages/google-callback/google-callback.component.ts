import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthService, AuthResponse } from '../../services/auth.service';
import { setUser } from '../../../../core/stores/user.actions';
import { UserState } from '../../../../core/stores/user.reducer';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './google-callback.component.html',
  styleUrls: ['./google-callback.component.css'],
})
export class GoogleCallbackComponent implements OnInit {
  status = signal<'loading' | 'success' | 'error'>('loading');
  message = signal('Completing Google sign-in…');

  constructor(
    private auth: AuthService,
    private store: Store<{ user: UserState }>,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.handleGoogleCallback();
  }

  private handleGoogleCallback(): void {
    // Extract id_token from the hash fragment (#id_token=...)
    const hashParams = new URLSearchParams(window.location.hash.substring(1));
    const idToken = hashParams.get('id_token');

    if (!idToken) {
      this.status.set('error');
      this.message.set('Error: No ID token received from Google.');
      return;
    }

    this.status.set('loading');
    this.message.set('Verifying Google token…');

    this.auth.googleVerify(idToken).subscribe({
      next: (res: AuthResponse) => {
        this.store.dispatch(setUser({ user: res }));

        this.status.set('success');
        this.message.set('Login successful! Redirecting…');

        setTimeout(() => this.router.navigate(['/dashboard']), 1500);
      },
      error: (err) => {
        console.error('Google callback failed:', err);
        this.status.set('error');
        this.message.set(err?.error?.message || 'Google sign-in failed. Please try again.');
      },
    });
  }

  retry(): void {
    this.status.set('loading');
    this.message.set('Retrying Google sign-in…');
    this.handleGoogleCallback();
  }
}
