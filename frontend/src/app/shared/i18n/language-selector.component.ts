import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { I18nService, SupportedLanguageCode } from '../../core/i18n';
import { TranslatePipe } from './translate.pipe';

@Component({
  selector: 'app-language-selector',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  template: `
    <label class="language-selector">
      <span>{{ 'Language' | t }}</span>
      <select
        name="language"
        [ngModel]="i18n.currentLanguage()"
        (ngModelChange)="changeLanguage($event)"
        [attr.aria-label]="'Language' | t">
        <option *ngFor="let language of i18n.supportedLanguages" [ngValue]="language.code">
          {{ language.label }}
        </option>
      </select>
    </label>
  `,
  styles: [`
    .language-selector {
      display: grid;
      gap: 0.3rem;
      color: var(--bsm-color-text-muted);
      font-weight: 700;
      font-size: 0.86rem;
    }

    .language-selector span {
      color: var(--bsm-color-text-muted);
      font-size: 0.75rem;
      text-transform: uppercase;
    }

    .language-selector select {
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      padding: 0.55rem 0.7rem;
      background: var(--bsm-color-bg);
      color: var(--bsm-color-text-brand);
      font: inherit;
      transition:
        border-color var(--bsm-motion-fast) var(--bsm-ease-standard),
        box-shadow var(--bsm-motion-fast) var(--bsm-ease-standard);
    }

    .language-selector select:focus {
      outline: none;
      border-color: var(--bsm-color-accent-accessible);
      box-shadow: var(--bsm-shadow-focus);
    }
  `]
})
export class LanguageSelectorComponent {
  readonly i18n = inject(I18nService);

  changeLanguage(languageCode: SupportedLanguageCode): void {
    this.i18n.setLanguage(languageCode);
  }
}
