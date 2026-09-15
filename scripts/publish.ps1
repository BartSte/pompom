param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot "src\Pompom\Pompom.csproj"
$distributionPath = Join-Path $repositoryRoot "dist"
$publishPath = Join-Path $distributionPath "Pompom-win-x64"
$archivePath = Join-Path $distributionPath "Pompom-win-x64.zip"
$stagingPath = Join-Path ([System.IO.Path]::GetTempPath()) ("Pompom-publish-" + [guid]::NewGuid())
$stagingPublishPath = Join-Path $stagingPath "Pompom-win-x64"
$stagingArchivePath = Join-Path $stagingPath "Pompom-win-x64.zip"

function Invoke-WithRetry {
    param(
        [Parameter(Mandatory)]
        [scriptblock] $Operation,

        [int] $MaxAttempts = 30,

        [int] $DelaySeconds = 1
    )

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        try {
            & $Operation
            return
        }
        catch {
            if ($attempt -eq $MaxAttempts) {
                throw
            }

            Write-Warning "The WSL share is busy. Retrying in $DelaySeconds second(s)."
            Start-Sleep -Seconds $DelaySeconds
        }
    }
}

function Invoke-WslCommand {
    param(
        [Parameter(Mandatory)]
        [string] $Distribution,

        [Parameter(Mandatory)]
        [string[]] $Command
    )

    & wsl.exe --distribution $Distribution --exec @Command

    if ($LASTEXITCODE -ne 0) {
        throw "The WSL command failed with exit code $LASTEXITCODE."
    }
}

try {
    New-Item -ItemType Directory -Path $stagingPath -Force | Out-Null

    dotnet publish $projectPath `
        --configuration $Configuration `
        --runtime win-x64 `
        --self-contained true `
        --output $stagingPublishPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }

    Compress-Archive -Path "$stagingPublishPath\*" -DestinationPath $stagingArchivePath

    $wslShare = [regex]::Match(
        $repositoryRoot,
        '^\\\\wsl\.localhost\\([^\\]+)(\\.*)$',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
    )

    if ($wslShare.Success) {
        $wslDistribution = $wslShare.Groups[1].Value
        $wslRepositoryPath = $wslShare.Groups[2].Value -replace '\\', '/'
        $wslDistributionPath = "$wslRepositoryPath/dist"
        $wslPublishPath = "$wslDistributionPath/Pompom-win-x64"
        $wslArchivePath = "$wslDistributionPath/Pompom-win-x64.zip"
        $wslStagingPath = (Invoke-WslCommand $wslDistribution @("wslpath", "-u", $stagingPath)).Trim()
        $wslStagingPublishPath = "$wslStagingPath/Pompom-win-x64"
        $wslStagingArchivePath = "$wslStagingPath/Pompom-win-x64.zip"

        Invoke-WslCommand $wslDistribution @("/usr/bin/mkdir", "-p", $wslDistributionPath) | Out-Null
        Invoke-WslCommand $wslDistribution @("/usr/bin/rm", "-rf", "--", $wslPublishPath, $wslArchivePath) | Out-Null
        Invoke-WslCommand $wslDistribution @("/usr/bin/cp", "-a", "--", $wslStagingPublishPath, $wslDistributionPath) | Out-Null
        Invoke-WslCommand $wslDistribution @("/usr/bin/cp", "-f", "--", $wslStagingArchivePath, $wslArchivePath) | Out-Null
    }
    else {
        New-Item -ItemType Directory -Path $distributionPath -Force | Out-Null

        if (Test-Path $publishPath) {
            Invoke-WithRetry {
                Remove-Item -LiteralPath $publishPath -Recurse -Force
            }
        }

        if (Test-Path $archivePath) {
            Invoke-WithRetry {
                Remove-Item -LiteralPath $archivePath -Force
            }
        }

        Invoke-WithRetry {
            Copy-Item -LiteralPath $stagingPublishPath -Destination $distributionPath -Recurse -Force
        }
        Invoke-WithRetry {
            Copy-Item -LiteralPath $stagingArchivePath -Destination $archivePath -Force
        }
    }

    Write-Host "Portable folder: $publishPath"
    Write-Host "Portable archive: $archivePath"
}
finally {
    if (Test-Path $stagingPath) {
        Remove-Item -LiteralPath $stagingPath -Recurse -Force
    }
}
