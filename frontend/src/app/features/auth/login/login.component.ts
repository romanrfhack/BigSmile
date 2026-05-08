import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService, LoginRequest } from '../../../core/auth/auth.service';
import { Router } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  template: `
    <section class="login-page" aria-labelledby="login-title" style="--login-background-image: url('brand/fondo.webp')">
      <div class="login-container">
        <header class="brand-panel">
          <img src="brand/logoBS.webp" alt="BigSmile" class="brand-logo" width="260" height="91" />
          <p class="brand-subtitle">{{ 'Dental Clinic Management Platform' | t }}</p>
        </header>

        <h1 id="login-title">{{ 'Sign in to Bigsmile' | t }}</h1>
        <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
          <div class="form-group">
            <label for="email">{{ 'Email' | t }}</label>
            <input
              type="email"
              id="email"
              name="email"
              [(ngModel)]="credentials.email"
              required
              email
              #email="ngModel"
              class="form-control"
              [placeholder]="'user@clinic.com.mx' | t"
            />
            <div *ngIf="email.invalid && (email.dirty || email.touched)" class="error">
              <div *ngIf="email.errors?.['required']">{{ 'Email is required.' | t }}</div>
              <div *ngIf="email.errors?.['email']">{{ 'Must be a valid email address.' | t }}</div>
            </div>
          </div>

          <div class="form-group">
            <label for="password">{{ 'Password' | t }}</label>
            <input
              type="password"
              id="password"
              name="password"
              [(ngModel)]="credentials.password"
              required
              minlength="6"
              #password="ngModel"
              class="form-control"
            />
            <div *ngIf="password.invalid && (password.dirty || password.touched)" class="error">
              <div *ngIf="password.errors?.['required']">{{ 'Password is required.' | t }}</div>
              <div *ngIf="password.errors?.['minlength']">{{ 'Password must be at least 6 characters.' | t }}</div>
            </div>
          </div>

          <button type="submit" [disabled]="loginForm.invalid || loading" class="btn btn-primary">
            {{ (loading ? 'Signing in...' : 'Sign in') | t }}
          </button>

          <div *ngIf="error" class="error-message">{{ error | t }}</div>
        </form>
      </div>
    </section>
  `,
  styles: [`
    .login-page {
      min-height: 100vh;
      min-height: 100dvh;
      display: grid;
      place-items: center;
      padding: clamp(1.25rem, 4vw, 3rem);
      background-color: var(--bsm-color-surface);
      background-image:
        linear-gradient(
          135deg,
          color-mix(in srgb, var(--bsm-color-bg) 72%, transparent),
          color-mix(in srgb, var(--bsm-color-accent-soft) 62%, transparent) 48%,
          color-mix(in srgb, var(--bsm-color-primary-soft) 56%, transparent)
        ),
        var(--login-background-image);
      background-size: cover;
      background-position: center;
      background-repeat: no-repeat;
    }

    .login-container {
      width: min(100%, 440px);
      position: relative;
      overflow: hidden;
      padding: clamp(1.65rem, 4vw, 2.35rem);
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-lg);
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-md);
    }

    .login-container::before {
      content: '';
      position: absolute;
      inset: 0 0 auto;
      height: 4px;
      background: var(--bsm-gradient-brand);
    }

    .brand-panel {
      display: grid;
      justify-items: center;
      gap: 0.75rem;
      margin-bottom: 1.75rem;
      text-align: center;
    }

    .brand-logo {
      display: block;
      width: min(260px, 78vw);
      height: auto;
    }

    .brand-subtitle {
      max-width: 30ch;
      margin: 0;
      color: var(--bsm-color-text-muted);
      font-weight: 600;
      line-height: 1.45;
    }

    h1 {
      color: var(--bsm-color-text-brand);
      margin: 0 0 1.25rem;
      font-size: clamp(1.55rem, 3vw, 1.85rem);
      line-height: 1.15;
      text-align: center;
    }

    .form-group {
      margin-bottom: 1.05rem;
    }

    label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 700;
      color: var(--bsm-color-text-brand);
    }

    .form-control {
      width: 100%;
      padding: 0.85rem 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      box-sizing: border-box;
      color: var(--bsm-color-text);
      background: var(--bsm-color-bg);
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .form-control:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }

    .error {
      color: var(--bsm-color-danger);
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    .error-message {
      margin-top: 1rem;
      padding: 0.75rem;
      background-color: var(--bsm-color-danger-soft);
      border: 1px solid var(--bsm-color-danger-soft);
      border-radius: var(--bsm-radius-sm);
      color: var(--bsm-color-danger);
    }

    .btn {
      width: 100%;
      padding: 0.9rem 1rem;
      border: none;
      border-radius: var(--bsm-radius-pill);
      font-size: 1rem;
      cursor: pointer;
      margin-top: 1rem;
      font-weight: 700;
    }

    .btn-primary {
      background-color: var(--bsm-color-primary);
      color: var(--bsm-color-bg);
    }

    .btn-primary:not(:disabled):hover {
      background-color: var(--bsm-color-primary-dark);
      box-shadow: var(--bsm-shadow-sm);
    }

    .btn-primary:disabled {
      background-color: var(--bsm-color-text-muted);
      cursor: not-allowed;
    }

    @media (max-width: 520px) {
      .login-page {
        align-items: start;
        padding-top: 1.5rem;
      }

      .login-container {
        border-radius: var(--bsm-radius-md);
      }
    }
  `]
})
export class LoginComponent {
  credentials: LoginRequest = { email: '', password: '' };
  loading = false;
  error = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    if (this.loading) return;
    this.loading = true;
    this.error = '';

    this.authService.login(this.credentials).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/patients']);
      },
      error: (err) => {
        this.loading = false;
        this.error = typeof err.error === 'string'
          ? err.error
          : err.error?.message || 'Login failed. Please check your credentials.';
      }
    });
  }
}
