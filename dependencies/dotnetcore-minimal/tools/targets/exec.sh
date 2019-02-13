#!/bin/bash

# This is an intermediate script that requires as 1st param the dotnet DLL, and then followed by subsequent CLI arguments of the specific tool.

if [ $# -eq 0 ]; then
	echo "Error: at least the executable DLL argument is required."
	exit 1
fi

scriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
projectPath=${scriptDir}/$1

dotnet ${projectPath} "${@:2}"