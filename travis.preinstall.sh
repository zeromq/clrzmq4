#!/usr/bin/env bash
set -e

# the used libzmq can be configured using different values for INSTALL_LIBZMQ_MACOS
# if value = port install via macports, else install via homebrew
# homebrew has zeromq only as x64 as of 2017-06-29, so it cannot be used (see also https://github.com/travis-ci/travis-ci/issues/5640)-

if [ "$(uname)" = "Darwin" ] ; then
  
  if [ "INSTALL_LIBZMQ_MACOS" == "port" ] ; then
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

  else # if [ "INSTALL_LIBZMQ_MACOS" == "brew" ] ; then

    brew update
    brew install zeromq # --universal
  fi

else # if [ "$(uname)" = "Linux" ]

#  curl http://download.opensuse.org/repositories/network:/messaging:/zeromq:/release-stable/Debian_9.0/Release.key >Release.key
#  sudo apt-key add Release.key
#  sudo add-apt-repository "deb http://download.opensuse.org/repositories/network:/messaging:/zeromq:/release-stable/Debian_9.0 ./"
  sudo apt-get update
  sudo apt-get install libzmq3-dev
fi

nuget restore clrzmq4.mono.sln

