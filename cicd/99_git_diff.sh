#!/usr/bin/env bash

GITDIFF_FILES=$(git diff $(git log --pretty=format:%H -n 2) --name-only)
if [[ ${GITDIFF_FILES} != *"/JukeboxAlexa/"* ]]; then echo "*** INFO: No changes found for JukeboxAlexa. Nothing to do." && exit 0; fi
echo "*** INFO: Changes found for JukeboxAlexa. Running updates"
cd JukeboxAlexa