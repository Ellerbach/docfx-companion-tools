#!/bin/bash
pushd "$(dirname "${BASH_SOURCE[0]}")"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "./build.ps1 $@; exit $LastExitCode;"
popd