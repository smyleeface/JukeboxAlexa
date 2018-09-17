#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source cicd/10_envvars.sh

    coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')
    
    mkdir coverlet
    cd coverlet
    echo "s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/ ${COVERLET_OUTPUT}"
#        aws s3 cp s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/ ${COVERLET_OUTPUT}
    
    for files in ./ ; do
        echo "***INFO: uploading coverage"
        tools/csmacnz.Coveralls \
            --commitId ${GITSHA} \
            --commitBranch "${GIT_BRANCH}" \
            --commitAuthor "${GIT_AUTHOR_NAME}" \
            --commitEmail "${GIT_AUTHOR_EMAIL}" \
            --commitMessage "${GIT_COMMIT_MESSAGE}" \
            --jobId "${CODEBUILD_BUILD_ID}" \
            --useRelativePaths \
            --opencover \
            -i ${files} \
            --repoToken ${coverallsToken}
    done
fi
