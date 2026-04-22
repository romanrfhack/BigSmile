export type BillingDocumentStatus = 'Draft' | 'Issued';

export interface BillingDocumentItem {
  billingItemId: string;
  sourceTreatmentQuoteItemId: string;
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

export interface BillingDocument {
  billingDocumentId: string;
  patientId: string;
  treatmentQuoteId: string;
  status: BillingDocumentStatus;
  currencyCode: string;
  totalAmount: number;
  items: BillingDocumentItem[];
  createdAtUtc: string;
  createdByUserId: string;
  lastUpdatedAtUtc: string;
  lastUpdatedByUserId: string;
  issuedAtUtc: string | null;
  issuedByUserId: string | null;
}

export interface ChangeBillingDocumentStatusRequest {
  status: BillingDocumentStatus;
}
