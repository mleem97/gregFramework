---
id: wiki-mapping
title: Wiki to Docusaurus Mapping
slug: /reference/wiki-mapping
---

## Why this mapping exists

The repository currently stores canonical docs in `.wiki/`. This page defines how those pages are grouped in Docusaurus.

## Audience buckets

- **Newbies**: install/update/troubleshooting, legal basics
- **Intermediates**: hooking, bridge usage, mod config, workflows
- **Pros**: architecture, runtime internals, release system, roadmap

## Suggested migration order

1. Start with onboarding pages (`Home`, End-User, Mod-Developer).
2. Migrate runtime references (`Architecture`, `FFI-Bridge-Reference`, `Web-UI-Bridge`).
3. Migrate governance pages (`Contributors`, Roadmap, Tasklist).
4. Keep bilingual mirrors where needed.

## Sync strategy

Use `npm run wiki:sync` to copy `.wiki/*.md` into a generated docs area for review-based migration.
