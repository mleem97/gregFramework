# FMF.BasedModTemplate

Use this template to create an FMF-based standalone mod for **Data Center**.

## What this template includes

- `FMF.BasedModTemplate.csproj`: Build setup with auto-detection for the game directory.
- `Main.cs`: Minimal FMF-based mod entrypoint (`MelonMod`) with framework checks.

## Requirements

- MelonLoader installed in the game.
- Generated IL2CPP assemblies (`MelonLoader/Il2CppAssemblies`).
- `FrikaModdingFramework.dll` available at runtime in `Data Center/Mods`.

## Quick usage

1. Copy this folder and rename it (for example `MyCompany.MyMod`).
1. Rename `FMF.BasedModTemplate.csproj` and update `AssemblyName`, `RootNamespace`, and class names.
1. Build:

```powershell
dotnet build .\Templates\FMF.BasedModTemplate\FMF.BasedModTemplate.csproj /p:GameDir="C:\Path\To\Data Center"
```

1. The built DLL is copied to `Data Center/Mods` automatically (local builds).
