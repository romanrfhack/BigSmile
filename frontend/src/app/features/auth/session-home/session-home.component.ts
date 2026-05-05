import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, CurrentUserResponse } from '../../../core/auth/auth.service';
import { TranslatePipe } from '../../../shared/i18n';

@Component({
  selector: 'app-session-home',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  template: `
    <section class="session-card">
      <header class="session-header">
        <div>
          <h2>{{ 'Current Access Context' | t }}</h2>
          <p>{{ 'Tenant-aware authorization is active for the current in-memory session.' | t }}</p>
        </div>
        <div class="session-actions">
          <button type="button" class="btn btn-secondary" (click)="refresh()" [disabled]="loading">
            {{ (loading ? 'Refreshing...' : 'Refresh context') | t }}
          </button>
          <button type="button" class="btn btn-primary" (click)="logout()">{{ 'Sign out' | t }}</button>
        </div>
      </header>

      <div *ngIf="error" class="error-message">{{ error | t }}</div>

      <ng-container *ngIf="current as session; else missingSession">
        <dl class="session-grid">
          <div>
            <dt>{{ 'User' | t }}</dt>
            <dd>{{ session.user.displayName || session.user.email }}</dd>
          </div>
          <div>
            <dt>{{ 'Email' | t }}</dt>
            <dd>{{ session.user.email }}</dd>
          </div>
          <div>
            <dt>{{ 'Role' | t }}</dt>
            <dd>{{ session.role }}</dd>
          </div>
          <div>
            <dt>{{ 'Scope' | t }}</dt>
            <dd>{{ session.scope }}</dd>
          </div>
          <div>
            <dt>{{ 'Tenant' | t }}</dt>
            <dd>{{ session.tenant?.name || ('Platform scope' | t) }}</dd>
          </div>
          <div>
            <dt>{{ 'Current branch' | t }}</dt>
            <dd>{{ session.currentBranch?.name || ('No branch selected' | t) }}</dd>
          </div>
        </dl>

        <section class="list-section">
          <h3>{{ 'Allowed branches' | t }}</h3>
          <p *ngIf="!session.branches.length" class="muted">{{ 'No branch-restricted assignments are active for this session.' | t }}</p>
          <ul *ngIf="session.branches.length" class="tag-list">
            <li *ngFor="let branch of session.branches">{{ branch.name }}</li>
          </ul>
        </section>

        <section class="list-section">
          <h3>{{ 'Granted permissions' | t }}</h3>
          <ul class="tag-list">
            <li *ngFor="let permission of session.permissions">{{ permission }}</li>
          </ul>
        </section>
      </ng-container>

      <ng-template #missingSession>
        <p class="muted">{{ 'No current session is loaded.' | t }}</p>
      </ng-template>
    </section>
  `,
  styles: [`
    .session-card {
      display: grid;
      gap: 1.5rem;
      border: 1px solid #d7deea;
      border-radius: 16px;
      padding: 1.5rem;
      background: linear-gradient(180deg, #ffffff 0%, #f5f8fc 100%);
      box-shadow: 0 18px 40px rgba(31, 55, 86, 0.08);
    }

    .session-header {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: flex-start;
    }

    .session-header h2 {
      margin: 0 0 0.25rem;
      color: #16324f;
    }

    .session-header p {
      margin: 0;
      color: #4b5d72;
    }

    .session-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
    }

    .session-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 1rem;
      margin: 0;
    }

    .session-grid div {
      padding: 1rem;
      border-radius: 12px;
      background: #ffffff;
      border: 1px solid #e0e8f2;
    }

    dt {
      margin-bottom: 0.4rem;
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: #6b7f96;
    }

    dd {
      margin: 0;
      color: #18324a;
      font-weight: 600;
    }

    .list-section h3 {
      margin: 0 0 0.75rem;
      color: #16324f;
    }

    .tag-list {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .tag-list li {
      padding: 0.5rem 0.75rem;
      border-radius: 999px;
      background: #e7f0fb;
      color: #214c7b;
      font-size: 0.9rem;
      font-weight: 600;
    }

    .btn {
      border: none;
      border-radius: 999px;
      padding: 0.7rem 1rem;
      font-weight: 600;
      cursor: pointer;
    }

    .btn-primary {
      background: #1d6ed8;
      color: #ffffff;
    }

    .btn-secondary {
      background: #e9eef5;
      color: #16324f;
    }

    .btn:disabled {
      cursor: not-allowed;
      opacity: 0.7;
    }

    .muted {
      color: #62758a;
      margin: 0;
    }

    .error-message {
      padding: 0.85rem 1rem;
      border-radius: 12px;
      background: #fdecec;
      color: #8a1f1f;
      border: 1px solid #f6cdcd;
    }

    @media (max-width: 768px) {
      .session-header {
        flex-direction: column;
      }

      .session-actions {
        width: 100%;
      }

      .session-actions .btn {
        width: 100%;
      }
    }
  `]
})
export class SessionHomeComponent implements OnInit {
  current: CurrentUserResponse | null = null;
  loading = false;
  error = '';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.current = this.authService.getCurrent();
    this.refresh();
  }

  refresh(): void {
    if (!this.authService.isAuthenticated() || this.loading) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.loadCurrentUser().subscribe({
      next: (current) => {
        this.current = current;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'The current access context could not be refreshed.';
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
