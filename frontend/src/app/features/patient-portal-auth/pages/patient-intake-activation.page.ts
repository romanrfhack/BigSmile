import { Location } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PatientPortalCardComponent } from '../components/patient-portal-card.component';
import { PatientIntakeAuthFacade } from '../facades/patient-intake-auth.facade';
import { PatientPortalSessionStore } from '../services/patient-portal-session.store';
import {
  extractActivationToken,
  passwordsMatchValidator
} from './patient-portal-activation.page';

@Component({
  selector: 'app-patient-intake-activation-page',
  standalone: true,
  imports: [ReactiveFormsModule, PatientPortalCardComponent],
  template: `
    <app-patient-portal-card
      eyebrow="Sala de espera"
      title="Activa tu acceso privado"
      description="Crea las credenciales que usarás para completar la información solicitada por la clínica.">

      @if (facade.current(); as current) {
        <section class="patient-success" aria-live="polite">
          <span class="patient-success__icon" aria-hidden="true">✓</span>
          <div>
            <h2>Acceso activado</h2>
            <p>
              Tu borrador privado fue creado para <strong>{{ current.tenantSubdomain }}</strong>.
              Conserva el usuario <strong>{{ current.loginName }}</strong> y tu contraseña.
            </p>
            <p>
              La captura completa se habilitará en el siguiente paso del flujo. No se ha creado ni
              modificado un expediente clínico canónico.
            </p>
          </div>
          <button
            type="button"
            class="patient-button patient-button--secondary"
            [disabled]="facade.loading()"
            (click)="logout()">
            {{ facade.loading() ? 'Cerrando…' : 'Cerrar sesión' }}
          </button>
        </section>
      } @else if (!hasActivationToken()) {
        <div class="patient-alert patient-alert--error" role="alert">
          Este enlace no es válido o ya no está disponible. Solicita a recepción una credencial nueva.
        </div>
      } @else {
        @if (facade.error()) {
          <div class="patient-alert patient-alert--error" role="alert" aria-live="polite">
            {{ facade.error() }}
          </div>
        }

        <form class="patient-form" [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <div class="patient-form__field">
            <label for="intake-login-name">Usuario</label>
            <input
              id="intake-login-name"
              type="text"
              formControlName="loginName"
              autocomplete="username"
              autocapitalize="none"
              spellcheck="false"
              maxlength="200" />
            <p class="patient-form__hint">
              Puede ser tu correo o un nombre de usuario fácil de recordar. Solo es único dentro de esta clínica.
            </p>
            @if (form.controls.loginName.touched && form.controls.loginName.hasError('required')) {
              <p class="patient-form__error">El usuario es obligatorio.</p>
            }
            @if (form.controls.loginName.touched && form.controls.loginName.hasError('minlength')) {
              <p class="patient-form__error">El usuario debe tener al menos 3 caracteres.</p>
            }
          </div>

          <div class="patient-form__field">
            <label for="intake-password">Contraseña</label>
            <input
              id="intake-password"
              type="password"
              formControlName="password"
              autocomplete="new-password"
              minlength="12"
              maxlength="128" />
            <p class="patient-form__hint">Usa al menos 12 caracteres.</p>
            @if (form.controls.password.touched && form.controls.password.hasError('required')) {
              <p class="patient-form__error">La contraseña es obligatoria.</p>
            }
            @if (form.controls.password.touched && form.controls.password.hasError('minlength')) {
              <p class="patient-form__error">La contraseña debe tener al menos 12 caracteres.</p>
            }
          </div>

          <div class="patient-form__field">
            <label for="intake-confirm-password">Confirmar contraseña</label>
            <input
              id="intake-confirm-password"
              type="password"
              formControlName="confirmPassword"
              autocomplete="new-password"
              minlength="12"
              maxlength="128" />
            @if (form.controls.confirmPassword.touched && form.controls.confirmPassword.hasError('required')) {
              <p class="patient-form__error">Confirma la contraseña.</p>
            }
            @if (form.touched && form.hasError('passwordMismatch')) {
              <p class="patient-form__error">Las contraseñas no coinciden.</p>
            }
          </div>

          <div class="patient-form__actions">
            <button
              class="patient-button patient-button--primary"
              type="submit"
              [disabled]="form.invalid || facade.loading()">
              {{ facade.loading() ? 'Activando…' : 'Activar acceso' }}
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

    .patient-success .patient-button {
      grid-column: 2;
      justify-self: start;
    }

    @media (max-width: 560px) {
      .patient-success {
        grid-template-columns: 1fr;
      }

      .patient-success .patient-button {
        grid-column: 1;
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
  private readonly linkedPatientSessionStore = inject(PatientPortalSessionStore);
  readonly facade = inject(PatientIntakeAuthFacade);

  private activationToken = '';
  readonly hasActivationToken = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    loginName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    password: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]]
  }, { validators: passwordsMatchValidator });

  ngOnInit(): void {
    this.linkedPatientSessionStore.clear();
    this.facade.clearSession();
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

  logout(): void {
    this.facade.logout().subscribe(() => {
      this.hasActivationToken.set(false);
    });
  }
}
