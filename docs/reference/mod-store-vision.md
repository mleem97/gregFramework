---
id: mod-store-vision
title: Mod Store Vision (Secure Git-Based)
slug: /reference/mod-store-vision
---

## Vision

Build a Mod Store with Git-based submissions, automated malware scanning, and policy checks before mods are visible.

## Core principles

- All submissions are reviewable in Git.
- Every build artifact is reproducible from source.
- Security checks are mandatory before publish.

## Security pipeline (target)

1. Submit via pull request (manifest + source + checksums).
2. CI runs static checks and signature/metadata validation.
3. Virus scanning runs on produced artifacts.
4. Maintainer review and policy checks.
5. Publish approved metadata index consumed by launcher/site.

## Recommended metadata fields

- `modId`, `version`, `authors`
- `supportedGameVersions`
- `requiredFrameworkVersion`
- `sha256` for each downloadable artifact
- source URL and license
