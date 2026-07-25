import { FormControl, FormGroup } from '@angular/forms';
import {
  extractActivationToken,
  passwordsMatchValidator
} from './patient-portal-activation.page';

describe('patient portal activation helpers', () => {
  it('reads a one-time token from the fragment parameter format', () => {
    expect(extractActivationToken('token=abc_def-123')).toBe('abc_def-123');
  });

  it('accepts a raw fragment token for manually generated pilot links', () => {
    expect(extractActivationToken('abc_def-123')).toBe('abc_def-123');
  });

  it('returns no token for missing or malformed fragment data', () => {
    expect(extractActivationToken(null)).toBe('');
    expect(extractActivationToken('')).toBe('');
    expect(extractActivationToken('%E0%A4%A')).toBe('');
  });

  it('validates that password confirmation matches', () => {
    const matching = new FormGroup({
      password: new FormControl('twelve-character-password'),
      confirmPassword: new FormControl('twelve-character-password')
    });
    const different = new FormGroup({
      password: new FormControl('twelve-character-password'),
      confirmPassword: new FormControl('different-password')
    });

    expect(passwordsMatchValidator(matching)).toBeNull();
    expect(passwordsMatchValidator(different)).toEqual({ passwordMismatch: true });
  });
});
