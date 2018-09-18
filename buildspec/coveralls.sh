#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    S3_BUCKET="dev-smyleegithubeventroutes-codecoveragereports-loog4breq9fw"

    source cicd/env_vars.sh
    
    # download files from S3
    mkdir coverage
    aws s3 cp --recursive s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA} ./

    TOKEN=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')
    
    for files in ./; do
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
            --repoToken ${TOKEN}
    done
fi
