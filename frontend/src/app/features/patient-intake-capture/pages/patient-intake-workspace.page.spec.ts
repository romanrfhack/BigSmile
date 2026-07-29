import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { PatientIntakeWorkspaceFacade } from '../facades/patient-intake-workspace.facade';
import { PatientIntakeWorkspacePageComponent } from './patient-intake-workspace.page';

describe('PatientIntakeWorkspacePageComponent lifecycle protection', () => {
  let fixture: ComponentFixture<PatientIntakeWorkspacePageComponent>;
  let facade: Record<string, ReturnType<typeof vi.fn>>;
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    facade = {
      initialize: vi.fn(),
      status: vi.fn().mockReturnValue('ready'),
      mode: vi.fn().mockReturnValue('patient'),
      intake: vi.fn().mockReturnValue(null),
      error: vi.fn().mockReturnValue(null),
      creating: vi.fn().mockReturnValue(false),
      saving: vi.fn().mockReturnValue(false),
      saveOutcome: vi.fn().mockReturnValue(null),
      saveError: vi.fn().mockReturnValue(null),
      saveTarget: vi.fn().mockReturnValue(null),
      blockingState: vi.fn().mockReturnValue(null),
      recoveryState: vi.fn().mockReturnValue(null),
      saveBlocked: vi.fn().mockReturnValue(false),
      canCreate: vi.fn().mockReturnValue(false),
      canReplaceExpired: vi.fn().mockReturnValue(false),
      createDraft: vi.fn(),
      reload: vi.fn(),
      reloadLatest: vi.fn(),
      replaceExpiredDraft: vi.fn(),
      saveNonMedicalDraft: vi.fn(),
      saveMedicalDraft: vi.fn(),
      logout: vi.fn().mockReturnValue(of(void 0))
    };
    router = { navigate: vi.fn().mockResolvedValue(true) };

    await TestBed.configureTestingModule({
      imports: [PatientIntakeWorkspacePageComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ tenantSubdomain: 'clinic-a' }) } }
        },
        { provide: Router, useValue: router }
      ]
    })
      .overrideComponent(PatientIntakeWorkspacePageComponent, {
        set: {
          providers: [{ provide: PatientIntakeWorkspaceFacade, useValue: facade }]
        }
      })
      .compileComponents();

    fixture = TestBed.createComponent(PatientIntakeWorkspacePageComponent);
  });

  afterEach(() => {
    window.localStorage.clear();
    vi.restoreAllMocks();
  });

  it('warns on route navigation while either section is dirty', () => {
    const component = fixture.componentInstance;
    setDirty(component, true, false);
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    expect(component.canDeactivate()).toBe(false);
    expect(window.confirm).toHaveBeenCalledTimes(1);
  });

  it('allows route navigation without warning after authoritative reset', () => {
    const component = fixture.componentInstance;
    setDirty(component, false, false);
    const confirm = vi.spyOn(window, 'confirm');

    expect(component.canDeactivate()).toBe(true);
    expect(confirm).not.toHaveBeenCalled();
  });

  it('registers beforeunload protection without persisting form values', () => {
    const component = fixture.componentInstance;
    setDirty(component, false, true);
    const event = new Event('beforeunload', { cancelable: true }) as BeforeUnloadEvent;

    component.onBeforeUnload(event);

    expect(event.defaultPrevented).toBe(true);
    expect(window.sessionStorage.length).toBe(0);
    expect(window.localStorage.getItem('bigsmile.ui.language')).toBe('en-US');
  });

  it('does not logout when the patient rejects discarding unsaved changes', () => {
    const component = fixture.componentInstance;
    setDirty(component, true, true);
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.logout();

    expect(facade['logout']).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('reloads a conflict only after explicit discard confirmation', () => {
    const component = fixture.componentInstance;
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.reloadLatest();

    expect(facade['reloadLatest']).toHaveBeenCalledTimes(1);
  });
});

function setDirty(
  component: PatientIntakeWorkspacePageComponent,
  demographicsDirty: boolean,
  medicalDirty: boolean
): void {
  (component as unknown as {
    demographicsForm: { form: { dirty: boolean } };
    medicalQuestionnaire: { form: { dirty: boolean } };
  }).demographicsForm = { form: { dirty: demographicsDirty } };
  (component as unknown as {
    medicalQuestionnaire: { form: { dirty: boolean } };
  }).medicalQuestionnaire = { form: { dirty: medicalDirty } };
}
