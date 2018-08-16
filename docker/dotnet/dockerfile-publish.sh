#!/usr/bin/env bash

PROFILE=$1

# creates the docker image used in codebuild to publish updated zip file
REPO=892750500233.dkr.ecr.us-west-2.amazonaws.com
REPO_NAME="jukebox-dotnet"

LOGIN=$(aws ecr get-login --no-include-email --region us-west-2 --profile ${PROFILE})
$LOGIN

docker build -t ${REPO_NAME} -f docker/dotnet/Dockerfile-dotnet .
docker tag ${REPO_NAME}:latest ${REPO}/${REPO_NAME}:latest
docker push ${REPO}/${REPO_NAME}:latest