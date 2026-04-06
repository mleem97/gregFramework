# WorkshopUploader

Desktop helper for **Steam Workshop** (Steamworks integration pending) and **DevServer betas** at `https://gregframework.eu`.

## Build

```bash
dotnet build WorkshopUploader.csproj -c Debug
```

Targets **.NET 9** with **.NET MAUI** (Windows).

## Publish (single-file, win10-x64)

```bash
dotnet publish WorkshopUploader.csproj -c Release
```

Output: `bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/WorkshopUploader.exe`

To ship the name `full.exe`, rename the published exe or add a publish profile.

## Deploy next to the game

Copy the publish folder to:

`{GameRoot}/WorkshopUploader/`

so it sits alongside the game executable (not inside `Mods` or `MelonLoader`).

See also: [docs/Steam-Workshop-and-Tooling.md](../docs/Steam-Workshop-and-Tooling.md) and [docs/devserver-betas.md](../docs/devserver-betas.md).
