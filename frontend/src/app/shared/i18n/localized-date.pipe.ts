import { formatDate, registerLocaleData } from '@angular/common';
import localeEsMx from '@angular/common/locales/es-MX';
import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18nService } from '../../core/i18n';

registerLocaleData(localeEsMx, 'es-MX');

@Pipe({
  name: 'bsDate',
  standalone: true,
  pure: false
})
export class LocalizedDatePipe implements PipeTransform {
  private readonly i18n = inject(I18nService);

  transform(value: string | number | Date | null | undefined, format = 'medium'): string {
    if (value === null || value === undefined || value === '') {
      return '';
    }

    try {
      return formatDate(value, format, this.i18n.currentLanguage());
    } catch {
      return `${value}`;
    }
  }
}
