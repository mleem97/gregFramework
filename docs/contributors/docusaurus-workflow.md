---
id: docusaurus-workflow
title: Docusaurus Contributor Workflow
slug: /contributors/docusaurus-workflow
---

## Local workflow

Markdown and MDX live in the repo-root `docs/` folder. The Docusaurus app is in `wiki/`.

```bash
cd wiki
npm install
npm run start
```

## Build workflow

```bash
cd wiki
npm run build
npm run serve
```

## Can we hide Docusaurus build stuff from non-contributors?

Short answer for a **public repo**: **not fully**.

What you can do:

- Keep generated output (`build/`, `.docusaurus/`, `node_modules/`) out of Git using `.gitignore`.
- Put docs tooling under `wiki/` so core runtime contributors can ignore it; content stays in `docs/`.
- Use path-based CODEOWNERS to limit review noise.
- Trigger docs CI on `docs/**` and `wiki/**` changes.

What you cannot do in a public repo:

- Fully hide tracked source files from non-contributors.

If you need true visibility restriction, use a private repo/submodule for docs infra.
