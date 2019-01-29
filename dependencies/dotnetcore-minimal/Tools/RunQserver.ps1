# Make sure we don't fail silently.
$ErrorActionPreference = 'Stop'
# Fail on uninitialized variables and non-existing properties.
Set-StrictMode -Version Latest

$projectPath = Join-Path $PSScriptRoot ..\QboxNext.Qserver\QboxNext.Qserver.csproj
dotnet build $projectPath

$exePath = Join-Path $PSScriptRoot ..\QboxNext.Qserver\bin\Debug\netcoreapp2.1\QboxNext.Qserver.dll
dotnet $exePath
