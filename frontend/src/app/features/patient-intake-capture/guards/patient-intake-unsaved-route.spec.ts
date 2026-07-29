import { routes } from '../../../app.routes';
import { patientIntakeUnsavedChangesGuard } from './patient-intake-unsaved-changes.guard';

describe('patient intake unsaved route boundary', () => {
  it('protects only the patient intake workspace route', () => {
    const patientPortal = routes.find(route => route.path === 'patient-portal');
    const intake = patientPortal?.children?.find(route => route.path === ':tenantSubdomain/intake');
    const login = patientPortal?.children?.find(route => route.path === ':tenantSubdomain/intake-login');

    expect(intake?.canDeactivate).toContain(patientIntakeUnsavedChangesGuard);
    expect(login?.canDeactivate).toBeUndefined();
    expect(routes.find(route => route.path === 'patients')?.canDeactivate).toBeUndefined();
  });
});
