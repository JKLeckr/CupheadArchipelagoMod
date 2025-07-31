#!/usr/bin/env python3
"""
check-hooks.py - scan hooks for HarmonyPatch mismatches.
"""

import os
import re
import sys

# === Configuration ===

# Files (relative to the repo root) to ignore entirely
IGNORED_FILES: set[str] = {
    'SaveKeyUpdaterHook.cs',
    # Add more paths here as needed, using forward slashes
}

# Directories (relative to the repo root) to ignore entirely
IGNORED_DIRS: set[str] = {
    'Mitigations',
    # Add more paths here as needed, using forward slashes
}

# Specific (HookClassName, TargetClassName) pairs to ignore
IGNORED_PATCHES: set[tuple[str, str]] = {
    ('LevelWeaponHook', 'AbstractLevelWeapon'),
    ('PlaneWeaponHook', 'AbstractPlaneWeapon'),
    # Add more tuples here to whitelist particular patches
}

# Verbose
VERBOSE = True

# Regex patterns
harmony_patch_re = re.compile(r'\[HarmonyPatch\s*\(\s*typeof\s*\(\s*([\w\.]+)\s*\)')
class_re = re.compile(r'\bclass\s+(\w+)')


def get_relative_path(root, path) -> str:
    """Get Relative Path."""
    return os.path.relpath(path, root).replace(os.sep, '/')

def is_ignored_file(relpath) -> bool:
    """Check if a file should be skipped based on its relative path."""
    return relpath in IGNORED_FILES

def is_ignored_dir(relpath) -> bool:
    """Check if a directory should be skipped based on its relative path."""
    return relpath in IGNORED_DIRS

def check_file(path) -> list[str]:
    issues = []
    depth = 0
    classes = []

    with open(path, 'r', encoding='utf-8', errors='ignore') as f:
        for lineno, line in enumerate(f, start=1):
            # Track class declarations with their nesting depth
            for m in class_re.finditer(line):
                classes.append({'name': m.group(1), 'depth': depth})

            # Check for HarmonyPatch attributes
            for m in harmony_patch_re.finditer(line):
                full_target = m.group(1)
                target = full_target.split('.')[-1]

                # Find the enclosing Hook class
                hook = None
                for cls in reversed(classes):
                    if cls['name'].endswith('Hook') and cls['depth'] <= depth:
                        hook = cls['name']
                        break

                # Validate or ignore
                if hook:
                    # Skip whitelisted pairs
                    if (hook, target) in IGNORED_PATCHES:
                        if VERBOSE:
                            print(f"{path}: Allowed: Hook '{hook}' patches '{target}'")
                        continue

                    base = hook[:-4]  # strip "Hook"
                    if base != target:
                        issues.append(f"{path}:{lineno}: Hook '{hook}' patches '{target}'")
                else:
                    issues.append(f"{path}:{lineno}: patch '{target}' outside any Hook class")

            # Update brace depth
            depth += line.count('{') - line.count('}')

            # Pop classes when exiting their scope
            while classes and classes[-1]['depth'] >= depth:
                classes.pop()

    return issues


def main():
    if len(sys.argv) != 2:
        print(f"Usage: {sys.argv[0]} <root-directory>", file=sys.stderr)
        sys.exit(1)

    root = sys.argv[1]
    all_issues = []

    for dirpath, _, files in os.walk(root):
        if is_ignored_dir(get_relative_path(root, dirpath)):
            if VERBOSE:
                print(f"{dirpath}: Ignored")
        else:
            for fn in files:
                filepath = os.path.join(dirpath, fn)
                if is_ignored_file(get_relative_path(root, filepath)):
                    if VERBOSE:
                        print(f"{filepath}: Ignored")
                elif fn.endswith('.cs'):
                    all_issues += check_file(filepath)

    for issue in all_issues:
        print(issue)

    sys.exit(1 if all_issues else 0)


if __name__ == '__main__':
    main()
