# ADR 005 — Frontend Operational UX Redesign

- **Status:** Accepted
- **Date:** 2026-05-06
- **Decision Type:** Frontend UX/UI convention
- **Scope:** Frontend visual conventions, operational layout patterns, progressive screen redesign
- **Applies To:** Frontend documentation and future frontend redesign slices

---

## Context

A client perceived BigSmile as lacking enough color and dynamism.

Several actions or sections in operational screens could fall out of view because of excessive scrolling.

BigSmile needs to look more attractive and modern without degrading operational speed for clinic staff and clinicians.

The frontend already has `--bsm-*` visual tokens for brand color, accent color, surfaces, borders, focus, radius, shadows, and motion. Future redesign work should build on those tokens instead of creating isolated visual patterns.

Because BigSmile is a secure multi-tenant SaaS product, frontend redesign must not casually change backend behavior, tenant isolation, branch scope, authorization, permissions, API contracts, migrations, seeds, or accepted product scope.

## Decision

BigSmile will apply a progressive frontend redesign, not a big bang redesign.

Before redesigning screens one by one, BigSmile formalizes shared operational UX/UI patterns in `docs/frontend-ux-guidelines.md`.

Future redesign slices must prioritize visibility of primary actions and avoid hiding critical workflow actions behind unnecessary scrolling.

Operational screens should use compact page headers, summary strips, tabs, drawers, modals, and sticky action bars according to the rules documented in the UX guide.

BigSmile will use purposeful microanimations for state clarity and interaction feedback, while avoiding scroll-jacking, decorative infinite animations, and motion that slows data entry.

All redesign work must preserve accessibility expectations, including visible focus, keyboard navigation, contrast, non-color-only status communication, accessible modals/drawers, and `prefers-reduced-motion`.

The existing `--bsm-*` tokens remain the base visual system. New visual needs should extend the token set intentionally rather than hardcoding inconsistent local styles.

## Consequences

Future frontend screens and redesign slices must follow `docs/frontend-ux-guidelines.md`.

Redesign changes must be implemented by small, reviewable slices, ideally screen by screen or workflow by workflow.

The frontend must not accumulate isolated, inconsistent redesign patterns.

Critical actions must not be hidden behind avoidable scroll when tabs, drawers, compact summaries, or sticky action bars can keep the workflow visible.

Review of future frontend work should include visual acceptance criteria and the redesign checklist from the UX guide.

## Non-goals

- Do not introduce a new UI library.
- Do not change backend behavior or API contracts.
- Do not modify tenant isolation.
- Do not modify authentication, guards, tenant context, branch context, permissions, migrations, or seeds.
- Do not reopen doctor-based views in Scheduling.
- Do not redesign the whole system in a single PR.
- Do not present deferred capabilities such as automated reminders, external messaging providers, advanced dashboards, OCR, payments, taxes, CFDI/PAC, or patient portal as implemented UI.

## Follow-up

Apply the guide progressively to future frontend redesign slices and keep each slice bounded to the owning feature.

If future redesign work changes shared frontend state strategy, introduces a UI dependency, or changes major frontend module boundaries, create a separate ADR.
