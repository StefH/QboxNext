#!/bin/sh

find . -type f -name '*Tests.csproj' -exec dotnet test --filter TestCategory!='Integration' "{}" \;
