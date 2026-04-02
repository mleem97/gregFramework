Set-StrictMode -Version Latest

$script:RepoRoot =
if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    Split-Path -Parent $PSScriptRoot
}
else {
    (Get-Location).Path
}

function Get-GitHubHeaders {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Token
    )

    return @{
        Authorization          = "Bearer $Token"
        Accept                 = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
        'User-Agent'           = 'DataCenter-AEMod-local-release-uploader'
    }
}

function Get-OrCreateRelease {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Owner,

        [Parameter(Mandatory = $true)]
        [string]$Repo,

        [Parameter(Mandatory = $true)]
        [string]$Tag,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Body,

        [Parameter(Mandatory = $true)]
        [hashtable]$Headers
    )

    $releaseUri = "https://api.github.com/repos/$Owner/$Repo/releases/tags/$Tag"

    try {
        return Invoke-RestMethod -Method Get -Uri $releaseUri -Headers $Headers -ErrorAction Stop
    }
    catch {
        $statusCode = $null
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        if ($statusCode -ne 404) {
            throw
        }
    }

    $createUri = "https://api.github.com/repos/$Owner/$Repo/releases"
    $payload = @{
        tag_name   = $Tag
        name       = $Name
        body       = $Body
        draft      = $false
        prerelease = $false
    } | ConvertTo-Json -Depth 5

    return Invoke-RestMethod -Method Post -Uri $createUri -Headers $Headers -Body $payload -ContentType 'application/json'
}

function Remove-ExistingAsset {
    param(
        [Parameter(Mandatory = $true)]
        $Release,

        [Parameter(Mandatory = $true)]
        [string]$AssetName,

        [Parameter(Mandatory = $true)]
        [hashtable]$Headers
    )

    $existing = @($Release.assets | Where-Object { $_.name -eq $AssetName })
    foreach ($asset in $existing) {
        $deleteUri = "https://api.github.com/repos/$($Release.author.login)/$($Release.url.Split('/')[5])/releases/assets/$($asset.id)"
        Invoke-RestMethod -Method Delete -Uri $deleteUri -Headers $Headers -ErrorAction Stop | Out-Null
    }
}

function Upload-ReleaseAsset {
    param(
        [Parameter(Mandatory = $true)]
        $Release,

        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [hashtable]$Headers
    )

    $fileName = [System.IO.Path]::GetFileName($FilePath)

    $uploadBase = [string]$Release.upload_url
    $uploadBase = $uploadBase -replace '\{\?name,label\}', ''
    $uploadUri = "$uploadBase?name=$([System.Uri]::EscapeDataString($fileName))"

    $existing = @($Release.assets | Where-Object { $_.name -eq $fileName })
    foreach ($asset in $existing) {
        $deleteUri = "https://api.github.com/repos/$($Release.url.Split('/')[4])/$($Release.url.Split('/')[5])/releases/assets/$($asset.id)"
        Invoke-RestMethod -Method Delete -Uri $deleteUri -Headers $Headers -ErrorAction Stop | Out-Null
    }

    $uploadHeaders = @{}
    foreach ($key in $Headers.Keys) {
        $uploadHeaders[$key] = $Headers[$key]
    }
    $uploadHeaders['Content-Type'] = 'application/octet-stream'

    Invoke-RestMethod -Method Post -Uri $uploadUri -Headers $uploadHeaders -InFile $FilePath -ErrorAction Stop | Out-Null
}

function Publish-LocalRelease {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Tag,

        [Parameter()]
        [string]$Owner = 'mleem97',

        [Parameter()]
        [string]$Repo = 'DataCenter-AEMod',

        [Parameter()]
        [string]$Configuration = 'Release',

        [Parameter()]
        [switch]$SkipBuild,

        [Parameter()]
        [string]$Token = $env:GITHUB_TOKEN,

        [Parameter()]
        [string]$ReleaseName,

        [Parameter()]
        [string]$ReleaseBody = 'Local build upload (contains game-dependent DLL outputs).'
    )

    if ([string]::IsNullOrWhiteSpace($Token)) {
        throw 'Missing GitHub token. Set GITHUB_TOKEN or pass -Token.'
    }

    if ([string]::IsNullOrWhiteSpace($ReleaseName)) {
        $ReleaseName = $Tag
    }

    Push-Location $script:RepoRoot
    try {
        if (-not $SkipBuild) {
            dotnet build .\FrikaMF.csproj -c $Configuration -p:TreatWarningsAsErrors=true -nologo
            if ($LASTEXITCODE -ne 0) {
                throw "FrikaMF build failed with exit code $LASTEXITCODE"
            }

            dotnet build .\HexLabelMod\HexLabelMod.csproj -c $Configuration -nologo
            if ($LASTEXITCODE -ne 0) {
                throw "HexLabelMod build failed with exit code $LASTEXITCODE"
            }
        }

        $frameworkDll = Join-Path $script:RepoRoot ("bin\$Configuration\net6.0\DataCenterModLoader.dll")
        $hexDll = Join-Path $script:RepoRoot ("HexLabelMod\bin\$Configuration\net6.0\HexLabelMod.dll")

        if (-not (Test-Path -LiteralPath $frameworkDll)) {
            throw "Missing framework DLL: $frameworkDll"
        }

        if (-not (Test-Path -LiteralPath $hexDll)) {
            throw "Missing HexLabel DLL: $hexDll"
        }

        $headers = Get-GitHubHeaders -Token $Token
        $release = Get-OrCreateRelease -Owner $Owner -Repo $Repo -Tag $Tag -Name $ReleaseName -Body $ReleaseBody -Headers $headers

        Upload-ReleaseAsset -Release $release -FilePath $frameworkDll -Headers $headers
        Upload-ReleaseAsset -Release $release -FilePath $hexDll -Headers $headers

        Write-Host "[Release] Uploaded assets to https://github.com/$Owner/$Repo/releases/tag/$Tag"
    }
    finally {
        Pop-Location
    }
}
