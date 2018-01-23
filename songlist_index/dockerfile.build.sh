#!/usr/bin/env bash

# creates the docker image used in codebuild to publish updated zip file
REPO=952671759649.dkr.ecr.us-west-2.amazonaws.com
REPO_NAME=jukebox_songlist_index_build

LOGIN=$(aws ecr get-login --no-include-email --region us-west-2)
$LOGIN

docker build -t ${REPO_NAME} .
docker tag ${REPO_NAME}:latest ${REPO}/${REPO_NAME}:latest
docker push ${REPO}/${REPO_NAME}:latest