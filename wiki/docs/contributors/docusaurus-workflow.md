---
id: docusaurus-workflow
title: Docusaurus Contributor Workflow
slug: /contributors/docusaurus-workflow
---

## Local workflow

```bash
npm install
npm run start
```

## Build workflow

```bash
npm run build
npm run serve
```

## Can we hide Docusaurus build stuff from non-contributors?

Short answer for a **public repo**: **not fully**.

What you can do:

- Keep generated output (`build/`, `.docusaurus/`, `node_modules/`) out of Git using `.gitignore`.
- Put docs tooling under `wiki/` so core runtime contributors can ignore it.
- Use path-based CODEOWNERS to limit review noise.
- Trigger docs CI only on `wiki/**` changes.

What you cannot do in a public repo:

- Fully hide tracked source files from non-contributors.

If you need true visibility restriction, use a private repo/submodule for docs infra.
