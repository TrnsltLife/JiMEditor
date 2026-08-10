# Cross-publish a self-contained Mac build from Windows PowerShell / Cursor.
# Requires: .NET 9 SDK, sibling ..\JiMEditor for assets.
param(
  [ValidateSet("osx-arm64", "osx-x64")]
  [string] $Rid = "osx-arm64"
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$Project = Join-Path $Root "src\JiMEditor.Avalonia\JiMEditor.Avalonia.csproj"
$Assets = Join-Path $Root "..\JiMEditor\Assets"

if (-not (Test-Path $Assets)) {
  throw "Missing sibling JiMEditor assets at ..\JiMEditor\Assets"
}

dotnet publish $Project `
  -c Release `
  -r $Rid `
  --self-contained true `
  -p:UseAppHost=true

$Out = Join-Path $Root "src\JiMEditor.Avalonia\bin\Release\net9.0\$Rid\publish"
Write-Host "Published: $Out"
