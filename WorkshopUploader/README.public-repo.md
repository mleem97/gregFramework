# GregTools Modmanager (Workshop Uploader)

Windows **.NET MAUI** app for **Data Center** Steam Workshop: Mod Store, local upload projects, `metadata.json` / `content/config.json`, and Steam publish (Facepunch.Steamworks).

## Build

```powershell
dotnet build WorkshopUploader.sln -c Release
```

```powershell
dotnet publish WorkshopUploader.csproj -c Release -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
```

See the original `README.md` in this directory for troubleshooting, Steam layout, and headless CLI.

## License

Use the same license as the parent [DataCenterExporter / gregFramework](https://github.com/mleem97/gregFramework) project unless you add a dedicated `LICENSE` to this repository.
