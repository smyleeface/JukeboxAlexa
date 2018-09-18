#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    S3BUCKET="dev-smyleegithubeventroutes-codecoveragereports-loog4breq9fw"

    source buildspec/env_vars.sh
    
    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa

    echo "***INFO: Restoring packages"
    dotnet restore

    echo "***INFO: Building packages"
    dotnet build

    for BUILD_DIRECTORY in ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/JukeboxAlexa.* ; do

        if [[ -d $BUILD_DIRECTORY ]]; then

            PROJECT_NAME=${BUILD_DIRECTORY##*/}
            COVERLET_OUTPUT=${BUILD_DIRECTORY}/coverage.xml
            
            echo "***INFO: coverlet ${COVERLET_OUTPUT}"
            ${CODEBUILD_SRC_DIR}/tools/coverlet ${BUILD_DIRECTORY}/bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
                --output ${COVERLET_OUTPUT} \
                --target /usr/bin/dotnet \
                --targetargs "test ${BUILD_DIRECTORY} --no-build" \
                --format opencover \
                --exclude-by-file "**/obj/**" \
                --exclude-by-file "**/bin/**"
                
            # upload report to s3
            
#            echo "${COVERLET_OUTPUT} s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/${PROJECT_NAME}/coverage.xml"
            aws s3 cp ${COVERLET_OUTPUT} s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/${PROJECT_NAME}/coverage.xml
        fi
    done
fi

