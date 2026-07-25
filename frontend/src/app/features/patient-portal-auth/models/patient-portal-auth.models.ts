export interface CurrentPatientPortalSession {
  accountId: string;
  patientId: string;
  tenantSubdomain: string;
  loginName: string;
  sessionVersion: number;
}

export interface PatientPortalAuthenticationResponse {
  accessToken: string;
  expiresAtUtc: string;
  current: CurrentPatientPortalSession;
}

export interface ActivatePatientPortalAccountRequest {
  activationToken: string;
  loginName: string;
  password: string;
}

export interface LoginPatientPortalAccountRequest {
  loginName: string;
  password: string;
}
