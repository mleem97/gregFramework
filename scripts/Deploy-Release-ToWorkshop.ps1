#Requires -Version 7
<#
.SYNOPSIS
  Builds all framework, plugin, and mod projects, then packages each into a
  Steamworks-compatible Workshop project folder under <GameRoot>/workshop.

.DESCRIPTION
  Each Workshop project gets:
    <project>/content/<target path>/<assembly>.dll
    <project>/metadata.json  (title, description, tags, visibility)

  The content/ folder mirrors the game directory structure:
    - Framework DLL:     content/Mods/
    - Plugin DLLs:       content/FMF/Plugins/
    - Standalone mods:   content/Mods/

  After running, use the WorkshopUploader UI or CLI to publish each folder.

.EXAMPLE
  pwsh -File scripts/Deploy-Release-ToWorkshop.ps1
.EXAMPLE
  pwsh -File scripts/Deploy-Release-ToWorkshop.ps1 -GameDir 'D:\Games\Data Center'
#>
param(
    [string]$GameDir = $env:DATA_CENTER_GAME_DIR,
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

#region Resolve game directory
if ([string]::IsNullOrWhiteSpace($GameDir)) {
    $findScript = Join-Path $RepoRoot 'scripts\Find-DataCenterPath.ps1'
    if (Test-Path -LiteralPath $findScript) {
        $GameDir = (& pwsh -NoProfile -ExecutionPolicy Bypass -File $findScript | Select-Object -Last 1).Trim()
    }
}
if ([string]::IsNullOrWhiteSpace($GameDir) -or -not (Test-Path -LiteralPath $GameDir)) {
    throw "Data Center game folder not found. Pass -GameDir or set DATA_CENTER_GAME_DIR."
}
$WorkshopRoot = Join-Path $GameDir 'workshop'
Write-Host "[workshop] WorkshopRoot = $WorkshopRoot"
#endregion

#region Build all projects
$AllProjects = @(
    'framework\FrikaMF.csproj',
    'plugins\FFM.Plugin.Multiplayer\FFM.Plugin.Multiplayer.csproj',
    'plugins\FFM.Plugin.Sysadmin\FFM.Plugin.Sysadmin.csproj',
    'plugins\FFM.Plugin.AssetExporter\FFM.Plugin.AssetExporter.csproj',
    'plugins\FFM.Plugin.WebUIBridge\FFM.Plugin.WebUIBridge.csproj',
    'plugins\FFM.Plugin.PlayerModels\FFM.Plugin.PlayerModels.csproj',
    'mods\FMF.ConsoleInputGuard\FMF.ConsoleInputGuard.csproj',
    'mods\FMF.Mod.GregifyEmployees\FMF.GregifyEmployees.csproj',
    'mods\FMF.Mod.HexLabelMod\FMF.HexLabelMod.csproj',
    'mods\FMF.Plugin.LangCompatBridge\FMF.JoniMLCompatMod.csproj'
)

foreach ($rel in $AllProjects) {
    $proj = Join-Path $RepoRoot $rel
    Write-Host "[workshop] Building $proj"
    & dotnet build $proj -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $proj" }
}
#endregion

$tfm = 'net6.0'

function New-WorkshopProject {
    param(
        [string]$Name,
        [string]$DllSource,
        [string]$ContentSubPath,
        [string]$Title,
        [string]$Description,
        [string[]]$Tags
    )

    $projectDir = Join-Path $WorkshopRoot $Name
    $contentDir = Join-Path $projectDir "content\$ContentSubPath"
    New-Item -ItemType Directory -Path $contentDir -Force | Out-Null

    $dllName = [System.IO.Path]::GetFileName($DllSource)
    Copy-Item -LiteralPath $DllSource -Destination (Join-Path $contentDir $dllName) -Force
    Write-Host "[workshop] $DllSource -> $contentDir\$dllName"

    $meta = @{
        publishedFileId = 0
        title = $Title
        description = $Description
        visibility = 'Private'
        previewImageRelativePath = 'preview.png'
        tags = $Tags
    } | ConvertTo-Json -Depth 3

    $metaPath = Join-Path $projectDir 'metadata.json'
    if (-not (Test-Path -LiteralPath $metaPath)) {
        Set-Content -LiteralPath $metaPath -Value $meta -Encoding utf8NoBOM
        Write-Host "[workshop] Created $metaPath"
    } else {
        Write-Host "[workshop] Skipped $metaPath (already exists)"
    }
}

#region Package framework
$fwDll = Join-Path $RepoRoot "framework\bin\$Configuration\$tfm\FrikaModdingFramework.dll"
if (-not (Test-Path -LiteralPath $fwDll)) { throw "Missing: $fwDll" }
New-WorkshopProject `
    -Name 'FrikaModFramework' `
    -DllSource $fwDll `
    -ContentSubPath 'Mods' `
    -Title 'FrikaModFramework' `
    -Description 'Core modding framework for Data Center. Required by all FMF-based mods and plugins.' `
    -Tags @('modded', 'melonloader', 'framework', 'fmf')
#endregion

#region Package plugins
$plugins = @(
    @{ Id = 'FFM.Plugin.Multiplayer';    Folder = 'plugins\FFM.Plugin.Multiplayer';    Desc = 'Multiplayer support plugin for FrikaModFramework.' },
    @{ Id = 'FFM.Plugin.Sysadmin';       Folder = 'plugins\FFM.Plugin.Sysadmin';       Desc = 'Sysadmin tools and server management plugin.' },
    @{ Id = 'FFM.Plugin.AssetExporter';  Folder = 'plugins\FFM.Plugin.AssetExporter';  Desc = 'Asset export utilities for Data Center modding.' },
    @{ Id = 'FFM.Plugin.WebUIBridge';    Folder = 'plugins\FFM.Plugin.WebUIBridge';    Desc = 'Web UI bridge plugin for in-game browser-based interfaces.' },
    @{ Id = 'FFM.Plugin.PlayerModels';   Folder = 'plugins\FFM.Plugin.PlayerModels';   Desc = 'Custom player models plugin for Data Center.' }
)

foreach ($p in $plugins) {
    $dll = Join-Path $RepoRoot "$($p.Folder)\bin\$Configuration\$tfm\$($p.Id).dll"
    if (-not (Test-Path -LiteralPath $dll)) { throw "Missing: $dll" }
    New-WorkshopProject `
        -Name $p.Id `
        -DllSource $dll `
        -ContentSubPath 'FMF\Plugins' `
        -Title $p.Id `
        -Description $p.Desc `
        -Tags @('modded', 'fmf', 'plugin')
}
#endregion

#region Package mods
$mods = @(
    @{ Name = 'FMF.ConsoleInputGuard';  Folder = 'mods\FMF.ConsoleInputGuard';           Assembly = 'FMF.ConsoleInputGuard';  Desc = 'Console input guard mod for Data Center.' },
    @{ Name = 'FMF.GregifyEmployees';   Folder = 'mods\FMF.Mod.GregifyEmployees';        Assembly = 'FMF.GregifyEmployees';   Desc = 'Gregify Employees gameplay mod.' },
    @{ Name = 'FMF.HexLabelMod';        Folder = 'mods\FMF.Mod.HexLabelMod';             Assembly = 'FMF.HexLabelMod';        Desc = 'Hex label display mod for Data Center.' },
    @{ Name = 'FMF.JoniMLCompatMod';    Folder = 'mods\FMF.Plugin.LangCompatBridge';      Assembly = 'FMF.JoniMLCompatMod';    Desc = 'Language compatibility bridge mod.' }
)

foreach ($m in $mods) {
    $dll = Join-Path $RepoRoot "$($m.Folder)\bin\$Configuration\$tfm\$($m.Assembly).dll"
    if (-not (Test-Path -LiteralPath $dll)) { throw "Missing: $dll" }
    New-WorkshopProject `
        -Name $m.Name `
        -DllSource $dll `
        -ContentSubPath 'Mods' `
        -Title $m.Name `
        -Description $m.Desc `
        -Tags @('modded', 'melonloader', 'mod')
}
#endregion

#region Package Gregtools Modmanager (WorkshopUploader self-contained)
$wuProj = Join-Path $RepoRoot 'workshopuploader\WorkshopUploader.csproj'
Write-Host "[workshop] Publishing $wuProj (self-contained, trimmed)"
& dotnet publish $wuProj -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "WorkshopUploader publish failed" }

$wuPublish = Join-Path $RepoRoot "workshopuploader\bin\$Configuration\net9.0-windows10.0.19041.0\win10-x64\publish"
if (-not (Test-Path -LiteralPath $wuPublish)) { throw "Missing: $wuPublish" }

$mmProject = Join-Path $WorkshopRoot 'Gregtools Modmanager'
$mmContent = Join-Path $mmProject 'content'
New-Item -ItemType Directory -Path $mmContent -Force | Out-Null
Copy-Item -Path (Join-Path $wuPublish '*') -Destination $mmContent -Recurse -Force

#region Strip unnecessary files from Workshop content
$beforeSize = (Get-ChildItem $mmContent -Recurse -File | Measure-Object Length -Sum).Sum
Write-Host "[workshop] Pre-cleanup size: $([math]::Round($beforeSize / 1MB, 1)) MB"

# Debug / diagnostics DLLs not needed at runtime
$debugDlls = @(
    'mscordaccore.dll',
    'mscordaccore_amd64_amd64_*.dll',
    'mscordbi.dll',
    'Microsoft.DiaSymReader.Native.amd64.dll',
    'clrgcexp.dll'
)
foreach ($pattern in $debugDlls) {
    Get-ChildItem $mmContent -Filter $pattern | ForEach-Object {
        Write-Host "[workshop]   Removing debug: $($_.Name)"
        Remove-Item $_.FullName -Force
    }
}

# Features the app doesn't use
$unusedDlls = @(
    'msquic.dll',
    'Microsoft.Windows.AI.*.dll',
    'Microsoft.Windows.Widgets.dll',
    'Microsoft.Security.Authentication.OAuth.dll',
    'Microsoft.Windows.Media.Capture.dll'
)
foreach ($pattern in $unusedDlls) {
    Get-ChildItem $mmContent -Filter $pattern | ForEach-Object {
        Write-Host "[workshop]   Removing unused: $($_.Name)"
        Remove-Item $_.FullName -Force
    }
}

# Unused .winmd metadata files (keep only UI/Graphics/Runtime essentials)
$keepWinmd = @(
    'Microsoft.UI.Xaml.winmd',
    'Microsoft.UI.winmd',
    'Microsoft.UI.Text.winmd',
    'Microsoft.Graphics.winmd',
    'Microsoft.Windows.ApplicationModel.WindowsAppRuntime.winmd',
    'Microsoft.Windows.ApplicationModel.DynamicDependency.winmd',
    'Microsoft.Windows.AppLifecycle.winmd',
    'Microsoft.Windows.ApplicationModel.Resources.winmd',
    'Microsoft.Windows.System.Power.winmd'
)
Get-ChildItem $mmContent -Filter '*.winmd' | Where-Object { $_.Name -notin $keepWinmd } | ForEach-Object {
    Write-Host "[workshop]   Removing winmd: $($_.Name)"
    Remove-Item $_.FullName -Force
}

# WinUI locale folders: keep only app-supported locales + en-us
$keepLocales = @('de','de-DE','en-us','en-GB','es','es-ES','it','it-IT',
                 'ja','ja-JP','pl','pl-PL','ru','ru-RU','zh','zh-CN','zh-TW','zh-Hans','zh-Hant')
Get-ChildItem $mmContent -Directory |
    Where-Object { $_.Name -match '^[a-z]{2}(-[A-Za-z]+)?$' -and $_.Name -notin $keepLocales } |
    ForEach-Object {
        Remove-Item $_.FullName -Recurse -Force
    }
$removedLocales = 110 - $keepLocales.Count
Write-Host "[workshop]   Removed ~$removedLocales unused WinUI locale folders"

$afterSize = (Get-ChildItem $mmContent -Recurse -File | Measure-Object Length -Sum).Sum
Write-Host "[workshop] Post-cleanup size: $([math]::Round($afterSize / 1MB, 1)) MB (saved $([math]::Round(($beforeSize - $afterSize) / 1MB, 1)) MB)"
#endregion

Write-Host "[workshop] WorkshopUploader -> $mmContent"

$mmMeta = @{
    publishedFileId = 0
    title = 'Gregtools Modmanager'
    description = 'All-in-one Mod Manager and Workshop Uploader for Data Center. Browse, install, and publish Steam Workshop content. Manage mod dependencies (MelonLoader, FrikaModFramework, plugins) in one place.'
    visibility = 'Private'
    previewImageRelativePath = 'preview.png'
    tags = @('tool', 'modmanager', 'workshop', 'fmf')
} | ConvertTo-Json -Depth 3

$mmMetaPath = Join-Path $mmProject 'metadata.json'
if (-not (Test-Path -LiteralPath $mmMetaPath)) {
    Set-Content -LiteralPath $mmMetaPath -Value $mmMeta -Encoding utf8NoBOM
    Write-Host "[workshop] Created $mmMetaPath"
} else {
    Write-Host "[workshop] Skipped $mmMetaPath (already exists)"
}
#endregion

Write-Host ""
Write-Host "[workshop] Done. All projects packaged under: $WorkshopRoot"
Write-Host "[workshop] Use WorkshopUploader to publish each folder to Steam."
