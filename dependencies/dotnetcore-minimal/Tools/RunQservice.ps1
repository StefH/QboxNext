# Make sure we don't fail silently.
$ErrorActionPreference = 'Stop'
# Fail on uninitialized variables and non-existing properties.
Set-StrictMode -Version Latest

$projectPath = Join-Path $PSScriptRoot ..\QboxNext.Qservice\QboxNext.Qservice.csproj
dotnet build $projectPath

$exePath = Join-Path $PSScriptRoot ..\QboxNext.Qservice\bin\Debug\netcoreapp2.1\QboxNext.Qservice.dll
dotnet $exePath
