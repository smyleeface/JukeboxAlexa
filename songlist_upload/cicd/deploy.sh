#!/usr/bin/env bash

profile=default
region=us-west-2
CloudformationS3BucketUrl=https://s3-us-west-2.amazonaws.com/smyleeface-public/jukebox_alexa/cicd/songlist_upload/cf_codebuild.yaml
Environment=cicd
CodeBuildServiceName=jukebox-songlist-upload
CodeBuildGitHubUrl=https://github.com/smyleeface/jukebox_alexa
CodeBuildServiceRoleName=codebuild-service-role
CodeBuildArtifactsLocation=smyleeface-public
CodeBuildArtifactsName=songlist_upload.zip
CodeBuildArtifactsPath=JukeboxAlexa/CodeBuildArtifacts
CodeBuildBuildSpecPath=songlist_upload/buildspec.yaml
CodeBuildEcrContianer=952671759649.dkr.ecr.us-west-2.amazonaws.com/jukebox-python:latest
CodeBuildProjectDescription="Packages and publishes the code for the lambda function that updates the song list when csv is uploaded to s3"

# Upload to S3
aws s3 cp cf_codebuild.yaml s3://smyleeface-public/jukebox_alexa/cicd/songlist_upload/cf_codebuild.yaml

## set parameters
echo "*** INFO: set stack parameters"
stackParameters="\
    ParameterKey=CloudformationS3BucketUrl,ParameterValue=${CloudformationS3BucketUrl} \
    ParameterKey=Environment,ParameterValue=${Environment} \
    ParameterKey=CodeBuildServiceName,ParameterValue=${CodeBuildServiceName} \
    ParameterKey=CodeBuildGitHubUrl,ParameterValue=${CodeBuildGitHubUrl} \
    ParameterKey=CodeBuildServiceRoleName,ParameterValue=${CodeBuildServiceRoleName} \
    ParameterKey=CodeBuildArtifactsLocation,ParameterValue=${CodeBuildArtifactsLocation} \
    ParameterKey=CodeBuildArtifactsName,ParameterValue=${CodeBuildArtifactsName} \
    ParameterKey=CodeBuildArtifactsPath,ParameterValue=${CodeBuildArtifactsPath} \
    ParameterKey=CodeBuildBuildSpecPath,ParameterValue=${CodeBuildBuildSpecPath} \
    ParameterKey=CodeBuildEcrContianer,ParameterValue=${CodeBuildEcrContianer} \
    ParameterKey=CodeBuildProjectDescription,ParameterValue=${CodeBuildProjectDescription}
"

# set configuration values
echo "*** INFO: find stack"
stackName=${Environment}-${CodeBuildServiceName}-codebuild-main
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
