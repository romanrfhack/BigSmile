# Bigsmile Tenant Model

## Purpose

This document defines the tenant model of **Bigsmile**.

Its goal is to establish clear rules for how the platform will represent, isolate, secure, and operate multiple clinics within the same SaaS product.

This document is foundational because **multi-tenancy is not an optional feature in Bigsmile**. It is part of the core architecture from the beginning.

---

## 1. Core Definitions

### Tenant
A **Tenant** represents a clinic or private dental practice that uses Bigsmile.

A tenant is the primary boundary for:
- data ownership
- access control
- configuration
- branding
- operational isolation
- reporting scope
- product licensing in future phases

### Branch
A **Branch** represents a physical location, office, or branch that belongs to a tenant.

A branch exists to support:
- operational segmentation
- scheduling by location
- user assignment by location
- reporting by location
- future multi-branch administration

### Platform
The **Platform** represents the Bigsmile system itself and the internal administration capabilities operated by the product owner.

Platform operations are not the same as tenant operations and must be modeled explicitly.

---

## 2. Foundational Rule

The most important rule in the Bigsmile tenant model is:

**Every business record belongs to a Tenant.**

This means:
- tenant isolation is always enforced by `TenantId`
- `BranchId` never replaces `TenantId`
- cross-tenant access is forbidden unless explicitly allowed for platform-level operations
- tenant boundaries must be enforced by architecture, not by developer memory

---

## 3. High-Level Model

### Relationship model

- One **Tenant** can have many **Branches**
- One **Tenant** can have many **Users**
- One **User** may belong to one tenant at a time in the initial version
- One **User** may be assigned to one or more branches inside the same tenant
- One **Tenant** owns all of its patients, appointments, clinical records, treatments, payments, and documents

### Initial scope simplification

For the first versions of Bigsmile:

- a user is expected to operate within a single tenant context
- branch assignments may be one or many
- platform administrators are outside normal tenant restrictions but still audited

This keeps the model simple without sacrificing the future SaaS direction.

---

## 4. Tenant Boundary

The **Tenant** is the main security and ownership boundary of the platform.

### A tenant owns:
- patients
- appointments
- clinical records
- clinical medical questionnaire answers
- odontograms
- treatment plans
- charges and payments
- documents
- users and roles within the tenant
- branch configuration
- tenant configuration
- branding and future plan settings

### A tenant does not own:
- platform-wide administrative settings
- platform-level plans catalog
- global product configuration
- platform observability and internal administration

---

## 5. Branch Boundary

A **Branch** is an operational boundary, not the primary security boundary.

### A branch is used for:
- appointment scheduling by location
- branch-specific calendars
- branch-level reporting
- assigning users to operational locations
- filtering patient and billing activity by location where relevant

### A branch is not used for:
- replacing tenant isolation
- allowing cross-tenant access
- weakening tenant security rules

### Important rule

A record associated with a branch must still belong to a tenant.

For example:
- an appointment belongs to a tenant and may also belong to a branch
- a payment belongs to a tenant and may also belong to a branch
- a patient belongs to a tenant, even if the patient is attended in one or multiple branches

---

## 6. Entity Classification by Scope

Bigsmile entities should be classified according to their scope.

### 6.1 Platform-Scoped Entities
These belong to the Bigsmile platform and are not owned by a tenant.

Examples:
- platform administrator accounts
- subscription plans catalog
- platform feature definitions
- internal operational settings
- platform audit records for internal administration

### 6.2 Tenant-Scoped Entities
These belong to a tenant and must always include `TenantId`.

Examples:
- tenant
- tenant settings
- tenant users
- patient
- clinical record
- treatment plan
- payment
- document
- notification templates owned by a tenant

### 6.3 Branch-Scoped Operational Entities
These belong to a tenant and also carry a `BranchId` when required.

Examples:
- appointment
- appointment block
- cash session
- branch-specific reports
- branch assignment records
- branch-level operational logs

### 6.4 Shared Reference Entities
These are reference records that may be global or tenant-customizable depending on the module.

Examples:
- treatment catalog templates
- permissions catalog
- feature catalog
- notification event types

The implementation should explicitly decide which reference catalogs are:
- platform-global only
- tenant-overridable
- tenant-owned

---

## 7. Identity and Membership Model

### 7.1 User Model

A user represents a person who can access the system.

Examples:
- dentist
- front desk staff
- assistant
- tenant administrator
- platform administrator

### 7.2 Membership

A user must not be treated as automatically global.

Access is determined by explicit membership.

Initial model:
- a user may be associated to one tenant
- a user may have one or more role assignments
- a user may be assigned to one or more branches within the tenant

Suggested records:
- `User`
- `UserTenantMembership`
- `UserBranchAssignment`
- `Role`
- `Permission`

### 7.3 Role Scope

Roles must be interpreted within scope.

Examples:
- `PlatformAdmin` -> platform scope
- `TenantAdmin` -> tenant scope
- `Dentist` -> tenant scope
- `FrontDesk` -> tenant scope
- `Assistant` -> tenant scope

A role alone is not enough; it must be evaluated together with its scope and membership.

---

## 8. Request Tenant Resolution

Tenant resolution must be explicit, centralized, and consistent.

It must not be reconstructed ad hoc in handlers or controllers.

### 8.1 Initial Resolution Rules

For normal tenant users:
- the tenant comes from the authenticated user membership
- the user cannot arbitrarily switch to another tenant

For platform administrators:
- tenant override may be allowed only in explicitly supported platform operations
- tenant override must be audited
- tenant override must be blocked in contexts where it is not needed

Current foundation note:
- authenticated requests resolve `user id`, `tenant id`, `branch id`, and `scope` from JWT claims
- development headers remain a local fallback only for anonymous requests
- platform override is activated by explicit authorization policy/handler flow, not by general request headers
- frontend auth/session, tenant, branch, token, and permission state must not be persisted in `localStorage`; the UI language preference `bigsmile.ui.language` is allowed because it is non-sensitive and has no authorization meaning

### 8.2 TenantContext

Every authenticated request should have a request-scoped `TenantContext`.

`TenantContext` should expose at minimum:
- current tenant id
- current branch id if relevant
- current user id
- current access scope
- whether the request is platform-scoped or tenant-scoped
- whether an explicit platform override is active for the current request

### 8.3 BranchContext

Branch context may be:
- explicit
- selected by the user
- inferred from the current working location
- required for branch-bound operations

Branch context must always remain subordinate to the tenant context.

---

## 9. Data Modeling Rules

### 9.1 Required TenantId

Every tenant-owned business record must include `TenantId`.

This includes, at minimum:
- patients
- appointments
- clinical records
- clinical medical questionnaire answers
- odontograms
- treatment plans
- payments
- documents
- tenant users
- tenant settings

### 9.2 Required BranchId

`BranchId` should be included only when the business meaning requires branch association.

Examples:
- appointments
- cash sessions
- branch user assignments
- branch operational logs

Do not add `BranchId` to every entity by default unless the domain needs it.

### 9.3 Ownership Integrity

For any record that includes both `TenantId` and `BranchId`:
- the referenced branch must belong to the same tenant
- cross-tenant foreign key mismatches must be impossible by validation and data integrity rules

### 9.4 Unique Constraints

Uniqueness must usually be scoped by tenant.

Examples:
- patient code unique within a tenant
- branch name unique within a tenant
- username or email uniqueness rules depending on final identity strategy
- treatment catalog item codes unique within tenant if tenant-customized

Initial recommendation:
- prefer composite indexes using `TenantId` where uniqueness belongs to tenant scope

---

## 10. Isolation Enforcement

Tenant isolation must be enforced technically in multiple layers.

### 10.1 Domain and Modeling
- tenant-owned aggregates include `TenantId`
- branch-bound entities must remain tenant-bound
- aggregate creation must ensure valid tenant ownership

### 10.2 Application Layer
- use cases must run inside a resolved tenant context
- authorization must validate tenant scope
- commands and queries must not allow arbitrary tenant impersonation

### 10.3 Infrastructure Layer
- EF Core global query filters must enforce tenant isolation
- repository access must respect tenant scope
- bypass mechanisms must be explicit and limited
- the current foundation filters `Tenant`, `Branch`, and `UserTenantMembership` during authenticated requests
- authenticated writes must fail when the target tenant does not match the current tenant context unless an explicit platform override is active

### 10.4 API Layer
- transport contracts must not allow unsafe direct tenant selection in normal tenant flows
- platform-only endpoints must be clearly separated from tenant endpoints
- cross-tenant access attempts must fail predictably

### 10.5 Testing Layer
Bigsmile must include automated tests for:
- tenant isolation
- forbidden cross-tenant reads
- forbidden cross-tenant writes
- allowed platform override scenarios
- branch access restrictions inside a tenant

---

## 11. Platform Operations vs Tenant Operations

This distinction is critical.

### 11.1 Tenant Operations
These are normal operational actions performed within a clinic.

Examples:
- create patient
- schedule appointment
- update clinical record
- register payment
- upload patient document

These must always run inside tenant scope.

### 11.2 Platform Operations
These are internal Bigsmile administration actions.

Examples:
- create tenant
- suspend tenant
- configure plan assignment
- manage platform-wide feature availability
- inspect tenant onboarding status

These may operate outside normal tenant filters, but only:
- explicitly
- securely
- with audit trail
- with narrow authorization
- through policy-gated override activation rather than silent platform bypass

### 11.3 Dual-Mode Operations
Some features may later support both modes.

Example:
- a platform admin may need to inspect a tenant dashboard for support purposes

These cases must be designed carefully and audited.

---

## 12. Authorization Model by Scope

Authorization should not be based only on role names.

It must combine:
- authenticated user identity
- tenant membership
- branch assignment where relevant
- role
- permission
- scope of operation

Current foundational permission set:
- `auth.self.read`
- `platform.tenants.read`
- `tenant.read`
- `branch.read.any`
- `branch.read.assigned`

### Example access logic

A user may create an appointment only if:
- the user belongs to the tenant
- the user has scheduling permission
- the selected branch belongs to the same tenant
- the user is assigned to that branch if branch restriction applies

A platform admin may inspect tenant settings only if:
- the endpoint is platform-enabled
- the operation is audited
- the user holds platform-level authorization

---

## 13. Initial Role and Scope Matrix

### PlatformAdmin
Scope:
- platform

Can:
- create and manage tenants
- inspect onboarding status
- manage plans and platform features
- perform audited support operations

### TenantAdmin
Scope:
- tenant

Can:
- manage tenant users
- manage branches
- configure tenant settings
- access most tenant-level operational modules

### Dentist
Scope:
- tenant / branch depending on configuration

Can:
- view patients
- manage clinical records
- update odontograms
- create treatment plans
- view relevant appointments

### FrontDesk
Scope:
- tenant / branch depending on configuration

Can:
- register patients
- schedule and manage appointments
- register payments
- manage basic patient communications

### Assistant
Scope:
- tenant / branch depending on configuration

Can:
- assist in operational workflows according to granted permissions

This matrix is an initial foundation, not a final permission catalog.

---

## 14. Onboarding Model

### 14.1 Tenant Creation
When a new clinic is onboarded, the system should create:
- tenant record
- default settings
- at least one branch
- initial tenant administrator
- initial role mappings
- basic feature availability
- optional branding placeholders

### 14.2 Branch Bootstrap
Initial recommendation:
- every tenant starts with one default branch
- more branches can be added later
- the first branch simplifies MVP workflows without compromising the model

### 14.3 Initial Defaults
Suggested tenant defaults:
- one active branch
- one tenant admin
- baseline permissions
- default timezone
- default branding placeholders
- initial module enablement according to plan or stage

---

## 15. Reporting Scope

Reports must clearly declare their scope.

### Possible report scopes
- platform scope
- tenant scope
- branch scope

### Examples
- total tenants onboarded -> platform scope
- daily income for one clinic -> tenant scope
- appointments by location -> branch scope

This distinction is important to avoid accidental data leaks and confusing analytics.

---

## 16. Auditing Requirements

Audit logs should capture tenant and branch context whenever relevant.

### Recommended audit fields
- actor user id
- actor scope
- tenant id
- branch id if applicable
- action type
- target entity type
- target entity id
- timestamp
- correlation id
- result status

### Critical audited actions
- tenant creation and suspension
- branch creation and update
- user role changes
- platform overrides
- access-sensitive clinical changes
- payment changes
- document access where required by policy

---

## 17. Future Evolution

The tenant model should allow future evolution without breaking the foundation.

Potential future capabilities:
- multi-branch analytics
- per-tenant branding
- per-tenant feature flags
- plan-based feature enablement
- branch-level restrictions
- support impersonation with strict audit
- eventual tenant-specific data partitioning if scale or compliance requires it

### Important note

The initial implementation should not prematurely optimize for:
- separate database per tenant
- microservices by tenant
- excessive infrastructure complexity

The current model should remain simple, explicit, and safe.

---

## 18. Non-Goals for Initial Stage

To keep the initial design focused, the following are not immediate goals:
- advanced franchise ownership hierarchies
- multi-tenant user accounts spanning many clinics with arbitrary switching
- complex cross-tenant collaboration
- tenant-specific infrastructure deployments
- physical database isolation per tenant from day one

These may be added later if the business requires them.

---

## 19. Rules That Must Never Be Broken

The following rules are mandatory:

- every tenant-owned record must be tied to a tenant
- branch never replaces tenant as the primary boundary
- tenant filtering must not rely on manual discipline alone
- platform bypass must be explicit and audited
- cross-tenant reads and writes are forbidden by default
- authorization must be evaluated with scope, not just with role names
- future product growth must preserve tenant isolation

---

## 20. Final Principle

The Bigsmile tenant model exists to guarantee that the product can grow commercially without compromising security, maintainability, or architectural clarity.

Every future feature must answer this question:

**Does this preserve the tenant boundary clearly and safely?**
