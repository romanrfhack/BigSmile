import { Injectable, inject } from '@angular/core';
import encodeQR from 'qr';
import { I18nService } from '../../../core/i18n';

@Injectable({ providedIn: 'root' })
export class LocalQrCodeService {
  private readonly i18n = inject(I18nService);

  createSvg(value: string): SVGSVGElement {
    const normalizedValue = value?.trim();
    if (!normalizedValue) {
      throw new Error('A waiting-room URL is required to render a QR code.');
    }

    const svg = encodeQR(normalizedValue, 'svg', { ecc: 'medium' }) as SVGSVGElement;
    svg.setAttribute('role', 'img');
    svg.setAttribute('aria-label', this.i18n.translate('Waiting-room link QR code'));
    svg.setAttribute('focusable', 'false');
    svg.setAttribute('width', '100%');
    svg.setAttribute('height', '100%');
    svg.style.display = 'block';
    svg.style.maxWidth = '100%';
    svg.style.height = 'auto';
    return svg;
  }
}
