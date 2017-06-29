#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

# TODO or use uname? what does that return on Mac OS?
if [ "$(sw_vers -productName)" == "Mac OS X" ] ; then
  # homebrew has zeromq only as x64 as of 2017-06-29, so we must use macports (see also https://github.com/travis-ci/travis-ci/issues/5640)
  #brew install zeromq --universal
  wget --retry-connrefused --waitretry=1 -O /tmp/macports.pkg https://github.com/macports/macports-base/releases/download/v2.4.1/MacPorts-2.4.1-10.11-ElCapitan.pkg
  sudo installer -pkg /tmp/macports.pkg -target /
  export PATH=/opt/local/bin:/opt/local/sbin:$PATH
  sudo rm /opt/local/etc/macports/archive_sites.conf
  echo "name macports_archives" >archive_sites.conf
  echo "name local_archives" >>archive_sites.conf
  echo "urls http://packages.macports.org/ http://nue.de.packages.macports.org/" >>archive_sites.conf
  sudo cp archive_sites.conf /opt/local/etc/macports/
  sudo port -v install zmq +universal || true # ignore errors, since this seems to always fail with "Updating database of binaries failed"
  file /usr/local/lib/*mq*.dylib
  file /opt/local/lib/*mq*.dylib
  find /usr/local -name '*zmq*' # DEBUG
  find /usr/local -name '*zeromq*' # DEBUG
  echo DYLD_LIBRARY_PATH=$DYDYLD_LIBRARY_PATH
  echo DYLD_FALLBACK_LIBRARY_PATH=$DYLD_FALLBACK_LIBRARY_PATH
fi

nuget install NUnit.ConsoleRunner -Version 3.6.1 -OutputDirectory testrunner
# nuget install coveralls.net -Version 0.7.0 -OutputDirectory tools

xbuild /p:Configuration=Release clrzmq4.mono.sln

export MONO_TRACE_LISTENER=Console.Out

# MONO_OPTIONS="--profile=monocov:outfile=ZeroMQ.cov,+[ZeroMQ]"
mono ./testrunner/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe ./ZeroMQTest/bin/Release/ZeroMQTest.dll

# monocov --export-xml=ZeroMQ.cov.xml ZeroMQ.cov
# REPO_COMMIT_AUTHOR=$(git show -s --pretty=format:"%cn")
# REPO_COMMIT_AUTHOR_EMAIL=$(git show -s --pretty=format:"%ce")
# REPO_COMMIT_MESSAGE=$(git show -s --pretty=format:"%s")
# mono .\\tools\\coveralls.net.0.7.0\\tools\\csmacnz.Coveralls.exe --monocov -i ./ZeroMQ.cov.xml --repoToken $COVERALLS_REPO_TOKEN --commitId $TRAVIS_COMMIT --commitBranch $TRAVIS_BRANCH --commitAuthor "$REPO_COMMIT_AUTHOR" --commitEmail "$REPO_COMMIT_AUTHOR_EMAIL" --commitMessage "$REPO_COMMIT_MESSAGE" --jobId $TRAVIS_JOB_ID  --serviceName travis-ci  --useRelativePaths

revision=${TRAVIS_JOB_ID:=1}
revision=$(printf "%04d" $revision)

#nuget pack ./src/Invio.Extensions.DependencyInjection -c Release -o ./artifacts
