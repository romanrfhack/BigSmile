export type OdontogramToothStatus =
  | 'Unknown'
  | 'Healthy'
  | 'Missing'
  | 'Restored'
  | 'Caries';

export type OdontogramSurfaceCode = 'O' | 'M' | 'D' | 'B' | 'L';

export type OdontogramSurfaceStatus =
  | 'Unknown'
  | 'Healthy'
  | 'Restored'
  | 'Caries';

export const ODONTOGRAM_TOOTH_STATUSES: OdontogramToothStatus[] = [
  'Unknown',
  'Healthy',
  'Missing',
  'Restored',
  'Caries'
];

export const ODONTOGRAM_SURFACE_CODES: OdontogramSurfaceCode[] = ['O', 'M', 'D', 'B', 'L'];

export const ODONTOGRAM_SURFACE_STATUSES: OdontogramSurfaceStatus[] = [
  'Unknown',
  'Healthy',
  'Restored',
  'Caries'
];

export interface OdontogramSurfaceState {
  surfaceCode: OdontogramSurfaceCode;
  status: OdontogramSurfaceStatus;
  updatedAtUtc: string;
  updatedByUserId: string;
}

export interface OdontogramToothState {
  toothCode: string;
  status: OdontogramToothStatus;
  updatedAtUtc: string;
  updatedByUserId: string;
  surfaces: OdontogramSurfaceState[];
}

export interface Odontogram {
  odontogramId: string;
  patientId: string;
  teeth: OdontogramToothState[];
  createdAtUtc: string;
  createdByUserId: string;
  lastUpdatedAtUtc: string;
  lastUpdatedByUserId: string;
}

export interface UpdateToothStatusRequest {
  status: OdontogramToothStatus;
}

export interface UpdateSurfaceStatusRequest {
  status: OdontogramSurfaceStatus;
}
