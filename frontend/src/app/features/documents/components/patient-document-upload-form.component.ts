import { CommonModule } from '@angular/common';
import { Component, ElementRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild } from '@angular/core';

@Component({
  selector: 'app-patient-document-upload-form',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="upload-card">
      <div>
        <h3>Upload document</h3>
        <p>Allowed types: PDF, JPG, PNG. Maximum size: 10 MB. Files stay private and are only available through authorized API download.</p>
      </div>

      <label class="file-picker">
        <span>Select file</span>
        <input
          #fileInput
          type="file"
          accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
          (change)="onFileSelected($event)">
      </label>

      <div *ngIf="selectedFile" class="selected-file">
        <strong>{{ selectedFile.name }}</strong>
        <span>{{ formatSize(selectedFile.size) }}</span>
      </div>

      <p *ngIf="error" class="error">{{ error }}</p>

      <button type="button" (click)="submit()" [disabled]="!selectedFile || saving">
        {{ saving ? 'Uploading...' : 'Upload document' }}
      </button>
    </section>
  `,
  styles: [`
    .upload-card {
      border-radius: 20px;
      border: 1px solid #d7dfe8;
      background: linear-gradient(180deg, #ffffff 0%, #f5f9fc 100%);
      padding: 1.4rem 1.5rem;
      box-shadow: 0 20px 36px rgba(20, 48, 79, 0.08);
      display: grid;
      gap: 1rem;
    }

    h3 {
      margin: 0;
      color: #16324f;
    }

    p {
      margin: 0.45rem 0 0;
      color: #5b6e84;
      max-width: 62ch;
    }

    .file-picker {
      display: inline-flex;
      align-items: center;
      gap: 0.85rem;
      color: #16324f;
      font-weight: 700;
      cursor: pointer;
    }

    .file-picker input {
      max-width: 100%;
    }

    .selected-file {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
      color: #16324f;
    }

    .selected-file span {
      color: #5b6e84;
      font-weight: 600;
    }

    .error {
      color: #8c2525;
      margin: 0;
    }

    button {
      width: fit-content;
      border: 0;
      border-radius: 999px;
      padding: 0.8rem 1.25rem;
      background: #0a5bb5;
      color: #ffffff;
      font-weight: 700;
      cursor: pointer;
    }

    button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  `]
})
export class PatientDocumentUploadFormComponent implements OnChanges {
  @Input() saving = false;
  @Input() error: string | null = null;
  @Input() revision = 0;

  @Output() uploadRequested = new EventEmitter<File>();

  @ViewChild('fileInput')
  private fileInput?: ElementRef<HTMLInputElement>;

  selectedFile: File | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['revision'] && !changes['revision'].firstChange) {
      this.clearSelection();
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.item(0) ?? null;
  }

  submit(): void {
    if (!this.selectedFile) {
      return;
    }

    this.uploadRequested.emit(this.selectedFile);
  }

  formatSize(sizeBytes: number): string {
    if (sizeBytes >= 1024 * 1024) {
      return `${(sizeBytes / (1024 * 1024)).toFixed(1)} MB`;
    }

    return `${Math.max(1, Math.round(sizeBytes / 1024))} KB`;
  }

  private clearSelection(): void {
    this.selectedFile = null;
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }
}
