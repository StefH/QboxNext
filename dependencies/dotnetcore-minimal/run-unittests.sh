#!/bin/sh

# https://confluence.atlassian.com/bitbucket/test-reporting-in-pipelines-939708543.html

find . -type f -name '*Tests.csproj' -exec dotnet test --logger 'trx;LogFileName=../test-results/results.trx' --filter TestCategory!='Integration' "{}" \;

# Compile reports

dotnet tool install -g trx2junit
export PATH="$PATH:/root/.dotnet/tools"

trx2junit **/test-results/*.trx
