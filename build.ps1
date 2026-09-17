param(
    [string]$ValheimDir = $env:VALHEIM_DIR,
    [string]$BepInExDir = $env:BEPINEX_DIR,
    [ValidateSet("Debug","Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

function Find-ValheimDir {
    $candidates = @(
        "$env:ProgramFiles(x86)\Steam\steamapps\common\Valheim",
        "C:\Steam\steamapps\common\Valheim",
        "D:\Steam\steamapps\common\Valheim",
        "E:\Steam\steamapps\common\Valheim",
        "C:\Program Files (x86)\Steam\steamapps\common\Valheim",
        "D:\Program Files (x86)\Steam\steamapps\common\Valheim",
        "E:\Program Files (x86)\Steam\steamapps\common\Valheim"
    )

    foreach ($p in $candidates) {
        if ($p -and (Test-Path (Join-Path $p "valheim_Data\Managed\assembly_valheim.dll"))) {
            return $p
        }
    }

    return $null
}

function Find-BepInExDir {
    $roots = @(
        "$env:APPDATA\r2modmanPlus-local\Valheim\profiles",
        "$env:APPDATA\Thunderstore Mod Manager\DataFolder\Valheim\profiles"
    )

    foreach ($root in $roots) {
        if (-not (Test-Path $root)) { continue }

        $hit = Get-ChildItem -Path $root -Filter "BepInEx.dll" -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.DirectoryName -match "\\BepInEx\\core$" } |
            Select-Object -First 1

        if ($hit) { return $hit.DirectoryName }
    }

    return $null
}

if (-not $ValheimDir) { $ValheimDir = Find-ValheimDir }
if (-not $BepInExDir) { $BepInExDir = Find-BepInExDir }

if (-not $ValheimDir) {
    throw "Valheim not found. Set `$env:VALHEIM_DIR to the folder containing valheim.exe."
}

$requiredManaged = @(
    "assembly_valheim.dll",
    "assembly_utils.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.IMGUIModule.dll",
    "UnityEngine.TextRenderingModule.dll",
    "UnityEngine.InputLegacyModule.dll"
)

foreach ($dll in $requiredManaged) {
    $candidate = Join-Path $ValheimDir ("valheim_Data\Managed\" + $dll)
    if (-not (Test-Path $candidate)) {
        throw "Required Valheim/Unity assembly not found: $candidate"
    }
}

if (-not $BepInExDir -or -not (Test-Path (Join-Path $BepInExDir "BepInEx.dll"))) {
    throw "BepInEx.dll not found. Set `$env:BEPINEX_DIR to your profile's BepInEx\core folder."
}

if (-not (Test-Path (Join-Path $BepInExDir "0Harmony.dll"))) {
    throw "0Harmony.dll not found in BepInEx\core. Reinstall/update BepInExPack Valheim in this profile."
}

Write-Host "Valheim: $ValheimDir"
Write-Host "BepInEx: $BepInExDir"
Write-Host "Building $Configuration..."

dotnet build .\Inject0rHUD.csproj `
    -c $Configuration `
    /p:ValheimDir="$ValheimDir" `
    /p:BepInExDir="$BepInExDir"

if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

Write-Host ""
Write-Host "Built: .\bin\$Configuration\Inject0rHUD.dll"
