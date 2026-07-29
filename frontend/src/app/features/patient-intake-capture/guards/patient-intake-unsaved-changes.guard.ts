import { CanDeactivateFn } from '@angular/router';

export interface PatientIntakeUnsavedChangesAware {
  canDeactivate(): boolean;
}

export const patientIntakeUnsavedChangesGuard: CanDeactivateFn<PatientIntakeUnsavedChangesAware> =
  (component) => component.canDeactivate();
