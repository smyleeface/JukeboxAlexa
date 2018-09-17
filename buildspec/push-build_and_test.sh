#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source buildspec/env_vars.sh

    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa

    echo "***INFO: Restoring packages"
    dotnet restore

    echo "***INFO: Building packages"
    dotnet build

    echo "***INFO: Testing solution"
    dotnet test

    ls -la ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/JukeboxAlexa.SonglistIndex/
fi