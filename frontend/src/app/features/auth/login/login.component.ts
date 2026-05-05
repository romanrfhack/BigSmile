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
    <div class="login-container">
      <h2>{{ 'Sign in to Bigsmile' | t }}</h2>
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
            placeholder="admin@bigsmile.local"
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
            placeholder="••••••"
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
  `,
  styles: [`
    .login-container {
      max-width: 400px;
      margin: 2rem auto;
      padding: 2rem;
      border: 1px solid #ddd;
      border-radius: 8px;
      background: #fff;
    }
    .form-group {
      margin-bottom: 1rem;
    }
    label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
    }
    .form-control {
      width: 100%;
      padding: 0.5rem;
      border: 1px solid #ccc;
      border-radius: 4px;
      box-sizing: border-box;
    }
    .error {
      color: #dc3545;
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }
    .error-message {
      margin-top: 1rem;
      padding: 0.75rem;
      background-color: #f8d7da;
      border: 1px solid #f5c6cb;
      border-radius: 4px;
      color: #721c24;
    }
    .btn {
      width: 100%;
      padding: 0.75rem;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      cursor: pointer;
      margin-top: 1rem;
    }
    .btn-primary {
      background-color: #007bff;
      color: white;
    }
    .btn-primary:disabled {
      background-color: #6c757d;
      cursor: not-allowed;
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
