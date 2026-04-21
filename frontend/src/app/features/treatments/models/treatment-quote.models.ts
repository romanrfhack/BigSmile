export type TreatmentQuoteStatus = 'Draft' | 'Proposed' | 'Accepted';

export const TREATMENT_QUOTE_STATUSES: TreatmentQuoteStatus[] = [
  'Draft',
  'Proposed',
  'Accepted'
];

export interface TreatmentQuoteItem {
  quoteItemId: string;
  sourceTreatmentPlanItemId: string;
  title: string;
  category: string | null;
  quantity: number;
  notes: string | null;
  toothCode: string | null;
  surfaceCode: string | null;
  unitPrice: number;
  lineTotal: number;
  createdAtUtc: string;
  createdByUserId: string;
}

export interface TreatmentQuote {
  treatmentQuoteId: string;
  patientId: string;
  treatmentPlanId: string;
  status: TreatmentQuoteStatus;
  currencyCode: string;
  total: number;
  items: TreatmentQuoteItem[];
  createdAtUtc: string;
  createdByUserId: string;
  lastUpdatedAtUtc: string;
  lastUpdatedByUserId: string;
}

export interface UpdateTreatmentQuoteItemPriceRequest {
  unitPrice: number;
}

export interface ChangeTreatmentQuoteStatusRequest {
  status: TreatmentQuoteStatus;
}
