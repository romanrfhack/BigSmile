import { TestBed } from '@angular/core/testing';
import { LocalQrCodeService } from './local-qr-code.service';

describe('LocalQrCodeService', () => {
  it('renders an inline SVG without remote image or script references', () => {
    TestBed.configureTestingModule({});
    const service = TestBed.inject(LocalQrCodeService);

    const svg = service.createSvg('https://example.test/patient-portal/intake-activate#token=secret');

    expect(svg.tagName.toLowerCase()).toBe('svg');
    expect(svg.getAttribute('role')).toBe('img');
    expect(svg.querySelector('script')).toBeNull();
    expect(svg.querySelector('image')).toBeNull();
    expect(svg.querySelector('[href]')).toBeNull();
    expect(svg.querySelector('[src]')).toBeNull();
  });

  it('rejects an empty URL', () => {
    TestBed.configureTestingModule({});
    expect(() => TestBed.inject(LocalQrCodeService).createSvg('')).toThrow();
  });
});
