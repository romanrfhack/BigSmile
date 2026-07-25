import { Location } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientPortalCardComponent } from '../components/patient-portal-card.component';
import { PatientPortalAuthFacade } from '../facades/patient-portal-auth.facade';

@Component({
  selector: 'app-patient-portal-activation-page',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, PatientPortalCardComponent],
  template: `
    <app-patient-portal-card
      eyebrow="Patient portal"
      title="Activate your patient access"
      description="Create the private credentials you will use to return and complete information requested by your clinic.">

      @if (!hasActivationToken()) {
        <div class="patient-alert patient-alert--error" role="alert">
          {{ 'This activation link is not valid or is no longer available.' | t }}
          {{ 'Contact the clinic so authorized staff can issue a new invitation.' | t }}
        </div>
      } @else {
        @if (facade.error()) {
          <div class="patient-alert patient-alert--error" role="alert" aria-live="polite">
            {{ facade.error() | t }}
          </div>
        }

        <form class="patient-form" [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <div class="patient-form__field">
            <label for="patient-login-name">{{ 'Login name' | t }}</label>
            <input
              id="patient-login-name"
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
            <label for="patient-password">{{ 'Password' | t }}</label>
            <input
              id="patient-password"
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
            <label for="patient-confirm-password">{{ 'Confirm password' | t }}</label>
            <input
              id="patient-confirm-password"
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
  styleUrl: '../styles/patient-portal-auth-page.scss'
})
export class PatientPortalActivationPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly location = inject(Location);
  readonly facade = inject(PatientPortalAuthFacade);

  private activationToken = '';
  readonly hasActivationToken = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    loginName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    password: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]]
  }, { validators: passwordsMatchValidator });

  ngOnInit(): void {
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
      activationToken: this.activationToken,
      loginName: value.loginName,
      password: value.password
    }).subscribe({
      next: response => {
        void this.router.navigate(
          ['/patient-portal', response.current.tenantSubdomain, 'home'],
          { replaceUrl: true }
        );
      },
      error: () => {
        // The facade intentionally exposes only a generic, non-enumerating message.
      }
    });
  }
}

export function extractActivationToken(fragment: string | null | undefined): string {
  const normalizedFragment = (fragment ?? '').trim();
  if (!normalizedFragment) {
    return '';
  }

  const tokenFromParams = new URLSearchParams(normalizedFragment).get('token');
  if (tokenFromParams) {
    return tokenFromParams.trim();
  }

  try {
    return decodeURIComponent(normalizedFragment).trim();
  } catch {
    return '';
  }
}

export function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value as string | undefined;
  const confirmation = control.get('confirmPassword')?.value as string | undefined;

  if (!password || !confirmation) {
    return null;
  }

  return password === confirmation ? null : { passwordMismatch: true };
}
