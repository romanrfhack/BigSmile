import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientPortalCardComponent } from '../components/patient-portal-card.component';
import { PatientIntakeAuthFacade } from '../facades/patient-intake-auth.facade';
import { normalizeTenantRealm } from '../guards/patient-portal-auth.guard';

@Component({
  selector: 'app-patient-intake-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, PatientPortalCardComponent],
  template: `
    <app-patient-portal-card
      eyebrow="Waiting room"
      title="Continue your intake"
      description="Use the private credentials created when you activated the waiting-room link.">

      <div class="patient-alert patient-alert--info">
        <strong>{{ 'Clinic access' | t }}:</strong> {{ tenantRealm() }}
      </div>

      @if (facade.error()) {
        <div class="patient-alert patient-alert--error" role="alert" aria-live="polite">
          {{ facade.error() | t }}
        </div>
      }

      <form class="patient-form" [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <div class="patient-form__field">
          <label for="patient-intake-login-name">{{ 'Login name' | t }}</label>
          <input
            id="patient-intake-login-name"
            type="text"
            formControlName="loginName"
            autocomplete="username"
            autocapitalize="none"
            spellcheck="false"
            maxlength="200" />
          @if (form.controls.loginName.touched && form.controls.loginName.hasError('required')) {
            <p class="patient-form__error">{{ 'Login name is required.' | t }}</p>
          }
        </div>

        <div class="patient-form__field">
          <label for="patient-intake-password">{{ 'Password' | t }}</label>
          <input
            id="patient-intake-password"
            type="password"
            formControlName="password"
            autocomplete="current-password"
            maxlength="128" />
          @if (form.controls.password.touched && form.controls.password.hasError('required')) {
            <p class="patient-form__error">{{ 'Password is required.' | t }}</p>
          }
        </div>

        <div class="patient-form__actions">
          <button
            class="patient-button patient-button--primary"
            type="submit"
            [disabled]="form.invalid || facade.loading() || !tenantRealm()">
            {{ (facade.loading() ? 'Signing in...' : 'Continue intake') | t }}
          </button>
        </div>
      </form>
    </app-patient-portal-card>
  `,
  styleUrl: '../styles/patient-portal-auth-page.scss'
})
export class PatientIntakeLoginPageComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly facade = inject(PatientIntakeAuthFacade);

  readonly tenantRealm = signal('');
  readonly form = this.formBuilder.nonNullable.group({
    loginName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    password: ['', [Validators.required, Validators.maxLength(128)]]
  });

  ngOnInit(): void {
    this.tenantRealm.set(normalizeTenantRealm(this.route.snapshot.paramMap.get('tenantSubdomain')));
  }

  submit(): void {
    this.facade.clearError();
    this.form.markAllAsTouched();

    const tenantRealm = this.tenantRealm();
    if (!tenantRealm || this.form.invalid || this.facade.loading()) {
      return;
    }

    this.facade.login(tenantRealm, this.form.getRawValue()).subscribe({
      next: response => {
        void this.router.navigate(
          ['/patient-portal', response.current.tenantSubdomain, 'intake'],
          { replaceUrl: true }
        );
      },
      error: () => {
        // The facade intentionally exposes only a generic, non-enumerating message.
      }
    });
  }
}
