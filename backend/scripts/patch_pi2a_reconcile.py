from pathlib import Path

path = Path(__file__).with_name("reconcile_pi2a_docs.py")
text = path.read_text(encoding="utf-8")
old = '    "- `PatientId`, portal account and linked Patient come from verified server context\\n- phone, date of birth and contact data cannot be used to claim a record publicly\\n- PI-1 is completed through PI-1A to PI-1D; intake opens only through PI-2 after its own decisions",'
new = '    "- `TenantId`, portal account and linked Patient come from verified server context\\n- phone, date of birth and contact data cannot be used to claim a record publicly\\n- PI-1 is completed through PI-1A to PI-1D; intake opens only through PI-2 after its own decisions",'
if text.count(old) != 1:
    raise RuntimeError("Expected one tenant-model replacement source in reconciliation script.")
path.write_text(text.replace(old, new, 1), encoding="utf-8")
