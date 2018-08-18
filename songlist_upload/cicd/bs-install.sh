#!/usr/bin/env bash

set -e

GITDIFF_FILES=$(git diff $(git log --pretty=format:%H -n 2) --name-only)

if [[ ${GITDIFF_FILES} != *"/songlist_upload/"* ]]; then
    echo "*** INFO - No changes found for songlist_upload. Nothing to do."
    exit 0
fi

echo "*** INFO - Changes found for songlist_upload. Running updates"
pip install -r requirements.txt -t .

