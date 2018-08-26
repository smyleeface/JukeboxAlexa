#!/usr/bin/env bash

set -e

if [[ $CODEBUILD_BUILD_SUCCEEDING == 1 ]]; then
    cd ${CODEBUILD_SRC_DIR}/songlist_upload

    GITSHA=$(git rev-parse HEAD)
    echo "*** INFO - Latest gitsha - ${GITSHA}"

    FILENAME=songlist_upload-${GITSHA}.zip
    echo "*** INFO - Filename - ${FILENAME}"

    echo "*** INFO - package code"
    tar -zcvf ${FILENAME} --exclude-from=".eggs"

    echo "*** INFO - upload package"
    aws s3 cp ${FILENAME} s3://smyleeface-public/JukeboxAlexa/songlist_upload/${FILENAME} --acl public-read
else
    exit 1
fi