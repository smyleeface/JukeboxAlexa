#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    ls -la ${CODEBUILD_SRC_DIR}

    S3_BUCKET="dev-smyleegithubeventroutes-codecoveragereports-loog4breq9fw"

    source buildspec/env_vars.sh

    # download files from S3
    mkdir coverage
    cd coverage
    aws s3 cp --recursive s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA} ./

    TOKEN=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')

    for COVFILES in */coverage.xml; do
        echo "***INFO: uploading coverage"
        echo ${COVFILES}
        ${CODEBUILD_SRC_DIR}/tools/csmacnz.Coveralls \
            --commitId ${GITSHA} \
            --commitBranch "${GIT_BRANCH}" \
            --commitAuthor "${GIT_AUTHOR_NAME}" \
            --commitEmail "${GIT_AUTHOR_EMAIL}" \
            --commitMessage "${GIT_COMMIT_MESSAGE}" \
            --jobId "${CODEBUILD_BUILD_ID}" \
            --useRelativePaths \
            --opencover \
            -i ${COVFILES} \
            --repoToken ${TOKEN}
    done
fi
