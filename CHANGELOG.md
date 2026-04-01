# 📒 Changelog

All notable project changes are documented in this file.

---

## [Unreleased]

### ✨ Added

- Structured export folders: `Models`, `Textures`, `Sprites`, `Materials`, `Scripts`, `Settings`.
- Optional export of unused assets to `NotUsed/Models` and `NotUsed/Textures`.
- Additional files: `components.txt`, `materials.txt`, `objects.txt`, `summary.txt`.

### 🔧 Changed

- Improved IL2CPP runtime compatibility (string comparison without problematic overloads).
- Build configuration: excluded `il2cpp-unpack` files from compilation.

### 🐛 Fixed

- Reduced/fixed failures related to `ReadOnlySpan<T>.GetPinnableReference()` in the export path.

---

## [Historical Commits]

### 🧾 Migrated to Conventional Commits

- `chore: initialize repository metadata files`
- `feat: add initial DataCenterExporter project and core mod code`
- `feat: extend export structure and beta export functionality`
- `docs: add documentation set and build/changelog updates`
