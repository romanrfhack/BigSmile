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

  isLoginRoute(): boolean {
    const primaryRoute = this.router.parseUrl(this.router.url).root.children['primary'];
    const pathSegments = primaryRoute?.segments ?? [];

    return pathSegments.length === 1 && pathSegments[0].path === 'login';
  }
}
