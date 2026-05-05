import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
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
  protected readonly title = signal('frontend');

  canAccessDashboard(): boolean {
    return this.authService.hasPermissions(['dashboard.read']);
  }
}
