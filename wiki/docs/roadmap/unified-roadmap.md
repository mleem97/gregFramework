---
id: unified-roadmap
title: Unified Roadmap
slug: /roadmap/unified-roadmap
---

This is the canonical roadmap page for the project. It consolidates previous roadmap documents and removes duplicate planning tracks.

## Status Summary

- **Framework Runtime**: active
- **Docs & Contributor Tooling**: active
- **Mod Distribution & Audit Pipeline**: active planning
- **Multiplayer (Steamworks)**: blocked externally

## Important Multiplayer Note

Multiplayer was planned around **Steamworks P2P** and that direction remains conceptually valid.  
However, it is **currently not implementable in production** because the game developer has not integrated Steamworks support in the game runtime/API surface required for a stable implementation.

## Consolidated Workstreams

### 1) Framework & Hooking

- Stabilize hook coverage and event contracts.
- Keep C# and Rust bridge compatibility versioned.
- Expand diagnostics for mod-runtime compatibility failures.

### 2) Documentation & Information Architecture

- Keep `/wiki/*` as canonical docs namespace.
- Maintain separated Plugin Wiki and Mod Wiki sections.
- Remove duplicate docs by keeping one canonical page per topic.

### 3) Mod/Plugin Catalog & Distribution

- Maintain dynamic `/mods` catalog with links to wiki and release endpoints.
- Keep release metadata visible (version, author, dependencies, language).
- Keep release-state gating (`NotReleasedYet`) explicit for users.

### 4) Security Audit & Submission Workflow

- Accept plugin submissions via Git repository URL.
- Run automated static checks and suspicious-pattern scans.
- Gate publication/release visibility on successful audit outcome.

### 5) Multiplayer Track (Blocked)

- Keep architecture docs for future Steamworks integration readiness.
- Track dependencies on game-side Steamworks implementation.
- Re-open implementation only when game runtime exposes required primitives.

## Legacy Roadmap Documents

The following pages are now legacy references and should not be used as primary planning sources:

- `wiki-import/ROADMAP`
- `wiki-import/Steamworks-P2P-Multiplayer-Roadmap`
- `roadmap/mod-store-stages`
