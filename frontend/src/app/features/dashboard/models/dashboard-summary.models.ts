export interface DashboardSummary {
  activePatientsCount: number;
  todayAppointmentsCount: number;
  todayPendingAppointmentsCount: number;
  activeDocumentsCount: number;
  activeTreatmentPlansCount: number;
  acceptedQuotesCount: number;
  issuedBillingDocumentsCount: number;
  generatedAtUtc: string;
}
