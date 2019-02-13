# QboxNext

[![Build Status](https://dev.azure.com/QboxNext2019/QboxNext/_apis/build/status/QboxNext?branchName=master)](https://dev.azure.com/QboxNext2019/QboxNext/_build/latest?definitionId=1?branchName=master)

## Introduction

This repository is a modified clone of Qplatform.
The modifications are:

- only the code needed for Qserver, Qservice and the Qbox simulator was kept, the rest was removed,
- a stand-alone version of DumpQBX was added,
- a stand-alone version of ParseQboxMessage was added,
- all root namespaces were set to 'QboxNext',
- all sensitive information like connectionstrings and signing keys was removed,
- all csprojs have been rebuilt as .NET Core projects,
- all databases have been removed: metadata SQL database, metadata cache Redis database and status Mongo database),
- no Qbox metadata is being retrieved from a database, for now specifying Qbox Duo (default) or Qbox Mono can be done through configuration.
- all Qbox data is written to qboxnext\QboxNextData on Windows and /var/qboxnextdata on Linux (yes, it runs on Linux now!).

## How to develop

For simplified instructies on how to get Qserver and Qservice working a Raspberry Pi, see [this page](https://qboxnext.miraheze.org/wiki/Qserver_en_Qservice_op_Raspberry_Pi3_installeren).

To work on this code:
1. Download [Visual Studio Express](https://visualstudio.microsoft.com/vs/express/) or [Visual Studio Code](https://code.visualstudio.com/).
2. Install [git](https://git-scm.com/download/win).
3. Open a command shell and change to a directory for your git repositories, for example ```cd /d d:\git```
4. Enter the command to clone this repository: ```git clone https://bitbucket.org/qboxnext/qboxnext```
5. Open Visual Studio.
6. Open the sln, in this example d:\git\qboxnext\QboxNext.Qserver.sln
7. In the menu select Build->Rebuild Solution

Note: If you're not using Visual Studio, you can build the solution by running the following shell command (you need the [dotnet-sdk-2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2) package for this). 
```
dotnet build QboxNext.Qserver.sln
```
Make sure you're either in the folder, or pointing the command to the right folder by replacing QboxNext.Qserver with a filepath.

## Creating runtime binaries

### Linux/Git Bash/WLS

```
./publish.sh
```

### Windows

```
dotnet publish -c Release
```

> All built applications will be located in `./dist` folder

## Creating runtime binaries for the Raspberry Pi

> Note: `dotnet publish` is not supported on Rasberry PI. Build on a Windows/Linux system with .NET Core SDK installed.

### Linux/Git Bash/WLS

```
./publish.sh -r linux-arm
```

### Windows

```
dotnet publish -c Release -r linux-arm
```

> All built applications will be located in `./dist` folder


## Qserver

An ASP.NET application that receives and processes messages from Qboxes. When run it uses the built-in [Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-2.2) web server. 
It will run on port 5000, the default Kestrel port.

## Qservice

Another service, this one creates graph data from the data that was received by Qserver.

### Testing on Windows

When Qserver is running it can be tested using Powershell. Open Powershell and run this snippet:

```powershell
$body = @"
FAFB070DABB7440780/KFM5KAIFA-METER 1-3:0.2.8(40) 0-0:1.0.0(000102045905W) 
0-0:96.1.1(4530303033303030303030303032343133) 1-0:1.8.1(000001.011*kWh) 
1-0:1.8.2(000000.000*kWh) 1-0:2.8.1(000000.000*kWh) 
1-0:2.8.2(000000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(00.034*kW) 
1-0:2.7.0(00.000*kW) 0-0:17.0.0(999.9*kW) 0-0:96.3.10(1) 0-0:96.7.21(00073) 
0-0:96.7.9(00020) 1-0:99.97.0(3)(0-0:96.7.19)(000124235657W)(0000003149*s)(000124225935W)(0000000289*s)(000101000001W)(2147483647*s) 
1-0:32.32.0(00005) 1-0:52.32.0(00006) 1-0:72.32.0(00001) 1-0:32.36.0(00000) 
1-0:52.36.0(00000) 1-0:72.36.0(00000) 0-0:96.13.1() 0-0:96.13.0() 1-0:31.7.0(000*A) 
1-0:51.7.0(000*A) 1-0:71.7.0(000*A) 1-0:21.7.0(00.034*kW) 1-0:22.7.0(00.000*kW) 1-0:41.7.0(00.000*kW) 
1-0:42.7.0(00.000*kW) 1-0:61.7.0(00.000*kW) 1-0:62.7.0(00.000*kW) 0-1:24.1.0(003) 
0-1:96.1.0(4730303131303033303832373133363133) 0-1:24.2.1(000102043601W)(62869.839*m3) 0-1:24.4.0(1) !583C
"@
invoke-webrequest -method POST http://localhost:5000/device/qbox/6618-1400-0200/15-46-002-442 -body $body -ContentType text/html
```

It should show something like this:

```
StatusCode        : 200
StatusDescription : OK
Content           : FB1695698300
RawContent        : HTTP/1.1 200 OK
                    Content-Length: 14
                    Content-Type: text/plain; charset=utf-8
                    Date: Thu, 03 Jan 2019 06:23:45 GMT
                    Server: Kestrel

                    FB1695698300
Forms             : {}
Headers           : {[Content-Length, 14], [Content-Type, text/plain; charset=utf-8], [Date, Thu, 03 Jan 2019 06:23:45
                    GMT], [Server, Kestrel]}
Images            : {}
InputFields       : {}
Links             : {}
ParsedHtml        : mshtml.HTMLDocumentClass
RawContentLength  : 14
```

### Testing on Linux

Copy the script below into a text file, save it and give it a name like "testqserver.sh".

```bash
#!/bin/bash       
read -r -d '' BODY <<- EOM
	FAFB070DABB7440780/KFM5KAIFA-METER 1-3:0.2.8(40) 0-0:1.0.0(000102045905W) 
	0-0:96.1.1(4530303033303030303030303032343133) 1-0:1.8.1(000001.011*kWh) 
	1-0:1.8.2(000000.000*kWh) 1-0:2.8.1(000000.000*kWh) 
	1-0:2.8.2(000000.000*kWh) 0-0:96.14.0(0001) 1-0:1.7.0(00.034*kW) 
	1-0:2.7.0(00.000*kW) 0-0:17.0.0(999.9*kW) 0-0:96.3.10(1) 0-0:96.7.21(00073) 
	0-0:96.7.9(00020) 1-0:99.97.0(3)(0-0:96.7.19)(000124235657W)(0000003149*s)(000124225935W)(0000000289*s)(000101000001W)(2147483647*s) 
	1-0:32.32.0(00005) 1-0:52.32.0(00006) 1-0:72.32.0(00001) 1-0:32.36.0(00000) 
	1-0:52.36.0(00000) 1-0:72.36.0(00000) 0-0:96.13.1() 0-0:96.13.0() 1-0:31.7.0(000*A) 
	1-0:51.7.0(000*A) 1-0:71.7.0(000*A) 1-0:21.7.0(00.034*kW) 1-0:22.7.0(00.000*kW) 1-0:41.7.0(00.000*kW) 
	1-0:42.7.0(00.000*kW) 1-0:61.7.0(00.000*kW) 1-0:62.7.0(00.000*kW) 0-1:24.1.0(003) 
	0-1:96.1.0(4730303131303033303832373133363133) 0-1:24.2.1(000102043601W)(62869.839*m3) 0-1:24.4.0(1) !583C
EOM

echo 'Body:'
echo "$BODY"

curl http://localhost:5000/device/qbox/6618-1400-0200/15-46-002-442 -d "$BODY" -H "Content-Type: text/html" -v
```

Run the script by using the following commands in the terminal (obviously you'll need to be in the same folder as where you saved the file):

```bash
chmod u+x testqserver.sh && \
./testqserver.sh
```

Another option is to run the Qbox simulator. See SimulateQbox.

### Notes

- When starting Qserver from Visual Studio, the log files end up in QboxNext.Qserver\bin\Debug\netcoreapp2.1\logs

## DumpQbx

DumpQbx is a tool to convert a QBX file into a readable text file.

The generated text file will look like this:

```
StartOfFile: 2014-04-08 14:23:00
EndOfFile:   2014-04-15 14:23:00
ID:          d19a4946-229d-4525-b68a-3090bd3b3c0c
Timestamp NL     :        raw,        kWh,      money, quality (kWh can be kWh, Wh or mWh depending on Precision setting)
2014-04-08 14:23 :       1011,          0,          0,     0
2014-04-08 14:24 : empty slot
...
```

The 'raw' column contains the counter value as received by Qserver. In the current implementation the unit is Wh. 
The column 'kWh' is the raw value converted to kWh.
The 'money' column is obsolete and can be ignored.
The column 'quality' specifies if the value was a value received by Qserver or an interpolated value.

### How to run

Run the command below. You may have to replace the file paths depending on the Qbox you want to dump. Check the `/var/qboxnext` folder to see the folders with qbx files in them.

```bash
sudo sh -c \
'./dist/DumpQbx.sh \
--qbx=/var/qboxnextdata/Qbox_00-00-000-000/00-00-000-000_00000181.qbx \
--values \
> /var/qboxnextdata/Qbox_00-00-000-000/00-00-000-000_00000181.qbx.txt'
```

## ParseQboxMessage

ParseQboxMessage will parse the Qbox messages given on the command line and show its contents in a readable format. You can find those messages in the Qserver log files, look for lines containing 'input:'.

### How to run

```bash
./dist/ParseQboxMessage.sh --message=<message>
```

## SimulateQbox

SimulateQbox is a simulator that can simulate Qboxes attached to several types of meters and simulate several usage and generation patterns.

> To use it make sure Qserver is running.

To view an explanation of the different meter types and patterns, run SimulateQbox without parameters:

```bash
./dist/SimulateQbox.sh
```

### How to run

```bash
./dist/SimulateQbox.sh --qserver=http://localhost:5000 \
--qboxserial=00-00-000-000 --metertype=smart \
--pattern='181:flat(2);182:zero;281:zero;282:zero;2421:zero'
```

## General notes

- When you change nlog.config of a project, you have to do a rebuild of the project, otherwise the modified file may not be copied to the output directory.
