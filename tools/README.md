# Repository tools

## `Generate-FmfHookCatalog.ps1`

Generates the Docusaurus page [`docs-site/docs/reference/fmf-hooks-catalog.md`](../docs-site/docs/reference/fmf-hooks-catalog.md) from:

- `FrikaMF/HookNames.cs` — string literals and `EventId` → hook mappings
- `FrikaMF/EventIds.cs` — numeric event id constants

Run from repo root:

```powershell
./tools/Generate-FmfHookCatalog.ps1
```

Optional:

```powershell
./tools/Generate-FmfHookCatalog.ps1 -SkipChangelogNote
```

Commit the regenerated `.md` when hook names or event ids change.
