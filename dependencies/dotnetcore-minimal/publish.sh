#!/bin/bash

echo
echo Publishing QboxNext to ./dist folder.
echo

dotnet publish QboxNext.Qserver.sln -c Release "$@"

echo Done.