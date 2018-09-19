#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    cd ${CODEBUILD_SRC_DIR}
    
    coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')
    codeCovToken=$(aws ssm get-parameter --name /general/codecov/token  --with-decryption --query  Parameter | jq -r '.Value')

    for directory in ./src/JukeboxAlexa/JukeboxAlexa.*/ ; do

        echo "****DIRECTORY: ${directory}"

        cd ${CODEBUILD_SRC_DIR}
        
        echo "***INFO: coverlet ${directory}"
        ${CODEBUILD_SRC_DIR}/tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
            --target /usr/bin/dotnet \
            --targetargs "test ${directory} --no-build" \
            --format opencover \
            --exclude-by-file "**/obj/**" \
            --exclude-by-file "**/bin/**" \
            --merge-with ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/coverage.json
        
#        echo "***INFO: coverlet ${directory}"
#        ${CODEBUILD_SRC_DIR}/tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
#            --output ${directory}coverage.xml \
#            --target /usr/bin/dotnet \
#            --targetargs "test ${directory} --no-build" \
#            --format opencover \
#            --exclude-by-file "**/obj/**" \
#            --exclude-by-file "**/bin/**" 

#        echo "***INFO: uploading Coveralls"
#        ./tools/csmacnz.Coveralls \
#            --commitId ${GITSHA} \
#            --commitBranch "${GIT_BRANCH}" \
#            --commitAuthor "${GIT_AUTHOR_NAME}" \
#            --commitEmail "${GIT_AUTHOR_EMAIL}" \
#            --commitMessage "${GIT_COMMIT_MESSAGE}" \
#            --jobId "${CODEBUILD_BUILD_ID}" \
#            --useRelativePaths \
#            --opencover \
#            -i ${directory}coverage.xml \
#            --repoToken ${coverallsToken}
#            
#        echo "***INFO: uploading CodeCov"
#        tools/codecov \
#            -f "${directory}coverage.xml" \
#            -t ${codeCovToken} \
#            -B ${GIT_BRANCH} \
#            -C ${GITSHA} \
#            -b "${CODEBUILD_BUILD_ID}"
    done
    
    echo "***INFO: uploading Coveralls"
    ${CODEBUILD_SRC_DIR}/tools/csmacnz.Coveralls \
        --commitId ${GITSHA} \
        --commitBranch "${GIT_BRANCH}" \
        --commitAuthor "${GIT_AUTHOR_NAME}" \
        --commitEmail "${GIT_AUTHOR_EMAIL}" \
        --commitMessage "${GIT_COMMIT_MESSAGE}" \
        --jobId "${CODEBUILD_BUILD_ID}" \
        --useRelativePaths \
        --opencover \
        -i ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/coverage.json \
        --repoToken ${coverallsToken}

    echo "***INFO: uploading CodeCov"
    ${CODEBUILD_SRC_DIR}/tools/codecov \
        -f ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/coverage.json \
        -t ${codeCovToken} \
        -B ${GIT_BRANCH} \
        -C ${GITSHA} \
        -b "${CODEBUILD_BUILD_ID}"
fi







#_-------------------------------------------------
##!/usr/bin/env bash
#
#set -e
#
#if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then
#
#    S3_BUCKET="dev-smyleegithubeventroutes-codecoveragereports-loog4breq9fw"
#
#    source buildspec/env_vars.sh
#    
#    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa
#
#    echo "***INFO: Restoring packages"
#    dotnet restore
#
#    echo "***INFO: Building packages"
#    dotnet build
#
#    for BUILD_DIRECTORY in ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/JukeboxAlexa.* ; do
#
#        if [[ -d $BUILD_DIRECTORY ]]; then
#
#            PROJECT_NAME=${BUILD_DIRECTORY##*/}
#            COVERLET_OUTPUT=${BUILD_DIRECTORY}/coverage.xml
#            
#            echo "***INFO: coverlet ${COVERLET_OUTPUT}"
#            ${CODEBUILD_SRC_DIR}/tools/coverlet ${BUILD_DIRECTORY}/bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
#                --output ${COVERLET_OUTPUT} \
#                --target /usr/bin/dotnet \
#                --targetargs "test ${BUILD_DIRECTORY} --no-build" \
#                --format opencover \
#                --exclude-by-file "**/obj/**" \
#                --exclude-by-file "**/bin/**"
#                
#            # upload report to s3
#            
##            echo "${COVERLET_OUTPUT} s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/${PROJECT_NAME}/coverage.xml"
#            aws s3 cp ${COVERLET_OUTPUT} s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/${PROJECT_NAME}/coverage.xml
#        fi
#    done
#    
#    # start coveralls build
#    echo "aws codebuild start-build --project-name ${REPO##*/}-coveralls --source-version ${GIT_BRANCH} --environment-variables-override \
#        name=GITSHA,value=${GITSHA},type=PLAINTEXT \
#        name=GIT_BRANCH,value=${GIT_BRANCH},type=PLAINTEXT \
#        name=REPO,value=${REPO},type=PLAINTEXT \
#        name=JOB_ID,value=${JOB_ID},type=PLAINTEXT \
#        name=GIT_AUTHOR_NAME,value=${GIT_AUTHOR_NAME},type=PLAINTEXT \
#        name=GIT_AUTHOR_EMAIL,value=${GIT_AUTHOR_EMAIL},type=PLAINTEXT \
#        name=GIT_COMMIT_MESSAGE,value=${GIT_COMMIT_MESSAGE},type=PLAINTEXT"
#        
#    aws codebuild start-build --project-name "${REPO##*/}-coveralls" --source-version "${GIT_BRANCH}" --environment-variables-override \
#        name=GITSHA,value=${GITSHA},type=PLAINTEXT \
#        name=GIT_BRANCH,value=${GIT_BRANCH},type=PLAINTEXT \
#        name=REPO,value=${REPO},type=PLAINTEXT \
#        name=JOB_ID,value=${JOB_ID},type=PLAINTEXT \
#        name=GIT_AUTHOR_NAME,value=${GIT_AUTHOR_NAME},type=PLAINTEXT \
#        name=GIT_AUTHOR_EMAIL,value=${GIT_AUTHOR_EMAIL},type=PLAINTEXT \
#        name=GIT_COMMIT_MESSAGE,value="${GIT_COMMIT_MESSAGE}",type=PLAINTEXT
#fi
#
