import { DOCUMENT } from '@angular/common';
import { Inject, Injectable } from '@angular/core';
import { I18nService } from '../../../core/i18n';

@Injectable({ providedIn: 'root' })
export class PatientIntakeLinkBrowserActions {
  constructor(
    @Inject(DOCUMENT) private readonly document: Document,
    private readonly i18n: I18nService
  ) {}

  async copyText(value: string): Promise<void> {
    const normalizedValue = value?.trim();
    if (!normalizedValue) {
      throw new Error('There is no waiting-room link to copy.');
    }

    const clipboard = this.document.defaultView?.navigator.clipboard;
    if (!clipboard?.writeText) {
      throw new Error('Clipboard access is not available in this browser.');
    }

    await clipboard.writeText(normalizedValue);
  }

  confirmRevoke(): boolean {
    return this.document.defaultView?.confirm(
      this.i18n.translate('Revoke this waiting-room link? It can no longer be used.')
    ) ?? false;
  }

  printCurrentHandoff(): void {
    const currentWindow = this.document.defaultView;
    if (!currentWindow) {
      throw new Error('Print is not available in this browser.');
    }

    this.document.body.classList.add('bsm-printing-waiting-room-handoff');
    try {
      currentWindow.print();
    } finally {
      this.document.body.classList.remove('bsm-printing-waiting-room-handoff');
    }
  }
}
