import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientPortalCardComponent } from '../../patient-portal-auth/components/patient-portal-card.component';
import { normalizeTenantRealm } from '../../patient-portal-auth/guards/patient-portal-auth.guard';
import { PatientIntakeDemographicsFormComponent } from '../components/patient-intake-demographics-form.component';
import { PatientIntakeWorkspaceFacade } from '../facades/patient-intake-workspace.facade';

@Component({
  selector: 'app-patient-intake-workspace-page',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    TranslatePipe,
    PatientPortalCardComponent,
    PatientIntakeDemographicsFormComponent
  ],
  providers: [PatientIntakeWorkspaceFacade],
  template: `
    <app-patient-portal-card
      eyebrow="Patient information"
      title="Your private intake"
      description="Review and save the information requested by your clinic.">

      @if (facade.status() === 'loading') {
        <div class="patient-alert patient-alert--info" aria-live="polite">
          {{ 'Loading your intake...' | t }}
        </div>
      }

      @if (facade.error()) {
        <div class="patient-alert patient-alert--error" role="alert" aria-live="polite">
          {{ facade.error() | t }}
        </div>
      }

      @if (facade.status() === 'unauthorized') {
        <div class="patient-alert patient-alert--error" role="alert">
          {{ 'Your intake session is not available.' | t }}
        </div>
        <div class="patient-form__actions">
          <a
            class="patient-button patient-button--secondary"
            [routerLink]="['/patient-portal', tenantRealm(), 'login']">
            {{ 'Existing patient sign in' | t }}
          </a>
          <a
            class="patient-button patient-button--primary"
            [routerLink]="['/patient-portal', tenantRealm(), 'intake-login']">
            {{ 'Waiting-room sign in' | t }}
          </a>
        </div>
      }

      @if (facade.status() === 'missing') {
        <div class="patient-alert patient-alert--info">
          {{ (facade.canCreate()
            ? 'No intake draft exists yet. Create it explicitly to begin.'
            : 'The intake draft assigned to this waiting-room account is not available.') | t }}
        </div>

        @if (facade.canCreate()) {
          <div class="patient-form__actions">
            <button
              type="button"
              class="patient-button patient-button--primary"
              [disabled]="facade.creating()"
              (click)="facade.createDraft()">
              {{ (facade.creating() ? 'Creating draft...' : 'Create intake draft') | t }}
            </button>
          </div>
        }
      }

      @if (facade.status() === 'error') {
        <div class="patient-form__actions">
          <button type="button" class="patient-button patient-button--secondary" (click)="facade.reload()">
            {{ 'Retry' | t }}
          </button>
        </div>
      }

      @if (facade.status() === 'ready' && facade.intake(); as intake) {
        <section class="intake-boundary" aria-live="polite">
          <dl>
            <div>
              <dt>{{ 'Access mode' | t }}</dt>
              <dd>{{ (facade.mode() === 'patient' ? 'Existing patient' : 'Waiting-room patient') | t }}</dd>
            </div>
            <div>
              <dt>{{ 'Draft status' | t }}</dt>
              <dd>{{ intake.status }}</dd>
            </div>
            <div>
              <dt>{{ 'Current revision' | t }}</dt>
              <dd>{{ intake.currentRevisionNumber }}</dd>
            </div>
            <div>
              <dt>{{ 'Expires' | t }}</dt>
              <dd>{{ intake.expiresAtUtc | date:'medium' }}</dd>
            </div>
          </dl>

          <div class="patient-alert patient-alert--info">
            {{ 'This information remains in your private draft until the clinic reviews it. Medical history questions are not editable in this step.' | t }}
          </div>

          <app-patient-intake-demographics-form
            [intake]="intake"
            [saving]="facade.saving()"
            [saveOutcome]="facade.saveOutcome()"
            [saveError]="facade.saveError()"
            (saveRequested)="facade.saveNonMedicalDraft($event)">
          </app-patient-intake-demographics-form>
        </section>
      }

      @if (facade.status() !== 'loading' && facade.status() !== 'unauthorized') {
        <div class="patient-form__actions patient-form__actions--session">
          <button
            type="button"
            class="patient-button patient-button--secondary"
            [disabled]="facade.saving()"
            (click)="logout()">
            {{ 'End session' | t }}
          </button>
        </div>
      }
    </app-patient-portal-card>
  `,
  styleUrl: '../../patient-portal-auth/styles/patient-portal-auth-page.scss',
  styles: [`
    .intake-boundary,
    .intake-boundary dl {
      display: grid;
      gap: 1rem;
    }

    .intake-boundary dl {
      grid-template-columns: repeat(2, minmax(0, 1fr));
      margin: 0;
    }

    .intake-boundary dl div {
      padding: 0.8rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-surface);
    }

    .intake-boundary dt {
      color: var(--bsm-color-text-muted);
      font-size: 0.75rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .intake-boundary dd {
      margin: 0.25rem 0 0;
      color: var(--bsm-color-text-brand);
      font-weight: 800;
    }

    .patient-form__actions--session {
      margin-top: 1rem;
    }

    @media (max-width: 560px) {
      .intake-boundary dl {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class PatientIntakeWorkspacePageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly facade = inject(PatientIntakeWorkspaceFacade);
  readonly tenantRealm = signal('');

  ngOnInit(): void {
    const realm = normalizeTenantRealm(this.route.snapshot.paramMap.get('tenantSubdomain'));
    this.tenantRealm.set(realm);
    this.facade.initialize(realm);
  }

  logout(): void {
    const mode = this.facade.mode();
    const target = mode === 'patient_intake' ? 'intake-login' : 'login';

    this.facade.logout().subscribe(() => {
      void this.router.navigate(
        ['/patient-portal', this.tenantRealm(), target],
        { replaceUrl: true }
      );
    });
  }
}
