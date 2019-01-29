[CmdletBinding()]
Param(
	[Parameter(Mandatory=$true)]
	[string]$qboxserial
)	

# Make sure we don't fail silently.
$ErrorActionPreference = 'Stop'
# Fail on uninitialized variables and non-existing properties.
Set-StrictMode -Version Latest

$projectPath = Join-Path $PSScriptRoot ..\QboxNext.DumpQbx\QboxNext.DumpQbx.csproj
dotnet build $projectPath

function DumpQbx([string]$qboxSerial, [string]$counter)
{
	$exePath = Join-Path $PSScriptRoot ..\QboxNext.DumpQbx\bin\Debug\netcoreapp2.1\QboxNext.DumpQbx.dll
	$qbxPath = "D:\QboxNextData\Qbox_$qboxSerial\$($qboxSerial)_$counter.qbx"
	if (Test-Path $qbxPath)
	{
		Write-Output $qbxPath
		dotnet $exePath "--qbx=$qbxPath" "--qboxserial=00-00-000-000" "--metertype=smart" "--pattern=181:flat(2);182:zero;281:zero;282:zero;2421:zero" --values > "$qbxPath.txt"
	}
}

DumpQbx $qboxSerial "00000181"
DumpQbx $qboxSerial "00000182"
DumpQbx $qboxSerial "00000281"
DumpQbx $qboxSerial "00000282"
DumpQbx $qboxSerial "00002421"
DumpQbx $qboxSerial "00000001_secondary"
DumpQbx $qboxSerial "00000181_Client0"
DumpQbx $qboxSerial "00000182_Client0"
DumpQbx $qboxSerial "00000281_Client0"
DumpQbx $qboxSerial "00000282_Client0"
DumpQbx $qboxSerial "00002421_Client0"
DumpQbx $qboxSerial "00000001_Client0_secondary"
