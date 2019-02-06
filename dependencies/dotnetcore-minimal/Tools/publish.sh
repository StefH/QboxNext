#!/bin/bash
set -eo pipefail 
set -u

if ! which sshpass > /dev/null 2>&1; then
	echo "Please run this script from an environment that supports sshpass"
	exit 1
fi
if ! which ssh > /dev/null 2>&1; then
	echo "Please run this script from an environment that supports ssh"
	exit 1
fi
if ! which rsync > /dev/null 2>&1; then
	echo "Please run this script from an environment that supports rsync"
	exit 1
fi

scriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd ${scriptDir}/../QboxNext.Qserver
/mnt/c/Program\ Files/dotnet/dotnet.exe publish -c Release -r linux-arm
cd ${scriptDir}/../QboxNext.Qservice
/mnt/c/Program\ Files/dotnet/dotnet.exe publish -c Release -r linux-arm
cd ${scriptDir}/..

# replace 192.168.2.82 with the wifi IP-address of your RPi
piIp=192.168.2.82
piUser=pi
piPassword=raspberry

echo Killing Qserver on Raspberry Pi...
sshpass -p ${piPassword} ssh ${piUser}@${piIp} "pkill -f /home/pi/qserver/QboxNext.Qserver" || true
echo Killing Qservice on Raspberry Pi...
sshpass -p ${piPassword} ssh ${piUser}@${piIp} "pkill -f /home/pi/qservice/QboxNext.Qservice" || true
echo Copying Qserver to Raspberry Pi...
sshpass -p ${piPassword} rsync -avzuh -e ssh QboxNext.Qserver/bin/Release/netcoreapp2.1/linux-arm/publish/* ${piUser}@${piIp}:/home/pi/qserver
echo Copying Qservice to Raspberry Pi...
sshpass -p ${piPassword} rsync -avzuh -e ssh QboxNext.Qservice/bin/Release/netcoreapp2.1/linux-arm/publish/* ${piUser}@${piIp}:/home/pi/qservice
echo Starting QboxNext services on Raspberry Pi...
sshpass -p ${piPassword} ssh ${piUser}@${piIp} "nohup ~/start_qserver.sh > /dev/null 2>&1 &"
