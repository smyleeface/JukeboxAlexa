#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source buildspec/env_vars.sh
    
    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa

    echo "***INFO: Restoring packages"
    dotnet restore

    echo "***INFO: Building packages"
    dotnet build

    for directory in ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/JukeboxAlexa.*/ ; do

        PROJECT_NAME=${directory##*/}
        COVERLET_OUTPUT=${directory}coverage-${PROJECT_NAME}.xml
        
        echo "***INFO: coverlet ${COVERLET_OUTPUT}"
        ${CODEBUILD_SRC_DIR}/tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
            --output ${COVERLET_OUTPUT} \
            --target /usr/bin/dotnet \
            --targetargs "test ${directory} --no-build" \
            --format opencover \
            --exclude-by-file "**/obj/**" \
            --exclude-by-file "**/bin/**"
            
        # upload report to s3
        
        echo "${COVERLET_OUTPUT} s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/"
#        aws s3 cp ${COVERLET_OUTPUT} s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA}/
    done
fi

