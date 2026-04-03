param(
    [Parameter()]
    [string]$WikiRepoUrl = 'https://github.com/mleem97/FrikaModFramework.wiki.git',

    [Parameter()]
    [string]$SourceWikiPath = '.wiki',

    [Parameter()]
    [string]$CommitMessage = 'docs(wiki): sync wiki content from repository .wiki folder',

    [Parameter()]
    [string]$GitHubToken,

    [Parameter()]
    [string]$GitHubActor,

    [Parameter()]
    [switch]$SkipPush
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
$sourcePath = Join-Path $repoRoot $SourceWikiPath

if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw "Source wiki path not found: $sourcePath"
}

$gitCmd = Get-Command git -ErrorAction SilentlyContinue
if ($null -eq $gitCmd) {
    throw 'git is required but was not found in PATH.'
}

$cloneUrl = $WikiRepoUrl
if (-not [string]::IsNullOrWhiteSpace($GitHubToken)) {
    if ($WikiRepoUrl -notmatch '^https://') {
        throw 'GitHubToken auth currently requires an https WikiRepoUrl.'
    }

    $tokenUser = if (-not [string]::IsNullOrWhiteSpace($GitHubActor)) { $GitHubActor } else { 'x-access-token' }
    $encodedToken = [System.Uri]::EscapeDataString($GitHubToken)
    $cloneUrl = $WikiRepoUrl -replace '^https://', ("https://$tokenUser:$encodedToken@")
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('FrikaWikiSync_' + [guid]::NewGuid().ToString('N'))
$targetPath = Join-Path $tempRoot 'wiki'

try {
    New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

    Write-Host "[WikiSync] Cloning wiki repository: $WikiRepoUrl"
    & git clone $cloneUrl $targetPath
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to clone wiki repository: $WikiRepoUrl"
    }

    Write-Host "[WikiSync] Mirroring files from $sourcePath"
    $robocopySource = $sourcePath
    $robocopyTarget = $targetPath
    & robocopy $robocopySource $robocopyTarget /MIR /R:1 /W:1 /NFL /NDL /NP /XF '.git' '.gitignore' /XD '.git' | Out-Null
    $robocopyExit = $LASTEXITCODE
    if ($robocopyExit -ge 8) {
        throw "robocopy failed with exit code $robocopyExit"
    }

    Push-Location $targetPath
    try {
        & git add -A
        if ($LASTEXITCODE -ne 0) {
            throw 'git add failed while preparing wiki sync commit.'
        }

        $status = (& git status --porcelain)
        if ([string]::IsNullOrWhiteSpace(($status | Out-String))) {
            Write-Host '[WikiSync] No wiki changes detected. Nothing to commit.'
            return
        }

        & git commit -m $CommitMessage
        if ($LASTEXITCODE -ne 0) {
            throw 'git commit failed for wiki sync.'
        }

        if (-not $SkipPush) {
            & git push origin HEAD
            if ($LASTEXITCODE -ne 0) {
                throw 'git push failed for wiki sync.'
            }

            Write-Host '[WikiSync] Wiki sync completed and pushed.'
        }
        else {
            Write-Host '[WikiSync] Commit created locally in temp clone (push skipped).'
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
