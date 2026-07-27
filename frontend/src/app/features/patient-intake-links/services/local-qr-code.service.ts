import { DOCUMENT } from '@angular/common';
import { Inject, Injectable, inject } from '@angular/core';
import encodeQR from 'qr';
import { I18nService } from '../../../core/i18n';

@Injectable({ providedIn: 'root' })
export class LocalQrCodeService {
  private readonly i18n = inject(I18nService);

  constructor(@Inject(DOCUMENT) private readonly document: Document) {}

  createSvg(value: string): SVGSVGElement {
    const normalizedValue = value?.trim();
    if (!normalizedValue) {
      throw new Error('A waiting-room URL is required to render a QR code.');
    }

    const rendered = encodeQR(normalizedValue, 'svg', { ecc: 'medium' });
    const svg = this.normalizeSvg(rendered);
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

  private normalizeSvg(rendered: SVGSVGElement | string): SVGSVGElement {
    const elementConstructor = this.document.defaultView?.Element;
    if (
      elementConstructor &&
      rendered instanceof elementConstructor &&
      rendered.tagName.toLowerCase() === 'svg'
    ) {
      return rendered as SVGSVGElement;
    }

    const markup = typeof rendered === 'string' ? rendered.trim() : `${rendered}`.trim();
    if (!markup.toLowerCase().includes('<svg')) {
      throw new Error('The local QR encoder did not return SVG markup.');
    }

    const template = this.document.createElement('template');
    template.innerHTML = markup;
    const svg = template.content.querySelector('svg');
    if (!svg) {
      throw new Error('The local QR encoder returned invalid SVG markup.');
    }

    return svg as SVGSVGElement;
  }
}
