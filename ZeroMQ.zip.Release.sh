#!/bin/bash
zip -r ZeroMQ.Release.zip * -x bin/Debug\* obj\* .git\* i386/v\* amd64/v\* *.nupkg ZeroMQ.*.zip 
