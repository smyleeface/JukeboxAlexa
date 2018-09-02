#!/usr/bin/env bash

TIER=$1
CROSS_ACCOUNT_ROLE_NAME="CrossAccountRole-JukeboxAlexa"

if [[ "${TIER}" == "production" ]]; then
    ACCOUNT_ID=$(aws ssm get-parameter --name /dev/JukeboxAlexa/account/production  --with-decryption --query Parameter | jq -r '.Value')
else
    ACCOUNT_ID=$(aws ssm get-parameter --name /dev/JukeboxAlexa/account/staging  --with-decryption --query Parameter | jq -r '.Value')
fi

# ${TIER} credentials
echo "***INFO: Generating ${TIER} credentials"
credentials=$(aws sts assume-role --role-arn "arn:aws:iam::${ACCOUNT_ID}:role/${CROSS_ACCOUNT_ROLE_NAME}" --role-session-name ${GITSHA})
AWS_ACCESS_KEY_ID=echo ${credentials} | jq -r '.Credentials.AccessKeyId'
AWS_SECRET_ACCESS_KEY=echo ${credentials} | jq -r '.Credentials.SecretAccessKey'
AWS_SESSION_TOKEN=echo ${credentials} | jq -r '.Credentials.SessionToken'

echo "***INFO: Deploying to ${TIER}"
cd ${CODEBUILD_SRC_DIR}/JukeboxAlexa
lash deploy --tier ${TIER}
