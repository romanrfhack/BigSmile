import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UI_LANGUAGE_STORAGE_KEY } from '../../core/i18n';
import { LanguageSelectorComponent } from './language-selector.component';

describe('LanguageSelectorComponent', () => {
  let fixture: ComponentFixture<LanguageSelectorComponent>;

  beforeEach(async () => {
    window.localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [LanguageSelectorComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(LanguageSelectorComponent);
    fixture.detectChanges();
  });

  afterEach(() => {
    window.localStorage.clear();
  });

  it('renders Spanish as the default UI language', () => {
    expect(fixture.nativeElement.textContent).toContain('Idioma');
  });

  it('switches to English and persists the non-sensitive preference', () => {
    fixture.componentInstance.changeLanguage('en-US');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Language');
    expect(window.localStorage.getItem(UI_LANGUAGE_STORAGE_KEY)).toBe('en-US');
  });
});
