#!/usr/bin/env bash

set -e

PROFILE=$1
ACCOUNT=$2

REPO=${ACCOUNT}.dkr.ecr.us-west-2.amazonaws.com
REPO_NAMES="dotnet python"

$(aws ecr get-login --no-include-email --region us-west-2 --profile ${PROFILE})

for repoName in ${REPO_NAMES}; do
    newRepoName=jukebox-${repoName}

    echo "pushing image"
    docker push ${REPO}/${newRepoName}:latest
done
