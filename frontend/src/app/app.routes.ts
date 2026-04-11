import { Routes } from '@angular/router';
import { anonymousOnlyGuard, authGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { SessionHomeComponent } from './features/auth/session-home/session-home.component';
import { PatientFormPageComponent } from './features/patients/pages/patient-form.page';
import { PatientListPageComponent } from './features/patients/pages/patient-list.page';
import { PatientProfilePageComponent } from './features/patients/pages/patient-profile.page';
import { SchedulingPageComponent } from './features/scheduling/pages/scheduling.page';

export const routes: Routes = [
  { path: 'login', component: LoginComponent, canActivate: [anonymousOnlyGuard] },
  {
    path: 'app',
    component: SessionHomeComponent,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['auth.self.read']
    }
  },
  {
    path: 'patients',
    component: PatientListPageComponent,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.read']
    }
  },
  {
    path: 'patients/new',
    component: PatientFormPageComponent,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.write']
    }
  },
  {
    path: 'patients/:id/edit',
    component: PatientFormPageComponent,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.write']
    }
  },
  {
    path: 'patients/:id',
    component: PatientProfilePageComponent,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['patient.read']
    }
  },
  {
    path: 'scheduling',
    component: SchedulingPageComponent,
    canActivate: [authGuard],
    data: {
      requiredPermissions: ['scheduling.read']
    }
  },
  { path: '', redirectTo: '/patients', pathMatch: 'full' },
  { path: '**', redirectTo: '/patients' }
];
