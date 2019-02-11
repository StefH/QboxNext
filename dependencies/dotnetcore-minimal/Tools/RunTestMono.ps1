# Make sure we don't fail silently.
$ErrorActionPreference = 'Stop'
# Fail on uninitialized variables and non-existing properties.
Set-StrictMode -Version Latest

$slnPath = Join-Path $PSScriptRoot ..\QboxNext.Qserver.sln
dotnet build $slnPath

Write-Output "Launching Qserver"
$projectRoot = Join-Path $PSScriptRoot ..\src\QboxNext.Qserver
Start-Process dotnet -ArgumentList "run --project $projectRoot --QboxType=Mono"

Write-Output "Launching Qservice"
$projectRoot = Join-Path $PSScriptRoot ..\src\QboxNext.Qservice
Start-Process dotnet -ArgumentList "run --project $projectRoot --QboxType=Mono"

Write-Output "Waiting for Qserver and Qservice to finish starting..."
Start-Sleep -Second 10

Write-Output "Launching SimulateQbox"
$projectRoot = Join-Path $PSScriptRoot ..\src\QboxNext.SimulateQbox
Start-Process dotnet -ArgumentList "run --project $projectRoot --qserver=http://localhost:5000 --qboxserial=00-00-000-001 --metertype=smart --pattern=181:flat(2);182:zero;281:zero;282:zero;2421:zero"

while ($true)
{
	Write-Output "Total today:"
	$now = get-date
	$today = get-date $now -format "yyyy-MM-dd"
	$tomorrow = get-date (get-date).AddDays(1) -format "yyyy-MM-dd"
	$response = (invoke-webrequest "http://localhost:5002/api/getseries?sn=00-00-000-001&from=$today&to=$tomorrow&resolution=day").content | convertfrom-json
	$response.data | Where-Object { $_.energyType -eq "NetLow" } | Select-Object data | convertto-json

	Write-Output "Live:"
	$response = (invoke-webrequest "http://localhost:5002/api/getlivedata?sn=00-00-000-001").content | convertfrom-json
	$response.data | Where-Object { $_.energyType -eq "NetLow" } | Select-Object power | convertto-json
	
	Start-Sleep -Second 10
}
