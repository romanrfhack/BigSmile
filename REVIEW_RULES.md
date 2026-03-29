Perfecto. Te dejo un **`REVIEW_RULES.md`** inicial, pensado específicamente para que **OpenClaw** y también tú/CODEX tengan una guía clara de revisión antes de dar por bueno un cambio en **Bigsmile**.

Guárdalo como:

`REVIEW_RULES.md`

```md
# REVIEW_RULES.md

# Project
Bigsmile

# Purpose of this file
This file defines the review rules that must be applied to any meaningful change in the repository.

Its goal is to protect Bigsmile as a:
- secure product
- maintainable product
- multi-tenant SaaS
- operationally efficient product
- coherent long-term codebase

This file is intended for:
- human reviewers
- AI-assisted reviewers
- CODEX/OpenClaw review passes
- self-review before finalizing changes

This file complements:
- README.md
- AGENTS.md
- PROJECT_MAP.md
- docs/architecture.md
- docs/tenant-model.md
- docs/product-roadmap.md
- docs/contributing.md
- docs/decisions/*.md

---

## 1. Review Philosophy

A change is not acceptable only because it compiles or appears to work.

Every review must evaluate whether the change:
- respects the architecture
- preserves tenant isolation
- preserves security
- fits the roadmap
- keeps the codebase understandable
- protects operational UX
- leaves sufficient evidence for later review

The review standard must remain high because Bigsmile is a commercial SaaS product, not a throwaway prototype.

---

## 2. Mandatory Review Dimensions

Every meaningful review must evaluate the change through these dimensions:

1. product fit
2. architecture fit
3. multi-tenant safety
4. security
5. code quality
6. testing
7. documentation
8. operational UX impact
9. risk and reversibility

A review is incomplete if it ignores any of these when relevant.

---

## 3. Product Fit Review

### Review questions
- Does this change solve a real product need?
- Does it belong to the current roadmap stage?
- Does it strengthen the operational core or the SaaS foundation?
- Is the scope appropriate for the current phase?
- Does it introduce premature complexity?

### Red flags
- feature added outside roadmap priorities without justification
- premium capability introduced before core workflow is stable
- infrastructure complexity added without business need
- large refactor disguised as a small feature

### Expected review outcome
The reviewer should clearly state whether the change is:
- aligned with roadmap
- acceptable but early
- out of scope
- risky due to product timing

---

## 4. Architecture Fit Review

### Review questions
- Does the change respect module ownership?
- Does it preserve the layered architecture?
- Is the code placed in the correct project/folder/feature?
- Does it avoid creating hidden coupling?
- Does it preserve the modular monolith direction?

### Backend rules to verify
- no business logic in controllers
- no domain logic in repositories
- no infrastructure leakage into domain
- application logic remains use-case oriented
- important invariants remain close to the domain

### Frontend rules to verify
- no giant page components
- no business logic hidden inside presentation-only components
- no scattered direct HTTP access across UI
- feature ownership remains clear
- data-access/facade/page separation is preserved

### Red flags
- code added to the wrong module “for convenience”
- generic utilities becoming silent business logic owners
- mixed-purpose files growing too large
- hidden dependencies across unrelated modules

### Expected review outcome
The reviewer should identify:
- correct ownership
- incorrect ownership
- any architectural drift
- any need for ADR/doc updates

---

## 5. Multi-Tenant Safety Review

This is one of the most critical review areas in Bigsmile.

### Mandatory review questions
- Is the affected data tenant-owned?
- Is `TenantId` modeled or preserved correctly?
- If `BranchId` is involved, does tenant remain the primary boundary?
- Could this read data from another tenant?
- Could this write data into another tenant?
- Could this allow unintended cross-tenant access?
- Is any platform bypass explicit and auditable?
- Is tenant scope inferred safely?

### Must-verify areas
- entity modeling
- query filtering
- handler logic
- repository/specification behavior
- platform override logic
- authorization scope
- API contract assumptions
- integration tests

### Red flags
- manual tenant filtering added in ad hoc ways where central enforcement exists
- missing tenant constraints in new business records
- accepting tenant context from unsafe client input
- platform support logic mixed with normal tenant logic
- branch used as if it replaced tenant isolation

### Expected review outcome
The reviewer must explicitly answer:
- tenant-safe
- tenant-risky
- blocked pending clarification

A change touching tenant-sensitive paths must never be approved with vague language.

---

## 6. Security Review

### Review questions
- Does the change affect authentication?
- Does it affect authorization?
- Are sensitive operations protected in backend?
- Are secrets handled safely?
- Is the access model preserved?
- Are patient, clinical, payment, or document data protected appropriately?
- Does the change create escalation paths or bypasses?

### Must-check sensitive areas
- auth/session
- user roles and permissions
- platform admin actions
- patient data
- clinical record access
- payments
- documents
- file access
- tenant switching
- branch restrictions

### Red flags
- secrets committed into repo
- hardcoded credentials or tokens
- security rules enforced only in frontend
- hidden admin bypasses
- permissions inferred implicitly without clear rule
- unsafe defaults introduced “temporarily”

### Expected review outcome
The reviewer should state:
- no material security concern found
- security concerns found
- requires deeper security review

---

## 7. Code Quality Review

### Review questions
- Is the code understandable?
- Is naming clear and explicit?
- Is responsibility focused?
- Is the implementation easy to maintain?
- Is duplication acceptable or should it be refactored?
- Is the solution too clever for its own good?
- Would another developer understand the intent quickly?

### Must-check quality signals
- class size
- file size
- method size
- explicitness
- cohesion
- dependency clarity
- error handling clarity
- consistency with existing conventions

### Red flags
- giant service classes
- giant UI components
- deep nesting
- hidden side effects
- unclear naming
- accidental complexity
- business rules duplicated across layers

### Expected review outcome
The reviewer should classify code quality as:
- strong
- acceptable with improvements
- needs refactor before merge

---

## 8. Testing Review

Testing is mandatory for meaningful changes.

### Review questions
- What was validated?
- Are the right tests present for the risk level?
- Does the change need unit tests?
- Does it need integration tests?
- Does it need architecture tests?
- Does it need frontend unit tests?
- Does it need end-to-end tests?
- Were relevant existing tests updated?

### High-risk changes that should almost always require tests
- tenant handling
- authorization
- patient data flows
- clinical workflows
- payments
- documents
- platform overrides
- branch restrictions
- route guards
- data-access logic

### Red flags
- no tests for high-risk changes
- validation limited to “it builds”
- missing regression coverage for changed behavior
- test evidence omitted from the summary

### Expected review outcome
The reviewer should explicitly list:
- tests run
- tests added
- tests still missing
- validation gaps

---

## 9. Documentation Review

### Review questions
- Does this change alter architecture?
- Does it alter tenant rules?
- Does it alter roadmap expectations?
- Does it alter module boundaries?
- Does it alter auth/session or permission behavior?
- Does it alter operational workflows that should be documented?

### Documentation rules
If the system becomes easier to misunderstand after the change, documentation is missing.

### Red flags
- architecture changed without updating docs
- tenant behavior changed without updating tenant-model docs
- roadmap implied by code no longer matches docs
- major decision made without ADR

### Expected review outcome
The reviewer should state:
- docs are aligned
- docs need update
- ADR needed

---

## 10. Operational UX Review

Bigsmile is an operational product. UX must be reviewed as part of correctness.

### Review questions
- Does this make core workflows faster or slower?
- Does it add friction to front desk operations?
- Does it add friction to clinician workflows?
- Does it make the UI more confusing?
- Does it preserve a clear path for common tasks?

### Core workflows to protect
- patient registration
- patient search
- appointment creation
- appointment rescheduling
- clinical note entry
- odontogram interaction
- treatment planning
- payment registration

### Red flags
- more steps without justification
- hidden actions
- confusing navigation
- overcomplicated forms
- visual clutter in high-frequency screens
- mixing advanced options into primary workflows without need

### Expected review outcome
The reviewer should classify UX impact as:
- improves workflow
- neutral
- adds friction
- requires redesign

---

## 11. Risk and Reversibility Review

### Review questions
- Is the change small, medium, or large risk?
- Is it reversible?
- Is it isolated?
- Does it affect critical flows?
- Does it introduce migration risk?
- Does it affect many modules at once?
- Could it cause production issues if wrong?

### Risk categories
- **Low risk**: local, reversible, well-tested, structurally aligned
- **Medium risk**: affects a core path or shared behavior, but bounded and testable
- **High risk**: affects tenant safety, auth, data integrity, payments, or major architecture rules

### Red flags
- large mixed diffs
- hard-to-reverse schema changes without plan
- unclear side effects
- critical flows changed without strong validation
- silent changes to shared behavior

### Expected review outcome
The reviewer must classify the change and describe why.

---

## 12. Required Review Packet

Every meaningful review should end with a concise review packet.

### Required format

#### Objective
What was the change trying to accomplish?

#### Scope
Which modules/files were affected?

#### Product fit
Is it aligned with the current roadmap and product direction?

#### Architecture review
Does it respect ownership and structure?

#### Tenant safety review
Is tenant isolation preserved?

#### Security review
Any security issues or concerns?

#### Testing review
What was validated? What is still missing?

#### Documentation review
Were docs updated? Is an ADR needed?

#### Risk level
Low / Medium / High

#### Approval recommendation
- approve
- approve with follow-up
- changes required
- blocked pending decision

This packet should be written clearly enough for later human or CODEX review.

---

## 13. Review Rules for AI-Assisted Work

AI-generated code requires extra review discipline.

### Mandatory AI review checks
- verify module ownership
- verify tenant safety explicitly
- verify auth/authz implications
- verify code placement
- verify no silent duplication of business rules
- verify file size and responsibility boundaries
- verify tests and docs are not missing

### Common AI-generated risks
- oversized files
- generic abstractions with unclear ownership
- weak authorization assumptions
- tenant leaks through missing filters
- logic mixed into the wrong layer
- UI orchestration hidden in components
- broad refactors without sufficient justification

### AI review principle
Do not trust generated code because it looks polished.

Review it against the architecture and tenant model.

---

## 14. Review Triggers That Require Extra Attention

The following changes require heightened review:

### Platform-level triggers
- tenant onboarding
- tenant suspension
- plan or feature enablement
- platform admin override behavior

### Identity triggers
- session/auth changes
- role changes
- permission model changes
- guard/policy changes

### Clinical triggers
- patient access paths
- clinical notes
- odontogram persistence
- document access
- treatment planning rules

### Financial triggers
- charges
- payments
- balances
- receipts
- refunds or reversals

### Structural triggers
- moving module ownership
- shared abstractions additions
- new cross-cutting patterns
- architecture test changes
- dependency direction changes

---

## 15. When Review Must Block the Change

A review must block the change if any of the following is true:

- tenant isolation is unclear or unsafe
- authorization is missing or ambiguous
- secrets or insecure defaults are introduced
- module ownership is clearly violated
- the change is too large to review properly
- tests are missing for a high-risk change
- documentation drift becomes significant
- the code introduces severe maintainability issues
- the change conflicts with accepted architecture decisions

Blocking is appropriate when the product foundation would be weakened.

---

## 16. When Follow-Up Is Acceptable

A change may be approved with follow-up only if:
- the core architecture is preserved
- tenant safety is not compromised
- security is not compromised
- remaining issues are bounded and non-critical
- follow-up items are explicit and tracked

Examples of acceptable follow-up:
- additional refactor of non-critical duplication
- minor documentation polish
- extra low-risk tests
- UI polish in a non-critical area

Examples of unacceptable follow-up:
- “we’ll fix tenant safety later”
- “we’ll add auth later”
- “we’ll move it to the right module later”
- “we’ll document the decision later” after a major architecture change

---

## 17. Review Checklist

Use this condensed checklist before final approval.

### Product
- [ ] Fits roadmap stage
- [ ] Solves a real product need
- [ ] Does not add premature complexity

### Architecture
- [ ] Correct module ownership
- [ ] Correct layer placement
- [ ] No obvious architecture drift
- [ ] No giant mixed-responsibility files

### Multi-tenancy
- [ ] Tenant ownership is explicit
- [ ] No cross-tenant leak risk found
- [ ] Branch usage is correct
- [ ] Platform bypass, if any, is explicit and audited

### Security
- [ ] Authorization is correct
- [ ] Sensitive paths remain protected
- [ ] No secrets or insecure defaults introduced

### Testing
- [ ] Appropriate tests added/updated
- [ ] Validation evidence provided
- [ ] No critical coverage gap left hidden

### Documentation
- [ ] Docs updated where needed
- [ ] ADR updated/created if needed

### UX
- [ ] Core workflow not degraded
- [ ] No unnecessary friction introduced

### Risk
- [ ] Risk level identified
- [ ] Change is reviewable and reasonably reversible

---

## 18. Final Review Question

Before approving a change, ask:

**Does this change make Bigsmile stronger as a secure, maintainable, multi-tenant SaaS product with a fast operational workflow for dental clinics?**
```
