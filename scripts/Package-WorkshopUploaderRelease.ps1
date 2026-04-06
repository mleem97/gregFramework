#Requires -Version 7
<#
.SYNOPSIS
  Publishes WorkshopUploader (self-contained win10-x64) and zips the output for distribution.

.EXAMPLE
  pwsh -File scripts/Package-WorkshopUploaderRelease.ps1
.EXAMPLE
  pwsh -File scripts/Package-WorkshopUploaderRelease.ps1 -Configuration Release -OutputDir D:\dist
#>
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [string]$OutputDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$Proj = Join-Path $RepoRoot 'WorkshopUploader\WorkshopUploader.csproj'

$csprojXml = [xml](Get-Content -LiteralPath $Proj -Raw)
$version = ($csprojXml.Project.PropertyGroup | ForEach-Object { $_.ApplicationDisplayVersion } | Where-Object { $_ } | Select-Object -First 1).Trim()
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = '0.0.0'
}

Write-Host "[pack] dotnet publish $Proj -c $Configuration"
& dotnet publish $Proj -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$publishDir = Join-Path $RepoRoot "WorkshopUploader\bin\$Configuration\net9.0-windows10.0.19041.0\win10-x64\publish"
if (-not (Test-Path -LiteralPath $publishDir)) {
    throw "Publish output not found: $publishDir"
}

$artifacts = if ($OutputDir) {
    $OutputDir
} else {
    Join-Path $RepoRoot 'artifacts'
}
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null

$folderName = "Gregtools-Modmanager-$version-win10-x64"
$zipName = "$folderName.zip"
$zipPath = Join-Path $artifacts $zipName

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("gregtools-zip-" + [guid]::NewGuid().ToString('N'))
$innerPath = Join-Path $tempRoot $folderName
try {
    New-Item -ItemType Directory -Path $innerPath -Force | Out-Null
    Copy-Item -Path (Join-Path $publishDir '*') -Destination $innerPath -Recurse -Force
    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }
    Compress-Archive -Path $innerPath -DestinationPath $zipPath -CompressionLevel Optimal
}
finally {
    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}

$sizeMb = [math]::Round((Get-Item -LiteralPath $zipPath).Length / 1MB, 2)
Write-Host ""
Write-Host "[pack] OK: $zipPath ($sizeMb MB)"
Write-Host "[pack] Inner folder in archive: $folderName\"
