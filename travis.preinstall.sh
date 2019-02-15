#!/usr/bin/env bash
set -e

# the used libzmq can be configured using different values for INSTALL_LIBZMQ_MACOS
# values: empty = do not install libzmq, but use repository binaries; brew = install via homebrew; port = install via macports
# homebrew has zeromq only as x64 as of 2017-06-29, so it cannot be used (see also https://github.com/travis-ci/travis-ci/issues/5640)

if [ "$(uname)" = "Darwin" ] ; then
  # DIAGNOSTICS
  # otool -L amd64/libzmq.dylib
  # otool -L i386/libzmq.dylib
  
  if [ "INSTALL_LIBZMQ_MACOS" == "brew" ] ; then
    brew install zeromq --universal

    # file /usr/local/lib/*mq*.dylib # DIAGNOSTICS

    # cp /usr/local/lib/libzmq.dylib amd64
    # cp /usr/local/lib/libzmq.dylib i386  
  elif [ "INSTALL_LIBZMQ_MACOS" == "port" ] ; then
    wget --retry-connrefused --waitretry=1 -O /tmp/macports.pkg\
      https://github.com/macports/macports-base/releases/download/v2.4.1/MacPorts-2.4.1-10.11-ElCapitan.pkg
    sudo installer -pkg /tmp/macports.pkg -target /
    export PATH=/opt/local/bin:/opt/local/sbin:$PATH
    sudo rm /opt/local/etc/macports/archive_sites.conf
    echo "name macports_archives" >archive_sites.conf
    echo "name local_archives" >>archive_sites.conf
    echo "urls http://packages.macports.org/ http://nue.de.packages.macports.org/" >>archive_sites.conf
    sudo cp archive_sites.conf /opt/local/etc/macports/
  
    # ignore errors on call to port, since this seems to always fail with "Updating database of binaries failed"  
    while (sudo port -v install zmq +universal || true) | grep "Error: Port zmq not found" ; do echo "port install zmq failed, retrying" ; done
  
    # file /opt/local/lib/*mq*.dylib # DIAGNOSTICS
  
    # cp /opt/local/lib/libzmq.dylib amd64
    # cp /opt/local/lib/libzmq.dylib i386  
  fi

else # if [ "$(uname)" = "Linux" ]
  # assume that we are on Ubuntu (which is used on Travis-CI.org)
  curl http://download.opensuse.org/repositories/network:/messaging:/zeromq:/release-stable/xUbuntu_14.04/Release.key >Release.key
  sudo apt-key add Release.key
  sudo add-apt-repository "deb http://download.opensuse.org/repositories/network:/messaging:/zeromq:/release-stable/xUbuntu_14.04 ./"
  sudo apt-get update
  sudo apt-get install libzmq5
fi

nuget restore clrzmq4.mono.sln
# nuget install NUnit.ConsoleRunner -Version 3.6.1 -OutputDirectory testrunner
# nuget install coveralls.net -Version 0.7.0 -OutputDirectory tools
