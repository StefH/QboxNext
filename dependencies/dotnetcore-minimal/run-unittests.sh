#!/bin/sh

# Clean old results
rm -rf **/test-results

# https://confluence.atlassian.com/bitbucket/test-reporting-in-pipelines-939708543.html

# Install JUnit converter
export PATH="$PATH:/root/.dotnet/tools"
dotnet tool install -g trx2junit

# Exec all tests and convert to JUnit. Both steps must be tracked for exit code.
if ! dotnet test --logger 'trx;LogFileName=../test-results/results.trx' --filter TestCategory!='Integration' ; then
    if ! trx2junit **/test-results/*.trx ; then
        echo "Failed to convert reports." && exit 1
    fi
    echo "One or more tests have failed." && exit 1
else
    if ! trx2junit **/test-results/*.trx ; then
        echo "Failed to convert reports." && exit 1
    fi
    echo "All tests have passed."
fi
