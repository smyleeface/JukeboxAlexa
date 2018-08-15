#!/usr/bin/env bash

GITDIFF_FILES=$(git diff $(git log --pretty=format:%H -n 2) --name-only)

if [[ ${GITDIFF_FILES} = *"/infrastructure/"* ]]; then

    echo "Copy to S3"
    aws s3 cp ${CODEBUILD_SRC_DIR}/cloudformation/infrastructure/ s3://smyleeface-public/JukeboxAlexa/cloudformation/ --recursive --acl public-read

    echo "Validate clouformation"

    echo "Starting cloudformation update"
#    aws cloudformation deploy --stack-name "devel-JukeboxAlexa" --template-file "https://s3-us-west-2.amazonaws.com/smyleeface-public/JukeboxAlexa/cloudformation/jukebox-main.yaml"
else
    echo "No changes made"
fi