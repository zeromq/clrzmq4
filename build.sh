#!/usr/bin/env bash
set -e

# $TRAVIS_JOB_ID

# if [ "$(uname)" = "Darwin" ]; then # MacOSX
# else # if [ "$(uname)" = "Linux" ]; then
# fi


msbuild clrzmq4.mono.sln  /p:Configuration=Release  "$2" "$3" "$4" "$5"



#export MONO_TRACE_LISTENER=Console.Out
#mono ./testrunner/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe ./ZeroMQTest/bin/Release/ZeroMQTest.dll
# MONO_OPTIONS="--profile=monocov:outfile=ZeroMQ.cov,+[ZeroMQ]"
# monocov --export-xml=ZeroMQ.cov.xml ZeroMQ.cov

