#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

nuget install NUnit.Console -Version 3.6.1 -OutputDirectory testrunner

xbuild /p:Configuration=Release clrzmq4.mono.sln
mono ./testrunner/NUnit.Console.3.6.1/tools/nunit-console.exe ./bin/Release/ZeroMQ.Test.dll

# TODO add MonoCov

revision=${TRAVIS_JOB_ID:=1}
revision=$(printf "%04d" $revision)

#nuget pack ./src/Invio.Extensions.DependencyInjection -c Release -o ./artifacts
