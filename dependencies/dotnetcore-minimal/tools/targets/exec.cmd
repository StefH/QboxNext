rem This is an intermediate script that requires as 1st param the dotnet DLL, and then followed by subsequent CLI arguments of the specific tool.

rem if [ $# -eq 0 ]; then
rem 	echo "Error: at least the executable DLL argument is required."
rem 	exit 1
rem fi

SET scriptDir=%~dp0
SET projectPath=%scriptDir%%~1

shift
set params=%1
:loop
shift
if [%1]==[] goto afterloop
set params=%params% %1
goto loop
:afterloop

dotnet "%projectPath%" %params%