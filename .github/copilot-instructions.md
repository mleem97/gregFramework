# Copilot Instructions

## General Guidelines
- Use English language for all Markdown documentation files that are not excluded by .gitignore.
- Always use Conventional Commits with atomic commit messages (e.g., feat:, fix:, docs:, chore:) for this repository.
- Use `FrikaModFramework` in repository URLs and metadata.
- Use `https://github.com/mleem97/FrikaModFramework` for repository links.
- Use `https://github.com/mleem97/FrikaModFramework.wiki.git` as the wiki remote target.

## Release and Versioning Rules (Mandatory)
- Use release version format `XX.XX.XXXX` for framework releases.
- Treat `FrikaMF/JoniMF/ReleaseVersion.cs` as the single source of truth for framework release version.
- For automated release prep, use:
	- `pnpm release:prepare:major`
	- `pnpm release:prepare:medium`
	- `pnpm release:prepare:minor`
- Every release version update must prepend a new `CHANGELOG.md` entry automatically via `scripts/Update-ReleaseMetadata.ps1`.
- Local release upload must publish the framework artifact as `FrikaModdingFramework-v{XX.XX.XXXX}.dll`.
- Prefer `pnpm release:publish` for local release upload flow.

## Wiki Sync Rules (Mandatory)
- Keep editable wiki source files under `.wiki/` in this repository.
- Sync `.wiki/` to the GitHub wiki repo with `pnpm wiki:sync`.
- Use `scripts/Sync-Wiki.ps1` for wiki synchronization and do not manually copy wiki files.

## Project-Specific Rules
- Focus on game events and hook candidates within `Assembly-CSharp`, as this is the relevant game assembly.
- Keep runtime bridge code under `FrikaMF/JoniMF`.
- Keep Rust native mods in `Data Center/Mods/RustMods`.
- Keep game object/content packs in `Data Center_Data/StreamingAssets/Mods`.