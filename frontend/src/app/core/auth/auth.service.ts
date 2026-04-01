import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    displayName?: string;
  };
  tenant: {
    id: string;
    name: string;
  };
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenSubject = new BehaviorSubject<string | null>(null);
  private currentUserSubject = new BehaviorSubject<LoginResponse['user'] | null>(null);
  private currentTenantSubject = new BehaviorSubject<LoginResponse['tenant'] | null>(null);
  private currentRoleSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public currentTenant$ = this.currentTenantSubject.asObservable();
  public currentRole$ = this.currentRoleSubject.asObservable();

  constructor(private http: HttpClient) {
    // No persistent storage; token lives only in memory.
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.tokenSubject.next(response.token);
          this.currentUserSubject.next(response.user);
          this.currentTenantSubject.next(response.tenant);
          this.currentRoleSubject.next(response.role);
        })
      );
  }

  logout(): void {
    this.tokenSubject.next(null);
    this.currentUserSubject.next(null);
    this.currentTenantSubject.next(null);
    this.currentRoleSubject.next(null);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  getCurrentUser(): LoginResponse['user'] | null {
    return this.currentUserSubject.value;
  }

  getCurrentTenant(): LoginResponse['tenant'] | null {
    return this.currentTenantSubject.value;
  }

  getCurrentRole(): string | null {
    return this.currentRoleSubject.value;
  }
}