import { patientIntakeUnsavedChangesGuard } from './patient-intake-unsaved-changes.guard';

describe('patientIntakeUnsavedChangesGuard', () => {
  it('delegates the decision to the patient intake workspace only', () => {
    const component = { canDeactivate: vi.fn().mockReturnValue(false) };

    const result = patientIntakeUnsavedChangesGuard(
      component,
      {} as never,
      {} as never,
      {} as never
    );

    expect(component.canDeactivate).toHaveBeenCalledTimes(1);
    expect(result).toBe(false);
  });

  it('allows navigation after the component confirms it is safe', () => {
    const component = { canDeactivate: vi.fn().mockReturnValue(true) };

    expect(patientIntakeUnsavedChangesGuard(
      component,
      {} as never,
      {} as never,
      {} as never
    )).toBe(true);
  });
});
