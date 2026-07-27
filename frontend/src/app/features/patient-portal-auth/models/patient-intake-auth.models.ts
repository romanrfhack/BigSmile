export interface ActivatePatientIntakeAccountRequest {
  accessToken: string;
  loginName: string;
  password: string;
}

export interface LoginPatientIntakeAccountRequest {
  loginName: string;
  password: string;
}

export interface CurrentPatientIntakeSession {
  accountId: string;
  intakeId: string;
  tenantSubdomain: string;
  loginName: string;
  sessionVersion: number;
}

export interface PatientIntakeAuthenticationResponse {
  accessToken: string;
  expiresAtUtc: string;
  current: CurrentPatientIntakeSession;
}
