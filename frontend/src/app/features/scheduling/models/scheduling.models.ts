export type CalendarViewMode = 'day' | 'week';
export type AppointmentEditorMode = 'create' | 'edit' | 'reschedule';
export type AppointmentStatus = 'Scheduled' | 'Cancelled' | 'Attended' | 'NoShow';
export type AppointmentConfirmationStatus = 'Pending' | 'Confirmed';
export type AppointmentReminderChannel = 'Phone' | 'WhatsApp' | 'Email' | 'Other';
export type AppointmentReminderOutcome = 'Reached' | 'NoAnswer' | 'LeftMessage';
export type AppointmentReminderState = 'NotRequired' | 'Pending' | 'Due' | 'Completed';

export interface SchedulingBranch {
  id: string;
  tenantId: string;
  name: string;
  address: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface SchedulingPatientLookup {
  id: string;
  fullName: string;
  primaryPhone: string | null;
  email: string | null;
  hasClinicalAlerts: boolean;
}

export interface AppointmentSummary {
  id: string;
  branchId: string;
  patientId: string;
  patientFullName: string;
  startsAt: string;
  endsAt: string;
  status: AppointmentStatus;
  confirmationStatus: AppointmentConfirmationStatus;
  confirmedAtUtc: string | null;
  confirmedByUserId: string | null;
  reminderRequired?: boolean;
  reminderChannel?: AppointmentReminderChannel | null;
  reminderDueAtUtc?: string | null;
  reminderCompletedAtUtc?: string | null;
  reminderCompletedByUserId?: string | null;
  reminderUpdatedAtUtc?: string | null;
  reminderUpdatedByUserId?: string | null;
  notes: string | null;
  cancellationReason: string | null;
}

export interface AppointmentBlockSummary {
  id: string;
  branchId: string;
  startsAt: string;
  endsAt: string;
  label: string | null;
}

export interface AppointmentReminderLogEntry {
  id: string;
  appointmentId: string;
  channel: AppointmentReminderChannel;
  outcome: AppointmentReminderOutcome;
  notes: string | null;
  createdAtUtc: string;
  createdByUserId: string;
}

export interface AppointmentReminderWorkItem {
  appointmentId: string;
  branchId: string;
  patientId: string;
  patientFullName: string;
  startsAt: string;
  appointmentStatus: AppointmentStatus;
  confirmationStatus: AppointmentConfirmationStatus;
  reminderChannel: AppointmentReminderChannel | null;
  reminderDueAtUtc: string | null;
  reminderState: AppointmentReminderState;
  reminderCompletedAtUtc: string | null;
  reminderCompletedByUserId: string | null;
}

export interface CalendarDay {
  date: string;
  appointments: AppointmentSummary[];
  blockedSlots: AppointmentBlockSummary[];
}

export interface CalendarView {
  branchId: string;
  startDate: string;
  days: number;
  calendarDays: CalendarDay[];
}

export interface CreateAppointmentRequest {
  branchId: string;
  patientId: string;
  startsAt: string;
  endsAt: string;
  notes: string | null;
}

export interface UpdateAppointmentRequest {
  patientId: string;
  startsAt: string;
  endsAt: string;
  notes: string | null;
}

export interface RescheduleAppointmentRequest {
  startsAt: string;
  endsAt: string;
}

export interface CancelAppointmentRequest {
  reason: string | null;
}

export interface ChangeAppointmentConfirmationRequest {
  status: AppointmentConfirmationStatus;
}

export interface ConfigureAppointmentManualReminderRequest {
  required: boolean;
  channel: AppointmentReminderChannel | null;
  dueAtUtc: string | null;
}

export interface AddAppointmentReminderLogEntryRequest {
  channel: AppointmentReminderChannel;
  outcome: AppointmentReminderOutcome;
  notes: string | null;
}

export interface ManualReminderFollowUpRequest {
  channel: AppointmentReminderChannel;
  outcome: AppointmentReminderOutcome;
  notes: string | null;
  completeReminder: boolean;
  confirmAppointment: boolean;
}

export interface ManualReminderFollowUpResult {
  appointment: AppointmentSummary;
  reminderLogEntry: AppointmentReminderLogEntry;
}

export interface CreateAppointmentBlockRequest {
  branchId: string;
  startsAt: string;
  endsAt: string;
  label: string | null;
}

export interface AppointmentFormValue {
  patientId: string | null;
  startsAt: string;
  endsAt: string;
  notes: string | null;
}

export interface AppointmentBlockFormValue {
  startsAt: string;
  endsAt: string;
  label: string | null;
}

export interface AppointmentReminderLogFormValue {
  channel: AppointmentReminderChannel;
  outcome: AppointmentReminderOutcome;
  notes: string | null;
}

export interface AppointmentReminderFollowUpFormValue {
  channel: AppointmentReminderChannel;
  outcome: AppointmentReminderOutcome;
  notes: string | null;
  completeReminder: boolean;
  confirmAppointment: boolean;
}

export interface AppointmentManualReminderFormValue {
  channel: AppointmentReminderChannel;
  dueAtUtc: string;
}
