import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { anonymousOnlyGuard, authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  let router: Router;
  let authService: {
    isAuthenticated: () => boolean;
    hasPermissions: () => boolean;
    hasScopes: () => boolean;
  };

  beforeEach(() => {
    authService = {
      isAuthenticated: () => false,
      hasPermissions: () => true,
      hasScopes: () => true
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService }
      ]
    });

    router = TestBed.inject(Router);
  });

  it('redirects unauthenticated users to login', () => {
    const result = TestBed.runInInjectionContext(() => authGuard({ data: {} } as never, {} as never));

    expect(router.serializeUrl(result as UrlTree)).toBe(router.serializeUrl(router.createUrlTree(['/login'])));
  });

  it('redirects authenticated users without required permission to app', () => {
    authService.isAuthenticated = () => true;
    authService.hasPermissions = () => false;

    const result = TestBed.runInInjectionContext(() => authGuard({
      data: {
        requiredPermissions: ['auth.self.read']
      }
    } as never, {} as never));

    expect(router.serializeUrl(result as UrlTree)).toBe(router.serializeUrl(router.createUrlTree(['/app'])));
  });

  it('allows authenticated users with the required access', () => {
    authService.isAuthenticated = () => true;

    const result = TestBed.runInInjectionContext(() => authGuard({
      data: {
        requiredPermissions: ['auth.self.read']
      }
    } as never, {} as never));

    expect(result).toBe(true);
  });
});

describe('anonymousOnlyGuard', () => {
  let router: Router;
  let authService: {
    isAuthenticated: () => boolean;
  };

  beforeEach(() => {
    authService = {
      isAuthenticated: () => true
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService }
      ]
    });

    router = TestBed.inject(Router);
  });

  it('redirects authenticated users away from login', () => {
    const result = TestBed.runInInjectionContext(() => anonymousOnlyGuard({} as never, {} as never));

    expect(router.serializeUrl(result as UrlTree)).toBe(router.serializeUrl(router.createUrlTree(['/app'])));
  });
});
