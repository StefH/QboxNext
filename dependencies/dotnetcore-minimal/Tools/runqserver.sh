#!/bin/bash
set -eo pipefail 
set -u

scriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
projectPath=${scriptDir}/../QboxNext.Qserver/QboxNext.Qserver.csproj

dotnet run --project ${projectPath} $*
