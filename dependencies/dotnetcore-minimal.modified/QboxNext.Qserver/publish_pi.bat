@echo off
dotnet publish -c Release -r linux-arm

rem Next line requires WSL to be installed with Ubuntu: (replace 192.168.2.82 with the wifi IP-address of your RPi)
bash -c "sshpass -p raspberry rsync -avzuh -e ssh bin/Release/netcoreapp2.1/linux-arm/* pi@192.168.2.82:/home/pi/qboxnext"
