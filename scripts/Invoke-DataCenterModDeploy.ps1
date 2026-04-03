Set-StrictMode -Version Latest

$script:DeployScriptRoot = if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    $PSScriptRoot
}
elseif (-not [string]::IsNullOrWhiteSpace($PSCommandPath)) {
    Split-Path -Parent $PSCommandPath
}
else {
    (Get-Location).Path
}

function Get-ProjectAssemblyName {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CsprojPath
    )

    try {
        [xml]$projectXml = Get-Content -LiteralPath $CsprojPath -Raw -ErrorAction Stop
        $assemblyNameNode = $projectXml.SelectSingleNode('//Project/PropertyGroup/AssemblyName')
        if ($null -ne $assemblyNameNode -and -not [string]::IsNullOrWhiteSpace($assemblyNameNode.InnerText)) {
            return [string]$assemblyNameNode.InnerText
        }
    }
    catch {
    }

    return [System.IO.Path]::GetFileNameWithoutExtension($CsprojPath)
}

function Get-ExactGameProcesses {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ExecutablePath
    )

    $resolvedExecutablePath = [System.IO.Path]::GetFullPath($ExecutablePath)
    $processes = @(Get-CimInstance Win32_Process -Filter "Name = 'Data Center.exe'" -ErrorAction SilentlyContinue)

    foreach ($proc in $processes) {
        if ([string]::IsNullOrWhiteSpace($proc.ExecutablePath)) {
            continue
        }

        $resolvedProcPath = [System.IO.Path]::GetFullPath([string]$proc.ExecutablePath)
        if ([string]::Equals($resolvedProcPath, $resolvedExecutablePath, [System.StringComparison]::OrdinalIgnoreCase)) {
            [PSCustomObject]@{
                Id = [int]$proc.ProcessId
                Name = [string]$proc.Name
                Path = [string]$proc.ExecutablePath
            }
        }
    }
}

function Invoke-DataCenterModDeploy {
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
    param(
        [Parameter()]
        [string]$ProjectPath = (Join-Path $script:DeployScriptRoot "..\FrikaMF.csproj"),

        [Parameter()]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Release',

        [Parameter()]
        [string]$TargetModsDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Data Center\Mods',

        [Parameter()]
        [string]$SteamUri = 'steam://rungameid/4170200',

        [Parameter()]
        [string]$GameExecutablePath = 'C:\Program Files (x86)\Steam\steamapps\common\Data Center\Data Center.exe',

        [Parameter()]
        [string[]]$GameProcessNames = @('DataCenter', 'datacenter'),

        [Parameter()]
        [switch]$SkipGameStart
    )

    $exactGameProcesses = @(Get-ExactGameProcesses -ExecutablePath $GameExecutablePath)
    if ($exactGameProcesses.Count -gt 0) {
        $exactSummary = ($exactGameProcesses | ForEach-Object { "{0} (PID {1})" -f $_.Path, $_.Id }) -join ', '
        Write-Host "[Deploy] Exact game executable process(es) detected: $exactSummary"

        if ($PSCmdlet.ShouldProcess($exactSummary, 'Stop exact Data Center.exe process(es) before deploy')) {
            foreach ($proc in $exactGameProcesses) {
                Stop-Process -Id $proc.Id -Force -ErrorAction Stop
            }

            $timeoutSeconds = 10
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            do {
                Start-Sleep -Milliseconds 200
                $stillExactRunning = @(Get-ExactGameProcesses -ExecutablePath $GameExecutablePath)
            } while ($stillExactRunning.Count -gt 0 -and $sw.Elapsed.TotalSeconds -lt $timeoutSeconds)

            if ($stillExactRunning.Count -gt 0) {
                throw "Exact game executable process is still running after stop attempt. Aborting deploy."
            }
        }
    }

    $resolvedProjectPath = [System.IO.Path]::GetFullPath($ProjectPath)
    if (-not (Test-Path -LiteralPath $resolvedProjectPath)) {
        throw "Project file not found: $resolvedProjectPath"
    }

    $projectDirectory = Split-Path -Parent $resolvedProjectPath
    $assemblyName = Get-ProjectAssemblyName -CsprojPath $resolvedProjectPath
    $outputDllPath = Join-Path $projectDirectory ("bin\$Configuration\net6.0\$assemblyName.dll")
    $targetDllPath = Join-Path $TargetModsDirectory ("$assemblyName.dll")

    Write-Host "[Deploy] Project: $resolvedProjectPath"
    Write-Host "[Deploy] Output DLL: $outputDllPath"
    Write-Host "[Deploy] Target DLL: $targetDllPath"

    $runningGameProcesses = @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
            $GameProcessNames -contains $_.ProcessName
        })

    if ($runningGameProcesses.Count -gt 0) {
        $processSummary = ($runningGameProcesses | ForEach-Object { "{0} (PID {1})" -f $_.ProcessName, $_.Id }) -join ', '
        Write-Host "[Deploy] Running game process(es) detected: $processSummary"

        if ($PSCmdlet.ShouldProcess($processSummary, 'Stop running game process(es) before deploy')) {
            foreach ($proc in $runningGameProcesses) {
                Stop-Process -Id $proc.Id -Force -ErrorAction Stop
            }

            $timeoutSeconds = 10
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            do {
                Start-Sleep -Milliseconds 200
                $stillRunning = @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
                        $GameProcessNames -contains $_.ProcessName
                    })
            } while ($stillRunning.Count -gt 0 -and $sw.Elapsed.TotalSeconds -lt $timeoutSeconds)

            if ($stillRunning.Count -gt 0) {
                throw "Game process is still running after stop attempt. Aborting deploy."
            }
        }
    }

    if ($PSCmdlet.ShouldProcess($resolvedProjectPath, "Build project ($Configuration)")) {
        & dotnet build $resolvedProjectPath -c $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE"
        }
    }

    if (-not (Test-Path -LiteralPath $outputDllPath)) {
        if ($WhatIfPreference) {
            Write-Host "[Deploy] WhatIf: output DLL check skipped: $outputDllPath"
            return
        }

        throw "Built DLL not found: $outputDllPath"
    }

    if (-not (Test-Path -LiteralPath $TargetModsDirectory)) {
        if ($PSCmdlet.ShouldProcess($TargetModsDirectory, 'Create target Mods directory')) {
            New-Item -ItemType Directory -Path $TargetModsDirectory -Force | Out-Null
        }
    }

    if ($PSCmdlet.ShouldProcess($targetDllPath, 'Copy and overwrite deployed DLL')) {
        Copy-Item -LiteralPath $outputDllPath -Destination $targetDllPath -Force
        Write-Host "[Deploy] Copied $assemblyName.dll to Mods directory."
    }

    if (-not $SkipGameStart) {
        if ($PSCmdlet.ShouldProcess($SteamUri, 'Start game via Steam URI')) {
            Start-Process $SteamUri
            Write-Host '[Deploy] Game start triggered via Steam.'
        }
    }
}

function Invoke-Deploy {
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
    param(
        [Parameter(Position = 0)]
        [ValidateSet('--1', '--all', '1', 'all', 'Frika')]
        [string]$Target = '--1',

        [Parameter()]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Release',

        [Parameter()]
        [string]$TargetModsDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Data Center\Mods',

        [Parameter()]
        [string]$SteamUri = 'steam://rungameid/4170200',

        [Parameter()]
        [string]$GameExecutablePath = 'C:\Program Files (x86)\Steam\steamapps\common\Data Center\Data Center.exe',

        [Parameter()]
        [string[]]$GameProcessNames = @('DataCenter', 'datacenter'),

        [Parameter()]
        [switch]$SkipGameStart
    )

    $isAll = $Target -in @('--all', 'all')
    $useFrika = $Target -in @('--1', '1', 'Frika')

    $projectPaths = @((Join-Path $script:DeployScriptRoot '..\FrikaMF.csproj'))

    $projectPaths = @($projectPaths)

    Write-Host "[Deploy] Target selected: $(if ($isAll) { 'All (--all => FrikaMF)' } else { 'Frika (--1)' })"

    $failedTargets = @()

    for ($index = 0; $index -lt $projectPaths.Count; $index++) {
        $currentProjectPath = $projectPaths[$index]
        $isLast = $index -eq ($projectPaths.Count - 1)
        $effectiveSkipGameStart = if ($isLast) { $SkipGameStart } else { $true }
        $targetLabel = [System.IO.Path]::GetFileName($currentProjectPath)

        $deployParams = @{
            ProjectPath = $currentProjectPath
            Configuration = $Configuration
            TargetModsDirectory = $TargetModsDirectory
            SteamUri = $SteamUri
            GameExecutablePath = $GameExecutablePath
            GameProcessNames = $GameProcessNames
            SkipGameStart = $effectiveSkipGameStart
            WhatIf = $WhatIfPreference
        }

        try {
            Invoke-DataCenterModDeploy @deployParams
        }
        catch {
            if ($projectPaths.Count -gt 1) {
                $failedTargets += $targetLabel
                Write-Warning "[Deploy] Target failed and will be skipped: $targetLabel`n$($_.Exception.Message)"
                continue
            }

            throw
        }
    }

    if ($failedTargets.Count -gt 0) {
        $failedSummary = ($failedTargets | Sort-Object -Unique) -join ', '
        throw "One or more deploy targets failed: $failedSummary"
    }
}
