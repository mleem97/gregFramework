$ErrorActionPreference = 'SilentlyContinue'

function Normalize-PathValue {
    param([string]$PathValue)

    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return $null
    }

    return ($PathValue -replace '/', '\')
}

function Resolve-GameInstallFromLibrary {
    param([string]$LibraryPath)

    $normalized = Normalize-PathValue -PathValue $LibraryPath
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return $null
    }

    $candidate = Join-Path $normalized 'steamapps\common\Data Center'
    if (Test-Path -LiteralPath $candidate) {
        return $candidate
    }

    return $null
}

$steamRoot = $null
try {
    $steamRoot = (Get-ItemProperty -Path 'HKCU:\Software\Valve\Steam' -Name 'SteamPath').SteamPath
} catch {
}

$steamRoot = Normalize-PathValue -PathValue $steamRoot

$libraryCandidates = New-Object System.Collections.Generic.List[string]
if (-not [string]::IsNullOrWhiteSpace($steamRoot)) {
    [void]$libraryCandidates.Add($steamRoot)
}

$libraryFoldersVdf = if (-not [string]::IsNullOrWhiteSpace($steamRoot)) {
    Join-Path $steamRoot 'config\libraryfolders.vdf'
}

if (-not [string]::IsNullOrWhiteSpace($libraryFoldersVdf) -and (Test-Path -LiteralPath $libraryFoldersVdf)) {
    $lines = Get-Content -LiteralPath $libraryFoldersVdf

    foreach ($line in $lines) {
        if ($line -match '"path"\s+"([^"]+)"') {
            $parsedPath = Normalize-PathValue -PathValue $Matches[1].Replace('\\', '\')
            if (-not [string]::IsNullOrWhiteSpace($parsedPath)) {
                [void]$libraryCandidates.Add($parsedPath)
            }
            continue
        }

        if ($line -match '^\s*"\d+"\s+"([^"]+)"\s*$') {
            $parsedPath = Normalize-PathValue -PathValue $Matches[1].Replace('\\', '\')
            if (-not [string]::IsNullOrWhiteSpace($parsedPath)) {
                [void]$libraryCandidates.Add($parsedPath)
            }
        }
    }
}

$uniqueLibraries = $libraryCandidates | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique
$foundGamePaths = New-Object System.Collections.Generic.List[string]

foreach ($library in $uniqueLibraries) {
    $gamePath = Resolve-GameInstallFromLibrary -LibraryPath $library
    if (-not [string]::IsNullOrWhiteSpace($gamePath)) {
        [void]$foundGamePaths.Add($gamePath)
    }
}

foreach ($gamePath in ($foundGamePaths | Select-Object -Unique)) {
    $melondll = Join-Path $gamePath 'MelonLoader\net6\MelonLoader.dll'
    $asmCSharp = Join-Path $gamePath 'MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll'
    if ((Test-Path -LiteralPath $melondll) -and (Test-Path -LiteralPath $asmCSharp)) {
        Write-Output $gamePath
        exit 0
    }
}

if ($foundGamePaths.Count -gt 0) {
    Write-Output ($foundGamePaths[0])
    exit 0
}

exit 0
