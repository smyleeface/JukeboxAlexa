#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source cicd/env_vars.sh

    TOKEN=$(aws ssm get-parameter --name /general/codecov/token  --with-decryption --query  Parameter | jq -r '.Value')
    
    # do i need to git clone here?
    
    for files in ./${REPO}/${GIT_BRANCH}/${GITSHA}/${COVERLET_OUTPUT} ; do
        echo "***INFO: uploading coverage"
        ./codecov \
            -f ${files} \
            -t ${TOKEN}
#            --commitId ${GITSHA} \
#            --commitBranch "${GIT_BRANCH}" \
#            --commitAuthor "${GIT_AUTHOR_NAME}" \
#            --commitEmail "${GIT_AUTHOR_EMAIL}" \
#            --commitMessage "${GIT_COMMIT_MESSAGE}" \
#            --jobId "${CODEBUILD_BUILD_ID}" \
#            --useRelativePaths \
#            --opencover \
#            -i ${files} \
#            --repoToken ${coverallsToken}
    done
fi
