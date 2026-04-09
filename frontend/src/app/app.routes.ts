import { Routes } from '@angular/router';
import { anonymousOnlyGuard, authGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { SessionHomeComponent } from './features/auth/session-home/session-home.component';

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
  { path: '', redirectTo: '/app', pathMatch: 'full' },
  { path: '**', redirectTo: '/app' }
];
