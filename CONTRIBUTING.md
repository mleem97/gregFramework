# 🤝 Contributing

Thank you for your interest in contributing to `Frikadelle Modding Framework`.

---

## 📌 Core Principles

- Contributions must serve a legitimate modding purpose.
- Do not add features or changes that support copyright infringement, asset theft, or unauthorized redistribution.
- Keep changes small, focused, and easy to review.

## 🤖 AI Policy

- [AI Usage Policy & Disclaimer (Root)](./AI_POLICY.md)
- [AI Usage Policy & Disclaimer (Wiki)](./.wiki/AI-USAGE.md)

If AI tooling was used significantly, disclose it briefly in your PR and confirm manual review/testing.

---

## 🛠 Local Development Workflow

1. Fork the repository and create a branch (`feature/...`, `fix/...`).
1. Implement your changes.
1. If your change includes RustBridge updates, run a safe staged sync first (no overwrite):

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Sync-RustBridge.ps1 -Mode staged
```

1. Build locally:

```sh
dotnet build FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo
```

1. Validate functionality (hotkeys `F8`, `F9`, `F10`).
1. Open a pull request with a clear description.

---

## 🔁 RustBridge Sync Flow

- Upstream reference: [DataCenter-RustBridge](https://github.com/Joniii11/DataCenter-RustBridge.git).
- Default sync mode is `staged` and does not overwrite existing files in `FrikaMF`.
- Staged upstream candidates are written to [`.bridge-sync/staged/JoniMF`](./.bridge-sync/staged/JoniMF) as `*.upstream.cs` for manual merge review.
- Sync report is written to [`.bridge-sync/reports/rustbridge-sync-report.md`](./.bridge-sync/reports/rustbridge-sync-report.md).

## 🔍 CodeQL C# scanning

- Prefer the dedicated `CodeQL C#` workflow in [`.github/workflows/codeql-csharp.yml`](./.github/workflows/codeql-csharp.yml).
- It uses CodeQL `build-mode: manual` so the scan can resolve more types and call targets than `build-mode: none`.
- On CI, C# projects are built with `/p:CI=true`; `FMF.UIReplacementMod` also disables its React export during analysis with `/p:EnableReactBuildOnBuild=false`.
- If a project still cannot compile on hosted runners because local MelonLoader/game references are missing, the workflow keeps going so CodeQL can still analyze successfully built projects.

For manual pull using an already cloned upstream copy:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Sync-RustBridge.ps1 -Mode staged -UseExistingClonePath _tmp\rustbridge-upstream
```

Only use overwrite mode intentionally:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Sync-RustBridge.ps1 -Mode overwrite -AllowOverwrite
```

---

## 🧩 Coding Guidelines

- Keep the existing style in `Main.cs`.
- Do not add unnecessary dependencies.
- Preserve IL2CPP/MelonLoader compatibility.

---

## ✅ Pull Request Checklist

- [ ] Build is successful.
- [ ] Change is limited to the intended scope.
- [ ] README/docs updated where necessary.
- [ ] No ethically or legally problematic content.

---

## 📦 Release Notes for Maintainers

Release DLLs are uploaded from local builds, not from GitHub Actions build jobs.

```powershell
. .\scripts\Publish-LocalRelease.ps1
$env:GITHUB_TOKEN = "<github_token_with_repo_scope>"
Publish-LocalRelease -Tag "vX.Y.Z"
```
