#!/usr/bin/env python3
"""
Append Discord command ideas to docs/IDEA_BACKLOG.md.

Usage:
  set DISCORD_BOT_TOKEN=your_bot_token
  python tools/discord_bridge.py

Requires: pip install -r tools/requirements-discord.txt
"""

from __future__ import annotations

import os
import sys
from pathlib import Path


def _repo_root() -> Path:
    return Path(__file__).resolve().parent.parent


def main() -> None:
    token = os.environ.get("DISCORD_BOT_TOKEN", "").strip()
    if not token:
        print("Set environment variable DISCORD_BOT_TOKEN.", file=sys.stderr)
        sys.exit(1)

    try:
        import discord
        from discord.ext import commands
    except ImportError as ex:
        print("Install dependencies: pip install -r tools/requirements-discord.txt", file=sys.stderr)
        raise ex

    backlog = _repo_root() / "docs" / "IDEA_BACKLOG.md"

    intents = discord.Intents.default()
    intents.message_content = True
    bot = commands.Bot(command_prefix="!", intents=intents)

    @bot.command(name="request")
    async def request(ctx: commands.Context, *, idea: str) -> None:
        line = f"\n- [ ] {idea.strip()} (requested by {ctx.author})"
        backlog.parent.mkdir(parents=True, exist_ok=True)
        if not backlog.is_file():
            backlog.write_text("# Idea backlog\n\n## Pending\n", encoding="utf-8")
        with backlog.open("a", encoding="utf-8") as f:
            f.write(line)
        await ctx.send(f"Added to backlog: {idea.strip()}")

    @bot.event
    async def on_ready() -> None:
        print(f"Logged in as {bot.user} (use !request <text>)")

    bot.run(token)


if __name__ == "__main__":
    main()
