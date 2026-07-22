from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace_if_present(path: str, old: str, new: str) -> None:
    file_path = ROOT / path
    text = file_path.read_text(encoding="utf-8")
    count = text.count(old)
    if count == 0:
        print(f"SKIP {path}: target already absent")
        return
    if count > 1:
        raise RuntimeError(f"Expected at most one match in {path}, found {count}: {old}")
    file_path.write_text(text.replace(old, new, 1), encoding="utf-8")
    print(f"UPDATED {path}")


replace_if_present(
    "STATE — BigSmile.md",
    "- Taxes, discounts, Billing/payments o scheduling linkage.",
    "- Billing queda fuera de Release 5 y se acepta por separado en Release 6.1; payments y scheduling linkage permanecen diferidos.",
)
replace_if_present(
    "STATE — BigSmile.md",
    "- Mantener la navegación existente a Billing como capability no aceptada de Release 6.",
    "- Mantener la navegación hacia Billing alineada con Release 6.1, sin implicar payments, balances o fiscalización.",
)
replace_if_present(
    "README.md",
    "Advanced commercial and execution capabilities remain deferred: treatment catalog administration, multiple or archived plans, regenerate/versioning, multiple quotes or negotiation, taxes, discounts, Billing/Scheduling linkage, treatment execution/progress, insurance, financing, advanced approvals and Patient Portal access.",
    "Advanced commercial and execution capabilities remain deferred: treatment catalog administration, multiple or archived plans, regenerate/versioning, multiple quotes or negotiation, taxes, discounts, Scheduling linkage, treatment execution/progress, insurance, financing, advanced approvals and Patient Portal access. Billing is accepted separately through Release 6.1; payments remain deferred.",
)
replace_if_present(
    "README.md",
    "* Treatment execution, taxes/discounts, Billing/Scheduling linkage, versioning and negotiation remain deferred",
    "* Treatment execution, taxes/discounts, Scheduling linkage, versioning and negotiation remain deferred; Billing is accepted separately through Release 6.1",
)
replace_if_present(
    "AGENTS.md",
    "Do not reopen advanced Treatments/Quotes scope incidentally. Treatment catalog administration, multiple/archived plans, quote regeneration/versioning, multiple quotes, negotiation, taxes, discounts, Billing linkage, scheduling linkage, treatment execution/progress, insurance, financing, advanced approvals and Patient Portal access remain future bounded work.",
    "Do not reopen advanced Treatments/Quotes scope incidentally. Treatment catalog administration, multiple/archived plans, quote regeneration/versioning, multiple quotes, negotiation, taxes, discounts, scheduling linkage, treatment execution/progress, insurance, financing, advanced approvals and Patient Portal access remain future bounded work. Billing is accepted separately through Release 6.1; payments remain deferred.",
)
replace_if_present(
    "PROJECT_MAP.md",
    "Treatment catalog administration, multiple/archived plans, quote versioning/negotiation, execution/progress, taxes/discounts, Billing linkage and Patient Portal access remain deferred.",
    "Treatment catalog administration, multiple/archived plans, quote versioning/negotiation, execution/progress, taxes/discounts and Patient Portal access remain deferred. Billing is accepted separately through Release 6.1; payments remain deferred.",
)
replace_if_present(
    "docs/product-roadmap.md",
    "- Billing/payment linkage",
    "- Billing behavior is outside Release 5 and accepted separately through Release 6.1; payment linkage remains deferred",
)
replace_if_present(
    "docs/ux-redesign-reconciliation-and-plan.md",
    "- Billing or payment behavior;",
    "- Billing behavior as part of Release 5; Billing is accepted separately through Release 6.1, while payments remain deferred;",
)
replace_if_present(
    "docs/release-6-billing-audit-and-closure.md",
    "The repository already contains domain, application, API, persistence, authorization, Angular and automated-test coverage for a `BillingDocument` capability. Until this audit and the pending canonical reconciliation are merged, Billing remains `implemented but not formally accepted/reconciled`.",
    "The repository already contained domain, application, API, persistence, authorization, Angular and automated-test coverage for a `BillingDocument` capability. Before this audit and closure reconciliation, Billing remained correctly classified as `implemented but not formally accepted/reconciled`.",
)
replace_if_present(
    "docs/decisions/009-release-6-billing-document-foundation.md",
    "- Release 7 can become the next audit target after formal acceptance;",
    "- Release 7 becomes the next audit target after formal acceptance;",
)

print("Release 6 wording cleanup completed")
