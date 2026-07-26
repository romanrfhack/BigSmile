export type PatientIntakeAccessLinkStatus =
  | 'Active'
  | 'Expired'
  | 'Revoked'
  | 'Consumed'
  | 'Pending';

export interface PatientIntakeAccessLinkSummary {
  id: string;
  branchId: string | null;
  purpose: string;
  status: PatientIntakeAccessLinkStatus | string;
  createdAtUtc: string;
  expiresAtUtc: string;
  revokedAtUtc: string | null;
  consumedAtUtc: string | null;
}

export interface IssuedPatientIntakeAccessLink {
  id: string;
  branchId: string | null;
  purpose: string;
  accessToken: string;
  createdAtUtc: string;
  expiresAtUtc: string;
}

export interface IssuePatientIntakeAccessLinkRequest {
  branchId: string | null;
}

export interface WaitingRoomHandoff {
  clinicName: string;
  branchName: string | null;
  url: string;
  createdAtUtc: string;
  expiresAtUtc: string;
}

export type WaitingRoomCopyState = 'idle' | 'copying' | 'copied' | 'error';

export function isActiveWaitingRoomLink(link: PatientIntakeAccessLinkSummary): boolean {
  return link.status.toLowerCase() === 'active';
}
