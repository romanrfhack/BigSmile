import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { LanguageSelectorComponent, TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-patient-portal-shell',
  standalone: true,
  imports: [RouterOutlet, LanguageSelectorComponent, TranslatePipe],
  template: `
    <section class="patient-shell" aria-labelledby="patient-shell-title">
      <header class="patient-shell__header">
        <div class="patient-shell__brand">
          <img src="brand/logoBS.webp" alt="BigSmile" width="220" height="77" />
          <div>
            <p class="patient-shell__eyebrow">{{ 'Patient portal' | t }}</p>
            <h1 id="patient-shell-title">{{ 'Secure patient access' | t }}</h1>
          </div>
        </div>

        <app-language-selector />
      </header>

      <main class="patient-shell__content">
        <router-outlet />
      </main>

      <footer class="patient-shell__footer">
        <p>{{ 'Your access is private and limited to your own patient workflow.' | t }}</p>
      </footer>
    </section>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
      min-height: 100dvh;
    }

    .patient-shell {
      min-height: 100vh;
      min-height: 100dvh;
      display: grid;
      grid-template-rows: auto 1fr auto;
      background:
        radial-gradient(circle at top right, var(--bsm-color-primary-soft), transparent 38%),
        linear-gradient(145deg, var(--bsm-color-bg), var(--bsm-color-accent-soft));
      color: var(--bsm-color-text);
    }

    .patient-shell__header,
    .patient-shell__footer {
      width: min(100% - 2rem, 1120px);
      margin-inline: auto;
    }

    .patient-shell__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      padding-block: 1rem;
    }

    .patient-shell__brand {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .patient-shell__brand img {
      width: min(190px, 42vw);
      height: auto;
    }

    .patient-shell__eyebrow {
      margin: 0 0 0.2rem;
      color: var(--bsm-color-accent-dark);
      font-weight: 800;
      font-size: 0.78rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    h1 {
      margin: 0;
      color: var(--bsm-color-text-brand);
      font-size: clamp(1.1rem, 3vw, 1.45rem);
      line-height: 1.15;
    }

    .patient-shell__content {
      display: grid;
      place-items: center;
      width: min(100% - 2rem, 1120px);
      margin-inline: auto;
      padding-block: clamp(1rem, 4vw, 3rem);
    }

    .patient-shell__footer {
      padding-block: 1rem 1.5rem;
      color: var(--bsm-color-text-muted);
      text-align: center;
      font-size: 0.875rem;
    }

    .patient-shell__footer p {
      margin: 0;
    }

    @media (max-width: 680px) {
      .patient-shell__header {
        align-items: flex-start;
      }

      .patient-shell__brand {
        align-items: flex-start;
        flex-direction: column;
        gap: 0.4rem;
      }

      .patient-shell__brand img {
        width: 150px;
      }
    }
  `]
})
export class PatientPortalShellComponent implements OnInit {
  private readonly staffAuthService = inject(AuthService);

  ngOnInit(): void {
    // A patient-facing browser surface must never inherit an in-memory staff session.
    this.staffAuthService.logout();
  }
}
