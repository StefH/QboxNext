#!/bin/sh

dotnet restore
dotnet build --no-restore
