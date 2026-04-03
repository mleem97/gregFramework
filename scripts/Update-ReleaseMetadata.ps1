param(
    [Parameter()]
    [ValidateSet('major', 'medium', 'minor')]
    [string]$Bump = 'minor',

    [Parameter()]
    [string]$Version,

    [Parameter()]
    [string]$Summary = 'Automated release metadata update.',

    [Parameter()]
    [switch]$SkipChangelog
)

Set-StrictMode -Version Latest

$script:MetadataScriptRoot = if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    $PSScriptRoot
}
elseif (-not [string]::IsNullOrWhiteSpace($PSCommandPath)) {
    Split-Path -Parent $PSCommandPath
}
else {
    (Get-Location).Path
}

$script:RepoRoot = Split-Path -Parent $script:MetadataScriptRoot
$script:ReleaseVersionFile = Join-Path $script:RepoRoot 'FrikaMF\JoniMF\ReleaseVersion.cs'
$script:ChangelogFile = Join-Path $script:RepoRoot 'CHANGELOG.md'

function Get-CurrentReleaseVersion {
    [CmdletBinding()]
    param()

    if (-not (Test-Path -LiteralPath $script:ReleaseVersionFile)) {
        throw "Release version file not found: $script:ReleaseVersionFile"
    }

    $content = Get-Content -LiteralPath $script:ReleaseVersionFile -Raw -ErrorAction Stop
    $match = [regex]::Match($content, 'Current\s*=\s*"(?<version>\d{2}\.\d{2}\.\d{4})"')
    if (-not $match.Success) {
        throw 'Could not read release version from ReleaseVersion.cs.'
    }

    return $match.Groups['version'].Value
}

function Assert-ReleaseVersionFormat {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    if ($Version -notmatch '^\d{2}\.\d{2}\.\d{4}$') {
        throw "Invalid version '$Version'. Expected format XX.XX.XXXX."
    }
}

function Get-BumpedReleaseVersion {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CurrentVersion,

        [Parameter(Mandatory = $true)]
        [ValidateSet('major', 'medium', 'minor')]
        [string]$Bump
    )

    Assert-ReleaseVersionFormat -Version $CurrentVersion
    $parts = $CurrentVersion.Split('.')
    $major = [int]$parts[0]
    $medium = [int]$parts[1]
    $minor = [int]$parts[2]

    switch ($Bump) {
        'major' {
            $major++
            $medium = 0
            $minor = 0
        }
        'medium' {
            $medium++
            $minor = 0
        }
        'minor' {
            $minor++
        }
    }

    if ($major -gt 99 -or $medium -gt 99 -or $minor -gt 9999) {
        throw 'Version overflow. Cannot exceed 99.99.9999.'
    }

    return ('{0:00}.{1:00}.{2:0000}' -f $major, $medium, $minor)
}

function Set-ReleaseVersion {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    Assert-ReleaseVersionFormat -Version $Version

    $content = Get-Content -LiteralPath $script:ReleaseVersionFile -Raw -ErrorAction Stop
    $updated = [regex]::Replace($content, 'Current\s*=\s*"\d{2}\.\d{2}\.\d{4}"', "Current = `"$Version`"")

    if ($content -eq $updated) {
        throw 'ReleaseVersion.cs update failed. Pattern not replaced.'
    }

    Set-Content -LiteralPath $script:ReleaseVersionFile -Value $updated -NoNewline -Encoding utf8
}

function Update-Changelog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$NewVersion,

        [Parameter(Mandatory = $true)]
        [string]$PreviousVersion,

        [Parameter(Mandatory = $true)]
        [string]$Summary
    )

    if (-not (Test-Path -LiteralPath $script:ChangelogFile)) {
        throw "CHANGELOG.md not found: $script:ChangelogFile"
    }

    $today = Get-Date -Format 'yyyy-MM-dd'
    $newTag = "v$NewVersion"
    $previousTag = "v$PreviousVersion"

    $entry = @"
## [$NewVersion] - $today

### Changed

- $Summary

"@

    $changelog = Get-Content -LiteralPath $script:ChangelogFile -Raw -ErrorAction Stop
    if ($changelog -match [regex]::Escape("## [$NewVersion]")) {
        Write-Host "[ReleaseMeta] CHANGELOG already contains version $NewVersion. Skipping entry creation."
        return
    }

    $requiredHeader = @"
# Changelog

<!-- markdownlint-disable MD024 -->

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses framework release versions in `XX.XX.XXXX` format.

## [Unreleased]

### Changed

- Initial unreleased section.

"@

    if ($changelog -notmatch '(?m)^## \[Unreleased\]\s*$') {
        $changelog = $requiredHeader + $changelog.TrimStart()
    }

    $releaseLinkLine = "[${NewVersion}]: https://github.com/mleem97/FrikaModFramework/compare/$previousTag...$newTag"
    $unreleasedLinkLine = "[Unreleased]: https://github.com/mleem97/FrikaModFramework/compare/$newTag...HEAD"

    $insertPattern = '(?s)(^## \[Unreleased\]\s*\r?\n(?:.*?\r?\n){0,30}?\r?\n)'
    if (-not [regex]::IsMatch($changelog, $insertPattern)) {
        throw 'Unable to find [Unreleased] section in CHANGELOG.md.'
    }

    $updated = [regex]::Replace($changelog, $insertPattern, ('$1' + "`r`n" + $entry), 1)

    if ($updated -match '(?m)^\[Unreleased\]:') {
        $updated = [regex]::Replace($updated, '(?m)^\[Unreleased\]:.*$', $unreleasedLinkLine, 1)
    }
    else {
        $updated = $updated.TrimEnd() + "`r`n" + $unreleasedLinkLine + "`r`n"
    }

    if ($updated -notmatch [regex]::Escape($releaseLinkLine)) {
        $updated = $updated.TrimEnd() + "`r`n" + $releaseLinkLine + "`r`n"
    }

    Set-Content -LiteralPath $script:ChangelogFile -Value $updated -NoNewline -Encoding utf8
}

function Update-ReleaseMetadata {
    [CmdletBinding()]
    param(
        [Parameter()]
        [ValidateSet('major', 'medium', 'minor')]
        [string]$Bump = 'minor',

        [Parameter()]
        [string]$Version,

        [Parameter()]
        [string]$Summary = 'Automated release metadata update.',

        [Parameter()]
        [switch]$SkipChangelog
    )

    Push-Location $script:RepoRoot
    try {
        $currentVersion = Get-CurrentReleaseVersion
        $nextVersion = if ([string]::IsNullOrWhiteSpace($Version)) {
            Get-BumpedReleaseVersion -CurrentVersion $currentVersion -Bump $Bump
        }
        else {
            Assert-ReleaseVersionFormat -Version $Version
            $Version
        }

        if ($nextVersion -eq $currentVersion) {
            throw "New version equals current version ($currentVersion)."
        }

        Set-ReleaseVersion -Version $nextVersion

        if (-not $SkipChangelog) {
            Update-Changelog -NewVersion $nextVersion -PreviousVersion $currentVersion -Summary $Summary
        }

        Write-Host "[ReleaseMeta] Updated release version: $currentVersion -> $nextVersion"
        if ($SkipChangelog) {
            Write-Host '[ReleaseMeta] CHANGELOG update skipped.'
        }
    }
    finally {
        Pop-Location
    }
}

Update-ReleaseMetadata -Bump $Bump -Version $Version -Summary $Summary -SkipChangelog:$SkipChangelog
