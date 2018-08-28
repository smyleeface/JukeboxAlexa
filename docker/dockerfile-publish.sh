#!/usr/bin/env bash

set -e

PROFILE=$1
ACCOUNT=$2

REPO=${ACCOUNT}.dkr.ecr.us-west-2.amazonaws.com
REPO_NAMES="dotnet python"

for repoName in ${REPO_NAMES}; do
    newRepoName=jukebox-${repoName}

    echo "building image for repo: ${newRepoName}"
    docker build -t ${newRepoName} -f docker/dotnet/Dockerfile .

    echo "tagging image"
    docker tag ${newRepoName}:latest ${REPO}/${newRepoName}:latest

    echo "pushing image"
    docker push ${REPO}/${newRepoName}:latest
done
