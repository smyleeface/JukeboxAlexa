#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source cicd/env_vars.sh

    TOKEN=$(aws ssm get-parameter --name /general/codacy/token  --with-decryption --query  Parameter | jq -r '.Value')
    
    ## TODO might have to clone the repo
    
    for files in ./${REPO}/${GIT_BRANCH}/${GITSHA}/${COVERLET_OUTPUT} ; do
        echo "***INFO: uploading coverage"
        python-codacy-coverage -r ${files}
    done
fi
