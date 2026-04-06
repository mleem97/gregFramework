---
id: plugin-submission-audit
title: Plugin Submission & Security Audit Workflow
slug: /contributors/plugin-submission-audit
---

## Goal

Provide a repeatable workflow where community authors submit plugins through a Git repository URL, then pass an automated security/quality audit before publication in the wiki and release channels.

## Submission Model

1. Author opens a **Plugin Submission** issue.
2. Author provides a public Git repository URL (`https://...git`).
3. Maintainer triggers the security-audit workflow.

## Automated Audit Steps

- Clone submitted repository in CI.
- Run static scan for suspicious calls and execution vectors.
- Run secret and credential pattern checks.
- Produce an auditable report artifact.

## Release Gate Policy

- If audit result is **fail**, publication is blocked.
- If audit result is **pass**, maintainers can mark module as `releaseReady` and publish wiki/release visibility.

## Multiplayer Clarification

Steamworks multiplayer remains a planned direction but is currently blocked by missing Steamworks implementation on the game developer side.
