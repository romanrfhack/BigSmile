export type CalendarViewMode = 'day' | 'week';
export type AppointmentEditorMode = 'create' | 'edit' | 'reschedule';
export type AppointmentStatus = 'Scheduled' | 'Cancelled';

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
