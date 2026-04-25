import { authGuard } from './core/auth/auth.guard';
import { routes } from './app.routes';

describe('app routes', () => {
  it('protects the dashboard route with dashboard.read', () => {
    const route = routes.find((entry) => entry.path === 'dashboard');

    expect(route).toBeDefined();
    expect(route?.canActivate).toContain(authGuard);
    expect(route?.data?.['requiredPermissions']).toEqual(['dashboard.read']);
  });
});
