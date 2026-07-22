from pathlib import Path

path = Path(__file__).with_name("reconcile_release6_docs.py")
text = path.read_text(encoding="utf-8")

patches = [
    (
        'text = replace_once(text, "* **Release 5 — Treatments and Quotes**", "* **Release 5 — Treatments and Quotes**\\n* **Release 6 — Billing**", path)',
        'text = replace_once(text, "* **Release 4 — Odontogram**\\n* **Release 5 — Treatments and Quotes**", "* **Release 4 — Odontogram**\\n* **Release 5 — Treatments and Quotes**\\n* **Release 6 — Billing**", path)',
        "README milestone patch",
    ),
    (
        '- **Release 6 — Billing** — planned after Release 5\\n- **Release 7 — Documents and Dashboard** — planned after Release 6',
        '- **Release 6 — Billing** — next planned functional phase\\n- **Release 7 — Documents and Dashboard** — planned after Release 6',
        "product roadmap overview patch",
    ),
]

for old, new, label in patches:
    if old not in text:
        raise RuntimeError(f"{label} target was not found")
    text = text.replace(old, new, 1)

path.write_text(text, encoding="utf-8")
print("Patched Release 6 reconciliation script")
