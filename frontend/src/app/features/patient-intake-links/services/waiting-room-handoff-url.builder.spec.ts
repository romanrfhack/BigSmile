import { DOCUMENT, Location } from '@angular/common';
import { TestBed } from '@angular/core/testing';
import { WaitingRoomHandoffUrlBuilder } from './waiting-room-handoff-url.builder';

describe('WaitingRoomHandoffUrlBuilder', () => {
  it('places the one-time token only in the URL fragment', () => {
    const location = {
      prepareExternalUrl: vi.fn().mockReturnValue('/patient-portal/intake-activate')
    };

    TestBed.configureTestingModule({
      providers: [
        WaitingRoomHandoffUrlBuilder,
        { provide: Location, useValue: location }
      ]
    });

    const service = TestBed.inject(WaitingRoomHandoffUrlBuilder);
    const document = TestBed.inject(DOCUMENT);
    const url = service.build('abc_def-123');

    expect(url).toBe(
      `${document.defaultView?.location.origin}/patient-portal/intake-activate#token=abc_def-123`
    );
    expect(url.split('#')[0]).not.toContain('abc_def-123');
    expect(url).not.toContain('?token=');
  });

  it('rejects an empty one-time token', () => {
    TestBed.configureTestingModule({
      providers: [
        WaitingRoomHandoffUrlBuilder,
        { provide: Location, useValue: { prepareExternalUrl: () => '/patient-portal/intake-activate' } }
      ]
    });

    expect(() => TestBed.inject(WaitingRoomHandoffUrlBuilder).build('  ')).toThrow();
  });
});
