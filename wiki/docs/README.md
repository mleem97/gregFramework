# Where the wiki Markdown lives

The Docusaurus app in this folder uses **`path: ../docs`** (see `wiki/docusaurus.config.js`). All browsable Markdown/MDX sources are under the **repository root** [`docs/`](../docs/), not under `wiki/docs/`.

| Content | Path |
|--------|------|
| Legacy GitHub Wiki import | [`docs/wiki-import/`](../docs/wiki-import/) |
| Sync from `.wiki` | Run `node scripts/sync-wiki-to-docs.mjs` from `wiki/` (requires a `.wiki/` directory at the repo root) |

The **Legacy wiki import** section in the sidebar lists the full `docs/wiki-import/` tree (not only a single landing page).
