declare module 'qr' {
  export type QrErrorCorrectionLevel = 'low' | 'medium' | 'quartile' | 'high';

  export interface QrSvgOptions {
    ecc?: QrErrorCorrectionLevel;
    margin?: number;
    size?: number;
  }

  export default function encodeQR(
    value: string,
    output: 'svg',
    options?: QrSvgOptions
  ): SVGSVGElement;
}
