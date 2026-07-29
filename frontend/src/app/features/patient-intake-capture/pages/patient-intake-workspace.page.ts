import { DatePipe } from '@angular/common';
import { Component, HostListener, OnInit, ViewChild, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { I18nService } from '../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../shared/i18n';
import { PatientPortalCardComponent } from '../../patient-portal-auth/components/patient-portal-card.component';
import { normalizeTenantRealm } from '../../patient-portal-auth/guards/patient-portal-auth.guard';
import { PatientIntakeDemographicsFormComponent } from '../components/patient-intake-demographics-form.component';
import { PatientIntakeMedicalQuestionnaireComponent } from '../components/patient-intake-medical-questionnaire.component';
import { PatientIntakeWorkspaceFacade } from '../facades/patient-intake-workspace.facade';
import { PatientIntakeUnsavedChangesAware } from '../guards/patient-intake-unsaved-changes.guard';
import {
  PatientIntakeMedicalAnswerFormValue,
  PatientIntakeNonMedicalFormValue
} from '../models/patient-intake.models';

@Component({
  selector: 'app-patient-intake-workspace-page',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    TranslatePipe,
    PatientPortalCardComponent,
    PatientIntakeDemographicsFormComponent,
    PatientIntakeMedicalQuestionnaireComponent
  ],
  providers: [PatientIntakeWorkspaceFacade],
  template: `
    <app-patient-portal-card
      eyebrow="Patient information"
      title="Your private intake"
      description="Review and save the information requested by your clinic.">

      @if (facade.status() === 'loading') {
        <div class="patient-alert patient-alert--info" role="status" aria-live="polite">
          {{ 'Loading your intake...' | t }}
        </div>
      }

      @if (facade.error()) {
        <div class="patient-alert patient-alert--error" role="alert" aria-live="assertive">
          {{ facade.error() | t }}
        </div>
      }

      @if (facade.status() === 'unauthorized') {
        <div class="patient-alert patient-alert--error" role="alert" aria-live="assertive">
          {{ (facade.recoveryState() === 'waiting-room-reissue'
            ? 'This waiting-room intake access is no longer available. Ask reception for a new credential.'
            : 'Your intake session is not available. Sign in again to continue.') | t }}
        </div>
        <div class="patient-form__actions">
          @if (facade.recoveryState() === 'waiting-room-reissue') {
            <a
              class="patient-button patient-button--primary"
              [routerLink]="['/patient-portal', tenantRealm(), 'intake-login']">
              {{ 'Waiting-room sign in' | t }}
            </a>
          } @else {
            <a
              class="patient-button patient-button--primary"
              [routerLink]="['/patient-portal', tenantRealm(), 'login']">
              {{ 'Existing patient sign in' | t }}
            </a>
          }
        </div>
      }

      @if (facade.status() === 'missing') {
        <div class="patient-alert patient-alert--info" role="status" aria-live="polite">
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
        <section class="intake-boundary" aria-labelledby="intake-workspace-heading">
          <h2 id="intake-workspace-heading" class="visually-hidden">{{ 'Intake workspace' | t }}</h2>

          <div class="workspace-state" role="status" aria-live="polite">
            @if (facade.saving()) {
              {{ 'Saving your complete draft...' | t }}
            } @else if (hasUnsavedChanges()) {
              {{ 'You have unsaved changes.' | t }}
            } @else {
              {{ 'All visible changes are saved.' | t }}
            }
          </div>

          @if (facade.blockingState() === 'conflict') {
            <div class="patient-alert patient-alert--error lifecycle-alert" role="alert" aria-live="assertive">
              <div>
                <strong>{{ 'A newer version of this intake exists.' | t }}</strong>
                <p>{{ 'Your local edits remain on this screen. Reloading will discard them and load the latest saved version.' | t }}</p>
              </div>
              <button type="button" class="patient-button patient-button--secondary" (click)="reloadLatest()">
                {{ 'Reload latest version' | t }}
              </button>
            </div>
          }

          @if (facade.blockingState() === 'expired') {
            <div class="patient-alert patient-alert--error lifecycle-alert" role="alert" aria-live="assertive">
              <div>
                <strong>{{ 'This intake draft has expired.' | t }}</strong>
                <p>
                  {{ (facade.canReplaceExpired()
                    ? 'Your local edits cannot be saved. Start a new draft to continue.'
                    : 'Your local edits cannot be saved. Ask reception for a new waiting-room credential.') | t }}
                </p>
              </div>
              @if (facade.canReplaceExpired()) {
                <button
                  type="button"
                  class="patient-button patient-button--primary"
                  [disabled]="facade.creating()"
                  (click)="replaceExpiredDraft()">
                  {{ (facade.creating() ? 'Creating draft...' : 'Start a new draft') | t }}
                </button>
              }
            </div>
          }

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
            {{ 'Personal information and medical answers remain in this private draft until the clinic reviews them.' | t }}
          </div>

          <app-patient-intake-demographics-form
            [intake]="intake"
            [saving]="facade.saving()"
            [saveOutcome]="facade.saveTarget() === 'demographics' ? facade.saveOutcome() : null"
            [saveError]="facade.saveTarget() === 'demographics' ? facade.saveError() : null"
            (saveRequested)="saveNonMedical($event)">
          </app-patient-intake-demographics-form>

          <app-patient-intake-medical-questionnaire
            [intake]="intake"
            [saving]="facade.saving()"
            [saveOutcome]="facade.saveTarget() === 'medical' ? facade.saveOutcome() : null"
            [saveError]="facade.saveTarget() === 'medical' ? facade.saveError() : null"
            (saveRequested)="saveMedical($event)">
          </app-patient-intake-medical-questionnaire>
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
    .intake-boundary dl,
    .lifecycle-alert {
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

    .intake-boundary dd,
    .lifecycle-alert p {
      margin: 0.25rem 0 0;
    }

    .intake-boundary dd {
      color: var(--bsm-color-text-brand);
      font-weight: 800;
    }

    .workspace-state {
      padding: 0.75rem 1rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      background: var(--bsm-color-surface);
      color: var(--bsm-color-text-brand);
      font-weight: 750;
    }

    .lifecycle-alert {
      grid-template-columns: minmax(0, 1fr) auto;
      align-items: center;
    }

    .visually-hidden {
      position: absolute;
      inline-size: 1px;
      block-size: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      white-space: nowrap;
      border: 0;
    }

    .patient-form__actions--session {
      margin-top: 1rem;
    }

    @media (max-width: 640px) {
      .intake-boundary dl,
      .lifecycle-alert {
        grid-template-columns: 1fr;
      }

      .lifecycle-alert .patient-button {
        width: 100%;
      }
    }
  `]
})
export class PatientIntakeWorkspacePageComponent implements OnInit, PatientIntakeUnsavedChangesAware {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly i18n = inject(I18nService);
  private allowNavigation = false;

  @ViewChild(PatientIntakeDemographicsFormComponent)
  private demographicsForm?: PatientIntakeDemographicsFormComponent;

  @ViewChild(PatientIntakeMedicalQuestionnaireComponent)
  private medicalQuestionnaire?: PatientIntakeMedicalQuestionnaireComponent;

  readonly facade = inject(PatientIntakeWorkspaceFacade);
  readonly tenantRealm = signal('');

  ngOnInit(): void {
    const realm = normalizeTenantRealm(this.route.snapshot.paramMap.get('tenantSubdomain'));
    this.tenantRealm.set(realm);
    this.facade.initialize(realm);
  }

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (!this.hasUnsavedChanges()) {
      return;
    }

    event.preventDefault();
    event.returnValue = '';
  }

  hasUnsavedChanges(): boolean {
    return !this.facade.saving() && Boolean(
      this.demographicsForm?.form.dirty || this.medicalQuestionnaire?.form.dirty);
  }

  canDeactivate(): boolean {
    return this.allowNavigation ||
      !this.hasUnsavedChanges() ||
      window.confirm(this.i18n.translate('You have unsaved changes. Leave this page and discard them?'));
  }

  saveNonMedical(value: PatientIntakeNonMedicalFormValue): void {
    this.facade.saveNonMedicalDraft(value, this.medicalQuestionnaire?.currentValue());
  }

  saveMedical(value: PatientIntakeMedicalAnswerFormValue[]): void {
    this.facade.saveMedicalDraft(value, this.demographicsForm?.currentValue());
  }

  reloadLatest(): void {
    if (!window.confirm(this.i18n.translate(
      'Reload the latest saved version and discard the local edits shown on this screen?'))) {
      return;
    }

    this.facade.reloadLatest();
  }

  replaceExpiredDraft(): void {
    if (!window.confirm(this.i18n.translate(
      'Start a new intake draft and discard the expired local edits shown on this screen?'))) {
      return;
    }

    this.facade.replaceExpiredDraft();
  }

  logout(): void {
    if (this.hasUnsavedChanges() && !window.confirm(this.i18n.translate(
      'End the session and discard your unsaved changes?'))) {
      return;
    }

    const mode = this.facade.mode();
    const target = mode === 'patient_intake' ? 'intake-login' : 'login';
    this.allowNavigation = true;

    this.facade.logout().subscribe(() => {
      void this.router.navigate(
        ['/patient-portal', this.tenantRealm(), target],
        { replaceUrl: true }
      );
    });
  }
}
