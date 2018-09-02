#!/usr/bin/env bash

set -e

source 10_envvars.sh

cd ${CODEBUILD_SRC_DIR}/JukeboxAlexa

echo "***INFO: Restoring packages"
dotnet restore

echo "***INFO: Building packages"
dotnet build

echo "***INFO: Testing solution"
dotnet test
