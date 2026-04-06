#Requires -Version 7
<#
.SYNOPSIS
  Builds the in-game stack and copies outputs into the Data Center install for local testing.

.DESCRIPTION
  - FrikaModdingFramework.dll -> {GameRoot}/Mods
  - FFM.Plugin.*.dll           -> {GameRoot}/FMF/Plugins
  - FMF.ModPathRedirector.dll  -> {GameRoot}/Plugins (MelonLoader Plugins folder)

  By default does **not** build or deploy WorkshopUploader (desktop MAUI UI). Use -IncludeWorkshopUploader to add:
  - WorkshopUploader (full win10-x64 folder) -> {GameRoot}/WorkshopUploader

  Game path: -GameDir, or env DATA_CENTER_GAME_DIR, or scripts/Find-DataCenterPath.ps1

.EXAMPLE
  pwsh -File scripts/Deploy-Release-ToDataCenter.ps1
.EXAMPLE
  pwsh -File scripts/Deploy-Release-ToDataCenter.ps1 -GameDir 'D:\Games\Data Center' -IncludeWorkshopUploader
#>
param(
    [string]$GameDir = $env:DATA_CENTER_GAME_DIR,
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [switch]$IncludeWorkshopUploader
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

if ([string]::IsNullOrWhiteSpace($GameDir)) {
    $findScript = Join-Path $RepoRoot 'scripts\Find-DataCenterPath.ps1'
    if (Test-Path -LiteralPath $findScript) {
        $GameDir = (& pwsh -NoProfile -ExecutionPolicy Bypass -File $findScript | Select-Object -Last 1).Trim()
    }
}

if ([string]::IsNullOrWhiteSpace($GameDir) -or -not (Test-Path -LiteralPath $GameDir)) {
    throw "Data Center game folder not found. Pass -GameDir or set DATA_CENTER_GAME_DIR."
}

$GameProjects = @(
    'framework\FrikaMF.csproj',
    'plugins\FFM.Plugin.Multiplayer\FFM.Plugin.Multiplayer.csproj',
    'plugins\FFM.Plugin.Sysadmin\FFM.Plugin.Sysadmin.csproj',
    'plugins\FFM.Plugin.AssetExporter\FFM.Plugin.AssetExporter.csproj',
    'plugins\FFM.Plugin.WebUIBridge\FFM.Plugin.WebUIBridge.csproj',
    'plugins\FFM.Plugin.PlayerModels\FFM.Plugin.PlayerModels.csproj',
    'mods\FMF.ModPathRedirector\FMF.ModPathRedirector.csproj'
)

foreach ($rel in $GameProjects) {
    $proj = Join-Path $RepoRoot $rel
    Write-Host "[deploy] dotnet build $proj -c $Configuration"
    & dotnet build $proj -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed: $LASTEXITCODE" }
}

if ($IncludeWorkshopUploader) {
    $wuProj = Join-Path $RepoRoot 'WorkshopUploader\WorkshopUploader.csproj'
    Write-Host "[deploy] dotnet publish $wuProj -c $Configuration (self-contained)"
    & dotnet publish $wuProj -c $Configuration -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed: $LASTEXITCODE" }
}

$Mods = Join-Path $GameDir 'Mods'
$MlPlugins = Join-Path $GameDir 'Plugins'
$FmfPlugins = Join-Path $GameDir 'FMF\Plugins'
$dirs = @($Mods, $MlPlugins, $FmfPlugins)
if ($IncludeWorkshopUploader) {
    $dirs += (Join-Path $GameDir 'WorkshopUploader')
}
New-Item -ItemType Directory -Path $dirs -Force | Out-Null

$tfm = 'net6.0'
$FwDll = Join-Path $RepoRoot "framework\bin\$Configuration\$tfm\FrikaModdingFramework.dll"
if (-not (Test-Path -LiteralPath $FwDll)) { throw "Missing: $FwDll" }
Copy-Item -LiteralPath $FwDll -Destination (Join-Path $Mods 'FrikaModdingFramework.dll') -Force
Write-Host "[deploy] -> $Mods\FrikaModdingFramework.dll"

$RedirectorDll = Join-Path $RepoRoot "mods\FMF.ModPathRedirector\bin\$Configuration\$tfm\FMF.ModPathRedirector.dll"
if (-not (Test-Path -LiteralPath $RedirectorDll)) { throw "Missing: $RedirectorDll" }
Copy-Item -LiteralPath $RedirectorDll -Destination (Join-Path $MlPlugins 'FMF.ModPathRedirector.dll') -Force
Write-Host "[deploy] -> $MlPlugins\FMF.ModPathRedirector.dll"

$pluginNames = @(
    'FFM.Plugin.Multiplayer',
    'FFM.Plugin.Sysadmin',
    'FFM.Plugin.AssetExporter',
    'FFM.Plugin.WebUIBridge',
    'FFM.Plugin.PlayerModels'
)
foreach ($name in $pluginNames) {
    $src = Join-Path $RepoRoot "plugins\$name\bin\$Configuration\$tfm\$name.dll"
    if (-not (Test-Path -LiteralPath $src)) { throw "Missing: $src" }
    Copy-Item -LiteralPath $src -Destination (Join-Path $FmfPlugins "$name.dll") -Force
    Write-Host "[deploy] -> $FmfPlugins\$name.dll"
}

if ($IncludeWorkshopUploader) {
    $WuTarget = Join-Path $GameDir 'WorkshopUploader'
    $wuSrc = Join-Path $RepoRoot "WorkshopUploader\bin\$Configuration\net9.0-windows10.0.19041.0\win10-x64\publish"
    if (-not (Test-Path -LiteralPath $wuSrc)) { throw "Missing WorkshopUploader publish: $wuSrc" }
    Get-ChildItem -LiteralPath $WuTarget -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Copy-Item -Path (Join-Path $wuSrc '*') -Destination $WuTarget -Recurse -Force
    Write-Host "[deploy] -> $WuTarget (WorkshopUploader, self-contained)"
}

Write-Host "[deploy] Done. GameRoot=$GameDir"
