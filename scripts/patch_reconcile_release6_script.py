from pathlib import Path

path = Path(__file__).with_name("reconcile_release6_docs.py")
text = path.read_text(encoding="utf-8")
old = 'text = replace_once(text, "* **Release 5 — Treatments and Quotes**", "* **Release 5 — Treatments and Quotes**\\n* **Release 6 — Billing**", path)'
new = 'text = replace_once(text, "* **Release 4 — Odontogram**\\n* **Release 5 — Treatments and Quotes**", "* **Release 4 — Odontogram**\\n* **Release 5 — Treatments and Quotes**\\n* **Release 6 — Billing**", path)'
if old not in text:
    raise RuntimeError("README milestone patch target was not found")
path.write_text(text.replace(old, new, 1), encoding="utf-8")
print("Patched Release 6 reconciliation script")
