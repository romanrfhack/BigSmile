import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { OdontogramToothState } from '../models/odontogram.models';

@Component({
  selector: 'app-odontogram-grid',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="grid-shell">
      <button
        *ngFor="let tooth of teeth"
        type="button"
        class="tooth-card"
        [class.is-selected]="tooth.toothCode === selectedToothCode"
        [class.status-healthy]="tooth.status === 'Healthy'"
        [class.status-missing]="tooth.status === 'Missing'"
        [class.status-restored]="tooth.status === 'Restored'"
        [class.status-caries]="tooth.status === 'Caries'"
        (click)="toothSelected.emit(tooth.toothCode)">
        <span class="tooth-code">{{ tooth.toothCode }}</span>
        <span class="tooth-status">{{ tooth.status }}</span>
      </button>
    </section>
  `,
  styles: [`
    .grid-shell {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(110px, 1fr));
      gap: 0.75rem;
    }

    .tooth-card {
      border-radius: 18px;
      border: 1px solid #d6dfeb;
      background: #ffffff;
      padding: 0.85rem;
      display: grid;
      gap: 0.35rem;
      text-align: left;
      cursor: pointer;
      transition: border-color 120ms ease, transform 120ms ease, box-shadow 120ms ease;
    }

    .tooth-card:hover {
      border-color: #7aa8d9;
      transform: translateY(-1px);
      box-shadow: 0 10px 20px rgba(20, 48, 79, 0.08);
    }

    .is-selected {
      border-color: #0a5bb5;
      box-shadow: 0 0 0 2px rgba(10, 91, 181, 0.12);
    }

    .tooth-code {
      color: #16324f;
      font-size: 1rem;
      font-weight: 800;
    }

    .tooth-status {
      color: #5b6e84;
      font-size: 0.9rem;
      font-weight: 600;
    }

    .status-healthy {
      background: #eef8f0;
    }

    .status-missing {
      background: #fff2f2;
    }

    .status-restored {
      background: #eef4ff;
    }

    .status-caries {
      background: #fff4e8;
    }
  `]
})
export class OdontogramGridComponent {
  @Input() teeth: OdontogramToothState[] = [];
  @Input() selectedToothCode: string | null = null;

  @Output() readonly toothSelected = new EventEmitter<string>();
}
