param(
    [Parameter()]
    [string]$UpstreamRepoUrl = 'https://github.com/Joniii11/DataCenter-RustBridge.git',

    [Parameter()]
    [string]$UpstreamRef = 'main',

    [Parameter()]
    [ValidateSet('report', 'staged', 'overwrite')]
    [string]$Mode = 'report',

    [Parameter()]
    [string]$SourceSubPath = 'csharp/DataCenterModLoader',

    [Parameter()]
    [string]$TargetPath = 'FrikaMF/JoniMF',

    [Parameter()]
    [string]$StagingPath = '.bridge-sync/staged/JoniMF',

    [Parameter()]
    [string]$ReportPath = '.bridge-sync/reports/rustbridge-sync-report.md',

    [Parameter()]
    [string]$StatePath = '.bridge-sync/state/rustbridge-last-sync.json',

    [Parameter()]
    [string]$UseExistingClonePath,

    [Parameter()]
    [switch]$AllowOverwrite,

    [Parameter()]
    [switch]$SkipCleanup
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    $PSScriptRoot
}
elseif (-not [string]::IsNullOrWhiteSpace($PSCommandPath)) {
    Split-Path -Parent $PSCommandPath
}
else {
    (Get-Location).Path
}

$repoRoot = Split-Path -Parent $scriptRoot
$targetRoot = Join-Path $repoRoot $TargetPath
$stagingRoot = Join-Path $repoRoot $StagingPath
$reportFile = Join-Path $repoRoot $ReportPath
$stateFile = Join-Path $repoRoot $StatePath

if (-not (Test-Path -LiteralPath $targetRoot)) {
    throw "Target path not found: $targetRoot"
}

if ($Mode -eq 'overwrite' -and -not $AllowOverwrite) {
    throw "Mode 'overwrite' requires -AllowOverwrite."
}

$gitCmd = Get-Command git -ErrorAction SilentlyContinue
if ($null -eq $gitCmd) {
    throw 'git is required but was not found in PATH.'
}

$renameMap = @{
    'FFIBridge.cs' = 'FfiBridge.cs'
    'GameAPI.cs' = 'GameApi.cs'
    'EventSystem.cs' = 'EventDispatcher.cs'
}

$cloneRoot = $null
$sourceRoot = $null
$tempRoot = $null

if (-not [string]::IsNullOrWhiteSpace($UseExistingClonePath)) {
    $cloneRoot = Join-Path $repoRoot $UseExistingClonePath
    if (-not (Test-Path -LiteralPath $cloneRoot)) {
        throw "UseExistingClonePath does not exist: $cloneRoot"
    }

    $sourceRoot = Join-Path $cloneRoot $SourceSubPath
    if (-not (Test-Path -LiteralPath $sourceRoot)) {
        throw "Source sub path not found in existing clone: $sourceRoot"
    }

    Push-Location $cloneRoot
    try {
        $upstreamCommit = (& git rev-parse HEAD).Trim()
        $upstreamBranch = (& git branch --show-current).Trim()
    }
    finally {
        Pop-Location
    }
}
else {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('FrikaRustBridgeSync_' + [guid]::NewGuid().ToString('N'))
    $cloneRoot = Join-Path $tempRoot 'rustbridge'

    New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

    & git clone --depth 1 --branch $UpstreamRef $UpstreamRepoUrl $cloneRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to clone upstream repository: $UpstreamRepoUrl"
    }

    Push-Location $cloneRoot
    try {
        $upstreamCommit = (& git rev-parse HEAD).Trim()
        $upstreamBranch = (& git branch --show-current).Trim()
    }
    finally {
        Pop-Location
    }

    $sourceRoot = Join-Path $cloneRoot $SourceSubPath
    if (-not (Test-Path -LiteralPath $sourceRoot)) {
        throw "Source sub path not found in upstream repository: $sourceRoot"
    }
}

$sourceFiles = Get-ChildItem -LiteralPath $sourceRoot -File -Filter '*.cs' | Sort-Object Name
if ($sourceFiles.Count -eq 0) {
    throw "No C# source files found at upstream source path: $sourceRoot"
}

New-Item -ItemType Directory -Path (Split-Path -Parent $reportFile) -Force | Out-Null
New-Item -ItemType Directory -Path (Split-Path -Parent $stateFile) -Force | Out-Null
if ($Mode -eq 'staged') {
    New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null
}

$reportRows = New-Object System.Collections.Generic.List[string]
$copyActions = New-Object System.Collections.Generic.List[object]

foreach ($file in $sourceFiles) {
    $destinationName = if ($renameMap.ContainsKey($file.Name)) { $renameMap[$file.Name] } else { $file.Name }
    $destinationPath = Join-Path $targetRoot $destinationName
    $destinationExists = Test-Path -LiteralPath $destinationPath

    $action = 'inspect-only'
    $notes = ''
    $outputPath = ''

    if ($Mode -eq 'report') {
        if ($destinationExists) {
            $action = 'blocked-existing'
            $notes = 'existing target file would be overwritten (report mode blocks)'
        }
        else {
            $action = 'candidate-new'
            $notes = 'new file candidate'
        }
    }
    elseif ($Mode -eq 'staged') {
        if ($destinationExists) {
            $action = 'staged-review'
            $outputPath = Join-Path $stagingRoot ($destinationName + '.upstream.cs')
            Copy-Item -LiteralPath $file.FullName -Destination $outputPath -Force
            $notes = 'staged copy for manual merge review; target untouched'
        }
        else {
            $action = 'added-new'
            Copy-Item -LiteralPath $file.FullName -Destination $destinationPath -Force
            $outputPath = $destinationPath
            $notes = 'new file added (no overwrite)'
        }
    }
    elseif ($Mode -eq 'overwrite') {
        if ($destinationExists) {
            $action = 'overwritten'
            $notes = 'existing file overwritten by upstream'
        }
        else {
            $action = 'added-new'
            $notes = 'new file added from upstream'
        }

        Copy-Item -LiteralPath $file.FullName -Destination $destinationPath -Force
        $outputPath = $destinationPath
    }

    $finalDestination = if ([string]::IsNullOrWhiteSpace($outputPath)) { $destinationPath } else { $outputPath }

    $copyActions.Add([pscustomobject]@{
            Source = $file.FullName
            Destination = $finalDestination
            Action = $action
            Notes = $notes
        }) | Out-Null

    $safeSource = $file.FullName.Replace($repoRoot + [System.IO.Path]::DirectorySeparatorChar, '')
    $safeDestination = $finalDestination.Replace($repoRoot + [System.IO.Path]::DirectorySeparatorChar, '')
    $reportRows.Add("| $safeSource | $safeDestination | $action | $notes |") | Out-Null
}

$nowUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss UTC')
$stateObject = [pscustomobject]@{
    syncedAtUtc = $nowUtc
    upstreamRepoUrl = $UpstreamRepoUrl
    upstreamBranch = $upstreamBranch
    upstreamCommit = $upstreamCommit
    mode = $Mode
}
$stateObject | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $stateFile -Encoding UTF8

$reportContent = @()
$reportContent += '# RustBridge Sync Report'
$reportContent += ''
$reportContent += "- Synced At: $nowUtc"
$reportContent += "- Upstream Repo: $UpstreamRepoUrl"
$reportContent += "- Upstream Branch: $upstreamBranch"
$reportContent += "- Upstream Commit: $upstreamCommit"
$reportContent += "- Mode: $Mode"
$reportContent += "- Target Path: $TargetPath"
$reportContent += "- Staging Path: $StagingPath"
$reportContent += ''
$reportContent += '## File Actions'
$reportContent += ''
$reportContent += '| Source | Destination | Action | Notes |'
$reportContent += '|---|---|---|---|'
$reportContent += $reportRows
$reportContent += ''
$reportContent += '## Safety Notes'
$reportContent += ''
$reportContent += '- `report` mode never modifies target files.'
$reportContent += '- `staged` mode never overwrites existing target files.'
$reportContent += '- `overwrite` mode requires explicit `-AllowOverwrite`.'

$reportContent | Set-Content -LiteralPath $reportFile -Encoding UTF8

$summary = $copyActions | Group-Object Action | Sort-Object Name | ForEach-Object { "{0}={1}" -f $_.Name, $_.Count }
Write-Host "[RustBridgeSync] Completed. Actions: $($summary -join ', ')"
Write-Host "[RustBridgeSync] Report: $reportFile"

if (-not [string]::IsNullOrWhiteSpace($UseExistingClonePath)) {
    Write-Host "[RustBridgeSync] Existing clone mode used: $UseExistingClonePath"
}

if ($null -ne $tempRoot -and -not $SkipCleanup) {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
