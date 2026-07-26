import { DOCUMENT } from '@angular/common';
import { Inject, Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class PatientIntakeLinkBrowserActions {
  constructor(@Inject(DOCUMENT) private readonly document: Document) {}

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
      '¿Revocar este enlace de sala de espera? Ya no podrá utilizarse.'
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
