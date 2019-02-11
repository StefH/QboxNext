#!/bin/bash
set -eo pipefail 
set -u

if [ $# -ne 1 ]; then
	echo "Usage: dumpqbx.sh <Qbox serial number>"
	exit 1
fi
qboxSerial=$1

scriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
projectPath=${scriptDir}/../src/QboxNext.DumpQbx/QboxNext.DumpQbx.csproj

function DumpQbx()
{
	local qboxSerial=$1
	local counter=$2
	
	qbxPath="/d/QboxNextData/Qbox_${qboxSerial}/${qboxSerial}_${counter}.qbx"
	if [ -f ${qbxPath} ]; then
		echo ${qbxPath}
		dotnet run --project ${projectPath} "--qbx=${qbxPath}" --values > "${qbxPath}.txt"
	fi
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
