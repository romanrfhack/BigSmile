#!/usr/bin/env python3
"""
Architecture validation script for Bigsmile.
Validates that project references follow the layered architecture:
- Domain must not reference Infrastructure, Application, Api.
- Application must not reference Infrastructure, Api.
- Infrastructure must not reference Api.
- SharedKernel can be referenced by any layer.
"""

import os
import sys
import xml.etree.ElementTree as ET
import re

# Define projects and their allowed references
LAYERS = {
    "BigSmile.Domain": {"forbidden": ["BigSmile.Infrastructure", "BigSmile.Application", "BigSmile.Api"]},
    "BigSmile.Application": {"forbidden": ["BigSmile.Infrastructure", "BigSmile.Api"]},
    "BigSmile.Infrastructure": {"forbidden": ["BigSmile.Api"]},
    "BigSmile.Api": {"forbidden": []},
    "BigSmile.SharedKernel": {"forbidden": []},  # can be referenced by anyone
}

def validate_namespace_conventions(src_dir):
    """
    Validate that .cs files reside in a directory that matches their namespace.
    This ensures architectural layering is reflected in the file system.
    """
    warnings = []
    namespace_pattern = re.compile(r'^\s*namespace\s+([\w\.]+)')

    # Expected mapping from directory name to namespace prefix
    # e.g., files under src/BigSmile.Domain should have namespace starting with BigSmile.Domain
    for project_dir in os.listdir(src_dir):
        project_path = os.path.join(src_dir, project_dir)
        if not os.path.isdir(project_path):
            continue
        expected_prefix = project_dir  # e.g., BigSmile.Domain
        for root, dirs, files in os.walk(project_path):
            for file in files:
                if not file.endswith('.cs'):
                    continue
                file_path = os.path.join(root, file)
                try:
                    with open(file_path, 'r', encoding='utf-8') as f:
                        content = f.read(2000)  # read first 2KB, enough for namespace declaration
                        match = namespace_pattern.search(content)
                        if not match:
                            continue
                        namespace = match.group(1)
                        # Check that namespace starts with expected_prefix
                        if not namespace.startswith(expected_prefix):
                            warnings.append(
                                f"Namespace mismatch: {file_path} declares namespace '{namespace}' "
                                f"but is located under project '{expected_prefix}'"
                            )
                except Exception as e:
                    warnings.append(f"Could not read {file_path}: {e}")
    return warnings

def self_test():
    """Validate that the validator can detect a forbidden reference."""
    import tempfile
    import os
    import sys
    # Create a temporary .csproj with a forbidden reference
    xml_content = r"""<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="../BigSmile.Infrastructure/BigSmile.Infrastructure.csproj" />
  </ItemGroup>
</Project>"""
    try:
        with tempfile.NamedTemporaryFile(mode='w', suffix='.csproj', delete=False) as f:
            f.write(xml_content)
            temp_path = f.name
        refs = get_project_references(temp_path)
        os.unlink(temp_path)
        if "BigSmile.Infrastructure" not in refs:
            print("SELF-TEST FAILED: Validator failed to detect forbidden reference.", file=sys.stderr)
            sys.exit(2)
        else:
            print("✓ Self-test passed: validator can detect references.")
    except Exception as e:
        print(f"SELF-TEST ERROR: {e}", file=sys.stderr)
        sys.exit(2)


def get_project_references(project_path):
    """Return list of referenced project names."""
    try:
        tree = ET.parse(project_path)
        root = tree.getroot()
        refs = []
        # Try with namespace (old .NET Framework)
        ns = {"msbuild": "http://schemas.microsoft.com/developer/msbuild/2003"}
        for pr in root.findall(".//msbuild:ProjectReference", ns):
            include = pr.get("Include")
            if include:
                name = os.path.splitext(os.path.basename(include.replace("\\\\", "/")))[0]
                refs.append(name)
        # If no namespaced references found, try without namespace (SDK style)
        if not refs:
            for pr in root.findall(".//ProjectReference"):
                include = pr.get("Include")
                if include:
                    name = os.path.splitext(os.path.basename(include.replace("\\\\", "/")))[0]
                    refs.append(name)
        return refs
    except Exception as e:
        print(f"ERROR parsing {project_path}: {e}", file=sys.stderr)
        return []
def main():
    self_test()
    src_dir = os.path.join(os.path.dirname(__file__), "..", "src")
    if not os.path.exists(src_dir):
        print(f"ERROR: Source directory not found at {src_dir}", file=sys.stderr)
        sys.exit(1)

    errors = []
    for project_name, rules in LAYERS.items():
        project_file = os.path.join(src_dir, project_name, f"{project_name}.csproj")
        if not os.path.exists(project_file):
            errors.append(f"Project file missing: {project_file}")
            continue

        refs = get_project_references(project_file)
        for forbidden in rules["forbidden"]:
            if forbidden in refs:
                errors.append(f"{project_name} references forbidden project {forbidden}")

    # Namespace convention warnings
    warnings = validate_namespace_conventions(src_dir)
    if warnings:
        print("ARCHITECTURE WARNINGS (non‑fatal):")
        for warn in warnings:
            print(f"  ⚠ {warn}")

    if errors:
        print("ARCHITECTURE VALIDATION FAILED:")
        for err in errors:
            print(f"  - {err}")
        sys.exit(1)
    else:
        print("✓ Architecture validation passed.")
        sys.exit(0)

if __name__ == "__main__":
    main()