import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: UserDto;
  tenant: TenantDto;
  role: string;
}

export interface UserDto {
  id: string;
  email: string;
  displayName?: string;
}

export interface TenantDto {
  id: string;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = '/api/auth';
  private tokenSubject = new BehaviorSubject<string | null>(null);
  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  private currentTenantSubject = new BehaviorSubject<TenantDto | null>(null);
  private currentRoleSubject = new BehaviorSubject<string | null>(null);

  public token$ = this.tokenSubject.asObservable();
  public currentUser$ = this.currentUserSubject.asObservable();
  public currentTenant$ = this.currentTenantSubject.asObservable();
  public currentRole$ = this.currentRoleSubject.asObservable();

  constructor(private http: HttpClient) {
    // On initialization, we could attempt to restore session from a secure storage
    // For now, we keep everything in memory; token is lost on page refresh.
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request).pipe(
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

  getCurrentUser(): UserDto | null {
    return this.currentUserSubject.value;
  }

  getCurrentTenant(): TenantDto | null {
    return this.currentTenantSubject.value;
  }

  getCurrentRole(): string | null {
    return this.currentRoleSubject.value;
  }

  isAuthenticated(): boolean {
    return this.tokenSubject.value !== null;
  }
}