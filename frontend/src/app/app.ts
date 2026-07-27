import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { LanguageSelectorComponent, TranslatePipe } from './shared/i18n';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LanguageSelectorComponent, TranslatePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly title = signal('frontend');

  canAccessDashboard(): boolean {
    return this.authService.hasPermissions(['dashboard.read']);
  }

  canManagePatientIntakeLinks(): boolean {
    return this.authService.hasPermissions(['patientportal.intake.manage']);
  }

  isLoginRoute(): boolean {
    const pathSegments = this.primaryPathSegments();
    return pathSegments.length === 1 && pathSegments[0] === 'login';
  }

  isPatientPortalRoute(): boolean {
    return this.primaryPathSegments()[0] === 'patient-portal';
  }

  isShelllessRoute(): boolean {
    return this.isLoginRoute() || this.isPatientPortalRoute();
  }

  private primaryPathSegments(): string[] {
    const primaryRoute = this.router.parseUrl(this.router.url).root.children['primary'];
    return (primaryRoute?.segments ?? []).map(segment => segment.path);
  }
}
