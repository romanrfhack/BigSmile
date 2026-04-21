export type TreatmentPlanStatus = 'Draft' | 'Proposed' | 'Accepted';

export const TREATMENT_PLAN_STATUSES: TreatmentPlanStatus[] = [
  'Draft',
  'Proposed',
  'Accepted'
];

export const TREATMENT_PLAN_TOOTH_CODES = [
  '11', '12', '13', '14', '15', '16', '17', '18',
  '21', '22', '23', '24', '25', '26', '27', '28',
  '31', '32', '33', '34', '35', '36', '37', '38',
  '41', '42', '43', '44', '45', '46', '47', '48'
] as const;

export type TreatmentPlanToothCode = typeof TREATMENT_PLAN_TOOTH_CODES[number];

export const TREATMENT_PLAN_SURFACE_CODES = ['O', 'M', 'D', 'B', 'L'] as const;
export type TreatmentPlanSurfaceCode = typeof TREATMENT_PLAN_SURFACE_CODES[number];

export interface TreatmentPlanItem {
  itemId: string;
  title: string;
  category: string | null;
  quantity: number;
  notes: string | null;
  toothCode: string | null;
  surfaceCode: string | null;
  createdAtUtc: string;
  createdByUserId: string;
}

export interface TreatmentPlan {
  treatmentPlanId: string;
  patientId: string;
  status: TreatmentPlanStatus;
  items: TreatmentPlanItem[];
  createdAtUtc: string;
  createdByUserId: string;
  lastUpdatedAtUtc: string;
  lastUpdatedByUserId: string;
}

export interface AddTreatmentPlanItemRequest {
  title: string;
  category: string | null;
  quantity: number;
  notes: string | null;
  toothCode: string | null;
  surfaceCode: string | null;
}

export interface ChangeTreatmentPlanStatusRequest {
  status: TreatmentPlanStatus;
}
