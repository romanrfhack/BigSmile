import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  SimpleChanges,
  ViewChild,
  inject
} from '@angular/core';
import { LocalQrCodeService } from '../services/local-qr-code.service';

@Component({
  selector: 'app-local-qr-code',
  standalone: true,
  template: '<div #qrHost class="qr-host" aria-live="polite"></div>',
  styles: [`
    :host {
      display: block;
      width: min(100%, 18rem);
      aspect-ratio: 1;
    }

    .qr-host {
      display: grid;
      place-items: center;
      width: 100%;
      height: 100%;
      padding: 0.75rem;
      border: 1px solid var(--bsm-color-border);
      border-radius: var(--bsm-radius-md);
      background: #ffffff;
    }
  `]
})
export class LocalQrCodeComponent implements AfterViewInit, OnChanges {
  private readonly qrCodeService = inject(LocalQrCodeService);

  @Input({ required: true }) value = '';
  @ViewChild('qrHost', { static: true }) private qrHost?: ElementRef<HTMLDivElement>;

  ngAfterViewInit(): void {
    this.render();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['value'] && !changes['value'].firstChange) {
      this.render();
    }
  }

  private render(): void {
    const host = this.qrHost?.nativeElement;
    if (!host) {
      return;
    }

    host.replaceChildren();
    if (!this.value?.trim()) {
      return;
    }

    host.appendChild(this.qrCodeService.createSvg(this.value));
  }
}
