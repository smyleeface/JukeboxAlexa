#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source cicd/env_vars.sh

    # TODO: if github branch master -- TIER == `production` else `staging`

    TIER=$1
    CROSS_ACCOUNT_ROLE_NAME="CrossAccountRole-JukeboxAlexa"
    export ACCOUNT_ID=$(aws ssm get-parameter --name /dev/JukeboxAlexa/account/${TIER}  --with-decryption --query Parameter | jq -r '.Value')

    echo "***INFO: Generating ${TIER} credentials"
    credentials=$(aws sts assume-role --role-arn "arn:aws:iam::${ACCOUNT_ID}:role/${CROSS_ACCOUNT_ROLE_NAME}" --role-session-name ${GITSHA})
    export AWS_ACCESS_KEY_ID=$(echo ${credentials} | jq -r '.Credentials.AccessKeyId')
    export AWS_SECRET_ACCESS_KEY=$(echo ${credentials} | jq -r '.Credentials.SecretAccessKey')
    export AWS_SESSION_TOKEN=$(echo ${credentials} | jq -r '.Credentials.SessionToken')

    echo "AWS_ACCESS_KEY_ID=${AWS_ACCESS_KEY_ID}"
    echo "AWS_SECRET_ACCESS_KEY=${AWS_SECRET_ACCESS_KEY}"
    echo "AWS_SESSION_TOKEN=${AWS_SESSION_TOKEN}"

    echo "***INFO: Deploying to ${TIER}"
    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa
    lash deploy --tier ${TIER}
fi