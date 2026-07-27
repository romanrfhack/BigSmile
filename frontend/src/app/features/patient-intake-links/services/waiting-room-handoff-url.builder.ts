import { DOCUMENT, Location } from '@angular/common';
import { Inject, Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class WaitingRoomHandoffUrlBuilder {
  constructor(
    private readonly location: Location,
    @Inject(DOCUMENT) private readonly document: Document
  ) {}

  build(accessToken: string): string {
    const normalizedToken = accessToken?.trim();
    if (!normalizedToken) {
      throw new Error('A one-time waiting-room token is required.');
    }

    const origin = this.document.defaultView?.location.origin;
    if (!origin) {
      throw new Error('The application origin is not available.');
    }

    const externalPath = this.location.prepareExternalUrl('/patient-portal/intake-activate');
    const normalizedPath = externalPath.startsWith('/') ? externalPath : `/${externalPath}`;
    return `${origin}${normalizedPath}#token=${encodeURIComponent(normalizedToken)}`;
  }
}
