#!/bin/bash
zip -r ZeroMQ.Release.zip * -x bin/Debug\* obj\* .git\* packages\* i386/v\* amd64/v\* *.nupkg ZeroMQ.*.zip ZeroMQ.*.nupkg

