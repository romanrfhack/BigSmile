import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BranchSummary } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-patient-intake-link-issue-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <form class="issue-form" (ngSubmit)="submit()" novalidate>
      <div>
        <p class="issue-form__eyebrow">Nuevo ingreso</p>
        <h2>Generar enlace de sala de espera</h2>
        <p>
          Crea una credencial de un solo uso. El paciente podrá activar su acceso sin crear todavía
          un expediente canónico.
        </p>
      </div>

      <label for="waiting-room-branch">Sucursal operativa</label>
      <select id="waiting-room-branch" [formControl]="branchControl" [disabled]="issuing">
        <option value="">Sin sucursal específica</option>
        @for (branch of branches; track branch.id) {
          <option [value]="branch.id">{{ branch.name }}</option>
        }
      </select>
      <p class="issue-form__hint">
        La sucursal es contexto operativo opcional; el tenant sigue siendo la frontera de seguridad.
      </p>

      <button class="issue-form__submit" type="submit" [disabled]="issuing">
        {{ issuing ? 'Generando…' : 'Generar enlace seguro' }}
      </button>
    </form>
  `,
  styles: [`
    :host {
      display: block;
    }

    .issue-form {
      display: grid;
      gap: 0.85rem;
      padding: clamp(1rem, 2vw, 1.5rem);
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-lg);
      background: var(--bsm-gradient-surface);
      box-shadow: var(--bsm-shadow-sm);
    }

    .issue-form__eyebrow {
      margin: 0 0 0.25rem;
      color: var(--bsm-color-accent-dark);
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    h2,
    p {
      margin: 0;
    }

    h2 {
      color: var(--bsm-color-text-brand);
    }

    p,
    .issue-form__hint {
      color: var(--bsm-color-text-muted);
      line-height: 1.5;
    }

    label {
      color: var(--bsm-color-text-brand);
      font-weight: 800;
    }

    select {
      width: 100%;
      min-height: 2.8rem;
      padding: 0.65rem 0.75rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-sm);
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text);
    }

    .issue-form__hint {
      font-size: 0.85rem;
    }

    .issue-form__submit {
      justify-self: start;
      border: 0;
      border-radius: var(--bsm-radius-pill);
      padding: 0.75rem 1.1rem;
      background: var(--bsm-color-primary);
      color: #ffffff;
      font-weight: 800;
      cursor: pointer;
    }

    .issue-form__submit:disabled {
      opacity: 0.65;
    }

    @media (max-width: 640px) {
      .issue-form__submit {
        width: 100%;
      }
    }
  `]
})
export class PatientIntakeLinkIssueFormComponent {
  @Input() branches: BranchSummary[] = [];
  @Input() issuing = false;
  @Output() readonly issue = new EventEmitter<string | null>();

  readonly branchControl = new FormControl('', { nonNullable: true });

  submit(): void {
    if (this.issuing) {
      return;
    }

    const branchId = this.branchControl.value.trim();
    this.issue.emit(branchId || null);
  }
}
