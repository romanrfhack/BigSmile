export type OdontogramToothStatus =
  | 'Unknown'
  | 'Healthy'
  | 'Missing'
  | 'Restored'
  | 'Caries';

export const ODONTOGRAM_TOOTH_STATUSES: OdontogramToothStatus[] = [
  'Unknown',
  'Healthy',
  'Missing',
  'Restored',
  'Caries'
];

export interface OdontogramToothState {
  toothCode: string;
  status: OdontogramToothStatus;
  updatedAtUtc: string;
  updatedByUserId: string;
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
