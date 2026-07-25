import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientPortalCardComponent } from '../components/patient-portal-card.component';
import { PatientPortalAuthFacade } from '../facades/patient-portal-auth.facade';
import { normalizeTenantRealm } from '../guards/patient-portal-auth.guard';

@Component({
  selector: 'app-patient-portal-home-page',
  standalone: true,
  imports: [TranslatePipe, PatientPortalCardComponent],
  template: `
    <app-patient-portal-card
      eyebrow="Patient portal"
      title="Your patient access is ready"
      description="This secure session is limited to your own patient workflow.">

      @if (facade.error()) {
        <div class="patient-alert patient-alert--error" role="alert" aria-live="polite">
          {{ facade.error() | t }}
        </div>
      }

      @if (facade.current(); as current) {
        <div class="patient-session">
          <dl class="patient-session__summary">
            <dt>{{ 'Clinic access' | t }}</dt>
            <dd>{{ current.tenantSubdomain }}</dd>
            <dt>{{ 'Login name' | t }}</dt>
            <dd>{{ current.loginName }}</dd>
          </dl>

          <div class="patient-alert patient-alert--info">
            {{ 'The medical information form will be available in the next implementation step. No clinical data can be changed from this screen.' | t }}
          </div>

          <button
            class="patient-button patient-button--secondary"
            type="button"
            [disabled]="facade.loading()"
            (click)="logout()">
            {{ (facade.loading() ? 'Ending session...' : 'End session') | t }}
          </button>
        </div>
      } @else if (facade.loading()) {
        <div class="patient-alert patient-alert--info" aria-live="polite">
          {{ 'Refreshing patient session...' | t }}
        </div>
      }
    </app-patient-portal-card>
  `,
  styleUrl: '../styles/patient-portal-auth-page.scss'
})
export class PatientPortalHomePageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly facade = inject(PatientPortalAuthFacade);

  ngOnInit(): void {
    this.facade.refreshCurrent().subscribe({
      error: () => this.navigateToLogin()
    });
  }

  logout(): void {
    if (this.facade.loading()) {
      return;
    }

    this.facade.logout().subscribe({
      next: () => this.navigateToLogin()
    });
  }

  private navigateToLogin(): void {
    const currentRealm = this.facade.current()?.tenantSubdomain;
    const routeRealm = this.route.snapshot.paramMap.get('tenantSubdomain');
    const tenantRealm = normalizeTenantRealm(currentRealm ?? routeRealm);

    void this.router.navigate(
      ['/patient-portal', tenantRealm, 'login'],
      { replaceUrl: true }
    );
  }
}
