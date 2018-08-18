#!/usr/bin/env bash

set -e

if [[ $CODEBUILD_BUILD_SUCCEEDING == 1 ]]; then
    cd ${CODEBUILD_SRC_DIR}/songlist_upload

    echo "*** INFO - running tests for songlist_upload"
    python setup.py test || echo "command failed"; exit 1

else
    exit 1
fi