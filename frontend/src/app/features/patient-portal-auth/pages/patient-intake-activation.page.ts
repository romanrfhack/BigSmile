import { Location } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientPortalCardComponent } from '../components/patient-portal-card.component';
import { PatientIntakeAuthFacade } from '../facades/patient-intake-auth.facade';
import { PatientPortalSessionBoundary } from '../services/patient-portal-session-boundary.service';
import {
  extractActivationToken,
  passwordsMatchValidator
} from './patient-portal-activation.page';

@Component({
  selector: 'app-patient-intake-activation-page',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, PatientPortalCardComponent],
  template: `
    <app-patient-portal-card
      eyebrow="Waiting room"
      title="Activate your private access"
      description="Create the credentials you will use to complete the information requested by the clinic.">

      @if (facade.current(); as current) {
        <section class="patient-success" aria-live="polite">
          <span class="patient-success__icon" aria-hidden="true">✓</span>
          <div>
            <h2>{{ 'Access activated' | t }}</h2>
            <p>
              {{ 'Your private draft was created for {tenantSubdomain}.' | t:{ tenantSubdomain: current.tenantSubdomain } }}
              {{ 'Keep the login name {loginName} and your password.' | t:{ loginName: current.loginName } }}
            </p>
            <p>
              {{ 'The intake workspace is ready. No canonical clinical record was created or changed.' | t }}
            </p>
          </div>
          <div class="patient-success__actions">
            <button
              type="button"
              class="patient-button patient-button--primary"
              [disabled]="facade.loading()"
              (click)="continueToIntake(current.tenantSubdomain)">
              {{ 'Continue to intake' | t }}
            </button>
            <button
              type="button"
              class="patient-button patient-button--secondary"
              [disabled]="facade.loading()"
              (click)="logout()">
              {{ (facade.loading() ? 'Closing...' : 'Close session') | t }}
            </button>
          </div>
        </section>
      } @else if (!hasActivationToken()) {
        <div class="patient-alert patient-alert--error" role="alert">
          {{ 'This link is invalid or is no longer available. Ask reception for a new credential.' | t }}
        </div>
      } @else {
        @if (facade.error()) {
          <div class="patient-alert patient-alert--error" role="alert" aria-live="polite">
            {{ facade.error() | t }}
          </div>
        }

        <form class="patient-form" [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <div class="patient-form__field">
            <label for="intake-login-name">{{ 'Login name' | t }}</label>
            <input
              id="intake-login-name"
              type="text"
              formControlName="loginName"
              autocomplete="username"
              autocapitalize="none"
              spellcheck="false"
              maxlength="200" />
            <p class="patient-form__hint">
              {{ 'Use an email address or a username you can remember. It is unique only inside this clinic.' | t }}
            </p>
            @if (form.controls.loginName.touched && form.controls.loginName.hasError('required')) {
              <p class="patient-form__error">{{ 'Login name is required.' | t }}</p>
            }
            @if (form.controls.loginName.touched && form.controls.loginName.hasError('minlength')) {
              <p class="patient-form__error">{{ 'Login name must contain at least 3 characters.' | t }}</p>
            }
          </div>

          <div class="patient-form__field">
            <label for="intake-password">{{ 'Password' | t }}</label>
            <input
              id="intake-password"
              type="password"
              formControlName="password"
              autocomplete="new-password"
              minlength="12"
              maxlength="128" />
            <p class="patient-form__hint">{{ 'Use at least 12 characters.' | t }}</p>
            @if (form.controls.password.touched && form.controls.password.hasError('required')) {
              <p class="patient-form__error">{{ 'Password is required.' | t }}</p>
            }
            @if (form.controls.password.touched && form.controls.password.hasError('minlength')) {
              <p class="patient-form__error">{{ 'Password must contain at least 12 characters.' | t }}</p>
            }
          </div>

          <div class="patient-form__field">
            <label for="intake-confirm-password">{{ 'Confirm password' | t }}</label>
            <input
              id="intake-confirm-password"
              type="password"
              formControlName="confirmPassword"
              autocomplete="new-password"
              minlength="12"
              maxlength="128" />
            @if (form.controls.confirmPassword.touched && form.controls.confirmPassword.hasError('required')) {
              <p class="patient-form__error">{{ 'Password confirmation is required.' | t }}</p>
            }
            @if (form.touched && form.hasError('passwordMismatch')) {
              <p class="patient-form__error">{{ 'Passwords do not match.' | t }}</p>
            }
          </div>

          <div class="patient-form__actions">
            <button
              class="patient-button patient-button--primary"
              type="submit"
              [disabled]="form.invalid || facade.loading()">
              {{ (facade.loading() ? 'Activating access...' : 'Activate access') | t }}
            </button>
          </div>
        </form>
      }
    </app-patient-portal-card>
  `,
  styleUrl: '../styles/patient-portal-auth-page.scss',
  styles: [`
    .patient-success {
      display: grid;
      grid-template-columns: auto minmax(0, 1fr);
      gap: 1rem;
      align-items: start;
    }

    .patient-success__icon {
      display: grid;
      place-items: center;
      width: 2.5rem;
      height: 2.5rem;
      border-radius: 50%;
      background: var(--bsm-color-success-soft);
      color: var(--bsm-color-success-text);
      font-size: 1.35rem;
      font-weight: 900;
    }

    .patient-success h2,
    .patient-success p {
      margin: 0;
    }

    .patient-success h2 {
      color: var(--bsm-color-text-brand);
    }

    .patient-success p {
      margin-top: 0.5rem;
      color: var(--bsm-color-text-muted);
      line-height: 1.55;
    }

    .patient-success__actions {
      grid-column: 2;
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    @media (max-width: 560px) {
      .patient-success {
        grid-template-columns: 1fr;
      }

      .patient-success__actions {
        grid-column: 1;
      }

      .patient-success__actions .patient-button {
        width: 100%;
      }
    }
  `]
})
export class PatientIntakeActivationPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly location = inject(Location);
  private readonly sessionBoundary = inject(PatientPortalSessionBoundary);
  readonly facade = inject(PatientIntakeAuthFacade);

  private activationToken = '';
  readonly hasActivationToken = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    loginName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    password: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]]
  }, { validators: passwordsMatchValidator });

  ngOnInit(): void {
    this.sessionBoundary.clearAll();
    this.activationToken = extractActivationToken(this.route.snapshot.fragment);
    this.hasActivationToken.set(this.activationToken.length > 0 && this.activationToken.length <= 256);

    if (this.route.snapshot.fragment) {
      this.location.replaceState(this.router.url.split('#')[0]);
    }
  }

  submit(): void {
    this.facade.clearError();
    this.form.markAllAsTouched();

    if (!this.hasActivationToken() || this.form.invalid || this.facade.loading()) {
      return;
    }

    const value = this.form.getRawValue();
    this.facade.activate({
      accessToken: this.activationToken,
      loginName: value.loginName,
      password: value.password
    }).subscribe({
      next: () => {
        this.activationToken = '';
        this.hasActivationToken.set(false);
        this.form.reset();
      },
      error: () => {
        // The facade exposes a bounded, non-enumerating message.
      }
    });
  }

  continueToIntake(tenantSubdomain: string): void {
    void this.router.navigate(
      ['/patient-portal', tenantSubdomain, 'intake'],
      { replaceUrl: true }
    );
  }

  logout(): void {
    this.facade.logout().subscribe(() => {
      this.hasActivationToken.set(false);
    });
  }
}
