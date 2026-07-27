import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientIntakeLinksApi } from '../data-access/patient-intake-links.api';
import { PatientIntakeLinkBrowserActions } from '../services/patient-intake-link-browser-actions.service';
import { WaitingRoomHandoffUrlBuilder } from '../services/waiting-room-handoff-url.builder';
import { PatientIntakeLinksFacade } from './patient-intake-links.facade';

describe('PatientIntakeLinksFacade', () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  afterEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  it('loads metadata and builds a memory-only handoff from the one-time issue response', () => {
    const api = createApi();
    const builder = {
      build: vi.fn().mockReturnValue(
        'https://clinic.test/patient-portal/intake-activate#token=one-time-token'
      )
    } as unknown as WaitingRoomHandoffUrlBuilder;
    const facade = createFacade(api, builder);

    facade.initialize();
    facade.issue('branch-a');

    expect(facade.branches()).toEqual([{ id: 'branch-a', name: 'Centro' }]);
    expect(builder.build).toHaveBeenCalledWith('one-time-token');
    expect(facade.handoff()).toEqual({
      linkId: 'issued-link',
      clinicName: 'Clínica Uno',
      branchName: 'Centro',
      url: 'https://clinic.test/patient-portal/intake-activate#token=one-time-token',
      createdAtUtc: '2026-07-26T10:00:00Z',
      expiresAtUtc: '2026-07-26T10:30:00Z'
    });
    expect(facade.handoff()).not.toHaveProperty('accessToken');
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });

  it('delegates copy and print without network or persistent storage', async () => {
    const api = createApi();
    const browserActions = {
      copyText: vi.fn().mockResolvedValue(undefined),
      printCurrentHandoff: vi.fn(),
      confirmRevoke: vi.fn().mockReturnValue(true)
    } as unknown as PatientIntakeLinkBrowserActions;
    const facade = createFacade(api, undefined, browserActions);

    facade.issue(null);
    await facade.copyHandoff();
    facade.printHandoff();

    expect(browserActions.copyText).toHaveBeenCalledWith(
      'https://clinic.test/patient-portal/intake-activate#token=one-time-token'
    );
    expect(browserActions.printCurrentHandoff).toHaveBeenCalledTimes(1);
    expect(facade.copyState()).toBe('copied');
    expect(window.localStorage.length).toBe(0);
    expect(window.sessionStorage.length).toBe(0);
  });

  it('revokes only after confirmation and never recreates a raw URL from metadata', () => {
    const api = createApi();
    const browserActions = {
      copyText: vi.fn(),
      printCurrentHandoff: vi.fn(),
      confirmRevoke: vi.fn().mockReturnValue(true)
    } as unknown as PatientIntakeLinkBrowserActions;
    const facade = createFacade(api, undefined, browserActions);
    facade.initialize();

    const link = facade.links()[0];
    facade.revoke(link);

    expect(browserActions.confirmRevoke).toHaveBeenCalledTimes(1);
    expect(api.revoke).toHaveBeenCalledWith('existing-link');
    expect(facade.links()[0].status).toBe('Revoked');
    expect(facade.links()[0]).not.toHaveProperty('accessToken');
  });

  function createFacade(
    api = createApi(),
    builder = {
      build: vi.fn().mockReturnValue(
        'https://clinic.test/patient-portal/intake-activate#token=one-time-token'
      )
    } as unknown as WaitingRoomHandoffUrlBuilder,
    browserActions = {
      copyText: vi.fn().mockResolvedValue(undefined),
      printCurrentHandoff: vi.fn(),
      confirmRevoke: vi.fn().mockReturnValue(true)
    } as unknown as PatientIntakeLinkBrowserActions
  ): PatientIntakeLinksFacade {
    const auth = {
      getCurrent: () => ({
        user: { id: 'user-id', email: 'admin@example.test' },
        tenant: { id: 'tenant-id', name: 'Clínica Uno' },
        currentBranch: null,
        branches: [{ id: 'branch-a', name: 'Centro' }],
        permissions: ['patientportal.intake.manage'],
        role: 'TenantAdmin',
        scope: 'tenant'
      })
    } as unknown as AuthService;

    return new PatientIntakeLinksFacade(api, auth, builder, browserActions);
  }

  function createApi(): PatientIntakeLinksApi & {
    list: ReturnType<typeof vi.fn>;
    issue: ReturnType<typeof vi.fn>;
    revoke: ReturnType<typeof vi.fn>;
  } {
    return {
      list: vi.fn().mockReturnValue(of([{
        id: 'existing-link',
        branchId: null,
        purpose: 'NewPatientWaitingRoomRegistration',
        status: 'Active',
        createdAtUtc: '2026-07-26T09:00:00Z',
        expiresAtUtc: '2026-07-26T09:30:00Z',
        revokedAtUtc: null,
        consumedAtUtc: null
      }])),
      issue: vi.fn().mockReturnValue(of({
        id: 'issued-link',
        branchId: 'branch-a',
        purpose: 'NewPatientWaitingRoomRegistration',
        accessToken: 'one-time-token',
        createdAtUtc: '2026-07-26T10:00:00Z',
        expiresAtUtc: '2026-07-26T10:30:00Z'
      })),
      revoke: vi.fn().mockReturnValue(of(void 0))
    } as unknown as PatientIntakeLinksApi & {
      list: ReturnType<typeof vi.fn>;
      issue: ReturnType<typeof vi.fn>;
      revoke: ReturnType<typeof vi.fn>;
    };
  }
});
