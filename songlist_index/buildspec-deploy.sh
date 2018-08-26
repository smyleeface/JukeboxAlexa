#!/usr/bin/env bash

# NOTE (pattyr, 20180814): This is intended to run in the Dockerfile-python container

GITDIFF_FILES=$(git diff $(git log --pretty=format:%H -n 2) --name-only)

if [[ ${GITDIFF_FILES} = *"/songlist_index/"* ]]; then

    cd /project/songlist_index

    echo "*** INFO: run tests"
#    python setup.py test

    echo "*** INFO: Package Songlist Index"
    zip -r songlist_index.zip ./index.py

    echo "*** INFO: Upload to S3"
    aws s3 cp songlist_index.zip s3://smyleeface-public/JukeboxAlexa/songlist_index.zip --acl public-read

    echo "*** INFO: Update lambda function"

else
    echo "*** INFO: No changes made"
fi