#!/usr/bin/env python3
"""
Create GitHub issues from unchecked lines in docs/IDEA_BACKLOG.md using the gh CLI.

Prerequisites:
  - gh auth login
  - Label "status:pending-analysis" (create once: gh label create "status:pending-analysis" --color BFD4F2)

Usage:
  python tools/auto_issue_creator.py [--dry-run]
"""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
from pathlib import Path


def _repo_root() -> Path:
    return Path(__file__).resolve().parent.parent


def _parse_backlog(path: Path) -> list[tuple[int, str]]:
    """Return list of (line_index, idea_text) for unchecked items under ## Pending."""
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    in_pending = False
    items: list[tuple[int, str]] = []
    for i, line in enumerate(lines):
        if line.strip().startswith("## "):
            in_pending = line.strip().lower().startswith("## pending")
            continue
        if not in_pending:
            continue
        m = re.match(r"^\s*-\s*\[\s*\]\s*(.+)$", line)
        if m:
            items.append((i, m.group(1).strip()))
    return items


def _replace_line_checked(lines: list[str], index: int, issue_url: str) -> None:
    raw = lines[index]
    m = re.match(r"^(\s*-\s*)\[\s*\](\s*)(.*)$", raw)
    if not m:
        return
    prefix, space, rest = m.groups()
    lines[index] = f"{prefix}[x]{space}{rest} (tracked: {issue_url})"


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dry-run", action="store_true", help="Print actions only; do not call gh.")
    args = parser.parse_args()

    root = _repo_root()
    backlog = root / "docs" / "IDEA_BACKLOG.md"
    if not backlog.is_file():
        print(f"Missing {backlog}", file=sys.stderr)
        sys.exit(1)

    items = _parse_backlog(backlog)
    if not items:
        print("No unchecked items under '## Pending'.")
        return

    lines = backlog.read_text(encoding="utf-8").splitlines()
    for idx, idea in items:
        title = idea[:120] if len(idea) > 120 else idea
        body = f"Source: `docs/IDEA_BACKLOG.md` line {idx + 1}\n\n{idea}"
        if args.dry_run:
            print(f"Would create issue: {title!r}")
            continue

        cmd = [
            "gh",
            "issue",
            "create",
            "--title",
            title,
            "--body",
            body,
            "--label",
            "status:pending-analysis",
        ]
        try:
            r = subprocess.run(cmd, cwd=root, capture_output=True, text=True, check=True)
        except FileNotFoundError:
            print("gh CLI not found. Install GitHub CLI: https://cli.github.com/", file=sys.stderr)
            sys.exit(1)
        except subprocess.CalledProcessError as ex:
            print(ex.stderr or ex.stdout, file=sys.stderr)
            sys.exit(ex.returncode)

        url = (r.stdout or "").strip()
        print(f"Created: {url}")
        _replace_line_checked(lines, idx, url)

    if not args.dry_run:
        backlog.write_text("\n".join(lines) + "\n", encoding="utf-8")
        print("Updated backlog checkboxes.")


if __name__ == "__main__":
    main()
