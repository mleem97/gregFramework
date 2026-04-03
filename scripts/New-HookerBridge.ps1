param(
    [string]$OutputPath = "FrikaMF\JoniMF\Hooker.cs"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$target = Join-Path $repoRoot $OutputPath

if (Test-Path $target) {
    Write-Host "Hooker bridge already exists: $target"
    Write-Host "No changes made."
    exit 0
}

$stub = @"
using System;
using System.Collections.Generic;

namespace DataCenterModLoader;

public static class Hooker
{
    public static HookerInstallResult InstallByScan(int maxHooks = 500, string harmonyId = "dc.modloader.hooker")
    {
        return new HookerInstallResult(0, 0, 0, new[] { "Hooker scaffold created. Replace with implementation." });
    }

    public static HookerInstallResult InstallFromCatalog(string catalogPath, int maxHooks = 2000, string harmonyId = "dc.modloader.hooker")
    {
        return new HookerInstallResult(0, 0, 0, new[] { "Hooker scaffold created. Replace with implementation." });
    }
}

public sealed class HookerInstallResult
{
    public HookerInstallResult(int scanned, int installed, int failed, IReadOnlyList<string> errors)
    {
        Scanned = scanned;
        Installed = installed;
        Failed = failed;
        Errors = errors;
    }

    public int Scanned { get; }
    public int Installed { get; }
    public int Failed { get; }
    public IReadOnlyList<string> Errors { get; }
}
"@

New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
Set-Content -Path $target -Value $stub -Encoding UTF8
Write-Host "Created hooker bridge scaffold: $target"
