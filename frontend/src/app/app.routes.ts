import { Routes } from '@angular/router';
import { anonymousOnlyGuard, authGuard } from './core/auth/auth.guard';

const loadLoginComponent = () =>
  import('./features/auth/login/login.component').then((m) => m.LoginComponent);
const loadSessionHomeComponent = () =>
  import('./features/auth/session-home/session-home.component').then((m) => m.SessionHomeComponent);
const loadDashboardPage = () =>
  import('./features/dashboard/pages/dashboard.page').then((m) => m.DashboardPageComponent);
const loadPatientListPage = () =>
  import('./features/patients/pages/patient-list.page').then((m) => m.PatientListPageComponent);
const loadPatientFormPage = () =>
  import('./features/patients/pages/patient-form.page').then((m) => m.PatientFormPageComponent);
const loadPatientProfilePage = () =>
  import('./features/patients/pages/patient-profile.page').then(
    (m) => m.PatientProfilePageComponent,
  );
const loadClinicalRecordPage = () =>
  import('./features/clinical-records/pages/clinical-record.page').then(
    (m) => m.ClinicalRecordPageComponent,
  );
const loadOdontogramPage = () =>
  import('./features/odontogram/pages/odontogram.page').then((m) => m.OdontogramPageComponent);
const loadPatientDocumentsPage = () =>
  import('./features/documents/pages/patient-documents.page').then(
    (m) => m.PatientDocumentsPageComponent,
  );
const loadBillingDocumentPage = () =>
  import('./features/billing/pages/billing-document.page').then(
    (m) => m.BillingDocumentPageComponent,
  );
const loadTreatmentQuotePage = () =>
  import('./features/treatments/pages/treatment-quote.page').then(
    (m) => m.TreatmentQuotePageComponent,
  );
const loadTreatmentPlanPage = () =>
  import('./features/treatments/pages/treatment-plan.page').then(
    (m) => m.TreatmentPlanPageComponent,
  );
const loadSchedulingPage = () =>
  import('./features/scheduling/pages/scheduling.page').then((m) => m.SchedulingPageComponent);

export const routes: Routes = [
  { path: 'login', loadComponent: loadLoginComponent, canActivate: [anonymousOnlyGuard] },
  {
    path: 'app',
    loadComponent: loadSessionHomeComponent,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['auth.self.read'],
    },
  },
  {
    path: 'dashboard',
    loadComponent: loadDashboardPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['dashboard.read'],
    },
  },
  {
    path: 'patients',
    loadComponent: loadPatientListPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.read'],
    },
  },
  {
    path: 'patients/new',
    loadComponent: loadPatientFormPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.write'],
    },
  },
  {
    path: 'patients/:id/edit',
    loadComponent: loadPatientFormPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.write'],
    },
  },
  {
    path: 'patients/:id',
    loadComponent: loadPatientProfilePage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.read'],
    },
  },
  {
    path: 'patients/:id/clinical-record',
    loadComponent: loadClinicalRecordPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['clinical.read'],
    },
  },
  {
    path: 'patients/:id/odontogram',
    loadComponent: loadOdontogramPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['odontogram.read'],
    },
  },
  {
    path: 'patients/:id/documents',
    loadComponent: loadPatientDocumentsPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['document.read'],
    },
  },
  {
    path: 'patients/:id/treatment-plan/quote/billing',
    loadComponent: loadBillingDocumentPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['billing.read'],
    },
  },
  {
    path: 'patients/:id/treatment-plan/quote',
    loadComponent: loadTreatmentQuotePage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['treatmentquote.read'],
    },
  },
  {
    path: 'patients/:id/treatment-plan',
    loadComponent: loadTreatmentPlanPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['treatmentplan.read'],
    },
  },
  {
    path: 'scheduling',
    loadComponent: loadSchedulingPage,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['scheduling.read'],
    },
  },
  { path: '', redirectTo: '/patients', pathMatch: 'full' },
  { path: '**', redirectTo: '/patients' },
];
