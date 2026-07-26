import { PatientIntakeLinkBrowserActions } from './patient-intake-link-browser-actions.service';

describe('PatientIntakeLinkBrowserActions', () => {
  afterEach(() => {
    document.body.classList.remove('bsm-printing-waiting-room-handoff');
    vi.restoreAllMocks();
  });

  it('copies the exact handoff URL through the browser clipboard only', async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(window.navigator, 'clipboard', {
      configurable: true,
      value: { writeText }
    });
    const service = new PatientIntakeLinkBrowserActions(document);

    await service.copyText('https://clinic.test/patient-portal/intake-activate#token=secret');

    expect(writeText).toHaveBeenCalledWith(
      'https://clinic.test/patient-portal/intake-activate#token=secret'
    );
  });

  it('scopes printing to the handoff and always removes the print marker', () => {
    const print = vi.spyOn(window, 'print').mockImplementation(() => {
      expect(document.body.classList.contains('bsm-printing-waiting-room-handoff')).toBe(true);
    });
    const service = new PatientIntakeLinkBrowserActions(document);

    service.printCurrentHandoff();

    expect(print).toHaveBeenCalledTimes(1);
    expect(document.body.classList.contains('bsm-printing-waiting-room-handoff')).toBe(false);
  });

  it('requires explicit confirmation before revoke orchestration continues', () => {
    const confirm = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const service = new PatientIntakeLinkBrowserActions(document);

    expect(service.confirmRevoke()).toBe(true);
    expect(confirm).toHaveBeenCalledWith(
      '¿Revocar este enlace de sala de espera? Ya no podrá utilizarse.'
    );
  });
});
