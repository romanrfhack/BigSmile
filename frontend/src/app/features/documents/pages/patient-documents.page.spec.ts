import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { AuthService } from '../../../core/auth/auth.service';
import { PatientsFacade } from '../../patients/facades/patients.facade';
import { PatientDocumentsFacade } from '../facades/patient-documents.facade';
import { PatientDocumentsPageComponent } from './patient-documents.page';

describe('PatientDocumentsPageComponent', () => {
  let patientDocumentsFacade: any;
  let uploadCalls: Array<{ patientId: string; fileName: string }>;
  let downloadCalls: string[];
  let retireCalls: string[];

  beforeEach(async () => {
    window.localStorage.setItem('bigsmile.ui.language', 'en-US');
    uploadCalls = [];
    downloadCalls = [];
    retireCalls = [];

    patientDocumentsFacade = {
      documents: signal<any[]>([]),
      loadingDocuments: signal(false),
      documentsError: signal<string | null>(null),
      clearDocuments: () => undefined,
      loadDocuments: () => undefined,
      uploadDocument: (patientId: string, file: File) => {
        uploadCalls.push({ patientId, fileName: file.name });
        patientDocumentsFacade.documents.set([buildDocument(patientId)]);
        return of(buildDocument(patientId));
      },
      downloadDocument: (_patientId: string, documentId: string) => {
        downloadCalls.push(documentId);
        return of(new Blob(['pdf'], { type: 'application/pdf' }));
      },
      retireDocument: (patientId: string, documentId: string) => {
        retireCalls.push(`${patientId}:${documentId}`);
        patientDocumentsFacade.documents.set([]);
        return of(undefined);
      }
    };

    if (!(URL as any).createObjectURL) {
      (URL as any).createObjectURL = vi.fn(() => 'blob:mock');
    }

    if (!(URL as any).revokeObjectURL) {
      (URL as any).revokeObjectURL = vi.fn(() => undefined);
    }

    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock');
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => undefined);
    vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined);

    await TestBed.configureTestingModule({
      imports: [PatientDocumentsPageComponent],
      providers: [
        {
          provide: PatientDocumentsFacade,
          useValue: patientDocumentsFacade
        },
        {
          provide: PatientsFacade,
          useValue: {
            currentPatient: signal({
              id: 'patient-1',
              fullName: 'Ana Lopez',
              dateOfBirth: '1991-02-14',
              primaryPhone: null,
              email: null,
              isActive: true,
              hasClinicalAlerts: false,
              clinicalAlertsSummary: null,
              responsibleParty: null,
              createdAt: '2026-04-10T10:00:00Z',
              updatedAt: null
            }),
            loadingPatient: signal(false),
            detailError: signal<string | null>(null),
            clearCurrentPatient: () => undefined,
            loadPatient: () => undefined
          }
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => 'patient-1'
              }
            }
          }
        },
        {
          provide: AuthService,
          useValue: {
            hasPermissions: () => true
          }
        }
      ]
    }).compileComponents();
  });

  it('renders the empty state when there are no active documents', () => {
    const fixture = TestBed.createComponent(PatientDocumentsPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No active documents yet');
    expect(fixture.nativeElement.textContent).toContain('Documents are never auto-created in this slice');
  });

  it('uploads a document through the explicit upload flow', () => {
    const fixture = TestBed.createComponent(PatientDocumentsPageComponent);
    fixture.detectChanges();

    const input = fixture.nativeElement.querySelector('input[type="file"]') as HTMLInputElement;
    const file = new File(['pdf'], 'radiography.pdf', { type: 'application/pdf' });
    Object.defineProperty(input, 'files', {
      configurable: true,
      value: {
        item: (index: number) => index === 0 ? file : null,
        length: 1,
        0: file
      }
    });
    input.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    const uploadButton = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Upload document')) as HTMLButtonElement;
    uploadButton.click();
    fixture.detectChanges();

    expect(uploadCalls).toEqual([{ patientId: 'patient-1', fileName: 'radiography.pdf' }]);
    expect(fixture.nativeElement.textContent).toContain('radiography.pdf');
  });

  it('renders the active documents list', () => {
    patientDocumentsFacade.documents.set([buildDocument('patient-1')]);

    const fixture = TestBed.createComponent(PatientDocumentsPageComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Active documents');
    expect(fixture.nativeElement.textContent).toContain('radiography.pdf');
  });

  it('downloads a document through the authorized action', () => {
    patientDocumentsFacade.documents.set([buildDocument('patient-1')]);

    const fixture = TestBed.createComponent(PatientDocumentsPageComponent);
    fixture.detectChanges();

    const downloadButton = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Download')) as HTMLButtonElement;
    downloadButton.click();
    fixture.detectChanges();

    expect(downloadCalls).toEqual(['document-1']);
    expect(URL.createObjectURL).toHaveBeenCalled();
  });

  it('retires a document and removes it from the active list', () => {
    patientDocumentsFacade.documents.set([buildDocument('patient-1')]);

    const fixture = TestBed.createComponent(PatientDocumentsPageComponent);
    fixture.detectChanges();

    const retireButton = Array.from(fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>)
      .find((entry) => entry.textContent?.includes('Retire')) as HTMLButtonElement;
    retireButton.click();
    fixture.detectChanges();

    expect(retireCalls).toEqual(['patient-1:document-1']);
    expect(fixture.nativeElement.textContent).not.toContain('radiography.pdf');
    expect(fixture.nativeElement.textContent).toContain('No active documents yet');
  });

  function buildDocument(patientId: string) {
    return {
      documentId: 'document-1',
      patientId,
      originalFileName: 'radiography.pdf',
      contentType: 'application/pdf',
      sizeBytes: 2048,
      uploadedAtUtc: '2026-04-22T08:30:00Z',
      uploadedByUserId: 'user-1'
    };
  }
});
