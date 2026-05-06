# BigSmile Frontend UX/UI Operational Guidelines

## Purpose

This guide defines the operational UX/UI conventions for the progressive redesign of the BigSmile frontend.

Its purpose is to make BigSmile more attractive, clearer, and more dynamic without weakening its main product constraint: fast operational work for dental clinics.

This guide applies before redesigning screens one by one. It does not change backend behavior, API contracts, tenant isolation, branch scope, authentication, authorization, permissions, migrations, or seeds.

## Operational UX Principles

- Operational speed is a product feature.
- Primary actions must be visible without unnecessary scrolling.
- Screens should support repeated clinic work, not marketing-style exploration.
- Layouts must make patient, appointment, clinical, treatment, billing, and document context easy to scan.
- Visual richness should come from clear hierarchy, useful color, spacing, surfaces, badges, and focused microinteractions.
- The UI must stay accessible, keyboard usable, and predictable on desktop, tablet, and mobile.
- Redesign must be progressive by slice; avoid isolated one-off patterns that make screens inconsistent.

## Reducing Excessive Scroll

- Keep primary actions in the first viewport on desktop and tablet.
- Use compact page headers instead of tall hero blocks for operational screens.
- Use summary strips for high-value status and metrics instead of large vertical cards.
- Use tabs to separate parallel sections that do not need to be read at the same time.
- Use drawers for contextual secondary detail that supports the current workflow.
- Use modals only for focused create/edit/confirm flows that temporarily block the page.
- Use sticky action bars for long forms or long editable flows so save/cancel/submit actions remain reachable.
- Avoid stacking many full-width cards when a compact grid, table, tab, drawer, or split layout would reduce scanning distance.
- Avoid burying destructive or completion actions below long read-only content.

## Standard Operational Screen Layout

Most operational pages should use this structure:

1. Compact page header with title, patient/module context, status badges, and primary action.
2. Optional summary strip with 3 to 6 dense operational facts.
3. Main work area using a grid, split panel, table, calendar, or focused form.
4. Secondary context in tabs, drawers, side panels, or below-the-fold sections.
5. Sticky action bar when the screen contains long editable content.

Use this as a pattern, not as a rigid template. Dense workflows such as scheduling, odontogram, and billing may need specialized main work areas.

## Page Headers

- Keep headers compact: title, short subtitle, key badges, and 1 primary action.
- Use the module or patient name as the strongest signal.
- Put secondary commands in an overflow area, secondary button group, drawer trigger, or tab-specific toolbar.
- Avoid full marketing hero sections inside authenticated operational screens.
- Do not use large decorative gradients when they push operational content below the fold.

## Summary Strips

- Use summary strips for critical status, counts, next appointment, active alerts, outstanding amount, plan status, quote status, or document count.
- Keep summary items compact and scan-friendly.
- Prefer one horizontal strip on desktop and a two-column or horizontally scrollable compact strip on mobile.
- Use badges and concise labels; avoid paragraph text.
- Summary strips should not replace detailed sections when detail is required for decisions.

## Tabs

- Use tabs for peer sections inside the same context, such as patient profile sections, clinical notes/diagnoses/history, treatment plan/quote, or document categories.
- Do not use tabs to hide the primary action for the current workflow.
- Keep tab labels short and stable.
- Preserve keyboard navigation and visible active state.
- Avoid deeply nested tab sets.

## Drawers

- Use drawers for contextual detail, filters, preview, appointment detail, reminder helper content, document metadata, or supporting lists.
- Drawers should preserve the user's position in the main workflow.
- Use right-side drawers on desktop and full-screen or bottom-sheet style drawers on mobile.
- Drawers must have clear close controls, focus management, and keyboard escape behavior when implemented.
- Do not place critical irreversible actions only inside a hidden drawer.

## Modals

- Use modals for bounded create/edit/confirm tasks that require a clear commit or cancel action.
- Keep modal content focused; if the flow needs multiple sections, consider a page, drawer, or tabbed area instead.
- Modals must trap focus, restore focus on close, expose an accessible name, and support keyboard dismissal when safe.
- Destructive actions must require explicit user intent and must not rely on color alone.

## Sticky Action Bars

- Use sticky action bars for long forms, clinical entry, treatment plan editing, quote pricing, billing issue flows, or document upload flows when actions could otherwise fall out of view.
- Show the primary action, secondary cancel/back action, and relevant dirty/error status.
- Keep sticky bars visually lightweight with white or surface background, border, and subtle shadow.
- On mobile, place sticky actions at the bottom with touch-friendly spacing.

## Visual Tokens

Use the existing `--bsm-*` tokens as the visual base:

- Brand: `--bsm-color-primary`, `--bsm-color-primary-dark`, `--bsm-color-primary-soft`
- Accent: `--bsm-color-accent`, `--bsm-color-accent-accessible`, `--bsm-color-accent-dark`, `--bsm-color-accent-soft`
- Supporting color: `--bsm-color-sky`, `--bsm-color-lavender`
- Backgrounds: `--bsm-color-bg`, `--bsm-color-surface`
- Borders and text: `--bsm-color-border`, `--bsm-color-text`, `--bsm-color-text-brand`, `--bsm-color-text-muted`
- Gradients: `--bsm-gradient-brand`, `--bsm-gradient-surface`
- Shadows: `--bsm-shadow-sm`, `--bsm-shadow-md`, `--bsm-shadow-focus`
- Radius: `--bsm-radius-sm`, `--bsm-radius-md`, `--bsm-radius-lg`, `--bsm-radius-pill`
- Motion: `--bsm-motion-fast`, `--bsm-motion-normal`, `--bsm-motion-slow`, `--bsm-ease-standard`

When new reusable visual needs appear, add or extend tokens deliberately instead of hardcoding isolated colors across screens.

## Color Rules

- White should remain the predominant background for operational clarity.
- Use `--bsm-color-surface` and `--bsm-gradient-surface` for subtle separation, not heavy decoration.
- Use purple for brand identity, primary actions, selected states, and high-level emphasis.
- Use cyan for helpful accents, focus states, links, secondary highlights, and active operational signals.
- Use badges for statuses such as active/inactive, pending/confirmed, draft/proposed/accepted, issued, due, completed, and alerts.
- Badges must combine color with text or icons; do not rely on color alone.
- Use soft gradients sparingly for page headers, empty states, and highlight surfaces; avoid large gradients that dominate the screen.
- Visual states must be consistent: hover, active, selected, disabled, loading, success, warning, danger, and error.
- Danger/destructive states must be visually distinct from brand purple and cyan.

## Animation Rules

- Use microinteractions only when they clarify state: hover lift, focus ring, drawer transition, modal entrance, tab underline, loading shimmer, or successful save feedback.
- Keep animations short and sober, usually within `--bsm-motion-fast` or `--bsm-motion-normal`.
- Do not use scroll-jacking.
- Do not use infinite decorative animations.
- Do not animate core layout in a way that causes reading or data-entry friction.
- Always respect `prefers-reduced-motion`.

## Responsive Rules

### Desktop

- Prefer dense but readable layouts with two-column or split-panel work areas when useful.
- Keep primary actions in the header or sticky local toolbar.
- Use side drawers and side panels for supporting detail.
- Tables, calendars, and odontogram work areas should use the available width.

### Tablet

- Collapse secondary panels below or into drawers.
- Preserve primary actions in the first viewport.
- Avoid cramped three-column layouts.
- Use tabs or segmented controls to reduce vertical stacking.

### Mobile

- Use single-column layouts.
- Convert summary strips to compact wrapping items or horizontal scroll.
- Place primary actions in sticky bottom bars when form length requires it.
- Use full-screen drawers/modals for complex focused tasks.
- Avoid dense tables; use cards or row groups with the same information hierarchy.

## Accessibility Rules

- All interactive elements must have visible focus states using `--bsm-shadow-focus` or an equivalent tokenized focus treatment.
- Text and controls must meet contrast expectations against their backgrounds.
- Keyboard users must be able to reach, operate, and exit tabs, drawers, modals, menus, forms, calendars, and upload controls.
- Do not communicate status by color alone; pair color with labels, icons, or text.
- Modals and drawers must manage focus correctly when implemented.
- Form fields must have labels, useful validation messages, and clear error association.
- Loading and error states must be perceivable by assistive technology when relevant.
- Touch targets should remain comfortable on mobile and tablet.

## Forms

- Group fields by operational meaning, not by backend DTO shape.
- Keep common workflows short; move rare or advanced fields into secondary sections.
- Show inline validation near the field and a concise summary when needed.
- Keep primary submit action visible for long forms through sticky action bars.
- Use clear required/optional treatment.
- Preserve entered data on validation errors.
- Use server-side errors as the final source of truth; frontend validation is a usability aid only.

## Tables and Lists

- Use tables when comparison across columns matters.
- Use list rows or compact cards when the item itself is the main unit of work.
- Keep high-frequency row actions visible or available through a predictable action menu.
- Support empty, loading, and error states in the same visual region where data would appear.
- Avoid wide tables on mobile; convert to stacked row groups.
- Keep row density suitable for operational scanning.

## Empty States

- Empty states should state what is missing and provide the next safe action when one exists.
- Avoid decorative-only empty states.
- Keep empty states compact enough that the action remains visible.
- Do not imply autocreation for modules where explicit creation is an accepted product rule.

## Loading States

- Use skeletons or compact loading indicators in the area being loaded.
- Avoid full-page blockers unless the entire route cannot render without the data.
- Preserve layout dimensions while loading to reduce layout shift.
- Do not use infinite decorative spinners when a skeleton or inline loading label is clearer.

## Error States

- Show errors close to the failed area.
- Use plain language and a clear recovery action when possible.
- Distinguish validation errors from authorization, missing resource, network, and server errors.
- For sensitive data, do not expose internal details in UI messages.
- Keep retry actions visible when retry is safe.

## Module Patterns

### Dashboard

- Use compact KPI cards in a summary grid.
- Prioritize scanning: active patients, today appointments, pending appointments, documents, treatment plans, accepted quotes, and issued billing documents.
- Avoid advanced analytics, charts, branch dashboards, doctor dashboards, and historical reporting until those slices are intentionally opened.

### Patients

- Optimize for fast search, fast registration, and clear patient identity.
- Keep patient name, status, alerts, and primary actions visible.
- Use tabs for profile, clinical context links, documents, treatments, and billing entry points when those sections coexist.
- Do not hide clinical alerts or active/inactive state below scroll.

### Scheduling

- Calendar and worklist views must keep branch context visible.
- Appointment create/edit/reschedule actions should remain low friction.
- Confirmation, manual reminder, and follow-up actions must be explicit and not imply automated delivery.
- Use drawers for appointment detail, reminder log, manual reminder preparation, and template helper content when it avoids losing calendar context.
- Do not reopen doctor-based views without a dedicated provider/doctor assignment slice.

### Clinical Records

- Prioritize readable clinical context and fast note entry.
- Use tabs or grouped sections for snapshot, allergies, notes, diagnoses, timeline, and snapshot history.
- Keep sensitive clinical actions clear and deliberate.
- Do not merge bounded timeline and snapshot history into a single ambiguous stream.

### Odontogram

- Keep the dental chart or tooth grid as the central work area.
- Use side panels or drawers for selected tooth/surface status, findings, and findings history.
- Preserve explicit creation and update flows; do not imply autocreation.
- Use color and labels together for tooth, surface, and finding states.

### Treatments

- Keep plan status, quote status, totals, and primary next action visible.
- Use compact item lists with clear dental references when present.
- Use sticky actions for pricing or long item workflows.
- Do not imply advanced pricing, discounts, taxes, billing linkage, treatment execution, or multi-quote workflows outside accepted slices.

### Billing

- Keep billing document status, total, currency, source quote state, and issue action visible.
- Use clear read-only treatment once a document is issued.
- Avoid payment, receipt, tax, CFDI/PAC, cancellation, or balance UI until those slices are opened.

### Documents

- Keep upload, active document list, file type, size, date, and download/retire actions easy to scan.
- Use compact upload states and clear validation for type and size limits.
- Do not imply public sharing, OCR, rich preview, versioning, generated PDFs, or external document workflows before those slices exist.

## Do / Don't

### Do

- Keep the primary action visible.
- Use tokens instead of hardcoded visual drift.
- Prefer compact operational layouts over decorative page sections.
- Use tabs, drawers, modals, and sticky bars according to workflow need.
- Pair color with labels or icons.
- Respect keyboard navigation and reduced motion.
- Redesign by bounded slices.

### Don't

- Do not hide critical actions behind excessive scroll.
- Do not introduce a new UI library without a documented decision.
- Do not create one-off styles that ignore `--bsm-*` tokens.
- Do not use scroll-jacking or decorative infinite animations.
- Do not redesign screens in a big bang.
- Do not change auth, tenant, branch, permissions, API contracts, migrations, or seeds as part of visual redesign.
- Do not present deferred roadmap capabilities as implemented UI.

## Visual Acceptance Criteria

A redesigned screen is acceptable when:

- The primary workflow is clear within the first viewport on desktop and tablet.
- The page header is compact and useful.
- Primary actions are visible or sticky when content is long.
- Status, ownership context, and sensitive module context are easy to scan.
- Color use is consistent with `--bsm-*` tokens.
- Empty, loading, error, disabled, hover, focus, selected, and read-only states are covered.
- The screen works on desktop, tablet, and mobile without incoherent overlap.
- Keyboard navigation and focus visibility are preserved.
- Motion is purposeful and respects reduced-motion preferences.
- The screen does not imply unimplemented product capabilities.

## Redesign Review Checklist

- [ ] Does the screen preserve the accepted module scope?
- [ ] Are tenant, branch, auth, permissions, and API behavior untouched unless explicitly in scope?
- [ ] Is the primary action visible without unnecessary scrolling?
- [ ] Is the page header compact?
- [ ] Is high-value status presented in a summary strip, badge, or visible local toolbar?
- [ ] Are tabs, drawers, modals, and sticky action bars used consistently with this guide?
- [ ] Are `--bsm-*` tokens used for colors, radius, shadows, focus, and motion?
- [ ] Is white still the dominant operational background?
- [ ] Are purple and cyan used as purposeful accents?
- [ ] Are badge states labeled and not color-only?
- [ ] Are empty, loading, and error states designed?
- [ ] Are forms grouped by workflow and validated clearly?
- [ ] Do tables/lists remain scannable and responsive?
- [ ] Does mobile avoid cramped tables and hidden actions?
- [ ] Is focus visible for every interactive element?
- [ ] Can keyboard users complete the workflow?
- [ ] Are modals/drawers accessible when present?
- [ ] Does motion respect `prefers-reduced-motion`?
- [ ] Was the redesign implemented as a small, reviewable slice?
