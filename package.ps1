param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

function Fail([string]$Message) {
    throw "Release check failed: $Message"
}

$manifestPath = Join-Path $PSScriptRoot "manifest.json"
$readmePath = Join-Path $PSScriptRoot "README.md"
$changelogPath = Join-Path $PSScriptRoot "CHANGELOG.md"
$licensePath = Join-Path $PSScriptRoot "LICENSE"
$iconPath = Join-Path $PSScriptRoot "icon.png"
$pluginPath = Join-Path $PSScriptRoot "src\Plugin.cs"

foreach ($required in @(
    $manifestPath,
    $readmePath,
    $changelogPath,
    $licensePath,
    $iconPath,
    $pluginPath
)) {
    if (-not (Test-Path $required)) {
        Fail "missing file: $required"
    }
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($manifest.name)) {
    Fail "manifest.json has no package name"
}

if ($manifest.name -ne "Inject0rHUD") {
    Fail "manifest package name must be Inject0rHUD"
}

$version = [string]$manifest.version_number
if ($version -notmatch '^\d+\.\d+\.\d+$') {
    Fail "manifest version '$version' is not valid X.Y.Z SemVer"
}

if ([string]::IsNullOrWhiteSpace($manifest.description)) {
    Fail "manifest description is empty"
}

if ($null -eq $manifest.dependencies -or $manifest.dependencies.Count -eq 0) {
    Fail "manifest has no dependencies"
}

$pluginSource = Get-Content $pluginPath -Raw
$versionMatch = [regex]::Match(
    $pluginSource,
    'PluginVersion\s*=\s*"(?<version>\d+\.\d+\.\d+)"'
)

if (-not $versionMatch.Success) {
    Fail "could not find PluginVersion in src\Plugin.cs"
}

$pluginVersion = $versionMatch.Groups["version"].Value
if ($pluginVersion -ne $version) {
    Fail "Plugin.cs version ($pluginVersion) does not match manifest.json ($version)"
}

Add-Type -AssemblyName System.Drawing
$image = [System.Drawing.Image]::FromFile($iconPath)
try {
    if ($image.Width -ne 256 -or $image.Height -ne 256) {
        Fail "icon.png must be exactly 256x256; found $($image.Width)x$($image.Height)"
    }
}
finally {
    $image.Dispose()
}

$dll = Join-Path $PSScriptRoot "bin\$Configuration\Inject0rHUD.dll"
if (-not (Test-Path $dll)) {
    Write-Host "Release DLL not found; running build.ps1..."
    & (Join-Path $PSScriptRoot "build.ps1") -Configuration $Configuration
}

if (-not (Test-Path $dll)) {
    Fail "Inject0rHUD.dll was not produced by the build"
}

$dist = Join-Path $PSScriptRoot "dist"
$stage = Join-Path $dist "_package"
$plugins = Join-Path $stage "plugins"
$zip = Join-Path $dist "Inject0rHUD-$version.zip"

if (Test-Path $stage) {
    Remove-Item $stage -Recurse -Force
}

New-Item -ItemType Directory -Path $plugins -Force | Out-Null

Copy-Item $dll (Join-Path $plugins "Inject0rHUD.dll")
Copy-Item $manifestPath $stage
Copy-Item $readmePath $stage
Copy-Item $changelogPath $stage
Copy-Item $licensePath $stage
Copy-Item $iconPath $stage

$forbidden = Get-ChildItem $stage -Recurse -File | Where-Object {
    $_.Extension -in @(".pdb", ".cs", ".csproj", ".sln") -or
    $_.Name -match 'assembly_(valheim|utils)\.dll'
}

if ($forbidden) {
    $names = ($forbidden | ForEach-Object { $_.FullName }) -join ", "
    Fail "package contains development/game files: $names"
}

if (Test-Path $zip) {
    Remove-Item $zip -Force
}

# Thunderstore requires manifest.json, README.md and icon.png at the archive root.
Compress-Archive `
    -Path (Join-Path $stage "*") `
    -DestinationPath $zip `
    -CompressionLevel Optimal

if (-not (Test-Path $zip)) {
    Fail "package archive was not created"
}

$sizeMb = [math]::Round((Get-Item $zip).Length / 1MB, 2)

Write-Host ""
Write-Host "Release checks passed." -ForegroundColor Green
Write-Host "Version: $version"
Write-Host "Package: $zip"
Write-Host "Size: $sizeMb MB"
Write-Host ""
Write-Host "Before uploading, import this ZIP into a clean Thunderstore/r2modman profile once more."
