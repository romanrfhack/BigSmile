declare module 'qr' {
  export type QrErrorCorrectionLevel = 'low' | 'medium' | 'quartile' | 'high';

  export interface QrSvgOptions {
    ecc?: QrErrorCorrectionLevel;
    encoding?: 'numeric' | 'alphanumeric' | 'byte';
    scale?: number;
    border?: number;
  }

  export default function encodeQR(
    value: string,
    output: 'svg',
    options?: QrSvgOptions
  ): SVGSVGElement;
}
