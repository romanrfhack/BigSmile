import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { EmptyStateComponent } from './empty-state.component';
import { LoadingSkeletonComponent } from './loading-skeleton.component';
import { PageHeaderComponent } from './page-header.component';
import { SectionCardComponent } from './section-card.component';
import { StatusBadgeComponent } from './status-badge.component';
import { StickyActionBarComponent } from './sticky-action-bar.component';
import { SummaryCardComponent } from './summary-card.component';

@Component({
  standalone: true,
  imports: [PageHeaderComponent],
  template: `
    <app-page-header
      title="Patients"
      subtitle="Fast patient registration"
      eyebrow="Operations">
      <button page-header-actions type="button">New patient</button>
    </app-page-header>
  `
})
class PageHeaderHostComponent {}

@Component({
  standalone: true,
  imports: [SectionCardComponent],
  template: `
    <app-section-card title="Clinical summary" subtitle="Current patient context" variant="accent">
      <button section-card-actions type="button">Refresh</button>
      <p>Projected clinical content</p>
    </app-section-card>
  `
})
class SectionCardHostComponent {}

@Component({
  standalone: true,
  imports: [EmptyStateComponent],
  template: `
    <app-empty-state
      title="No documents"
      description="Upload the first active patient document."
      icon="D">
      <button empty-state-action type="button">Upload</button>
    </app-empty-state>
  `
})
class EmptyStateHostComponent {}

@Component({
  standalone: true,
  imports: [StickyActionBarComponent],
  template: `
    <app-sticky-action-bar [ariaLabel]="'Treatment form actions'">
      <button type="button">Cancel</button>
      <button type="submit">Save</button>
    </app-sticky-action-bar>
  `
})
class StickyActionBarHostComponent {}

describe('Shared UI components', () => {
  it('renders PageHeader text and projected actions', async () => {
    await TestBed.configureTestingModule({
      imports: [PageHeaderHostComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(PageHeaderHostComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Operations');
    expect(text).toContain('Patients');
    expect(text).toContain('Fast patient registration');
    expect(text).toContain('New patient');
  });

  it('renders SectionCard content and applies the selected variant', async () => {
    await TestBed.configureTestingModule({
      imports: [SectionCardHostComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(SectionCardHostComponent);
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.bsm-section-card') as HTMLElement;
    const text = fixture.nativeElement.textContent as string;

    expect(card.getAttribute('data-variant')).toBe('accent');
    expect(text).toContain('Clinical summary');
    expect(text).toContain('Current patient context');
    expect(text).toContain('Refresh');
    expect(text).toContain('Projected clinical content');
  });

  it('renders SummaryCard content and applies tone', async () => {
    await TestBed.configureTestingModule({
      imports: [SummaryCardComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(SummaryCardComponent);
    fixture.componentInstance.label = 'Confirmed';
    fixture.componentInstance.value = 8;
    fixture.componentInstance.helper = 'Appointments confirmed today';
    fixture.componentInstance.tone = 'success';
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.bsm-summary-card') as HTMLElement;
    const text = fixture.nativeElement.textContent as string;

    expect(card.getAttribute('data-tone')).toBe('success');
    expect(card.getAttribute('aria-label')).toContain('Confirmed: 8');
    expect(text).toContain('Confirmed');
    expect(text).toContain('8');
    expect(text).toContain('Appointments confirmed today');
  });

  it('renders StatusBadge with label, tone, and size', async () => {
    await TestBed.configureTestingModule({
      imports: [StatusBadgeComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(StatusBadgeComponent);
    fixture.componentInstance.label = 'Pending';
    fixture.componentInstance.tone = 'warning';
    fixture.componentInstance.size = 'sm';
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.bsm-status-badge') as HTMLElement;

    expect(badge.textContent).toContain('Pending');
    expect(badge.getAttribute('data-tone')).toBe('warning');
    expect(badge.getAttribute('data-size')).toBe('sm');
    expect(badge.getAttribute('aria-label')).toBe('Pending');
  });

  it('renders EmptyState copy and projected action', async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyStateHostComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(EmptyStateHostComponent);
    fixture.detectChanges();

    const state = fixture.nativeElement.querySelector('.bsm-empty-state') as HTMLElement;
    const text = fixture.nativeElement.textContent as string;

    expect(state.getAttribute('aria-live')).toBe('polite');
    expect(text).toContain('No documents');
    expect(text).toContain('Upload the first active patient document.');
    expect(text).toContain('Upload');
  });

  it('renders LoadingSkeleton with the requested variant and accessible status', async () => {
    await TestBed.configureTestingModule({
      imports: [LoadingSkeletonComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(LoadingSkeletonComponent);
    fixture.componentInstance.variant = 'table-row';
    fixture.componentInstance.ariaLabel = 'Loading appointments';
    fixture.detectChanges();

    const skeleton = fixture.nativeElement.querySelector('.bsm-loading-skeleton') as HTMLElement;
    const cells = fixture.nativeElement.querySelectorAll('.bsm-loading-skeleton__block--cell');

    expect(skeleton.getAttribute('data-variant')).toBe('table-row');
    expect(skeleton.getAttribute('role')).toBe('status');
    expect(skeleton.getAttribute('aria-label')).toBe('Loading appointments');
    expect(cells.length).toBe(4);
  });

  it('renders StickyActionBar projected actions with an accessible region label', async () => {
    await TestBed.configureTestingModule({
      imports: [StickyActionBarHostComponent]
    }).compileComponents();

    const fixture = TestBed.createComponent(StickyActionBarHostComponent);
    fixture.detectChanges();

    const bar = fixture.nativeElement.querySelector('.bsm-sticky-action-bar') as HTMLElement;
    const text = fixture.nativeElement.textContent as string;

    expect(bar.getAttribute('role')).toBe('region');
    expect(bar.getAttribute('aria-label')).toBe('Treatment form actions');
    expect(text).toContain('Cancel');
    expect(text).toContain('Save');
  });
});
