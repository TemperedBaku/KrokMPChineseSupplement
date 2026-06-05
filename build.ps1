param(
  [string]$GameDir = "D:\steam\steamapps\common\Casualties Unknown Demo"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $GameDir)) {
  Write-Error "GameDir not found: $GameDir"
  exit 1
}

$required = @(
  "$GameDir\BepInEx\core\BepInEx.dll",
  "$GameDir\BepInEx\core\0Harmony.dll",
  "$GameDir\CasualtiesUnknown_Data\Managed\UnityEngine.dll",
  "$GameDir\CasualtiesUnknown_Data\Managed\UnityEngine.CoreModule.dll",
  "$GameDir\CasualtiesUnknown_Data\Managed\UnityEngine.IMGUIModule.dll",
  "$GameDir\CasualtiesUnknown_Data\Managed\UnityEngine.UIModule.dll",
  "$GameDir\CasualtiesUnknown_Data\Managed\UnityEngine.UI.dll"
)

foreach ($file in $required) {
  if (-not (Test-Path $file)) {
    Write-Error "Required reference not found: $file"
    exit 1
  }
}

dotnet build .\src\KrokMPChineseSupplement\KrokMPChineseSupplement.csproj -c Release -p:GameDir="$GameDir"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = ".\src\KrokMPChineseSupplement\bin\Release\KrokMPChineseSupplement.dll"
if (-not (Test-Path $dll)) {
  Write-Error "Build finished but DLL not found: $dll"
  exit 1
}

if (Test-Path .\release\KrokMPChineseSupplement) { Remove-Item .\release\KrokMPChineseSupplement -Recurse -Force }
New-Item -Force -ItemType Directory .\release\KrokMPChineseSupplement | Out-Null
Copy-Item $dll .\release\KrokMPChineseSupplement\KrokMPChineseSupplement.dll -Force
Copy-Item .\translations\*.json .\release\KrokMPChineseSupplement\ -Force
if (Test-Path .\README.md) { Copy-Item .\README.md .\release\KrokMPChineseSupplement\README.md -Force }
Compress-Archive -Path .\release\KrokMPChineseSupplement -DestinationPath .\KrokMPChineseSupplement_release.zip -Force
Write-Host "Built .\KrokMPChineseSupplement_release.zip"
Write-Host "Install to: BepInEx\plugins\KrokMPChineseSupplement\"
