#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} && "${GIT_BRANCH}" != "master" ]]; then

    source buildspec/env_vars.sh

    ls -la ${CODEBUILD_SRC_DIR}
    git status

    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa

    echo "***INFO: Restoring packages"
    dotnet restore

    echo "***INFO: Building packages"
    dotnet build

    echo "***INFO: Testing solution"
    dotnet test

fi