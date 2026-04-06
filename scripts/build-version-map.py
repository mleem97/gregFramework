#!/usr/bin/env python3
"""Build semver prefix per commit (oldest first), starting at 0.0.1. Writes scripts/version-map.json."""
from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]


def run_git(*args: str) -> str:
    return subprocess.check_output(["git", *args], cwd=REPO, text=True)


def classify(subject: str) -> str:
    low = subject.lower()
    if "breaking change" in low or "breaking:" in low or "breaking(" in low:
        return "major"
    if low.startswith("merge "):
        return "patch"
    m = re.match(
        r"^(feat|fix|docs|chore|ci|style|refactor|perf|test|build|revert|feat\([^)]+\)|fix\([^)]+\)|chore\([^)]+\)|docs\([^)]+\)|ci\([^)]+\))\s*[:!]",
        low,
    )
    if m:
        token = m.group(1).split("(")[0]
        if "!" in subject.split(":", 1)[0]:
            return "major"
        if token == "feat" or token.startswith("feat"):
            return "minor"
        return "patch"
    if low.startswith("feat") or low.startswith("feature"):
        return "minor"
    if low.startswith("add ") or low.startswith("implement"):
        return "minor"
    return "patch"


def bump(major: int, minor: int, patch: int, kind: str) -> tuple[int, int, int]:
    if kind == "major":
        return major + 1, 0, 0
    if kind == "minor":
        return major, minor + 1, 0
    return major, minor, patch + 1


def main() -> None:
    out_path = REPO / "scripts" / "version-map.json"
    hashes = run_git("rev-list", "--reverse", "HEAD").splitlines()
    mapping: dict[str, str] = {}
    major, minor, patch = 0, 0, 1

    for i, h in enumerate(hashes):
        subj = run_git("log", "-1", "--format=%s", h).strip()
        if i == 0:
            mapping[h] = "0.0.1"
            continue
        kind = classify(subj)
        major, minor, patch = bump(major, minor, patch, kind)
        mapping[h] = f"{major}.{minor}.{patch}"

    out_path.write_text(json.dumps(mapping, indent=0) + "\n", encoding="utf-8")
    print(f"Wrote {len(mapping)} entries to {out_path}", file=sys.stderr)


if __name__ == "__main__":
    main()
