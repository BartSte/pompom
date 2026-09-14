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

New-Item -ItemType Directory -Path $distributionPath -Force | Out-Null

if (Test-Path $publishPath) {
    Remove-Item -Path $publishPath -Recurse -Force
}

dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    --output $publishPath

if (Test-Path $archivePath) {
    Remove-Item $archivePath
}

Compress-Archive -Path "$publishPath\*" -DestinationPath $archivePath

Write-Host "Portable folder: $publishPath"
Write-Host "Portable archive: $archivePath"
