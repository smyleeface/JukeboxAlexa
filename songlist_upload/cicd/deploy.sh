#!/usr/bin/env bash

profile=default
region=us-west-2
CloudformationS3BucketUrl=https://s3-us-west-2.amazonaws.com/smyleeface-public/jukebox_alexa/cicd/songlist_upload/codebuild.yaml

python codebuild.py > codebuild.yaml

## Upload to S3
aws s3 cp codebuild.yaml s3://smyleeface-public/jukebox_alexa/cicd/songlist_upload/codebuild.yaml

# set configuration values
echo "*** INFO: find stack"
stackName=cicd-jukebox-songlist-upload-codebuild-main
stackExists=$(aws --profile ${profile} --region ${region} \
    cloudformation describe-stacks \
    --stack-name ${stackName} \
    --query "Stacks[?StackName==\`${stackName}\`]" \
    --output text)
stackTemplateUrl=${CloudformationS3BucketUrl}

if [[ -z "${stackExists}" ]]; then
    echo "Creating ${stackName} now ..."
    aws --profile ${profile} --region ${region} \
        cloudformation create-stack \
        --stack-name ${stackName} \
        --template-url ${stackTemplateUrl} \
        --parameters ${stackParameters} \
        --capabilities CAPABILITY_NAMED_IAM
else
    echo "stackExists ${stackName} updating now ..."
    aws --profile ${profile} --region ${region} \
        cloudformation update-stack \
        --stack-name ${stackName} \
        --template-url ${stackTemplateUrl} \
        --parameters ${stackParameters} \
        --capabilities CAPABILITY_NAMED_IAM
fi
