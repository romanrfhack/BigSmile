export type OdontogramArch = 'upper' | 'lower';

export interface OdontogramToothLayout {
  toothCode: string;
  arch: OdontogramArch;
  x: number;
  y: number;
  rotation: number;
}

export const ODONTOGRAM_UPPER_ARCH_ORDER = [
  '18', '17', '16', '15', '14', '13', '12', '11',
  '21', '22', '23', '24', '25', '26', '27', '28'
] as const;

export const ODONTOGRAM_LOWER_ARCH_ORDER = [
  '48', '47', '46', '45', '44', '43', '42', '41',
  '31', '32', '33', '34', '35', '36', '37', '38'
] as const;

export const ODONTOGRAM_TOOTH_LAYOUT: OdontogramToothLayout[] = [
  { toothCode: '18', arch: 'upper', x: 7.5, y: 14, rotation: -24 },
  { toothCode: '17', arch: 'upper', x: 13, y: 16, rotation: -22 },
  { toothCode: '16', arch: 'upper', x: 18.8, y: 18.5, rotation: -18 },
  { toothCode: '15', arch: 'upper', x: 25, y: 21.5, rotation: -14 },
  { toothCode: '14', arch: 'upper', x: 31.8, y: 24.5, rotation: -10 },
  { toothCode: '13', arch: 'upper', x: 38.3, y: 27, rotation: -6 },
  { toothCode: '12', arch: 'upper', x: 43, y: 29, rotation: -3 },
  { toothCode: '11', arch: 'upper', x: 47.5, y: 30, rotation: -1 },
  { toothCode: '21', arch: 'upper', x: 52.5, y: 30, rotation: 1 },
  { toothCode: '22', arch: 'upper', x: 57, y: 29, rotation: 3 },
  { toothCode: '23', arch: 'upper', x: 61.7, y: 27, rotation: 6 },
  { toothCode: '24', arch: 'upper', x: 68.2, y: 24.5, rotation: 10 },
  { toothCode: '25', arch: 'upper', x: 75, y: 21.5, rotation: 14 },
  { toothCode: '26', arch: 'upper', x: 81.2, y: 18.5, rotation: 18 },
  { toothCode: '27', arch: 'upper', x: 87, y: 16, rotation: 22 },
  { toothCode: '28', arch: 'upper', x: 92.5, y: 14, rotation: 24 },
  { toothCode: '48', arch: 'lower', x: 7.5, y: 86, rotation: 24 },
  { toothCode: '47', arch: 'lower', x: 13, y: 84, rotation: 22 },
  { toothCode: '46', arch: 'lower', x: 18.8, y: 81.5, rotation: 18 },
  { toothCode: '45', arch: 'lower', x: 25, y: 78.5, rotation: 14 },
  { toothCode: '44', arch: 'lower', x: 31.8, y: 75.5, rotation: 10 },
  { toothCode: '43', arch: 'lower', x: 38.3, y: 73, rotation: 6 },
  { toothCode: '42', arch: 'lower', x: 43, y: 71, rotation: 3 },
  { toothCode: '41', arch: 'lower', x: 47.5, y: 70, rotation: 1 },
  { toothCode: '31', arch: 'lower', x: 52.5, y: 70, rotation: -1 },
  { toothCode: '32', arch: 'lower', x: 57, y: 71, rotation: -3 },
  { toothCode: '33', arch: 'lower', x: 61.7, y: 73, rotation: -6 },
  { toothCode: '34', arch: 'lower', x: 68.2, y: 75.5, rotation: -10 },
  { toothCode: '35', arch: 'lower', x: 75, y: 78.5, rotation: -14 },
  { toothCode: '36', arch: 'lower', x: 81.2, y: 81.5, rotation: -18 },
  { toothCode: '37', arch: 'lower', x: 87, y: 84, rotation: -22 },
  { toothCode: '38', arch: 'lower', x: 92.5, y: 86, rotation: -24 }
];
