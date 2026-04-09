import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export type AccessScope = 'anonymous' | 'tenant' | 'branch' | 'platform';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserSummary {
  id: string;
  email: string;
  displayName?: string;
}

export interface TenantSummary {
  id: string;
  name: string;
}

export interface BranchSummary {
  id: string;
  name: string;
}

export interface CurrentUserResponse {
  user: UserSummary;
  tenant: TenantSummary | null;
  currentBranch: BranchSummary | null;
  branches: BranchSummary[];
  permissions: string[];
  role: string;
  scope: AccessScope;
}

export interface LoginResponse {
  token: string;
  current: CurrentUserResponse;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenSubject = new BehaviorSubject<string | null>(null);
  private currentSubject = new BehaviorSubject<CurrentUserResponse | null>(null);

  public current$ = this.currentSubject.asObservable();

  constructor(private http: HttpClient) {
    // No persistent storage; token lives only in memory.
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.setSession(response.token, response.current);
        })
      );
  }

  loadCurrentUser(): Observable<CurrentUserResponse> {
    return this.http.get<CurrentUserResponse>(`${environment.apiUrl}/api/auth/me`)
      .pipe(
        tap(current => this.currentSubject.next(current))
      );
  }

  logout(): void {
    this.tokenSubject.next(null);
    this.currentSubject.next(null);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  isAuthenticated(): boolean {
    return this.getToken() !== null && this.currentSubject.value !== null;
  }

  getCurrent(): CurrentUserResponse | null {
    return this.currentSubject.value;
  }

  getCurrentUser(): UserSummary | null {
    return this.currentSubject.value?.user ?? null;
  }

  getCurrentTenant(): TenantSummary | null {
    return this.currentSubject.value?.tenant ?? null;
  }

  getCurrentRole(): string | null {
    return this.currentSubject.value?.role ?? null;
  }

  getCurrentScope(): AccessScope | null {
    return this.currentSubject.value?.scope ?? null;
  }

  hasPermissions(requiredPermissions?: string[]): boolean {
    if (!requiredPermissions?.length) {
      return true;
    }

    const current = this.currentSubject.value;
    if (!current) {
      return false;
    }

    return requiredPermissions.every(permission => current.permissions.includes(permission));
  }

  hasScopes(requiredScopes?: AccessScope[]): boolean {
    if (!requiredScopes?.length) {
      return true;
    }

    const current = this.currentSubject.value;
    return current !== null && requiredScopes.includes(current.scope);
  }

  private setSession(token: string, current: CurrentUserResponse): void {
    this.tokenSubject.next(token);
    this.currentSubject.next(current);
  }
}
