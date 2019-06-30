#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source buildspec/env_vars.sh
    
    echo "CROSS_ACCOUNT_ROLE_ARN ${CROSS_ACCOUNT_ROLE_ARN}"
    
    # NOTE(pattyr, 20190728): LAMBDASHARP_TIER is included as an environment variable when triggering CodeBuild Project
    # NOTE(pattyr, 20190728): CROSS_ACCOUNT_ROLE_ARN is included as an environment variable in the CodeBuild Project
    echo "***INFO: Generating ${LAMBDASHARP_TIER} credentials"
    credentials=$(aws sts assume-role --role-arn "${CROSS_ACCOUNT_ROLE_ARN}" --role-session-name ${GITSHA})
    export AWS_ACCESS_KEY_ID=$(echo ${credentials} | jq -r '.Credentials.AccessKeyId')
    export AWS_SECRET_ACCESS_KEY=$(echo ${credentials} | jq -r '.Credentials.SecretAccessKey')
    export AWS_SESSION_TOKEN=$(echo ${credentials} | jq -r '.Credentials.SessionToken')

    echo "***INFO: Deploying to ${LAMBDASHARP_TIER}"
    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/buildspec
    lash deploy --tier ${LAMBDASHARP_TIER}
fi
