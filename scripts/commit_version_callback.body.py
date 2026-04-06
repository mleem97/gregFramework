"""Rewrite commit message subject lines using scripts/version-map.json (git hook body)."""

import json
import re
from pathlib import Path

_MAP = json.loads(Path("scripts/version-map.json").read_text(encoding="utf-8"))


def _strip_leading_version(subject: str) -> str:
    s = subject.strip()
    s = re.sub(
        r"^\s*(?:chore\s*\(release\)\s*:\s*)?v?\d+\.\d+\.\d+[^\s:]*\s*:\s*",
        "",
        s,
        flags=re.I,
    )
    s = re.sub(
        r"^\s*(?:chore\s*\(release\)\s*:\s*)?v?\d+\.\d+\.\d+[^\s]*\s*",
        "",
        s,
        flags=re.I,
    )
    return s.strip()


oid = commit.original_id
if oid:
    key = oid.decode("ascii") if isinstance(oid, bytes) else str(oid)
    ver = _MAP.get(key)
    if ver:
        msg = commit.message.decode("utf-8", errors="replace")
        lines = msg.split("\n")
        first = lines[0] if lines else ""
        rest = _strip_leading_version(first)
        lines[0] = f"{ver}: {rest}" if rest else ver
        commit.message = "\n".join(lines).encode("utf-8")
